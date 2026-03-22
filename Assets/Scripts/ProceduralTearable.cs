using UnityEngine;

/// <summary>
/// 程序生成的占位撕裂物体
/// 用于测试撕裂效果，后续替换正式美术资源
/// </summary>
public class ProceduralTearable : TearableObject
{
    [Header("占位图形配置")]
    [SerializeField] private ShapeType shapeType = ShapeType.Rectangle;
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color stripeColor = Color.gray;
    [SerializeField] private float width = 2f;
    [SerializeField] private float height = 1f;
    [SerializeField] private bool addStripes = false;

    [Header("撕裂效果")]
    [SerializeField] private bool showTearLine = true;
    [SerializeField] private float tearLineWidth = 0.02f;

    public enum ShapeType
    {
        Rectangle,  // 矩形（胶带）
        Circle,     // 圆形（贴纸）
        RoundedRect // 圆角矩形（包装）
    }

    protected override void Awake()
    {
        base.Awake();
        GenerateMesh();
    }

    /// <summary>
    /// 程序生成Mesh
    /// </summary>
    private void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices;
        Vector2[] uvs;
        int[] triangles;

        switch (shapeType)
        {
            case ShapeType.Rectangle:
                CreateRectangleMesh(out vertices, out uvs, out triangles);
                break;
            case ShapeType.Circle:
                CreateCircleMesh(out vertices, out uvs, out triangles);
                break;
            case ShapeType.RoundedRect:
                CreateRoundedRectMesh(out vertices, out uvs, out triangles);
                break;
            default:
                CreateRectangleMesh(out vertices, out uvs, out triangles);
                break;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // 应用Mesh
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // 添加MeshRenderer
        var meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // 创建材质
        Material mat = new Material(Shader.Find("Custom/TearEffect"));
        mat.color = baseColor;
        meshRenderer.material = mat;

        // 添加碰撞体
        var meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    private void CreateRectangleMesh(out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        vertices = new Vector3[]
        {
            new Vector3(-halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, halfHeight, 0),
            new Vector3(-halfWidth, halfHeight, 0)
        };

        uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };
    }

    private void CreateCircleMesh(out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        int segments = 32;
        float radius = Mathf.Min(width, height) / 2f;

        vertices = new Vector3[segments + 1];
        uvs = new Vector2[segments + 1];
        triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            vertices[i + 1] = new Vector3(x, y, 0);
            uvs[i + 1] = new Vector2(0.5f + x / radius * 0.5f, 0.5f + y / radius * 0.5f);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 2 > segments) ? 1 : i + 2;
        }
    }

    private void CreateRoundedRectMesh(out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        // 简化为普通矩形（圆角需要更多顶点）
        CreateRectangleMesh(out vertices, out uvs, out triangles);
    }

    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();

        // 添加条纹效果（胶带）
        if (addStripes && instanceMaterial != null)
        {
            // 使用shader实现条纹
            instanceMaterial.SetFloat("_StripeAmount", tornAmount * 0.3f);
        }
    }

    /// <summary>
    /// 创建占位胶带
    /// </summary>
    public static GameObject CreatePlaceholderTape(Vector3 position, Vector2 size)
    {
        var go = new GameObject("PlaceholderTape");
        go.transform.position = position;

        var tearable = go.AddComponent<ProceduralTearable>();
        tearable.shapeType = ShapeType.Rectangle;
        tearable.width = size.x;
        tearable.height = size.y;
        tearable.addStripes = true;

        return go;
    }

    /// <summary>
    /// 创建占位贴纸
    /// </summary>
    public static GameObject CreatePlaceholderSticker(Vector3 position, float radius)
    {
        var go = new GameObject("PlaceholderSticker");
        go.transform.position = position;

        var tearable = go.AddComponent<ProceduralTearable>();
        tearable.shapeType = ShapeType.Circle;
        tearable.width = radius * 2;
        tearable.height = radius * 2;
        tearable.baseColor = new Color(1f, 0.8f, 0.6f);

        return go;
    }
}
