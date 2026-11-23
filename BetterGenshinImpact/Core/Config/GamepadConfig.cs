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

    /// <summary>
    /// 右摇杆灵敏度：鼠标移动 1 像素对应的摇杆偏移量，越大转视角越快
    /// </summary>
    [ObservableProperty]
    private int _rightStickSensitivity = 64;

    /// <summary>
    /// 右摇杆脉冲保持时间（毫秒），用于把一次鼠标移动换算成一次短暂的摇杆偏移
    /// </summary>
    [ObservableProperty]
    private int _rightStickPulseMs = 30;

    /// <summary>
    /// 摇杆转动 1 度需要的保持时间（毫秒），用于 TurnByAngle 标定
    /// </summary>
    [ObservableProperty]
    private double _turnMillisecondsPerDegree = 6.0;

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
    /// 落下（滑翔/攀爬/下落攻击场景使用）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _drop = GamepadButton.None;

    /// <summary>
    /// 切换小队角色1（手柄默认十字键↑）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _switchMember1 = GamepadButton.Up;

    /// <summary>
    /// 切换小队角色2（手柄默认十字键→）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _switchMember2 = GamepadButton.Right;

    /// <summary>
    /// 切换小队角色3（手柄默认十字键←）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _switchMember3 = GamepadButton.Left;

    /// <summary>
    /// 切换小队角色4（手柄默认十字键↓）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _switchMember4 = GamepadButton.Down;

    /// <summary>
    /// 切换小队角色5（默认 LB + Y）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _switchMember5 = GamepadButton.Y;

    /// <summary>
    /// 切换小队角色5 的修饰键（默认 LB）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _switchMember5Modifier = GamepadButton.LeftShoulder;

    /// <summary>
    /// 特定玩法内交互操作（T 键，如抓取叶片等）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _interactionInSomeMode = GamepadButton.None;

    /// <summary>
    /// 快捷使用小道具（默认 LB + B）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _quickUseGadget = GamepadButton.B;

    /// <summary>
    /// 快捷使用小道具的修饰键（默认 LB）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _quickUseGadgetModifier = GamepadButton.LeftShoulder;

    /// <summary>
    /// 视角回正（对应鼠标中键/R3）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _cameraRecenter = GamepadButton.RightThumb;

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
    /// 打开派蒙菜单（默认 ☰ Menu）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openPaimonMenu = GamepadButton.Start;

    /// <summary>
    /// 打开好友聊天（默认 ⧉ View）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openChatScreen = GamepadButton.Back;

    /// <summary>
    /// 打开任务菜单
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openQuestMenu = GamepadButton.None;

    /// <summary>
    /// 打开角色界面
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openCharacterScreen = GamepadButton.None;

    /// <summary>
    /// 打开冒险之证界面
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openAdventurerHandbook = GamepadButton.None;

    /// <summary>
    /// 打开队伍配置界面
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openPartySetupScreen = GamepadButton.None;

    /// <summary>
    /// 打开纪行界面
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openBattlePassScreen = GamepadButton.None;

    /// <summary>
    /// 打开活动面板
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openTheEventsMenu = GamepadButton.None;

    /// <summary>
    /// 开启任务追踪
    /// </summary>
    [ObservableProperty]
    private GamepadButton _questNavigation = GamepadButton.None;

    /// <summary>
    /// 长按打开元素视野
    /// </summary>
    [ObservableProperty]
    private GamepadButton _elementalSight = GamepadButton.None;

    #endregion
}
