Shader "MaskEffect/FogWall"
{
    Properties
    {
        [Header(Fog Appearance)]
        _Color ("Fog Color", Color) = (0.02, 0.02, 0.05, 0.92)
        _TopColor ("Top Fade Color", Color) = (0.05, 0.03, 0.08, 0.0)
        _NoiseScale ("Noise Scale", Range(0.5, 10)) = 3.0
        _DetailScale ("Detail Noise Scale", Range(1, 20)) = 8.0
        _ScrollSpeed ("Scroll Speed", Range(0, 2)) = 0.3
        _Density ("Fog Density", Range(0, 1)) = 0.88

        [Header(Dissolve)]
        _Dissolve ("Dissolve Amount", Range(0, 1)) = 0.0
        _DissolveEdge ("Dissolve Edge Width", Range(0, 0.2)) = 0.08
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (0.4, 0.1, 0.6, 1.0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "FogWallPass"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _TopColor;
                half _NoiseScale;
                half _DetailScale;
                half _ScrollSpeed;
                half _Density;
                half _Dissolve;
                half _DissolveEdge;
                half4 _DissolveEdgeColor;
            CBUFFER_END

            // ---- Noise functions (matching MarsSurface.shader conventions) ----

            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < octaves; i++)
                {
                    value += amplitude * valueNoise(p * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                return value;
            }

            // ---- Vertex / Fragment ----

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                // Scrolling noise coordinates (drift in both directions for area fog)
                float2 noiseUV = uv * _NoiseScale + float2(_Time.y * _ScrollSpeed, _Time.y * _ScrollSpeed * 0.4);
                float2 detailUV = uv * _DetailScale + float2(_Time.y * _ScrollSpeed * 0.7, _Time.y * 0.15);

                // Layered noise for organic fog look
                float noise = fbm(noiseUV, 4);
                float detail = fbm(detailUV, 3) * 0.3;
                float fogPattern = saturate(noise + detail);

                // Edge fade on all 4 sides (soft rectangular border)
                float edgeSoftness = 0.2;
                float edgeFade = smoothstep(0.0, edgeSoftness, uv.x)
                               * smoothstep(0.0, edgeSoftness, 1.0 - uv.x)
                               * smoothstep(0.0, edgeSoftness, uv.y)
                               * smoothstep(0.0, edgeSoftness, 1.0 - uv.y);

                // Center density boost (denser in the middle of the area)
                float2 center = uv - 0.5;
                float centerDist = length(center) * 1.4;
                float centerBoost = 1.0 - saturate(centerDist);

                // Combined alpha
                float alpha = fogPattern * edgeFade * lerp(0.7, 1.0, centerBoost) * _Density;

                // Color: base fog color with subtle variation from noise
                half4 color = lerp(_Color, _TopColor, fogPattern * 0.4);

                // Dissolve effect
                float dissolveNoise = fbm(uv * 6.0 + float2(3.7, 1.2), 3);
                float dissolveThreshold = _Dissolve * 1.3;
                float dissolveMask = smoothstep(dissolveThreshold - _DissolveEdge, dissolveThreshold, dissolveNoise);

                // Glowing dissolve edge
                float edgeGlow = smoothstep(dissolveThreshold - _DissolveEdge, dissolveThreshold, dissolveNoise)
                               - smoothstep(dissolveThreshold, dissolveThreshold + _DissolveEdge, dissolveNoise);
                color.rgb = lerp(color.rgb, _DissolveEdgeColor.rgb, edgeGlow * 2.0);

                alpha *= dissolveMask;

                // Hard clip fully transparent pixels
                clip(alpha - 0.01);

                return half4(color.rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
