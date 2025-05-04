Shader "Custom/Skill_Background_Light"
{
    Properties
    {
        _MainTex ("Source Image", 2D) = "white" {}
        _TargetColor ("Target Color", Color) = (1, 1, 1, 1)
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowStrength ("Glow Strength", Range(0, 1)) = 0.5
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TargetColor;
            float4 _GlowColor;
            float _GlowStrength;
            float _EdgeThreshold;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex_olor = tex2D(_MainTex, i.uv);
                float alpha = tex_olor.a;

                // Separate main body and edge glow regions
                float body_alpha = step(_EdgeThreshold, alpha); // Main body detection
                float edge_glow = smoothstep(0.0, _GlowStrength, alpha) * (1.0 - body_alpha);

                // Main body color blending
                fixed4 body_color = lerp(tex_olor, _TargetColor, body_alpha);

                // Edge glow blending
                fixed4 glow_color = lerp(body_color, _GlowColor, edge_glow);

                // Combine final color, preserving original alpha
                fixed4 output_color = glow_color;
                output_color.a = alpha;

                return output_color;
            }
            ENDCG
        }
    }
}
