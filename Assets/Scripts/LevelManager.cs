using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// 关卡管理器
/// 负责关卡加载、配置、进度追踪
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Serializable]
    public class LevelConfig
    {
        public int levelId;
        public string levelName;
        public string sceneName;
        public LevelType levelType;
        public float targetTearPercent = 0.8f;  // 目标撕裂比例
        public int baseCoins = 10;               // 基础金币奖励
        public Sprite thumbnail;                  // 关卡缩略图
        public string description;                // 关卡描述
    }

    [Serializable]
    public class ChapterConfig
    {
        public int chapterId;
        public string chapterName;
        public LevelConfig[] levels;
    }

    public enum LevelType
    {
        Tape,       // 胶带关
        Sticker,    // 贴纸关
        Package,    // 包装关
        Mixed       // 复合关
    }

    [Header("关卡配置")]
    [SerializeField] private ChapterConfig[] chapters;

    [Header("场景配置")]
    [SerializeField] private string gameSceneName = "GameScene";

    private int currentLevelId = 1;
    private LevelConfig currentLevelConfig;

    public event Action<int, int> OnLevelStarUpdated; // levelId, stars
    public event Action OnLevelLoaded;

    public LevelConfig CurrentLevelConfig => currentLevelConfig;
    public ChapterConfig[] Chapters => chapters;

    /// <summary>
    /// 加载指定关卡
    /// </summary>
    public void LoadLevel(int levelId)
    {
        if (!IsValidLevel(levelId))
        {
            Debug.LogError($"Invalid level ID: {levelId}");
            return;
        }

        currentLevelId = levelId;
        currentLevelConfig = GetLevelConfig(levelId);

        // 通知游戏开始
        GameManager.Instance.StartLevel(levelId);

        // 异步加载场景
        SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 获取关卡配置
    /// </summary>
    public LevelConfig GetLevelConfig(int levelId)
    {
        foreach (var chapter in chapters)
        {
            foreach (var level in chapter.levels)
            {
                if (level.levelId == levelId)
                    return level;
            }
        }
        return null;
    }

    /// <summary>
    /// 检查关卡ID是否有效
    /// </summary>
    public bool IsValidLevel(int levelId)
    {
        return GetLevelConfig(levelId) != null;
    }

    /// <summary>
    /// 获取关卡星级
    /// </summary>
    public int GetLevelStars(int levelId)
    {
        return GameManager.Instance.GetLevelStars(levelId);
    }

    /// <summary>
    /// 计算关卡奖励
    /// </summary>
    public int CalculateReward(int levelId, float tearPercent)
    {
        var config = GetLevelConfig(levelId);
        if (config == null) return 0;

        int baseReward = config.baseCoins;
        int starBonus = 0;

        // 根据撕裂比例计算星级和额外奖励
        if (tearPercent >= 1.0f)
            starBonus = baseReward * 2; // 3星满奖励
        else if (tearPercent >= 0.9f)
            starBonus = baseReward;    // 2星
        else if (tearPercent >= config.targetTearPercent)
            starBonus = baseReward / 2; // 1星

        return baseReward + starBonus;
    }

    /// <summary>
    /// 完成关卡
    /// </summary>
    public void CompleteLevel(float tearPercent)
    {
        int stars = CalculateStars(tearPercent);
        GameManager.Instance.CompleteLevel(currentLevelId, stars);
        OnLevelStarUpdated?.Invoke(currentLevelId, stars);
    }

    /// <summary>
    /// 根据撕裂比例计算星级
    /// </summary>
    public int CalculateStars(float tearPercent)
    {
        var config = currentLevelConfig;
        if (config == null) return 1;

        if (tearPercent >= 1.0f)
            return 3;
        else if (tearPercent >= 0.9f)
            return 2;
        else if (tearPercent >= config.targetTearPercent)
            return 1;

        return 0; // 未达成最低目标
    }

    /// <summary>
    /// 获取章节数量
    /// </summary>
    public int GetChapterCount()
    {
        return chapters != null ? chapters.Length : 0;
    }

    /// <summary>
    /// 获取指定章节的关卡数量
    /// </summary>
    public int GetLevelCountInChapter(int chapterId)
    {
        var chapter = GetChapterConfig(chapterId);
        return chapter?.levels.Length ?? 0;
    }

    /// <summary>
    /// 获取章节配置
    /// </summary>
    public ChapterConfig GetChapterConfig(int chapterId)
    {
        if (chapters == null || chapters.Length == 0)
            return null;

        foreach (var chapter in chapters)
        {
            if (chapter.chapterId == chapterId)
                return chapter;
        }
        return null;
    }

    /// <summary>
    /// 获取当前关卡ID
    /// </summary>
    public int GetCurrentLevelId()
    {
        return currentLevelId;
    }

    /// <summary>
    /// 检查关卡是否解锁
    /// </summary>
    public bool IsLevelUnlocked(int levelId)
    {
        return GameManager.Instance.IsLevelUnlocked(levelId);
    }

    /// <summary>
    /// 重试当前关卡
    /// </summary>
    public void RetryLevel()
    {
        LoadLevel(currentLevelId);
    }

    /// <summary>
    /// 返回关卡选择
    /// </summary>
    public void ReturnToLevelSelect()
    {
        GameManager.Instance.SetGameState(GameState.LevelSelect);
        SceneManager.LoadSceneAsync("LevelSelectScene", LoadSceneMode.Single);
    }
}
