using System;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.Core.Simulator.Extensions;

namespace BetterGenshinImpact.Core.Input;

/// <summary>
/// 手柄 UI 导航封装：把“菜单焦点/列表滚动/地图光标/地图缩放”等鼠标替代操作统一成动作。
/// 未启用手柄模式时调用这些方法不会产生键盘鼠标输入。
/// </summary>
public static class GamepadUiNavigator
{
    /// <summary>
    /// 当前是否为手柄后端
    /// </summary>
    public static bool IsAvailable => InputHub.Backend is GamepadInputBackend;

    private static GamepadInputBackend? Backend => InputHub.Backend as GamepadInputBackend;

    /// <summary>
    /// 移动焦点（左摇杆短按）
    /// </summary>
    public static void Focus(GamepadStickDirection direction)
    {
        var action = direction switch
        {
            GamepadStickDirection.Up => GIActions.FocusUp,
            GamepadStickDirection.Down => GIActions.FocusDown,
            GamepadStickDirection.Left => GIActions.FocusLeft,
            GamepadStickDirection.Right => GIActions.FocusRight,
            _ => GIActions.FocusDown,
        };
        InputHub.Foreground.SimulateAction(action);
    }

    /// <summary>
    /// 滚动列表；steps 为正向上、为负向下
    /// </summary>
    public static void Scroll(int steps)
    {
        var action = steps > 0 ? GIActions.ScrollUp : GIActions.ScrollDown;
        for (var i = 0; i < Math.Abs(steps); i++)
        {
            InputHub.Foreground.SimulateAction(action);
        }
    }

    /// <summary>
    /// 地图放大（RT）
    /// </summary>
    public static void ZoomIn() => InputHub.Foreground.SimulateAction(GIActions.MapZoomIn);

    /// <summary>
    /// 地图缩小（LT）
    /// </summary>
    public static void ZoomOut() => InputHub.Foreground.SimulateAction(GIActions.MapZoomOut);

    /// <summary>
    /// 镜头拉近（LB + LT；手柄专属的镜头距离调节）
    /// </summary>
    public static void CameraZoomIn() => InputHub.Foreground.SimulateAction(GIActions.CameraZoomIn);

    /// <summary>
    /// 镜头拉远（LB + RT；手柄专属的镜头距离调节）
    /// </summary>
    public static void CameraZoomOut() => InputHub.Foreground.SimulateAction(GIActions.CameraZoomOut);

    /// <summary>
    /// 拖动地图（左摇杆持续偏移，x/y 归一化到 -1..1）；地图光标移动等价于拖动地图
    /// </summary>
    public static void DragMap(double x, double y, int durationMs = 300)
    {
        Backend?.SetLeftStick(x, y, durationMs);
    }
}
