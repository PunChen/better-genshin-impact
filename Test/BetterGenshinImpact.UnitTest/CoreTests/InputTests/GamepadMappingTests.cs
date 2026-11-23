using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.Core.Simulator.Extensions;
using Fischless.WindowsInput.Gamepad;

namespace BetterGenshinImpact.UnitTest.CoreTests.InputTests;

/// <summary>
/// 手柄按键映射测试：不依赖 ViGEmBus 驱动
/// </summary>
public class GamepadMappingTests
{
    [Fact]
    public void DefaultBindings_MatchGenshinControllerLayout()
    {
        // 与原神“设置-按键-战斗界面/系统界面”的默认手柄按键保持一致
        var config = new GamepadConfig();

        Assert.Equal(GamepadButton.A, config.NormalAttack);
        Assert.Equal(GamepadButton.RightTrigger, config.ElementalSkill);
        Assert.Equal(GamepadButton.Y, config.ElementalBurst);
        Assert.Equal(GamepadButton.RightShoulder, config.Sprint);
        Assert.Equal(GamepadButton.B, config.Jump);
        Assert.Equal(GamepadButton.X, config.PickUpOrInteract);

        Assert.Equal(GamepadButton.B, config.Confirm);
        Assert.Equal(GamepadButton.A, config.Cancel);
        Assert.Equal(GamepadButton.B, config.SkipDialog);
    }

    [Theory]
    [InlineData(GamepadButton.X)]
    [InlineData(GamepadButton.A)]
    [InlineData(GamepadButton.B)]
    [InlineData(GamepadButton.Y)]
    public void Confirm_CanBeBoundToAnyFaceButton(GamepadButton button)
    {
        var config = new GamepadConfig { Confirm = button };

        Assert.True(GamepadActionMap.TryGetButton(GIActions.Confirm, config, out var actual));
        Assert.Equal(button, actual);
    }

    [Fact]
    public void UnboundButton_IsReportedAsUnbound()
    {
        var config = new GamepadConfig { OpenMap = GamepadButton.None };

        Assert.True(GamepadActionMap.TryGetButton(GIActions.OpenMap, config, out var button));
        Assert.Equal(GamepadButton.None, button);
    }

    [Fact]
    public void UnsupportedAction_IsNotMapped()
    {
        var config = new GamepadConfig();

        Assert.False(GamepadActionMap.TryGetButton(GIActions.OpenWishScreen, config, out _));
    }

    [Fact]
    public void PartySwitchAndTaskActions_AreMappedWhenBound()
    {
        var config = new GamepadConfig
        {
            SwitchMember1 = GamepadButton.Up,
            SwitchMember2 = GamepadButton.Left,
            SwitchMember3 = GamepadButton.Right,
            SwitchMember4 = GamepadButton.Down,
            SwitchMember5 = GamepadButton.Guide,
            Drop = GamepadButton.B,
            InteractionInSomeMode = GamepadButton.X,
            OpenCharacterScreen = GamepadButton.Y,
            OpenAdventurerHandbook = GamepadButton.Back,
            OpenPartySetupScreen = GamepadButton.LeftShoulder,
        };

        Assert.True(GamepadActionMap.TryGetButton(GIActions.SwitchMember1, config, out var member1));
        Assert.Equal(GamepadButton.Up, member1);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.SwitchMember2, config, out var member2));
        Assert.Equal(GamepadButton.Left, member2);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.SwitchMember3, config, out var member3));
        Assert.Equal(GamepadButton.Right, member3);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.SwitchMember4, config, out var member4));
        Assert.Equal(GamepadButton.Down, member4);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.SwitchMember5, config, out var member5));
        Assert.Equal(GamepadButton.Guide, member5);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.Drop, config, out var drop));
        Assert.Equal(GamepadButton.B, drop);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.InteractionInSomeMode, config, out var interact));
        Assert.Equal(GamepadButton.X, interact);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.OpenCharacterScreen, config, out var characterScreen));
        Assert.Equal(GamepadButton.Y, characterScreen);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.OpenAdventurerHandbook, config, out var handbook));
        Assert.Equal(GamepadButton.Back, handbook);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.OpenPartySetupScreen, config, out var partySetup));
        Assert.Equal(GamepadButton.LeftShoulder, partySetup);
    }

    [Fact]
    public void TaskActions_DefaultToUnbound()
    {
        // 其余任务类动作默认不绑定，避免未验证过的默认键位误触发游戏内行为
        var config = new GamepadConfig();

        Assert.Equal(GamepadButton.None, config.Drop);
        Assert.Equal(GamepadButton.None, config.InteractionInSomeMode);
        Assert.Equal(GamepadButton.None, config.OpenCharacterScreen);
        Assert.Equal(GamepadButton.None, config.OpenAdventurerHandbook);
        Assert.Equal(GamepadButton.None, config.OpenPartySetupScreen);
    }

    [Fact]
    public void SprintKeyboardAndMouse_ShareSameBinding()
    {
        var config = new GamepadConfig { Sprint = GamepadButton.RightTrigger };

        Assert.True(GamepadActionMap.TryGetButton(GIActions.SprintKeyboard, config, out var keyboardButton));
        Assert.True(GamepadActionMap.TryGetButton(GIActions.SprintMouse, config, out var mouseButton));
        Assert.Equal(keyboardButton, mouseButton);
    }

    [Fact]
    public void Movement_CombinesHeldDirectionsIntoLeftStick()
    {
        var stick = new LeftStickState();

        stick.Set(GIActions.MoveForward, true);
        stick.Set(GIActions.MoveRight, true);
        Assert.Equal((short.MaxValue, short.MaxValue), stick.GetAxisValues());

        stick.Set(GIActions.MoveForward, false);
        Assert.Equal((short.MaxValue, 0), stick.GetAxisValues());
    }

    [Fact]
    public void Movement_OppositeDirectionsCancelOut()
    {
        var stick = new LeftStickState();

        stick.Set(GIActions.MoveLeft, true);
        Assert.Equal((-short.MaxValue, 0), stick.GetAxisValues());

        stick.Set(GIActions.MoveRight, true);
        Assert.Equal((0, 0), stick.GetAxisValues());
    }

    [Fact]
    public void Reset_ClearsAllDirections()
    {
        var stick = new LeftStickState();
        stick.Set(GIActions.MoveForward, true);
        stick.Set(GIActions.MoveLeft, true);

        stick.Reset();

        Assert.Equal((0, 0), stick.GetAxisValues());
    }

    [Fact]
    public void CameraRecenter_DefaultsToRightThumb()
    {
        var config = new GamepadConfig();

        Assert.Equal(GamepadButton.RightThumb, config.CameraRecenter);
        Assert.True(config.RightStickSensitivity > 0);
        Assert.True(config.RightStickPulseMs > 0);
        Assert.True(config.TurnMillisecondsPerDegree > 0);
    }

    [Fact]
    public void MenuActions_AreMappedToConfig()
    {
        var config = new GamepadConfig
        {
            OpenBattlePassScreen = GamepadButton.Back,
            OpenTheEventsMenu = GamepadButton.Guide,
            QuestNavigation = GamepadButton.LeftShoulder,
            ElementalSight = GamepadButton.RightThumb,
        };

        Assert.True(GamepadActionMap.TryGetButton(GIActions.OpenBattlePassScreen, config, out var battlePass));
        Assert.Equal(GamepadButton.Back, battlePass);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.OpenTheEventsMenu, config, out var events));
        Assert.Equal(GamepadButton.Guide, events);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.QuestNavigation, config, out var quest));
        Assert.Equal(GamepadButton.LeftShoulder, quest);

        Assert.True(GamepadActionMap.TryGetButton(GIActions.ElementalSight, config, out var sight));
        Assert.Equal(GamepadButton.RightThumb, sight);
    }

    [Fact]
    public void PartySwitchAndGadget_UseGamepadDefaults()
    {
        var config = new GamepadConfig();

        Assert.Equal(GamepadButton.Up, config.SwitchMember1);
        Assert.Equal(GamepadButton.Right, config.SwitchMember2);
        Assert.Equal(GamepadButton.Left, config.SwitchMember3);
        Assert.Equal(GamepadButton.Down, config.SwitchMember4);

        // 角色5 = LB + Y；快捷道具 = LB + B
        Assert.Equal(GamepadButton.Y, config.SwitchMember5);
        Assert.Equal(GamepadButton.LeftShoulder, config.SwitchMember5Modifier);
        Assert.Equal(GamepadButton.B, config.QuickUseGadget);
        Assert.Equal(GamepadButton.LeftShoulder, config.QuickUseGadgetModifier);

        Assert.Equal(GamepadButton.Back, config.OpenChatScreen);
    }

    [Fact]
    public void ComboActions_ExposeModifier()
    {
        var config = new GamepadConfig();

        Assert.True(GamepadActionMap.TryGetModifier(GIActions.SwitchMember5, config, out var switchModifier));
        Assert.Equal(GamepadButton.LeftShoulder, switchModifier);

        Assert.True(GamepadActionMap.TryGetModifier(GIActions.QuickUseGadget, config, out var gadgetModifier));
        Assert.Equal(GamepadButton.LeftShoulder, gadgetModifier);

        Assert.False(GamepadActionMap.TryGetModifier(GIActions.Confirm, config, out _));
    }

    [Fact]
    public void WheelMenuBindings_DefaultDirections()
    {
        var config = new GamepadConfig();

        Assert.Equal(GamepadStickDirection.Down, config.OpenMapWheelDirection);
        Assert.Equal(GamepadStickDirection.Right, config.OpenInventoryWheelDirection);
        Assert.Equal(GamepadStickDirection.DownLeft, config.OpenQuestMenuWheelDirection);
        Assert.Equal(GamepadStickDirection.DownRight, config.OpenCharacterScreenWheelDirection);
        Assert.Equal(GamepadStickDirection.UpLeft, config.OpenBattlePassScreenWheelDirection);
        Assert.Equal(GamepadStickDirection.Left, config.OpenTheEventsMenuWheelDirection);
        Assert.Equal(GamepadStickDirection.UpRight, config.OpenAdventurerHandbookWheelDirection);

        Assert.True(GamepadActionMap.TryGetWheel(GIActions.OpenMap, config, out var modifier, out var direction));
        Assert.Equal(GamepadButton.LeftShoulder, modifier);
        Assert.Equal(GamepadStickDirection.Down, direction);

        Assert.False(GamepadActionMap.TryGetWheel(GIActions.Confirm, config, out _, out _));
    }
}
