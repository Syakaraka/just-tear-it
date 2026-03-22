using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 主菜单管理器
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text coinText;
    [SerializeField] private Text versionText;

    [Header("设置面板")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Button settingsCloseButton;

    [Header("场景配置")]
    [SerializeField] private string levelSelectSceneName = "LevelSelectScene";

    private SettingsManager settingsManager;

    private void Start()
    {
        // 设置游戏状态
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameState.MainMenu);
        }

        // 获取设置管理器
        settingsManager = SettingsManager.Instance;

        // 初始化UI
        InitUI();

        // 更新金币显示
        UpdateCoinDisplay();

        // 监听金币变化
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged += (total, delta) => UpdateCoinDisplay();
        }

        // 显示版本信息
        if (versionText != null)
        {
            versionText.text = $"v{Application.version}";
        }

        // 播放背景音乐
        PlayBackgroundMusic();
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitUI()
    {
        // 主按钮
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // 设置面板
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 绑定设置UI
        BindSettingsUI();
    }

    /// <summary>
    /// 绑定设置UI
    /// </summary>
    private void BindSettingsUI()
    {
        if (settingsManager == null) return;

        // 从SettingsManager获取当前值并绑定UI
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = settingsManager.MasterVolume;
            masterVolumeSlider.onValueChanged.AddListener(settingsManager.SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = settingsManager.MusicVolume;
            musicVolumeSlider.onValueChanged.AddListener(settingsManager.SetMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = settingsManager.SFXVolume;
            sfxVolumeSlider.onValueChanged.AddListener(settingsManager.SetSFXVolume);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = settingsManager.VibrationEnabled;
            vibrationToggle.onValueChanged.AddListener(settingsManager.SetVibration);
        }

        if (settingsCloseButton != null)
        {
            settingsCloseButton.onClick.AddListener(OnSettingsCloseClicked);
        }
    }

    /// <summary>
    /// 开始游戏按钮点击
    /// </summary>
    private void OnPlayClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneManager.LoadSceneAsync(levelSelectSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 设置按钮点击
    /// </summary>
    private void OnSettingsClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 设置关闭按钮点击
    /// </summary>
    private void OnSettingsCloseClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 退出按钮点击
    /// </summary>
    private void OnQuitClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        // 在编辑器中退出无效，仅用于真机
#if UNITY_ANDROID
        Application.Quit();
#endif
    }

    /// <summary>
    /// 更新金币显示
    /// </summary>
    private void UpdateCoinDisplay()
    {
        if (coinText != null && GameManager.Instance != null)
        {
            coinText.text = GameManager.Instance.Coins.ToString();
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    private void PlayBackgroundMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBackgroundMusic();
        }
    }

    private void OnDestroy()
    {
        // 取消监听
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= (total, delta) => UpdateCoinDisplay();
        }
    }
}
