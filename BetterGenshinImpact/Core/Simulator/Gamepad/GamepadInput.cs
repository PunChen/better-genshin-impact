using System;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.Service;
using Fischless.WindowsInput.Gamepad;
using Microsoft.Extensions.Logging;

namespace BetterGenshinImpact.Core.Simulator.Gamepad;

/// <summary>
/// 手柄输入门面。整个程序共用一个虚拟 Xbox360 手柄实例：
/// <list type="bullet">
/// <item>手柄模式关闭、动作未绑定、驱动不可用时 <see cref="TrySimulate"/> 返回 false，调用方自动回退键鼠；</item>
/// <item>手柄注入是系统级输入，不依赖窗口焦点，因此在后台模式下同样会注入（能否被游戏接收取决于游戏本身）；</item>
/// <item>所有按键状态会被跟踪，任务结束时可 <see cref="ReleaseAll"/> 防止按键卡住。</item>
/// </list>
/// </summary>
public static class GamepadInput
{
    private static readonly object SyncRoot = new();

    private static IGamepadTransport? _transport;
    private static GamepadActionDispatcher? _dispatcher;
    private static bool _connectFailureLogged;
    private static bool _connectFailed;
    private static ILogger? _logger;

    /// <summary>
    /// 手柄模式是否已启用（仅表示配置开关，不代表已连接）
    /// </summary>
    public static bool IsEnabled => ConfigService.Config?.GamepadConfig.Enabled == true;

    /// <summary>
    /// 虚拟手柄是否已连接
    /// </summary>
    public static bool IsConnected
    {
        get
        {
            lock (SyncRoot)
            {
                return _transport?.IsConnected == true;
            }
        }
    }

    /// <summary>
    /// 最近一次手柄操作失败的原因
    /// </summary>
    public static string? LastError => _transport?.LastError;

    /// <summary>
    /// 尝试模拟一个游戏动作。返回 true 表示已由手柄接管；返回 false 表示调用方应继续走键鼠逻辑。
    /// </summary>
    public static bool TrySimulate(GIActions action, KeyType type)
    {
        var config = ConfigService.Config?.GamepadConfig;
        if (config is null || !config.Enabled)
        {
            return false;
        }

        try
        {
            lock (SyncRoot)
            {
                if (!EnsureConnected())
                {
                    return false;
                }

                return _dispatcher!.TrySimulate(action, type, config);
            }
        }
        catch (Exception e)
        {
            LogWarning($"手柄模拟动作 {action} 失败：{e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 按下并释放一个按键（用于设置页的“测试按键”）
    /// </summary>
    public static bool Press(GamepadButton button)
    {
        var config = ConfigService.Config?.GamepadConfig;
        if (config is null || button == GamepadButton.None)
        {
            return false;
        }

        try
        {
            lock (SyncRoot)
            {
                // 手动测试时允许重试连接
                _connectFailed = false;
                if (!EnsureConnected())
                {
                    return false;
                }

                _dispatcher!.Press(button, config.PressDurationMs);
                return true;
            }
        }
        catch (Exception e)
        {
            LogWarning($"手柄测试按键失败：{e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 提前建立连接，便于在设置页或任务启动时暴露驱动问题
    /// </summary>
    public static bool TryConnect()
    {
        lock (SyncRoot)
        {
            // 用户主动触发时允许重试
            _connectFailed = false;
            return EnsureConnected();
        }
    }

    /// <summary>
    /// 释放所有按键并断开虚拟手柄
    /// </summary>
    public static void Disconnect()
    {
        lock (SyncRoot)
        {
            ReleaseAllCore();
            _transport?.Disconnect();
        }
    }

    /// <summary>
    /// 释放所有按键与摇杆（任务取消、异常、程序退出时调用）
    /// </summary>
    public static void ReleaseAll()
    {
        lock (SyncRoot)
        {
            ReleaseAllCore();
        }
    }

    /// <summary>
    /// 释放虚拟手柄资源（程序退出时调用）
    /// </summary>
    public static void Dispose()
    {
        lock (SyncRoot)
        {
            ReleaseAllCore();
            try
            {
                _transport?.Dispose();
            }
            catch (Exception e)
            {
                LogWarning($"释放虚拟手柄失败：{e.Message}");
            }

            _transport = null;
            _dispatcher = null;
            _connectFailed = false;
            _connectFailureLogged = false;
        }
    }

    private static bool EnsureConnected()
    {
        if (_connectFailed && _transport is not null && !_transport.IsConnected)
        {
            // 已经确认失败（通常是未安装驱动），不再对每次动作都重试，避免高频异常
            return false;
        }

        _transport ??= new Xbox360Transport();

        if (_transport.IsConnected)
        {
            _dispatcher ??= new GamepadActionDispatcher(_transport);
            return true;
        }

        if (_transport.TryConnect())
        {
            _connectFailed = false;
            _connectFailureLogged = false;
            _dispatcher = new GamepadActionDispatcher(_transport);
            return true;
        }

        _connectFailed = true;
        if (!_connectFailureLogged)
        {
            _connectFailureLogged = true;
            LogWarning("虚拟手柄连接失败，将回退到键鼠模式。请确认已安装 ViGEmBus 驱动：" +
                       $"{_transport.LastError}");
        }

        return false;
    }

    private static void ReleaseAllCore()
    {
        try
        {
            _dispatcher?.ReleaseAll();
        }
        catch (Exception e)
        {
            LogWarning($"释放手柄按键失败：{e.Message}");
        }
    }

    private static void LogWarning(string message)
    {
        try
        {
            _logger ??= App.GetLogger<GamepadLogCategory>();
            _logger.LogWarning(message);
        }
        catch
        {
            // 日志服务尚未初始化时忽略，不能因为日志影响输入逻辑
        }
    }

    /// <summary>
    /// 仅用于获取日志分类，静态类不能直接作为泛型参数
    /// </summary>
    private sealed class GamepadLogCategory
    {
    }
}
