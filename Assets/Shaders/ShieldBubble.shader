Shader "MaskEffect/ShieldBubble"
{
    Properties
    {
        _Color ("Shield Color", Color) = (0.45, 0.75, 1.0, 1.0)
        _FresnelPower ("Fresnel Power", Range(1.0, 8.0)) = 3.0
        _Intensity ("Intensity", Range(0.5, 5.0)) = 1.5
        _PulseSpeed ("Pulse Speed", Range(0.1, 10.0)) = 1.5
        _PulseAmount ("Pulse Amount", Range(0.0, 0.5)) = 0.15
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+2"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Blend SrcAlpha One
        ZWrite Off
        ZTest LEqual
        Cull Back

        Pass
        {
            Name "ShieldBubblePass"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 viewDirWS   : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4  _Color;
                float  _FresnelPower;
                float  _Intensity;
                float  _PulseSpeed;
                float  _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = posInputs.positionCS;
                output.normalWS    = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS   = GetWorldSpaceViewDir(posInputs.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS  = normalize(input.normalWS);
                float3 viewDir   = normalize(input.viewDirWS);

                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), _FresnelPower);
                float pulse   = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                half4 col;
                col.rgb = _Color.rgb * _Intensity * pulse;
                col.a   = fresnel * _Color.a * pulse;
                return col;
            }
            ENDHLSL
        }
    }
}
