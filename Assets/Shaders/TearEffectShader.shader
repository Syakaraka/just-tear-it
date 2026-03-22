Shader "Custom/TearEffect"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TornTex ("Torn Edge Texture", 2D) = "white" {}
        _TornAmount ("Torn Amount", Range(0, 1)) = 0
        _TornEdgeColor ("Torn Edge Color", Color) = (1, 1, 1, 1)
        _TornEdgeWidth ("Torn Edge Width", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _TornTex;
        float _TornAmount;
        float4 _TornEdgeColor;
        float _TornEdgeWidth;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_TornTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);

            // 撕裂效果混合
            float4 tornEdge = tex2D (_TornTex, IN.uv_TornTex);

            // 根据撕裂量混合颜色
            float4 finalColor = lerp(c, tornEdge * _TornEdgeColor, _TornAmount);

            o.Albedo = finalColor.rgb;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
