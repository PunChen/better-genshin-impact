# 手柄按键映射明细（feat/xbox-game-input-main）

> 依据游戏内「设置 → 按键 → 手柄」截图与逐项确认整理（2026-11）。
> 对应代码：`GamepadConfig`、`GamepadActionMap`、`GamepadInputBackend`、`GamepadUiNavigator`。
> 总体方案见 [gamepad-adaptation-plan.md](./gamepad-adaptation-plan.md)。

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
| 呼出快捷轮盘 | LB | 各 `*WheelModifier` | `LeftShoulder` |
| 打开派蒙界面 | ☰ Menu | `OpenPaimonMenu` | `Start` |
| 好友聊天 | ⧉ View | `OpenChatScreen` | `Back` |
| 确认 / 取消 | B / A | `Confirm` / `Cancel` | `B` / `A` |
| 移动角色 | 左摇杆 | `GIActions.Move*` | 左摇杆（内置默认，开启手柄模式即生效，非独立可绑定按键） |
| 转动视角 | 右摇杆 | `MoveMouseBy` | 右摇杆（内置默认，开启手柄模式即生效，非独立可绑定按键） |

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

## 2. 已确认并落地的映射

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
GamepadUiNavigator.DragMap(1, 0, 300);
```

底层还提供：

```csharp
((GamepadInputBackend)InputHub.Backend).SetLeftStick(x, y, durationMs);
((GamepadInputBackend)InputHub.Backend).SetRightStick(x, y, durationMs);
((GamepadInputBackend)InputHub.Backend).TurnByAngle(degrees);
```

## 4. 已定方案与待确认项

### 4.1 已定方案（待实施）

| 编号 | 项目 | 方案 |
|---|---|---|
| A7 | 打开队伍配置 | 菜单 + 文字识别：打开派蒙菜单（Start）→ 文字识别定位「队伍配置」→ 左摇杆上下左右导航锁定该项 → 确认 |
| A9 | 打开设置菜单（F6） | 菜单 + 图标识别：打开派蒙菜单（Start）→ 左摇杆上下左右导航锁定齿轮形状图标 → 确认。⚠️ 需要采集齿轮图标的图片资源才能识别（仓库禁止新增 `Assets` 目录，模板需走外部资源包流程） |
| A12 | 切换走/跑 | 采纳建议：不做映射，轻推左摇杆即走。`SwitchToWalkOrRun` 保持 `None` 即为最终状态，无需改动 |
| A13 | 落下（滑翔/攀爬中） | 实测确认为 **A**（原建议 B 作废）：滑翔状态按 A 触发下落攻击；攀爬状态按 A 取消攀爬。**已落地**：`Drop` 默认 `A`（与 NormalAttack/Cancel 同键，由游戏按上下文区分语义） |
| A14 | 特定玩法交互（T） | 实测确认为 **LB + X**：`InteractionInSomeMode=X`、`InteractionInSomeModeModifier=LeftShoulder`（X 与 PickUpOrInteract 同键，加 LB 修饰区分）。**已落地** |
| A15 | 中断挑战（P） | 实测确认为 **LS + RS**（两摇杆同时按下）。**已落地**：`AbandonChallenge=LeftThumb`、`AbandonChallengeModifier=RightThumb`（先按住 RS 再按 LS，避免单独按下 LS 误触发任务追踪） |
| A16 | 打开多人游戏界面（F2） | 实测确认为 **LB + 十字键→**。**已落地**：`OpenCoOpScreen=Right`、`OpenCoOpScreenModifier=LeftShoulder` |
| A17 | 查看教程详情（G） | 实测确认为 **LB + 十字键↑**。**已落地**：`CheckTutorialDetails=Up`、`CheckTutorialDetailsModifier=LeftShoulder` |

### 4.2 待确认项

| 编号 | 项目 | 当前建议 |
|---|---|---|
| B2 | 列表滚动 | 左摇杆上下（已按此实现，待实机确认） |
| B3 | 标签页切换 | LB / RB |
| C1 | 钓鱼：抛竿/收杆/提竿 | 待确认 |
| C2 | 音游按键 | 待确认 |
| C3 | 七圣召唤：选牌/确认/结束回合 | 十字键 + B/A |
| C5 | 烹饪：确认/取消 | B/A |
| D1 | 子会话鼠标转发 | 禁用 / 右摇杆 / 保留 |
| D2 | 键鼠宏含鼠标事件 | 拒绝提示 / 尽量映射 |
| D3 | JS 脚本鼠标 API | 启动告警 / 禁用 |
| D4 | 地图、滑条拖拽 | 不支持 / 摇杆模拟（地图已支持） |

## 5. 全量映射清单（57 项 GIActions 状态标记）

> 与 `GIActions` 枚举（`Core/Simulator/Extensions/Enums.cs`）逐项对照，状态统计：**✅ 已实现 46 项 / ⚠️ 已实现但默认未绑定 1 项 / ❌ 缺失 9 项 / 🚫 无需映射 1 项**。

### 5.1 移动方向（左摇杆）— 全部已实现

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| MoveForward | 向前移动 | 左摇杆 | ✅ | 反查 W |
| MoveBackward | 向后移动 | 左摇杆 | ✅ | 反查 S |
| MoveLeft | 向左移动 | 左摇杆 | ✅ | 反查 A |
| MoveRight | 向右移动 | 左摇杆 | ✅ | 反查 D |

### 5.2 战斗与角色动作 — 全部已实现并已绑定默认值（仅队伍配置 1 项未绑定，见 §5.5）

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| NormalAttack | 普通攻击 | A | ✅ | |
| ElementalSkill | 元素战技 | RT | ✅ | |
| ElementalBurst | 元素爆发 | Y | ✅ | |
| Jump | 跳跃 | B | ✅ | |
| SprintKeyboard / SprintMouse | 冲刺 | RB | ✅ | 两个动作共用 `Sprint` |
| SwitchAimingMode | 切换瞄准模式 | LT | ✅ | |
| SwitchToWalkOrRun | 切换走/跑 | 无（轻推左摇杆） | ✅ | 方案 A12：保持 `None` 为最终状态 |
| Drop | 落下（滑翔/攀爬） | A | ✅ | 方案 A13：滑翔中 A=下落攻击，攀爬中 A=取消攀爬（与 NormalAttack/Cancel 同键，上下文区分） |
| PickUpOrInteract | 拾取/交互 | X | ✅ | |
| InteractionInSomeMode | 特定玩法交互（T） | LB + X | ✅ | 方案 A14：`X` + 修饰键 `LeftShoulder` |
| QuickUseGadget | 快捷使用小道具 | LB + B | ✅ | 组合键 |
| QuestNavigation | 任务追踪 | LS（左摇杆按下） | ✅ | |
| ElementalSight | 元素视野 | LB + 十字键← | ✅ | 组合键 |
| AbandonChallenge | 中断挑战 | LS + RS | ✅ | 组合键：先按住 RS（视角回正，无害）再按 LS，避免单独 LS 误触发任务追踪（方案 A15） |
| SwitchMember1 | 切换角色 1 | 十字键↑ | ✅ | |
| SwitchMember2 | 切换角色 2 | 十字键→ | ✅ | |
| SwitchMember3 | 切换角色 3 | 十字键← | ✅ | |
| SwitchMember4 | 切换角色 4 | 十字键↓ | ✅ | |
| SwitchMember5 | 切换角色 5 | LB + Y | ✅ | 组合键 |

### 5.3 对话与确认 — 全部已实现

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| Confirm | 确认 | B | ✅ | |
| Cancel | 取消/返回 | A | ✅ | ⚠️ `KeyPress(VK_ESCAPE)` 反查为 A，菜单中语义错位待修（适配方案 B1） |
| SkipDialog | 跳过/推进对话 | B | ✅ | |

### 5.4 界面打开（快捷轮盘 LB + 右摇杆方向）— 全部已实现

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| OpenMap | 打开地图 | LB + 右摇杆↓ | ✅ | |
| OpenInventory | 打开背包 | LB + 右摇杆→ | ✅ | |
| OpenQuestMenu | 打开任务菜单 | LB + 右摇杆↙ | ✅ | |
| OpenCharacterScreen | 打开角色界面 | LB + 右摇杆↘ | ✅ | |
| OpenBattlePassScreen | 打开纪行 | LB + 右摇杆↖ | ✅ | |
| OpenTheEventsMenu | 打开活动面板 | LB + 右摇杆← | ✅ | |
| OpenAdventurerHandbook | 打开冒险之证 | LB + 右摇杆↗ | ✅ | |

### 5.5 界面打开（独立按键 / 已定方案）

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| OpenPaimonMenu | 打开派蒙界面 | Start（☰ Menu） | ✅ | |
| OpenChatScreen | 好友聊天 | Back（⧉ View） | ✅ | |
| OpenPartySetupScreen | 打开队伍配置 | `None` | ⚠️ | 已定方案 A7：派蒙菜单 + 文字识别「队伍配置」+ 左摇杆导航锁定 |
| OpenTheSettingsMenu | 打开玩法系统界面（F6） | — | ❌ | 已定方案 A9：派蒙菜单 + 左摇杆导航锁定齿轮图标（需采集图标资源）；唯一活跃调用点 `EnterAndExitWonderlandJob.cs:23` |
| OpenCoOpScreen | 打开多人游戏界面 | LB + 十字键→ | ✅ | 方案 A16：`Right` + 修饰键 `LeftShoulder` |
| CheckTutorialDetails | 查看教程详情 | LB + 十字键↑ | ✅ | 方案 A17：`Up` + 修饰键 `LeftShoulder` |

### 5.6 UI 导航原语 — 全部已实现

| GIActions | 功能 | 手柄 | 状态 | 备注 |
|---|---|---|---|---|
| FocusUp / FocusDown / FocusLeft / FocusRight | 界面焦点移动 | 左摇杆 | ✅ | `GamepadUiNavigator` |
| ScrollUp / ScrollDown | 列表滚动 | 左摇杆上下 | ✅ | 待确认项 B2，待实机确认 |
| MapZoomIn | 地图放大 | LT | ✅ | |
| MapZoomOut | 地图缩小 | RT | ✅ | |

### 5.7 缺失/无需映射项（8 项缺失 + 1 项无需映射；第 9 项缺失 `OpenTheSettingsMenu` 见 §5.5）

| GIActions | 功能 | 键鼠默认键 | 状态 | 建议 |
|---|---|---|---|---|
| ShortcutWheel | 呼出快捷轮盘 | Tab | ❌ | 可不补：轮盘已由各 `*Wheel` 动作内部实现（按住 LB + 拨右摇杆） |
| OpenWishScreen | 打开祈愿界面 | F3 | ❌ | 按需：游戏有手柄设置项（`Wish=73`） |
| OpenTheFurnishingScreen | 打开摆设界面（尘歌壶） | F7 | ❌ | 按需 |
| OpenStellarReunion | 打开星之归还 | F8 | ❌ | 可不补（条件触发，无任务使用） |
| OpenNotificationDetails | 打开通知详情 | Y | ❌ | 可不补 |
| OpenSpecialEnvironmentInformation | 打开特殊环境说明 | U | ❌ | 可不补 |
| OpenFriendsScreen | 打开好友界面 | O | ❌ | 按需 |
| HideUI | 隐藏主界面 | `/` | ❌ | 可不补（拍照场景） |
| ShowCursor | 呼出鼠标 | Alt | 🚫 | 无需映射：手柄模式无鼠标光标概念，反查表不含 Alt 是正确的 |

### 5.8 非 GIActions 的特殊映射

| 输入 | 手柄 | 状态 | 备注 |
|---|---|---|---|
| 鼠标中键（视角回正） | RS（右摇杆按下，`CameraRecenter=RightThumb`） | ✅ | `GamepadChannel.OnMouseButton` 特判 |
| `MoveMouseBy`（鼠标相对移动） | 右摇杆 30ms 脉冲 | ✅ | 精度有限；比例控制走 `SetRightStick`/`TurnByAngle` |

### 5.9 场景级缺失（GIActions 未建模）

| 场景 | 状态 | 备注 |
|---|---|---|
| 音游按键（`MusicNote11-37`、`MusicLeft*` 等游戏设置项） | ❌ 未建模 | AutoMusicGame 适配时需新增专用动作，绕过键盘反查直走 `GamepadChannel` |
| 钓鱼：抛竿/收杆/提竿 | ⏳ 待确认项 C1 | 待实机确认按键提示 |
| 七圣召唤：选牌/确认/结束回合 | ⏳ 待确认项 C3 | 建议 十字键 + B/A |
| 烹饪：确认/取消 | ⏳ 待确认项 C5 | 建议 B/A |

### 5.10 键盘反查表（ReverseMappableActions）覆盖说明

`GamepadChannel.ReverseMappableActions` 含 40 项：§5.1–§5.5 已实现的键盘动作全部在内（含 WASD 与 2026-11-24 新增的 P/F2/G），因此**键鼠宏/JS 脚本直按这些键可正确映射到手柄**。不在反查表的按键（如 F3/F6/F7/F8、Alt、`/` 等）会 warn 丢弃——与 §5.7 缺失项一一对应。

## 6. 注意事项

- 旧版 `User/Config.json` 中手柄绑定可能仍是 `None`，升级后需在「按键绑定设置 → 手柄」点一次**恢复默认**。
- 手柄模式必须在游戏内和 BGI 内同时开启才会生效。
- 所有绝对坐标点击仍无法直接映射为手柄；菜单类模块需改用焦点导航 + 确认。
