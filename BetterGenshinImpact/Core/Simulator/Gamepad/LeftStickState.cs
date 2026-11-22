using System.Collections.Generic;
using BetterGenshinImpact.Core.Simulator.Extensions;

namespace BetterGenshinImpact.Core.Simulator.Gamepad;

/// <summary>
/// 左摇杆状态：把“按住/松开”的四个移动方向合成为摇杆坐标。
/// 抽出独立类型是为了在不依赖虚拟手柄驱动的情况下做单元测试。
/// </summary>
public sealed class LeftStickState
{
    private readonly HashSet<GIActions> _held = [];

    /// <summary>
    /// 记录一个移动方向的按下/松开
    /// </summary>
    public void Set(GIActions direction, bool pressed)
    {
        if (pressed)
        {
            _held.Add(direction);
        }
        else
        {
            _held.Remove(direction);
        }
    }

    /// <summary>
    /// 清空所有方向
    /// </summary>
    public void Reset()
    {
        _held.Clear();
    }

    /// <summary>
    /// 取得合成后的摇杆坐标（-32767 ~ 32767），X 向右为正，Y 向上为正
    /// </summary>
    public (short X, short Y) GetAxisValues()
    {
        var x = 0;
        var y = 0;
        if (_held.Contains(GIActions.MoveLeft))
        {
            x -= 1;
        }

        if (_held.Contains(GIActions.MoveRight))
        {
            x += 1;
        }

        if (_held.Contains(GIActions.MoveForward))
        {
            y += 1;
        }

        if (_held.Contains(GIActions.MoveBackward))
        {
            y -= 1;
        }

        return ((short)(x * short.MaxValue), (short)(y * short.MaxValue));
    }
}
