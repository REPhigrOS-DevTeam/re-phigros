Shader "Phira/vignette"
{
    Properties {
        _color ("Color", Color) = (0, 0, 0, 1)
        _extend ("Extend", Range(0.0, 1.0)) = 0.25
        _radius ("Radius", Range(0.0, 50.0)) = 15.0
        _MainTex ("Texture", 2D) = "white" {}
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

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _color;
            float _extend;
            float _radius;

            float4 frag (v2f i) : SV_Target {
                float2 new_uv = i.uv * (1.0 - i.uv.yx);
                float vig = new_uv.x * new_uv.y * _radius;
                vig = pow(vig, _extend);
                return lerp(_color, tex2D(_MainTex, i.uv), vig);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
