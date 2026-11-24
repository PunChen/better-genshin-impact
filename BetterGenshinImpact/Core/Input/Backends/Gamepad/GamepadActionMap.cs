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
            case GIActions.Drop:
                button = config.Drop;
                return true;
            case GIActions.SwitchMember1:
                button = config.SwitchMember1;
                return true;
            case GIActions.SwitchMember2:
                button = config.SwitchMember2;
                return true;
            case GIActions.SwitchMember3:
                button = config.SwitchMember3;
                return true;
            case GIActions.SwitchMember4:
                button = config.SwitchMember4;
                return true;
            case GIActions.SwitchMember5:
                button = config.SwitchMember5;
                return true;
            case GIActions.InteractionInSomeMode:
                button = config.InteractionInSomeMode;
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
            case GIActions.OpenChatScreen:
                button = config.OpenChatScreen;
                return true;
            case GIActions.OpenQuestMenu:
                button = config.OpenQuestMenu;
                return true;
            case GIActions.OpenCharacterScreen:
                button = config.OpenCharacterScreen;
                return true;
            case GIActions.OpenAdventurerHandbook:
                button = config.OpenAdventurerHandbook;
                return true;
            case GIActions.OpenPartySetupScreen:
                button = config.OpenPartySetupScreen;
                return true;
            case GIActions.OpenBattlePassScreen:
                button = config.OpenBattlePassScreen;
                return true;
            case GIActions.OpenTheEventsMenu:
                button = config.OpenTheEventsMenu;
                return true;
            case GIActions.QuestNavigation:
                button = config.QuestNavigation;
                return true;
            case GIActions.ElementalSight:
                button = config.ElementalSight;
                return true;
            case GIActions.MapZoomIn:
                button = config.MapZoomIn;
                return true;
            case GIActions.MapZoomOut:
                button = config.MapZoomOut;
                return true;
            default:
                button = GamepadButton.None;
                return false;
        }
    }

    /// <summary>
    /// 尝试取得动作对应的组合键修饰键（如 LB + Y）。没有组合键时返回 false。
    /// </summary>
    public static bool TryGetModifier(GIActions action, GamepadConfig config, out GamepadButton modifier)
    {
        switch (action)
        {
            case GIActions.SwitchMember5:
                modifier = config.SwitchMember5Modifier;
                return true;
            case GIActions.QuickUseGadget:
                modifier = config.QuickUseGadgetModifier;
                return true;
            case GIActions.ElementalSight:
                modifier = config.ElementalSightModifier;
                return true;
            case GIActions.InteractionInSomeMode:
                modifier = config.InteractionInSomeModeModifier;
                return true;
            default:
                modifier = GamepadButton.None;
                return false;
        }
    }

    /// <summary>
    /// 尝试取得“快捷轮盘”绑定（按住修饰键 + 右摇杆方向）。
    /// 返回 false 表示该动作没有轮盘绑定或方向为空。
    /// </summary>
    public static bool TryGetWheel(GIActions action, GamepadConfig config, out GamepadButton modifier, out GamepadStickDirection direction)
    {
        switch (action)
        {
            case GIActions.OpenMap:
                modifier = config.OpenMapWheelModifier;
                direction = config.OpenMapWheelDirection;
                break;
            case GIActions.OpenInventory:
                modifier = config.OpenInventoryWheelModifier;
                direction = config.OpenInventoryWheelDirection;
                break;
            case GIActions.OpenQuestMenu:
                modifier = config.OpenQuestMenuWheelModifier;
                direction = config.OpenQuestMenuWheelDirection;
                break;
            case GIActions.OpenCharacterScreen:
                modifier = config.OpenCharacterScreenWheelModifier;
                direction = config.OpenCharacterScreenWheelDirection;
                break;
            case GIActions.OpenBattlePassScreen:
                modifier = config.OpenBattlePassScreenWheelModifier;
                direction = config.OpenBattlePassScreenWheelDirection;
                break;
            case GIActions.OpenTheEventsMenu:
                modifier = config.OpenTheEventsMenuWheelModifier;
                direction = config.OpenTheEventsMenuWheelDirection;
                break;
            case GIActions.OpenAdventurerHandbook:
                modifier = config.OpenAdventurerHandbookWheelModifier;
                direction = config.OpenAdventurerHandbookWheelDirection;
                break;
            default:
                modifier = GamepadButton.None;
                direction = GamepadStickDirection.None;
                return false;
        }

        return modifier != GamepadButton.None && direction != GamepadStickDirection.None;
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
            case GIActions.FocusUp:
            case GIActions.ScrollUp:
                x = 0;
                y = 1;
                return true;
            case GIActions.FocusDown:
            case GIActions.ScrollDown:
                x = 0;
                y = -1;
                return true;
            case GIActions.FocusLeft:
                x = -1;
                y = 0;
                return true;
            case GIActions.FocusRight:
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
