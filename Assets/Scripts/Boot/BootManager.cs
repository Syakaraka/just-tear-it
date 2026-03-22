using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 启动场景管理器
/// 负责初始化游戏资源、加载数据并跳转到主场景
/// </summary>
public class BootManager : MonoBehaviour
{
    [Header("场景配置")]
    [SerializeField] private string mainSceneName = "MainScene";

    [Header("加载配置")]
    [SerializeField] private float minLoadTime = 1.5f;

    [Header("初始化配置")]
    [SerializeField] private bool initializeAudio = true;
    [SerializeField] private bool initializeAnalytics = true;

    private float loadTimer = 0f;
    private bool isLoading = false;
    private bool initializationComplete = false;

    private void Start()
    {
        // 开始初始化流程
        StartInitialization();
    }

    private void Update()
    {
        if (!isLoading) return;

        loadTimer += Time.deltaTime;

        // 等待最小加载时间后跳转
        if (loadTimer >= minLoadTime && initializationComplete)
        {
            isLoading = false;
            LoadMainScene();
        }
    }

    /// <summary>
    /// 开始初始化流程
    /// </summary>
    private void StartInitialization()
    {
        isLoading = true;
        loadTimer = 0f;
        initializationComplete = false;

        // 初始化游戏管理器（单例）
        InitGameManager();

        // 初始化音频
        if (initializeAudio)
        {
            InitAudio();
        }

        // 初始化数据分析
        if (initializeAnalytics)
        {
            InitAnalytics();
        }

        // 初始化设置管理器
        InitSettings();

        // 标记初始化完成
        initializationComplete = true;
    }

    /// <summary>
    /// 初始化游戏管理器
    /// </summary>
    private void InitGameManager()
    {
        if (GameManager.Instance == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
        }

        // 初始化玩家数据
        GameManager.Instance.InitPlayerData();

        Debug.Log("[Boot] GameManager initialized");
    }

    /// <summary>
    /// 初始化音频系统
    /// </summary>
    private void InitAudio()
    {
        if (AudioManager.Instance == null)
        {
            var go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
            DontDestroyOnLoad(go);
        }

        // 加载音频设置
        AudioManager.Instance.LoadSettings();

        Debug.Log("[Boot] AudioManager initialized");
    }

    /// <summary>
    /// 初始化数据分析
    /// </summary>
    private void InitAnalytics()
    {
        if (AnalyticsManager.Instance == null)
        {
            var go = new GameObject("AnalyticsManager");
            go.AddComponent<AnalyticsManager>();
            DontDestroyOnLoad(go);
        }

        Debug.Log("[Boot] AnalyticsManager initialized");
    }

    /// <summary>
    /// 初始化设置管理器
    /// </summary>
    private void InitSettings()
    {
        if (SettingsManager.Instance == null)
        {
            var go = new GameObject("SettingsManager");
            go.AddComponent<SettingsManager>();
            DontDestroyOnLoad(go);
        }

        // 加载设置
        SettingsManager.Instance.LoadSettings();

        Debug.Log("[Boot] SettingsManager initialized");
    }

    /// <summary>
    /// 加载主场景
    /// </summary>
    private void LoadMainScene()
    {
        // 设置时间刻度
        Time.timeScale = 1f;

        // 异步加载主场景
        SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 立即跳转（跳过加载动画）
    /// </summary>
    public void SkipLoading()
    {
        isLoading = false;
        initializationComplete = true;
        LoadMainScene();
    }
}
