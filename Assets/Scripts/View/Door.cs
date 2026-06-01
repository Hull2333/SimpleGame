using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour //调用在场景中一般时屋子内的Door对象上
{
    private CursorManager cursorManager;
    public Animator anim;
    private bool isOpened = false;
    private bool canOpened = true;
    private void Start()
    {
        cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.Mouse1) && cursorManager.canPat && canOpened)
            {
                canOpened = false;
                isOpened = !isOpened;
                anim.SetBool("isOpened", isOpened);
                StartCoroutine(WaitForDoorAnim());
            }
        }
    }
    private IEnumerator WaitForDoorAnim()
    {
        yield return new WaitForSeconds(0.4f);
        canOpened = true;
    }
}
