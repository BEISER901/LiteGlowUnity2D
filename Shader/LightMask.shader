Shader "Hidden/LiteGlow2D/LightMask"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Float) = 1
        [Enum(Plain Alpha, 0, Smooth Center, 1)]
        _Mode ("Mode", Float) = 0
        [Toggle]
        _UseTexture ("Use Texture", Float) = 0
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            ColorMask RGBA

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _Intensity;
            float _Mode;
            float _UseTexture;
            float4 _Color;
            float _AlphaCutoff;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float SmoothCenter(float2 uv)
            {
                float2 d = uv - 0.5;
                float dist = length(d) * 2.0;
                return saturate(1.0 - dist);
            }

            half4 frag(v2f i) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                float3 baseColor = (_UseTexture > 0.5)
                    ? tex.rgb * _Color.rgb
                    : _Color.rgb;

                float mask = smoothstep(
                    _AlphaCutoff - 0.05,
                    _AlphaCutoff + 0.05,
                    tex.a
                );

                float smooth = SmoothCenter(i.uv);

                float t = saturate(abs(_Intensity));

                float3 finalColor;

                if (_Intensity >= 0)
                {
                    float3 lit = lerp(baseColor, _Color, t);
                    finalColor = lit;
                }
                else
                {
                    finalColor = lerp(baseColor, float3(0,0,0), t);
                }

                float alpha;

                if (_Mode < 0.5)
                    alpha = mask;
                else
                    alpha = smooth;

                return float4(finalColor, alpha * _Color.a * abs(_Intensity));
            }

            ENDHLSL
        }
    }
}