using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// 撕裂管理器
/// 核心玩法：处理手指滑动撕裂物体的逻辑
/// </summary>
public class TearManager : MonoBehaviour
{
    [Header("撕裂配置")]
    [SerializeField] private float tearThreshold = 0.1f;      // 撕裂阈值
    [SerializeField] private float tearSpeed = 5f;              // 撕裂速度
    [SerializeField] private float tearWidth = 0.05f;          // 撕裂宽度

    [Header("撕裂物体")]
    [SerializeField] private List<TearableObject> tearableObjects = new List<TearableObject>();

    [Header("效果配置")]
    [SerializeField] private GameObject tearParticlePrefab;     // 撕裂粒子特效
    [SerializeField] private AudioClip tearSound;               // 撕裂音效

    private Camera mainCamera;
    private AudioSource audioSource;
    private bool isTouchActive = false;
    private Vector2 lastTouchPosition;
    private List<Vector2> tearPath = new List<Vector2>();

    // 撕裂进度
    private float totalTornAmount = 0f;
    private float targetAmount = 1f;

    // 回调
    public System.Action<float> OnTearProgressChanged;
    public System.Action OnTearComplete;

    public float TearProgress => totalTornAmount;

    private void Awake()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        // 启用EnhancedTouch
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        HandleTouchInput();
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
                    OnTouchStart(touch.screenPosition);
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

    private void OnTouchStart(Vector2 position)
    {
        isTouchActive = true;
        lastTouchPosition = position;
        tearPath.Clear();
        tearPath.Add(position);
    }

    private void OnTouchMove(Vector2 position)
    {
        if (!isTouchActive) return;

        Vector2 currentPos = position;
        float distance = Vector2.Distance(currentPos, lastTouchPosition);

        // 距离足够进行撕裂检测
        if (distance > tearThreshold)
        {
            // 检测撕裂
            CheckTear(lastTouchPosition, currentPos);

            lastTouchPosition = currentPos;
            tearPath.Add(currentPos);
        }
    }

    private void OnTouchEnd()
    {
        isTouchActive = false;
    }

    /// <summary>
    /// 检测撕裂
    /// </summary>
    private void CheckTear(Vector2 start, Vector2 end)
    {
        Ray ray = mainCamera.ScreenPointToRay(start);
        RaycastHit[] hits = Physics.RaycastAll(ray);

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
            }
        }
    }

    /// <summary>
    /// 播放撕裂效果
    /// </summary>
    private void PlayTearEffect(Vector3 position, Vector2 direction)
    {
        // 播放粒子
        if (tearParticlePrefab != null)
        {
            var particle = Instantiate(tearParticlePrefab, position, Quaternion.LookRotation(direction));
            Destroy(particle, 1f);
        }

        // 播放音效
        if (tearSound != null)
        {
            audioSource.PlayOneShot(tearSound);
        }
    }

    /// <summary>
    /// 检查是否完成撕裂目标
    /// </summary>
    private void CheckCompletion()
    {
        if (totalTornAmount >= targetAmount)
        {
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
}
