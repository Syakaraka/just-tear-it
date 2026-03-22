using UnityEngine;
using System;

/// <summary>
/// 音频管理器
/// 统一管理游戏中的所有音频播放
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("AudioManager");
                var newInstance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
                instance = newInstance;
            }
            return instance;
        }
    }

    [Header("音量配置")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;

    [Header("音频源")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("撕裂音效")]
    [SerializeField] private AudioClip[] tearSounds;           // 轻/中/重撕裂音效
    [SerializeField] private AudioClip tearCompleteSound;         // 撕裂完成音效
    [SerializeField] private AudioClip[] swipeSounds;           // 滑动音效

    [Header("UI音效")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip levelSelectSound;
    [SerializeField] private AudioClip popSound;

    [Header("背景音乐")]
    [SerializeField] private AudioClip[] bgMusic;

    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            UpdateVolumes();
        }
    }

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            UpdateVolumes();
        }
    }

    public float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            UpdateVolumes();
        }
    }

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
            return;
        }

        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }
    }

    /// <summary>
    /// 播放撕裂音效（根据撕裂速度）
    /// </summary>
    public void PlayTearSound(float tearSpeed)
    {
        if (tearSounds == null || tearSounds.Length == 0) return;

        // 根据撕裂速度选择音效
        float normalizedSpeed = Mathf.Clamp01(tearSpeed / 10f);
        int index = Mathf.Min(Mathf.FloorToInt(normalizedSpeed * tearSounds.Length), tearSounds.Length - 1);

        sfxSource.PlayOneShot(tearSounds[index], sfxVolume * normalizedSpeed);
    }

    /// <summary>
    /// 播放撕裂完成音效
    /// </summary>
    public void PlayTearCompleteSound()
    {
        if (tearCompleteSound != null)
        {
            sfxSource.PlayOneShot(tearCompleteSound, sfxVolume);
        }
    }

    /// <summary>
    /// 播放滑动音效
    /// </summary>
    public void PlaySwipeSound()
    {
        if (swipeSounds == null || swipeSounds.Length == 0) return;

        int index = UnityEngine.Random.Range(0, swipeSounds.Length);
        sfxSource.PlayOneShot(swipeSounds[index], sfxVolume * 0.3f);
    }

    /// <summary>
    /// 播放按钮点击音效
    /// </summary>
    public void PlayButtonClick()
    {
        if (buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound, sfxVolume * 0.5f);
        }
    }

    /// <summary>
    /// 播放关卡选择音效
    /// </summary>
    public void PlayLevelSelect()
    {
        if (levelSelectSound != null)
        {
            sfxSource.PlayOneShot(levelSelectSound, sfxVolume * 0.6f);
        }
    }

    /// <summary>
    /// 播放弹出音效
    /// </summary>
    public void PlayPopSound()
    {
        if (popSound != null)
        {
            sfxSource.PlayOneShot(popSound, sfxVolume * 0.7f);
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBackgroundMusic(int index = 0)
    {
        if (bgMusic == null || bgMusic.Length == 0) return;

        if (index >= bgMusic.Length) index = 0;

        musicSource.clip = bgMusic[index];
        musicSource.Play();
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBackgroundMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBackgroundMusic()
    {
        musicSource.Pause();
    }

    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeBackgroundMusic()
    {
        musicSource.UnPause();
    }

    /// <summary>
    /// 从PlayerPrefs加载音量设置
    /// </summary>
    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        UpdateVolumes();
    }

    /// <summary>
    /// 保存音量设置到PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
}
