Shader "Custom/Alpha_Mask"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MaskColor ("Mask Color", Color) = (0, 0, 0, 0.5)
    }
    SubShader
    {
        Tags
        {
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
            #include "UnityUI.cginc"

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
            float4 _MaskColor;
            float4 _ClipRect;

            v2_f vert(appdata v)
            {
                v2_f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2_f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);

                if (color.a > 0)
                    color.rgb = lerp(color.rgb, _MaskColor.rgb, _MaskColor.a);

                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                return color;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}