Shader "Custom/Dissolve"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0.5
        _EdgeWidth ("Edge Width", Range(0, 1)) = 0.05
        _EdgeColor ("Edge Color", Color) = (1, 0, 0, 1)
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

            struct v2_f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _DissolveTex;
            float _DissolveAmount;
            float _EdgeWidth;
            float4 _EdgeColor;

            v2_f vert(appdata v)
            {
                v2_f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2_f i) : COLOR
            {
                half4 col = tex2D(_MainTex, i.uv);
                
                if (col.a < 0.01)
                    discard;

                float dissolve = tex2D(_DissolveTex, i.uv).r;

                if (dissolve >= _DissolveAmount)
                    discard;

                float edge = smoothstep(_DissolveAmount - _EdgeWidth, _DissolveAmount + _EdgeWidth, dissolve);
                col.rgb = lerp(col.rgb, _EdgeColor.rgb, edge);

                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Transparent"
}
