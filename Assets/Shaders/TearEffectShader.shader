Shader "Custom/TearEffect"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TornAmount ("Torn Amount", Range(0, 1)) = 0
        _TornEdgeColor ("Torn Edge Color", Color) = (1, 0.8, 0.6, 1)
        _TornEdgeWidth ("Torn Edge Width", Range(0, 0.2)) = 0.05
        _TearTint ("Tear Tint", Color) = (1, 1, 1, 1)
        _UVOffset ("UV Offset", Vector) = (0, 0, 0, 0)
        _Glossiness ("Surface Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        float _TornAmount;
        float4 _TornEdgeColor;
        float _TornEdgeWidth;
        float4 _TearTint;
        float2 _UVOffset;
        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_TornTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 获取基础颜色
            half4 c = tex2D (_MainTex, IN.uv_MainTex + _UVOffset);

            // 计算撕裂边缘效果
            float tearFactor = _TornAmount;

            // 撕裂边缘渐变
            float edgeFactor = smoothstep(1.0 - _TornEdgeWidth, 1.0, tearFactor);

            // 边缘颜色变化（磨损效果）
            float3 wornColor = lerp(c.rgb, _TornEdgeColor.rgb, edgeFactor * 0.8);

            // UV扰动效果（撕裂时纹理偏移）
            float2 uvDistortion = (1.0 - tearFactor) * 0.1;
            half4 distortedTex = tex2D(_MainTex, IN.uv_MainTex + uvDistortion);
            wornColor = lerp(distortedTex.rgb, wornColor, tearFactor);

            // 撕裂后的颜色调整
            float3 finalColor = lerp(c.rgb * _TearTint.rgb, wornColor, tearFactor);

            // 表面粗糙度变化
            half smoothness = _Glossiness * (1.0 - tearFactor * 0.5);
            o.Smoothness = smoothness;

            // 金属感
            o.Metallic = _Metallic * (1.0 - tearFactor * 0.3);

            // 边缘高亮/阴影
            o.Emission = _TornEdgeColor * edgeFactor * 0.1 * tearFactor;

            o.Albedo = finalColor;
            o.Alpha = c.a;
        }
        ENDCG
    }

    FallBack "Standard"
}
