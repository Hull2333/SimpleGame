using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour  //调用在家具预制体上
{
    public int itemID;
   
    private BoxCollider2D collider2D => gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>();
    public void SetCollider(bool isActive)
    {
        collider2D.enabled = isActive;
       
    }
   

}
