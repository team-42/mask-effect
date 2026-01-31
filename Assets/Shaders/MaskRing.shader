Shader "MaskEffect/MaskRing"
{
    Properties
    {
        _Color ("Ring Color", Color) = (1,1,1,1)
        _InnerRadius ("Inner Radius", Range(0.0, 0.5)) = 0.35
        _OuterRadius ("Outer Radius", Range(0.0, 0.5)) = 0.45
        _GlowWidth ("Glow Width", Range(0.0, 0.2)) = 0.08
        _GlowIntensity ("Glow Intensity", Range(0.0, 5.0)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0.0, 5.0)) = 1.5
        _PulseAmount ("Pulse Amount", Range(0.0, 1.0)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _InnerRadius;
            float _OuterRadius;
            float _GlowWidth;
            float _GlowIntensity;
            float _PulseSpeed;
            float _PulseAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Map UV to centered coordinates (-0.5 to 0.5)
                float2 centered = i.uv - 0.5;
                float dist = length(centered);

                // Ring shape with soft edges
                float ringCenter = (_InnerRadius + _OuterRadius) * 0.5;
                float ringHalfWidth = (_OuterRadius - _InnerRadius) * 0.5;

                // Smooth ring mask
                float ring = 1.0 - smoothstep(0.0, ringHalfWidth, abs(dist - ringCenter));

                // Outer glow
                float outerGlow = 1.0 - smoothstep(0.0, _GlowWidth, dist - _OuterRadius);
                outerGlow = max(0, outerGlow) * 0.5;

                // Inner glow
                float innerGlow = 1.0 - smoothstep(0.0, _GlowWidth, _InnerRadius - dist);
                innerGlow = max(0, innerGlow) * 0.3;

                // Combine
                float alpha = saturate(ring + outerGlow + innerGlow);

                // Pulse animation
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                alpha *= pulse;

                // Apply color with glow intensity
                fixed4 col = _Color * _GlowIntensity;
                col.a = alpha * _Color.a;

                // Clip fully transparent pixels
                clip(col.a - 0.01);

                return col;
            }
            ENDCG
        }
    }
}
