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
    /// 落下（滑翔中 A 触发下落攻击，攀爬中 A 取消攀爬；默认 A）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _drop = GamepadButton.A;

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
    /// 特定玩法内交互操作（T 键，如抓取叶片等；默认 LB + X）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _interactionInSomeMode = GamepadButton.X;

    /// <summary>
    /// 特定玩法内交互的组合键修饰键（默认 LB）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _interactionInSomeModeModifier = GamepadButton.LeftShoulder;

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

    /// <summary>
    /// 中断挑战（默认 LS + RS 同时按下；先按住 RS 再按 LS，避免单独按下 LS 误触发任务追踪）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _abandonChallenge = GamepadButton.LeftThumb;

    /// <summary>
    /// 中断挑战的组合键修饰键（默认 RS）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _abandonChallengeModifier = GamepadButton.RightThumb;

    /// <summary>
    /// 地图放大（默认 LT）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _mapZoomIn = GamepadButton.LeftTrigger;

    /// <summary>
    /// 地图缩小（默认 RT）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _mapZoomOut = GamepadButton.RightTrigger;

    /// <summary>
    /// 镜头拉近（默认 LB + LT）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _cameraZoomIn = GamepadButton.LeftTrigger;

    /// <summary>
    /// 镜头拉近的组合键修饰键（默认 LB）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _cameraZoomInModifier = GamepadButton.LeftShoulder;

    /// <summary>
    /// 镜头拉远（默认 LB + RT）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _cameraZoomOut = GamepadButton.RightTrigger;

    /// <summary>
    /// 镜头拉远的组合键修饰键（默认 LB）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _cameraZoomOutModifier = GamepadButton.LeftShoulder;

    /// <summary>
    /// 界面焦点/列表滚动的左摇杆保持时间（毫秒）
    /// </summary>
    [ObservableProperty]
    private int _uiNavigationPressMs = 120;

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
    /// 打开多人游戏界面（默认 LB + 十字键→）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openCoOpScreen = GamepadButton.Right;

    /// <summary>
    /// 打开多人游戏界面的组合键修饰键（默认 LB）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openCoOpScreenModifier = GamepadButton.LeftShoulder;

    /// <summary>
    /// 查看教程详情（默认 LB + 十字键↑）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _checkTutorialDetails = GamepadButton.Up;

    /// <summary>
    /// 查看教程详情的组合键修饰键（默认 LB）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _checkTutorialDetailsModifier = GamepadButton.LeftShoulder;

    /// <summary>
    /// 打开好友界面（无专用手柄按键；默认走派蒙菜单 + 文字识别「好友」+ 左摇杆导航，此处可绑定直连键）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _openFriendsScreen = GamepadButton.None;

    /// <summary>
    /// 开启任务追踪（默认 LS/左摇杆按下）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _questNavigation = GamepadButton.LeftThumb;

    /// <summary>
    /// 长按打开元素视野（默认 LB + 十字键左）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _elementalSight = GamepadButton.Left;

    /// <summary>
    /// 元素视野的组合键修饰键（默认 LB）
    /// </summary>
    [ObservableProperty]
    private GamepadButton _elementalSightModifier = GamepadButton.LeftShoulder;

    #endregion

    #region 快捷轮盘（LB + 右摇杆）

    /// <summary>
    /// 快捷轮盘：按住修饰键后拨动右摇杆选择菜单，保持时间（毫秒）
    /// </summary>
    [ObservableProperty]
    private int _wheelSelectionHoldMs = 180;

    /// <summary>打开地图：轮盘修饰键</summary>
    [ObservableProperty]
    private GamepadButton _openMapWheelModifier = GamepadButton.LeftShoulder;

    /// <summary>打开地图：轮盘方向（默认下）</summary>
    [ObservableProperty]
    private GamepadStickDirection _openMapWheelDirection = GamepadStickDirection.Down;

    /// <summary>打开背包：轮盘修饰键</summary>
    [ObservableProperty]
    private GamepadButton _openInventoryWheelModifier = GamepadButton.LeftShoulder;

    /// <summary>打开背包：轮盘方向（默认右）</summary>
    [ObservableProperty]
    private GamepadStickDirection _openInventoryWheelDirection = GamepadStickDirection.Right;

    /// <summary>打开任务菜单：轮盘修饰键</summary>
    [ObservableProperty]
    private GamepadButton _openQuestMenuWheelModifier = GamepadButton.LeftShoulder;

    /// <summary>打开任务菜单：轮盘方向（默认左下）</summary>
    [ObservableProperty]
    private GamepadStickDirection _openQuestMenuWheelDirection = GamepadStickDirection.DownLeft;

    /// <summary>打开角色界面：轮盘修饰键</summary>
    [ObservableProperty]
    private GamepadButton _openCharacterScreenWheelModifier = GamepadButton.LeftShoulder;

    /// <summary>打开角色界面：轮盘方向（默认右下）</summary>
    [ObservableProperty]
    private GamepadStickDirection _openCharacterScreenWheelDirection = GamepadStickDirection.DownRight;

    /// <summary>打开纪行界面：轮盘修饰键</summary>
    [ObservableProperty]
    private GamepadButton _openBattlePassScreenWheelModifier = GamepadButton.LeftShoulder;

    /// <summary>打开纪行界面：轮盘方向（默认左上）</summary>
    [ObservableProperty]
    private GamepadStickDirection _openBattlePassScreenWheelDirection = GamepadStickDirection.UpLeft;

    /// <summary>打开活动面板：轮盘修饰键</summary>
    [ObservableProperty]
    private GamepadButton _openTheEventsMenuWheelModifier = GamepadButton.LeftShoulder;

    /// <summary>打开活动面板：轮盘方向（默认左）</summary>
    [ObservableProperty]
    private GamepadStickDirection _openTheEventsMenuWheelDirection = GamepadStickDirection.Left;

    /// <summary>打开冒险之证：轮盘修饰键</summary>
    [ObservableProperty]
    private GamepadButton _openAdventurerHandbookWheelModifier = GamepadButton.LeftShoulder;

    /// <summary>打开冒险之证：轮盘方向（默认右上）</summary>
    [ObservableProperty]
    private GamepadStickDirection _openAdventurerHandbookWheelDirection = GamepadStickDirection.UpRight;

    #endregion
}
