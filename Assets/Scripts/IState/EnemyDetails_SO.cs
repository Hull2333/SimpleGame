using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EnemyDetails_OS", menuName = "Enemy/EnemyDataList")]
public class EnemyDetails_OS : ScriptableObject
{
    public List<EnemyDetails> EnemyDetailsList;
}
