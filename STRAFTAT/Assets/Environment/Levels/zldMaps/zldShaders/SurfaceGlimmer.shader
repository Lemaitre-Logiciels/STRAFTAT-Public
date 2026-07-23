Shader "Custom/SurfaceHubWall"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _UVScale ("UV Scale", Range(0.001, 10)) = 4.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "sayaka.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _UVScale;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            float2 uv = sayaka_tileuv(IN.uv_MainTex, float2(_UVScale,_UVScale));
            int xindex = abs(sayaka_indexsplit(IN.uv_MainTex.x, _UVScale));
            int yindex = abs(sayaka_indexsplit(IN.uv_MainTex.y, _UVScale));
            xindex %= 4;
            yindex %= 6;
            float maxTime = 0.4f;
            float timeWrapped = _Time % maxTime;
            int targetIndex = sayaka_indexsplit(timeWrapped, maxTime / 4);
            int targetYIndex = sayaka_indexsplit(timeWrapped, maxTime / 6);
            float2 timeSegment = sayaka_tileuv(float2(timeWrapped, timeWrapped), float2(maxTime / 4, maxTime / 6));
            if (xindex == targetIndex) {
                o.Smoothness = 1.0 - timeSegment.x;
            }
            if (yindex == targetIndex) {
                o.Smoothness = 1.0 - timeSegment.y;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
