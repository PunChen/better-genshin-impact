using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BetterGenshinImpact.Core.Recognition;
using BetterGenshinImpact.Core.Recognition.OCR;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.GameTask.AutoPick;
using BetterGenshinImpact.GameTask.AutoPick.Assets;
using BetterGenshinImpact.GameTask.Model.Area;
using OpenCvSharp;

namespace BetterGenshinImpact.GameTask.Common.BgiVision;

/// <summary>
/// 一些基础图像识别操作
/// </summary>
public static partial class Bv
{
    public static string FindFKeyText(ImageRegion region)
    {
        // 手柄模式下拾取提示为 X 图标（与键鼠 F 图标不同），图标较小，
        // 按图标中心扩展出与键鼠模式等价的几何，使后续文字偏移计算保持一致（与 AutoPickTrigger 同一处理方式）
        var gamepadMode = GamepadMode.Enabled;
        using var foundRectArea = region.Find(gamepadMode
            ? RecognitionAssets.Get("AutoPick", "ControllerPick", region)
            : AutoPickAssets.Get(region, TaskContext.Instance().Config.AutoPickConfig.PickKey).PickRo);
        if (foundRectArea.IsEmpty())
        {
            return string.Empty;
        }

        var scale = TaskContext.Instance().SystemInfo.AssetScale;
        int textX, textY, textHeight;
        if (gamepadMode)
        {
            textX = (int)(foundRectArea.X + foundRectArea.Width / 2d - 20 * scale);
            textY = (int)(foundRectArea.Y + foundRectArea.Height / 2d - 20 * scale);
            textHeight = (int)(40 * scale);
        }
        else
        {
            textX = foundRectArea.X;
            textY = foundRectArea.Y;
            textHeight = foundRectArea.Height;
        }

        var textRect = new Rect(textX + (int)(115 * scale), textY,
            (int)((400 - 115) * scale), textHeight);
        if (textRect.X + textRect.Width > region.SrcMat.Width
            || textRect.Y + textRect.Height > region.SrcMat.Height)
        {
            Debug.WriteLine("AutoPickTrigger: 文字区域 out of range");
            return string.Empty;
        }

        using var textMat = new Mat(region.SrcMat, textRect);
        var boundingRect = AutoPickTrigger.GetWhiteTextBoundingRect(textMat);
        // 如果找到有效区域
        if (boundingRect.Width > 5 && boundingRect.Height > 5)
        {
            // 截取只包含文字的区域
            using var textOnlyMat = new Mat(textMat, new Rect(0, 0,
                boundingRect.Right + 3 < textMat.Width ? boundingRect.Right + 3 : textMat.Width, textMat.Height));
            return OcrFactory.Paddle.OcrWithoutDetector(textOnlyMat);
        }
        else
        {
            return OcrFactory.Paddle.Ocr(textMat);
        }
    }
}
