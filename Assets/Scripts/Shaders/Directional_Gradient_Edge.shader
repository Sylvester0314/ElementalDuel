Shader "Custom/Directional_Gradient_Edge"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _TopEdge ("Top Edge Width", Range(0, 0.5)) = 0.1
        _BottomEdge ("Bottom Edge Width", Range(0, 0.5)) = 0.1
        _LeftEdge ("Left Edge Width", Range(0, 0.5)) = 0.1
        _RightEdge ("Right Edge Width", Range(0, 0.5)) = 0.1
        _EdgeIntensity ("Edge Intensity", Range(0.1, 2.0)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "RenderType" = "Transparent"
        }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

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
            };

            sampler2D _MainTex;
            float _TopEdge;
            float _BottomEdge;
            float _LeftEdge;
            float _RightEdge;
            float _EdgeIntensity;

            v2_f vert(appdata v)
            {
                v2_f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2_f i) : SV_Target
            {
                float2 uv = i.uv;

                float top_weight = smoothstep(1.0 - _TopEdge, 1.0, uv.y);
                float bottom_weight = smoothstep(_BottomEdge, 0.0, uv.y);
                float left_weight = smoothstep(_LeftEdge, 0.0, uv.x);
                float right_weight = smoothstep(1.0 - _RightEdge, 1.0, uv.x);

                float edge_alpha = 1.0 - _EdgeIntensity * (
                    top_weight + bottom_weight + left_weight + right_weight
                );

                fixed4 color = tex2D(_MainTex, uv);
                color.a *= saturate(edge_alpha);
                
                return color;
            }
            ENDCG
        }
    }
}