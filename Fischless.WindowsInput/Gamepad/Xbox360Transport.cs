using System;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Fischless.WindowsInput.Gamepad;

/// <summary>
/// 基于 ViGEmBus 的虚拟 Xbox360 手柄。
/// 需要用户预先安装 ViGEmBus 驱动；驱动缺失时只会返回失败，不会抛出异常，调用方可以回退到键鼠。
/// </summary>
public sealed class Xbox360Transport : IGamepadTransport
{
    private readonly object _lock = new();
    private ViGEmClient? _client;
    private IXbox360Controller? _controller;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsConnected { get; private set; }

    /// <inheritdoc />
    public string? LastError { get; private set; }

    /// <inheritdoc />
    public bool TryConnect()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return false;
            }

            if (IsConnected)
            {
                return true;
            }

            try
            {
                _client ??= new ViGEmClient();
                if (_controller is null)
                {
                    _controller = _client.CreateXbox360Controller();
                }

                _controller.Connect();
                IsConnected = true;
                LastError = null;
                return true;
            }
            catch (Exception e)
            {
                // 最常见的原因是未安装 ViGEmBus 驱动（VigemBusNotFoundException）
                LastError = e.Message;
                IsConnected = false;
                DisposeController();
                return false;
            }
        }
    }

    /// <inheritdoc />
    public void Disconnect()
    {
        lock (_lock)
        {
            if (IsConnected)
            {
                try
                {
                    _controller?.Disconnect();
                }
                catch (Exception e)
                {
                    LastError = e.Message;
                }
            }

            IsConnected = false;
            DisposeController();
        }
    }

    /// <inheritdoc />
    public void SetButton(GamepadButton button, bool pressed)
    {
        if (button == GamepadButton.None)
        {
            return;
        }

        lock (_lock)
        {
            if (!IsConnected || _controller is null)
            {
                return;
            }

            try
            {
                // 扳机键是滑块，不能使用 SetButtonState
                switch (button)
                {
                    case GamepadButton.LeftTrigger:
                        _controller.SetSliderValue(Xbox360Slider.LeftTrigger, pressed ? byte.MaxValue : byte.MinValue);
                        return;
                    case GamepadButton.RightTrigger:
                        _controller.SetSliderValue(Xbox360Slider.RightTrigger, pressed ? byte.MaxValue : byte.MinValue);
                        return;
                }

                if (TryToXbox360Button(button, out var xboxButton))
                {
                    _controller.SetButtonState(xboxButton, pressed);
                }
            }
            catch (Exception e)
            {
                LastError = e.Message;
                IsConnected = false;
            }
        }
    }

    /// <inheritdoc />
    public void SetAxis(GamepadAxis axis, short value)
    {
        lock (_lock)
        {
            if (!IsConnected || _controller is null)
            {
                return;
            }

            try
            {
                _controller.SetAxisValue(ToXbox360Axis(axis), value);
            }
            catch (Exception e)
            {
                LastError = e.Message;
                IsConnected = false;
            }
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        lock (_lock)
        {
            if (!IsConnected || _controller is null)
            {
                return;
            }

            try
            {
                foreach (GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
                {
                    if (button == GamepadButton.None)
                    {
                        continue;
                    }

                    if (button == GamepadButton.LeftTrigger)
                    {
                        _controller.SetSliderValue(Xbox360Slider.LeftTrigger, byte.MinValue);
                    }
                    else if (button == GamepadButton.RightTrigger)
                    {
                        _controller.SetSliderValue(Xbox360Slider.RightTrigger, byte.MinValue);
                    }
                    else if (TryToXbox360Button(button, out var xboxButton))
                    {
                        _controller.SetButtonState(xboxButton, false);
                    }
                }

                _controller.SetAxisValue(Xbox360Axis.LeftThumbX, 0);
                _controller.SetAxisValue(Xbox360Axis.LeftThumbY, 0);
                _controller.SetAxisValue(Xbox360Axis.RightThumbX, 0);
                _controller.SetAxisValue(Xbox360Axis.RightThumbY, 0);
            }
            catch (Exception e)
            {
                LastError = e.Message;
                IsConnected = false;
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (IsConnected)
            {
                try
                {
                    _controller?.Disconnect();
                }
                catch (Exception e)
                {
                    LastError = e.Message;
                }
            }

            IsConnected = false;
            DisposeController();
            try
            {
                _client?.Dispose();
            }
            catch (Exception e)
            {
                LastError = e.Message;
            }

            _client = null;
        }
    }

    private void DisposeController()
    {
        if (_controller is null)
        {
            return;
        }

        try
        {
            // IXbox360Controller 没有公开 IDisposable，但具体实现有，这里按需释放
            (_controller as IDisposable)?.Dispose();
        }
        catch
        {
            // 忽略断开过程中的异常
        }

        _controller = null;
    }

    private static bool TryToXbox360Button(GamepadButton button, out Xbox360Button xboxButton)
    {
        switch (button)
        {
            case GamepadButton.A:
                xboxButton = Xbox360Button.A;
                return true;
            case GamepadButton.B:
                xboxButton = Xbox360Button.B;
                return true;
            case GamepadButton.X:
                xboxButton = Xbox360Button.X;
                return true;
            case GamepadButton.Y:
                xboxButton = Xbox360Button.Y;
                return true;
            case GamepadButton.LeftShoulder:
                xboxButton = Xbox360Button.LeftShoulder;
                return true;
            case GamepadButton.RightShoulder:
                xboxButton = Xbox360Button.RightShoulder;
                return true;
            case GamepadButton.LeftThumb:
                xboxButton = Xbox360Button.LeftThumb;
                return true;
            case GamepadButton.RightThumb:
                xboxButton = Xbox360Button.RightThumb;
                return true;
            case GamepadButton.Up:
                xboxButton = Xbox360Button.Up;
                return true;
            case GamepadButton.Down:
                xboxButton = Xbox360Button.Down;
                return true;
            case GamepadButton.Left:
                xboxButton = Xbox360Button.Left;
                return true;
            case GamepadButton.Right:
                xboxButton = Xbox360Button.Right;
                return true;
            case GamepadButton.Start:
                xboxButton = Xbox360Button.Start;
                return true;
            case GamepadButton.Back:
                xboxButton = Xbox360Button.Back;
                return true;
            case GamepadButton.Guide:
                xboxButton = Xbox360Button.Guide;
                return true;
            default:
                xboxButton = null!;
                return false;
        }
    }

    private static Xbox360Axis ToXbox360Axis(GamepadAxis axis)
    {
        return axis switch
        {
            GamepadAxis.LeftThumbX => Xbox360Axis.LeftThumbX,
            GamepadAxis.LeftThumbY => Xbox360Axis.LeftThumbY,
            GamepadAxis.RightThumbX => Xbox360Axis.RightThumbX,
            GamepadAxis.RightThumbY => Xbox360Axis.RightThumbY,
            _ => Xbox360Axis.LeftThumbX,
        };
    }
}
