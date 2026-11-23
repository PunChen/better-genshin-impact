using System;
using System.Collections.Generic;
using System.Threading;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.Service;
using Fischless.WindowsInput.Gamepad;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;

namespace BetterGenshinImpact.Core.Input.Backends.Gamepad;

/// <summary>
/// 手柄后端：使用 ViGEm 创建虚拟 Xbox360 手柄。
/// 需要游戏内切到手柄控制，并在本软件中同时开启手柄模式才会生效。
/// </summary>
public sealed class GamepadInputBackend : IInputBackend
{
    private readonly GamepadChannel _channel = new();

    public InputBackendKind Kind => InputBackendKind.Gamepad;

    public IInputChannel Foreground => _channel;

    public IInputChannel Background => _channel;

    public void ReleaseAll() => _channel.ReleaseAll();

    public void Dispose() => _channel.Dispose();

    /// <summary>
    /// 提前连接虚拟手柄，便于任务启动时在日志中暴露驱动问题
    /// </summary>
    public bool TryConnect() => _channel.TryConnect();

    /// <summary>
    /// 最近一次连接/操作失败的原因
    /// </summary>
    public string? LastError => _channel.LastError;

    /// <summary>
    /// 设置页“测试按键”使用：临时连接虚拟手柄按一次后立即释放
    /// </summary>
    public static bool TestButton(GamepadButton button, int durationMs, out string? error)
    {
        if (button == GamepadButton.None)
        {
            error = "未绑定按键";
            return false;
        }

        using var transport = new Xbox360Transport();
        if (!transport.TryConnect())
        {
            error = transport.LastError;
            return false;
        }

        try
        {
            transport.SetButton(button, true);
            Thread.Sleep(Math.Max(1, durationMs));
            transport.SetButton(button, false);
            error = null;
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }
}

/// <summary>
/// 手柄通道：
/// <list type="bullet">
/// <item>覆盖 <see cref="SimulateAction"/>，把 <see cref="GIActions"/> 直接映射成手柄输入；</item>
/// <item>键盘按键按当前键位配置反查成 <see cref="GIActions"/> 再映射，兼容大量直接按键的旧代码；</item>
/// <item>鼠标移动映射到右摇杆，原始鼠标按键/坐标点击不支持（提示后忽略）。</item>
/// </list>
/// </summary>
internal sealed class GamepadChannel : InputChannelBase, IDisposable
{
    /// <summary>
    /// 按下后保持时间（毫秒），可通过配置修改
    /// </summary>
    private const int DefaultPressDurationMs = 60;

    /// <summary>
    /// 右摇杆脉冲保持时间，模拟一次鼠标移动
    /// </summary>
    private const int RightStickPulseMs = 30;

    /// <summary>
    /// 需要参与“键盘按键反查动作”的动作清单。
    /// 顺序有讲究：Space 同时可能是跳跃和推进对话，这里优先按对话推进处理。
    /// 移动方向（WASD）也在其中：键鼠宏回放、JS 脚本等直接按 WASD 的旧代码可以正确映射到左摇杆。
    /// </summary>
    private static readonly GIActions[] ReverseMappableActions =
    [
        // Space 同时可能是跳跃和推进对话，优先按“确认/推进对话”处理
        GIActions.Confirm,
        GIActions.SkipDialog,
        GIActions.Cancel,
        GIActions.PickUpOrInteract,
        GIActions.NormalAttack,
        GIActions.ElementalSkill,
        GIActions.ElementalBurst,
        GIActions.SprintKeyboard,
        GIActions.SprintMouse,
        GIActions.SwitchAimingMode,
        GIActions.Jump,
        GIActions.Drop,
        GIActions.QuickUseGadget,
        GIActions.SwitchToWalkOrRun,
        GIActions.InteractionInSomeMode,
        GIActions.MoveForward,
        GIActions.MoveBackward,
        GIActions.MoveLeft,
        GIActions.MoveRight,
        GIActions.SwitchMember1,
        GIActions.SwitchMember2,
        GIActions.SwitchMember3,
        GIActions.SwitchMember4,
        GIActions.SwitchMember5,
        GIActions.OpenMap,
        GIActions.OpenInventory,
        GIActions.OpenCharacterScreen,
        GIActions.OpenAdventurerHandbook,
        GIActions.OpenPartySetupScreen,
        GIActions.OpenPaimonMenu,
        GIActions.OpenQuestMenu,
    ];

    private readonly object _sync = new();
    private readonly LeftStickState _leftStick = new();

    private Xbox360Transport? _transport;
    private bool _connectFailed;
    private bool _connectWarningLogged;

    private static GamepadConfig? Config => ConfigService.Config?.GamepadConfig;

    #region IInputChannel

    public override IInputChannel SimulateAction(GIActions action, KeyType type = KeyType.KeyPress)
    {
        var config = Config;
        if (config is null || !config.Enabled || !EnsureTransport())
        {
            // 未开启或驱动不可用：提示并忽略，避免误用键盘导致游戏输入错乱
            WarnUnsupported("手柄模式未启用或虚拟手柄不可用");
            return this;
        }

        if (GamepadActionMap.TryGetDirection(action, out _, out _))
        {
            SimulateDirection(action, type, config);
            return this;
        }

        if (GamepadActionMap.TryGetButton(action, config, out var button) && button != GamepadButton.None)
        {
            SimulateButton(button, type, config);
            return this;
        }

        WarnUnsupported($"动作 {action} 没有绑定手柄按键");
        return this;
    }

    public override bool IsKeyDown(User32.VK key)
    {
        // 手柄输入没有对应的键盘按下状态；仍保留基类记录，便于释放逻辑复用
        return base.IsKeyDown(key);
    }

    public override void ReleaseAll()
    {
        lock (_sync)
        {
            _leftStick.Reset();
            try
            {
                _transport?.Reset();
            }
            catch (Exception e)
            {
                Logger.LogDebug(e, "释放手柄按键失败");
            }
        }

        base.ReleaseAll();
    }

    #endregion

    #region 原子操作

    protected override void OnKeyDown(User32.VK key)
    {
        if (!TryMapKey(key, out var action))
        {
            WarnUnsupported($"按键 {key}");
            return;
        }

        if (!TryExecute(action, KeyType.KeyDown))
        {
            WarnUnsupported($"动作 {action}");
        }
    }

    protected override void OnKeyUp(User32.VK key)
    {
        if (!TryMapKey(key, out var action))
        {
            WarnUnsupported($"按键 {key}");
            return;
        }

        if (!TryExecute(action, KeyType.KeyUp))
        {
            WarnUnsupported($"动作 {action}");
        }
    }

    protected override void OnKeyPress(User32.VK key)
    {
        // 直接走动作层，保证按下与抬起使用同一个手柄按键
        if (!TryMapKey(key, out var action))
        {
            WarnUnsupported($"按键 {key}");
            return;
        }

        if (!TryExecute(action, KeyType.KeyPress))
        {
            WarnUnsupported($"动作 {action}");
        }
    }

    protected override void OnMouseButton(InputMouseButton button, bool down)
    {
        // 手柄模式下无法做绝对坐标点击，避免误映射成攻击
        WarnUnsupported($"鼠标按键 {button}");
    }

    protected override void OnMoveBy(int dx, int dy)
    {
        if (!EnsureTransport())
        {
            WarnUnsupported("鼠标移动（虚拟手柄不可用）");
            return;
        }

        // 用右摇杆脉冲近似一次鼠标移动，dx/dy 越大摇杆偏移越大
        var x = (short)Math.Clamp(dx * 256, short.MinValue, short.MaxValue);
        var y = (short)Math.Clamp(dy * 256, short.MinValue, short.MaxValue);

        lock (_sync)
        {
            _transport!.SetAxis(GamepadAxis.RightThumbX, x);
            _transport.SetAxis(GamepadAxis.RightThumbY, y);
        }

        Thread.Sleep(RightStickPulseMs);

        lock (_sync)
        {
            _transport?.SetAxis(GamepadAxis.RightThumbX, 0);
            _transport?.SetAxis(GamepadAxis.RightThumbY, 0);
        }
    }

    protected override void OnMoveTo(double absX, double absY)
    {
        WarnUnsupported("鼠标绝对移动");
    }

    protected override void OnScroll(int clicks)
    {
        WarnUnsupported("鼠标滚轮");
    }

    #endregion

    #region 手柄操作

    private bool TryExecute(GIActions action, KeyType type)
    {
        var config = Config;
        if (config is null || !config.Enabled || !EnsureTransport())
        {
            return false;
        }

        if (GamepadActionMap.TryGetDirection(action, out _, out _))
        {
            SimulateDirection(action, type, config);
            return true;
        }

        if (GamepadActionMap.TryGetButton(action, config, out var button) && button != GamepadButton.None)
        {
            SimulateButton(button, type, config);
            return true;
        }

        return false;
    }

    private void SimulateButton(GamepadButton button, KeyType type, GamepadConfig config)
    {
        lock (_sync)
        {
            var transport = _transport;
            if (transport is null)
            {
                return;
            }

            switch (type)
            {
                case KeyType.KeyPress:
                    transport.SetButton(button, true);
                    Thread.Sleep(Math.Max(1, config.PressDurationMs));
                    transport.SetButton(button, false);
                    break;
                case KeyType.KeyDown:
                    transport.SetButton(button, true);
                    break;
                case KeyType.KeyUp:
                    transport.SetButton(button, false);
                    break;
                case KeyType.Hold:
                    transport.SetButton(button, true);
                    Thread.Sleep(1000);
                    transport.SetButton(button, false);
                    break;
            }
        }
    }

    private void SimulateDirection(GIActions action, KeyType type, GamepadConfig config)
    {
        lock (_sync)
        {
            switch (type)
            {
                case KeyType.KeyPress:
                    _leftStick.Set(action, true);
                    ApplyLeftStick();
                    Thread.Sleep(Math.Max(1, config.PressDurationMs));
                    _leftStick.Set(action, false);
                    ApplyLeftStick();
                    break;
                case KeyType.KeyDown:
                    _leftStick.Set(action, true);
                    ApplyLeftStick();
                    break;
                case KeyType.KeyUp:
                    _leftStick.Set(action, false);
                    ApplyLeftStick();
                    break;
                case KeyType.Hold:
                    _leftStick.Set(action, true);
                    ApplyLeftStick();
                    Thread.Sleep(1000);
                    _leftStick.Set(action, false);
                    ApplyLeftStick();
                    break;
            }
        }
    }

    private void ApplyLeftStick()
    {
        if (_transport is null)
        {
            return;
        }

        var (x, y) = _leftStick.GetAxisValues();
        _transport.SetAxis(GamepadAxis.LeftThumbX, x);
        _transport.SetAxis(GamepadAxis.LeftThumbY, y);
    }

    #endregion

    #region 连接与反查

    /// <summary>
    /// 提前建立连接（任务启动时调用）
    /// </summary>
    public bool TryConnect() => EnsureTransport();

    /// <summary>
    /// 最近一次连接失败的原因
    /// </summary>
    public string? LastError => _transport?.LastError;

    private bool EnsureTransport()
    {
        lock (_sync)
        {
            if (_connectFailed && _transport is not null)
            {
                return false;
            }

            _transport ??= new Xbox360Transport();

            if (_transport.IsConnected)
            {
                return true;
            }

            if (_transport.TryConnect())
            {
                _connectFailed = false;
                return true;
            }

            _connectFailed = true;
            if (!_connectWarningLogged)
            {
                _connectWarningLogged = true;
                Logger.LogWarning("虚拟手柄连接失败，请确认已安装 ViGEmBus 驱动：{Error}", _transport.LastError);
            }

            return false;
        }
    }

    private static bool TryMapKey(User32.VK key, out GIActions action)
    {
        foreach (var candidate in ReverseMappableActions)
        {
            var keyId = candidate.ToActionKey();
            if (keyId is KeyId.None or KeyId.Unknown)
            {
                continue;
            }

            if (keyId.ToVK() == key)
            {
                action = candidate;
                return true;
            }
        }

        action = default;
        return false;
    }

    #endregion

    public void Dispose()
    {
        lock (_sync)
        {
            try
            {
                _transport?.Dispose();
            }
            catch (Exception e)
            {
                Logger.LogDebug(e, "释放虚拟手柄失败");
            }

            _transport = null;
        }
    }
}
