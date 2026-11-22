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
