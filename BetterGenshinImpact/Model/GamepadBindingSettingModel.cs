using CommunityToolkit.Mvvm.ComponentModel;
using Fischless.WindowsInput.Gamepad;

namespace BetterGenshinImpact.Model;

/// <summary>
/// 手柄按键绑定设置行（用于按键设置页的列表）
/// </summary>
public partial class GamepadBindingSettingModel : ObservableObject
{
    /// <summary>
    /// 绑定的手柄按键
    /// </summary>
    [ObservableProperty]
    private GamepadButton _buttonValue;

    /// <summary>
    /// 界面上显示的动作名称
    /// </summary>
    public string ActionName { get; }

    /// <summary>
    /// 对应 <see cref="BetterGenshinImpact.Core.Config.GamepadConfig"/> 上的属性名
    /// </summary>
    public string ConfigPropertyName { get; }

    public GamepadBindingSettingModel(string actionName, string configPropertyName, GamepadButton buttonValue)
    {
        ActionName = actionName;
        ConfigPropertyName = configPropertyName;
        ButtonValue = buttonValue;
    }
}
