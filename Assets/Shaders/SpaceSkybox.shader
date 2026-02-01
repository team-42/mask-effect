Shader "MaskEffect/SpaceSkybox"
{
    Properties
    {
        [Header(Background)]
        _TopColor ("Top Color", Color) = (0.01, 0.01, 0.03, 1)
        _BottomColor ("Bottom Color", Color) = (0.02, 0.01, 0.04, 1)
        _HorizonColor ("Horizon Color", Color) = (0.05, 0.02, 0.08, 1)
        _HorizonSharpness ("Horizon Sharpness", Range(1, 10)) = 3

        [Header(Stars)]
        _StarDensity ("Star Density", Range(0, 100)) = 40
        _StarBrightness ("Star Brightness", Range(0, 5)) = 2.5
        _StarSize ("Star Size", Range(0, 0.1)) = 0.02
        _Twinkle ("Twinkle Speed", Range(0, 5)) = 1.5

        [Header(Nebula)]
        _NebulaColor1 ("Nebula Color 1", Color) = (0.15, 0.05, 0.2, 1)
        _NebulaColor2 ("Nebula Color 2", Color) = (0.05, 0.1, 0.25, 1)
        _NebulaIntensity ("Nebula Intensity", Range(0, 1)) = 0.3
        _NebulaScale ("Nebula Scale", Range(0.5, 5)) = 2
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                half4 _HorizonColor;
                half _HorizonSharpness;
                half _StarDensity;
                half _StarBrightness;
                half _StarSize;
                half _Twinkle;
                half4 _NebulaColor1;
                half4 _NebulaColor2;
                half _NebulaIntensity;
                half _NebulaScale;
            CBUFFER_END

            // Hash functions for procedural generation
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float hash31(float3 p)
            {
                p = frac(p * float3(0.1031, 0.1030, 0.0973));
                p += dot(p, p.yxz + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            // Simple 3D noise for nebula
            float noise3D(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f); // smoothstep

                float n000 = hash31(i);
                float n100 = hash31(i + float3(1, 0, 0));
                float n010 = hash31(i + float3(0, 1, 0));
                float n110 = hash31(i + float3(1, 1, 0));
                float n001 = hash31(i + float3(0, 0, 1));
                float n101 = hash31(i + float3(1, 0, 1));
                float n011 = hash31(i + float3(0, 1, 1));
                float n111 = hash31(i + float3(1, 1, 1));

                float nx00 = lerp(n000, n100, f.x);
                float nx10 = lerp(n010, n110, f.x);
                float nx01 = lerp(n001, n101, f.x);
                float nx11 = lerp(n011, n111, f.x);

                float nxy0 = lerp(nx00, nx10, f.y);
                float nxy1 = lerp(nx01, nx11, f.y);

                return lerp(nxy0, nxy1, f.z);
            }

            // Fractal Brownian Motion for nebula clouds
            float fbm(float3 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * noise3D(p * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                return value;
            }

            // Star field - returns brightness of star at direction
            float starField(float3 dir, float density, float size)
            {
                // Create grid cells on the unit sphere
                float3 cellSize = float3(15.0, 15.0, 15.0);
                float3 cell = floor(dir * cellSize);
                float3 cellFrac = frac(dir * cellSize);

                float star = 0.0;

                // Check neighboring cells for stars
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            float3 neighbor = cell + float3(x, y, z);
                            float rnd = hash31(neighbor);

                            // Only place a star if random value is below density threshold
                            if (rnd < density * 0.01)
                            {
                                // Star position within cell
                                float3 starPos = float3(
                                    hash31(neighbor + 100),
                                    hash31(neighbor + 200),
                                    hash31(neighbor + 300)
                                );

                                float3 diff = cellFrac - starPos - float3(x, y, z);
                                float dist = length(diff);

                                // Star brightness based on distance
                                float brightness = smoothstep(size, 0.0, dist);
                                brightness *= brightness; // sharper falloff

                                // Vary star brightness
                                float sizeMult = hash31(neighbor + 400);
                                brightness *= 0.3 + 0.7 * sizeMult;

                                // Twinkle
                                float twinkle = sin(_Time.y * _Twinkle * (0.5 + hash31(neighbor + 500)) + hash31(neighbor + 600) * 6.28);
                                brightness *= 0.7 + 0.3 * twinkle;

                                // Slight color variation
                                star = max(star, brightness);
                            }
                        }
                    }
                }

                return star;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDir = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.viewDir);

                // Background gradient
                float upDot = dir.y;
                half3 bgColor;

                if (upDot > 0)
                {
                    // Above horizon: blend horizon to top
                    float t = pow(upDot, 1.0 / _HorizonSharpness);
                    bgColor = lerp(_HorizonColor.rgb, _TopColor.rgb, t);
                }
                else
                {
                    // Below horizon: blend horizon to bottom
                    float t = pow(-upDot, 1.0 / _HorizonSharpness);
                    bgColor = lerp(_HorizonColor.rgb, _BottomColor.rgb, t);
                }

                // Nebula clouds
                float3 nebulaPos = dir * _NebulaScale;
                float nebula1 = fbm(nebulaPos + float3(0.5, 0.2, 0.8));
                float nebula2 = fbm(nebulaPos * 1.3 + float3(3.1, 1.4, 2.7));

                // Shape nebula - more visible in certain regions
                nebula1 = smoothstep(0.35, 0.7, nebula1);
                nebula2 = smoothstep(0.4, 0.75, nebula2);

                half3 nebulaColor = _NebulaColor1.rgb * nebula1 + _NebulaColor2.rgb * nebula2;
                bgColor += nebulaColor * _NebulaIntensity;

                // Stars
                float stars = starField(dir, _StarDensity, _StarSize);

                // Brighter stars have slight blue-white tint
                half3 starColor = lerp(half3(0.8, 0.85, 1.0), half3(1.0, 1.0, 1.0), stars);
                bgColor += starColor * stars * _StarBrightness;

                // Add a few bright accent stars at second layer
                float bigStars = starField(dir * 0.5 + float3(7.7, 3.3, 1.1), _StarDensity * 0.15, _StarSize * 2.5);
                half3 bigStarColor = lerp(half3(1.0, 0.9, 0.7), half3(0.7, 0.8, 1.0), hash31(floor(dir * 7.5)));
                bgColor += bigStarColor * bigStars * _StarBrightness * 1.5;

                return half4(bgColor, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}