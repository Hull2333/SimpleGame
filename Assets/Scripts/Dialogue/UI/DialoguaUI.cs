using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MFarm.Dialogue;
using TMPro;

public class DialoguaUI : MonoBehaviour //调用在dialogue Canvas对象上
{
    public GameObject dialogueBox;
    public Text dialogueText;
    public Image faceRight, faceLeft;
    public TextMeshProUGUI nameRight, nameLeft;
    //继续按钮
    public GameObject continueBox;

    private void Awake()
    {
        continueBox.SetActive(false);
    }

    private void OnEnable()
    {
        //显示NPC对话UI
        EventHandler.ShowDialogueEvent += OnShowDialogueEvent;
    }

    private void OnDisable()
    {
        EventHandler.ShowDialogueEvent -= OnShowDialogueEvent;
    }

    private void OnShowDialogueEvent(DialoguePiece piece)
    {
        StartCoroutine(ShowDialogue(piece));
    }
    /// <summary>
    /// 设置对话UI
    /// </summary>
    /// <param name="piece"></param>
    /// <returns></returns>
    private IEnumerator ShowDialogue(DialoguePiece piece)
    {
        //对话UI的初始状态
        if(piece != null)
        {
            piece.isDone = false;
            dialogueBox.SetActive(true);
            continueBox.SetActive(false);
            //清空对话文本
            dialogueText.text = string.Empty;

            if(piece.name != string.Empty)
            {
                if (piece.onLeft)
                {
                    faceRight.gameObject.SetActive(false);
                    faceLeft.gameObject.SetActive(true);
                    faceLeft.sprite = piece.faceImage;
                    nameLeft.text = piece.name;
                }
                else
                {
                    faceLeft.gameObject.SetActive(false);
                    faceRight.gameObject.SetActive(true);
                    faceRight.sprite = piece.faceImage;
                    nameRight.text = piece.name;
                }
            }
            //如果未设置NPC的名字和头像，则不显示
            else
            {
                faceLeft.gameObject.SetActive(false);
                faceRight.gameObject.SetActive(false);
                nameLeft.gameObject.SetActive(false);
                nameRight.gameObject.SetActive(false);
            }

            //piece.dialogueText显示内容，1f显示时间，WaitForCompletion()等候前面的方法执行完
            yield return dialogueText.DOText(piece.dialogueText, 1f).WaitForCompletion();
            piece.isDone = true;
            //一句段话结束后，显示继续按钮 
            if(piece.hasToPause && piece.isDone)
            {
                continueBox.SetActive(true);
            }
        }
        else
        {
            dialogueBox.SetActive(false) ;
            yield break;
        }
    }
}

