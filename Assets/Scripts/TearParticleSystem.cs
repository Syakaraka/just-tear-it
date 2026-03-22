using UnityEngine;

/// <summary>
/// 撕裂粒子效果组件
/// 挂载到撕裂物体上产生撕裂时的碎片效果
/// </summary>
public class TearParticleSystem : MonoBehaviour
{
    [Header("粒子配置")]
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private int particlesPerEmit = 5;
    [SerializeField] private float emissionRate = 0.1f; // 秒

    [Header("粒子属性")]
    [SerializeField] private Color particleColor = Color.white;
    [SerializeField] private float minSize = 0.02f;
    [SerializeField] private float maxSize = 0.1f;
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float lifetime = 0.5f;

    [Header("碎片形状")]
    [SerializeField] private bool useCustomShape = true;
    [SerializeField] private float shapeRadius = 0.05f;

    private float lastEmitTime = 0f;
    private ParticleSystem.EmitParams emitParams;

    private void Awake()
    {
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
        }

        if (particleSystem == null)
        {
            particleSystem = gameObject.AddComponent<ParticleSystem>();
            ConfigureParticleSystem();
        }

        emitParams = new ParticleSystem.EmitParams();
        emitParams.startColor = particleColor;
    }

    private void ConfigureParticleSystem()
    {
        var main = particleSystem.main;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startLifetime = lifetime;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // 发射形状
        var shape = particleSystem.shape;
        shape.enabled = useCustomShape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = shapeRadius;

        // 渲染设置
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        // 创建简单的白色碎片材质
        var mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.color = particleColor;
        renderer.material = mat;
    }

    /// <summary>
    /// 发射撕裂碎片
    /// </summary>
    public void Emit(Vector3 position, Vector2 direction, float intensity = 1f)
    {
        if (Time.time - lastEmitTime < emissionRate) return;

        lastEmitTime = Time.time;

        // 设置发射参数
        emitParams.startColor = Color.Lerp(particleColor, Color.white, intensity * 0.5f);
        emitParams.startSize = Mathf.Lerp(minSize, maxSize, intensity);
        emitParams.startSpeed = Mathf.Lerp(minSpeed, maxSpeed, intensity);

        // 计算发射方向
        Vector3 emitDirection = new Vector3(direction.x, direction.y, 0).normalized;
        float angle = Mathf.Atan2(emitDirection.y, emitDirection.x) * Mathf.Rad2Deg;

        // 应用旋转
        emitParams.rotation3D = new Vector3(0, 0, angle);

        // 发射粒子
        particleSystem.Emit(emitParams, particlesPerEmit);
    }

    /// <summary>
    /// 设置粒子颜色
    /// </summary>
    public void SetParticleColor(Color color)
    {
        particleColor = color;
        emitParams.startColor = color;

        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startColor = color;
        }
    }

    /// <summary>
    /// 设置发射强度
    /// </summary>
    public void SetEmissionIntensity(float intensity)
    {
        particlesPerEmit = Mathf.RoundToInt(intensity * 5);
    }
}
