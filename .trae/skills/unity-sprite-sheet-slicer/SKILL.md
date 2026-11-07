---
name: unity-sprite-sheet-slicer
description: "Unity 2022.3 Sprite Sheet 切片。通过编辑 .meta 将 PNG 设为 Multiple Sprite 并定义子图 rect/pivot。Invoke when user asks to slice/split sprite sheet or set sprite pivots."
---

# Unity Sprite Sheet Slicer

在 Unity 2022.3 中把一张 PNG 切成多个子 Sprite，并可统一设置 pivot。**通过直接编辑 `.png.meta` 的 `spriteSheet` 字段完成**，不走 `TextureImporter` C# API（Codedom 编译器不识别 2022.3 的新 API）。

## 适用场景

- 用户说「把这张图分成 N 份」「切 sprite sheet」「设 sprite 的轴点为 (x, y)」
- Unity 版本 = 2022.3（其他版本字段名可能有差异，先确认）

## 工作流

### 1. 确认输入

- 图片路径（用户给的相对路径，如 `Assets/Material/Build/02-Sheet.png`）
- 切分方式：均分（N 份，横/竖）还是手动指定每个 rect
- pivot：统一一个值还是每个不同（格式 `(x, y)`，范围 0-1）

### 2. 读图片尺寸

```powershell
Add-Type -AssemblyName System.Drawing
$img = [System.Drawing.Image]::FromFile("<完整绝对路径>")
Write-Output "$($img.Width)x$($img.Height)"
$img.Dispose()
```

根据尺寸和切分数计算每个 rect 的 `x/y/width/height`。

### 3. 读一份已切片的 meta 作为模板

如果项目里没有任何已切片的 sprite meta，按下面的最小模板写。

### 4. 编辑 .meta

需要改动两处：

**a) `spriteMode` 必须是 2**（Multiple）。如果原值是 1 或 0，改成 2。

**b) 替换 `spriteSheet` 下的 `sprites` 和 `nameFileIdTable`：**

```yaml
  spriteSheet:
    serializedVersion: 2
    sprites:
    - serializedVersion: 2
      name: <图片名>_0
      rect:
        serializedVersion: 2
        x: 0
        y: 0
        width: <w>
        height: <h>
      alignment: 9
      pivot: {x: 0.5, y: 0.1}
      border: {x: 0, y: 0, z: 0, w: 0}
      outline: []
      physicsShape: []
      tessellationDetail: 0
      bones: []
      spriteID: <32位十六进制，每张唯一>
      internalID: <正整数，每张唯一>
      vertices: []
      indices: 
      edges: []
      weights: []
    # ... 重复 N 次
    outline: []
    physicsShape: []
    bones: []
    spriteID: <原始值，不要改>
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable:
      <图片名>_0: <internalID 整数值>
      <图片名>_1: <internalID 整数值>
```

**字段说明：**

| 字段 | 值 | 说明 |
|------|----|------|
| `alignment` | 9 | 表示使用自定义 pivot（Custom），忽略全局 `spritePivot` |
| `pivot` | `{x, y}` | 0-1 浮点数，(0,0) 左下，(0.5,0.5) 中心，(1,1) 右上 |
| `rect.x` / `rect.y` | 整数像素 | Unity sprite rect 原点在**左下角**，y 向上递增 |
| `rect.width` / `rect.height` | 整数像素 | 切片尺寸 |
| `spriteID` | 32 位十六进制 | 每张唯一。用递增十六进制，如 `a1b2c3d4e5f60000...0001` |
| `internalID` | 正整数 | 每张唯一，用递增数如 `100001`, `100002` |

**注意：** 根层级的 `spriteSheet.spriteID` 和 `internalID`（0）不要改，它们属于主纹理。

### 5. 让 Unity 重新导入

```
// 通过 MCP mcp_0 execute_code 执行：
UnityEditor.AssetDatabase.Refresh();
var ti = UnityEditor.AssetImporter.GetAtPath("Assets/xxx.png") as UnityEditor.TextureImporter;
// 读取 ti.spritesheet.Length 和每个 spritesheet[i].rect / .pivot 确认
```

如果 spritesheet 长度为 0，通常是 YAML 缩进或字段名写错了——回退检查模板对比。

## 常见陷阱

- **Codedom 编译器不识别 2022.3 的 TextureImporter 属性**（`spriteMode`、`textureWidth` 等），不要用 C# 脚本写切分逻辑，直接改 meta YAML
- **Unity 坐标原点**：rect.y 从图片**底部**开始算，不是顶部。横向排列的切分 y 都设 0
- **nameFileIdTable 必须同步**：key 是 sprite 名，value 是 internalID 整数。漏了 Unity 会警告
- **spriteID 要 32 位十六进制**，可以用固定前缀 + 递增后缀保证唯一
