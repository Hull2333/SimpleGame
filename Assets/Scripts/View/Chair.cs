using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour //??????????????????????Chiar??????
{
    //????????ãý????
    public Vector2Int chairFaceDir;
    private BoxCollider2D BoxCollider2D => GetComponent<BoxCollider2D>();
   
    public void QuitChairBoxCollider()
    {
        BoxCollider2D.enabled = true;
    }
    public void ActiveChairBoxCollider()
    {
        BoxCollider2D.enabled = false;
    }
}