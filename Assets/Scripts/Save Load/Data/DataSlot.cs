using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Transition;
namespace MFarm.Save
{
    /// <summary>
    /// 游戏存档，string是GUID，GameSaveData该存档的所有内容
    /// </summary>
    public class DataSlot
    {
        public Dictionary<string,GameSaveData> dataDict = new Dictionary<string,GameSaveData>();
        #region 用来UI显示进度详情
        /// <summary>
        /// 进度存档的时间显示
        /// </summary>
        public string DataTime
        {
            get
            {
                var key = TimeManager.Instance.GUID;

                if (dataDict.ContainsKey(key))
                {
                    var timeData = dataDict[key];
                    return timeData.timeDict["gameYear"] + "年/" + (Season)timeData.timeDict["gameSeason"] + "/" + timeData.timeDict["gameMonth"] + "月/" + timeData.timeDict["gameDay"] + "日/";

                }
                else
                {
                    return string.Empty;
                } 
            }
        }
        /// <summary>
        /// 进度存档的场景显示
        /// </summary>
        public string DataScene
        {
            get
            {
                var key = TransitionManager.Instance.GUID;
                if(dataDict.ContainsKey(key))
                {
                    var transitionData = dataDict[key];
                    return transitionData.dataSceneName switch
                    {
                        "1.Field" => "家园",
                        "2.house" => "家",
                        "3.Market" => "青草西小道",
                        _ => string.Empty
                    };
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        #endregion
    }


}

