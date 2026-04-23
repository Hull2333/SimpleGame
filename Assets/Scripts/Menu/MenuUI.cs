using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour //调用在Menu对象上
{
    //获取Menu的各种Panel
    public GameObject[] penels;
    /// <summary>
    /// 控制Menu的所有Panel显示
    /// </summary>
    /// <param name="index"></param>
    public void SwitchPanel(int index)
    {
        for(int i = 0; i < penels.Length; i++)
        {
            if(i== 0)
            {
                //把当前要显示的Panel移动到最下边
                penels[1].gameObject.SetActive(true);

            }
        }
    }
    /// <summary>
    /// 退出游戏按钮
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }
    /// <summary>
    /// 关闭当前UI Panel
    /// </summary>
    /// <param name="index"></param>
    public void QuitCurrentPanel( int index )
    {
        for (int i = 0; i < penels.Length; i++)
        {
            if(i == index)
            {
                penels[i].gameObject.SetActive(false);
            }
        }
    }
     

}
