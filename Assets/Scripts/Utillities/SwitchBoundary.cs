using Cinemachine;
using UnityEngine;

public class SwitchBoundary : MonoBehaviour //调用在Virtual Camera对象摄像机上
{
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += SwitchConfinerShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= SwitchConfinerShape;
    }
    /// <summary>
    /// 加载场景之后把找到摄像机边界
    /// </summary>
    private void SwitchConfinerShape()
    {
        PolygonCollider2D polygonCollider2D = GameObject.FindGameObjectWithTag("BoundaryConfiner").GetComponent<PolygonCollider2D>();
        CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();
        confiner.m_BoundingShape2D = polygonCollider2D;
        //在切换新场景时，需要先把源场景的边界缓存清除才可以更新新场景的边界
        confiner.InvalidatePathCache();
    }
}
