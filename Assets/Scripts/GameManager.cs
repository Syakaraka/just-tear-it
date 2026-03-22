using UnityEngine;
using System;

/// <summary>
/// 游戏总管理器（单例）
/// 负责游戏状态管理、数据存储、场景协调
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }

    [Header("游戏状态")]
    [SerializeField] private GameState currentState = GameState.Boot;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int totalLevels = 15;

    [Header("玩家数据")]
    [SerializeField] private int coins = 0;
    [SerializeField] private int[] levelStars = new int[15]; // 每关星级
    [SerializeField] private int currentChapter = 1;

    // 事件
    public event Action<GameState> OnGameStateChanged;
    public event Action<int, int> OnCoinsChanged;
    public event Action<int, int> OnLevelCompleted; // levelId, stars

    public GameState CurrentState => currentState;
    public int CurrentLevel => currentLevel;
    public int TotalLevels => totalLevels;
    public int Coins => coins;
    public int[] LevelStars => levelStars;
    public int CurrentChapter => currentChapter;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化玩家数据
    /// </summary>
    public void InitPlayerData()
    {
        // 从PlayerPrefs加载数据
        coins = PlayerPrefs.GetInt("Coins", 0);

        // 加载每关星级
        string starsData = PlayerPrefs.GetString("LevelStars", "");
        if (!string.IsNullOrEmpty(starsData))
        {
            string[] stars = starsData.Split(',');
            for (int i = 0; i < Mathf.Min(stars.Length, levelStars.Length); i++)
            {
                int.TryParse(stars[i], out levelStars[i]);
            }
        }

        currentChapter = PlayerPrefs.GetInt("CurrentChapter", 1);
    }

    /// <summary>
    /// 保存玩家数据
    /// </summary>
    public void SavePlayerData()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetString("LevelStars", string.Join(",", levelStars));
        PlayerPrefs.SetInt("CurrentChapter", currentChapter);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 切换游戏状态
    /// </summary>
    public void SetGameState(GameState state)
    {
        if (currentState == state) return;

        currentState = state;
        OnGameStateChanged?.Invoke(state);
    }

    /// <summary>
    /// 开始指定关卡
    /// </summary>
    public void StartLevel(int levelId)
    {
        if (levelId < 1 || levelId > totalLevels)
        {
            Debug.LogWarning($"Invalid level ID: {levelId}");
            return;
        }

        currentLevel = levelId;
        SetGameState(GameState.Playing);
    }

    /// <summary>
    /// 完成关卡并记录成绩
    /// </summary>
    public void CompleteLevel(int levelId, int stars)
    {
        if (levelId < 1 || levelId > totalLevels) return;

        // 更新星级（取最高）
        int index = levelId - 1;
        if (stars > levelStars[index])
        {
            levelStars[index] = stars;
        }

        // 计算奖励
        int reward = CalculateReward(stars);
        AddCoins(reward);

        // 解锁下一关
        UnlockNextLevel(levelId);

        SavePlayerData();
        OnLevelCompleted?.Invoke(levelId, stars);
    }

    /// <summary>
    /// 计算关卡奖励
    /// </summary>
    private int CalculateReward(int stars)
    {
        return stars * 10; // 1星=10, 2星=20, 3星=30
    }

    /// <summary>
    /// 添加金币
    /// </summary>
    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins, amount);
        SavePlayerData();
    }

    /// <summary>
    /// 消耗金币
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;

        coins -= amount;
        OnCoinsChanged?.Invoke(coins, -amount);
        SavePlayerData();
        return true;
    }

    /// <summary>
    /// 解锁下一关
    /// </summary>
    private void UnlockNextLevel(int currentLevelId)
    {
        // 章节解锁逻辑
        int newChapter = (currentLevelId - 1) / 5 + 1;
        if (newChapter > currentChapter)
        {
            currentChapter = newChapter;
        }
    }

    /// <summary>
    /// 获取关卡星级
    /// </summary>
    public int GetLevelStars(int levelId)
    {
        if (levelId < 1 || levelId > totalLevels) return 0;
        return levelStars[levelId - 1];
    }

    /// <summary>
    /// 检查关卡是否解锁
    /// </summary>
    public bool IsLevelUnlocked(int levelId)
    {
        if (levelId == 1) return true;

        // 前一关至少获得1星则解锁
        int prevStars = GetLevelStars(levelId - 1);
        return prevStars > 0;
    }

    /// <summary>
    /// 获取章节起始关卡
    /// </summary>
    public int GetChapterStartLevel(int chapter)
    {
        return (chapter - 1) * 5 + 1;
    }

    /// <summary>
    /// 重置所有数据
    /// </summary>
    public void ResetAllData()
    {
        coins = 0;
        levelStars = new int[totalLevels];
        currentChapter = 1;
        SavePlayerData();
    }
}

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    Boot,
    MainMenu,
    LevelSelect,
    Playing,
    Paused,
    Result,
    Shop
}
