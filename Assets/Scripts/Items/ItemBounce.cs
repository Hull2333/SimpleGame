using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
namespace MFarm.Inventory
{
    public class ItemBounce : MonoBehaviour //调用在BounceItemBase对象上
    {
        private Transform spriteTrans;
        public CircleCollider2D coll;
        //判断扔出的距离
        private float distance;
        //扔出的方向
        private Vector2 direction;
        //到达的位置
        private Vector3 targetPos;
        private float inSkyTime;
        //item抛出的最大力度、最小力度
        private float maxForce = 5f;
        private float minForce = 4f;
        private Vector2 originalPos;
        //随机方向点
        private Vector2 randomPos;
        private Rigidbody2D rigidbody2D => transform.GetComponent<Rigidbody2D>();
        // ========== 新增：二次弹跳参数 ==========
        private int secondaryBounceCount = 2;
        private float secondaryDecay = 0.4f;       // 力度衰减系数
        private float secondaryInterval = 0.18f;   // 弹跳间隔
        private bool isSecondaryBouncing = false;
        private bool hasLanded = false;

        private void Start()
        {
            //spriteTrans = transform.GetChild(0);
            //扔出物品时关闭物品碰撞体，避免物体刚扔出来就被玩家捡回        
            GiveItemForce();
        }
        private void Update()
        {

            inSkyTime -= Time.deltaTime;
            if(inSkyTime <= 0 && !hasLanded)
            {
                coll.enabled = true;
                hasLanded = true;
                //不再立刻停止速度，改为启动弹跳协程
                StartCoroutine(SecondaryBounce());
            }

        }
        public void OnEnable()
        {
            EventHandler.ItemFirstPos += OnItemFirstPos;
        }
        public void OnDisable()
        {
            EventHandler.ItemFirstPos -= OnItemFirstPos;
        }

        private void OnItemFirstPos(Vector3 pos)
        {
            originalPos = pos;
        }

        /// <summary>
        /// 生成扔出的物品
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dir"></param>
        public void InitBounceItem(Vector3 target,Vector2 dir)
        {
            coll.enabled = false;
            direction = dir;
            targetPos = target;
            distance = Vector3.Distance(target, transform.position);
            spriteTrans.position += Vector3.up * 1.5f;
        }
    
        /// <summary>
        /// 给刚出现的物体施加随机力和方向
        /// </summary>
        private void GiveItemForce()
        {
            inSkyTime = 0.5f;
            randomPos = new Vector2(originalPos.x + Random.Range(-0.3f, 0.3f), originalPos.y + Random.Range(0.1f, 1f));
            direction = (randomPos - originalPos).normalized;
            //施加方向和随机的力
            rigidbody2D.AddForce(direction * Random.Range(minForce, maxForce), ForceMode2D.Impulse);

        }
        /// <summary>
        /// 二次弹跳协程，施加衰减的随机力
        /// </summary>
        /// <returns></returns>
        private IEnumerator SecondaryBounce()
        {
            isSecondaryBouncing = true;
            for (int i = 0; i < secondaryBounceCount; i++)
            {
                yield return new WaitForSeconds(secondaryInterval);
                // 衰减力度
                float decay = Mathf.Pow(secondaryDecay, i + 1);
                float currentMin = minForce * decay;
                float currentMax = maxForce * decay;
                // 停止当前速度，施加新力
                rigidbody2D.velocity = Vector2.zero;
                rigidbody2D.AddForce(direction * Random.Range(currentMin, currentMax), ForceMode2D.Impulse);
                yield return new WaitForSeconds(0.09f - 0.03f * i);
                rigidbody2D.velocity = Vector2.zero;
            }
            // 弹跳结束，最终停止
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.gravityScale = 0;
            isSecondaryBouncing = false;
        }
    }

}
