using UnityEngine;

/// <summary>
/// 贴纸撕裂物体
/// 特点：任意方向撕裂，无方向性限制
/// </summary>
public class StickerTearable : TearableObject
{
    [Header("贴纸配置")]
    [SerializeField] private bool allowMultiDirection = true;  // 允许多方向撕裂
    [SerializeField] private float peelResistance = 0.3f;       // 剥离阻力

    private Vector2[] tearDirections;
    private int tearCount = 0;

    protected override void Start()
    {
        base.Start();

        // 初始化撕裂方向数组
        tearDirections = new Vector2[8]; // 最多记录8个撕裂点
    }

    public override float Tear(Vector2 tearDirection, float tearSpeed)
    {
        if (IsTorn) return 0f;

        // 贴纸可以任意方向撕裂
        float tearDelta = (tearSpeed * tearStrength) / (tearResistance + peelResistance) * Time.deltaTime;

        // 如果允许多方向撕裂，累积撕裂效果
        if (allowMultiDirection)
        {
            // 记录撕裂方向
            if (tearCount < tearDirections.Length)
            {
                tearDirections[tearCount++] = tearDirection;
            }

            // 多方向撕裂效果衰减
            float multiDirectionBonus = 1f + (tearCount * 0.05f);
            tearDelta *= multiDirectionBonus;
        }

        tornAmount = Mathf.Clamp01(tornAmount + tearDelta);

        UpdateVisuals();
        OnTornChanged?.Invoke(tornAmount);

        return tearDelta;
    }

    /// <summary>
    /// 获得撕裂点数（用于成就系统等）
    /// </summary>
    public int GetTearCount()
    {
        return tearCount;
    }

    /// <summary>
    /// 重置撕裂状态
    /// </summary>
    public override void ResetTear()
    {
        base.ResetTear();
        tearCount = 0;
        tearDirections = new Vector2[8];
    }

    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();

        // 贴纸撕裂视觉效果：边缘卷曲
        if (tornAmount > 0.3f && TryGetComponent<Renderer>(out var renderer))
        {
            // 模拟贴纸边缘卷起的效果
            // 可以通过修改UV或顶点来实现
        }
    }
}
