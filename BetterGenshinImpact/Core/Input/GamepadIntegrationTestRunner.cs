using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Recognition.OCR;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.Common;
using BetterGenshinImpact.GameTask.Common.BgiVision;
using BetterGenshinImpact.GameTask.Runtime;
using Fischless.WindowsInput.Gamepad;
using Newtonsoft.Json;

namespace BetterGenshinImpact.Core.Input;

/// <summary>
/// 手柄模式实机测试运行器：既支持设置页“一键测试”，也支持
/// <c>BetterGI.exe --gamepad-test &lt;case.json&gt;</c> 启动后执行外置步骤文件。
/// 需要游戏已切到手柄模式，且没有其他任务占用游戏手柄后端。
/// </summary>
public static class GamepadIntegrationTestRunner
{
    private static readonly object LogLock = new();

    /// <summary>
    /// 设置页一键测试：传送到蒙德城（固定等价步骤）
    /// </summary>
    public static Task RunTeleportToMondstadtAsync(Action<string> log, CancellationToken ct = default)
    {
        return RunStepsAsync(DefaultTeleportSteps(), log, null, ct);
    }

    /// <summary>
    /// 读取外置 JSON 步骤文件并执行（命令行 --gamepad-test 使用）
    /// </summary>
    public static async Task RunCaseAsync(string casePath, Action<string> log, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(casePath) || !File.Exists(casePath))
        {
            throw new FileNotFoundException($"测试步骤文件不存在: {casePath}");
        }

        var json = await File.ReadAllTextAsync(casePath, ct);
        var testCase = JsonConvert.DeserializeObject<GamepadTestCase>(json)
                       ?? throw new InvalidOperationException($"测试步骤文件解析失败: {casePath}");

        log($"[{DateTime.Now:HH:mm:ss.fff}] 加载用例：{testCase.Name}（{testCase.Steps.Count} 步）");
        await RunStepsAsync(testCase.Steps, log, Path.GetDirectoryName(Path.GetFullPath(casePath)), ct);
    }

    private static List<GamepadTestStep> DefaultTeleportSteps()
    {
        return
        [
            new() { Type = "openMap", Modifier = "LeftShoulder", Direction = "Down", HoldMs = 350, Ms = 1500, Retries = 3, Screenshot = "01_map" },
            new() { Type = "zoomMax", Button = "LeftTrigger", DurationMs = 600, Ms = 400, Attempts = 6, TargetScale = 0.97, Screenshot = "02_map_max_zoom" },
            new() { Type = "press", Button = "Y", Ms = 1000, Screenshot = "03_region_list" },
            new() { Type = "stick", Stick = "Left", X = 0, Y = 1, DurationMs = 150, Repeat = 5, Ms = 300 },
            new() { Type = "press", Button = "B", Ms = 1500, Screenshot = "04_region_selected" },
            new() { Type = "ocrAssert", Text = "蒙德" },
            new() { Type = "press", Button = "B", Ms = 3000, Screenshot = "05_teleported" },
        ];
    }

    private static async Task RunStepsAsync(
        IReadOnlyList<GamepadTestStep> steps,
        Action<string> log,
        string? caseDir,
        CancellationToken ct)
    {
        await Task.Run(async () =>
        {
            void Log(string message)
            {
                var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
                WriteLogFile(line);
                log(line);
            }

            Log("开始手柄测试");

            // 设置页一键测试时 BGI 可能只是打开着、尚未启动截图器；先补启动，保证 Bv/OCR 可用
            if (!TaskContext.Instance().IsInitialized)
            {
                var runtimeService = App.GetService<GameRuntimeService>();
                if (runtimeService is null)
                {
                    throw new InvalidOperationException("截图器未初始化，且 GameRuntimeService 不可用");
                }

                Log("截图器未启动，正在启动游戏捕获…");
                if (!await runtimeService.StartAsync())
                {
                    throw new InvalidOperationException("截图器启动失败，请确认游戏已启动");
                }

                Log("游戏捕获已启动");
            }

            if (!TaskContext.Instance().IsInitialized)
            {
                throw new InvalidOperationException("截图器仍未绑定游戏窗口，请确认游戏已启动且分辨率符合要求");
            }

            if (InputHub.Backend is Backends.Gamepad.GamepadInputBackend)
            {
                throw new InvalidOperationException("当前有任务正在使用手柄后端，请先停止任务后再运行测试");
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
                Log("警告：BGI 尚未绑定游戏窗口，地图识别/OCR 可能不可用");
            }
            else if (!TaskContext.Instance().Config.GamepadConfig.Enabled)
            {
                throw new InvalidOperationException("手柄模式未启用，请先在「按键绑定设置 → 手柄」中开启");
            }

            await Task.Delay(800, ct);

            using var transport = new Xbox360Transport();
            if (!transport.TryConnect())
            {
                throw new InvalidOperationException(transport.LastError ?? "虚拟手柄连接失败，请检查 ViGEmBus 驱动");
            }

            Log("虚拟 Xbox360 手柄已连接");

            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var label = $"[{i + 1}/{steps.Count}] {step.Type} {step.Description}".Trim();
                Log(label);

                switch (step.Type.ToLowerInvariant())
                {
                    case "wait":
                        await Task.Delay(Math.Clamp(step.Ms ?? 500, 0, 60000), ct);
                        break;

                    case "press":
                        Press(transport, ParseButton(step.Button), Math.Clamp(step.DurationMs ?? 80, 1, 5000));
                        await Task.Delay(Math.Clamp(step.Ms ?? 0, 0, 60000), ct);
                        break;

                    case "wheel":
                        PressWheel(transport, ParseButton(step.Modifier), ParseDirection(step.Direction),
                            Math.Clamp(step.HoldMs ?? 180, 1, 5000));
                        await Task.Delay(Math.Clamp(step.Ms ?? 0, 0, 60000), ct);
                        break;

                    case "stick":
                        for (var n = 0; n < Math.Max(1, step.Repeat ?? 1); n++)
                        {
                            PressStick(transport, step.Stick ?? "Left", step.X ?? 0, step.Y ?? 0,
                                Math.Clamp(step.DurationMs ?? 300, 1, 10000));
                            await Task.Delay(120, ct);
                        }

                        await Task.Delay(Math.Clamp(step.Ms ?? 0, 0, 60000), ct);
                        break;

                    case "openmap":
                        await OpenMapAsync(transport, step, Log, ct);
                        break;

                    case "zoommax":
                        await ZoomMaxAsync(transport, step, Log, ct);
                        break;

                    case "ocrassert":
                        await OcrAssertAsync(step, Log);
                        break;

                    case "screenshot":
                        SaveScreenshot(caseDir, step.Screenshot ?? $"step_{i + 1:00}");
                        break;

                    default:
                        throw new InvalidOperationException($"未知步骤类型: {step.Type}");
                }

                if (!string.IsNullOrWhiteSpace(step.Screenshot) && step.Type.ToLowerInvariant() != "screenshot")
                {
                    SaveScreenshot(caseDir, step.Screenshot);
                }
            }

            Log("测试完成");
        }, ct);
    }

    private static async Task OpenMapAsync(Xbox360Transport transport, GamepadTestStep step, Action<string> log, CancellationToken ct)
    {
        var retries = Math.Max(1, step.Retries ?? 3);
        for (var attempt = 1; attempt <= retries; attempt++)
        {
            PressWheel(transport, ParseButton(step.Modifier), ParseDirection(step.Direction), Math.Clamp(step.HoldMs ?? 350, 1, 5000));
            await Task.Delay(Math.Clamp(step.Ms ?? 1500, 0, 60000), ct);
            var hit = IsBigMapUi(log);
            log(hit ? $"地图已打开（第 {attempt} 次尝试）" : $"第 {attempt} 次打开地图未检测到地图界面，重试…");
            if (hit)
            {
                return;
            }
        }

        throw new InvalidOperationException("打开地图失败：连续多次未识别到大地图界面");
    }

    private static async Task ZoomMaxAsync(Xbox360Transport transport, GamepadTestStep step, Action<string> log, CancellationToken ct)
    {
        var attempts = Math.Max(1, step.Attempts ?? 6);
        var target = step.TargetScale ?? 0.97;
        for (var i = 0; i < attempts; i++)
        {
            Press(transport, ParseButton(step.Button), Math.Clamp(step.DurationMs ?? 600, 1, 5000));
            await Task.Delay(Math.Clamp(step.Ms ?? 400, 0, 60000), ct);
            var scale = TryGetBigMapScale();
            log(scale is null ? $"第 {i + 1} 次放大（缩放级别不可用）" : $"第 {i + 1} 次放大，缩放级别={scale:F3}");
            if (scale >= target)
            {
                return;
            }
        }
    }

    private static async Task OcrAssertAsync(GamepadTestStep step, Action<string> log)
    {
        var text = TryOcrScreen();
        log($"OCR：{Shorten(text)}");
        if (string.IsNullOrWhiteSpace(step.Text))
        {
            return;
        }

        if (!text.Contains(step.Text))
        {
            throw new InvalidOperationException($"OCR 未在画面中确认到「{step.Text}」");
        }

        await Task.CompletedTask;
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

    private static void PressStick(Xbox360Transport transport, string stick, double x, double y, int durationMs)
    {
        var (xAxis, yAxis) = string.Equals(stick, "Right", StringComparison.OrdinalIgnoreCase)
            ? (GamepadAxis.RightThumbX, GamepadAxis.RightThumbY)
            : (GamepadAxis.LeftThumbX, GamepadAxis.LeftThumbY);

        transport.SetAxis(xAxis, (short)Math.Clamp(x * short.MaxValue, short.MinValue, short.MaxValue));
        transport.SetAxis(yAxis, (short)Math.Clamp(y * short.MaxValue, short.MinValue, short.MaxValue));
        Thread.Sleep(durationMs);
        transport.SetAxis(xAxis, 0);
        transport.SetAxis(yAxis, 0);
    }

    private static GamepadButton ParseButton(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            !Enum.TryParse<GamepadButton>(name, true, out var button) ||
            button == GamepadButton.None)
        {
            throw new InvalidOperationException($"无效按键: {name}");
        }

        return button;
    }

    private static GamepadStickDirection ParseDirection(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            !Enum.TryParse<GamepadStickDirection>(name, true, out var direction) ||
            direction == GamepadStickDirection.None)
        {
            throw new InvalidOperationException($"无效方向: {name}");
        }

        return direction;
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

    private static void SaveScreenshot(string? caseDir, string name)
    {
        try
        {
            var dir = string.IsNullOrWhiteSpace(caseDir)
                ? Global.Absolute("log/gamepad-integration-test")
                : Path.Combine(caseDir, "screenshots");
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

public sealed class GamepadTestCase
{
    public string Name { get; set; } = "gamepad-test";

    public List<GamepadTestStep> Steps { get; set; } = [];
}

public sealed class GamepadTestStep
{
    /// <summary>wait / press / wheel / stick / openMap / zoomMax / ocrAssert / screenshot</summary>
    public string Type { get; set; } = "";

    public string? Description { get; set; }

    public string? Button { get; set; }

    public string? Modifier { get; set; }

    public string? Direction { get; set; }

    public string? Stick { get; set; }

    /// <summary>OCR 断言需要包含的文本</summary>
    public string? Text { get; set; }

    /// <summary>截图名称（相对 screenshots 目录）</summary>
    public string? Screenshot { get; set; }

    public int? Ms { get; set; }

    public int? DurationMs { get; set; }

    public int? HoldMs { get; set; }

    public int? Retries { get; set; }

    public int? Attempts { get; set; }

    public int? Repeat { get; set; }

    public double? X { get; set; }

    public double? Y { get; set; }

    public double? TargetScale { get; set; }
}
