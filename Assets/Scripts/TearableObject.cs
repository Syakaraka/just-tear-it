using UnityEngine;
using System;

/// <summary>
/// 可撕裂物体基类
/// </summary>
public class TearableObject : MonoBehaviour
{
    [Header("撕裂配置")]
    [SerializeField] protected float tearStrength = 1f;      // 撕裂强度
    [SerializeField] protected float tearResistance = 0.5f;    // 撕裂阻力
    [SerializeField] protected float tornAmount = 0f;           // 已撕裂比例

    [Header("撕裂阈值")]
    [SerializeField] protected float tearThreshold = 0.1f;      // 单次撕裂阈值
    [SerializeField] protected float maxTearDistance = 10f;   // 最大撕裂距离

    [Header("视觉配置")]
    [SerializeField] protected Material tornMaterial;           // 撕裂后的材质
    [SerializeField] protected GameObject tornEdgePrefab;       // 撕裂边缘特效

    // 状态
    public bool IsTorn => tornAmount >= 1f;
    public float TornAmount => tornAmount;

    // 事件
    public event Action<float> OnTornChanged;

    /// <summary>
    /// 执行撕裂
    /// </summary>
    /// <param name="tearDirection">撕裂方向</param>
    /// <param name="tearSpeed">撕裂速度</param>
    /// <returns>撕裂量(0-1)</returns>
    public virtual float Tear(Vector2 tearDirection, float tearSpeed)
    {
        if (IsTorn) return 0f;

        // 计算撕裂效果
        float tearEffect = (tearSpeed * tearStrength) / tearResistance;
        float tearDelta = tearEffect * Time.deltaTime;

        tornAmount = Mathf.Clamp01(tornAmount + tearDelta);
        UpdateVisuals();

        OnTornChanged?.Invoke(tornAmount);

        return tearDelta;
    }

    /// <summary>
    /// 更新视觉效果
    /// </summary>
    protected virtual void UpdateVisuals()
    {
        if (tornMaterial != null && GetComponent<Renderer>() != null)
        {
            // 混合撕裂程度到材质
            GetComponent<Renderer>().material.Lerp(GetComponent<Renderer>().sharedMaterial, tornMaterial, tornAmount);
        }
    }

    /// <summary>
    /// 完全撕裂
    /// </summary>
    public virtual void FullyTear()
    {
        tornAmount = 1f;
        UpdateVisuals();
        PlayTornEffect();
    }

    /// <summary>
    /// 播放撕裂完成特效
    /// </summary>
    protected void PlayTornEffect()
    {
        if (tornEdgePrefab != null)
        {
            Instantiate(tornEdgePrefab, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// 重置撕裂状态
    /// </summary>
    public virtual void ResetTear()
    {
        tornAmount = 0f;
        UpdateVisuals();
    }
}
