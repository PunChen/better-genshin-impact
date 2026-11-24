using BetterGenshinImpact.Core.Config;
using CommunityToolkit.Mvvm.ComponentModel;
using Fischless.WindowsInput.Gamepad;

namespace BetterGenshinImpact.Model;

/// <summary>
/// 手柄双键组合绑定行：修饰键 + 主键，两个键都支持短按/长按
/// </summary>
public partial class GamepadComboBindingSettingModel : ObservableObject
{
    public string ActionName { get; }

    public string ModifierPropertyName { get; }

    public string ButtonPropertyName { get; }

    public string ModifierModePropertyName { get; }

    public string ButtonModePropertyName { get; }

    [ObservableProperty]
    private GamepadButton _modifier;

    [ObservableProperty]
    private GamepadButton _button;

    [ObservableProperty]
    private GamepadPressMode _modifierMode;

    [ObservableProperty]
    private GamepadPressMode _buttonMode;

    public GamepadComboBindingSettingModel(
        string actionName,
        string modifierPropertyName,
        string buttonPropertyName,
        string modifierModePropertyName,
        string buttonModePropertyName,
        GamepadButton modifier,
        GamepadButton button,
        GamepadPressMode modifierMode,
        GamepadPressMode buttonMode)
    {
        ActionName = actionName;
        ModifierPropertyName = modifierPropertyName;
        ButtonPropertyName = buttonPropertyName;
        ModifierModePropertyName = modifierModePropertyName;
        ButtonModePropertyName = buttonModePropertyName;
        Modifier = modifier;
        Button = button;
        ModifierMode = modifierMode;
        ButtonMode = buttonMode;
    }
}
