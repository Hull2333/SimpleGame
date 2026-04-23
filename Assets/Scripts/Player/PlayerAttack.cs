using System;
using System.Collections;
using UnityEngine;
public class PlayerAttack : MonoBehaviour   //调用在Tool对象上
{
    public PlayerController playerController;
    //玩家攻击力
    public int playerDamage;
    [Header("近战攻击")]
    //攻击框大小
    public Vector2 attackBoxSize;
    public Transform attackBoxCenter;
    private Vector2 attackBoxCenterOffset;
    public float attackCirD;
    //x、y轴偏移量
    private float offsetX;
    private float offsetY;
    //敌人图层
    public LayerMask enemyLayer;
    //收割场景图层
    public LayerMask reapableLayer;
    public CursorManager cursorManager;
    //防御碰撞框
    public BoxCollider2D[] defenceBoxs;
    /// <summary>
    /// 攻击框设置和激活，在攻击动画帧中调用
    /// </summary>
    /// <param name="attackClip"></param>
    private void MeleeAttackAnimEvent(float attackClip)
    {

        //调整向下攻击框的偏移量
        if (attackClip == 1)
        {
            attackBoxSize = new Vector2(2.5f, 1f);
            offsetX = 0f;
            offsetY = 0.2f;
            attackBoxCenterOffset = new Vector2(attackBoxCenter.position.x + offsetX, attackBoxCenter.position.y + offsetY);
        }
        //左攻击框
        if (attackClip == 2)
        {
            attackBoxSize = new Vector2(1f, 2.5f);
            offsetX = -1f;
            offsetY = 1.5f;
            attackBoxCenterOffset = new Vector2(attackBoxCenter.position.x + offsetX, attackBoxCenter.position.y + offsetY);
        }
        //右攻击框
        if (attackClip == 3)
        {
            attackBoxSize = new Vector2(1f, 2.5f);
            offsetX = 1f;
            offsetY = 1.5f;
            attackBoxCenterOffset = new Vector2(attackBoxCenter.position.x + offsetX, attackBoxCenter.position.y + offsetY);
        }
        //上攻击框
        if (attackClip == 4)
        {
            attackBoxSize = new Vector2(2.5f, 1f);
            offsetX = 0f;
            offsetY = 2.5f;
            attackBoxCenterOffset = new Vector2(attackBoxCenter.position.x + offsetX, attackBoxCenter.position.y + offsetY);
        }
        //剑处决攻击框
        if (attackClip == 5)
        {
            attackBoxSize = new Vector2(5f,5f);
            attackBoxCenterOffset = new Vector2(attackBoxCenter.position.x , attackBoxCenter.position.y);
        }
        //激活攻击框并开始检测互动图层
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackBoxCenterOffset, attackBoxSize, 0, enemyLayer);
        Collider2D[] reapColliders = Physics2D.OverlapBoxAll(attackBoxCenterOffset, attackBoxSize, 0, reapableLayer);
        switch (cursorManager.currentItem.itemType)
        {

            case ItemType.Sword:
                //攻击框激活时开始遍历触碰到的所有碰撞体
                foreach (Collider2D hitCollider in hitColliders)
                {

                    if (hitCollider.GetComponent<FSM>() != null)
                    {
                        //处决伤害
                        if(attackClip == 5)
                        {
                            hitCollider.GetComponent<FSM>().EnemyHurted(cursorManager.currentItem.maxValue * 2, cursorManager.currentItem.forceValue * 2f);
                        }
                        else
                        {
                            hitCollider.GetComponent<FSM>().EnemyHurted(UnityEngine.Random.Range(cursorManager.currentItem.minValue, cursorManager.currentItem.maxValue), cursorManager.currentItem.forceValue);
                        }
                        
                    }

                }
                break;
            //收割
            case ItemType.ReapTool:
                foreach (Collider2D reapCollider in reapColliders)
                {
                    if (reapCollider.GetComponent<BoxCollider2D>())
                    {
                        EventHandler.CallPlayerDecreaseStminaEvent(1);
                        //reapCollider.GetComponent<ItemInterActive>().PlayBreakAnim();
                        //reapCollider.GetComponent<ReapItem>().SpawnHarvestItems();
                        reapCollider.GetComponent<ReapableItem>().SpawnItems();

                    }
                }
                break;




        }


    }
    /// <summary>
    /// 回到Idle动画，调用在防御和处决动画帧
    /// </summary>
    public void BackToIdle(int index)
    {

        StartCoroutine(QuitDefenceBox(index));

    }
    /// <summary>
    ///  防御完后恢复站立且关闭对应的防御碰撞框
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private IEnumerator QuitDefenceBox(int index)
    {
        //等待该帧结束后执行
        yield return new WaitForEndOfFrame();
        defenceBoxs[index].enabled = false;
        playerController.playerCollider.enabled = true;
        playerController.inputDisable = false;
        foreach (var anim in playerController.animators)
        {
            anim.SetTrigger("toIdle");
        }
    }
    /// <summary>
    /// 开启对应方向的防御碰撞框,调用在武器防御动画上
    /// </summary>
    /// <param name="index"></param>
    public void TakeUpDefenceBox(int index)
    {
        playerController.playerCollider.enabled = false;
        defenceBoxs[index].enabled = true;
    }
    /// <summary>
    /// 在编辑器中绘制攻击框
    /// </summary>
    private void OnDrawGizmos()
    {
        //attackBoxCenter = transform.position;
        //攻击框边框颜色
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(attackBoxCenterOffset, attackBoxSize);
        //Gizmos.DrawSphere(attackBoxCenter, attackCirD);
    }
}


