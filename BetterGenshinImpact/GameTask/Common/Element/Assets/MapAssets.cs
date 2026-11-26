using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.GameTask.Model.Assets;
using BetterGenshinImpact.GameTask.Model.Area;
using OpenCvSharp;
using System;

namespace BetterGenshinImpact.GameTask.Common.Element.Assets;

public sealed class MapAssets
{
    /// <summary>
    /// 下游小地图预处理期望的参考尺寸（MiniMapPreprocessor 基于该尺寸设计）
    /// </summary>
    public const int MiniMapReferenceSize = 212;

    private static readonly CaptureAssetsCache<MapAssets> Cache = new(static size => new MapAssets(size));

    /// <summary>
    /// 原始捕获图上的小地图裁剪区域（已按捕获分辨率缩放）
    /// </summary>
    public Rect MimiMapRect { get; }

    /// <summary>
    /// 小地图裁剪区域缩放到参考尺寸的倍率（1 表示无需缩放）
    /// </summary>
    public double MiniMapNormalizeScale => (double)MiniMapReferenceSize / MimiMapRect.Width;

    /// <summary>
    /// 键鼠 HUD 小地图区域
    /// </summary>
    public static Rect MimiMapRect1080P = new(62, 19, 212, 212);

    /// <summary>
    /// 手柄 HUD 小地图区域（比键鼠更靠右下、尺寸更小）
    /// </summary>
    public static Rect MimiMapRectGamepad1080P = new(126, 48, 176, 176);

    private MapAssets(CaptureSize captureSize)
    {
        var baseRect = GamepadMode.Enabled ? MimiMapRectGamepad1080P : MimiMapRect1080P;
        // 允许大于 1，保证 4K/2K 捕获时小地图区域同步放大
        var scale = captureSize.Width / 1920.0;
        MimiMapRect = new Rect(
            (int)Math.Round(baseRect.X * scale),
            (int)Math.Round(baseRect.Y * scale),
            (int)Math.Round(baseRect.Width * scale),
            (int)Math.Round(baseRect.Height * scale));
    }

    /// <summary>
    /// 裁剪小地图并归一化到 <see cref="MiniMapReferenceSize"/>，供小地图预处理/模板匹配使用。
    /// 手柄模式的小地图区域比参考尺寸小，必须缩放，否则中心区域和特征尺度都会对不上。
    /// </summary>
    public Mat GetMiniMapMat(Mat source)
    {
        using var crop = new Mat(source, MimiMapRect);
        if (crop.Width == MiniMapReferenceSize && crop.Height == MiniMapReferenceSize)
        {
            return crop.Clone();
        }

        var resized = new Mat();
        Cv2.Resize(crop, resized, new Size(MiniMapReferenceSize, MiniMapReferenceSize));
        return resized;
    }

    public static MapAssets Get(Region region)
    {
        return Cache.Get(region);
    }

    public static MapAssets Get(int captureWidth, int captureHeight)
    {
        return Cache.Get(captureWidth, captureHeight);
    }
}
