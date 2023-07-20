Shader "Phira/grayscale"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _factor ("Factor", Range(0.0, 1.0)) = 1.0
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _factor;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 color = tex2D(_MainTex, i.uv);
                float3 lum = float3(0.299, 0.587, 0.114);
                float3 gray = float3(dot(lum, color.rgb), dot(lum, color.rgb), dot(lum, color.rgb));
                return fixed4(lerp(color.rgb, gray, _factor), 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
