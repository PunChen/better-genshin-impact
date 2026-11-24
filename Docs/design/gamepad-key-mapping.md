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
| 移动角色 | 左摇杆 | `GIActions.Move*` | — |
| 转动视角 | 右摇杆 | `MoveMouseBy` | — |

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
| 视角回正（鼠标中键） | R3 | `CameraRecenter=RightThumb` |

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

## 4. 待确认项

| 编号 | 项目 | 当前建议 |
|---|---|---|
| A7 | 打开队伍配置 | LB + 右摇杆某方向？否则留空 |
| A9 | 打开设置菜单（F6） | 无 |
| A12 | 切换走/跑 | 无（轻推左摇杆） |
| A13 | 落下（滑翔/攀爬中） | B |
| A14 | 特定玩法交互（T） | X |
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

## 5. 注意事项

- 旧版 `User/Config.json` 中手柄绑定可能仍是 `None`，升级后需在「按键绑定设置 → 手柄」点一次**恢复默认**。
- 手柄模式必须在游戏内和 BGI 内同时开启才会生效。
- 所有绝对坐标点击仍无法直接映射为手柄；菜单类模块需改用焦点导航 + 确认。
