Shader "Phira/noise"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _seed ("Seed", Float) = 81
        _power ("Power", Range(0, 1)) = 0.03
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
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float _seed;
            float _power;
 
            float2 random(float2 pos) {
                return frac(sin(float2(dot(pos, float2(12.9898,78.233)), dot(pos, float2(-148.998,-65.233)))) * 43758.5453);
            }
 
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target {
                float2 new_uv = i.uv + (random(i.uv + float2(_seed, 0.0)) - float2(0.5, 0.5)) * _power;
                return tex2D(_MainTex, new_uv);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
