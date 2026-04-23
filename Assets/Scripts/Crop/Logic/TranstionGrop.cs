using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranstionGrop : MonoBehaviour
{
    public GameObject cropPrefab;

   
    void Start()
    {
        Instantiate(cropPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    
}
