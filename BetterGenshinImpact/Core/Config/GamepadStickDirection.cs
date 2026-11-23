namespace BetterGenshinImpact.Core.Config;

/// <summary>
/// 手柄右摇杆方向（用于 LB + 右摇杆的快捷轮盘选择）
/// </summary>
public enum GamepadStickDirection
{
    /// <summary>不使用</summary>
    None = 0,

    /// <summary>上</summary>
    Up,

    /// <summary>下</summary>
    Down,

    /// <summary>左</summary>
    Left,

    /// <summary>右</summary>
    Right,

    /// <summary>左上</summary>
    UpLeft,

    /// <summary>右上</summary>
    UpRight,

    /// <summary>左下</summary>
    DownLeft,

    /// <summary>右下</summary>
    DownRight,
}
