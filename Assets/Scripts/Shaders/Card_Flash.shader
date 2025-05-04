Shader "Custom/Card_Flash"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _EmissionColor ("Glow Color", Color) = (1, 1, 1, 1)
        _EmissionPower ("Glow Intensity", Range(0, 5)) = 1.0
        _Color ("Tint Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2_f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _EmissionColor;
            float4 _Color;
            float _EmissionPower;

            v2_f vert (appdata_t v)
            {
                v2_f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2_f i) : SV_Target
            {
                fixed4 tex_color = tex2D(_MainTex, i.uv);
                fixed4 emission = _EmissionColor * _EmissionPower * tex_color.a;

                emission.a *= _Color.a * i.color.a;
                
                return emission;
            }
            ENDCG
        }
    }
}