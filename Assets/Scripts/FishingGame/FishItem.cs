using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishItem : MonoBehaviour //调用在FishItem对象上
{
    private ParticleSystem dropletEffect;

    private void OnEnable()
    {
        dropletEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        dropletEffect.Play();
    }
    
}
