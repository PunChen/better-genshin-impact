using BetterGenshinImpact.Core.Input;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.Core.Recognition;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.Model.Area;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BetterGenshinImpact.GameTask.Common.TaskControl;

namespace BetterGenshinImpact.GameTask.Common.Job;

/// <summary>
/// 派蒙菜单导航（方案 A7/A9/A18）：
/// 打开派蒙菜单（Start）→ OCR 定位目标菜单项 → 左摇杆上下导航锁定 → 确认（B）。
/// 手柄模式下无专用按键的界面打开动作共用此流程。
/// ⚠️ 待实机验证的假设（详见 Docs/design/gamepad-key-mapping.md §7.3）：
/// 1. 打开派蒙菜单后焦点默认在第一项「队伍配置」；
/// 2. 左摇杆单次脉冲（FocusDown/FocusUp）在菜单中移动一项（与 B2 列表滚动同一未验证点）；
/// 3. 「设置」入口有可 OCR 的文字标签（无文字则需图标资源，见 A9 备注）。
/// </summary>
public static class PaimonMenuNavigateJob
{
    /// <summary>
    /// 派蒙菜单项白名单（过滤 OCR 噪音，用于确定可导航项的纵向顺序）
    /// </summary>
    private static readonly string[] MenuItemWhiteList =
    {
        "队伍配置", "冒险之证", "祈愿", "图鉴", "成就", "任务", "好友", "邮件", "尘歌壶", "旅行札记", "设置", "退出游戏",
    };

    /// <summary>
    /// 菜单列表 OCR 区域（1080p 坐标，按 AssetScale 缩放）
    /// </summary>
    private const int MenuListX = 0;
    private const int MenuListY = 80;
    private const int MenuListWidth = 520;
    private const int MenuListHeight = 920;

    /// <summary>
    /// 打开界面（模式感知）：手柄模式且目标界面在派蒙菜单中可达时走菜单导航（A7/A9/A18），否则直按绑定键。
    /// </summary>
    public static async Task OpenScreenAsync(GIActions action, CancellationToken ct = default)
    {
        var targetText = action switch
        {
            GIActions.OpenPartySetupScreen => "队伍配置",
            GIActions.OpenFriendsScreen => "好友",
            GIActions.OpenTheSettingsMenu => "设置",
            _ => null,
        };

        if (GamepadMode.Enabled && targetText != null)
        {
            await OpenMenuItemAsync(targetText, ct);
            return;
        }

        InputHub.Foreground.SimulateAction(action);
    }

    /// <summary>
    /// 通过派蒙菜单打开指定菜单项（如「队伍配置」「好友」「设置」）。
    /// 键鼠模式请勿使用此方法（直接按绑定键即可）。
    /// </summary>
    public static async Task OpenMenuItemAsync(string targetText, CancellationToken ct = default)
    {
        TaskControl.Logger.LogInformation("派蒙菜单导航：打开「{Text}」", targetText);

        // 1. 打开派蒙菜单（Start）
        InputHub.Foreground.SimulateAction(GIActions.OpenPaimonMenu);
        await Delay(800, ct);

        // 2. OCR 定位目标菜单项（重试等待菜单弹出动画）
        var items = new List<string>();
        var targetIndex = -1;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            (items, targetIndex) = FindMenuItems(targetText);
            if (targetIndex >= 0)
            {
                break;
            }

            await Delay(500, ct);
        }

        if (targetIndex < 0)
        {
            // 返回原界面，避免停留在半开的菜单（A = 取消/返回）
            InputHub.Foreground.SimulateAction(GIActions.Cancel);
            throw new InvalidOperationException(
                $"派蒙菜单中未找到菜单项「{targetText}」，OCR 识别结果：{(items.Count > 0 ? string.Join("、", items) : "无")}");
        }

        // 3. 左摇杆导航：假设焦点初始在第一项（见类注释假设 1），逐项下移到目标
        for (var i = 0; i < targetIndex; i++)
        {
            InputHub.Foreground.SimulateAction(GIActions.FocusDown);
            await Delay(300, ct);
        }

        // 4. 确认（B）
        await Delay(200, ct);
        InputHub.Foreground.SimulateAction(GIActions.Confirm);
        await Delay(500, ct);
    }

    /// <summary>
    /// OCR 派蒙菜单列表，返回（按纵向排序的白名单菜单项，目标项索引；未找到目标时索引为 -1）
    /// </summary>
    private static (List<string> Items, int TargetIndex) FindMenuItems(string targetText)
    {
        var scale = TaskContext.Instance().SystemInfo.AssetScale;
        using var region = CaptureToRectArea();
        var results = region.FindMulti(new RecognitionObject
        {
            RecognitionType = RecognitionTypes.Ocr,
            RegionOfInterest = new Rect(
                (int)(MenuListX * scale),
                (int)(MenuListY * scale),
                (int)(MenuListWidth * scale),
                (int)(MenuListHeight * scale)),
        });

        var items = new List<(string Key, int Y)>();
        foreach (var ra in results)
        {
            using (ra)
            {
                var text = ra.Text;
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                foreach (var key in MenuItemWhiteList)
                {
                    if (text.Contains(key))
                    {
                        // 同一菜单项可能命中多条 OCR 结果，只保留一次
                        if (items.All(x => x.Key != key))
                        {
                            items.Add((key, ra.Y));
                        }

                        break;
                    }
                }
            }
        }

        var ordered = items.OrderBy(x => x.Y).Select(x => x.Key).ToList();
        return (ordered, ordered.IndexOf(targetText));
    }
}