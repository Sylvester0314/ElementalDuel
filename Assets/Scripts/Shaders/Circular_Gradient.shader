Shader "Custom/Circular_Gradient"
{
    Properties
    {
        _ColorInner ("Inner Color", Color) = (1, 1, 1, 1)
        _ColorOuter ("Outer Color", Color) = (0, 0, 0, 1)
        _Radius ("Gradient Radius", Range(0, 2)) = 1.0
        _HardClip ("Hard Clip", Float) = 0
        _MainTex ("Alpha Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
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

            struct v2_f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _ColorInner;
            float4 _ColorOuter;
            float _Radius;
            float _HardClip;

            v2_f vert (appdata v)
            {
                v2_f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex).xy;
                return o;
            }

            fixed4 frag (v2_f i) : SV_Target
            {
                float original_alpha = tex2D(_MainTex, i.uv).a;

                float2 center = float2(0.5, 0.5);
                float2 screen_dir = i.screenPos - center;
                float distance = length(screen_dir) * 2;

                float gradient = saturate(distance / _Radius);
                fixed4 color = lerp(_ColorInner, _ColorOuter, gradient);

                if (_HardClip > 0.5 && distance > _Radius)
                    color.a = 0;

                color.a *= original_alpha;

                return color;
            }
            ENDCG
        }
    }
}