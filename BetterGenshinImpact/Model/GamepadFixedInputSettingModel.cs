namespace BetterGenshinImpact.Model;

/// <summary>
/// 固定摇杆方向测试行（不可修改摇杆本身，只提供测试）
/// </summary>
public sealed class GamepadFixedInputSettingModel
{
    public string ActionName { get; }

    /// <summary>显示用固定值</summary>
    public string ValueText { get; }

    /// <summary>true = 右摇杆（视角），false = 左摇杆（移动/地图光标）</summary>
    public bool IsRightStick { get; }

    /// <summary>摇杆 X 偏移（-1..1）</summary>
    public double X { get; }

    /// <summary>摇杆 Y 偏移（-1..1）</summary>
    public double Y { get; }

    public GamepadFixedInputSettingModel(string actionName, string valueText, bool isRightStick, double x, double y)
    {
        ActionName = actionName;
        ValueText = valueText;
        IsRightStick = isRightStick;
        X = x;
        Y = y;
    }
}
