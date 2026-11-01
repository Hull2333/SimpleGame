using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchFish : MonoBehaviour //调用在Catch对象上
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("SmallFish"))
        {
            FishingGameManager.Instance.currentFishProgress += 1.12f * Time.deltaTime;
            GetComponent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
