using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 陷阱触发区（如蛛网）：玩家进入后让隐藏的陷阱型敌人从天上落下到指定位置。
/// 只触发一次；下落期间敌人无敌且不运行状态机，落地后才开始正常 AI。
/// enemies 数组与 landingPoints 数组按索引一一对应：enemies[i] 落到 landingPoints[i]。
/// 每个敌人可在 FSM.Parameter.appearPreviewPrefab 上配置一个"预警效果预制体"
/// （影子、光圈等），触发时立即显示并与下落同步进行。
/// 下落期间可在蜘蛛出生锚点和蜘蛛之间画一条丝线（LineRenderer），纯视觉，落地时消失。
/// </summary>
public class TrapPlace : MonoBehaviour
{
    [Header("陷阱敌人根物体列表（场景里先设为隐藏）")]
    public GameObject[] enemies;
    [Header("落点列表（与 enemies 按索引一一对应）")]
    public Transform[] landingPoints;
    [Header("预警效果设置")]
    //预警效果预制体（影子/光圈）已配在每个敌人的 FSM.Parameter.appearPreviewPrefab 上；
    //TrapPlace 触发时立即在落点实例化预警，预警与敌人下落同步进行，落地时销毁预警
    [Header("下落设置")]
    //从落点正上方多高处开始下落（TopDown 相机建议 3-5，太高会超出视野）
    public float dropHeight = 5f;
    //下落耗时（秒）
    public float dropDuration = 0.5f;
    [Header("吊丝设置")]
    //是否启用吊丝（蜘蛛从天上掉下来时身上牵着的那条丝）
    public bool useSpiderLine = true;
    //丝线厚度
    public float lineWidth = 0.02f;
    //丝线颜色（默认白色半透明，像蛛丝）
    public Color lineColor = new Color(1f, 1f, 1f, 0.7f);
    //丝线在 Sorting Layer 上的 Order in Layer，设成 -1 确保在蜘蛛精灵背后
    public int lineSortOrder = -1;
    //落地后丝线缩回锚点的耗时（秒），缩完才销毁
    public float retractDuration = 0.3f;
    [Header("减速设置")]
    //玩家踩在陷阱上时的减速比例（0.4 = 速度减少 40%）
    public float slowRate = 0.4f;

    private bool hasTriggered;

    private void Start()
    {
        //保险：即使忘记在 Editor 里隐藏敌人，也先隐藏
        if (enemies != null)
        {
            foreach (GameObject e in enemies)
            {
                if (e != null && e.activeSelf)
                {
                    e.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Editor / 调试用：强制触发一次陷阱，不管玩家在哪。
    /// </summary>
    public void ForceDrop()
    {
        hasTriggered = true;
        StartCoroutine(DropAllEnemies());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        //玩家踩在陷阱上时减速（与是否已经触发过出场无关）
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            player.SetSpeedMultiplier(1f - slowRate);
        }

        if (hasTriggered)
        {
            return;
        }
        if (enemies == null || enemies.Length == 0 || landingPoints == null || landingPoints.Length != enemies.Length)
        {
            return;
        }
        hasTriggered = true;
        StartCoroutine(DropAllEnemies());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        //离开陷阱恢复原本速度
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            player.SetSpeedMultiplier(1f);
        }
    }

    /// <summary>
    /// 完整出场流程：实例化预警 → 创建吊丝（纯视觉） → 蜘蛛同步下落（吊丝每帧跟着更新） → 落地销毁预警+吊丝+启动 AI
    /// </summary>
    private IEnumerator DropAllEnemies()
    {
        FSM[] fsms = new FSM[enemies.Length];
        //记录每个敌人对应的预警效果实例，落地时统一销毁
        List<GameObject> previewInstances = new List<GameObject>();
        //记录每个敌人对应的吊丝 LineRenderer，key = enemies 数组索引
        Dictionary<int, LineRenderer> spiderLines = new Dictionary<int, LineRenderer>();

        //=== 第一阶段：准备预警 + 激活蜘蛛 + 创建吊丝 ===
        Vector3[] froms = new Vector3[enemies.Length];
        Vector3[] tos = new Vector3[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            if (enemy == null || landingPoints[i] == null) continue;

            FSM fsm = enemy.GetComponentInChildren<FSM>(true);
            if (fsm == null) continue;
            fsms[i] = fsm;

            GameObject prefab = fsm.parameter.appearPreviewPrefab;
            if (prefab != null)
            {
                GameObject preview = Instantiate(prefab, landingPoints[i].position, Quaternion.identity);
                previewInstances.Add(preview);
            }

            fsm.enabled = false;
            enemy.SetActive(true);

            Collider2D col = fsm.GetComponent<Collider2D>();
            Rigidbody2D rb = fsm.GetComponent<Rigidbody2D>();
            col.enabled = false;
            rb.simulated = false;

            Vector3 target = landingPoints[i].position;
            tos[i] = target;
            froms[i] = target + Vector3.up * dropHeight;
            enemy.transform.position = froms[i];
            fsm.parameter.animator.SetBool("isAppear", true);

            if (useSpiderLine)
            {
                LineRenderer lr = CreateSpiderLine(froms[i], fsms[i]);
                spiderLines[i] = lr;
            }
        }

        //=== 第二阶段：所有敌人同步下落，吊丝点 2 每帧跟着蜘蛛位置更新 ===
        float timer = 0f;
        while (timer < dropDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / dropDuration);
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null) continue;
                enemies[i].transform.position = Vector3.Lerp(froms[i], tos[i], t);
                if (spiderLines.TryGetValue(i, out LineRenderer lr))
                {
                    lr.SetPosition(1, enemies[i].transform.position);
                }
            }
            yield return null;
        }

        //=== 第三阶段：落地，销毁预警，恢复碰撞+物理但**先不启用 FSM**，让蜘蛛原地等丝缩完 ===
        foreach (GameObject p in previewInstances)
        {
            if (p != null) Destroy(p);
        }
        previewInstances.Clear();

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            if (enemy == null || fsms[i] == null) continue;
            enemy.transform.position = tos[i];
            Collider2D col = fsms[i].GetComponent<Collider2D>();
            Rigidbody2D rb = fsms[i].GetComponent<Rigidbody2D>();
            col.enabled = true;
            rb.simulated = true;
            fsms[i].parameter.parentPos = landingPoints[i];
            fsms[i].parameter.animator.SetBool("isAppear", false);
            //注意：fsms[i].enabled = true 先不放这里，等丝缩完再启用
        }

        //=== 第四阶段：丝线缩回锚点（丝尾从蜘蛛位置 Lerp 回出生点），缩完再销毁 ===
        if (useSpiderLine && spiderLines.Count > 0 && retractDuration > 0f)
        {
            float retractTimer = 0f;
            while (retractTimer < retractDuration)
            {
                retractTimer += Time.deltaTime;
                float t = Mathf.Clamp01(retractTimer / retractDuration);
                foreach (var kv in spiderLines)
                {
                    int idx = kv.Key;
                    LineRenderer lr = kv.Value;
                    if (lr == null) continue;
                    //丝尾从蜘蛛刚落地位置 Lerp 回锚点（出生位置 froms[idx]）
                    Vector2 tip = Vector2.Lerp(tos[idx], froms[idx], t);
                    lr.SetPosition(1, tip);
                }
                yield return null;
            }
        }
        foreach (LineRenderer lr in spiderLines.Values)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        spiderLines.Clear();

        //=== 第五阶段：丝缩完了，统一启用所有蜘蛛的 FSM，开始正常 AI ===
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null || fsms[i] == null) continue;
            fsms[i].enabled = true;
        }
        //触发区保持开启：出场只触发一次由 hasTriggered 控制，碰撞体还要负责减速区域的进出检测
    }

    /// <summary>
    /// 创建一条吊丝：锚点固定在 anchorPos，点 2 初始也在 anchorPos（蜘蛛出生位置），
    /// 点 2 在外部协程里每帧更新跟着蜘蛛走。
    /// 自动继承蜘蛛的 Sorting Layer，sortingOrder 设为蜘蛛 Sprite - 1 确保在蜘蛛背后。
    /// 视觉上做上粗下细的渐变（像真实蛛丝）。
    /// </summary>
    private LineRenderer CreateSpiderLine(Vector3 anchorPos, FSM fsm)
    {
        GameObject lineObj = new GameObject("SpiderLine");

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = false;
        lr.positionCount = 2;
        lr.SetPosition(0, anchorPos);
        lr.SetPosition(1, anchorPos);

        //宽度曲线：上方粗一点（1.7x），下方收细（0.5x），像真的蛛丝
        lr.widthCurve = new AnimationCurve(
            new Keyframe(0f, lineWidth * 1.7f),
            new Keyframe(1f, lineWidth * 0.5f));
        lr.widthMultiplier = 1f;

        //颜色整体半透明白色
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        //关键：Sorting Layer 和 Order 对齐蜘蛛 SpriteRenderer
        var sr = fsm.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            lr.sortingLayerID = sr.sortingLayerID;
            lr.sortingOrder = sr.sortingOrder + lineSortOrder; // lineSortOrder 默认 -1 = 在蜘蛛背后
        }
        else
        {
            lr.sortingOrder = lineSortOrder;
        }

        //材质：用 Unity 自带的默认 Sprite-Default 材质，保证 2D 像素项目里渲染正确
        lr.material = new Material(Shader.Find("Sprites/Default"));

        return lr;
    }
}
