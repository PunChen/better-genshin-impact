using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.Core.Simulator.Gamepad;
using Fischless.WindowsInput.Gamepad;

namespace BetterGenshinImpact.UnitTest.CoreTests.SimulatorTests;

/// <summary>
/// 手柄动作分发测试：不依赖 ViGEmBus 驱动，用假传输层验证映射与状态机
/// </summary>
public class GamepadDispatchTests
{
    [Theory]
    [InlineData(GamepadButton.X)]
    [InlineData(GamepadButton.A)]
    [InlineData(GamepadButton.B)]
    [InlineData(GamepadButton.Y)]
    public void Confirm_CanBeBoundToAnyFaceButton(GamepadButton button)
    {
        var config = new GamepadConfig { Enabled = true, PressDurationMs = 1, Confirm = button };
        var transport = new FakeGamepadTransport();
        var dispatcher = new GamepadActionDispatcher(transport);

        Assert.True(dispatcher.TrySimulate(GIActions.Confirm, KeyType.KeyPress, config));
        Assert.Equal([(button, true), (button, false)], transport.ButtonCalls);
    }

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

        // 对话推进与确认是同一个键（B）
        Assert.Equal(GamepadButton.B, config.SkipDialog);
    }

    [Fact]
    public void DisabledMode_FallsBackToKeyboard()
    {
        var config = new GamepadConfig { Enabled = false };
        var transport = new FakeGamepadTransport();
        var dispatcher = new GamepadActionDispatcher(transport);

        Assert.False(dispatcher.TrySimulate(GIActions.Confirm, KeyType.KeyPress, config));
        Assert.Empty(transport.ButtonCalls);
    }

    [Fact]
    public void UnboundButton_FallsBackToKeyboard()
    {
        var config = new GamepadConfig { Enabled = true, OpenMap = GamepadButton.None };
        var transport = new FakeGamepadTransport();
        var dispatcher = new GamepadActionDispatcher(transport);

        Assert.False(dispatcher.TrySimulate(GIActions.OpenMap, KeyType.KeyPress, config));
        Assert.Empty(transport.ButtonCalls);
    }

    [Fact]
    public void UnsupportedAction_FallsBackToKeyboard()
    {
        var config = new GamepadConfig { Enabled = true };
        var transport = new FakeGamepadTransport();
        var dispatcher = new GamepadActionDispatcher(transport);

        Assert.False(dispatcher.TrySimulate(GIActions.OpenTheEventsMenu, KeyType.KeyPress, config));
    }

    [Fact]
    public void SprintKeyboardAndMouse_ShareSameBinding()
    {
        var config = new GamepadConfig { Enabled = true, PressDurationMs = 1, Sprint = GamepadButton.RightTrigger };
        var transport = new FakeGamepadTransport();
        var dispatcher = new GamepadActionDispatcher(transport);

        Assert.True(dispatcher.TrySimulate(GIActions.SprintKeyboard, KeyType.KeyPress, config));
        Assert.True(dispatcher.TrySimulate(GIActions.SprintMouse, KeyType.KeyPress, config));
        Assert.Equal(4, transport.ButtonCalls.Count);
        Assert.All(transport.ButtonCalls, call => Assert.Equal(GamepadButton.RightTrigger, call.Button));
    }

    [Fact]
    public void Movement_CombinesHeldDirectionsIntoLeftStick()
    {
        var config = new GamepadConfig { Enabled = true };
        var transport = new FakeGamepadTransport();
        var dispatcher = new GamepadActionDispatcher(transport);

        Assert.True(dispatcher.TrySimulate(GIActions.MoveForward, KeyType.KeyDown, config));
        Assert.True(dispatcher.TrySimulate(GIActions.MoveRight, KeyType.KeyDown, config));

        Assert.Equal(short.MaxValue, transport.GetAxis(GamepadAxis.LeftThumbX));
        Assert.Equal(short.MaxValue, transport.GetAxis(GamepadAxis.LeftThumbY));

        Assert.True(dispatcher.TrySimulate(GIActions.MoveForward, KeyType.KeyUp, config));
        Assert.Equal(short.MaxValue, transport.GetAxis(GamepadAxis.LeftThumbX));
        Assert.Equal(0, transport.GetAxis(GamepadAxis.LeftThumbY));
    }

    [Fact]
    public void Movement_OppositeDirectionsCancelOut()
    {
        var config = new GamepadConfig { Enabled = true };
        var transport = new FakeGamepadTransport();
        var dispatcher = new GamepadActionDispatcher(transport);

        dispatcher.TrySimulate(GIActions.MoveLeft, KeyType.KeyDown, config);
        Assert.Equal(-short.MaxValue, transport.GetAxis(GamepadAxis.LeftThumbX));

        dispatcher.TrySimulate(GIActions.MoveRight, KeyType.KeyDown, config);
        Assert.Equal(0, transport.GetAxis(GamepadAxis.LeftThumbX));
    }

    [Fact]
    public void ReleaseAll_ResetsStickAndTransport()
    {
        var config = new GamepadConfig { Enabled = true };
        var transport = new FakeGamepadTransport();
        var dispatcher = new GamepadActionDispatcher(transport);

        dispatcher.TrySimulate(GIActions.MoveForward, KeyType.KeyDown, config);
        dispatcher.ReleaseAll();

        Assert.True(transport.ResetCalled);
    }

    [Fact]
    public void Press_ReturnsFalse_WhenTransportDisconnected()
    {
        var config = new GamepadConfig { Enabled = true };
        var transport = new FakeGamepadTransport { IsConnected = false };
        var dispatcher = new GamepadActionDispatcher(transport);

        Assert.False(dispatcher.TrySimulate(GIActions.Confirm, KeyType.KeyPress, config));
        Assert.Empty(transport.ButtonCalls);
    }

    private sealed class FakeGamepadTransport : IGamepadTransport
    {
        private readonly Dictionary<GamepadAxis, short> _axes = new();

        public List<(GamepadButton Button, bool Pressed)> ButtonCalls { get; } = [];

        public bool ResetCalled { get; private set; }

        public bool IsConnected { get; set; } = true;

        public string? LastError => null;

        public bool TryConnect()
        {
            IsConnected = true;
            return true;
        }

        public void Disconnect()
        {
            IsConnected = false;
        }

        public void SetButton(GamepadButton button, bool pressed)
        {
            ButtonCalls.Add((button, pressed));
        }

        public void SetAxis(GamepadAxis axis, short value)
        {
            _axes[axis] = value;
        }

        public short GetAxis(GamepadAxis axis)
        {
            return _axes.TryGetValue(axis, out var value) ? value : (short)0;
        }

        public void Reset()
        {
            ResetCalled = true;
            _axes.Clear();
        }

        public void Dispose()
        {
        }
    }
}
