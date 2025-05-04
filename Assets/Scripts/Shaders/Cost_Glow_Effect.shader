Shader "Custom/Cost_Glow_Effect"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Channel ("Channel to use (0=R, 1=G, 2=B)", Range(0, 2)) = 0
        _GlowSpeed ("Glow Speed", Float) = 1.0
        _GlowIntensity ("Glow Intensity", Float) = 1.0
        _MaxBrightness ("Max Brightness", Float) = 1.5
        _LightCount ("Light Count", Range(3, 6)) = 3
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Channel;
            float _GlowSpeed;
            float _GlowIntensity;
            float _MaxBrightness;
            int _LightCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float SampleChannel(float3 color, float channel)
            {
                if (channel < 0.5) return color.r;
                if (channel < 1.5) return color.g;
                return color.b;
            }

            float GetWavePattern(float2 uv, float time, float offset)
            {
                float2 center = float2(0.5, 0.5);
                float2 dir = uv - center;
                float dist = length(dir);
                float angle = atan2(dir.y, dir.x);

                // Add wave-like motion in polar coordinates
                angle += sin(time * _GlowSpeed + offset) * 0.2;
                dist -= cos(time * _GlowSpeed + offset) * 0.05;

                return max(0.0, 1.0 - smoothstep(0.3, 0.6, dist));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;
                fixed4 texColor = tex2D(_MainTex, i.uv);
                float3 color = texColor.rgb;
                float alpha = texColor.a;

                // Skip glow for fully transparent areas
                if (alpha <= 0.0)
                    return fixed4(0, 0, 0, 0);

                // Sample the selected channel
                float channelValue = SampleChannel(color, _Channel);

                // Directional wave-like glow effect
                float glowAlpha = 0.0;
                for (int j = 0; j < _LightCount; j++)
                {
                    float offset = (float(j) / float(_LightCount)) * 6.28318530718; // Distribute around 360°
                    glowAlpha += GetWavePattern(i.uv, time, offset);
                }
                glowAlpha /= float(_LightCount); // Average contributions
                glowAlpha *= channelValue * _GlowIntensity;

                // Clamp brightness to prevent overexposure
                float3 finalColor = min(color + glowAlpha, _MaxBrightness);

                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
