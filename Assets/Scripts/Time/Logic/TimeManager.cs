using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MFarm.Save;
using Unity.VisualScripting;

public class TimeManager : Singleton<TimeManager>,ISaveable    //调用在TimeManager对象上
{
    [HideInInspector] public int gameSecond, gameMinute,gameHour,gameDay,gameMonth,gameYear;
    [HideInInspector] public Weekday gameWeekday = Weekday.周四;
    public Season gameSeason=Season.春天;
    //一季有多少月
    private int monthInSeason = 1;
    //天气预报，0为今天，1~3为未来三天
    private Weather[] weatherForecast = new Weather[4];
    //今天的天气
    public Weather currentWeather => weatherForecast[0];
    //各季节随机各天气的权重，行顺序与Season枚举一致，列顺序与Weather枚举一致
    private readonly int[,] weatherWeight = new int[,]
    {
        { 50, 40, 10 },   //春天：晴天50%、雨天40%、打雷10%
        { 60, 20, 20 },   //夏天：晴天60%、雨天20%、打雷20%
        { 80, 20, 0 },    //秋天：晴天80%、雨天20%、打雷0%
        { 60, 40, 0 },    //冬天：晴天60%、雨天40%、打雷0%
    };
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
        //先随机一套天气预报，读档时会被存档数据覆盖
        RandomWeatherForecast();
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
        //按G键快速过一天，与正常时间流逝走同一套日期推进逻辑
        if (Input.GetKeyDown(KeyCode.G))
        {
            NextDay();
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
        gameWeekday = Weekday.周四;
        //新游戏重新随机四天的天气预报
        RandomWeatherForecast();
        //==========测试用：自定义新游戏第一天的星期和天气，测试完删掉这一段即可==========
        //星期可选：周日、周一、周二、周三、周四、周五、周六
        //天气可选：Sunny（晴天）、Rainy（雨天）、Thunder（打雷）
        gameWeekday = Weekday.周日;
        weatherForecast[0] = Weather.Sunny;
        //==============================================================================
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
                    gameHour = 0;
                    NextDay();
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
    /// 推进到下一天，处理星期、月份、季节和年份的变化
    /// </summary>
    public void NextDay()
    {
        gameDay++;
        //每过一天，星期循环推进一天
        gameWeekday = GetNextWeekday(gameWeekday);
        if(gameDay > Settings.dayHold)
        {
            gameMonth++;
            gameDay = 1;
            if(gameMonth > 4)
            {
                gameMonth = 1;
            }
            //每过一整月，减少monthInSeason
            monthInSeason--;
            if(monthInSeason == 0)
            {
                monthInSeason = 1;
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
            EventHandler.CallGameDayEvent(gameDay, gameSeason, gameWeekday);
        }
        //天气预报整体前移一天，只随机补上新的第四天
        for (int i = 0; i < weatherForecast.Length - 1; i++)
        {
            weatherForecast[i] = weatherForecast[i + 1];
        }
        weatherForecast[weatherForecast.Length - 1] = GetRandomWeather();
    }
    /// <summary>
    /// 随机生成四天的天气预报
    /// </summary>
    private void RandomWeatherForecast()
    {
        for (int i = 0; i < weatherForecast.Length; i++)
        {
            weatherForecast[i] = GetRandomWeather();
        }
    }
    /// <summary>
    /// 按当前季节的概率随机一个天气
    /// </summary>
    private Weather GetRandomWeather()
    {
        int seasonIndex = (int)gameSeason;
        int weatherCount = weatherWeight.GetLength(1);
        //先算出当前季节所有天气的权重总和
        int totalWeight = 0;
        for (int i = 0; i < weatherCount; i++)
        {
            totalWeight += weatherWeight[seasonIndex, i];
        }
        //在权重总和内随机一个数，落在哪个天气的权重区间就返回哪个天气
        int roll = UnityEngine.Random.Range(0, totalWeight);
        for (int i = 0; i < weatherCount; i++)
        {
            roll -= weatherWeight[seasonIndex, i];
            if (roll < 0)
            {
                return (Weather)i;
            }
        }
        return Weather.Sunny;
    }
    /// <summary>
    /// 获取下一天的星期，0~6环形循环
    /// </summary>
    private Weekday GetNextWeekday(Weekday weekday)
    {
        //Weekday共有7个枚举值（周日~周六）
        int weekdayNumber = (int)weekday;
        weekdayNumber++;
        if (weekdayNumber > 6)
        {
            weekdayNumber = 0;
        }
        return (Weekday)weekdayNumber;
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
        saveData.timeDict.Add("gameWeekday", (int)gameWeekday);
        //保存今天和未来三天的天气
        for (int i = 0; i < weatherForecast.Length; i++)
        {
            saveData.timeDict.Add("weather" + i, (int)weatherForecast[i]);
        }

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
        //旧存档中没有gameWeekday字段，没有时保持默认值，避免抛KeyNotFoundException
        if (saveData.timeDict.ContainsKey("gameWeekday"))
        {
            gameWeekday = (Weekday)saveData.timeDict["gameWeekday"];
        }
        //旧存档中没有天气字段，没有时保持当前随机值
        for (int i = 0; i < weatherForecast.Length; i++)
        {
            string weatherKey = "weather" + i;
            if (saveData.timeDict.ContainsKey(weatherKey))
            {
                weatherForecast[i] = (Weather)saveData.timeDict[weatherKey];
            }
        }
    }
}
