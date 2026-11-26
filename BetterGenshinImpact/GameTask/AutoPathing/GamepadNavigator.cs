using System;
using System.Threading;
using System.Threading.Tasks;
using BetterGenshinImpact.Core.Input;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.GameTask.AutoPathing.Model;
using BetterGenshinImpact.GameTask.Common;
using BetterGenshinImpact.GameTask.Common.Map;
using BetterGenshinImpact.GameTask.Common.Map.Maps;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using static BetterGenshinImpact.GameTask.Common.TaskControl;

namespace BetterGenshinImpact.GameTask.AutoPathing;

/// <summary>
/// 手柄模式移动到指定游戏坐标的标准流程：
/// 小地图定位 → 计算目标角度 → 右摇杆转到目标角度 → 左摇杆持续前推走过去。
/// </summary>
public static class GamepadNavigator
{
    /// <summary>
    /// 手柄模式移动到指定游戏坐标。
    /// </summary>
    /// <param name="gameX">目标游戏坐标 X</param>
    /// <param name="gameY">目标游戏坐标 Y</param>
    /// <param name="mapName">地图名称</param>
    /// <param name="mapMatchMethod">地图匹配方式</param>
    /// <param name="ct">取消令牌</param>
    /// <param name="arriveDistance">到达判定距离</param>
    /// <param name="heavyPushDistance">超过该距离使用重推（跑），否则轻推（走）</param>
    /// <param name="timeoutSeconds">超时时间</param>
    /// <returns>是否到达</returns>
    public static async Task<bool> MoveToAsync(
        double gameX,
        double gameY,
        string mapName,
        string mapMatchMethod,
        CancellationToken ct,
        double arriveDistance = 3,
        double heavyPushDistance = 10,
        int timeoutSeconds = 120)
    {
        var map = MapManager.GetMap(mapName, mapMatchMethod);
        var imagePoint = map.ConvertGenshinMapCoordinatesToImageCoordinates(
            new Point2f((float)gameX, (float)gameY));
        var imageWaypoint = new Waypoint { X = imagePoint.X, Y = imagePoint.Y };

        var gamepad = InputHub.Backend as GamepadInputBackend;
        var rotateTask = new CameraRotateTask(ct);
        var startTime = DateTime.UtcNow;
        var leftStickHeld = false;

        try
        {
            for (var i = 0; i < 600 && !ct.IsCancellationRequested; i++)
            {
                using var ra = TaskControl.CaptureToRectArea();
                var position = Navigation.GetPositionStable(ra, mapName, mapMatchMethod);
                if (position == default)
                {
                    await TaskControl.Delay(100, ct);
                    continue;
                }

                var distance = Navigation.GetDistance(imageWaypoint, position);
                if (i % 10 == 0)
                {
                    Logger.LogInformation("手柄移动：位置({X:F1},{Y:F1})，目标({TX:F1},{TY:F1})，距离 {Distance:F1}",
                        position.X, position.Y, imageWaypoint.X, imageWaypoint.Y, distance);
                }

                if (distance < arriveDistance)
                {
                    Logger.LogInformation("手柄移动：已到达目标附近，距离 {Distance:F1}", distance);
                    return true;
                }

                if ((DateTime.UtcNow - startTime).TotalSeconds > timeoutSeconds)
                {
                    throw new Exception($"手柄模式：移动到目标超时（距离 {distance:F1}）");
                }

                // 1. 计算正确角度：目标朝向 - 当前相机朝向
                var targetOrientation = Navigation.GetTargetOrientation(imageWaypoint, position);
                var cameraOrientation = CameraOrientation.Compute(ra.SrcMat);
                var relativeAngle = NormalizeAngle(targetOrientation - cameraOrientation);

                // 2. 角度偏差较大时，先用右摇杆转到目标角度（转向前先松开左摇杆）
                if (Math.Abs(relativeAngle) > 8)
                {
                    if (leftStickHeld && gamepad is not null)
                    {
                        gamepad.ReleaseLeftStick();
                        leftStickHeld = false;
                    }

                    await rotateTask.WaitUntilRotatedTo(targetOrientation, 5, 10);
                }

                // 3. 角度对齐后左摇杆持续前推，一路走过去
                var magnitude = distance > heavyPushDistance ? 1.0 : 0.45;
                if (gamepad is not null)
                {
                    gamepad.HoldLeftStick(0, magnitude);
                    leftStickHeld = true;
                }
                else
                {
                    InputHub.Foreground.SimulateAction(GIActions.MoveForward, KeyType.KeyDown);
                }

                await TaskControl.Delay(100, ct);
            }
        }
        finally
        {
            if (gamepad is not null)
            {
                gamepad.ReleaseLeftStick();
            }
            else
            {
                InputHub.Foreground.SimulateAction(GIActions.MoveForward, KeyType.KeyUp);
            }
        }

        return false;
    }

    private static double NormalizeAngle(double angle)
    {
        angle %= 360;
        if (angle > 180)
        {
            angle -= 360;
        }
        else if (angle < -180)
        {
            angle += 360;
        }

        return angle;
    }
}
