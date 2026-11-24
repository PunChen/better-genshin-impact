# 手柄按键映射明细（feat/xbox-game-input-main）

> 依据游戏内「设置 → 按键 → 手柄」截图与逐项确认整理（2026-11）。
> 对应代码：`GamepadConfig`、`GamepadActionMap`、`GamepadInputBackend`、`GamepadUiNavigator`。
> 总体方案见 [gamepad-adaptation-plan.md](./gamepad-adaptation-plan.md)。

> **标记体系（2026-11-24 起）**：⬜ 未确认 → 🔵 已确认待实现 → 🟡 已实现待验证 → 🟢 已验证正确；🚫 无需映射（终态）。
> 「已确认」= 游戏内手柄按键/方案经实测确认；「已验证」= BGI 手柄输出经实机验证与游戏响应一致（当前暂无 🟢 项，实机回归后逐项标记）。

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

> 该布局与基础战斗不同，AutoFight 枪械模式后续需要独立 profile。

## 2. 已确认并落地的映射（🟡 已实现待验证）

### 2.1 切人与道具（组合键）

| 功能 | 手柄 | 配置字段 |
|---|---|---|
| 切换小队角色 1 | 十字键↑ | `SwitchMember1=Up` |
| 切换小队角色 2 | 十字键→ | `SwitchMember2=Right` |
| 切换小队角色 3 | 十字键← | `SwitchMember3=Left` |
| 切换小队角色 4 | 十字键↓ | `SwitchMember4=Down` |
| 切换小队角色 5 | **LB + Y** | `SwitchMember5=Y`、`SwitchMember5Modifier=LeftShoulder` |
| 快捷使用小道具 | **LB + B** | `QuickUseGadget=B`、`QuickUseGadgetModifier=LeftShoulder` |
| 任务追踪 | **LS（左摇杆按下）** | `QuestNavigation=LeftThumb` |
| 元素视野 | **LB + 十字键左** | `ElementalSight=Left`、`ElementalSightModifier=LeftShoulder` |
| 视角回正（鼠标中键） | RS（右摇杆按下；PS 手柄称 R3） | `CameraRecenter=RightThumb` |

组合键执行顺序：按住修饰键 → 按目标键 → 保持 `PressDurationMs` → 释放目标键 → 释放修饰键。

### 2.2 快捷轮盘（LB + 右摇杆方向）

| 功能 | 手柄 | 配置字段 |
|---|---|---|
| 打开地图 | LB + 右摇杆↓ | `OpenMapWheelDirection=Down` |
| 打开背包 | LB + 右摇杆→ | `OpenInventoryWheelDirection=Right` |
| 打开任务菜单 | LB + 右摇杆↙ | `OpenQuestMenuWheelDirection=DownLeft` |
| 打开角色界面 | LB + 右摇杆↘ | `OpenCharacterScreenWheelDirection=DownRight` |
| 打开纪行 | LB + 右摇杆↖ | `OpenBattlePassScreenWheelDirection=UpLeft` |
| 打开活动面板 | LB + 右摇杆← | `OpenTheEventsMenuWheelDirection=Left` |
| 打开冒险之证 | LB + 右摇杆↗ | `OpenAdventurerHandbookWheelDirection=UpRight` |
| 打开祈愿界面 | LB + 右摇杆↑ | `OpenWishScreenWheelDirection=Up` |

修饰键均为 `LeftShoulder`；执行流程：按住 LB 打开轮盘 → 拨右摇杆 → 保持 `WheelSelectionHoldMs`（默认 180ms）→ 回中 → 松开 LB。

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
| B6 | 运动状态判断（滑翔/攀爬）手柄适配 | `Bv.GetMotionStatus` 依赖键盘按键提示图标（SpaceKey/XKey 模板），手柄模式下游戏显示手柄提示，判断**恒返回 Normal**；20 个调用点受影响（TrapEscaper、SkillBoostHelper、PathExecutor、StopFlyingHandler、UpDownGrabLeafHandler、AutoTrackPathTask）。需采集手柄模式下滑翔/攀爬的按键提示截图并新增模板（调查结果详见 §7.2） | ⬜ 未确认（需实机截图） |
| C1 | 钓鱼：抛竿/收杆/提竿 | 待确认 | ⬜ 未确认 |
| C2 | 音游按键 | 待确认 | ⬜ 未确认 |
| C3 | 七圣召唤：选牌/确认/结束回合 | 十字键 + B/A | ⬜ 未确认 |
| C5 | 烹饪：确认/取消 | B/A | ⬜ 未确认 |
| D1 | 子会话鼠标转发 | 禁用 / 右摇杆 / 保留 | ⬜ 未确认 |
| D2 | 键鼠宏含鼠标事件 | 拒绝提示 / 尽量映射 | ⬜ 未确认 |
| D3 | JS 脚本鼠标 API | 启动告警 / 禁用 | ⬜ 未确认 |
| D4 | 地图、滑条拖拽 | 不支持 / 摇杆模拟（地图已支持） | ⬜ 未确认 |

## 5. 全量映射清单（59 项 GIActions 状态标记）

> 与 `GIActions` 枚举（`Core/Simulator/Extensions/Enums.cs`）逐项对照（59 项），状态统计：**🟡 已实现待验证 52 项 / 🔵 已确认待实现 1 项 / ⬜ 未确认 5 项 / 🚫 无需映射 1 项**（暂无 🟢 已验证正确项）。

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
| NormalAttack | 普通攻击 | A | 🟡 | |
| ElementalSkill | 元素战技 | RT | 🟡 | |
| ElementalBurst | 元素爆发 | Y | 🟡 | |
| Jump | 跳跃 | B | 🟡 | |
| SprintKeyboard / SprintMouse | 冲刺 | RB | 🟡 | 两个动作共用 `Sprint` |
| SwitchAimingMode | 切换瞄准模式 | LT | 🟡 | |
| SwitchToWalkOrRun | 切换走/跑 | 无（轻推左摇杆） | 🟡 | 方案 A12：保持 `None` 为最终状态 |
| Drop | 落下（滑翔/攀爬） | A | 🟡 | 方案 A13：滑翔中 A=下落攻击，攀爬中 A=取消攀爬（与 NormalAttack/Cancel 同键，上下文区分） |
| PickUpOrInteract | 拾取/交互 | X | 🟡 | |
| InteractionInSomeMode | 特定玩法交互（T） | LB + X | 🟡 | 方案 A14：`X` + 修饰键 `LeftShoulder` |
| QuickUseGadget | 快捷使用小道具 | LB + B | 🟡 | 组合键 |
| ShortcutWheel | 呼出快捷轮盘 | LB（按住） | 🟡 | 方案 A19：按住呼出轮盘，方向选择走 §5.4 轮盘机制 |
| QuestNavigation | 任务追踪 | LS（左摇杆按下） | 🟡 | |
| ElementalSight | 元素视野 | LB + 十字键← | 🟡 | 组合键 |
| AbandonChallenge | 中断挑战 | LS + RS | 🟡 | 组合键：先按住 RS（视角回正，无害）再按 LS，避免单独 LS 误触发任务追踪（方案 A15） |
| SwitchMember1 | 切换角色 1 | 十字键↑ | 🟡 | |
| SwitchMember2 | 切换角色 2 | 十字键→ | 🟡 | |
| SwitchMember3 | 切换角色 3 | 十字键← | 🟡 | |
| SwitchMember4 | 切换角色 4 | 十字键↓ | 🟡 | |
| SwitchMember5 | 切换角色 5 | LB + Y | 🟡 | 组合键 |

### 5.3 对话与确认 — 全部已实现

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| Confirm | 确认 | B | 🟡 | |
| Cancel | 取消/返回 | A | 🟡 | `KeyPress(VK_ESCAPE)` 反查为 A 正确（B1 已确认：游戏内 B=确认、A=取消/返回，语义无误）；BGI 输出待实机回归验证 |
| SkipDialog | 跳过/推进对话 | B | 🟡 | |

### 5.4 界面打开（快捷轮盘 LB + 右摇杆方向）— 全部已实现

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| OpenMap | 打开地图 | LB + 右摇杆↓ | 🟡 | |
| OpenInventory | 打开背包 | LB + 右摇杆→ | 🟡 | |
| OpenQuestMenu | 打开任务菜单 | LB + 右摇杆↙ | 🟡 | |
| OpenCharacterScreen | 打开角色界面 | LB + 右摇杆↘ | 🟡 | |
| OpenBattlePassScreen | 打开纪行 | LB + 右摇杆↖ | 🟡 | |
| OpenTheEventsMenu | 打开活动面板 | LB + 右摇杆← | 🟡 | |
| OpenAdventurerHandbook | 打开冒险之证 | LB + 右摇杆↗ | 🟡 | |
| OpenWishScreen | 打开祈愿界面 | LB + 右摇杆↑ | 🟡 | 方案 A20（2026-11-24 实测更正：非十字键↑；轮盘第 8 向，8 向全满） |

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
| MapZoomIn | 地图放大 | LT | 🟡 | |
| MapZoomOut | 地图缩小 | RT | 🟡 | |
| CameraZoomIn | 镜头拉近 | LB + LT | 🟡 | 镜头距离调节为手柄专属功能，键盘无对应键，不进反查表 |
| CameraZoomOut | 镜头拉远 | LB + RT | 🟡 | 同上 |

### 5.7 ⬜ 未确认与无需映射项（未确认 5 项 + 无需映射 1 项；另 1 项已确认待实现见 §4.1/§5.5）

| GIActions | 功能 | 键鼠默认键 | 状态 | 建议 |
|---|---|---|---|---|
| OpenTheFurnishingScreen | 打开摆设界面（尘歌壶） | F7 | ⬜ | 按需 |
| OpenStellarReunion | 打开星之归还 | F8 | ⬜ | 可不补（条件触发，无任务使用） |
| OpenNotificationDetails | 打开通知详情 | Y | ⬜ | 可不补 |
| OpenSpecialEnvironmentInformation | 打开特殊环境说明 | U | ⬜ | 可不补 |
| HideUI | 隐藏主界面 | `/` | ⬜ | 可不补（拍照场景） |
| ShowCursor | 呼出鼠标 | Alt | 🚫 | 无需映射：手柄模式无鼠标光标概念，反查表不含 Alt 是正确的 |

### 5.8 非 GIActions 的特殊映射

| 输入 | 手柄 | 状态 | 备注 |
|---|---|---|---|
| 鼠标中键（视角回正） | RS（右摇杆按下，`CameraRecenter=RightThumb`） | 🟡 | `GamepadChannel.OnMouseButton` 特判 |
| `MoveMouseBy`（鼠标相对移动） | 右摇杆 30ms 脉冲 | 🟡 | 精度有限；比例控制走 `SetRightStick`/`TurnByAngle` |

### 5.9 场景级缺失（GIActions 未建模）

| 场景 | 状态 | 备注 |
|---|---|---|
| 音游按键（`MusicNote11-37`、`MusicLeft*` 等游戏设置项） | ⬜ 未建模 | AutoMusicGame 适配时需新增专用动作，绕过键盘反查直走 `GamepadChannel` |
| 钓鱼：抛竿/收杆/提竿 | ⬜ 待确认项 C1 | 待实机确认按键提示 |
| 七圣召唤：选牌/确认/结束回合 | ⬜ 待确认项 C3 | 建议 十字键 + B/A |
| 烹饪：确认/取消 | ⬜ 待确认项 C5 | 建议 B/A |

### 5.10 键盘反查表（ReverseMappableActions）覆盖说明

`GamepadChannel.ReverseMappableActions` 含 43 项：§5.1–§5.5 已实现的键盘动作全部在内（含 WASD 与 2026-11-24 新增的 P/F2/G/O/Tab/F3），因此**键鼠宏/JS 脚本直按这些键可正确映射到手柄**。不在反查表的按键（如 F6/F7/F8、Alt、`/` 等）会 warn 丢弃——与 §5.7 缺失项一一对应。另 `MapZoomIn/Out`、`CameraZoomIn/Out` 为手柄专属动作（无键盘键），仅在 `TryGetButton`/`GamepadUiNavigator` 中定义，不进反查表。

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

### 7.2 已知缺口（B6）：运动状态判断手柄失效

`Bv.GetMotionStatus()`（`MotionStatus.Normal/Fly/Climb`，滑翔/攀爬运动状态）依赖**键盘按键提示图标**（`SpaceKey`/`XKey` 模板）：手柄模式下游戏显示手柄按键提示，键盘提示图标不再出现，判断**恒返回 Normal**。受影响调用点 20 处：

- `AutoPathing/TrapEscaper`、`AutoPathing/SkillBoostHelper`（11 处）、`AutoPathing/PathExecutor`
- `AutoPathing/Handler/UpDownGrabLeafHandler`、`AutoPathing/Handler/StopFlyingHandler`
- `AutoTrackPath/AutoTrackPathTask`

**修复方案（推荐，2026-11-24）**：与键鼠逻辑对称——`GetMotionStatus` 增加 `GamepadMode.Enabled` 分支，用**手柄按键提示图标**替代键盘键帽模板（走外部资源包流程）：

| 状态 | 键鼠模式 HUD 特征（现行逻辑） | 手柄模式预期特征（待截图确认） |
|---|---|---|
| Normal | 无 空格/X 提示 | 无 B/A 提示 |
| Fly（滑翔） | 空格提示出现，无 X 提示 | B 提示出现（跳跃/上升），无 A 提示 |
| Climb（攀爬） | 空格 + X 提示同时出现 | B + A 提示同时出现（A=取消攀爬，对应 A13） |

实现骨架：`if (GamepadMode.Enabled)` 分支中模板匹配手柄 B/A 提示图标（`ControllerJumpButton`/`ControllerDropButton`，命名暂定），`bExist ? (aExist ? Climb : Fly) : Normal`。

**需采集的对照截图**（⬜）：① 手柄模式滑翔 HUD（右下按键提示区）；② 手柄模式攀爬 HUD；③ 手柄模式游泳/水下 HUD（边界情况：游泳若也显示 B 提示会误判为 Fly——需确认键鼠模式游泳是否同样显示空格提示，若是则手柄保持同构行为即可）；④ 键鼠模式对应三张（顺带复核现行键鼠逻辑的空格/X 组合假设）。

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
| 选点 | 圈对准传送点图标后按 **B** | 左键点击 |
| 缩放 | **LT/RT**（放大/缩小方向待确认） | 滚轮 |
| 候选列表（同名点待选） | 左摇杆上下选择 + **B** 确认 | 左键点击列表项 |
| 区域列表 | **Y** 打开（默认显示当前区域，屏幕右侧列出全部可选区域）→ 左摇杆上下左右选中 → **B** 跳转该区域默认传送点 → 再 **B** 传送 | 左键点击左上角区域名开列表再点击 |

识别层实证：传送按钮、选项列表、传送点图标**两模式一致**，模板可直接复用；仅确认/取消等按键提示角标（A/B 图标）不同。

**待确认**：① LT/RT 放大/缩小方向；② 选中传送点后传送面板弹出，B 是否直接触发传送（决定 QuickTeleport 传送按钮 `ra.Click()` 的替代方式：直接按 B vs 焦点导航）；③ 选中圈的视觉特征截图（做视觉伺服模板，走外部资源包）；④ `MapScaleButton`/`MapSettingsButton` 模板手柄模式命中率（LT/RT 缩放时滑块控件是否照常显示——zoom 闭环测量依赖它）。
