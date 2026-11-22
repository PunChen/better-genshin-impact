using System;
using System.Threading;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Simulator.Extensions;
using Fischless.WindowsInput.Gamepad;

namespace BetterGenshinImpact.Core.Simulator.Gamepad;

/// <summary>
/// 手柄动作分发：把手柄状态翻译成具体按键/摇杆操作。
/// 不负责连接管理，便于用假传输层做单元测试。
/// </summary>
public sealed class GamepadActionDispatcher
{
    private readonly IGamepadTransport _transport;
    private readonly LeftStickState _leftStick = new();

    public GamepadActionDispatcher(IGamepadTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// 尝试用手柄模拟动作。返回 false 表示未处理，调用方应继续使用键鼠逻辑。
    /// </summary>
    public bool TrySimulate(GIActions action, KeyType type, GamepadConfig config)
    {
        if (!config.Enabled || !_transport.IsConnected)
        {
            return false;
        }

        if (GamepadActionMap.TryGetDirection(action, out _, out _))
        {
            SimulateDirection(action, type, config);
            return true;
        }

        if (!GamepadActionMap.TryGetButton(action, config, out var button) || button == GamepadButton.None)
        {
            return false;
        }

        SimulateButton(button, type, config);
        return true;
    }

    /// <summary>
    /// 按下并释放一个按键
    /// </summary>
    public void Press(GamepadButton button, int durationMs)
    {
        _transport.SetButton(button, true);
        Thread.Sleep(Math.Max(1, durationMs));
        _transport.SetButton(button, false);
    }

    /// <summary>
    /// 释放所有按键与摇杆
    /// </summary>
    public void ReleaseAll()
    {
        _leftStick.Reset();
        _transport.Reset();
    }

    private void SimulateButton(GamepadButton button, KeyType type, GamepadConfig config)
    {
        switch (type)
        {
            case KeyType.KeyPress:
                Press(button, config.PressDurationMs);
                break;
            case KeyType.KeyDown:
                _transport.SetButton(button, true);
                break;
            case KeyType.KeyUp:
                _transport.SetButton(button, false);
                break;
            case KeyType.Hold:
                _transport.SetButton(button, true);
                Thread.Sleep(1000);
                _transport.SetButton(button, false);
                break;
        }
    }

    private void SimulateDirection(GIActions action, KeyType type, GamepadConfig config)
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

    private void ApplyLeftStick()
    {
        var (x, y) = _leftStick.GetAxisValues();
        _transport.SetAxis(GamepadAxis.LeftThumbX, x);
        _transport.SetAxis(GamepadAxis.LeftThumbY, y);
    }
}
