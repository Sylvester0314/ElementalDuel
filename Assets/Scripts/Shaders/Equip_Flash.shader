Shader "Custom/Equip_Flash"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {} 
        _MaskTex ("Flash Mask", 2D) = "white" {}  
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.0
        _GlowSpeed ("Glow Speed", Range(0, 5)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowSpeed;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2_f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2_f vert(appdata_t v)
            {
                v2_f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2_f i) : SV_Target
            {
                fixed4 base_color = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.uv);

                // Calculate dynamic luminescence value
                float r_glow = 0.4 + 0.6 * (sin(_Time.y * _GlowSpeed) + 1.0) / 2.0;
                float b_glow = 0.4 + 0.4 * (sin(_Time.y * _GlowSpeed * 1.5) + 1.0) / 2.0;
                float g_glow = 0.2 + 0.9 * (sin(_Time.y * _GlowSpeed * 1.0) + 1.0) / 2.0;

                // Calculate Outer Glow
                float outside_mask = step(base_color.a, 0.1);
                g_glow *= outside_mask * mask.g * 1.5;

                // Comprehensive luminous effect
                float glow = mask.r * r_glow + mask.b * b_glow + g_glow;
                // Apply Global Illumination Intensity
                glow *= _GlowIntensity;

                // Calculate the final color
                fixed4 final_color = base_color + _GlowColor * glow;
                final_color.a = max(base_color.a, glow);
                return final_color;
            }
            ENDCG
        }
    }
}