using BetterGenshinImpact.Core.Config;
using CommunityToolkit.Mvvm.ComponentModel;
using Fischless.WindowsInput.Gamepad;

namespace BetterGenshinImpact.Model;

/// <summary>
/// 手柄快捷轮盘绑定行（按住修饰键 + 右摇杆方向）
/// </summary>
public partial class GamepadWheelBindingSettingModel : ObservableObject
{
    /// <summary>动作名称</summary>
    public string ActionName { get; }

    /// <summary>修饰键对应的配置属性名</summary>
    public string ModifierPropertyName { get; }

    /// <summary>方向对应的配置属性名</summary>
    public string DirectionPropertyName { get; }

    /// <summary>修饰键（默认 LB）</summary>
    [ObservableProperty]
    private GamepadButton _modifier;

    /// <summary>右摇杆方向</summary>
    [ObservableProperty]
    private GamepadStickDirection _direction;

    public GamepadWheelBindingSettingModel(string actionName, string modifierPropertyName, string directionPropertyName,
        GamepadButton modifier, GamepadStickDirection direction)
    {
        ActionName = actionName;
        ModifierPropertyName = modifierPropertyName;
        DirectionPropertyName = directionPropertyName;
        Modifier = modifier;
        Direction = direction;
    }
}
