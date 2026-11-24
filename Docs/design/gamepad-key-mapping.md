# 手柄按键映射明细（feat/xbox-game-input-main）

> 依据游戏内「设置 → 按键 → 手柄」截图与逐项确认整理（2026-11）。
> 对应代码：`GamepadConfig`、`GamepadActionMap`、`GamepadInputBackend`、`GamepadUiNavigator`。
> 总体方案见 [gamepad-adaptation-plan.md](./gamepad-adaptation-plan.md)。

> **标记体系（2026-11-24 起）**：⬜ 未确认 → 🔵 已确认待实现 → 🟡 已实现待验证 → 🟢 已验证正确；🚫 无需映射（终态）。
> 「已确认」= 游戏内手柄按键/方案经实测确认；「已验证」= BGI 手柄输出经实机验证与游戏响应一致（当前已有 33 项映射经实机测试标 🟢，其余待回归）。

## 1. 游戏内基准布局（截图确认）

### 1.1 基础战斗界面

| 游戏动作 | 手柄 | BGI 配置字段 | 默认值 |
|---|---|---|---|
| 普通攻击 | A | `NormalAttack` | `A` |
| 元素战技 | RT | `ElementalSkill` | `RightTrigger` |
| 元素爆发 | Y | `ElementalBurst` | `Y` |
| 冲刺 | RB | `Sprint` | `RightShoulder` |
| 跳跃 / 特定模式向上 | B | `Jump` | `B` |
| 拾取 / 交互 | X | `PickUpOrInteract` | `X` |
| 切换瞄准模式 | LT | `SwitchAimingMode` | `LeftTrigger` |
| 呼出快捷轮盘 | LB | `ShortcutWheel` | `LeftShoulder`（按住呼出，方向选择见 §2.2） |
| 打开派蒙界面 | ☰ Menu | `OpenPaimonMenu` | `Start` |
| 好友聊天 | ⧉ View | `OpenChatScreen` | `Back` |
| 确认 / 取消 | B / A | `Confirm` / `Cancel` | `B` / `A` |
| 移动角色 | 左摇杆 | `GIActions.Move*` | 左摇杆（内置默认，开启手柄模式即生效，非独立可绑定按键） |
| 转动视角 | 右摇杆 | `MoveMouseBy` | 右摇杆（内置默认，开启手柄模式即生效，非独立可绑定按键） |
| 镜头拉近 | LB + LT | `CameraZoomIn` | LB + LT（镜头距离调节，手柄专属功能） |
| 镜头拉远 | LB + RT | `CameraZoomOut` | LB + RT（同） |

### 1.2 第三人称射击（独立布局）

| 游戏动作 | 手柄 |
|---|---|
| 开火 | RT |
| 枪械技能 | Y |
| 装填弹药 | B |
| 冲刺 / 滑铲 | RB |
| 跳跃 / 翻越掩体 | A |
| 拾取 / 交互 | X |
| 切换瞄准模式 | LT |
| 切换枪械方案 / 榴晶 | 十字键 |

> 2026-11-25 实机：本表除「镜头拉近/拉远」外均已通过单键测试（🟢）；移动/视角为内置摇杆行为。

> 该布局与基础战斗不同，AutoFight 枪械模式后续需要独立 profile。

## 2. 已确认并落地的映射（🟡 已实现待验证）

### 2.1 切人与道具（组合键）

| 功能 | 手柄 | 配置字段 | 状态 |
|---|---|---|---|
| 切换小队角色 1 | 十字键↑ | `SwitchMember1=Up` | 🟢 已验证正确 |
| 切换小队角色 2 | 十字键→ | `SwitchMember2=Right` | 🟢 已验证正确 |
| 切换小队角色 3 | 十字键← | `SwitchMember3=Left` | 🟢 已验证正确 |
| 切换小队角色 4 | 十字键↓ | `SwitchMember4=Down` | 🟢 已验证正确 |
| 切换小队角色 5 | **LB + Y** | `SwitchMember5=Y`、`SwitchMember5Modifier=LeftShoulder` | 🟢 已验证正确 |
| 快捷使用小道具 | **LB + B** | `QuickUseGadget=B`、`QuickUseGadgetModifier=LeftShoulder` | 🟢 已验证正确 |
| 任务追踪 | **LS（左摇杆按下）** | `QuestNavigation=LeftThumb` | 🟡 已实现待验证 |
| 元素视野 | **LB + 十字键左** | `ElementalSight=Left`、`ElementalSightModifier=LeftShoulder` | 🟡 已实现待验证 |
| 视角回正（鼠标中键） | RS（右摇杆按下；PS 手柄称 R3） | `CameraRecenter=RightThumb` | 🟡 已实现待验证 |

> 双键组合在设置页以单行显示：修饰键 + 修饰键按压方式（短/长） + 主键 + 主键按压方式（短/长）。
> 短按使用 PressDurationMs（默认 60ms，测试至少 120ms），长按使用 LongPressDurationMs（默认 800ms）。
> 修饰键长按时先保持长按时间再按主键；镜头拉近/拉远默认两个键都是长按。

组合键执行顺序：按住修饰键 → 按目标键 → 保持 `PressDurationMs` → 释放目标键 → 释放修饰键。
呼出快捷轮盘（A19）需要长按，使用 `ShortcutWheelPressDurationMs=800`；其他组合键仍用 `PressDurationMs`。

### 2.2 快捷轮盘（LB + 右摇杆方向）— 🟢 已验证正确

| 功能 | 手柄 | 配置字段 | 状态 |
|---|---|---|---|
| 打开地图 | LB + 右摇杆↓ | `OpenMapWheelDirection=Down` | 🟢 已验证正确 |
| 打开背包 | LB + 右摇杆→ | `OpenInventoryWheelDirection=Right` | 🟢 已验证正确 |
| 打开任务菜单 | LB + 右摇杆↙ | `OpenQuestMenuWheelDirection=DownLeft` | 🟢 已验证正确 |
| 打开角色界面 | LB + 右摇杆↘ | `OpenCharacterScreenWheelDirection=DownRight` | 🟢 已验证正确 |
| 打开纪行 | LB + 右摇杆↖ | `OpenBattlePassScreenWheelDirection=UpLeft` | 🟢 已验证正确 |
| 打开活动面板 | LB + 右摇杆← | `OpenTheEventsMenuWheelDirection=Left` | 🟢 已验证正确 |
| 打开冒险之证 | LB + 右摇杆↗ | `OpenAdventurerHandbookWheelDirection=UpRight` | 🟢 已验证正确 |
| 打开祈愿界面 | LB + 右摇杆↑ | `OpenWishScreenWheelDirection=Up` | 🟢 已验证正确 |

修饰键均为 `LeftShoulder`；执行流程：按住 LB 打开轮盘 → 拨右摇杆 → 保持 `WheelSelectionHoldMs`（默认 300ms）→ 回中 → 松开 LB。

### 2.3 菜单 / 地图 UI 原语

| 功能 | 手柄 | 代码 |
|---|---|---|
| 焦点上/下/左/右 | 左摇杆 | `GIActions.FocusUp/Down/Left/Right` |
| 列表滚动（上/下） | 左摇杆上下 | `GIActions.ScrollUp/Down` |
| 多选项选择 | 左摇杆上下 + 确认 | `FocusUp/Down` + `Confirm` |
| 地图光标移动 / 拖动地图 | 左摇杆 | `GamepadUiNavigator.DragMap()` |
| 地图放大 | LT | `MapZoomIn=LeftTrigger` |
| 地图缩小 | RT | `MapZoomOut=RightTrigger` |
| 确认 / 返回 | B / A | `Confirm` / `Cancel` |

焦点与滚动的摇杆保持时间由 `UiNavigationPressMs`（默认 120ms）控制。

## 3. 对外 API

```csharp
GamepadUiNavigator.Focus(GamepadStickDirection.Down);
GamepadUiNavigator.Scroll(-3);        // 正数向上，负数向下
GamepadUiNavigator.ZoomIn();          // LT
GamepadUiNavigator.ZoomOut();         // RT
GamepadUiNavigator.CameraZoomIn();    // LB + LT（镜头拉近）
GamepadUiNavigator.CameraZoomOut();   // LB + RT（镜头拉远）
GamepadUiNavigator.DragMap(1, 0, 300);
```

底层还提供：

```csharp
((GamepadInputBackend)InputHub.Backend).SetLeftStick(x, y, durationMs);
((GamepadInputBackend)InputHub.Backend).SetRightStick(x, y, durationMs);
((GamepadInputBackend)InputHub.Backend).TurnByAngle(degrees);
```

## 4. 已定方案与待确认项

### 4.1 已确认方案（按实施状态标记）

| 编号 | 项目 | 方案 | 状态 |
|---|---|---|---|
| A7 | 打开队伍配置 | 菜单 + 文字识别：打开派蒙菜单（Start）→ 文字识别定位「队伍配置」→ 左摇杆上下左右导航锁定该项 → 确认。已落地：`PaimonMenuNavigateJob.OpenScreenAsync`（8 个 `OpenPartySetupScreen` 调用点已接入，待验证假设见 §7.3） | 🟡 已实现待验证 |
| A9 | 打开设置菜单（F6） | 菜单 + 导航：打开派蒙菜单（Start）→ 左摇杆上下左右导航锁定设置入口 → 确认。⚠️ 备注：① 唯一活跃调用点 `EnterAndExitWonderlandJob`（千星奇域入口）的 F6 按游戏内语义为「打开玩法系统界面（尘歌壶猫尾酒馆等特定场景）」，与派蒙菜单「设置」是否同一界面**待实机确认，该调用点未接线**；② 已实现「设置」文字定位导航路径（`PaimonMenuNavigateJob.OpenScreenAsync`），若设置入口旁有文字标签则无需图标资源，待实机确认（原齿轮图标资源方案作废/备用） | 🔵 已确认待实现（F6 语义待确认） |
| A12 | 切换走/跑 | 采纳建议：不做映射，轻推左摇杆即走。`SwitchToWalkOrRun` 保持 `None` 即为最终状态，无需改动 | 🟡 已实现待验证（需实机确认轻推左摇杆为走而非跑） |
| A13 | 落下（滑翔/攀爬中） | 实测确认为 **A**（原建议 B 作废）：滑翔状态按 A 触发下落攻击；攀爬状态按 A 取消攀爬。已落地：`Drop` 默认 `A`（与 NormalAttack/Cancel 同键，由游戏按上下文区分语义） | 🟡 已实现待验证 |
| A14 | 特定玩法交互（T） | 实测确认为 **LB + X**：`InteractionInSomeMode=X`、`InteractionInSomeModeModifier=LeftShoulder`（X 与 PickUpOrInteract 同键，加 LB 修饰区分）。已落地 | 🟡 已实现待验证 |
| A15 | 中断挑战（P） | 实测确认为 **LS + RS**（两摇杆同时按下）。已落地：`AbandonChallenge=LeftThumb`、`AbandonChallengeModifier=RightThumb`（先按住 RS 再按 LS，避免单独按下 LS 误触发任务追踪） | 🟡 已实现待验证 |
| A16 | 打开多人游戏界面（F2） | 实测确认为 **LB + 十字键→**。已落地：`OpenCoOpScreen=Right`、`OpenCoOpScreenModifier=LeftShoulder` | 🟡 已实现待验证 |
| A17 | 查看教程详情（G） | 实测确认为 **LB + 十字键↑**。已落地：`CheckTutorialDetails=Up`、`CheckTutorialDetailsModifier=LeftShoulder` | 🟡 已实现待验证 |
| A18 | 打开好友界面（O） | 使用**派蒙菜单 + 文字识别「好友」+ 左摇杆上下左右导航锁定**（与 A7 同模式，无专用手柄按键）。已落地：`PaimonMenuNavigateJob.OpenScreenAsync` 支持 `OpenFriendsScreen`（当前无活跃调用方，待任务接入；待验证假设见 §7.3） | 🟡 已实现待验证 |
| A19 | 呼出快捷轮盘（Tab） | 实测确认为 **LB**（按住呼出，拨右摇杆选方向，与 §2.2 轮盘机制一致）。已落地：`ShortcutWheel=LeftShoulder`，键盘 Tab 反查直通 LB | 🟡 已实现待验证 |
| A20 | 打开祈愿界面（F3） | 实测更正（原误记 LB+十字键↑）：**LB + 右摇杆↑**，轮盘第 8 向（原 7 向中 ↑ 唯一未占用，现 8 向全满）。已落地：`OpenWishScreenWheelModifier=LeftShoulder`、`OpenWishScreenWheelDirection=Up`（走 `TryGetWheel` 轮盘机制），直连键字段 `OpenWishScreen` 保留默认 `None`；键盘 F3 反查直通轮盘 | 🟡 已实现待验证 |

### 4.2 ⬜ 未确认项

| 编号 | 项目 | 当前建议 | 状态 |
|---|---|---|---|
| B2 | 列表滚动 | 左摇杆上下 | 🟡 已实现待验证（`ScrollUp/Down` → 左摇杆，待实机确认） |
| B3 | 标签页切换 | LB / RB | ⬜ 未确认 |
| B6 | 运动状态判断（滑翔/攀爬）手柄适配 | 已用 2026-11-25 手柄实机截图制作 `ControllerJumpButton`（下落图标）/`ControllerClimbButton`（攀爬图标）模板，`Bv.GetMotionStatus` 增加 `GamepadMode.Enabled` 分支：仅下落图标=Fly，下落+攀爬=Climb，都没有=Normal（详见 §7.2） | 🟡 已实现待验证 |
| C1 | 钓鱼：抛竿/收杆/提竿 | 待确认 | ⬜ 未确认 |
| C2 | 音游按键 | 待确认 | ⬜ 未确认 |
| C3 | 七圣召唤：选牌/确认/结束回合 | 十字键 + B/A | ⬜ 未确认 |
| C5 | 烹饪：确认/取消 | B/A | ⬜ 未确认 |
| D1 | 子会话鼠标转发 | 禁用 / 右摇杆 / 保留 | ⬜ 未确认 |
| D2 | 键鼠宏含鼠标事件 | 拒绝提示 / 尽量映射 | ⬜ 未确认 |
| D3 | JS 脚本鼠标 API | 启动告警 / 禁用 | ⬜ 未确认 |
| D4 | 地图、滑条拖拽 | 不支持 / 摇杆模拟（地图已支持） | ⬜ 未确认 |

## 5. 全量映射清单（59 项 GIActions 状态标记）


> **2026-11-25 实机测试**：从「确认」到「呼出快捷轮盘」以及全部 8 个快捷轮盘方向均已逐个测试通过，状态标为 🟢；快捷轮盘保持时间已调整为 800ms。

> 与 `GIActions` 枚举（`Core/Simulator/Extensions/Enums.cs`）逐项对照（59 项），状态统计：**🟢 已验证正确 33 项 / 🟡 已实现待验证 19 项 / 🔵 已确认待实现 1 项 / ⏸️ 暂不映射 5 项 / 🚫 无需映射 1 项**（共 59 项）。

### 5.1 移动方向（左摇杆）— 全部已实现

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| MoveForward | 向前移动 | 左摇杆 | 🟡 | 反查 W |
| MoveBackward | 向后移动 | 左摇杆 | 🟡 | 反查 S |
| MoveLeft | 向左移动 | 左摇杆 | 🟡 | 反查 A |
| MoveRight | 向右移动 | 左摇杆 | 🟡 | 反查 D |

### 5.2 战斗与角色动作 — 全部已实现并已绑定默认值（仅队伍配置 1 项未绑定，见 §5.5）

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| NormalAttack | 普通攻击 | A | 🟢 | |
| ElementalSkill | 元素战技 | RT | 🟢 | |
| ElementalBurst | 元素爆发 | Y | 🟢 | |
| Jump | 跳跃 | B | 🟢 | |
| SprintKeyboard / SprintMouse | 冲刺 | RB | 🟢 | 两个动作共用 `Sprint` |
| SwitchAimingMode | 切换瞄准模式 | LT | 🟢 | |
| SwitchToWalkOrRun | 切换走/跑 | 无（轻推左摇杆） | 🟢 | 方案 A12：保持 `None` 为最终状态 |
| Drop | 落下（滑翔/攀爬） | A | 🟢 | 方案 A13：滑翔中 A=下落攻击，攀爬中 A=取消攀爬（与 NormalAttack/Cancel 同键，上下文区分） |
| PickUpOrInteract | 拾取/交互 | X | 🟢 | |
| InteractionInSomeMode | 特定玩法交互（T） | LB + X | 🟢 | 方案 A14：`X` + 修饰键 `LeftShoulder` |
| QuickUseGadget | 快捷使用小道具 | LB + B | 🟢 | 组合键 |
| ShortcutWheel | 呼出快捷轮盘 | LB（按住） | 🟢 | 方案 A19：按住呼出轮盘，方向选择走 §5.4 轮盘机制 |
| QuestNavigation | 任务追踪 | LS（左摇杆按下） | 🟡 | |
| ElementalSight | 元素视野 | LB + 十字键← | 🟡 | 组合键 |
| AbandonChallenge | 中断挑战 | LS + RS | 🟢 | 组合键：先按住 RS（视角回正，无害）再按 LS，避免单独 LS 误触发任务追踪（方案 A15） |
| SwitchMember1 | 切换角色 1 | 十字键↑ | 🟢 | |
| SwitchMember2 | 切换角色 2 | 十字键→ | 🟢 | |
| SwitchMember3 | 切换角色 3 | 十字键← | 🟢 | |
| SwitchMember4 | 切换角色 4 | 十字键↓ | 🟢 | |
| SwitchMember5 | 切换角色 5 | LB + Y | 🟢 | 组合键 |

### 5.3 对话与确认 — 全部已实现

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| Confirm | 确认 | B | 🟢 | |
| Cancel | 取消/返回 | A | 🟢 | `KeyPress(VK_ESCAPE)` 反查为 A 正确（B1 已确认：游戏内 B=确认、A=取消/返回，语义无误）；BGI 输出待实机回归验证 |
| SkipDialog | 跳过/推进对话 | B | 🟢 | |

### 5.4 界面打开（快捷轮盘 LB + 右摇杆方向）— 🟢 已验证正确

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| OpenMap | 打开地图 | LB + 右摇杆↓ | 🟢 | 2026-11-25 实机测试通过 |
| OpenInventory | 打开背包 | LB + 右摇杆→ | 🟢 | |
| OpenQuestMenu | 打开任务菜单 | LB + 右摇杆↙ | 🟢 | |
| OpenCharacterScreen | 打开角色界面 | LB + 右摇杆↘ | 🟢 | |
| OpenBattlePassScreen | 打开纪行 | LB + 右摇杆↖ | 🟢 | |
| OpenTheEventsMenu | 打开活动面板 | LB + 右摇杆← | 🟢 | |
| OpenAdventurerHandbook | 打开冒险之证 | LB + 右摇杆↗ | 🟢 | |
| OpenWishScreen | 打开祈愿界面 | LB + 右摇杆↑ | 🟢 | 方案 A20（2026-11-24 实测更正：非十字键↑；轮盘第 8 向，8 向全满） |

### 5.5 界面打开（独立按键 / 已定方案）

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| OpenPaimonMenu | 打开派蒙界面 | Start（☰ Menu） | 🟡 | |
| OpenChatScreen | 好友聊天 | Back（⧉ View） | 🟡 | |
| OpenPartySetupScreen | 打开队伍配置 | `None` | 🟡 | 方案 A7 已落地：`PaimonMenuNavigateJob` 派蒙菜单导航（见 §7.3） |
| OpenTheSettingsMenu | 打开玩法系统界面（F6） | — | 🔵 | 方案 A9：派蒙菜单导航路径已实现，F6 语义待实机确认（见 §4.1 A9 备注 / §7.3） |
| OpenCoOpScreen | 打开多人游戏界面 | LB + 十字键→ | 🟡 | 方案 A16：`Right` + 修饰键 `LeftShoulder` |
| CheckTutorialDetails | 查看教程详情 | LB + 十字键↑ | 🟡 | 方案 A17：`Up` + 修饰键 `LeftShoulder` |
| OpenFriendsScreen | 打开好友界面 | 无直连键（默认 `None`） | 🟡 | 方案 A18 已落地：`PaimonMenuNavigateJob` 派蒙菜单导航（无活跃调用方，见 §7.3） |

### 5.6 UI 导航原语 — 全部已实现

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| FocusUp / FocusDown / FocusLeft / FocusRight | 界面焦点移动 | 左摇杆 | 🟡 | `GamepadUiNavigator` |
| ScrollUp / ScrollDown | 列表滚动 | 左摇杆上下 | 🟡 | B2：已实现待验证（待实机确认） |
| MapZoomIn | 地图放大 | LT | 🟢 | |
| MapZoomOut | 地图缩小 | RT | 🟢 | |
| CameraZoomIn | 镜头拉近 | LB（长按 500ms）+ LT（600ms） | 🟡 | 镜头距离调节为手柄专属功能；需长按 LB 再按 LT |
| CameraZoomOut | 镜头拉远 | LB（长按 500ms）+ RT（600ms） | 🟡 | 同上 |

### 5.7 ⏸️ 暂不映射与无需映射项（暂不映射 5 项 + 无需映射 1 项；另 1 项已确认待实现见 §4.1/§5.5）

> 2026-11-24 决定：以下 5 项**标记「暂不映射」**——均无内置功能调用方（仅键盘层定义），只有 JS 脚本/宏生态可能用到；有需要时再实测手柄键补映射。

| GIActions | 功能 | 键鼠默认键 | 状态 | 建议 |
|---|---|---|---|---|
| OpenTheFurnishingScreen | 打开摆设界面（尘歌壶） | F7 | ⏸️ 暂不映射 | 按需 |
| OpenStellarReunion | 打开星之归还 | F8 | ⏸️ 暂不映射 | 可不补（条件触发，无任务使用） |
| OpenNotificationDetails | 打开通知详情 | Y | ⏸️ 暂不映射 | 可不补 |
| OpenSpecialEnvironmentInformation | 打开特殊环境说明 | U | ⏸️ 暂不映射 | 可不补 |
| HideUI | 隐藏主界面 | `/` | ⏸️ 暂不映射 | 可不补（拍照场景） |
| ShowCursor | 呼出鼠标 | Alt | 🚫 | 无需映射：手柄模式无鼠标光标概念，反查表不含 Alt 是正确的 |

### 5.8 非 GIActions 的特殊映射

| 输入 | 手柄 | 状态 | 备注 |
|---|---|---|---|
| 鼠标中键（视角回正） | RS（右摇杆按下，`CameraRecenter=RightThumb`） | 🟡 | `GamepadChannel.OnMouseButton` 特判 |
| `MoveMouseBy`（鼠标相对移动） | 右摇杆脉冲（`RightStickPulseMs`，默认 30ms） | 🟡 | 精度有限；比例控制走 `SetRightStick`/`TurnByAngle` |

### 5.9 场景级缺失（GIActions 未建模）

| 场景 | 状态 | 备注 |
|---|---|---|
| 音游按键（`MusicNote11-37`、`MusicLeft*` 等游戏设置项） | ⬜ 未建模 | AutoMusicGame 适配时需新增专用动作，绕过键盘反查直走 `GamepadChannel` |
| 钓鱼：抛竿/收杆/提竿 | ⬜ 待确认项 C1 | 待实机确认按键提示 |
| 七圣召唤：选牌/确认/结束回合 | ⬜ 待确认项 C3 | 建议 十字键 + B/A |
| 烹饪：确认/取消 | ⬜ 待确认项 C5 | 建议 B/A |

### 5.10 键盘反查表（ReverseMappableActions）覆盖说明

`GamepadChannel.ReverseMappableActions` 含 42 项：§5.1–§5.5 已实现的键盘动作全部在内（含 WASD 与 2026-11-24 新增的 P/F2/G/O/Tab/F3），因此**键鼠宏/JS 脚本直按这些键可正确映射到手柄**。不在反查表的按键（如 F6/F7/F8、Alt、`/` 等）会 warn 丢弃——与 §5.7 缺失项一一对应。另 `MapZoomIn/Out`、`CameraZoomIn/Out` 为手柄专属动作（无键盘键），仅在 `TryGetButton`/`GamepadUiNavigator` 中定义，不进反查表。

### 5.11 映射覆盖全量审计（2026-11-24）

**GIActions 59 项覆盖**：52 项有手柄映射路径（`TryGetButton`/`TryGetWheel`/`TryGetModifier`/`TryGetDirection` 四路合计 69 个 case）；**7 项无映射**：

| GIActions | 键盘键 | 状态 | 说明 |
|---|---|---|---|
| OpenTheSettingsMenu | F6 | 🔵 | A9：派蒙菜单导航路径已实现，F6 语义待实机确认（`EnterAndExitWonderlandJob:23` VK_F6 直按丢弃） |
| ShowCursor | Alt | 🚫 | 手柄无鼠标语义，永久不映射 |
| OpenTheFurnishingScreen | F7 | ⏸️ | 暂不映射（2026-11-24 决定）：尘歌壶摆设界面，无内置调用方，有需要再补 |
| OpenStellarReunion | F8 | ⏸️ | 暂不映射：星之归还，同上 |
| OpenNotificationDetails | Y | ⏸️ | 暂不映射：通知详情，同上 |
| OpenSpecialEnvironmentInformation | U | ⏸️ | 暂不映射：特殊环境说明，同上 |
| HideUI | `/` | ⏸️ | 暂不映射：隐藏主界面，同上 |

后 5 项 ⏸️ 在 GameTask 内无 `SimulateAction` 调用（仅键盘层定义），已标记**暂不映射（低优先级）**——只有 JS 脚本/宏生态按 F7/F8/Y/U/`/` 时会丢弃，有需要再实测补映射。

**功能点裸 VK 审计**（GameTask 98 处 `VK.VK_*` 使用归类）：

| 裸 VK | 处数 | 反查结果 | 结论 |
|---|---|---|---|
| VK_ESCAPE | 30+ | Cancel → A ✓ | 全部可用 |
| VK_SPACE | 5（ChooseTalkOptionTask ×3、AutoCookTask 等） | 优先 Confirm → B ✓（反查表首项语义处理） | 语义正确 |
| VK_W/A/S/D | 若干 | Move* → 左摇杆 ✓ | 可用 |
| VK_X | 1（AutoLeyLineOutcrop:1401） | Drop → A ✓ | 可用 |
| VK_F | 1（TpTask:2719） | PickUpOrInteract ✓ | 可用 |
| **VK_RETURN** | 1（ReturnMainUiTask:53 兜底） | **无反查 → 丢弃** | 兜底链手柄模式少一环，建议改 `SimulateAction(Confirm)`（影响小） |
| VK_F6 | 1（EnterAndExitWonderlandJob:23） | 无映射 | A9 🔵 已知 |
| **VK_1-4 `GetAsyncKeyState`** | 1（SkillCdTrigger:360） | **不走 InputHub，读物理键盘状态** | **新发现功能缺口**：手柄玩家十字键切人 BGI 感知不到 → **技能 CD 悬浮窗手柄模式不触发**；需监听虚拟手柄十字键（ViGEm 回环/XInput 轮询） |
| 字母键直发 | AutoMusicGame | A/S/D → 左摇杆（危险） | 批次 5 已知 |
| VK_LBUTTON 状态读取 | AutoFishing:364 | 鼠标状态检测 | 批次 5 已知 |

## 6. 注意事项

- 旧版 `User/Config.json` 中手柄绑定可能仍是 `None`，升级后需在「按键绑定设置 → 手柄」点一次**恢复默认**。
- 手柄模式必须在游戏内和 BGI 内同时开启才会生效。
- 所有绝对坐标点击仍无法直接映射为手柄；菜单类模块需改用焦点导航 + 确认。

## 7. 游戏界面判断（场景识别）调查结果

> 2026-11-24 审计结论：所需三个场景（**对话界面 / 地图界面 / 大世界画面**）的判断能力**均已具备**，无需新建设施。
> 代码位置：`GameTask/Common/BgiVision/BvStatus.cs`（`Bv` 静态类）+ `GameTask/TaskTriggerDispatcher.cs`（调度层）。

### 7.1 已具备能力

| 场景 | 判断方法 | 机制 | 状态 |
|---|---|---|---|
| 对话界面判断 | `Bv.IsInTalkUi()` | 键鼠模式：DisabledUiButton 模板；**手柄模式特判**：View（双方框）+ Menu（三横杠）两个按钮同时出现才算对话（`GamepadMode.Enabled` 分支，命中时遮罩窗口画红框便于调试） | 🟢 **已验证正确**（2026-11-24 实机：剧情对话中 View+Menu 双命中稳定） |
| 地图界面判断 | `Bv.IsInBigMapUi()` | 地图缩放按钮 / 设置按钮模板匹配（ROI 固定 1080p 坐标）；另有 `BigMapIsUnderground()`（地下图层）、`GetBigMapScale()`（缩放级别） | 🟡 识别层部分实证：传送按钮/选项列表/传送点图标两模式一致（2026-11-24 实测，模板可复用）；缩放/设置控件（`MapScaleButton`/`MapSettingsButton`）命中率待验证 |
| 大世界画面判断 | `Bv.IsInMainUi()` | 左上四分之一 ROI 匹配派蒙头像 + 复苏提示排除；`WaitForMainUi()` 异步等待 | ⬜ 待实机验证，风险分层：**派蒙头像两模式大概率一致（低风险）**；**复苏排除 `IsInRevivePrompt` 高风险**——门禁是 AutoFight 黑色确认按钮模板（键鼠样式），手柄模式复苏弹窗按钮显示手柄按键提示样式，模板可能不命中 → 门禁失败后 OCR「复苏」不执行 → **全队死亡时复苏弹窗检测失效，`IsInMainUi` 误判为大世界**（影响 `WaitForMainUi`/返回主界面等调用方） |
| 统一入口 | `Bv.WhichGameUi()` | `GameUiCategory { Talk > BigMap > Main > Unknown }` 按优先级分类；`TaskTriggerDispatcher` 每帧计算 `CurrentGameUiCategory`，触发器按 `SupportedGameUiCategory` 过滤（AutoSkip=Talk、QuickTeleport=BigMap、MapMask 含 BigMap 判断） | 🟡 待验证 |

> ⚠️ **核心待验证假设（2026-11-24 修正）**：游戏连接手柄后可能**整体切换为手柄 UI 布局**——不只按键提示图标变化，控件的位置、大小、样式都可能不同。风险分层：派蒙头像两模式大概率一致（低风险）；**地图缩放控件与复苏确认按钮模板为键鼠样式资产，手柄模式命中率高风险**。必须手柄模式下实机截图比对、逐项验证模板命中率后才能转 🟢。若命中率不足，需按对话判断的先例补 `GamepadMode.Enabled` 分支 + 手柄模式专用模板（走外部资源包）；复苏排除失效的后果是全队死亡时 `IsInMainUi` 误判为大世界。

其他可复用判断：`IsInDomain()`（秘境）、`IsInPartyViewUi()`（队伍选择）、`IsInAnyClosableUi()`（任意可关闭 UI）、`IsInPromptDialog()`（提示/确认框）、`IsInBlessingOfTheWelkinMoon()`（空月祝福）、`DetectChatUi()`（聊天面板状态机：Closed/PanelOpen/InputOpen）。

> 注：触发器层使用 `WhichGameUiForTriggers`（Talk > BigMap，**不判 Main**——派蒙模板匹配每帧开销大，属性能取舍），因此 `CaptureContent.CurrentGameUiCategory` 永远不会是 `Main`；任务层使用完整版 `WhichGameUi`（含 Main）。目前无触发器声明 `SupportedGameUiCategory=Main`，无实际影响；未来若有「仅主界面」触发器需注意此设计。

### 7.2 运动状态判断（B6）：手柄适配已实现

`Bv.GetMotionStatus()`（`MotionStatus.Normal/Fly/Climb`）原先只识别键鼠 `SpaceKey`/`XKey` 提示，手柄模式下恒返回 Normal。受影响调用点 20 处：

- `AutoPathing/TrapEscaper`、`AutoPathing/SkillBoostHelper`（11 处）、`AutoPathing/PathExecutor`
- `AutoPathing/Handler/UpDownGrabLeafHandler`、`AutoPathing/Handler/StopFlyingHandler`
- `AutoTrackPath/AutoTrackPathTask`

**实现（2026-11-25）**：`Bv.GetMotionStatus` 增加 `GamepadMode.Enabled` 分支，识别右下角技能区的**手柄动作图标**（不是常驻的 A/B 键位角标）：

| 状态 | 手柄 HUD 特征（实机截图确认） | 识别对象 |
|---|---|---|
| Normal | 无滑翔/攀爬图标（只有普通攻击剑图标） | — |
| Fly（滑翔） | 出现“下落/人物+下箭头”图标（B 键位） | `ControllerJumpButton` |
| Climb（攀爬） | 下落图标 + “攀爬/人物+上箭头”图标（A 键位）同时出现 | `ControllerJumpButton` + `ControllerClimbButton` |

判定逻辑：

```csharp
if (GamepadMode.Enabled)
{
    var jump = Find("ControllerJumpButton");
    if (!jump) return Normal;
    var climb = Find("ControllerClimbButton");
    return climb ? Climb : Fly;
}
```

资源与 ROI：

- 模板：`GameTask/Common/Element/Assets/1920x1080/controller_jump_button.png`、`controller_climb_button.png`；
- 识别对象：`Recognition.json` 的 `ControllerJumpButton`/`ControllerClimbButton`，ROI `@motionKeyTip = rect(cw - 300*s, ch - 230*s, 240*s, 170*s)`，`draw=true` 便于调试；
- 离线 NCC 验证（2730×1536 实机截图）：下落模板命中滑翔 0.995、攀爬 0.975；攀爬模板只命中攀爬 0.996，地图界面均 <0.5。

⚠️ 待实机验证：游泳/水下若也显示下落图标，会被判定为 Fly；需确认键鼠模式游泳是否同样出现空格提示，若是则保持同构即可。

### 7.3 派蒙菜单导航落地（A7/A18 已实现，A9 导航路径已实现）

新增 `GameTask/Common/Job/PaimonMenuNavigateJob.cs`（2026-11-24）：打开派蒙菜单（Start）→ OCR 定位目标菜单项（白名单过滤噪音）→ 左摇杆 `FocusDown` 逐项导航 → 确认（B）。`OpenScreenAsync(GIActions, ct)` 模式感知分派：手柄模式走菜单导航，键鼠模式直按绑定键（8 个 `OpenPartySetupScreen` 调用点已接入）。

⚠️ **待实机验证的假设**（若不成立需调整实现）：

1. **初始焦点假设**：打开派蒙菜单后焦点默认在第一项「队伍配置」（A7 场景因此无需导航直接确认）。若实机验证初始焦点不在第一项，需改为高亮检测导航。
2. **单次脉冲移动一项**：左摇杆 `FocusDown/FocusUp` 单次脉冲在菜单中移动一项（与 B2 列表滚动同一未验证点）。
3. **「设置」文字标签**：A9 依赖派蒙菜单设置入口有可 OCR 的「设置」文字；若齿轮图标无文字标签，A9 需回退图标识别方案（图标资源走外部资源包）。
4. **A9 语义待确认**：`OpenTheSettingsMenu`（F6）唯一活跃调用点 `EnterAndExitWonderlandJob`（千星奇域入口）的按键语义为「打开玩法系统界面（尘歌壶猫尾酒馆等特定场景）」，与派蒙菜单「设置」可能不是同一界面，**该调用点未接线**；待实机确认 F6 实际语义后再决定。
5. **战斗中派蒙菜单**（2026-11-24 检查补充）：AutoFight 战斗结束检测路径（`AutoFightTask` 924/963、`AutoFightSeek` 324/335 已接入 A7 导航）会在**战斗中**按 Start 打开派蒙菜单再导航到队伍配置——单机战斗中暂停菜单应可正常打开（游戏暂停）；联机不暂停但菜单可开。待实机确认。
6. **手柄模板资源包依赖**（2026-11-24 检查补充）：4 个手柄模板（`ControllerMenuButton`/`ControllerViewButton`/`ControllerChoiceButton`/`ControllerPick`）已在 `GameTask/AutoSkip/Assets/Recognition.json` 与 `GameTask/AutoPick/Assets/Recognition.json` 中声明，PNG 图像需由 BetterGI.Assets 外部资源包提供（仓库禁止新增 `Assets` 目录）——需确认资源包已包含对应图像。

### 7.4 地图界面手柄操作语义（2026-11-24 实测，批次 3 重构依据）

手柄模式大地图 UI 的上下文操作与键鼠的「拖拽/滚轮/点击」完全不同，是 TpTask/QuickTeleport 手柄化重构的交互规范：

| 操作 | 手柄 | 对比键鼠 |
|---|---|---|
| 光标 | 左摇杆移动**选中圈光标**（屏幕上可见的圈，可作视觉伺服特征） | 鼠标自由光标 |
| 视口移动 | 通过移动选中圈实现 | 按住左键拖拽 |
| 选点与传送 | 圈对准传送点图标后**单击 B 选中** → 面板弹出后**再次单击 B 确认传送**（两段式，2026-11-24 实测） | 左键点击 + 点传送按钮 |
| 缩放 | **LT 放大 / RT 缩小**（2026-11-25 地图实机截图确认） | 滚轮 |
| 候选列表（同名点待选） | 左摇杆上下选中待选项 → **再次单击 B 确认传送**（2026-11-24 实测） | 左键点击列表项 |
| 区域列表 | **Y** 打开（默认显示当前区域，屏幕右侧列出全部可选区域）→ 左摇杆上下左右选中 → **B** 跳转该区域默认传送点 → 再 **B** 传送 | 左键点击左上角区域名开列表再点击 |

识别层实证：传送按钮、选项列表、传送点图标**两模式一致**，模板可直接复用；仅确认/取消等按键提示角标（A/B 图标）不同。

**实测结论（2026-11-24，五项全部落地）**：
① **LT 放大、RT 缩小**——`MapZoomIn=LeftTrigger`、`MapZoomOut=RightTrigger`（2026-11-25 地图实机截图：右下角提示 LT 放大 / RT 缩小；镜头距离调节 LB+LT 拉近与本项不同语义）；
② 单击 B 选中传送点、再次单击 B 确认传送，面板无需导航；
③ **TODO（待补充截图）**：选中圈截图，做视觉伺服模板（走外部资源包）；
④ **缩放滑块在地图界面缩放时会显示**——`MapScaleButton` 模板可继续用于 zoom 闭环测量；TODO：待补一张手柄模式地图截图（含滑块与选中圈）复核；
⑤ 候选列表：识别传送锚点图标（复用 `MapChooseIcon` 模板）定位目标项，左摇杆导航 + B 确认传送。

### 7.5 手柄实现待实测假设与阻塞点（2026-11-24 实现批次）

**本轮已实现（🟡 待实机验证）**：

| 功能 | 实现 | 关键假设（待实测） |
|---|---|---|
| QuickTeleport 传送按钮 | 手柄模式按 B（Confirm）替代 `ra.Click()`（两段式第二段） | ① 面板出现时按 B 直接触发传送——**用户已实测** ✓ |
| QuickTeleport 候选列表 | 手柄模式按 B 替代点击最高项 | ② 假设列表默认焦点在第一项（最高项）；若无默认焦点需补左摇杆导航 + 高亮检测 |
| ChooseTalkOptionTask 选项导航 | `NavigateAndConfirmByGamepadAsync`：OCR 选项按 Y 升序取索引 → `FocusDown`×索引 → B 确认（覆盖 SingleSelectText / SelectLastOptionOnce / SelectLastOptionUntilEnd） | ③ 对话选项弹出时默认焦点在第一项；④ 单次 FocusDown 脉冲移动一项（250ms 间隔）；⑤ OCR 结果索引不受污染文本影响 |
| AutoSkip 感叹号/选项/底部三角 | 三处点击改手柄按 B（:882 / :1121 / :1378） | ⑥ 同 ③④ |
| AutoFight 宏鼠标命令 | `Avatar.cs` 手柄模式限频告警（mousedown/mouseup/click/scroll 丢弃提示，30s 一次） | 无假设 |

**阻塞点（未实现，待实测后补）**：

1. **邀约**（`AutoSkipTrigger` :704 跳过按钮 / :1267 邀约选项 / `HangoutOption`）：手柄模式邀约界面的跳过与选项选择方式未知，邀约剧情手柄语义待实测；
2. **提交物品**（`SubmitGoods` :1518/:1526/:1538）：手柄模式提交物品界面的物品选择方式未知（左摇杆+B？），黑白确认按钮按 B 为合理假设但整链阻塞未改；
3. **AutoPick 滚轮场景**（:337 `VerticalScroll`）：多物品拾取列表滚动的手柄对应操作未知；
4. **PathExecutor 复苏点击**（:334/:651 `ClickIfInReviveModal`）：复苏弹窗手柄默认焦点与确认键未知——若默认焦点在「取消」，按 B 会误退导致死亡状态循环，**有风险未实现**，待实测弹窗焦点行为；
5. **ClickChatExitUntilMainUi**（`ChooseTalkOptionTask` chatExit 点击）：手柄模式退出对话按钮的对应按键未知（A？），邀约/对话强退场景待实测。