Shader "MaskEffect/ShieldBar"
{
    Properties
    {
        _ShieldPercent ("Shield Percent", Range(0, 1)) = 1.0
        _ShieldColor ("Shield Color", Color) = (0.45, 0.75, 1.0, 1)
        _EmptyColor ("Empty Color", Color) = (0.05, 0.05, 0.15, 0.4)
        _BorderColor ("Border Color", Color) = (0, 0, 0, 1)
        _BorderWidth ("Border Width", Range(0, 0.1)) = 0.03
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off
        Offset -1, -1

        Pass
        {
            Name "ShieldBarPass"
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
                float  _ShieldPercent;
                half4  _ShieldColor;
                half4  _EmptyColor;
                half4  _BorderColor;
                float  _BorderWidth;
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
                float2 uv = input.uv;

                bool inBorder = uv.x < _BorderWidth || uv.x > (1.0 - _BorderWidth) ||
                               uv.y < _BorderWidth || uv.y > (1.0 - _BorderWidth);

                if (inBorder)
                    return _BorderColor;

                float innerX = (uv.x - _BorderWidth) / (1.0 - 2.0 * _BorderWidth);

                if (innerX < _ShieldPercent)
                    return _ShieldColor;
                else
                    return _EmptyColor;
            }
            ENDHLSL
        }
    }
}
