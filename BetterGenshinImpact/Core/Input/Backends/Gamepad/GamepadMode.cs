using BetterGenshinImpact.Service;

namespace BetterGenshinImpact.Core.Input.Backends.Gamepad;

/// <summary>
/// 手柄模式开关的只读入口。
/// 手柄模式需要游戏内也切换到手柄控制，并在本软件中同时开启才会生效。
/// </summary>
public static class GamepadMode
{
    /// <summary>
    /// 是否启用了手柄模式（仅表示配置开关，不代表虚拟手柄已连接）
    /// </summary>
    public static bool Enabled => ConfigService.Config?.GamepadConfig.Enabled == true;
}
