Shader "Hidden/LiteGlow2D/Composite"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _LightTex ("LightTex", 2D) = "white" {}
        _MaskTex ("MaskTex", 2D) = "white" {}
        _Light ("Light", Float) = 0
        _LightColor ("LightColor", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            TEXTURE2D(_LightTex);
            TEXTURE2D(_MaskTex);

            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_LightTex);
            SAMPLER(sampler_MaskTex);

            float _Light;
            float4 _LightColor;

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

            half4 frag(v2f i) : SV_Target
            {
                float4 light   = SAMPLE_TEXTURE2D(_LightTex, sampler_LightTex, i.uv);
                float4 shadow = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);

                float maskFactor = light.a + mask.a;

                float4 baseResult = shadow;

                float3 lightDir = _LightColor.rgb;

                float l = _Light;

                float3 lit = baseResult.rgb * (1.0 + max(l, 0.0) * lightDir);
                float3 dark = baseResult.rgb * (1.0 + min(l, 0.0));

                baseResult.rgb = lerp(lit, dark, step(0.0, -l));
                float t = saturate((_Light + 1.0) * 0.5);

                float3 col;
                float a;

                if (t < 0.5)
                {
                    float k = t / 0.5;
                    col = lerp(float3(0,0,0), _LightColor.rgb, k);
                    a = lerp(1.0, 0.0, k);
                }
                else
                {
                    float k = (t - 0.5) / 0.5;
                    col = lerp(_LightColor.rgb, float3(1,1,1), k);
                    a = lerp(0.0, 1.0, k);
                }
                float4 result = float4(col, a);

                float3 finalRGB = lerp(baseResult.rgb, result.rgb, result.a);
                float finalA = max(baseResult.a, result.a);

                return lerp(float4(finalRGB, finalA), light, maskFactor);
            }

            ENDHLSL
        }
    }
}