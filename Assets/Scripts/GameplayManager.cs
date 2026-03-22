using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 游戏场景管理器
/// 负责游戏核心玩法流程控制
/// </summary>
public class GameplayManager : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Image progressBar;           // 撕裂进度条
    [SerializeField] private Text progressText;            // 进度文字
    [SerializeField] private Button pauseButton;           // 暂停按钮
    [SerializeField] private Button retryButton;           // 重试按钮
    [SerializeField] private GameObject resultPanel;      // 结果面板
    [SerializeField] private Text resultStarsText;         // 结果星级文字
    [SerializeField] private Text resultCoinsText;        // 结果金币文字

    [Header("游戏配置")]
    [SerializeField] private float targetTearPercent = 0.8f;  // 目标撕裂比例

    [Header("场景配置")]
    [SerializeField] private string levelSelectScene = "LevelSelectScene";

    private TearManager tearManager;
    private LevelManager levelManager;
    private bool isGameActive = false;
    private bool isPaused = false;

    private void Awake()
    {
        // 初始化组件
        tearManager = FindObjectOfType<TearManager>();
        levelManager = FindObjectOfType<LevelManager>();
    }

    private void Start()
    {
        // 设置游戏状态
        GameManager.Instance.SetGameState(GameState.Playing);

        // 初始化UI
        InitUI();

        // 绑定事件
        BindEvents();

        // 开始游戏
        StartGame();
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitUI()
    {
        // 隐藏结果面板
        if (resultPanel != null)
            resultPanel.SetActive(false);

        // 更新进度显示
        UpdateProgress(0f);
    }

    /// <summary>
    /// 绑定事件
    /// </summary>
    private void BindEvents()
    {
        // 撕裂进度回调
        if (tearManager != null)
        {
            tearManager.OnTearProgressChanged += UpdateProgress;
            tearManager.OnTearComplete += OnTearComplete;
        }

        // 暂停按钮
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        // 重试按钮
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryLevel);
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartGame()
    {
        isGameActive = true;
        isPaused = false;

        // 重置撕裂状态
        if (tearManager != null)
        {
            tearManager.ResetTear();
        }
    }

    /// <summary>
    /// 更新进度显示
    /// </summary>
    private void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = Mathf.Clamp01(progress);
        }

        if (progressText != null)
        {
            int percent = Mathf.RoundToInt(progress * 100);
            progressText.text = $"{percent}%";
        }
    }

    /// <summary>
    /// 撕裂完成回调
    /// </summary>
    private void OnTearComplete()
    {
        if (!isGameActive) return;

        isGameActive = false;

        // 计算结果
        float tearPercent = tearManager != null ? tearManager.TearProgress : 1f;
        int stars = levelManager != null ? levelManager.CalculateStars(tearPercent) : 3;
        int coins = levelManager != null ? levelManager.CalculateReward(GetCurrentLevelId(), tearPercent) : 30;

        // 记录成绩
        levelManager?.CompleteLevel(tearPercent);

        // 显示结果
        ShowResult(stars, coins);
    }

    /// <summary>
    /// 获取当前关卡ID
    /// </summary>
    private int GetCurrentLevelId()
    {
        return levelManager != null ? levelManager.GetCurrentLevelId() : 1;
    }

    /// <summary>
    /// 显示结果面板
    /// </summary>
    private void ShowResult(int stars, int coins)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultStarsText != null)
        {
            resultStarsText.text = $"{stars} Stars";
        }

        if (resultCoinsText != null)
        {
            resultCoinsText.text = $"+{coins}";
        }

        GameManager.Instance.SetGameState(GameState.Result);
    }

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            GameManager.Instance.SetGameState(GameState.Paused);
        }
        else
        {
            Time.timeScale = 1f;
            GameManager.Instance.SetGameState(GameState.Playing);
        }
    }

    /// <summary>
    /// 重试关卡
    /// </summary>
    private void RetryLevel()
    {
        Time.timeScale = 1f;
        levelManager?.RetryLevel();
    }

    /// <summary>
    /// 返回关卡选择
    /// </summary>
    public void ReturnToLevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(levelSelectScene, LoadSceneMode.Single);
    }

    /// <summary>
    /// 下一关
    /// </summary>
    public void NextLevel()
    {
        int nextLevelId = GetCurrentLevelId() + 1;
        if (levelManager != null && levelManager.IsValidLevel(nextLevelId))
        {
            levelManager.LoadLevel(nextLevelId);
        }
        else
        {
            ReturnToLevelSelect();
        }
    }

    private void OnDestroy()
    {
        // 取消事件绑定
        if (tearManager != null)
        {
            tearManager.OnTearProgressChanged -= UpdateProgress;
            tearManager.OnTearComplete -= OnTearComplete;
        }

        Time.timeScale = 1f;
    }
}
