Shader "Custom/Brighten_Image"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  
        _Brightness ("Brightness", Range(1, 3)) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

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
            };

            struct v2_f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Brightness;

            v2_f vert (appdata_t v)
            {
                v2_f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2_f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _Brightness;
                return col;
            }
            ENDCG
        }
    }
}
