using MFarm.Inventory;
using MFarm.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemClick : MonoBehaviour //调用在ItemClick预制体上
{
    public int itemID;
    public ItemDetails itemDetails;
    private SpriteRenderer sprite => transform.GetChild(0).GetComponent<SpriteRenderer>();
    /// <summary>
    /// 设置物品的图片
    /// </summary>
    public void SetItemSprite()
    {
        itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
        sprite.sprite = itemDetails.itemOnWorldSprite;
    }
    /// <summary>
    /// 添加到玩家背包中
    /// </summary>
    public void AddItemToBag()
    {
        InventoryManager.Instance.AddRewardItem(itemID, 1);
        var key = $"{transform.position.x - 0.5f}" + "X" + $"{transform.position.y - 0.5f}" + "Y" + SceneManager.GetActiveScene().name;
        var tile = GridMapManager.Instance.GetTileDetails(key);
        tile.havePlace = -1;
        Destroy(gameObject);
    }
}
