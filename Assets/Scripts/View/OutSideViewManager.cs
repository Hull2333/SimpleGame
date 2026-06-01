using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutSideViewManager : MonoBehaviour //调用在OutSideViewManager对象上
{
    public GameObject cloud;
    public GameObject butterfly01;
    public GameObject[] birds;
    public AnimatorOverrideController[] butterAnims;
    private float cloudMoveSpeed = 1f;
    private float butterflyMoveSpeed = 2f;
    public Sprite[] cloudSprite;
    //场景云出现间隔
    private float cloudAppearTimer = 10f;
    private float butterflyAppearTimer = 5f;
    private float currentCloudAppearTime;
    private float currentButterAppearTime;
    //是否开始计时云出现间隔
    private bool startCloudTime = true;
    private bool startButterTime = true;
    private bool cloudMove;
    private bool butterMove;
    private GameObject[] cloudAppearPos;
    private GameObject[] cloudDisappearPos;
    private GameObject[] butterflyAppearPos;
    private GameObject[] butterflyDisappearPos;
    private Transform currentCloudDisappearPos;
    private Transform currentButterDisappearPos;
    public Transform birdLeftDownPos;
    public Transform birdRightUpPos;
    private bool canSetOutside;
    public void FixedUpdate()
    {
        if(SceneManager.GetActiveScene().name == "Farm" || SceneManager.GetActiveScene().name == "WestPath" || SceneManager.GetActiveScene().name == "Town")
        {
            if (cloudAppearPos != null && cloudDisappearPos != null && butterflyAppearPos != null && butterflyDisappearPos != null && canSetOutside)
            {
                if (startCloudTime)
                {
                    currentCloudAppearTime += Time.fixedDeltaTime;
                    if (currentCloudAppearTime >= cloudAppearTimer)
                    {
                        CloudAppear();
                        currentCloudAppearTime = 0;
                        startCloudTime = false;
                    }
                }
                if (cloudMove)
                {
                    CloudMove();
                }
                if (startButterTime)
                {
                    currentButterAppearTime += Time.fixedDeltaTime;
                    if (currentButterAppearTime >= butterflyAppearTimer)
                    {
                        ButterAppear();
                        currentButterAppearTime = 0;
                        startButterTime = false;
                    }
                }
                if (butterMove)
                {
                    ButterMove();
                }
            }
        }
    }
    public void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
       
    }
    public void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    private void BeforeSceneUnloadEvent()
    {
        if (SceneManager.GetActiveScene().name == "Farm" || SceneManager.GetActiveScene().name == "WestPath" || SceneManager.GetActiveScene().name == "Town")
        {
            canSetOutside = false;
        }
    }

    private void OnAfterSceneLoadedEvent()
    {
        if (SceneManager.GetActiveScene().name == "Farm" || SceneManager.GetActiveScene().name == "WestPath" || SceneManager.GetActiveScene().name == "Town")
        {
            cloudAppearPos = GameObject.FindGameObjectsWithTag("CloudAppear");
            cloudDisappearPos = GameObject.FindGameObjectsWithTag("CloudDisappear");
            butterflyAppearPos = GameObject.FindGameObjectsWithTag("ButterflyAppear");
            butterflyDisappearPos = GameObject.FindGameObjectsWithTag("ButterflyDisappear");
            birdLeftDownPos = GameObject.FindGameObjectWithTag("BirdLeftDownPos").transform;
            birdRightUpPos = GameObject.FindGameObjectWithTag("BirdRightUpPos").transform;
            canSetOutside = true;
            //初始化场景云各项参数
            cloud.GetComponent<SpriteRenderer>().enabled = false;
            cloudMove = false;
            currentCloudAppearTime = 0;
            startCloudTime = true;
            //初始化场景蝴蝶各项参数
            butterfly01.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            butterfly01.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;
            butterMove = false;
            currentButterAppearTime = 0;
            startButterTime = true;
        }
        
    }
    /// <summary>
    /// 场景云出现
    /// </summary>
    public void CloudAppear()
    {
        var randomNum = Random.Range(0, cloudSprite.Length);
        cloud.GetComponent<SpriteRenderer>().sprite = cloudSprite[randomNum];
        cloud.GetComponent<SpriteRenderer>().enabled = true;
        cloud.transform.position = cloudAppearPos[Random.Range(0,cloudAppearPos.Length)].transform.position;
        currentCloudDisappearPos = cloudDisappearPos[Random.Range(0, cloudDisappearPos.Length)].transform;
        cloudMove = true;
    }
    /// <summary>
    /// 场景云移动
    /// </summary>
    public void CloudMove()
    {
        cloud.transform.position = Vector2.MoveTowards(cloud.transform.position, currentCloudDisappearPos.position, cloudMoveSpeed * Time.fixedDeltaTime);
        var distance = Vector2.Distance(cloud.transform.position, currentCloudDisappearPos.position);
        if(distance <= 0.01f)
        {
            cloud.GetComponent<SpriteRenderer>().enabled = false;
            startCloudTime = true;
            cloudMove = false;
        }
    }
    /// <summary>
    /// 场景蝴蝶出现
    /// </summary>
    public void ButterAppear()
    {
        butterfly01.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        butterfly01.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = true;
        butterfly01.GetComponent<Animator>().enabled = true;
        butterfly01.GetComponent<Animator>().runtimeAnimatorController = butterAnims[Random.Range(0, butterAnims.Length)];
        butterfly01.transform.position = butterflyAppearPos[Random.Range(0, butterflyAppearPos.Length)].transform.position;
        currentButterDisappearPos = butterflyDisappearPos[Random.Range(0, butterflyDisappearPos.Length)].transform;
        butterMove = true;
    }
    /// <summary>
    /// 场景蝴蝶移动
    /// </summary>
    public void ButterMove()
    {
        butterfly01.transform.position = Vector2.MoveTowards(butterfly01.transform.position, currentButterDisappearPos.position, butterflyMoveSpeed * Time.fixedDeltaTime);
        var distance = Vector2.Distance(butterfly01.transform.position, currentButterDisappearPos.position);
        if(distance <= 0.01f)
        {
            butterfly01.GetComponent<Animator>().enabled = false;
            startButterTime = true;
            butterMove = false;
        }
    }
    public void BirdAppear()
    {
        if (birdLeftDownPos != null && birdRightUpPos != null)
        {
            for (int i = 0; i < birds.Length; i++)
            {
                birds[i].gameObject.SetActive(true); 
                birds[i].GetComponent<Transform>().position = new Vector2(Random.Range(birdLeftDownPos.position.x, birdRightUpPos.position.x), Random.Range(birdLeftDownPos.position.y, birdRightUpPos.position.y));
            }
            
        }
       
        
    }
}
