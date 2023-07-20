Shader "Phira/circleBlur" {
    Properties {
        _size ("Size", Range(0.0, 100.0)) = 10
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform float4 _MainTex_ST;
            uniform sampler2D _MainTex;
            uniform float _size;

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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 pixel_size = 1.0 / _ScreenParams.xy;
                float4 c = tex2D(_MainTex, i.uv);
                float length = dot(c, c);

                for (float x = -_size; x < _size; x++) {
                    for (float y = -_size; y < _size; ++y) {
                        if (x * x + y * y > _size * _size) continue;
                        float2 offset = pixel_size * float2(x, y);
                        float4 new_c = tex2D(_MainTex, i.uv + offset);
                        float new_length = dot(new_c, new_c);
                        if (new_length > length) {
                            length = new_length;
                            c = new_c;
                        }
                    }
                }

                return c;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}