
using TMPro;
using UnityEngine;

public class DamageNum : MonoBehaviour //调用在DamageNum预制体上
{
    public TextMeshPro damageNumText;
    /// <summary>
    /// 摧毁自身，调用在动画帧上
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    /// <summary>
    /// 设置伤害数字
    /// </summary>
    /// <param name="Num"></param>
    public void SetDamageNumText(int Num)
    {
        damageNumText.text = Num.ToString();
    }
    /// <summary>
    /// 改变伤害数字颜色
    /// </summary>
    /// <param name="isPowerful"></param>
    public void SetDamageNumColor(bool isPowerful)
    {
        if (isPowerful)
        {
            damageNumText.color = Color.yellow;
        }
        else
        {
            damageNumText.color = Color.white;
        }
    }
}
