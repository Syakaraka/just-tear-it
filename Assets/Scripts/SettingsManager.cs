using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置管理器
/// 管理音量、震动等游戏设置
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Button closeButton;

    [Header("设置键名")]
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string VIBRATION_KEY = "VibrationEnabled";

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    private bool vibrationEnabled = true;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public bool VibrationEnabled => vibrationEnabled;

    private void Start()
    {
        LoadSettings();
        BindUI();
    }

    /// <summary>
    /// 加载设置
    /// </summary>
    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        vibrationEnabled = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
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
    }

    /// <summary>
    /// 绑定UI事件
    /// </summary>
    private void BindUI()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = vibrationEnabled;
            vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        masterVolume = value;
        AudioListener.volume = value;
        SaveSettings();
    }

    private void OnMusicVolumeChanged(float value)
    {
        musicVolume = value;
        // TODO: 更新背景音乐音量
        SaveSettings();
    }

    private void OnSFXVolumeChanged(float value)
    {
        sfxVolume = value;
        // TODO: 更新音效音量
        SaveSettings();
    }

    private void OnVibrationChanged(bool enabled)
    {
        vibrationEnabled = enabled;
        SaveSettings();
    }

    private void OnCloseClicked()
    {
        SaveSettings();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 重置所有设置为默认值
    /// </summary>
    public void ResetToDefaults()
    {
        masterVolume = 1f;
        musicVolume = 1f;
        sfxVolume = 1f;
        vibrationEnabled = true;

        // 更新UI
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        if (vibrationToggle != null) vibrationToggle.isOn = vibrationEnabled;

        SaveSettings();
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
}
