Shader "Custom/Glowing_Point_Light"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Float) = 1.0
        _Speed ("Speed", Float) = 1.0
        _MinSize ("Min Size", Float) = 0.5
        _MaxSize ("Max Size", Float) = 1.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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

            sampler2D _MainTex;
            float4 _Color;
            float _Intensity;
            float _Speed;
            float _MinSize;
            float _MaxSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;
                float pulse = (sin(time) + 1.0) * 0.5; // 将sin值映射到0-1范围
                float scale = lerp(_MinSize, _MaxSize, pulse);

                float2 uv = (i.uv - 0.5) * (scale + _MaxSize) + 0.5;
                fixed4 tex_color = tex2D(_MainTex, uv);
                fixed4 final_color = tex_color * _Color * _Intensity;

                final_color.a = tex_color.a;

                return final_color;
            }
            ENDCG
        }
    }
}