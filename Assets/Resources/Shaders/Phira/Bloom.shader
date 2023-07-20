Shader "Phira/bloom" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _iThreshold("Threshold", Range(0.0,1.0)) = 0.01
        _iIntensity("Intensity", Range(0.0,10.0)) = 1.0
        _iColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader{
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _iThreshold;
            float _iIntensity;
            float4 _iColor;
            
            float grayScale(float3 color)
            {
                float3 t = float3(0.299, 0.587, 0.114);
                
                return dot(color, t);
            }
            
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f{
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            v2f vert(appdata v){
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target{
                float2 uv = i.uv;
                float3 color = tex2D(_MainTex, uv).rgb;

                if (grayScale(color) > _iThreshold)
                {
                    float3 offset = color * (pow(2.0, _iIntensity) - 1.0) * _iColor.rgb;
                    color = lerp(offset, color, 1.0 / pow(1.1, _iIntensity));
                }
                
                return fixed4(color, 1.0);
            }
            
            ENDCG
        }
    }
}