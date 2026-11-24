# 手柄模式鼠标依赖适配方案

> 基于 `feat/xbox-game-input-main` 分支对全部 43 个功能模块的只读代码审计（2026-11）。
> 目标：让所有依赖鼠标点击 UI 的功能在手柄模式下可用。
> 手柄模式总体设计见 [input-hub.md](./input-hub.md)。

---

## 1. 现状机制（已验证）

手柄模式（`GamepadMode.Enabled`）下 `GamepadInputBackend`/`GamepadChannel` 的行为：

| 输入 API | 手柄模式下的行为 | 代码位置 |
|---|---|---|
| `Click`/`ClickTo`/`GameRegionClick`/`GameRegion1080PPosClick`/`LeftButton`/`RightButton`/`VerticalScroll`/`MoveMouseTo`(绝对) | 限频 warn 后**静默丢弃** | `GamepadInputBackend.cs` `OnMouseButton`/`OnMoveTo`/`OnScroll` |
| `MiddleButton` | 映射为 `CameraRecenter`（默认右摇杆按下 RS），用于视角回正 | `GamepadInputBackend.cs` `OnMouseButton` |
| `MoveMouseBy` | 按配置灵敏度换算为右摇杆脉冲（默认 30ms，幅度按 `RightStickSensitivity` 缩放） | `GamepadInputBackend.cs` `OnMoveBy` |
| 键盘 VK | 经 `TryMapKey` 反查 `ReverseMappableActions` → `GIActions` → 手柄按键；未覆盖键丢弃 | `GamepadInputBackend.cs:100-134、437-456` |
| `SimulateAction(GIActions)` | 经 `GamepadActionMap.TryGetButton` 映射；未映射动作 warn 丢弃 | `GamepadActionMap.cs` |

所有 `Region.Click()` 最终走 `DesktopRegion.DesktopRegionClick()` → `InputHub.Foreground.Mouse`（`DesktopRegion.cs:17`），因此**全部模块的 UI 点击在手柄模式下统一失效**。

已适配（本分支完成，不在此列）：AutoSkip 对话（View/Menu 模板）、AutoPick（X 图标）、`BvStatus.IsInTalkUi` 手柄分支、任务启停时虚拟手柄连接/断开、设置页手柄绑定列表。

### 1.1 三个横切缺口

1. **缺口①**：`OpenBattlePassScreen`、`OpenTheEventsMenu`、`QuestNavigation`、`ElementalSight` 原先无配置字段/映射，已补；`AbandonChallenge`（LS+RS）、`OpenCoOpScreen`（LB+十字键→）、`CheckTutorialDetails`（LB+十字键↑）已于 2026-11-24 实测补齐；`OpenPartySetupScreen`（A7）与 `OpenFriendsScreen`（A18）已于 2026-11-24 落地派蒙菜单导航（`PaimonMenuNavigateJob`：Start → OCR 定位菜单项 → 左摇杆导航 → 确认，待验证假设见 gamepad-key-mapping.md §7.3）；`OpenTheSettingsMenu`（A9）导航路径已实现但 F6 语义（玩法系统界面 vs 设置菜单）待实机确认、`EnterAndExitWonderlandJob` 未接线；低频界面动作 5 项（`OpenTheFurnishingScreen` F7/`OpenStellarReunion` F8/`OpenNotificationDetails` Y/`OpenSpecialEnvironmentInformation` U/`HideUI` `/`）已标记**暂不映射**（2026-11-24 决定，低优先级：无内置调用方，仅脚本生态可能使用），有需要再实测补映射。场景判断审计（2026-11-24）：对话/地图/大世界三场景判断已具备（`Bv.IsInTalkUi` 手柄已特判且 **View+Menu 双命中已实机稳定验证 🟢** / `IsInBigMapUi` / `IsInMainUi`，详见 gamepad-key-mapping.md §7）；⚠️ 地图/大世界模板为键鼠时代资产，游戏连手柄后 UI 可能整体切换布局，命中率待实机验证（详见 §7.1 修正备注，复苏排除 `IsInRevivePrompt` 为高风险项），运动状态判断手柄缺口已登记 B6。
2. **缺口②**：鼠标中键（视角回正）原无手柄映射，已通过 `CameraRecenter`（默认 RS）补齐；其余绝对坐标点击仍无法模拟。
3. **缺口③**：点击被静默丢弃后模块卡在重试循环最终抛超时；且游戏连手柄后 UI 为手柄布局，PC 布局模板（如 `BtnWhiteConfirm`）可能不再匹配。
4. **缺口④**（2026-11-24 深挖传送/尘歌壶链路时发现）：`Bv.FindF`/`Bv.FindFAndPress`/`Bv.FindFKeyText` 系列依赖 AutoPick 的**键鼠 F 图标模板**（`PickRo`），手柄模式下 F 提示显示为 X 图标 → 恒返回 false。影响 **8 个模块 16 处调用**：地脉衍出（接触/地脉之花识别）、幽境危战、快捷尘歌壶（进入/离开尘歌壶）、LowerHeadThenWalkTo、凯瑟琳（冒险家协会）、合成台、尘歌壶找阿圆、钓鱼。**已于 2026-11-24 修复**：`FindF`/`FindFKeyText` 增加 `GamepadMode.Enabled` 分支换 `ControllerPick` 模板（X 图标较小，按图标中心扩展出与键鼠等价的 40px 几何，后续文字偏移计算不变，与 `AutoPickTrigger` 同一处理方式）；`FindFAndPress` 的按键走 `PickVk`(F) 反查 `PickUpOrInteract` ✓ 已可用。

### 1.2 已确认的默认绑定问题

- ~~`QuickUseGadget`、`SwitchMember1-5` 默认未绑定~~ **已过期（2026-11-24 更新）**：`QuickUseGadget` 默认 **LB+B**（A14 实测）、`SwitchMember1-4` 默认十字键、`SwitchMember5` 默认 Y——AutoEat（循环）/AutoWood（主循环）/AutoCombo 默认配置下核心链路可用，弹窗/退出环节仍待适配。
- `KeyPress(VK_ESCAPE)` 反查为 `Cancel=A`。按游戏内“系统界面”设置，**B=确认、A=取消/返回**，因此 ESC→A 语义正确，无需改成 B；需要逐菜单实测的是“某些菜单里 A 同时承担确认/取消”的上下文差异。
- 真正同键多义的是 **A = 取消/返回 + 普通攻击 + 跳跃**，对话/战斗/菜单三种上下文下含义不同，需要在模块层按语义调用 `GIActions` 而不是裸按 VK。

---

## 2. 公共基建（先行实施）

| # | 基建项 | 内容 | 受益模块 |
|---|---|---|---|
| B1 | **ESC/返回语义核对** | 游戏内系统键位为 **B=确认、A=取消/返回**，ESC 反查 `Cancel=A` 本身正确；B1 只做逐菜单实测与 `Cancel` 可配置化核对，**不把 ESC 改成 B** | 全部按 ESC 的模块（约 15 处） |
| B2 | **中键→RS 映射** | 新增 `CameraRecenter` 配置（默认 `RightThumb`），`OnMouseButton(Middle)` 按下/抬起映射到该键 | AutoDomain/AutoFight/AutoLeyLine/索敌等约 10 处 |
| B3 | **右摇杆比例控制原语** | `GamepadInputBackend.SetRightStick(x, y, durationMs)` + `TurnByAngle(degrees)`；`OnMoveBy` 改为按 `RightStickSensitivity` 比例换算，不再固定 ×256 饱和 | 相机类全部模块 |
| B4 | **映射与默认绑定补齐** | 补 `OpenBattlePassScreen`、`OpenTheEventsMenu`、`QuestNavigation`、`ElementalSight` 的配置字段与映射；`QuickUseGadget`/`SwitchMember1-5` **不擅自给默认键**（不同玩家布局差异大），改为未绑定时的启动告警与配置页引导 | AutoEat/AutoWood/幽境危战/纪行/莉奈娅 |
| B5 | **两个公共控制器（研究项，非基建）** | ① `GamepadFocusNavigator`：截图→识别焦点高亮→十字键/摇杆步进→复核→A 确认；② `GamepadMapCursor`：大地图光标视觉伺服闭环。两者依赖稳定的焦点/光标视觉特征，需先做可行性验证，未验证前不进入批次 1 | 几乎所有 L 级模块 |
| B6 | **诊断与兜底** | `WarnUnsupported` 增加丢弃计数/严格模式；模块能力声明（Full/Partial/None）+ UI 提示；B服登录等原生 Win32 弹窗保留绕过 InputHub 的真实鼠标旁路 | 适配期排障 + 登录流程 |

**模板资源**：需统一采集一批手柄 UI 模板（大地图、Paimon 菜单、冒险之证、纪行、邮件、队伍配置、合成台、洞天百宝、对话 A/B/X/Y 角标、确认/取消按钮、钓鱼/烹饪提示），按批次 1 先行。
（注意：仓库禁止新增 `BetterGenshinImpact\Assets` 目录，模板需走外部资源包流程。）

### 2.1 基建实现进度（2026-11-24）

| 项 | 状态 | 说明 |
|---|---|---|
| B1 ESC 语义 | ✅ 已核对 | 游戏内 B=确认、A=取消/返回；ESC→A 正确，不改键，仅保留 `Cancel` 可配置 |
| B2 中键→RS | ✅ 已实现 | 新增 `GamepadConfig.CameraRecenter`（默认 `RightThumb`），`OnMouseButton(Middle)` 按下并保持 `PressDurationMs` |
| B3 右摇杆原语 | ✅ 已实现 | 新增 `GamepadInputBackend.SetRightStick(x,y,ms)`、`TurnByAngle(deg)`；`OnMoveBy` 按 `RightStickSensitivity` 比例换算，Y 轴取反修正；新增 `RightStickPulseMs`/`TurnMillisecondsPerDegree` |
| B4 映射补齐 | ✅ 基本完成 | 纪行/活动/任务追踪/元素视野/祈愿（A20）/好友（A18）/队伍配置（A7）字段与映射均已补；`QuickUseGadget`（LB+B，A14）、`SwitchMember1-5`（十字键/Y）已有默认绑定；剩余 5 项低频动作（F7/F8/Y/U/`/`）已标记暂不映射（2026-11-24） |
| B5 焦点/地图控制器 | ⏳ 研究项 | 需先验证焦点高亮/地图光标视觉特征，不进入批次 1 |
| B6 诊断 | ⏳ 部分 | 已有 `WarnUnsupported` 限频告警；丢弃计数/严格模式待做 |

---

## 3. 分批实施计划

总量级：**S×11 / M×16 / L×16**，另有 8 项确认无需投入。

### 批次 1 —— 高频小改（基建 + S/M 级，可立即动手）

| 模块 | 现状 | 改动方案 | 量 |
|---|---|---|---|
| AutoFight | 核心链路可用 | B2 中键修复 + 瞄准改 `SimulateAction(SwitchAimingMode)`(LT，映射已有) + ✅ 宏含 `mousedown/mouseup/click/scroll` 手柄模式限频告警（2026-11-24，`Avatar.cs WarnGamepadMouseUnsupported`） | S-M |
| ReturnMainUiTask | 基本可用 | exitDoor 兜底分支改按键（B1 后自动收益）；`:53` VK_RETURN 无反查（手柄丢弃），建议改 `SimulateAction(Confirm)` | S |
| SkillCdTrigger | **手柄模式不触发**（2026-11-24 审计发现） | `GetAsyncKeyState(VK_1-4)` 只听物理键盘，手柄玩家十字键切人 BGI 感知不到 → 技能 CD 悬浮窗不显示；需监听虚拟手柄十字键输出（ViGEm 回环/XInput 轮询） | M |
| BlessingOfTheWelkinMoonTask | 失效 | 空白双击 → 按 A/B 跳过月卡弹窗 | S |
| AutoSkip 非对话路径 | 部分完成 | ✅ 感叹号（:882）/选项点击（:1121）/底部三角（:1378）已改手柄 B（2026-11-24）；❌ 剩余阻塞待实测（key-mapping §7.5）：邀约跳过按钮（:704）+ 邀约选项（:1267）、提交物品（:1518/:1526/:1538）手柄语义未知 | M |
| AutoPick 滚轮场景 | 低频失效 | `AutoPickTrigger:337` VerticalScroll 丢弃——多物品拾取列表滚动场景；待实测手柄对应操作后补 | S |
| AutoCook | 部分可用 | 确认弹窗"识别+按键"；验证手柄烹饪模板 | S |
| AutoEat（循环模式） | 🟡 核心可用 | `QuickUseGadget` 默认绑定已完成（A14：LB+B）；剩余：指定食物路径手柄下禁用提示 | S |
| AutoOpenChest | 基本可用 | 中键分支改右摇杆脉冲转身重试 | S |
| AutoCombo | 基本可用 | 右摇杆索敌参数调优 + 切人键绑定引导 | S |
| GameLoading | 完全失效 | 进入游戏/适龄/月卡/原石弹窗改按确认键；B服窗口走旁路（B6） | M |
| Macro | 部分失效 | 手柄模式下含鼠标事件的宏给明确警告 | S |
| GetGridIcons | 完全失效 | 开发工具：入口检测手柄模式直接报错退出 | S |

### 批次 2 —— 每日一条龙链路（核心价值）

| 模块 | 现状 | 改动方案 | 量 |
|---|---|---|---|
| ChooseTalkOptionTask | ✅ 已实现（2026-11-24） | 手柄模式选项导航：`NavigateAndConfirmByGamepadAsync`（FocusDown×索引 → B 确认），覆盖 SingleSelectText/SelectLastOptionOnce/SelectLastOptionUntilEnd 三个方法；待实测假设见 key-mapping §7.5（默认焦点第一项/单脉冲移动一项/OCR 索引干净） | M |
| ClaimMailRewardsTask | 失效 | 复用 `PaimonMenuNavigateJob`（Start→OCR→导航→确认，基建 2026-11-24 已就绪）+ 邮件界面内部焦点导航 | M |
| ClaimEncounterPointsRewardsTask | 失效 | 焦点导航 + A | M |
| CheckRewardsTask | 失效 | B4 补映射 + 页签导航 | M |
| ClaimBattlePassRewardsTask | 失效 | B4 补 `OpenBattlePassScreen` 映射 + 焦点导航 + A | M |
| AutoDomain 主流程 | 战斗可用、领奖卡死 | 单人/开始挑战改按 Confirm；领奖弹窗 A/B 模板；`PressUseResin` 按键化（**一处改造，三模块受益**） | M |
| AutoBoss | 领奖卡死 | 领奖 A/B 化；树脂预检改 OCR 顶部数字；补充树脂先禁用 | M |
| AutoLeyLineOutcrop | 领奖/复苏失效 | 复用树脂方案 + 复苏界面按 Confirm + 冒险之证导航 | M-L |
| SwitchPartyTask | 🟡 开界面已通 | 打开队伍配置已走 A7 派蒙菜单导航（2026-11-24，8 个调用点接入）；**队伍界面内部导航（队伍卡片选择/确认）仍为点击、继续失效**：方向键导航 + A + OCR 复核翻页 | L |
| OneKeyExpeditionTask | 失效 | 派遣按钮焦点导航 + A | M |
| GoToAdventurersGuildTask | 依赖项失败 | 随依赖项适配，自身仅回归验证 | S |
| AutoWood | 🟡 主循环可用 | 默认绑定已完成（A14）；剩余：ESC 改 `OpenPaimonMenu`(Start) + 退出流程手柄导航 | M |

### 批次 3 —— 大地图基建攻坚（自动寻路/传送的底层）

> **手柄模式地图 UI 操作语义（2026-11-24 实测）**：左摇杆移动**选中圈光标**（屏幕上可见，可作视觉伺服特征）；视口移动即移动圈；选点 = 圈对准后按 **B**；缩放 = **LT/RT**（方向待确认）；候选列表 = 左摇杆上下 + B；区域列表 = **Y** 打开（默认当前区域，右侧列出全部）→ 左摇杆上下左右选中 → B 跳转该区域默认传送点 → 再 B 传送；传送按钮/选项列表/传送点图标**两模式一致**（识别层可直接复用），仅确认/取消按键提示角标不同。

| 模块 | 现状 | 改动方案 | 量 |
|---|---|---|---|
| GamepadMapCursor 原语（B5） | 不存在 | 选中圈模板匹配定位 + 左摇杆视觉伺服移动圈（实测圈可见，可行性已确认）+ LT/RT 缩放档位闭环 | L |
| TpTask | 完全失效（一切传送的底层） | 按实测语义重构：圈伺服替代拖拽视口与绝对点击（圈对准 + B）；LT/RT 替代滚轮（zoom 档位化闭环重写）；候选列表左摇杆+B；区域切换 Y+左摇杆+B（替代 SwitchArea 点击链） | L |
| QuickTeleport | ✅ 已实现（2026-11-24） | 传送按钮（:113）与候选列表（:169）`ra.Click()` 改手柄 `SimulateAction(Confirm)`（两段式第二段按 B 确认传送，用户实测）；候选列表假设默认焦点第一项，待实测（key-mapping §7.5） | M |
| LowerHeadThenWalkToTask | 退化 | B3 比例控制 + 闭环微调 | M |
| PathExecutor 复苏点击 | 全队死亡不自动复苏 | `PathExecutor:334/:651` `ClickIfInReviveModal` 点击丢弃；改 OCR「复苏」定位 + 按确认（复苏弹窗确认键待实测） | S-M |
| ScanPickTask | 部分退化 | 同上；回正视角需校准方案 | M |
| LinneaMiningTask | 完全失效 | B4 补 `ElementalSight` + B3 视觉伺服瞄准 + 射击键 | L |

### 批次 4 —— 尘歌壶/合成/背包类

| 模块 | 现状 | 改动方案 | 量 |
|---|---|---|---|
| QuickSereniteaPot | 开背包后中断 | 背包 LB/RB 切页签 + 壶图标焦点导航 + A | M |
| GoToSereniteaPotTask | 完全失效 | 依赖批次 3 原语 + 找阿圆视角闭环 + 洞天百宝导航；**出壶方案已实测（2026-11-24）**：① 传送回提瓦特 = 区域切换方式（与地图区域列表 Y+左摇杆+B 同原理）；② 关页面 = A 直接退出，或识别「离开」选项 + B 确认离开；TODO：其他退出方式待补充 | L |
| QuickBuy | 完全失效 | 手柄商店网格十字键导航 + A，数量加减键 | M |
| GoToCraftingBenchTask | 合成后失效 | 数量加减改手柄键 + A 确认 | M |
| CraftMaterialTask | 完全失效 | 手柄合成界面整套：方向键列表导航 + OCR 数量闭环（替代滑块拖拽）+ 模板 | L |
| QuickClaimReward | 失效 | 限定高频界面（纪行/邮件/活动）做焦点导航 | M |
| SetTimeTask | 完全失效 | 手柄调时为数字+方向键模式，OCR 闭环步进重写（丢弃表盘几何） | L |
| SwitchCharacterStateMachineTask | 失效 | 网格行列差开环步进 + 校验（可复用 `FixedSizeGridCardLayout` 几何） | L |
| CountInventoryItem | 失效 | 方向键逐格焦点移动替代滚轮 | L |
| AutoArtifactSalvage | 开背包后失效 | LB/RB 切 tab + 快速选择导航（可先降级为仅快速分解） | M-L |
| AutoStygianOnslaught | 完全失效 | B4 补 `OpenTheEventsMenu` + 列表/Boss/难度焦点导航 | L |
| CharacterDevelopment | 完全失效 | 焦点导航重构，建议先"不支持"提示兜底 | L |
| AutoGeniusInvokation | 完全失效 | "摇杆移焦点→高亮检测→A"循环重构；先做不支持提示兜底 | L |

### 批次 5 —— 专项/低频/低可行性

| 模块 | 现状 | 改动方案 | 量 |
|---|---|---|---|
| AutoFishing | 完全失效 | 钓鱼按键提示识别（抛/提/拉条对应手柄键 hold）+ 换饵界面导航 + 模板重截 | L |
| AutoMusicGame | 完全失效且**危险**（VK_A/S/D 被反查成左摇杆导致**角色移动**！） | 新增专用琴键映射绕过键盘反查 + 专辑导航 | L |
| JS / BvFlow API 层 | `click/moveMouseTo/scroll/drag` 全失效 | 新增手柄语义 API（`pressAction("Confirm")`/`gamepadTurn(angle)`）；Click 类调用一次性警告 + 兼容性矩阵文档 | M |
| ChooseFOptionTask | 无法完成选择 | 实测手柄 F 选项切换方式（上下方向键/左摇杆）后改导航键 + OCR 复核 | M |
| ExitAndReloginJob | 失效 | 菜单导航 + A；登录界面直接 A | M |
| EnterAndExitWonderlandJob | 失效 | 奇域列表焦点导航 + A；低频 | M |
| UseRedeemCode | 失效 | webview 需虚拟光标，可行性最低——**优先评估网页 API 兑换替代** | L |

---

## 4. 分模块明细（含调用点）

### 4.1 传送与日常（审计组①）

#### QuickTeleport（快速传送）— L
- 调用点：`QuickTeleportTrigger.cs:113` 点击"传送"按钮；`:169` 点击传送点选项列表最高项
- 现状：识别有效但点击丢弃，快捷键按下后无反应
- 方案：左摇杆视觉伺服移动地图光标到目标传送点 → A 选择 → A 确认；选项列表改"上方向键选择 + A 确认"；与 TpTask 共用光标闭环原语

#### QuickBuy（快捷购买）— M
- 调用点：`QuickBuyTask.cs:38-51` 尘歌壶拖拽列表+定点点击；`:57` 右下购买/兑换；`:61-75` 弹窗购买
- 方案：洞天百宝手柄网格 + 十字键导航 + A；识别"已售/摩拉币"图标定位当前页；数量用加减键

#### QuickClaimReward（快捷领取奖励）— M
- 调用点：`OneKeyClaimRewardTask.cs:196` 点击领取图标；`:263` 滚轮下滑
- 方案：识别领取图标后焦点导航闭环；通用性差，限定高频界面（纪行/邮件/活动）

#### QuickForge（快捷锻造）— 无需投入
- `QuickForgeTask.cs` 为空壳类（6 行，无实现、全仓库无引用）

#### QuickSereniteaPot（快捷尘歌壶）— M
- 调用点：`QuickSereniteaPotTask.cs:70` 点背包道具页签；`:44` 点壶图标；`:82` 放置确认；`:125` 进入/离开尘歌壶
- 现状：`OpenInventory`/`PickUpOrInteract` 可生效，开背包后中断
- 方案：背包页签 LB/RB；壶图标焦点导航 + A；确认/传送选项识别后按 A

#### UseRedeemCode（兑换码）— L
- 调用点：`UseRedemptionCodeTask.cs:60/65/69` ESC 菜单→设置→账户→前往兑换；`:108-125` webview 内粘贴/确认/清除
- 方案：菜单部分十字键导航；webview 需右摇杆虚拟光标闭环，可行性最低，建议 API 替代

#### Common/Job 逐文件

| 文件 | 调用点 | 现状 | 方案 | 量 |
|---|---|---|---|---|
| ReturnMainUiTask.cs | `:36` exitDoor 点击 | 基本可用（ESC→B 生效） | 删/改兜底分支 | S |
| ChooseTalkOptionTask.cs | `:137` 退出对话；`:241` OCR 对话选项点击 | 不能工作（空格推进可生效） | 手柄对话 A/B/X/Y 角标直选 + 新模板 | M |
| ChooseFOptionTask.cs | `:131` MoveMouseBy 重试；`:144` 滚轮切 F 选项 | 不能完成选择 | 实测手柄切换方式后改导航键 + OCR 复核 | M |
| CheckRewardsTask.cs | `:92`/`:192` 点冒险之证"委托"页签 | 不能工作（F1 生效） | 页签识别 + LB/RB 或方向键切换 | M |
| ClaimBattlePassRewardsTask.cs | `:62/77/96` 定点点击+一键领取 | 不能工作；F4 动作未进 GamepadActionMap | 补映射 + 焦点导航 + A | M |
| ClaimEncounterPointsRewardsTask.cs | `:80` 委托按钮；`:122` 领取按钮 | 不能工作 | 焦点导航 + A | M |
| ClaimMailRewardsTask.cs | `:50` 邮件图标；`:57` 收取按钮 | 不能工作（ESC→B 可开菜单） | 手柄 Paimon 菜单模板 + 焦点导航 | M |
| BlessingOfTheWelkinMoonTask.cs | `:31/37/39` 空白双击跳过月卡 | 不能工作 | 改按 A/B 跳过 | S |
| ExitAndReloginJob.cs | `:41` 退出游戏；`:55` 确认；`:92` 进入游戏 | 不能工作 | 菜单导航 + A；登录界面直接 A | M |
| EnterAndExitWonderlandJob.cs | `:32/72/117/132` 奇域选择/确认/返回 | 不能工作 | 焦点导航 + A | M |
| GoToAdventurersGuildTask.cs | 自身无直接鼠标 | 依赖项失败 | 随依赖项适配，自身仅回归 | S |
| OneKeyExpeditionTask.cs（AutoSkip 目录） | `:28` 全部领取；`:43` 再次派遣 | 不能工作 | 焦点导航 + A | M |
| ExpeditionTask.cs（AutoSkip 目录） | `:44/83/91/95/103/138` | [Obsolete]（4.3 废弃） | 不建议投入 | — |
| GoToCraftingBenchTask.cs | 自身无直接鼠标，依赖 Bv.Click 系列 | 进入合成对话后失败 | 数量加减改手柄键 + A | M |
| CraftMaterialTask.cs | `:355-356` 筛选；`:372-382` 列表复位拖拽+滚轮；`:424` 材料点击；`:488/507` 加成角色；`:542-675` 滑块拖拽；`:568/575` 加减；`:691-697` 提交确认 | 完全不能工作 | 手柄合成界面整套重写：方向键导航 + OCR 数量闭环 | L |
| CountInventoryItem.cs | `:239-247` 滚动条长按拖拽；间接 GridScroller 滚轮 | 不能翻页 | 方向键逐格焦点移动 + 定期校验 | L |
| SwitchPartyTask.cs | `:126` 队伍按钮；`:155-159` 长按滚动；`:191/219/223` 队伍点击；`:241/253` 确认 | 不能工作（T 开界面生效） | 手柄队伍界面方向键导航 + A + OCR 复核翻页 | L |
| SwitchCharacterStateMachineTask.cs | `:1516-1524/1543` 排序；`:1596` 角色卡片；`:1657` OCR 点击 | 不能工作 | 网格行列差 → 方向键开环步进 + A | L |
| GoToSereniteaPotTask.cs | `:132/145/172` 地图图标与传送；`:271-340` 中键回正+MoveMouseBy 找阿圆；`:415-609` 商店购买/关闭 | 完全不能工作 | 依赖 TpTask 光标闭环 + 右摇杆比例 API + QuickBuy 导航 | L |
| LowerHeadThenWalkToTask.cs | `:55/88/108/144` MoveMouseBy 视角控制 | 退化为粗转，追踪失败 | B3 比例持续控制 + 闭环 | M |
| ScanPickTask.cs | `:67` 扫圈；`:171-173` 中键回正+低头 | 部分退化 | 同上；回正需校准方案 | M |
| LinneaMiningTask.cs | `:101-201` 中键长按元素视野；`:125-210` MoveMouseBy 瞄准；`:236` 左键射箭 | 完全不能工作 | B4 补 `ElementalSight`；瞄准 B3 视觉伺服 | L |
| SetTimeTask.cs | `:54/58-71/95-127` 菜单时间图标、表盘拖拽、空白点击 | 完全不能工作 | 手柄调时为数字+方向键，OCR 闭环步进重写 | L |
| WalkToFTask.cs | 无鼠标依赖 | **手柄模式可用** | 无需改动 | 0 |

#### Common/Map（大地图服务）— 无需投入
全目录无鼠标依赖（grep 零匹配）。`MapManager`、`BigMapTeyvat256Layer`（SIFT 匹配 `GetBigMapRect`）、MiniMap/CameraOrientation 均为纯计算；其输出喂给 TpTask 的鼠标流程，TpTask 适配后可继续复用做闭环反馈。

#### TpTask（传送任务，AutoTrackPath/TpTask.cs）— L
- 调用点：`:859/867` 传送点绝对点击；`:973` 回退点击；`:1299` ClickMapPoint；`:1634-1641` 地图拖拽；`:1717` 滚轮缩放（含每格校准）；`:1955-1975` 左键拖图；`:2428/2444` 区域切换；`:2573/2599` 地表/地下图层；`:3486` 候选列表点击
- 特例：`:2717-2721` `PressTeleportConfirmKey` 按 F 确认（手柄下 F→PickUpOrInteract 可发出，但手柄大地图确认键是 A，需实测等效性）
- 现状：完全不能工作；是所有自动路径/一条龙传送的底层依赖
- 方案：左摇杆光标视觉伺服 + RT/LT 或方向键缩放档位 + A 选择/A 传送；区域/图层切换识别手柄按钮后导航；Common/Map 结果做闭环反馈

### 4.2 战斗与副本（审计组②）

#### AutoDomain（自动秘境）— 主流程 M / 培养指南 L
- 调用点：`AutoDomainTask.cs:647-681` 周日选奖励（MoveMouseTo+滚轮+点击）；`:707` 单人挑战；`:749` 开始挑战；`:795/808` 地脉异常提示；`:1144` 中键回正；`:1380/1391` 锁东方向（→右摇杆，可用）；`:1422` 石化古树；`:1587-1628` 领奖退出/继续/取消；`:1703-1708` ExitDomain；`:1741-1788` `PressUseResin` + 20/40 切换
- TrainingGuide 三个 partial：`TrainingGuide.cs:177/186/281/408-413/451/593/604`、`TrainingGuide.Navigation.cs:40`、`TrainingGuide.LowAR.cs:25/33/202/218/223` 冒险之证导航
- 现状：战斗循环可用（WASD 反查→左摇杆），进本确认→领奖→用树脂→退出全链路失效
- 方案：单人/开始挑战改按 Confirm(A)；领奖弹窗识别手柄 A/B 提示模板；`PressUseResin` 定位树脂行后按 Confirm（**public static，被 AutoStygianOnslaught/AutoLeyLineOutcrop 复用，一处改造三处受益**）；培养指南按手柄布局冒险之证重写导航层

#### AutoBoss（自动周本/Boss）— M
- 调用点：`AutoBossTask.cs:305` 大地图点树脂图标；`:542/568/578` 补充树脂面板；`:729` 数量"+"；`:776/791` 弹窗关闭；`:1246-1264` 对准征讨之花（→右摇杆，可用）；`:1414` "使用原粹树脂"点击；`:1481/1485` 结果页关闭
- 方案：领奖识别手柄 A/B 提示按 Confirm/Cancel；树脂预检改 OCR 顶部数字（参照 AutoLeyLine `CountOriginalResin`）；补充树脂手柄模式先禁用

#### AutoStygianOnslaught（幽境危战）— L
- 调用点：`AutoStygianOnslaughtTask.cs:337` `OpenTheEventsMenu`（**无手柄映射，第一道阻断**）；`:356-368` 活动列表拖动；`:375/413/431/462` 条目/挑战/传送/确认；`:554-717` 继续/退出；`:901-928` 难度切换；`:962` Boss 点击；`:1115-1177` 预设队伍拖动；`:1200` 退出秘境
- 方案：① 补 `OpenTheEventsMenu` 手柄路径；② 列表/Boss/难度改"摇杆移动焦点 + 焦点高亮检测 + A 确认"（需新增模板）；③ 继续/退出按 A/B

#### AutoLeyLineOutcrop（地脉衍出）— M-L
- 调用点：`AutoLeyLineOutcropTask.cs:1391` 中键回正；`:1491/1495` 对准地脉花（→右摇杆，可用）；`:1557` 点"复苏"；`:1837-1900` 激活弹窗/用树脂/20-40 切换；`:2085-2223` 冒险之证导航/追踪/补充树脂
- 方案：复用 AutoDomain 树脂 A/B 方案；复苏界面按 Confirm；冒险之证手柄导航；中键→RS

#### AutoGeniusInvokation（七圣召唤）— L
- 调用点：`GeniusInvokationControl.cs:352/361/501-504/519/605-617/657-672/699/716-719/735/744/851-857` 全部固定/识别坐标点击与拖拽；`Model/Character.cs:80-111` 角色卡点击；`Model/Duel.cs:238`
- 现状：100% 依赖绝对坐标点击，完全不可用
- 方案：TCG 手柄布局支持完整摇杆导航，把坐标点击重构为"摇杆移焦点→识别焦点高亮→A 确认"循环；或降级为启动即提示不支持

#### AutoFight（自动战斗）— S-M ⭐
- **可用部分**（SimulateAction/反查）：`Avatar.cs:528-541` Attack、`:546-598` UseSkill、`:875-890` Dash、`:892-925` Walk、`:932-935` MoveCamera（→右摇杆）、`:375-398` 切人、`:1145` Jump、`:1161-1163` Charge；`AvatarSpecialActions.cs:183/228/290/368` 特化重击旋转（→右摇杆，可用需调优）；`AvatarRecognition.cs:461/493/516` 持续索敌
- **失效部分**：`Avatar.cs:1200-1215` Click（三键全丢弃）；`:1225-1228` Scroll；`:186/208` 游泳脱困 RightButton（瞄准无实现）；`AvatarRecognition.cs:538`、`AutoFightSeek.cs:304/342` 中键回正；`CombatCommand.cs:224-307` 宏 `mousedown/mouseup/click/scroll` 丢弃、`Aim` 未实现
- 方案：① B2 中键→RS；② 瞄准改 `SimulateAction(GIActions.SwitchAimingMode)`（LT，`GamepadActionMap.cs:48-50` 已有映射）；③ 策略文本含 click/scroll 宏时启动 warn

#### AutoArtifactSalvage（圣遗物分解）— M-L
- 调用点：`AutoArtifactSalvageTask.cs:117/199` 背包 tab 点击；`:236` 分解按钮；`:254/279` 快速选择/星级；`:295-315` 确认链；`:335-457` 筛选与格子选/反选；`GridScroller.cs:50/63/82` 滚轮
- 方案：LB/RB 切 tab（新增 GIAction）+ 十字键/A 导航 + 格子遍历改摇杆光标推算；可先降级为仅快速分解主路径

#### CharacterDevelopment（角色养成）— L
- 调用点：`CharacterDevelopmentTask.cs:552/564/871` 模板点击；`:613/644/660/708/728` TryClickText；`:795` 天赋 ClickTo；`CharacterSelectionHelper.cs:189/266` 点击 + GridScroller
- 方案：手柄布局 LB/RB 切分类 + 摇杆导航 + A 确认；需新增焦点高亮模板；建议暂缓

#### GetGridIcons（背包格子图标）— S（禁用）
- 调用点：`GetGridIconsTask.cs:94/140/176`；`GridIconsAccuracyTestTask.cs:125`
- 属开发工具非用户功能；方案：Start 入口检测手柄模式直接报错退出

### 4.3 生活与脚本（审计组③）

#### AutoFishing（自动钓鱼）— L
- 调用点：`Behaviours.cs:201` 右键开换饵；`:224/239` 选饵/确认；`:472/755/860/1047` 举竿/抛竿/收杆/提竿左键；**:1241-1315 拉条左键按住/松开（核心玩法）**；`:511-1429` 瞄准 MoveMouseBy；`Behaviours.PartII.cs:158/189` 转圈、`:254/344` 确认按钮；`AutoFishingTask.cs:273` SetTimeTask 昼夜切换
- 方案：识别手柄钓鱼 UI 按键提示（抛/提/拉条=RT hold）；GamepadChannel 增加右扳机按住原子接口；换饵改手柄菜单导航；重截手柄布局模板

#### AutoCook（自动烹饪）— S
- 调用点：`AutoCookTask.cs:74` `Bv.ClickWhiteConfirmButton`（丢弃）；`:89` `KeyPress(VK_SPACE)` → Confirm=B（可用，需实测烹饪确认键）；`:66/160` 模板需验证手柄布局

#### AutoEat（自动吃药）— 循环 S / 指定食物 L
- 调用点：`AutoEatTask.cs:172`、`AutoEatTrigger.cs:87/102` `QuickUseGadget`（默认 None 丢弃）；`:102/124` 食物格子/确认点击
- 方案：循环模式补 `QuickUseGadget` 默认绑定；指定食物模式建议手柄下禁用

#### AutoWood（自动伐木）— M
- 调用点：`AutoWoodTask.cs:447/454/475` `QuickUseGadget` 未绑定；`:488/506` ESC（反查 Cancel=A，打不开派蒙菜单，需 Start）；`:518/528/552` 退出/确认/进入游戏；`:417` WonderlandJob；`Login3rdParty.cs:114/129` B服窗口
- 方案：`QuickUseGadget` 默认绑定；PressEsc 改 `OpenPaimonMenu`；退出流程手柄菜单导航；B服窗口保留键鼠旁路

#### AutoOpenChest（自动开宝箱）— S
- 调用点：`AutoOpenChestTask.cs:67` 中键重置视角（丢弃，可能卡循环）；`:73` 转身 MoveMouseBy（右摇杆脉冲，粗可用）
- 方案：中键分支改右摇杆俯仰脉冲或转身重试

#### AutoMusicGame（自动音游）— L（⚠️ 危险）
- 调用点：`AutoMusicGameTask.cs:33-41` VK_A/S/D/J/K/L —— **A/S/D 被反查成左摇杆（角色移动！），J/K/L 丢弃**；`AutoAlbumTask.cs:104/118/135/130/141/158/190` 专辑流程全点击
- 方案：新增专用琴键映射（绕过键盘反查，直接走 GamepadChannel 原子接口）；专辑导航改手柄菜单导航

#### AutoCombo（自动连招）— S
- 调用点：`Behaviours.cs:295` NormalAttack→A（可用）；其余委托 AutoFight Avatar；`AutoComboRunTask.cs:108-121` 索敌右摇杆粗瞄准
- 注意：切人依赖 `SwitchMember1-5` 绑定（默认 None）

#### Macro（宏）— 拦截 M / 全量 L
- 调用点：`TurnAroundMacro.cs:15` MoveMouseBy（右摇杆单脉冲，语义变化）；`QuickEnhanceArtifactMacro.cs:25-37` 五连点全丢弃；`KeyMouseMacroPlayer.cs:89/122/149/157`（鼠标事件丢弃，键盘反查可用，`:177` MoveMouseBy 脉冲）
- 方案：先做拦截提示（手柄模式下含鼠标事件的宏给出明确警告）；`QuickEnhanceArtifact` 按手柄圣遗物 UI 重做

#### AutoSkip 非对话路径 — M
- 已适配：`AutoSkipTrigger.cs:357-445`（对话选项 ControllerChoiceButton + Confirm）、`:547-552` AdvanceDialogue
- 剩余调用点：`:603/607` 黑屏剧情点击；`:700/704` 邀约跳过；`:790-791` 原石奖励；`:1263/1267`→`HangoutOption.cs:70/75` 邀约选项；`:1333` ClosePopupPage（ESC 可发键，PageClose 模板需验证）；`:1374/1378` 底部三角；`:1482/1486` 角色弹窗；`:1518-1538` SubmitGoods 三连点；`OneKeyExpeditionTask.cs:28/43`；`AutoTrackTask.cs:94/118/120/143/181/215/236/276`（`QuestNavigation` 未映射）
- 方案：黑屏/弹窗/角色弹窗改按确认类按键（各 S，需实测手柄 UI 哪个键关闭）；邀约改"识别手柄选择 UI→摇杆方向+确认"（M）；AutoTrackTask 建议禁用或重写（M-L）

#### GameLoading（登录/加载）— M
- 调用点：`GameLoading.cs:103/148/158/175/181/192/201-213` 适龄提示/顶号弹窗/进入游戏/月卡/原石/B服协议
- 方案：EnterGame 识别后按确认键；各弹窗改按确认/取消；B服登录窗口为原生 Win32 弹窗，需保留绕过 InputHub 的真实鼠标旁路

#### JS API（GlobalMethod.cs）与 BvFlow — M
- 调用点：`GlobalMethod.cs:36-49` keyDown/keyUp/keyPress 反查可用；`:89-95` moveMouseBy 脉冲；`:97-165` moveMouseTo/click/leftButton*/rightButton*/middleButton*/verticalScroll 丢弃
- `BvFlow.cs:89-97` KeyPress 可用；`:99-163` Click/Drag 系列丢弃（`BvFlowServices.Create:537-565` 全走鼠标）；`BvPage.cs:149-152`；`BvLocator.cs:114-131`；`BvFlowAction.cs:57-119`；JS 注册 `EngineExtend.cs:153-173/102`
- 现状：依赖 click/moveMouseTo/滚轮/拖拽的 JS 脚本手柄模式失效；keyPress+moveMouseBy 脚本可部分运行
- 方案：新增手柄语义 JS API（`pressAction("Confirm")`/`gamepadTurn(angle)`）；BvFlow 增加 PressConfirm 类动作；输出兼容性矩阵文档

#### 无鼠标依赖（无需改动）
FarmingPlan（纯数据/统计）、MapMask（仅遮罩绘制）、SkillCd（仅识别+绘制）

---

## 5. 无需投入模块汇总

| 模块 | 原因 |
|---|---|
| Common/Map 全目录 | 纯计算零鼠标依赖，TpTask 适配后复用其输出 |
| WalkToFTask | 全走 GIActions，手柄模式已可用 |
| QuickForge | 空壳类，无实现无引用 |
| ExpeditionTask（AutoSkip 目录） | [Obsolete]（4.3 废弃） |
| FarmingPlan / MapMask / SkillCd | 无输入调用 |
| AvatarGridIconRecognizer / ItemIconRecognizer / ItemMetadataCache 等识别器/数据类 | 纯识别/数据/参数类 |

---

## 6. 实测确认清单（适配前必做）

1. **ESC→A 语义错位**（B1）：菜单中按 A 是"确认"而非"返回"，逐点实测各弹窗
2. **手柄 UI 模板兼容性**：连手柄后 PC 布局模板（`BtnWhiteConfirm` 等）是否还能匹配
3. **TpTask 的 F 传送确认**在手柄大地图中与交互键（默认 X）的等效性
4. **F/X 多选项交互**的手柄切换方式（ChooseFOptionTask 前置）
5. **A 同时是取消、普通攻击、跳跃**（默认配置）在对话/战斗/菜单上下文的实际表现；B 为确认
6. **钓鱼/烹饪/音游**手柄布局的实际按键提示
7. **`MoveMouseBy` 大距离转身**（如 0,7000）的右摇杆换算标定

---

## 7. 路线图

> **批次 1（基建+小改）→ 批次 2（一条龙链路）→ 批次 3（大地图攻坚）→ 批次 4/5（长尾）**
>
> 批次 1+2 完成后，"每日一条龙 + 副本领奖"即可在纯手柄模式跑通，覆盖绝大多数日常使用场景；批次 3 是解锁自动寻路传送的关键一战。

---

## 8. 键鼠绕过底层切换的现状审计（2026-11-24）

> 结论：只要调用走 `InputHub`/`SimulateAction`，底层切换是统一的；问题集中在“绕过 InputHub 的直连”和“InputHub 已收到但手柄后端无法表达的绝对坐标操作”。

### 8.1 完全绕过 InputHub（真正的底层缺口）

| 位置 | 说明 | 建议 |
|---|---|---|
| `Service/Instance/MessageHandlers/RelativeMouseMessageHandler.cs:91` | 子会话鼠标增量直接 `Simulation.SendInput.Mouse.MoveMouseBy` | 手柄模式下应改为右摇杆或明确禁用子会话鼠标 |
| `Core/Simulator/MouseEventSimulator.cs` | 直接 `user32.mouse_event` 绝对移动/按键；目前仅 `Simulation.MouseEvent` 属性暴露、业务未调用 | 保持弃用，禁止新增调用 |
| `Core/Monitor/LocalCursorCapture.cs` | `SetCursorPos` 用于本地光标捕获，不是模拟输入 | 保持现状 |
| JS 脚本 API `GlobalMethod`（`moveMouseTo`/`leftClick`/`rightClick`/`scroll` 等） | 脚本层直接调用鼠标/键盘接口，无法自动转成手柄语义 | 手柄模式启动脚本时告警；后续提供动作化 API |
| `KeyMouseMacroPlayer` | 已走 `InputHub`，但键鼠宏中的鼠标事件在手柄后端会被丢弃 | 回放手柄模式前检测宏是否含鼠标事件并提示 |

### 8.2 经 InputHub，但手柄后端不支持（会被 warn 丢弃）

| 类别 | 规模 | 典型位置 |
|---|---|---|
| 绝对坐标点击（`Region.Click`/`ClickTo`/`GameRegionClick`） | **238 处** | `DesktopRegion.cs`、`ClickExtension.cs`；`AutoLeyLineOutcropTask`(18)、`AutoStygianOnslaughtTask`(15)、`AutoArtifactSalvageTask`(13)、`AutoDomainTask`(13)、`GoToSereniteaPotTask`(13)、`AutoSkipTrigger`(9)、`SwitchCharacterStateMachineTask`(9)、`TpTask`(9) 等 |
| 绝对移动 `MoveMouseTo` | 8 处 | `DesktopRegion`(4)、`ClickExtension`(4)，最终服务上述点击/宏 |
| 鼠标拖拽 `LeftButtonDown/Up` | 3 处 | `BvFlow.cs` 的 `Drag` |
| 滚轮 `VerticalScroll` | ~20 处 | `AutoDomain` 训练手册、`AutoPick` 多选项、`TpTask` 地图缩放、`CraftMaterialTask`、`GridScroller`、`OneKeyClaimRewardTask`、`AutoFight/Avatar`、`AutoLeyLineOutcropTask`、`ChooseFOptionTask`、`GetGridIconsTask` |
| 未覆盖的裸 VK | `VK_F6`（打开设置菜单）、`VK_RETURN`（聊天）、`VK_V`（任务追踪，已补反查但默认未绑定） | `EnterAndExitWonderlandJob.cs:23`、`ReturnMainUiTask.cs:53`、`GlobalMethod.cs:216` |

### 8.3 已由底层统一覆盖

| 能力 | 说明 |
|---|---|
| `SimulateAction(GIActions)` | 全部经 `InputHub` 的模块自动切换；映射见 `GamepadActionMap` |
| 裸 VK 反查 | `ReverseMappableActions` 覆盖 WASD、Space、ESC、F、Ctrl、X、V 等常用键；未覆盖键 warn |
| `MoveMouseBy` | 按 `RightStickSensitivity` 换算右摇杆，替代原来的固定 ×256 饱和 |
| 鼠标中键 | 映射 `CameraRecenter`（默认 RS） |
| 手柄对话/拾取 UI | AutoSkip 的 View/Menu 模板、AutoPick 的 X 图标 |

### 8.4 结论

- 真正的“底层切换”已经集中在 `InputHub` + `GamepadChannel`；剩下的 238 处点击、20 处滚轮、拖拽和绝对移动**无法用单一底层映射解决**（手柄没有绝对坐标点击能力），必须按模块替换为“焦点导航/动作键/摇杆”语义；
- 优先处理批次 1/2 的公共链路：`BvFlow.Drag`、`GridScroller`、`CraftMaterialTask`、`TpTask` 地图缩放、`ChooseFOptionTask` 滚轮选人、`AutoPick` 多选项；
- `VK_F6`/`VK_RETURN` 需要新增 `OpenTheSettingsMenu`/`OpenChatScreen` 配置字段，或明确不支持并告警。