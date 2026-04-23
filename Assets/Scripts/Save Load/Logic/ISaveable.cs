using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Save
{
    /// <summary>
    /// interface接口
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// 让每个存档拿到自己的GUID
        /// </summary>
        string GUID {  get; }
        void RegisterSaveable()
        {
            SaveLoadManager.Instance.RegisterSaveable(this);
        }
        /// <summary>
        /// 通过GenerateSaveData()方法返回GameSaveData
        /// </summary>
        /// <returns></returns>
        GameSaveData GenerateSaveData();
        void RestoreData(GameSaveData saveData);
    }

}
