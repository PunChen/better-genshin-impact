using BetterGenshinImpact.Core.Input;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.Core.Recognition;
using BetterGenshinImpact.Core.Recognition.OCR;
using System;
using System.Threading;
using System.Threading.Tasks;
using BetterGenshinImpact.GameTask.AutoGeniusInvokation.Exception;
using BetterGenshinImpact.GameTask.AutoWood.Utils;
using BetterGenshinImpact.GameTask.Common.Element.Assets;
using BetterGenshinImpact.GameTask.Model.Area;
using Fischless.WindowsInput.Gamepad;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;
using static BetterGenshinImpact.GameTask.Common.TaskControl;

namespace BetterGenshinImpact.GameTask.Common.Job;

public class ExitAndReloginJob
{
    private readonly Login3rdParty _login3rdParty = new();

    private static RecognitionObject GetAutoWoodRecognitionObject(string objectName)
    {
        return RecognitionAssets.Get("AutoWood", objectName);
    }

    public async Task Start(CancellationToken ct)
    {
        Logger.LogInformation("退出至登录页面");
        SystemControl.FocusWindow(TaskContext.Instance().GameHandle);

        // 等待菜单界面出现
        await NewRetry.WaitForElementAppear(
            GetAutoWoodRecognitionObject("MenuBag"),
            () => InputHub.Foreground.Keyboard.KeyPress(User32.VK.VK_ESCAPE),
            ct,
            10,
            1200
        );

        if (GamepadMode.Enabled)
        {
            // 手柄模式：鼠标点击会被 GamepadChannel 丢弃，走长按 X 的手柄退出流程
            await ExitToLoginByGamepadAsync(ct);
        }
        else
        {
            // 点击退出按钮并等待确认弹窗出现
            await NewRetry.WaitForElementAppear(
                GetAutoWoodRecognitionObject("Confirm"),
                () => GameCaptureRegion.GameRegionClick((size, scale) => (50 * scale, size.Height - 50 * scale)),
                ct,
                5,
                800
            );

            // 点击确认退出并等待确认弹窗消失
            await NewRetry.WaitForElementDisappear(
                GetAutoWoodRecognitionObject("Confirm"),
                screen =>
                {
                    // 接收当前截图作为参数
                    screen.Find(RecognitionAssets.Get("AutoWood", "Confirm", screen), ra =>
                    {
                        ra.Click();
                        ra.Dispose();
                    });
                },
                ct,
                5,
                1000
            );
        }

        await Delay(1000, ct);

        //============== 重新登录流程 ==============
        // 手柄模式：游戏在进入游戏窗口前没有手柄按键适配（见 gamepad-adaptation-plan.md T12），
        // 重登阶段的键鼠点击在手柄模式下会被 GamepadChannel 丢弃，键鼠直通机制落地前此处会失败
        if (GamepadMode.Enabled)
        {
            Logger.LogWarning("手柄模式：登录界面无手柄支持，重登阶段维持键鼠流程（键鼠直通机制未实现，见 adaptation-plan T11/T12）");
        }

        Logger.LogInformation("点击登录");
        _login3rdParty.RefreshAvailabled();
        if (_login3rdParty is { Type: Login3rdParty.The3rdPartyType.Bilibili, IsAvailabled: true })
        {
            // await Delay(1, ct);
            Thread.Sleep(100);
            _login3rdParty.Login(ct);
            Logger.LogInformation("退出重登启用 B 服模式");
        }

        // 等待进入游戏按钮出现并点击
        var enterGameAppear = await NewRetry.WaitForElementAppear(
            GetAutoWoodRecognitionObject("EnterGame"),
            () => { },
            ct,
            120,
            1000
        );
        if (!enterGameAppear)
        {
            throw new RetryException("未检测进入游戏界面");
        }

        // 点击进入游戏按钮直到它消失
        var waitForEnterGameRoDisappear = await NewRetry.WaitForElementDisappear(
            GetAutoWoodRecognitionObject("EnterGame"),
            () => GameCaptureRegion.GameRegion1080PPosClick(955, 666),
            ct,
            120,
            1000
        );
        if (!waitForEnterGameRoDisappear)
        {
            throw new RetryException("未检测到进入游戏按钮消失, 可能未点击成功");
        }


        // 等待主界面出现
        var mainUiFound = await NewRetry.WaitForElementAppear(
            ElementRecognition.Get("PaimonMenu"),
            () => { },
            ct,
            120,
            1000
        );

        if (mainUiFound)
        {
            Logger.LogInformation("退出重新登录结束！");
        }
        else
        {
            Logger.LogWarning("未检测到主界面，登录可能未完成");
        }

        await Delay(500, ct);
    }

    /// <summary>
    /// 手柄模式退出至登录界面：派蒙菜单内长按 X 呼出三选项确认框
    /// （继续游戏 / 返回登录界面 / 退出到桌面），左摇杆下移选中「返回登录界面」后按 B 确认。
    /// 假设（未实测，见 gamepad-adaptation-plan.md T11）：确认框默认聚焦第一项「继续游戏」，
    /// 单次摇杆下移聚焦一项；聚焦态的高亮特征待实测后补充锁定校验。
    /// </summary>
    private async Task ExitToLoginByGamepadAsync(CancellationToken ct)
    {
        var gamepad = InputHub.Backend as GamepadInputBackend
                      ?? throw new InvalidOperationException("手柄后端不可用");

        for (var attempt = 0; attempt < 3; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            Logger.LogInformation("手柄退出：派蒙菜单内长按 X 呼出退出确认框（第 {Attempt} 次）", attempt + 1);
            gamepad.PressButton(GamepadButton.X, 1500);
            await Delay(800, ct);

            // OCR 确认三选项框出现（此处为中文界面文案，非中文语种的匹配待实测后处理）
            var dialogText = TryOcrScreenText();
            Logger.LogInformation("手柄退出：确认框 OCR={Text}", Shorten(dialogText));
            if (dialogText.Contains("返回登录界面"))
            {
                // 假设默认聚焦第一项「继续游戏」，下移一项到「返回登录界面」
                gamepad.SetLeftStick(0, -1, 60);
                await Delay(400, ct);

                var afterNavText = TryOcrScreenText();
                Logger.LogInformation("手柄退出：导航后 OCR={Text}（聚焦高亮特征待实测，当前按次数导航）", Shorten(afterNavText));

                Logger.LogInformation("手柄退出：按 B 确认返回登录界面");
                gamepad.PressButton(GamepadButton.B, 150);
                return;
            }

            Logger.LogWarning("手柄退出：未识别到退出确认框，重试长按 X");
        }

        throw new RetryException("手柄模式未能呼出退出确认框，退出失败");
    }

    private static string TryOcrScreenText()
    {
        try
        {
            using var ra = CaptureToRectArea();
            return OcrFactory.Paddle.Ocr(ra.SrcMat) ?? string.Empty;
        }
        catch (Exception e)
        {
            Logger.LogWarning("手柄退出 OCR 失败：{Message}", e.Message);
            return string.Empty;
        }
    }

    private static string Shorten(string text)
    {
        var oneLine = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
        return oneLine.Length <= 160 ? oneLine : oneLine[..160] + "…";
    }
}
