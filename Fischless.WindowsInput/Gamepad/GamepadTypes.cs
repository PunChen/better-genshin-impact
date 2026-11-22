using System;

namespace Fischless.WindowsInput.Gamepad;

/// <summary>
/// 虚拟手柄可操作的按键。
/// 扳机键（<see cref="LeftTrigger"/>、<see cref="RightTrigger"/>）在底层是滑块而不是按钮，
/// 但为了使上层按键映射保持一致，这里把它们也当作“按键”处理（按下即满行程）。
/// </summary>
public enum GamepadButton
{
    /// <summary>不绑定</summary>
    None = 0,

    /// <summary>A</summary>
    A,

    /// <summary>B</summary>
    B,

    /// <summary>X</summary>
    X,

    /// <summary>Y</summary>
    Y,

    /// <summary>左肩键 LB</summary>
    LeftShoulder,

    /// <summary>右肩键 RB</summary>
    RightShoulder,

    /// <summary>左扳机 LT</summary>
    LeftTrigger,

    /// <summary>右扳机 RT</summary>
    RightTrigger,

    /// <summary>左摇杆按下 LS</summary>
    LeftThumb,

    /// <summary>右摇杆按下 RS</summary>
    RightThumb,

    /// <summary>十字键上</summary>
    Up,

    /// <summary>十字键下</summary>
    Down,

    /// <summary>十字键左</summary>
    Left,

    /// <summary>十字键右</summary>
    Right,

    /// <summary>开始键（菜单键）</summary>
    Start,

    /// <summary>返回键（视图键）</summary>
    Back,

    /// <summary>Xbox 键</summary>
    Guide,
}

/// <summary>
/// 虚拟手柄的摇杆轴
/// </summary>
public enum GamepadAxis
{
    /// <summary>左摇杆 X</summary>
    LeftThumbX,

    /// <summary>左摇杆 Y</summary>
    LeftThumbY,

    /// <summary>右摇杆 X</summary>
    RightThumbX,

    /// <summary>右摇杆 Y</summary>
    RightThumbY,
}

/// <summary>
/// 虚拟手柄传输层。上层只依赖该接口，便于在无驱动环境下替换成假实现做单元测试。
/// </summary>
public interface IGamepadTransport : IDisposable
{
    /// <summary>
    /// 是否已经连接（创建了虚拟手柄）
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 最近一次失败的原因
    /// </summary>
    string? LastError { get; }

    /// <summary>
    /// 尝试连接虚拟手柄，失败时返回 false 且不抛出异常
    /// </summary>
    bool TryConnect();

    /// <summary>
    /// 断开虚拟手柄
    /// </summary>
    void Disconnect();

    /// <summary>
    /// 设置按键状态
    /// </summary>
    void SetButton(GamepadButton button, bool pressed);

    /// <summary>
    /// 设置摇杆轴的值（-32768 ~ 32767）
    /// </summary>
    void SetAxis(GamepadAxis axis, short value);

    /// <summary>
    /// 释放所有按键与摇杆，避免任务异常退出后按键卡住
    /// </summary>
    void Reset();
}
