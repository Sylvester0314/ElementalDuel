Shader "Custom/Mesh_Mask"
{
    Properties
    {
        _GridSize ("Grid Size", Float) = 20
        _LineWidth ("Line Width", Float) = 0.1
        _Rotation ("Rotation Angle", Range(0, 360)) = 0
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float _GridSize;
            float _LineWidth;
            float _Rotation;

            v2_f vert(appdata_t v)
            {
                v2_f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2_f i) : SV_Target
            {
                float angle = radians(_Rotation);
                float2x2 rotation_matrix = float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
                float2 rotated_uv = mul(rotation_matrix, i.uv - 0.5) + 0.5;

                float2 grid = frac(rotated_uv * _GridSize);
                float border = _LineWidth * 0.5;

                if (grid.x < border || grid.y < border || grid.x > (1 - border) || grid.y > (1 - border))
                {
                    return fixed4(0, 0, 0, 0);
                }

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}