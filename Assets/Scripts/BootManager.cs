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

    private float loadTimer = 0f;
    private bool isLoading = false;

    private void Start()
    {
        // 初始化游戏管理器
        InitGameManager();

        // 开始加载流程
        isLoading = true;
        loadTimer = 0f;
    }

    private void Update()
    {
        if (!isLoading) return;

        loadTimer += Time.deltaTime;

        // 等待最小加载时间后跳转
        if (loadTimer >= minLoadTime)
        {
            isLoading = false;
            LoadMainScene();
        }
    }

    /// <summary>
    /// 初始化游戏管理器（单例）
    /// </summary>
    private void InitGameManager()
    {
        // 确保GameManager单例存在
        if (GameManager.Instance == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
        }

        // 确保AdInterface单例存在
        if (AdInterface.Instance == null)
        {
            var adGo = new GameObject("AdInterface");
            adGo.AddComponent<AdInterface>();
            DontDestroyOnLoad(adGo);
        }

        // 初始化玩家数据
        GameManager.Instance.InitPlayerData();

        // 预加载广告
        AdInterface.Instance.PreloadAd();

        // 设置音频
        SetupAudio();
    }

    /// <summary>
    /// 设置音频配置
    /// </summary>
    private void SetupAudio()
    {
        // 设置背景音乐音量
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
    }

    /// <summary>
    /// 加载主场景
    /// </summary>
    private void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    /// <summary>
    /// 立即跳转（跳过加载动画）
    /// </summary>
    public void SkipLoading()
    {
        isLoading = false;
        LoadMainScene();
    }
}
