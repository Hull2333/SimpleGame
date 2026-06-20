using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MFarm.Save;
using Unity.VisualScripting;

public class TimeManager : Singleton<TimeManager>,ISaveable    //调用在TimeManager对象上
{
    [HideInInspector] public int gameSecond, gameMinute,gameHour,gameDay,gameMonth,gameYear;
    private Season gameSeason=Season.春天;
    //一季有多少月
    private int monthInSeason = 3;
    //计时器
    private float tikTime;
    //时间暂停
    public bool gameClockPause;
    //游戏当前时间戳
    public TimeSpan gameTime => new TimeSpan(gameHour,gameMinute,gameSecond);

    public string GUID => GetComponent<DataGUID>().guid;

    //灯光时间差
    private float timeDifference;
    public PlayerController playerController;
    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        //新游戏开始时需要重置的数据
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        //游戏结束的事件
        EventHandler.EndGameEvent += OnEndGameEvent;
    }
    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }
    /// <summary>
    /// 游戏一开始就执行游戏时间事件
    /// </summary>
    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
        gameClockPause = true;
        //EventHandler.CallGameMinuteEvent(gameMinute, gameHour,gameDay,gameSeason);
        //EventHandler.CallGameDateEvemnt(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        ////切换灯光
        //EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
    }
    private void Update()
    {
        if (!gameClockPause)
        {
            //创建秒钟计时器,当tikTime达到Setting设置的秒钟计时，直接跳跃到下一秒
            tikTime += Time.deltaTime;
            if (tikTime >= Settings.secondThreshold)
            {
                UpdateGameTime();
                tikTime -= Settings.secondThreshold;
                
            }
        }
        //按住T时，快速执行60次UpdateGameTime()方法，即按一下快速过1分钟
        if (Input.GetKey(KeyCode.T))
        {
            for (int i = 0; i < 60; i++)
            {
                UpdateGameTime();
            }
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            gameDay++;
            EventHandler.CallGameDayEvent(gameDay,gameSeason);
            EventHandler.CallGameDateEvemnt(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        }
    }

    /// <summary>
    /// 时间暂停
    /// </summary>
    /// <param name="gameState"></param>
    private void OnUpdateGameStateEvent(GameState gameState)
    {
        if (playerController.isNpcEvent)
        {
            gameClockPause = true;
        }
        else
        {
            gameClockPause = gameState == GameState.Pause;
        }
       
    }

    /// <summary>
    /// 在场景加载之后时间恢复
    /// </summary>
    private void OnAfterSceneLoadedEvent()
    {
        gameClockPause = false;
    }
    /// <summary>
    /// 在场景加载之前时间暂停
    /// </summary>
    private void OnBeforeSceneUnloadEvent()
    {
        gameClockPause = true;
    }

    private void OnStartNewGameEvent(int obj)
    {
        NewGameTime();
        gameClockPause = false;
    }

    private void OnEndGameEvent()
    {
        gameClockPause = true;
    }


    /// <summary>
    /// 初始化游戏的时间和季节
    /// </summary>
    private void NewGameTime()
    {
        gameSecond = 0;
        gameMinute = 0;
        gameHour = 7;
        gameDay = 1;
        gameMonth = 1;
        gameYear = 1;
        gameSeason = Season.春天;
    }
    /// <summary>
    /// 游戏时间、季节变化循环
    /// </summary>
    private void UpdateGameTime()
    {
        gameSecond++;
        if (gameSecond > Settings.secondHold)
        {
            gameMinute++;
            gameSecond = 0;
            if (gameMinute > Settings.minuteHold)
            {
                gameHour++;
                gameMinute = 0;
                if (gameHour > Settings.hourHold)
                {
                    gameDay++;
                    gameHour = 0;
                    if(gameDay > Settings.dayHold)
                    {
                        gameMonth++;
                        gameDay = 1;
                        if(gameMonth > 12)
                        {
                            gameMonth = 1;
                        }
                        //每过一整月，减少monthInSeason
                        monthInSeason--;
                        if(monthInSeason == 0)
                        {
                            monthInSeason = 3;
                            //gameSeason为枚举,int化后gameSeason值为4,因为有四个枚举
                            int seasonNumber = (int)gameSeason;
                            seasonNumber++;
                            if (seasonNumber > Settings.seasonHold)
                            {
                                seasonNumber = 0;
                                gameYear++;
                            }
                            //再把int化后gameSeason改为原来的枚举类型
                            gameSeason = (Season)seasonNumber;
                        }
                        //每天刷新各地图瓦片信息和作物信息
                        EventHandler.CallGameDayEvent(gameDay, gameSeason);
                    }
                    
                }
                //每一小时过去，开始执行小时事件
                EventHandler.CallGameDateEvemnt(gameHour, gameDay, gameMonth, gameYear, gameSeason);
            }
            // 每一分钟过去，开始执行分钟事件
            EventHandler.CallGameMinuteEvent(gameMinute, gameHour,gameDay, gameSeason);
            //切换灯光
            EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
        }
        
    }
    /// <summary>
    /// 根据当前游戏时间切换地图灯光模式
    /// </summary>
    /// <returns></returns>
    private LightShift GetCurrentLightShift()
    {
        if(gameTime >= Settings.morningTime &&  gameTime < Settings.nightTime)
        {
            timeDifference = (float)(gameTime - Settings.morningTime).TotalMinutes;
            return LightShift.Morning;
        }
        if(gameTime < Settings.morningTime || gameTime >= Settings.nightTime)
        {
            //Abs绝对值
            timeDifference = Mathf.Abs((float)(gameTime - Settings.nightTime).TotalMinutes);
            Debug.Log(timeDifference);
            return LightShift.Night;
        }
        return LightShift.Morning;
    }

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("gameYear", gameYear);
        saveData.timeDict.Add("gameMonth", gameMonth);
        saveData.timeDict.Add("gameDay", gameDay);
        saveData.timeDict.Add("gameHour", gameHour);
        saveData.timeDict.Add("gameMinute", gameMinute);
        saveData.timeDict.Add("gameSecond", gameSecond);
        saveData.timeDict.Add("gameSeason", (int)gameSeason);

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        gameYear = saveData.timeDict["gameYear"];
        gameMonth = saveData.timeDict["gameMonth"];
        gameDay = saveData.timeDict["gameDay"];
        gameHour = saveData.timeDict["gameHour"];
        gameMinute = saveData.timeDict["gameMinute"];
        gameSecond = saveData.timeDict["gameSecond"];
        gameSeason = (Season)saveData.timeDict["gameSeason"];
    }
}
