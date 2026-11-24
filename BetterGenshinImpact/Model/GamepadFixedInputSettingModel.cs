namespace BetterGenshinImpact.Model;

/// <summary>
/// 固定摇杆行（移动/视角），按键不可修改，只提供测试
/// </summary>
public sealed class GamepadFixedInputSettingModel
{
    public string ActionName { get; }

    /// <summary>显示用固定值</summary>
    public string ValueText { get; }

    /// <summary>true = 右摇杆（视角），false = 左摇杆（移动）</summary>
    public bool IsRightStick { get; }

    public GamepadFixedInputSettingModel(string actionName, string valueText, bool isRightStick)
    {
        ActionName = actionName;
        ValueText = valueText;
        IsRightStick = isRightStick;
    }
}
