Shader "Custom/Gradient_Mask"
{
    Properties
    {
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc" // 引入Unity UI的裁剪支持

            struct appdata
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
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            fixed4 _Color;
            float4 _ClipRect;

            v2_f vert (appdata v)
            {
                v2_f o;
                o.worldPosition = v.vertex; // 记录世界坐标
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MaskTex); // 应用遮罩纹理的缩放和偏移
                o.color = v.color * _Color; // 应用UI颜色
                return o;
            }

            fixed4 frag (v2_f i) : SV_Target
            {
                fixed4 source_olor = tex2D(_MainTex, i.uv);
                fixed mask_alpha = tex2D(_MaskTex, i.uv).a;

                source_olor.a *= mask_alpha;
                source_olor *= i.color;
                source_olor.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                return source_olor;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}