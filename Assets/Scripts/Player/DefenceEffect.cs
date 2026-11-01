using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceEffect : MonoBehaviour //调用在DefenceEffect01对象上
{
    private AnimatorStateInfo info;
    private void Update()
    {
        info = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.99f)
        {
            gameObject.SetActive(false);
        }
    }
}
