using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// 撕裂管理器
/// 核心玩法：处理手指滑动撕裂物体的逻辑
/// </summary>
public class TearManager : MonoBehaviour
{
    [Header("撕裂配置")]
    [SerializeField] private float tearThreshold = 0.05f;       // 触发撕裂的最小移动距离
    [SerializeField] private float tearSpeed = 5f;              // 基础撕裂速度
    [SerializeField] private float minSwipeSpeed = 0.1f;        // 最小滑动速度
    [SerializeField] private float maxSwipeSpeed = 20f;        // 最大滑动速度（用于音效音量计算）
    [SerializeField] private float tearWidth = 0.05f;          // 撕裂宽度

    [Header("撕裂物体")]
    [SerializeField] private List<TearableObject> tearableObjects = new List<TearableObject>();

    [Header("效果配置")]
    [SerializeField] private ParticleSystem tearParticleSystem; // 撕裂粒子系统
    [SerializeField] private AudioClip[] tearSounds;            // 撕裂音效数组（多种力度）
    [SerializeField] private AudioClip tearCompleteSound;      // 撕裂完成音效

    [Header("震动配置")]
    [SerializeField] private bool enableVibration = true;       // 是否启用震动
    [SerializeField] private float vibrationIntensity = 0.5f;   // 震动强度
    [SerializeField] private float vibrationDuration = 0.05f;    // 震动持续时间

    [Header("有效区域判定")]
    [SerializeField] private LayerMask tearableLayerMask;       // 可撕裂物体所在层
    [SerializeField] private float screenEdgeMargin = 50f;      // 屏幕边缘无效区域

    private Camera mainCamera;
    private AudioSource audioSource;
    private bool isTouchActive = false;
    private Vector2 lastTouchPosition;
    private Vector2 currentSwipeVelocity;
    private List<Vector2> tearPath = new List<Vector2>();
    private float lastTouchTime;
    private Vector2 lastSwipeDirection;

    // 撕裂进度
    private float totalTornAmount = 0f;
    private float targetAmount = 1f;

    // 回调
    public System.Action<float> OnTearProgressChanged;
    public System.Action OnTearComplete;
    public System.Action<float> OnTearSpeedChanged; // 用于音效音量和震动强度

    public float TearProgress => totalTornAmount;
    public float CurrentSwipeSpeed => currentSwipeVelocity.magnitude;
    public bool IsTouchActive => isTouchActive;

    private void Awake()
    {
        mainCamera = Camera.main;
        SetupAudioSource();
    }

    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D声音
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        HandleTouchInput();
        UpdateSwipeVelocity();
    }

    /// <summary>
    /// 更新滑动速度
    /// </summary>
    private void UpdateSwipeVelocity()
    {
        if (isTouchActive && tearPath.Count >= 2)
        {
            Vector2 recentDirection = tearPath[tearPath.Count - 1] - tearPath[Mathf.Max(0, tearPath.Count - 5)];
            float timeDelta = Time.deltaTime * 5; // 近似计算
            if (timeDelta > 0)
            {
                currentSwipeVelocity = recentDirection / timeDelta;
            }
        }
        else
        {
            currentSwipeVelocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 处理触控输入
    /// </summary>
    private void HandleTouchInput()
    {
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];

            switch (touch.phase)
            {
                case UnityEngine.InputSystem.EnhancedTouch.TouchPhase.Began:
                    if (IsInValidArea(touch.screenPosition))
                    {
                        OnTouchStart(touch.screenPosition);
                    }
                    break;

                case UnityEngine.InputSystem.EnhancedTouch.TouchPhase.Moved:
                case UnityEngine.InputSystem.EnhancedTouch.TouchPhase.Stationary:
                    OnTouchMove(touch.screenPosition);
                    break;

                case UnityEngine.InputSystem.EnhancedTouch.TouchPhase.Ended:
                case UnityEngine.InputSystem.EnhancedTouch.TouchPhase.Canceled:
                    OnTouchEnd();
                    break;
            }
        }
    }

    /// <summary>
    /// 检查触摸点是否在有效区域内
    /// </summary>
    private bool IsInValidArea(Vector2 screenPosition)
    {
        // 屏幕边缘区域判定
        if (screenPosition.x < screenEdgeMargin ||
            screenPosition.x > Screen.width - screenEdgeMargin ||
            screenPosition.y < screenEdgeMargin ||
            screenPosition.y > Screen.height - screenEdgeMargin)
        {
            return false;
        }
        return true;
    }

    private void OnTouchStart(Vector2 position)
    {
        isTouchActive = true;
        lastTouchPosition = position;
        lastTouchTime = Time.time;
        tearPath.Clear();
        tearPath.Add(position);
        currentSwipeVelocity = Vector2.zero;
    }

    private void OnTouchMove(Vector2 position)
    {
        if (!isTouchActive) return;

        Vector2 currentPos = position;
        float distance = Vector2.Distance(currentPos, lastTouchPosition);

        // 计算当前滑动速度
        float timeDelta = Time.time - lastTouchTime;
        if (timeDelta > 0)
        {
            Vector2 swipeDir = (currentPos - lastTouchPosition) / timeDelta;
            currentSwipeVelocity = Vector2.Lerp(currentSwipeVelocity, swipeDir, 0.5f);
        }

        // 距离足够进行撕裂检测
        if (distance > tearThreshold)
        {
            // 检查滑动速度是否足够
            float swipeSpeed = currentSwipeVelocity.magnitude;
            if (swipeSpeed >= minSwipeSpeed)
            {
                // 检测撕裂
                CheckTear(lastTouchPosition, currentPos);
            }

            lastTouchPosition = currentPos;
            lastTouchTime = Time.time;
            tearPath.Add(currentPos);
        }
    }

    private void OnTouchEnd()
    {
        isTouchActive = false;
        currentSwipeVelocity = Vector2.zero;
    }

    /// <summary>
    /// 检测撕裂
    /// </summary>
    private void CheckTear(Vector2 start, Vector2 end)
    {
        // 使用2D射线检测（针对2D撕裂物体）
        Ray ray = mainCamera.ScreenPointToRay(start);
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, (end - start).normalized, Vector2.Distance(start, end), tearableLayerMask);

        foreach (var hit in hits)
        {
            TearableObject tearable = hit.collider.GetComponent<TearableObject>();
            if (tearable != null && !tearable.IsTorn)
            {
                // 计算撕裂方向
                Vector2 tearDir = (end - start).normalized;

                // 执行撕裂
                float tearAmount = tearable.Tear(tearDir, tearSpeed);
                totalTornAmount += tearAmount;

                // 触发效果
                PlayTearEffect(hit.point, tearDir);

                // 检查完成度
                CheckCompletion();

                // 更新进度回调
                OnTearProgressChanged?.Invoke(totalTornAmount);

                // 速度回调
                OnTearSpeedChanged?.Invoke(currentSwipeVelocity.magnitude);
            }
        }

        // 如果没有2D碰撞体，回退到3D检测
        if (hits.Length == 0)
        {
            RaycastHit[] hits3D = Physics.RaycastAll(ray);
            foreach (var hit in hits3D)
            {
                TearableObject tearable = hit.collider.GetComponent<TearableObject>();
                if (tearable != null && !tearable.IsTorn)
                {
                    Vector2 tearDir = (end - start).normalized;
                    float tearAmount = tearable.Tear(tearDir, tearSpeed);
                    totalTornAmount += tearAmount;
                    PlayTearEffect(hit.point, tearDir);
                    CheckCompletion();
                    OnTearProgressChanged?.Invoke(totalTornAmount);
                    OnTearSpeedChanged?.Invoke(currentSwipeVelocity.magnitude);
                }
            }
        }
    }

    /// <summary>
    /// 播放撕裂效果
    /// </summary>
    private void PlayTearEffect(Vector3 position, Vector2 direction)
    {
        // 播放粒子效果
        PlayTearParticles(position, direction);

        // 播放撕裂音效
        PlayTearSound();

        // 触发震动反馈
        TriggerVibration();
    }

    /// <summary>
    /// 播放撕裂粒子
    /// </summary>
    private void PlayTearParticles(Vector3 position, Vector2 direction)
    {
        if (tearParticleSystem != null)
        {
            // 设置粒子发射位置和方向
            var main = tearParticleSystem.main;
            main.startSpeed = Mathf.Lerp(2f, 5f, currentSwipeVelocity.magnitude / maxSwipeSpeed);

            // 发射方向
            tearParticleSystem.transform.position = position;
            tearParticleSystem.Emit(5); // 发射粒子数量
        }
    }

    /// <summary>
    /// 播放撕裂音效（根据速度调整音量）
    /// </summary>
    private void PlayTearSound()
    {
        if (tearSounds == null || tearSounds.Length == 0) return;

        // 根据速度选择音效
        int soundIndex = Mathf.Clamp(
            Mathf.FloorToInt((currentSwipeVelocity.magnitude / maxSwipeSpeed) * tearSounds.Length),
            0,
            tearSounds.Length - 1
        );

        AudioClip clip = tearSounds[soundIndex];
        if (clip != null)
        {
            // 根据速度调整音量
            float volume = Mathf.Lerp(0.3f, 1f, currentSwipeVelocity.magnitude / maxSwipeSpeed);
            audioSource.PlayOneShot(clip, volume);
        }
    }

    /// <summary>
    /// 触发震动反馈
    /// </summary>
    private void TriggerVibration()
    {
        if (!enableVibration) return;

#if UNITY_ANDROID || UNITY_IOS
        // 根据撕裂速度计算震动强度
        float intensity = Mathf.Lerp(0.1f, vibrationIntensity, currentSwipeVelocity.magnitude / maxSwipeSpeed);

        // Android震动
        Handheld.Vibrate();

        // 发送震动事件供其他系统使用
        // Android原生震动需要通过JNI调用，这里使用Unity的简化API
#endif
    }

    /// <summary>
    /// 播放撕裂完成音效
    /// </summary>
    public void PlayCompleteSound()
    {
        if (tearCompleteSound != null)
        {
            audioSource.PlayOneShot(tearCompleteSound);
        }
    }

    /// <summary>
    /// 检查是否完成撕裂目标
    /// </summary>
    private void CheckCompletion()
    {
        if (totalTornAmount >= targetAmount)
        {
            PlayCompleteSound();
            OnTearComplete?.Invoke();
        }
    }

    /// <summary>
    /// 设置撕裂目标
    /// </summary>
    public void SetTargetAmount(float amount)
    {
        targetAmount = Mathf.Clamp01(amount);
    }

    /// <summary>
    /// 重置撕裂状态
    /// </summary>
    public void ResetTear()
    {
        totalTornAmount = 0f;
        tearPath.Clear();
        currentSwipeVelocity = Vector2.zero;
    }

    /// <summary>
    /// 添加可撕裂物体
    /// </summary>
    public void AddTearableObject(TearableObject obj)
    {
        if (!tearableObjects.Contains(obj))
        {
            tearableObjects.Add(obj);
        }
    }

    /// <summary>
    /// 移除可撕裂物体
    /// </summary>
    public void RemoveTearableObject(TearableObject obj)
    {
        tearableObjects.Remove(obj);
    }

    /// <summary>
    /// 获取撕裂轨迹
    /// </summary>
    public List<Vector2> GetTearPath()
    {
        return new List<Vector2>(tearPath);
    }

    /// <summary>
    /// 设置震动启用状态
    /// </summary>
    public void SetVibrationEnabled(bool enabled)
    {
        enableVibration = enabled;
    }

    /// <summary>
    /// 获取当前滑动方向
    /// </summary>
    public Vector2 GetCurrentSwipeDirection()
    {
        if (tearPath.Count < 2) return Vector2.zero;
        return (tearPath[tearPath.Count - 1] - tearPath[0]).normalized;
    }
}
