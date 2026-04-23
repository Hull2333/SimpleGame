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
        
        private void Start()
        {
            //spriteTrans = transform.GetChild(0);
            //扔出物品时关闭物品碰撞体，避免物体刚扔出来就被玩家捡回        
            GiveItemForce();
        }
        private void Update()
        {

            inSkyTime -= Time.deltaTime;
            if(inSkyTime <= 0)
            {
                rigidbody2D.gravityScale = 0;
                rigidbody2D.velocity = Vector2.zero;
                coll.enabled = true;
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
            Vector2 randomDir = (randomPos - originalPos).normalized;
            //施加方向和随机的力
            rigidbody2D.AddForce(randomDir * Random.Range(minForce, maxForce), ForceMode2D.Impulse);

        }
    }

}
