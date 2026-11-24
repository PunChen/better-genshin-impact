using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Recognition.OCR;
using BetterGenshinImpact.GameTask.Common;
using BetterGenshinImpact.GameTask.Common.BgiVision;
using BetterGenshinImpact.GameTask.Model.Area;
using BetterGenshinImpact.GameTask;
using Fischless.WindowsInput.Gamepad;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace BetterGenshinImpact.Core.Input;

/// <summary>
/// 手柄按键设置页的“一键集成测试”：直接驱动一个临时虚拟 Xbox360 手柄，
/// 按固定用例走“打开地图 → 区域列表 → 选择蒙德 → 确认传送”的流程。
/// 需要游戏已切到手柄模式，并且 BGI 未在运行其他手柄任务（避免抢占虚拟手柄）。
/// </summary>
public static class GamepadIntegrationTestRunner
{
    private static readonly object LogLock = new();

    /// <summary>
    /// 一键测试：传送到蒙德城
    /// </summary>
    public static async Task RunTeleportToMondstadtAsync(Action<string> log, CancellationToken ct = default)
    {
        await Task.Run(async () =>
        {
            void Log(string message)
            {
                var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
                WriteLogFile(line);
                log(line);
            }

            Log("开始：传送到蒙德城");
            if (InputHub.Backend is Backends.Gamepad.GamepadInputBackend)
            {
                throw new InvalidOperationException("当前有任务正在使用手柄后端，请先停止任务后再运行一键测试");
            }

            var handle = FindGameWindowHandle();
            if (handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("未找到游戏窗口，请先启动原神");
            }

            SystemControl.FocusWindow(handle);
            Log($"已聚焦游戏窗口，句柄={handle}");
            if (!TaskContext.Instance().IsInitialized)
            {
                Log("警告：BGI 尚未绑定游戏窗口，地图识别/OCR 可能不可用；建议先在主界面开始一次任务后再测试");
            }

            await Task.Delay(800, ct);

            using var transport = new Xbox360Transport();
            if (!transport.TryConnect())
            {
                throw new InvalidOperationException(transport.LastError ?? "虚拟手柄连接失败，请检查 ViGEmBus 驱动");
            }

            Log("虚拟 Xbox360 手柄已连接");

            // 1. 打开地图：LB + 右摇杆↓。按住时间过短会选不中轮盘项，这里加长并做视觉复核/重试。
            Log("步骤 1/5：打开地图（LB + 右摇杆↓，按住 350ms）");
            var mapOpened = false;
            for (var attempt = 1; attempt <= 3 && !mapOpened; attempt++)
            {
                PressWheel(transport, GamepadButton.LeftShoulder, GamepadStickDirection.Down, 350);
                await Task.Delay(1500, ct);
                mapOpened = IsBigMapUi(Log);
                Log(mapOpened ? $"地图已打开（第 {attempt} 次尝试）" : $"第 {attempt} 次打开地图未检测到地图界面，重试…");
            }

            if (!mapOpened)
            {
                SaveScreenshot("01_map_failed");
                throw new InvalidOperationException("打开地图失败：连续 3 次未识别到大地图界面");
            }

            SaveScreenshot("01_map");

            // 2. 放大到最大：大地图 LT 放大，循环直到缩放级别接近 1
            Log("步骤 2/5：放大地图到最大（LT）");
            for (var i = 0; i < 6; i++)
            {
                Press(transport, GamepadButton.LeftTrigger, 600);
                await Task.Delay(400, ct);
                var scale = TryGetBigMapScale();
                Log(scale is null ? $"第 {i + 1} 次放大（缩放级别不可用）" : $"第 {i + 1} 次放大，缩放级别={scale:F3}");
                if (scale >= 0.97)
                {
                    break;
                }
            }

            SaveScreenshot("02_map_max_zoom");

            // 3. 打开区域列表：Y
            Log("步骤 3/5：打开区域列表（Y）");
            Press(transport, GamepadButton.Y, 80);
            await Task.Delay(1000, ct);
            SaveScreenshot("03_region_list");
            var listText = TryOcrScreen();
            Log($"区域列表 OCR：{Shorten(listText)}");

            // 4. 选择蒙德：左摇杆向上滚到列表顶部，B 确认，再用 OCR 确认选中
            Log("步骤 4/5：选择蒙德（左摇杆↑ + B）");
            for (var i = 0; i < 5; i++)
            {
                PressStick(transport, GamepadAxis.LeftThumbX, GamepadAxis.LeftThumbY, 0, 1, 150);
                await Task.Delay(120, ct);
            }

            await Task.Delay(300, ct);
            Press(transport, GamepadButton.B, 80);
            await Task.Delay(1500, ct);
            SaveScreenshot("04_region_selected");

            var selectedText = TryOcrScreen();
            Log($"选区后 OCR：{Shorten(selectedText)}");
            if (!string.IsNullOrWhiteSpace(selectedText) && !selectedText.Contains("蒙德"))
            {
                throw new InvalidOperationException("OCR 未在画面中确认到「蒙德」，可能区域列表未选中蒙德");
            }

            // 5. 确认传送：B（两段式：默认传送点 → 传送）
            Log("步骤 5/5：确认传送（B）");
            Press(transport, GamepadButton.B, 80);
            await Task.Delay(3000, ct);
            SaveScreenshot("05_teleported");

            Log("测试完成：请检查是否已到达蒙德城");
        }, ct);
    }

    private static bool IsBigMapUi(Action<string> log)
    {
        try
        {
            using var region = TaskControl.CaptureToRectArea();
            var hit = Bv.IsInBigMapUi(region);
            log($"地图识别：截图 {region.Width}x{region.Height}，结果={hit}");
            return hit;
        }
        catch (Exception e)
        {
            log($"地图识别异常：{e.Message}");
            return false;
        }
    }

    private static double? TryGetBigMapScale()
    {
        try
        {
            using var region = TaskControl.CaptureToRectArea();
            return Bv.GetBigMapScale(region);
        }
        catch
        {
            return null;
        }
    }

    private static string TryOcrScreen()
    {
        try
        {
            using var region = TaskControl.CaptureToRectArea();
            return OcrFactory.Paddle.Ocr(region.SrcMat) ?? string.Empty;
        }
        catch (Exception e)
        {
            WriteLogFile($"[{DateTime.Now:HH:mm:ss.fff}] OCR 异常：{e.Message}");
            return string.Empty;
        }
    }

    private static string Shorten(string text)
    {
        var oneLine = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
        return oneLine.Length <= 200 ? oneLine : oneLine[..200] + "…";
    }

    private static Mat CaptureScreenMat()
    {
        var width = GetSystemMetrics(0);
        var height = GetSystemMetrics(1);
        using var bitmap = new Bitmap(width, height);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
        }

        return BitmapConverter.ToMat(bitmap);
    }

    private static void Press(Xbox360Transport transport, GamepadButton button, int durationMs)
    {
        transport.SetButton(button, true);
        Thread.Sleep(durationMs);
        transport.SetButton(button, false);
    }

    private static void PressWheel(Xbox360Transport transport, GamepadButton modifier, GamepadStickDirection direction, int holdMs)
    {
        var (x, y) = direction switch
        {
            GamepadStickDirection.Down => ((short)0, (short)-short.MaxValue),
            GamepadStickDirection.Up => ((short)0, short.MaxValue),
            GamepadStickDirection.Left => ((short)-short.MaxValue, (short)0),
            GamepadStickDirection.Right => (short.MaxValue, (short)0),
            _ => ((short)0, (short)0),
        };

        transport.SetButton(modifier, true);
        Thread.Sleep(60);
        transport.SetAxis(GamepadAxis.RightThumbX, x);
        transport.SetAxis(GamepadAxis.RightThumbY, y);
        Thread.Sleep(holdMs);
        transport.SetAxis(GamepadAxis.RightThumbX, 0);
        transport.SetAxis(GamepadAxis.RightThumbY, 0);
        Thread.Sleep(30);
        transport.SetButton(modifier, false);
    }

    private static void PressStick(Xbox360Transport transport, GamepadAxis xAxis, GamepadAxis yAxis, double x, double y, int durationMs)
    {
        transport.SetAxis(xAxis, (short)(x * short.MaxValue));
        transport.SetAxis(yAxis, (short)(y * short.MaxValue));
        Thread.Sleep(durationMs);
        transport.SetAxis(xAxis, 0);
        transport.SetAxis(yAxis, 0);
    }

    private static IntPtr FindGameWindowHandle()
    {
        var context = TaskContext.Instance();
        if (context.IsInitialized && context.GameHandle != IntPtr.Zero)
        {
            return context.GameHandle;
        }

        foreach (var processName in new[] { "YuanShen", "GenshinImpact" })
        {
            var process = Process.GetProcessesByName(processName)
                .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
            if (process is not null)
            {
                return process.MainWindowHandle;
            }
        }

        return IntPtr.Zero;
    }

    private static void SaveScreenshot(string name)
    {
        try
        {
            var dir = Global.Absolute("log/gamepad-integration-test");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{DateTime.Now:yyyyMMdd-HHmmss}-{name}.png");

            var width = GetSystemMetrics(0);
            var height = GetSystemMetrics(1);
            using var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }

            bitmap.Save(path, ImageFormat.Png);
        }
        catch
        {
            // 截图失败不影响测试流程
        }
    }

    private static void WriteLogFile(string line)
    {
        try
        {
            var dir = Global.Absolute("log");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"gamepad-integration-test-{DateTime.Now:yyyyMMdd}.log");
            lock (LogLock)
            {
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }
        catch
        {
            // 日志失败不影响测试
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);
}
