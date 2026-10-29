using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 根据水面 Tilemap 生成“离岸距离图”，供水面 shader 做“离岸越远、分三层逐级加深”的效果。
/// 距离图只在 Awake 时生成一次，水面是静态的，所以没有每帧开销。
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class WaterDepthMap : MonoBehaviour
{
    [Header("分层加深")]
    [Tooltip("第一层：离岸边多少格开始加深")]
    public int firstLayerCells = 2;
    [Tooltip("第二层：在第一层边缘之外多少格再次加深")]
    public int secondLayerCells = 3;
    [Tooltip("第三层：在第二层边缘之外多少格再次加深（最多三层）")]
    public int thirdLayerCells = 3;

    // 3-4 倒角距离的权重，用来近似欧氏距离
    const int OrthogonalWeight = 3;
    const int DiagonalWeight = 4;
    const int Unreachable = 1 << 20;

    Material runtimeMaterial;
    Texture2D depthTexture;

    void Awake()
    {
        Bake();
    }

    // 运行时创建的材质/贴图不属于场景资源，切换场景时要手动释放，避免累积
    void OnDestroy()
    {
        if (runtimeMaterial != null) Destroy(runtimeMaterial);
        if (depthTexture != null) Destroy(depthTexture);
    }

    // 生成离岸距离图并写入水面材质
    void Bake()
    {
        var tilemap = GetComponent<Tilemap>();
        var tilemapRenderer = GetComponent<TilemapRenderer>();
        if (tilemap == null || tilemapRenderer == null || tilemapRenderer.sharedMaterial == null) return;

        BoundsInt bounds = tilemap.cellBounds;
        int width = bounds.size.x;
        int height = bounds.size.y;
        if (width <= 0 || height <= 0) return;

        int[,] dist = BuildDistanceField(tilemap, bounds, width, height);

        // 距离图：R 通道存 0(岸边) ~ 1(最深处)
        if (depthTexture != null) Destroy(depthTexture);
        depthTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
        {
            name = "WaterDepthMap",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        // 三层加深的累积距离阈值（单位：格），第二层从第一层边缘开始算
        int threshold1 = Mathf.Max(0, firstLayerCells);
        int threshold2 = threshold1 + Mathf.Max(0, secondLayerCells);
        int threshold3 = threshold2 + Mathf.Max(0, thirdLayerCells);

        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float cells = dist[x, y] / (float)OrthogonalWeight;   // 换算成“格”
                // 按距离分三层，每过一层阈值就深 1/3；shader 里再乘 _DepthStrength
                float depth = 0f;
                if (cells >= threshold1) depth = 1f / 3f;
                if (cells >= threshold2) depth = 2f / 3f;
                if (cells >= threshold3) depth = 1f;
                pixels[y * width + x] = new Color(depth, depth, depth, 1f);
            }
        }
        depthTexture.SetPixels(pixels);
        depthTexture.Apply(false, false);

        // 用材质实例，避免运行时改动写回共享的材质资源
        if (runtimeMaterial != null) Destroy(runtimeMaterial);
        runtimeMaterial = new Material(tilemapRenderer.sharedMaterial);
        tilemapRenderer.sharedMaterial = runtimeMaterial;

        Vector3 worldMin = tilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        Vector3 worldMax = tilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));
        float sizeX = Mathf.Max(0.0001f, worldMax.x - worldMin.x);
        float sizeY = Mathf.Max(0.0001f, worldMax.y - worldMin.y);

        // 用纹理的 Tiling/Offset 把世界坐标映射到距离图 UV，shader 里直接 TRANSFORM_TEX
        runtimeMaterial.SetTexture("_DepthMap", depthTexture);
        runtimeMaterial.SetTextureScale("_DepthMap", new Vector2(1f / sizeX, 1f / sizeY));
        runtimeMaterial.SetTextureOffset("_DepthMap", new Vector2(-worldMin.x / sizeX, -worldMin.y / sizeY));
    }

    // 3-4 倒角距离变换：非水格为 0，水格算出到最近非水格的近似欧氏距离
    static int[,] BuildDistanceField(Tilemap tilemap, BoundsInt bounds, int width, int height)
    {
        var dist = new int[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                dist[x, y] = tilemap.HasTile(cell) ? Unreachable : 0;
            }
        }

        // 正向传播
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (dist[x, y] == 0) continue;
                int d = dist[x, y];
                if (x > 0) d = Mathf.Min(d, dist[x - 1, y] + OrthogonalWeight);
                if (y > 0)
                {
                    d = Mathf.Min(d, dist[x, y - 1] + OrthogonalWeight);
                    if (x > 0) d = Mathf.Min(d, dist[x - 1, y - 1] + DiagonalWeight);
                    if (x < width - 1) d = Mathf.Min(d, dist[x + 1, y - 1] + DiagonalWeight);
                }
                dist[x, y] = d;
            }
        }

        // 反向传播
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = width - 1; x >= 0; x--)
            {
                if (dist[x, y] == 0) continue;
                int d = dist[x, y];
                if (x < width - 1) d = Mathf.Min(d, dist[x + 1, y] + OrthogonalWeight);
                if (y < height - 1)
                {
                    d = Mathf.Min(d, dist[x, y + 1] + OrthogonalWeight);
                    if (x < width - 1) d = Mathf.Min(d, dist[x + 1, y + 1] + DiagonalWeight);
                    if (x > 0) d = Mathf.Min(d, dist[x - 1, y + 1] + DiagonalWeight);
                }
                dist[x, y] = d;
            }
        }

        return dist;
    }
}
