using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 可撕裂物体基类
/// </summary>
public class TearableObject : MonoBehaviour
{
    [Header("撕裂配置")]
    [SerializeField] protected float tearStrength = 1f;           // 撕裂强度
    [SerializeField] protected float tearResistance = 0.5f;       // 撕裂阻力
    [SerializeField] protected float tornAmount = 0f;              // 已撕裂比例

    [Header("撕裂视觉效果")]
    [SerializeField] protected Material tornMaterial;              // 撕裂后的材质
    [SerializeField] protected Color tornEdgeColor = new Color(1f, 0.8f, 0.6f); // 撕裂边缘颜色
    [SerializeField] protected float tornEdgeWidth = 0.05f;        // 撕裂边缘宽度
    [SerializeField] protected GameObject tornEdgePrefab;          // 撕裂边缘特效预制体

    [Header("Mesh撕裂配置")]
    [SerializeField] protected bool useMeshTear = true;           // 是否使用Mesh撕裂
    [SerializeField] protected int maxTearVertices = 50;           // 最大撕裂顶点数

    [Header("撕裂音效")]
    [SerializeField] protected AudioClip[] tearSounds;             // 撕裂音效数组

    // 状态
    public bool IsTorn => tornAmount >= 1f;
    public float TornAmount => tornAmount;
    public Vector2 LastTearDirection { get; private set; }

    // 事件
    public event Action<float> OnTornChanged;
    public event Action OnFullyTorn;

    // 内部状态
    protected Renderer objectRenderer;
    protected Material originalMaterial;
    protected Material instanceMaterial;
    protected List<Vector3> tearVertices = new List<Vector3>();
    protected Vector2 currentTearUV;

    protected virtual void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.sharedMaterial;
            instanceMaterial = new Material(originalMaterial);
            objectRenderer.material = instanceMaterial;
        }
    }

    /// <summary>
    /// 执行撕裂
    /// </summary>
    /// <param name="tearDirection">撕裂方向</param>
    /// <param name="tearSpeed">撕裂速度</param>
    /// <returns>撕裂量(0-1)</returns>
    public virtual float Tear(Vector2 tearDirection, float tearSpeed)
    {
        if (IsTorn) return 0f;

        LastTearDirection = tearDirection;

        // 计算撕裂效果
        float tearEffect = (tearSpeed * tearStrength) / tearResistance;
        float tearDelta = tearEffect * Time.deltaTime;

        tornAmount = Mathf.Clamp01(tornAmount + tearDelta);

        // 更新视觉效果
        UpdateVisuals();

        // 如果达到撕裂完成阈值，触发完成效果
        if (tornAmount >= 1f && tearDelta > 0)
        {
            OnFullyTorn?.Invoke();
        }

        OnTornChanged?.Invoke(tornAmount);

        return tearDelta;
    }

    /// <summary>
    /// 更新视觉效果
    /// </summary>
    protected virtual void UpdateVisuals()
    {
        if (instanceMaterial == null) return;

        // 设置撕裂进度到shader
        instanceMaterial.SetFloat("_TornAmount", tornAmount);
        instanceMaterial.SetColor("_TornEdgeColor", tornEdgeColor);
        instanceMaterial.SetFloat("_TornEdgeWidth", tornEdgeWidth);

        // 根据撕裂进度调整颜色（边缘磨损效果）
        if (tornAmount > 0.1f)
        {
            Color edgeColor = Color.Lerp(Color.white, tornEdgeColor, (tornAmount - 0.1f) / 0.9f);
            instanceMaterial.SetColor("_TearTint", edgeColor);
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
        OnFullyTorn?.Invoke();
    }

    /// <summary>
    /// 播放撕裂完成特效
    /// </summary>
    protected void PlayTornEffect()
    {
        if (tornEdgePrefab != null)
        {
            // 在撕裂中心位置生成特效
            Vector3 spawnPos = transform.position + Vector3.back * 0.1f;
            GameObject effect = Instantiate(tornEdgePrefab, spawnPos, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    /// <summary>
    /// 播放撕裂音效
    /// </summary>
    protected void PlayTearSound(float intensity = 1f)
    {
        if (tearSounds == null || tearSounds.Length == 0) return;

        // 根据撕裂量选择音效
        int index = Mathf.Clamp(
            Mathf.FloorToInt(tornAmount * tearSounds.Length),
            0,
            tearSounds.Length - 1
        );

        AudioSource.PlayClipAtPoint(tearSounds[index], transform.position, intensity);
    }

    /// <summary>
    /// 获取撕裂边缘的世界坐标
    /// </summary>
    public virtual Vector2 GetTearEdgeWorldPosition()
    {
        return transform.TransformPoint(currentTearUV);
    }

    /// <summary>
    /// 重置撕裂状态
    /// </summary>
    public virtual void ResetTear()
    {
        tornAmount = 0f;
        tearVertices.Clear();

        if (instanceMaterial != null && originalMaterial != null)
        {
            instanceMaterial.CopyPropertiesFromMaterial(originalMaterial);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }

    /// <summary>
    /// 获取撕裂信息（用于调试）
    /// </summary>
    public string GetTearDebugInfo()
    {
        return $"TornAmount: {tornAmount:F2}, IsTorn: {IsTorn}, Direction: {LastTearDirection}";
    }
}
