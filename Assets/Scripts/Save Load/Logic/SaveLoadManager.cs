using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
namespace MFarm.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>    //调用在SaveLoadManager对象上
    {
        private List<ISaveable> saveableList = new List<ISaveable>();
        //三条存档菜单
        public List<DataSlot> dataSlots = new List<DataSlot>(new DataSlot[3]);

        private string jsonFolder;

        private int currentDataIndex;

        protected override void Awake()
        {
            base.Awake();
            jsonFolder = Application.persistentDataPath + "/Save Data/";
            ReadSaveData();
        }


        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            //游戏结束时的事件
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("Save");
                Save(currentDataIndex);
            }
            if (Input.GetKeyUp(KeyCode.O))
            {
                Load(currentDataIndex);
            }
        }


        private void OnStartNewGameEvent(int index)
        {
            currentDataIndex = index;
        }

        private void OnEndGameEvent()
        {
            Debug.Log("Save");
            //保存当前游戏进度
            Save(currentDataIndex);
        }


        /// <summary>
        /// 将存档信息添加到ISaveable接口列表中
        /// </summary>
        /// <param name="saveable"></param>
        public void RegisterSaveable(ISaveable saveable)
        {
            if(!saveableList.Contains(saveable))
            {
                saveableList.Add(saveable);
            }
        }
        /// <summary>
        /// 读取游戏进度映射到SaveSlotUI上
        /// </summary>
        private void ReadSaveData()
        {
            //判断存档目录是否存在
            if (Directory.Exists(jsonFolder))
            {
                for(int i = 0; i < dataSlots.Count; i++)
                {
                    var resultPath = jsonFolder + "data" + i + ".sav";
                    if (File.Exists(resultPath))
                    {
                        var stringData = File.ReadAllText(resultPath);
                        var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
                        dataSlots[i] = jsonData;
                    }
                }
            }
        }
        /// <summary>
        /// 将所有Manager返回的SaveData存入,以及三条进度存档
        /// </summary>
        /// <param name="index"></param>
        private void Save(int index)
        {
            DataSlot data = new DataSlot();
            //将所有Manager返回的SaveData与GUID都存进SaveSlot中
            foreach (var saveable in saveableList)
            {
                data.dataDict.Add(saveable.GUID, saveable.GenerateSaveData());
            }

            dataSlots[index] = data;
            //存档文件
            var resultPath = jsonFolder + "data" + index + ".sav";
            //序列化存档文件
            var jsonData = JsonConvert.SerializeObject(dataSlots[index], Formatting.Indented);
            //如果不存在resultPath路径文件,新创建,有的话覆盖
            if (!File.Exists(resultPath))
            {
                Directory.CreateDirectory(jsonFolder);
            }
            Debug.Log("DATA" + index + "SAVED!");
            File.WriteAllText(resultPath, jsonData);
        }
        /// <summary>
        /// 读取存档
        /// </summary>
        /// <param name="index"></param>
        public void Load(int index)
        {
            currentDataIndex = index;
            var resultPath = jsonFolder + "data" + index + ".sav";
            //读取存档文件
            var stringData = File.ReadAllText(resultPath);
            //反序列化
            var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
            //把存档好的SaveData读取出来
            foreach(var saveable in saveableList)
            {
                saveable.RestoreData(jsonData.dataDict[saveable.GUID]);
            }


        }
    }
}

