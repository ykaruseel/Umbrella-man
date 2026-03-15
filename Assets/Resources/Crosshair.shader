Shader "UI/CrosshairRingFill"
{
    Properties
    {
        _Progress ("Progress", Range(0,1)) = 0
        _Thickness ("Thickness", Range(0.01,0.5)) = 0.15
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Progress;
            float _Thickness;
            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2 - 1;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.uv);

                float outer = 1.0;
                float inner = outer - _Thickness * 2;

                float ringMask = step(inner, dist) * step(dist, outer);
                float fillMask = step(dist, inner);

                float alpha =
                    ringMask * (1 - _Progress) +
                    fillMask * _Progress;

                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
