namespace BetterGenshinImpact.Core.Config;

/// <summary>
/// 手柄按键按下方式：短按或长按
/// </summary>
public enum GamepadPressMode
{
    /// <summary>短按（PressDurationMs）</summary>
    Short = 0,

    /// <summary>长按（LongPressDurationMs）</summary>
    Long,
}
