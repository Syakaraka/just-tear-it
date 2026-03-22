using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置管理器
/// 管理音量、震动等游戏设置
/// </summary>
public class SettingsManager : MonoBehaviour
{
    private static SettingsManager instance;
    public static SettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("SettingsManager");
                instance = go.AddComponent<SettingsManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("UI组件")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Button closeButton;

    [Header("当前设置值")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private bool vibrationEnabled = true;

    // PlayerPrefs键名
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string VIBRATION_KEY = "VibrationEnabled";

    // 属性访问
    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public bool VibrationEnabled => vibrationEnabled;

    public event System.Action OnSettingsChanged;

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

        LoadSettings();
    }

    /// <summary>
    /// 加载设置
    /// </summary>
    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        vibrationEnabled = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;

        ApplySettings();
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.SetInt(VIBRATION_KEY, vibrationEnabled ? 1 : 0);
        PlayerPrefs.Save();

        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// 应用设置到系统
    /// </summary>
    private void ApplySettings()
    {
        AudioListener.volume = masterVolume;

        // 通知其他系统设置已更改
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// 绑定UI组件
    /// </summary>
    public void BindUI(Slider master, Slider music, Slider sfx, Toggle vibration, Button close)
    {
        masterVolumeSlider = master;
        musicVolumeSlider = music;
        sfxVolumeSlider = sfx;
        vibrationToggle = vibration;
        closeButton = close;

        // 初始化UI值
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = vibrationEnabled;
            vibrationToggle.onValueChanged.AddListener(SetVibration);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnClose);
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        AudioListener.volume = masterVolume;
        SaveSettings();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        AudioManager.Instance.MusicVolume = musicVolume;
        SaveSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        AudioManager.Instance.SFXVolume = sfxVolume;
        SaveSettings();
    }

    public void SetVibration(bool enabled)
    {
        vibrationEnabled = enabled;
        SaveSettings();
    }

    private void OnClose()
    {
        SaveSettings();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 触发震动反馈
    /// </summary>
    public void TriggerVibration()
    {
        if (!vibrationEnabled) return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// 重置为默认值
    /// </summary>
    public void ResetToDefaults()
    {
        masterVolume = 1f;
        musicVolume = 1f;
        sfxVolume = 1f;
        vibrationEnabled = true;

        ApplySettings();
        SaveSettings();

        // 更新UI
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        if (vibrationToggle != null) vibrationToggle.isOn = vibrationEnabled;
    }
}
