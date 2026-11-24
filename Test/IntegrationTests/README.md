# 手柄模式集成测试（外置步骤文件）

一键测试也可以通过**启动 BetterGI 的方式**执行外置步骤文件，适合反复调试：
不需要改代码，改 JSON 即可。

## 前置条件

1. 游戏已启动，并在游戏内切到**手柄控制**；
2. BGI 中已启用手柄模式（`GamepadConfig.Enabled = true`）；
3. **没有其他 BGI 实例/任务占用游戏手柄后端**（否则会提示停止任务）；
4. `ViGEmBus` 驱动已安装。

## 运行

```powershell
# 关闭已运行的 BGI 后，在仓库根目录执行：
.\Test\IntegrationTests\run-gamepad-test.ps1
```

或指定用例/可执行文件：

```powershell
.\Test\IntegrationTests\run-gamepad-test.ps1 `
  -Case .\Test\IntegrationTests\Cases\teleport_mondstadt.json `
  -Exe .\BetterGenshinImpact\bin\x64\Debug\net8.0-windows10.0.22621.0\BetterGI.exe
```

脚本会启动 `BetterGI.exe --gamepad-test <json>`，BGI 启动截图器后依次执行步骤，
完成后自动退出；退出码 `0` 表示全部步骤通过，`1` 表示失败。

## 输出

- 日志：`log/gamepad-integration-test-yyyyMMdd.log`
- 截图：`Test/IntegrationTests/Cases/screenshots/`
- 失败时也会保存当前画面，便于定位卡在哪一步。

## 步骤类型

| 类型 | 字段 | 说明 |
|---|---|---|
| `wait` | `ms` | 等待 |
| `press` | `button`、`durationMs`、`ms` | 按一次手柄键 |
| `wheel` | `modifier`、`direction`、`holdMs`、`ms` | 按住修饰键 + 右摇杆方向 |
| `stick` | `stick`(Left/Right)、`x`、`y`、`durationMs`、`repeat`、`ms` | 摇杆偏移 |
| `openMap` | `modifier`、`direction`、`holdMs`、`ms`、`retries` | 打开地图并识别校验，失败重试 |
| `zoomMax` | `button`、`durationMs`、`ms`、`attempts`、`targetScale` | 循环放大直到缩放级别达标 |
| `ocrAssert` | `text` | OCR 必须包含指定文本，否则失败 |
| `screenshot` | `screenshot` | 保存截图（相对 `screenshots/`） |

`teleport_mondstadt.json` 的流程：

1. `LB + 右摇杆↓` 打开地图，`Bv.IsInBigMapUi` 校验，失败重试 3 次；
2. `LT` 放大，读 `Bv.GetBigMapScale`，达到 0.97 或 6 次后停止；
3. `Y` 打开区域列表；
4. 左摇杆↑ × 5 滚到列表顶部（蒙德）；
5. `B` 选中区域；
6. OCR 断言画面出现「蒙德」；
7. `B` 确认传送，等待 3 秒。

## 注意

- 区域列表顺序/默认焦点可能随账号不同；若未选中蒙德，可调整 `stick.repeat` 或改为「向下」。
- 地图缩放方向：`LT` 放大、`RT` 缩小（2026-11-25 实机截图确认）。
- 测试期间 BGI 主窗口会失焦到游戏，属正常现象。
