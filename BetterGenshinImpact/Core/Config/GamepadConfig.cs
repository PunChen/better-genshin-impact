using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Fischless.WindowsInput.Gamepad;

namespace BetterGenshinImpact.Core.Config;

/// <summary>
/// 手柄（虚拟 Xbox360 手柄）配置。
/// 默认关闭：未安装 ViGEmBus 驱动时切到手柄模式不会有任何效果，程序会自动回退到键鼠逻辑。
/// </summary>
[Serializable]
public partial class GamepadConfig : ObservableObject
{
    /// <summary>
    /// 是否启用手柄模式。开启后所有经 <c>GIActions</c> 发出的操作优先用手柄，
    /// 未绑定或驱动不可用时自动回退键鼠。
    /// </summary>
    [ObservableProperty]
    private bool _enabled;

    /// <summary>
    /// 单个按键的按下时长（毫秒）
    /// </summary>
    [ObservableProperty]
    private int _pressDurationMs = 60;

    /// <summary>
    /// 任务结束/程序退出时是否释放所有按键
    /// </summary>
    [ObservableProperty]
    private bool _releaseAllOnStop = true;

    #region 对话与菜单

    /// <summary>
    /// 确认（对话继续/菜单确认），可设置为 X/A/B/Y 中任意一个
    /// </summary>
    [ObservableProperty]
    private GamepadButton _confirm = GamepadButton.B;

    /// <summary>
    /// 取消/返回
    /// </summary>
    [ObservableProperty]
    private GamepadButton _cancel = GamepadButton.A;

    /// <summary>
    /// 跳过/推进对话
    /// </summary>
    [ObservableProperty]
    private GamepadButton _skipDialog = GamepadButton.B;

    #endregion

    #region 游戏操作

    /// <summary>
    /// 拾取/交互
    /// </summary>
    [ObservableProperty]
    private GamepadButton _pickUpOrInteract = GamepadButton.X;

    /// <summary>
    /// 普通攻击
    /// </summary>
    [ObservableProperty]
    private GamepadButton _normalAttack = GamepadButton.A;

    /// <summary>
    /// 元素战技
    /// </summary>
    [ObservableProperty]
    private GamepadButton _elementalSkill = GamepadButton.RightTrigger;

    /// <summary>
    /// 元素爆发
    /// </summary>
    [ObservableProperty]
    private GamepadButton _elementalBurst = GamepadButton.Y;

    /// <summary>
    /// 跳跃
    /// </summary>
    [ObservableProperty]
    private GamepadButton _jump = GamepadButton.B;

    /// <summary>
    /// 冲刺
    /// </summary>
    [ObservableProperty]
    private GamepadButton _sprint = GamepadButton.RightShoulder;

    /// <summary>
    /// 切换瞄准模式
    /// </summary>
    [ObservableProperty]
    private GamepadButton _switchAimingMode = GamepadButton.LeftTrigger;

    /// <summary>
    /// 切换走/跑
    /// </summary>
    [ObservableProperty]
    private GamepadButton _switchToWalkOrRun = GamepadButton.None;

    /// <summary>
    /// 快捷使用小道具
    /// </summary>
    [ObservableProperty]
    private GamepadButton _quickUseGadget = GamepadButton.None;

    #endregion

    #region 界面

    /// <summary>
    /// 打开地图
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openMap = GamepadButton.None;

    /// <summary>
    /// 打开背包
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openInventory = GamepadButton.None;

    /// <summary>
    /// 打开派蒙菜单
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openPaimonMenu = GamepadButton.Start;

    /// <summary>
    /// 打开任务菜单
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openQuestMenu = GamepadButton.None;

    #endregion
}
