Shader "Custom/VHSNoiseOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Noise Intensity", Range(0,1)) = 0.2
        _Speed ("Noise Speed", Range(0,5)) = 1
        _LineStrength ("Line Strength", Range(0,1)) = 0.15
        _Opacity ("Opacity", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Intensity;
            float _Speed;
            float _LineStrength;
            float _Opacity;

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float time = _Time.y * _Speed;

                // White noise
                float noise = rand(float2(uv.y * 1000, time));

                // Horizontal VHS lines
                float lineNoise = sin((uv.y + time * 0.5) * 800) * _LineStrength;

                float finalNoise = (noise + lineNoise) * _Intensity;

                return half4(finalNoise, finalNoise, finalNoise, finalNoise * _Opacity);
            }
            ENDHLSL
        }
    }
}