using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour //调用在所有Chair对向上
{
    //椅子面朝方向
    public Vector2Int chairFaceDir;
    private BoxCollider2D BoxCollider2D => GetComponent<BoxCollider2D>();
    
    public void QuitChairBoxCollider()
    {
        BoxCollider2D.enabled = false;
    }
    public void ActiveChairBoxCollider()
    {
        BoxCollider2D.enabled = true;
    }
}
