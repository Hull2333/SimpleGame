---
name: hull-maingame-skill
description: 在 D:\SimpleGame\SimpleGame 这个 Unity 2022.3 的 2D 像素 TopDown 种田项目里实现新功能或还原参考效果。用户说 帮我实现XXX功能、如图 帮我实现图片中的效果、或给出截图和 GIF 时使用。不用于其他 Unity 项目。
---

# Hull MainGame Skill

在**已有工程**里加功能，不新建工程、不换架构。根目录 `D:\SimpleGame\SimpleGame`。

先读 [references/project-map.md](references/project-map.md) 查目录、系统、事件、数据表、场景清单。

## 工作流（每次走完）

1. **读输入**
   - 文字需求：拆成「谁触发 → 状态怎么变 → 表现是什么 → 要不要存档」四问，缺哪问就问用户。
   - 截图 / GIF：用 Read 打开，逐项拆解层级（世界层还是 UI 层）、像素尺寸、动画帧数与节奏、交互时序、数值变化。产出条目清单后再写代码。
   - 现有资源（Sprite、预制体）优先到 `Assets/` 里找，不要凭空生成美术资源。
2. **定位（必做，不许跳过）**
   - 用 Grep 找同类功能，至少读 1 个已完成的相似实现，确认能复用的管理器、事件、SO 字段、预制体。
   - 优先给现有 SO 加字段、给现有事件加参数，而不是新建一套系统。
3. **先问后写（硬性门槛）**：定位完成后，**必须先向用户提问并得到答复**，才能动代码。
   - 找到相似实现 → 问「是否套用现有的 `X` 模板（管理器 / 事件 / 预制体）来做」。
   - 没找到相似实现 → 问「是否可以新建脚本 `Y` 来实现，落在哪个系统目录」。
   - 同一需求有两条以上实现路径（例如改 Tilemap 状态 vs 新增独立管理器）时，一并列出取舍再问。
   - 禁止在不告知用户的情况下额外"造轮子"。
4. **实现**：遵守下面的架构铁律。
5. **验证**：跑静态检查清单，并把用户必须在 Editor 里做的事写清楚（挂脚本、拖资源、填 SO、Play 验收）。

## 架构铁律

- **命名空间**：新代码用 `MFarm.<系统>`（Map / Inventory / CropPlant / Save / Dialogue / Transition / AStar / Task / Cooked）。Player、UI、View、Items、Menu 等历史文件在全局命名空间，写这些目录时跟随文件现状，不擅自补命名空间。
- **管理器**：继承 `Singleton<T>`，`Awake` 里调 `base.Awake()`；跨场景常驻对象才用单例。
- **通信**：只走 `Utillities/EventHandler.cs`。新增 `public static event Action<...> XxxEvent` 与 `CallXxxEvent` **成对**；订阅写在 `OnEnable`、退订写在 `OnDisable`，必须逐条对称。
- **存档**：需要持久的系统实现 `MFarm.Save.ISaveable`：`GUID => GetComponent<DataGUID>().guid`、`Start` 里 `RegisterSaveable()`、实现 `GenerateSaveData()` / `RestoreData()`。新增持久字段同时改 `GameSaveData`。
- **数据**：数值与内容进 ScriptableObject（`[CreateAssetMenu]` 命名 `<系统>/<名称>`，放 `Assets/Scripts/*/Data`，资源放 `Assets/GameData/<系统>`）；代码不硬编码物品 ID；全局常量放 `Utillities/Settings.cs`。
- **地图/瓦片**：地块状态统一读写 `GridMapManager.Instance.tileDetailsDict`，key 为 `"{x-0.5}X{y-0.5}Y{场景名}"`（按现有写法），不要自建坐标字典。
- **时间**：跨天逻辑挂 `EventHandler.GameDayEvent`，分钟级挂 `GameMinuteEvent`，季节变化通过 `Season` 参数；不要在 `Update` 里自己计时。
- **交互**：鼠标行为走 `CursorManager` → `MouseClickedEvent` / `MouseHoldEvent` / `ExecuteActionAfterAnimation`（动画播完才结算）。新交互先读 `CursorManager.currentItem` / `currentTile`。
- **表现**：UI 与数值动效用 DOTween；粒子走 `EventHandler.ParticleEffectEvent` + `ParticalEffectType`；音效走 `PlaySoundEvent` / `InitSoundEffect`，音名进 `SoundName` 枚举。
- **输入**：老 Input Manager（`UnityEngine.Input`），不用新 Input System。
- **像素**：1 像素 = `Settings.pixelSize` = 0.05 世界单位；新增 Sprite 保持与现有资源相同 PPU；不改 Sorting Layer 顺序。
- **注释**：新增代码一律中文注释；逻辑不直观的地方在代码上方说明作用。

## 改含中文的 `.cs` 时必须字节安全写入

`Assets/` 下的 `.cs` **不是** UTF-8：绝大多数是 GBK（无 BOM、CRLF），个别文件是纯 LF（如 `Player/PlayerController.cs`）。直接对这类文件使用文本编辑工具，会把 GBK 中文写成 UTF-8 字节，注释整段变成 `?`。改动前先逐文件探测，按探测结果写回。

**探测**：读全部字节 → 用 `New-Object Text.UTF8Encoding($false,$true)` 严格解码，成功即 UTF-8，抛异常即 GBK；再统计 `\r` 与 `\n` 数量判断行尾（CR 数为 0 即纯 LF）。

**写入**（`SKILL.md`、`*.uxml`、`*.md` 是 UTF-8，可直接编辑，以下只针对 GBK 源码）：

1. 读写脚本存成 `.ps1` 放在项目外（如 `%TEMP%`），**脚本本身必须是纯 ASCII** —— PowerShell 5 会把无 BOM 的 UTF-8 脚本按 ANSI 解析，脚本里出现中文会让锚点匹配失败。
2. 要插入的中文先单独写进一个 UTF-8 临时文件，脚本里用 `[IO.File]::ReadAllText($p, $utf8)` 显式读取，并 `TrimStart([char]0xFEFF)` 去掉 BOM。
3. 源码读写：`$gbk = [Text.Encoding]::GetEncoding(936)`，`$t = $gbk.GetString([IO.File]::ReadAllBytes($f))` → `$t.Replace(锚点, 锚点 + "`r`n" + $中文)` → `[IO.File]::WriteAllBytes($f, $gbk.GetBytes($t))`。
4. 只替换 ASCII 代码行时可用字节透传 `[Text.Encoding]::GetEncoding(28591)`（PowerShell 5 没有 `[Text.Encoding]::Latin1`），避免解码再编码整份文件。
5. **最大坑**：Latin1 透传流程里把 UTF-8 读出的中文直接拼进去，会原样写成 UTF-8 字节 → GBK 解码后变成 `?`。中文必须先用 `$gbk.GetBytes($中文)` 转成 GBK 字节，再转 Latin1 字符串拼接。
6. 插入行的行尾要和文件其余部分一致；混入 CRLF 或 LF 会产生整文件行尾噪声（写回后用 CR / LF 计数复核）。
7. PowerShell 正则匹配 `?` 必须转义成 `\?`，否则报 `Quantifier {x,y} following nothing`。

**交付前校验**：花括号数量配对、`[\u4e00-\u9fff]` CJK 计数合理、`\?` 计数（注意三元运算符的 `?` 是正常的）、按原编码抽查新增行能否正确显示、`git diff --numstat` 只有新增没有删改、临时脚本与临时文本文件全部清理。

## 常见需求落点

| 需求 | 落点 |
|---|---|
| 新物品 | `Utillities/DataCollection.cs` 的 `ItemDetails` + `ItemDataList_SO` 资源 + `Logic/ItemManager.cs` / `Logic/InventoryManager.cs` |
| 新作物 | `Crop/Data/CropDetails.cs` + `CropDetailsList_SO` 资源 + `Crop/Logic/CropManager.cs` |
| 新可敲击 / 可收割物 | `Crop/Logic/KnockableItem.cs` / `ReapableItem.cs` + `GridType` 枚举 + `GridMapManager` |
| 新地块类型 | `Utillities/Enums.cs` 的 `GridType` + `Map/Data/MapData_SO.cs` + `Map/Logic/GridMapManager.cs` |
| 新场景 / 新地图 | `MapData_SO` 资源 + `Transition/TransitionManager.cs` + `SceneName` 特性；场景放 `Assets/Scenes` |
| NPC 行为 / 日程 | `NPC/Logic/NPCMovement.cs` + `ScheduleDataList_SO` + `AStar/` |
| 新 UI | 脚本放 `Assets/Scripts/UI`，预制体放 `Assets/Prefabs/UI`，由事件驱动；`Menu/UIManager.cs` 只管暂停 / 睡觉 / 结算面板，不是全局 UI 注册表 |
| 新任务 / 委托 | `Quest/` 与 `Task/` + `QuestData_OS` / `TaskData_SO` |
| 新菜谱 | `Cooked/` + `CookedData_SO` + `CookedMenu` 枚举 |
| 新技能等级 | `Player/PlayerController.cs` 的经验字段 + `Increase*SkillEvent` |

## 禁止

- 不新建平行工程结构、不引入新框架或第三方包、不动 `Packages/`、`ProjectSettings/`、`Assets/Plugins/`（除非用户明确要求）。
- 不重构没坏的系统，不"顺手优化"相邻代码。发现无关死代码只报告，不删除。
- 未经用户确认不新建脚本或新系统；「有相似实现就套模板、没有就新建」这两种情况都要先问。
- 运行时代码禁止 `using UnityEditor`（`Player/PlayerController.cs` 现有该问题，属历史遗留，只报告）。
- 不手写 `.meta`，不手改 `.prefab` / `.unity` 的 YAML（需要改场景或预制体时，给出操作步骤让用户在 Editor 里做）。

## 验证清单（交付前逐项确认）

- [ ] 引用的类型、命名空间、字段可见性正确；无 `UnityEditor` 引用
- [ ] 新事件成对定义并在 `OnEnable` / `OnDisable` 对称订阅退订
- [ ] 新增可持久状态已写入 `GenerateSaveData()` 与 `RestoreData()`
- [ ] 新增 public / `[SerializeField]` 字段命名能让用户在 Inspector 里认出来，SO 新字段有合理默认值
- [ ] 明确列出用户需要手动完成的事项，并用一句话说明怎么验收（我无法启动 Unity Editor 编译）
