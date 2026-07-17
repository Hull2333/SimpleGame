using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour //调用在场景中一般时屋子内的Door对象上
{
    private CursorManager cursorManager;
    public Animator anim;
    public bool isOpened = false;
    private bool canOpened = true;
    //关门计时器
    private float closeDelay = 3f;
    //检测玩家是否在门附件
    public bool playerInRange;
    private void Start()
    {
        cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (Input.GetKey(KeyCode.Mouse1) && cursorManager.canPat && canOpened)
            {
                OpenDoor();
            }
        }
    }
    /// <summary>
    /// 玩家离开门的检测范围
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
    public void OpenDoor()
    {
        canOpened = false;
        isOpened = !isOpened;
        anim.SetBool("isOpened", isOpened);
        StartCoroutine(WaitForDoorAnim());
        // 门被打开时，启动自动关闭计时器
        if (isOpened)
            StartCoroutine(AutoCloseDoor());
    }
    private IEnumerator WaitForDoorAnim()
    {
        yield return new WaitForSeconds(0.4f);
        canOpened = true;

    }
    /// <summary>
    /// 自动关门
    /// </summary>
    /// <returns></returns>
    private IEnumerator AutoCloseDoor()
    {
        yield return new WaitForSeconds(closeDelay);
        // 确认门还没被手动关闭
        if (isOpened && !playerInRange)
        {
            isOpened = false;
            anim.SetBool("isOpened", false);
        }
    }
}
