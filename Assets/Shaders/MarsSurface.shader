Shader "MaskEffect/MarsSurface"
{
    Properties
    {
        [Header(Base Colors)]
        _Color1 ("Sand Color", Color) = (0.76, 0.42, 0.22, 1)
        _Color2 ("Rock Color", Color) = (0.45, 0.25, 0.15, 1)
        _Color3 ("Dust Color", Color) = (0.85, 0.55, 0.35, 1)
        _DarkCrevice ("Crevice Color", Color) = (0.25, 0.12, 0.08, 1)

        [Header(Terrain Shape)]
        _NoiseScale ("Noise Scale", Range(0.5, 20)) = 4
        _DetailScale ("Detail Scale", Range(5, 80)) = 25
        _RockScale ("Rock Pattern Scale", Range(1, 15)) = 8
        _Roughness ("Surface Roughness", Range(0, 1)) = 0.7

        [Header(Displacement)]
        _HeightStrength ("Height Strength", Range(0, 2)) = 0.3
        _TessAmount ("Tessellation", Range(1, 16)) = 1

        [Header(Distance Fade)]
        _FadeStart ("Fade Start Distance", Float) = 15
        _FadeEnd ("Fade End Distance", Float) = 40
        _HorizonColor ("Horizon Dust Color", Color) = (0.72, 0.45, 0.3, 1)

        [Header(Lighting)]
        _Metallic ("Metallic", Range(0, 1)) = 0.05
        _Smoothness ("Smoothness", Range(0, 1)) = 0.15
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry-1" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color1;
                half4 _Color2;
                half4 _Color3;
                half4 _DarkCrevice;
                half _NoiseScale;
                half _DetailScale;
                half _RockScale;
                half _Roughness;
                half _HeightStrength;
                half _TessAmount;
                half _FadeStart;
                half _FadeEnd;
                half4 _HorizonColor;
                half _Metallic;
                half _Smoothness;
            CBUFFER_END

            // ---- Noise functions ----

            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float hash3(float3 p)
            {
                p = frac(p * float3(0.1031, 0.1030, 0.0973));
                p += dot(p, p.yxz + 33.33);
                return frac((p.x + p.y) * p.z);
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

            // Gradient noise (Perlin-like)
            float2 gradHash(float2 p)
            {
                float a = hash(p) * 6.2831853;
                return float2(cos(a), sin(a));
            }

            float gradientNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float2 ga = gradHash(i + float2(0, 0));
                float2 gb = gradHash(i + float2(1, 0));
                float2 gc = gradHash(i + float2(0, 1));
                float2 gd = gradHash(i + float2(1, 1));

                float va = dot(ga, f - float2(0, 0));
                float vb = dot(gb, f - float2(1, 0));
                float vc = dot(gc, f - float2(0, 1));
                float vd = dot(gd, f - float2(1, 1));

                return lerp(lerp(va, vb, u.x), lerp(vc, vd, u.x), u.y) + 0.5;
            }

            // FBM with multiple octaves
            float fbm(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                float lacunarity = 2.0;
                float persistence = 0.5;

                for (int i = 0; i < octaves; i++)
                {
                    value += amplitude * valueNoise(p * frequency);
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                return value;
            }

            // Voronoi for rock cracks
            float voronoi(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float minDist = 1.0;
                float secondMin = 1.0;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 pt = float2(hash(i + neighbor), hash(i + neighbor + 100));
                        float2 diff = neighbor + pt - f;
                        float dist = length(diff);

                        if (dist < minDist)
                        {
                            secondMin = minDist;
                            minDist = dist;
                        }
                        else if (dist < secondMin)
                        {
                            secondMin = dist;
                        }
                    }
                }

                return secondMin - minDist; // Edge distance for cracks
            }

            // ---- Vertex / Fragment ----

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                // Displace Y based on noise for subtle terrain bumps
                float terrainHeight = fbm(worldPos.xz * _NoiseScale * 0.1, 4);
                float detail = valueNoise(worldPos.xz * _DetailScale * 0.1) * 0.3;
                worldPos.y += (terrainHeight + detail - 0.5) * _HeightStrength;

                output.positionWS = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 worldUV = input.positionWS.xz;

                // --- Layer 1: Large-scale terrain coloring ---
                float largeTerrain = fbm(worldUV * _NoiseScale * 0.1, 5);

                // --- Layer 2: Medium rock patterns ---
                float rockPattern = fbm(worldUV * _RockScale * 0.1 + 3.7, 4);

                // --- Layer 3: Fine detail / gravel ---
                float fineDetail = valueNoise(worldUV * _DetailScale * 0.1);
                float fineDetail2 = valueNoise(worldUV * _DetailScale * 0.15 + 17.3);

                // --- Layer 4: Voronoi cracks ---
                float cracks = voronoi(worldUV * _RockScale * 0.15);
                float crackMask = smoothstep(0.0, 0.08, cracks);

                // --- Combine colors ---
                // Base: blend sand and dust based on large terrain
                half3 baseColor = lerp(_Color1.rgb, _Color3.rgb, smoothstep(0.35, 0.65, largeTerrain));

                // Rock areas
                float rockMask = smoothstep(0.4, 0.7, rockPattern);
                baseColor = lerp(baseColor, _Color2.rgb, rockMask * 0.7);

                // Fine gravel variation
                float gravelMix = fineDetail * fineDetail2;
                baseColor = lerp(baseColor, baseColor * (0.8 + 0.4 * gravelMix), _Roughness);

                // Dark crevices from voronoi
                baseColor = lerp(_DarkCrevice.rgb, baseColor, crackMask);

                // Slight random speckle
                float speckle = hash(floor(worldUV * 50.0));
                baseColor *= 0.92 + 0.16 * speckle;

                // --- Compute normal from noise (bump mapping) ---
                float eps = 0.05;
                float h0 = fbm((worldUV) * _NoiseScale * 0.1, 4);
                float hx = fbm((worldUV + float2(eps, 0)) * _NoiseScale * 0.1, 4);
                float hz = fbm((worldUV + float2(0, eps)) * _NoiseScale * 0.1, 4);

                float3 bumpNormal = normalize(float3(
                    (h0 - hx) / eps * _HeightStrength,
                    1.0,
                    (h0 - hz) / eps * _HeightStrength
                ));

                // Blend with geometric normal
                float3 N = normalize(lerp(input.normalWS, bumpNormal, 0.6));

                // --- Distance fade to horizon dust ---
                float camDist = distance(input.positionWS, _WorldSpaceCameraPos);
                float fadeFactor = saturate((camDist - _FadeStart) / (_FadeEnd - _FadeStart));
                baseColor = lerp(baseColor, _HorizonColor.rgb, fadeFactor * fadeFactor);

                // --- URP Lighting ---
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = N;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.fogCoord = input.fogCoord;

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseColor;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = float3(0, 0, 1);
                surfaceData.occlusion = lerp(0.7, 1.0, crackMask);
                surfaceData.alpha = 1.0;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, input.fogCoord);

                return color;
            }
            ENDHLSL
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            ShadowVaryings ShadowVert(ShadowAttributes input)
            {
                ShadowVaryings output;
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(worldPos, worldNormal, _LightDirection));
                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // Depth pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            DepthVaryings DepthVert(DepthAttributes input)
            {
                DepthVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthFrag(DepthVaryings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}