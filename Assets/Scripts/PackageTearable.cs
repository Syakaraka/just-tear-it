using UnityEngine;

/// <summary>
/// 包装撕裂物体
/// 特点：需要沿预设路径撕裂，模拟开箱体验
/// </summary>
public class PackageTearable : TearableObject
{
    [Header("包装配置")]
    [SerializeField] private Transform[] tearPathPoints;       // 撕裂路径点
    [SerializeField] private float pathThreshold = 0.5f;         // 路径判定阈值
    [SerializeField] private bool followPathRequired = true;      // 是否需要沿路径撕裂

    [Header("撕裂分段")]
    [SerializeField] private int totalSegments = 3;              // 总分段数
    private int currentSegment = 0;

    private Vector2 currentTargetPoint;

    protected override void Start()
    {
        base.Start();

        if (tearPathPoints != null && tearPathPoints.Length > 0)
        {
            // 设置初始目标点
            currentTargetPoint = tearPathPoints[0].position;
        }
    }

    public override float Tear(Vector2 tearDirection, float tearSpeed)
    {
        if (IsTorn) return 0f;

        float tearDelta = 0f;

        if (followPathRequired && tearPathPoints != null)
        {
            // 检查是否在正确的撕裂路径上
            bool onPath = CheckPathProgress(tearDirection);

            if (onPath)
            {
                // 沿路径撕裂，正常效率
                tearDelta = (tearSpeed * tearStrength) / tearResistance * Time.deltaTime;
                currentSegment++;
            }
            else
            {
                // 偏离路径，效率很低
                tearDelta = (tearSpeed * tearStrength) / tearResistance * tearDirection.magnitude * 0.1f * Time.deltaTime;
            }
        }
        else
        {
            // 无需沿路径撕裂
            tearDelta = (tearSpeed * tearStrength) / tearResistance * Time.deltaTime;
        }

        tornAmount = Mathf.Clamp01(tornAmount + tearDelta);

        UpdateVisuals();
        OnTornChanged?.Invoke(tornAmount);

        return tearDelta;
    }

    /// <summary>
    /// 检查撕裂进度是否符合路径
    /// </summary>
    private bool CheckPathProgress(Vector2 tearDirection)
    {
        if (currentSegment >= tearPathPoints.Length - 1)
            return true; // 已到最后一段

        Vector2 targetPoint = tearPathPoints[currentSegment + 1].position;
        Vector2 currentPos = tearPathPoints[currentSegment].position;

        // 计算到目标点的方向
        Vector2 toTarget = (targetPoint - currentPos).normalized;
        float angle = Vector2.Angle(tearDirection, toTarget);

        return angle <= maxDeviationAngle;
    }

    /// <summary>
    /// 获取当前撕裂分段
    /// </summary>
    public int GetCurrentSegment()
    {
        return currentSegment;
    }

    /// <summary>
    /// 获取总分段数
    /// </summary>
    public int GetTotalSegments()
    {
        return totalSegments;
    }

    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();

        // 包装撕裂视觉效果：显示撕裂进度线
        if (tornAmount > 0f && TryGetComponent<Renderer>(out var renderer))
        {
            // 显示当前撕裂位置
            float progress = tornAmount;
            // 可以通过shader或纹理偏移来显示撕裂进度
        }
    }
}
