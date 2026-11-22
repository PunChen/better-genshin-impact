using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Simulator.Extensions;
using Fischless.WindowsInput.Gamepad;

namespace BetterGenshinImpact.Core.Input.Backends.Gamepad;

/// <summary>
/// 游戏动作（<see cref="GIActions"/>）到手柄按键/摇杆的映射。
/// 与 <c>SimulateKeyHelper</c> 的键鼠映射一一对应，未列出的动作表示“手柄不支持”，会自动回退键鼠。
/// </summary>
public static class GamepadActionMap
{
    /// <summary>
    /// 尝试取得动作对应的手柄按键。返回 false 表示该动作没有绑定手柄。
    /// </summary>
    public static bool TryGetButton(GIActions action, GamepadConfig config, out GamepadButton button)
    {
        switch (action)
        {
            case GIActions.Confirm:
                button = config.Confirm;
                return true;
            case GIActions.Cancel:
                button = config.Cancel;
                return true;
            case GIActions.SkipDialog:
                button = config.SkipDialog;
                return true;
            case GIActions.PickUpOrInteract:
                button = config.PickUpOrInteract;
                return true;
            case GIActions.NormalAttack:
                button = config.NormalAttack;
                return true;
            case GIActions.ElementalSkill:
                button = config.ElementalSkill;
                return true;
            case GIActions.ElementalBurst:
                button = config.ElementalBurst;
                return true;
            case GIActions.Jump:
                button = config.Jump;
                return true;
            case GIActions.SprintKeyboard:
            case GIActions.SprintMouse:
                button = config.Sprint;
                return true;
            case GIActions.SwitchAimingMode:
                button = config.SwitchAimingMode;
                return true;
            case GIActions.SwitchToWalkOrRun:
                button = config.SwitchToWalkOrRun;
                return true;
            case GIActions.QuickUseGadget:
                button = config.QuickUseGadget;
                return true;
            case GIActions.OpenMap:
                button = config.OpenMap;
                return true;
            case GIActions.OpenInventory:
                button = config.OpenInventory;
                return true;
            case GIActions.OpenPaimonMenu:
                button = config.OpenPaimonMenu;
                return true;
            case GIActions.OpenQuestMenu:
                button = config.OpenQuestMenu;
                return true;
            default:
                button = GamepadButton.None;
                return false;
        }
    }

    /// <summary>
    /// 尝试取得动作对应的左摇杆方向（x/y 取值 -1、0、1）
    /// </summary>
    public static bool TryGetDirection(GIActions action, out int x, out int y)
    {
        switch (action)
        {
            case GIActions.MoveForward:
                x = 0;
                y = 1;
                return true;
            case GIActions.MoveBackward:
                x = 0;
                y = -1;
                return true;
            case GIActions.MoveLeft:
                x = -1;
                y = 0;
                return true;
            case GIActions.MoveRight:
                x = 1;
                y = 0;
                return true;
            default:
                x = 0;
                y = 0;
                return false;
        }
    }
}
