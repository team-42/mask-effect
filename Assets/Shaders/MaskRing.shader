Shader "MaskEffect/MaskRing"
{
    Properties
    {
        _Color ("Ring Color", Color) = (1,1,1,1)
        _RingRadius ("Ring Radius", Range(0.1, 0.5)) = 0.42
        _RingThickness ("Ring Thickness", Range(0.001, 0.1)) = 0.025
        _GlowFalloff ("Glow Falloff", Range(0.01, 0.3)) = 0.1
        _GlowIntensity ("Glow Intensity", Range(0.0, 15.0)) = 5.0
        _CoreBrightness ("Core Brightness", Range(1.0, 30.0)) = 12.0
        _PulseSpeed ("Pulse Speed", Range(0.0, 5.0)) = 1.2
        _PulseAmount ("Pulse Amount", Range(0.0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+1"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "MaskRingPass"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _RingRadius;
                float _RingThickness;
                float _GlowFalloff;
                float _GlowIntensity;
                float _CoreBrightness;
                float _PulseSpeed;
                float _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centered = input.uv - 0.5;
                float dist = length(centered);

                // Distance from the ring centerline
                float ringDist = abs(dist - _RingRadius);

                // Sharp bright core of the ring
                float core = exp(-ringDist * ringDist / (_RingThickness * _RingThickness));

                // Soft glow around the ring
                float glow = exp(-ringDist / _GlowFalloff);

                // Combine: bright core + softer glow
                float intensity = core * _CoreBrightness + glow * _GlowIntensity;

                // Pulse
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                intensity *= pulse;

                // Fade at quad edge to avoid hard cutoff
                float edgeFade = smoothstep(0.5, 0.45, dist);
                intensity *= edgeFade;

                // Color output
                half4 col;
                col.rgb = _Color.rgb * intensity;
                col.a = saturate(intensity);

                // Hard clip transparent pixels
                clip(col.a - 0.01);

                return col;
            }
            ENDHLSL
        }
    }
}
