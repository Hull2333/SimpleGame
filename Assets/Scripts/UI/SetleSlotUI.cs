using System.Collections;
using System.Collections.Generic;
using MFarm.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetleSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public Image setleItemImage;
    public TextMeshProUGUI SetleAmount;
    public TextMeshProUGUI SetleAllValue;
    // 该结算格对应的物品数据，由 UIManager 生成结算格时写入
    public ItemDetails itemDetails;
    private UIManager uiManager;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
    }

    // 鼠标进入结算格时显示物品详情
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemDetails == null || uiManager == null || uiManager.SetleDesUI == null)
        {
            return;
        }
        uiManager.SetleDesUI.SetActive(true);
        uiManager.setleItemName.text = itemDetails.itemName;
        // 单价按出售价计算，与结算总价值使用同一算法
        uiManager.setleItemValue.text = ((int)(itemDetails.itemPrice * itemDetails.sellPercentage)).ToString();
        MoveDesToMouse();
    }

    // 鼠标在本格上移动时，让详情跟随鼠标
    public void OnPointerMove(PointerEventData eventData)
    {
        MoveDesToMouse();
    }

    // 鼠标离开结算格时隐藏物品详情
    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiManager == null || uiManager.SetleDesUI == null)
        {
            return;
        }
        uiManager.SetleDesUI.SetActive(false);
    }

    // 把详情界面移动到当前鼠标位置
    private void MoveDesToMouse()
    {
        if (uiManager == null || uiManager.SetleDesUI == null)
        {
            return;
        }
        // 结算界面是 Overlay 画布，屏幕坐标可以直接作为位置
        uiManager.SetleDesUI.transform.position = Input.mousePosition;
    }
}
