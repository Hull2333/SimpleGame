using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DialogueUI : Singleton<DialogueUI> //调用在DialogueCanvas01上
{
    [Header("Basic Elements")]
    public Image partrait;
    public Animator partraitAnim;
    public Text mainContent;
    public Button nextButton;
    public Text name;
    public GameObject dialoguePanel;
    [Header("Options")]
    public RectTransform optionPanel;
    public DialogueOptionUI optionPrefab;
    [Header("Data")]
    [HideInInspector] public DialogueData_OS currentData;
    //从那一条开始对话
    private int currentIndex = 0;
    //正在和NPC对话
    public bool talkWithNPC;

    /// <summary>
    /// override在Singleton单例模式下启动
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        //点击nextButton继续后续对话
        nextButton.onClick.AddListener(ContinueDialogue);
    }
    /// <summary>
    /// 继续对话
    /// </summary>
    private void ContinueDialogue()
    {
        //避免报错，只有currentIndex小于当前对话数据库的piece数才显示对话
        if (currentIndex < currentData.dialoguePieces.Count)
        {
            //当当前对话isEnd为true也结束对话
            if (currentData.dialoguePieces[currentIndex].isEnd)
            {
                partrait.gameObject.SetActive(false);
                name.gameObject.SetActive(false);
                dialoguePanel.SetActive(false);
                //有nextButton的对话结束后时间恢复
                EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
                EventHandler.CallPromoteNPCEvent();
                talkWithNPC = false;
            }
            else
            {
                if (currentData.dialoguePieces[currentIndex].shakeImage)
                {
                    ShakePartraitImage();
                }
                UpdateMainDialogue(currentData.dialoguePieces[currentIndex]);
            } 
        }
        //点击nextButton后面没有对话后关闭对话UI
        else
        {
            partrait.gameObject.SetActive(false);
            name.gameObject.SetActive(false);
            dialoguePanel.SetActive(false);
            //有nextButton的对话结束后时间恢复
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
            EventHandler.CallPromoteNPCEvent();
            talkWithNPC = false;
        }
    }
    /// <summary>
    /// 获取对话数据库
    /// </summary>
    /// <param name="data"></param>
    public void UpdateDialogueData(DialogueData_OS data)
    {
        currentData = data;
        currentIndex = 0;
    }
    /// <summary>
    /// 开启对话框，设置对话框要素
    /// </summary>
    /// <param name="piece"></param>
    public void UpdateMainDialogue(DialoguePieces piece)
    {
        talkWithNPC = true;
        //对话时游戏时间暂停
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        dialoguePanel.SetActive(true);
        currentIndex++;
        //控制对话框图片显示
        if (piece.Image != null)
        {
            partrait.gameObject.SetActive(true);
            name.gameObject.SetActive(true);
            name.text = piece.name.ToString();
            partrait.sprite = piece.Image;
        }
        else
        {
            partrait.gameObject.SetActive(false);
            name.gameObject.SetActive(false);
        }
        StartCoroutine(DelayTime(piece));
       
        //设置nextButton显示
        //当此piece没有OptionButton且还有后续对话时显示nextButton
        if (piece.options.Count == 0 && currentData.dialoguePieces.Count > 0)
        { 
            nextButton.interactable = true;
            nextButton.gameObject.SetActive(true);
           
        }
        //piece下有OptionButton就把nextButton失效且不显示
        else
        {
            nextButton.interactable = false;
            nextButton.gameObject.SetActive(false);
        }
        //创建option
        CreateOption(piece);
    }
    /// <summary>
    /// 延迟文本的出现
    /// </summary>
    /// <param name="currentpiece"></param>
    /// <returns></returns>
    private IEnumerator DelayTime(DialoguePieces currentpiece)
    {
        yield return new WaitForSeconds(0.3f);
        //将对话文本初始化
        mainContent.text = "";
        mainContent.DOText(currentpiece.text, 1f);
    }
    /// <summary>
    /// 生成OptionButton
    /// </summary>
    /// <param name="piece"></param>
    private void CreateOption(DialoguePieces piece)
    {
        //清空optionPanel下的所有子物体
        if (optionPanel.childCount > 0)
        {
            for(int i = 0;i< optionPanel.childCount; i++)
            {
                Destroy(optionPanel.GetChild(i).gameObject);
            }
        }
        //根据picec生成对应数量的optionButton
        for(int i = 0; i < piece.options.Count; i++)
        {
            var option = Instantiate(optionPrefab, optionPanel);
            option.UpdateOption(piece, piece.options[i]);
        }
    }
    /// <summary>
    /// 生成OptionButton时的对话结束后关闭对话UI
    /// </summary>
    public void QuitDialogueUI()
    {
        talkWithNPC = false;
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        dialoguePanel.SetActive(false);
        name.gameObject.SetActive(false);
        partrait.gameObject.SetActive(false);
    }
    /// <summary>
    /// 抖动NPC头像
    /// </summary>
    public void ShakePartraitImage()
    {
        //抖动NPC头像
        partraitAnim.SetTrigger("partraitShaking");
    }
}
