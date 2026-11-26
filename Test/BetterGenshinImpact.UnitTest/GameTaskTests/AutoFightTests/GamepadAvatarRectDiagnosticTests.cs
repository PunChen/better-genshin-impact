using BetterGenshinImpact.Core.Recognition.ONNX;
using BetterGenshinImpact.GameTask.AutoFight.Assets;
using BetterGenshinImpact.GameTask.Model.Area;
using Compunet.YoloSharp;
using Compunet.YoloSharp.Data;
using OpenCvSharp;
using Vanara.PInvoke;
using Xunit;
using Xunit.Abstractions;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoFightTests;

/// <summary>
/// 手柄 HUD 队伍头像范围回归测试。
/// 使用生产代码 <see cref="AutoFightAssets.BuildGamepadAvatarSideRects"/> 生成槽位，
/// 在 1920/2559/2730 三种捕获宽度下验证 4 个角色都能被 YOLO 正确分类。
/// 截图放在仓库 .tmp 目录，缺失时跳过。
/// </summary>
public class GamepadAvatarRectDiagnosticTests(ITestOutputHelper output)
{
    private static readonly string TempDir =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\.tmp"));

    private static BgiYoloPredictor? predictor;

    private static BgiYoloPredictor Predictor => LazyInitializer.EnsureInitialized(ref predictor,
        () => new BgiOnnxSessionFactory(new FakeLogger<BgiOnnxFactory>(), ResolvedInferenceRuntime.Cpu())
            .CreateYoloPredictor(BgiOnnxModel.BgiAvatarSide));

    private static readonly string[] ExpectedNames = ["Marionette", "Columbina", "Clorinde", "Ganyu"];

    [Theory]
    [InlineData("game_correct_1920.png", 1920)]
    [InlineData("game_correct.jpg", 2730)]
    [InlineData("game_resized_physical.png", 2559)]
    public void GamepadRects_ShouldClassifyAtAnyResolution(string fileName, int expectedWidth)
    {
        var path = Path.Combine(TempDir, fileName);
        if (!File.Exists(path))
        {
            output.WriteLine($"缺少截图：{path}，跳过");
            return;
        }

        using var mat = new Mat(path);
        Assert.Equal(expectedWidth, mat.Width);
        var systemInfo = new FakeSystemInfo(new RECT(0, 0, mat.Width, mat.Height), 1);
        var gameCaptureRegion = systemInfo.DesktopRectArea.Derive(mat, 0, 0);
        var scale = gameCaptureRegion.Width / 1920.0;
        output.WriteLine($"截图：{fileName} {gameCaptureRegion.Width}x{gameCaptureRegion.Height}，缩放 {scale:F4}");

        var rects = AutoFightAssets.BuildGamepadAvatarSideRects(gameCaptureRegion.Width);
        Assert.Equal(4, rects.Count);

        for (var i = 0; i < rects.Count; i++)
        {
            var rect = rects[i];
            using var crop = gameCaptureRegion.DeriveCrop(rect);
            var top = Predictor.Predictor.Classify(crop.CacheImage).GetTopClass();
            output.WriteLine($"  row{i + 1} rect=({rect.X},{rect.Y},{rect.Width}) -> {top.Name.Name} {top.Confidence:F3}");

            Assert.True(top.Confidence > 0.9f, $"row{i + 1} 置信度过低：{top.Name.Name} {top.Confidence:F3}");
            Assert.Equal(ExpectedNames[i], top.Name.Name, ignoreCase: true);
        }
    }
}
