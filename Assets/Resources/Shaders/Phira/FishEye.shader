Shader "Phira/fisheye"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _power ("Power", Range(-1.0, 1.0)) = -0.1
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _power;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = float2(i.uv.x, i.uv.y * _ScreenParams.y / _ScreenParams.x);
                float2 m = float2(0.5, 0.5 / aspect);
                float2 d = p - m;
                float r = sqrt(dot(d, d));

                float new_power = (2.0 * 3.141592 / (2.0 * sqrt(dot(m, m)))) * _power;

                float bind = new_power > 0.0 ? sqrt(dot(m, m)) : (aspect < 1.0 ? m.x : m.y);

                float2 nuv;
                if (new_power > 0.0)
                    nuv = m + normalize(d) * tan(r * new_power) * bind / tan(bind * new_power);
                else
                    nuv = m + normalize(d) * atan(r * -new_power * 10.0) * bind / atan(-new_power * bind * 10.0);

                return tex2D(_MainTex, float2(nuv.x, nuv.y * aspect));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
