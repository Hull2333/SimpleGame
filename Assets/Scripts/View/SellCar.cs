using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MFarm.Inventory;

public class SellCar : MonoBehaviour // Sell car script attached to the Car object in the Farm scene
{
    // Appear (start) position and stop (end) position of the car
    public Transform carStartPos;
    public Transform carEndPos;
    // Uniform move speed (units per second)
    public float moveSpeed = 3f;
    private Animator carAnim;
    // SpriteRenderer of the two child objects: Body (car body) and Cover (back door)
    private SpriteRenderer bodyRenderer;
    private SpriteRenderer coverRenderer;
    // True while the appear / leave routine is playing, used to ignore duplicated triggers
    private bool isRoutineRunning;
    // 光标管理器，用于判断玩家是否空手
    private CursorManager cursorManager;
    // 车主身上的所有碰撞体，用于判断鼠标是否点在车身上
    private Collider2D[] carColliders;

    private void Awake()
    {
        carAnim = GetComponent<Animator>();
        bodyRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
        coverRenderer = transform.Find("Cover").GetComponent<SpriteRenderer>();
        cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
        carColliders = GetComponents<Collider2D>();
    }

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    // Time trigger: the car shows up at 7:01 and leaves at 23:01
    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        if (isRoutineRunning)
        {
            return;
        }
        if (hour == 7 && minute == 1)
        {
            StartCoroutine(AppearRoutine());
        }
        else if (hour == 23 && minute == 1)
        {
            StartCoroutine(LeaveRoutine());
        }
    }

    // Caught up the car state to the current game time when the player arrives in the Farm scene
    private void OnAfterSceneLoadedEvent()
    {
        if (SceneManager.GetActiveScene().name != "Farm" || isRoutineRunning)
        {
            return;
        }
        int currentMinute = TimeManager.Instance.gameHour * 60 + TimeManager.Instance.gameMinute;
        if (currentMinute < 7 * 60 + 1)
        {
            // Before 7:01 the car has not shown up yet, keep it hidden
            SetRenderers(false);
        }
        else if (currentMinute <= 7 * 60 + 5)
        {
            // Arrived after 7:01 but not past 7:05, still play the whole appear routine
            StartCoroutine(AppearRoutine());
        }
        else if (currentMinute <= 23 * 60)
        {
            // Business hours: the car is already parked at the end position
            ParkAt(carEndPos);
            SetRenderers(true);
        }
        else
        {
            // Past 23:00 the car has already left
            ParkAt(carStartPos);
            SetRenderers(false);
        }
    }

    /// <summary>
    /// The car shows up: enable the renderers, shake, drive from the start position to the
    /// end position, wait 1s, then stop shaking and go back to the idle animation.
    /// </summary>
    private IEnumerator AppearRoutine()
    {
        isRoutineRunning = true;
        SetRenderers(true);
        // Start from the appear position with the shake animation playing
        transform.position = carStartPos.position;
        carAnim.SetBool("IsShake", true);
        yield return MoveTo(carEndPos.position);
        // Wait in place for 1s, then stop shaking (the controller returns to Car01_Idle)
        yield return new WaitForSeconds(1f);
        carAnim.SetBool("IsShake", false);
        isRoutineRunning = false;
    }

    /// <summary>
    /// The car leaves: enable the renderers, shake, drive from the end position back to the
    /// start position, wait 1s, then stop shaking and hide the renderers.
    /// </summary>
    private IEnumerator LeaveRoutine()
    {
        isRoutineRunning = true;
        SetRenderers(true);
        carAnim.SetBool("IsShake", true);
        yield return MoveTo(carStartPos.position);
        yield return new WaitForSeconds(1f);
        carAnim.SetBool("IsShake", false);
        SetRenderers(false);
        isRoutineRunning = false;
    }

    // Uniform move towards the target position and snap on arrival
    private IEnumerator MoveTo(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > Settings.pixelSize)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }

    // Show / hide the car body and the back door
    private void SetRenderers(bool visible)
    {
        bodyRenderer.enabled = visible;
        coverRenderer.enabled = visible;
    }

    // Put the car at the given point in the idle animation
    private void ParkAt(Transform target)
    {
        transform.position = target.position;
        carAnim.SetBool("IsShake", false);
    }

    // 玩家处于车的触发范围内时，空手左键点击车身即打开卖车界面与玩家背包
    private void OnTriggerStay2D(Collider2D other)
    {
        // 只响应玩家
        if (!other.CompareTag("Player"))
        {
            return;
        }
        // 未营业（还没出现、已收摊、正在播放出现/收摊流程）时不能点开
        if (!IsCarOpen())
        {
            return;
        }
        // 界面已经打开时不再响应，避免把界面反复开关
        if (InventoryManager.Instance.anyBagOpened)
        {
            return;
        }
        // 只有空手时才能点开，避免与手持工具的左键使用冲突
        if (!IsHandEmpty())
        {
            return;
        }
        // 左键点在车身上时才打开（其余情况仍然交给原有的工具/采集逻辑）
        if (Input.GetMouseButtonDown(0) && IsMouseOnCar())
        {
            InventoryUI.Instance.OpenSellCarUI();
        }
    }

    // 车是否处于营业状态：车身可见且没有在播放出现/收摊流程
    private bool IsCarOpen()
    {
        return bodyRenderer.enabled && !isRoutineRunning;
    }

    // 玩家是否空手（光标没有选中任何物品）
    private bool IsHandEmpty()
    {
        return cursorManager.currentItem == null || cursorManager.currentItem.itemID == 0;
    }

    // 鼠标当前位置是否落在车身的实体碰撞体上（触发器只用于靠近检测，不算车身）
    private bool IsMouseOnCar()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        foreach (Collider2D col in carColliders)
        {
            if (!col.isTrigger && col.OverlapPoint(mouseWorldPos))
            {
                return true;
            }
        }
        return false;
    }
}
