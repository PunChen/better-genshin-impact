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

        Assert.False(GamepadActionMap.TryGetButton(GIActions.OpenTheEventsMenu, config, out _));
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
        // 新增的任务类动作默认不绑定，避免未验证过的默认键位误触发游戏内行为
        var config = new GamepadConfig();

        Assert.Equal(GamepadButton.None, config.Drop);
        Assert.Equal(GamepadButton.None, config.SwitchMember1);
        Assert.Equal(GamepadButton.None, config.SwitchMember2);
        Assert.Equal(GamepadButton.None, config.SwitchMember3);
        Assert.Equal(GamepadButton.None, config.SwitchMember4);
        Assert.Equal(GamepadButton.None, config.SwitchMember5);
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
}
