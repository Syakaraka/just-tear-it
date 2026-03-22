using UnityEngine;

/// <summary>
/// 胶带撕裂物体
/// 特点：需要沿特定方向撕裂，有方向性限制
/// </summary>
public class TapeTearable : TearableObject
{
    [Header("胶带配置")]
    [SerializeField] private Vector2 preferredTearDirection = Vector2.right; // 首选撕裂方向
    [SerializeField] private float directionPenalty = 0.5f;                   // 方向错误时的惩罚倍率
    [SerializeField] private float maxDeviationAngle = 45f;                   // 最大偏离角度

    private Vector2 currentTearStart;

    /// <summary>
    /// 检查撕裂方向是否有效
    /// </summary>
    private float GetDirectionMultiplier(Vector2 tearDir)
    {
        float angle = Vector2.Angle(tearDir, preferredTearDirection);
        float normalizedAngle = angle / 180f;

        if (angle <= maxDeviationAngle)
        {
            // 方向正确，完整效率
            return 1f;
        }
        else if (angle <= 90f)
        {
            // 中等偏离，效率降低
            return Mathf.Lerp(1f, directionPenalty, (angle - maxDeviationAngle) / (90f - maxDeviationAngle));
        }
        else
        {
            // 严重偏离，极低效率
            return directionPenalty;
        }
    }

    public override float Tear(Vector2 tearDirection, float tearSpeed)
    {
        if (IsTorn) return 0f;

        float directionMultiplier = GetDirectionMultiplier(tearDirection);
        float effectiveSpeed = tearSpeed * directionMultiplier;

        float tearDelta = (effectiveSpeed * tearStrength) / tearResistance * Time.deltaTime;
        tornAmount = Mathf.Clamp01(tornAmount + tearDelta);

        UpdateVisuals();
        OnTornChanged?.Invoke(tornAmount);

        return tearDelta * directionMultiplier;
    }

    /// <summary>
    /// 设置首选撕裂方向
    /// </summary>
    public void SetPreferredDirection(Vector2 direction)
    {
        preferredTearDirection = direction.normalized;
    }

    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();

        // 胶带撕裂时的视觉效果
        if (TryGetComponent<Renderer>(out var renderer))
        {
            // 可以在这里添加撕裂边缘的视觉效果
            float edgeWear = tornAmount * 0.5f;
            // 模拟撕裂边缘的磨损效果
        }
    }
}
