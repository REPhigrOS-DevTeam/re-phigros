Shader "Phira/shockwave"
{
    Properties {
        _progress ("Progress", Range(0, 1)) = 0.2
        _centerX ("Center X", Range(0, 1)) = 0.5
        _centerY ("Center Y", Range(0, 1)) = 0.5
        _width ("Width", Range(0, 1)) = 0.1
        _distortion ("Distortion", Range(0, 1)) = 0.8
        _expand ("Expand", Range(0, 100)) = 10
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;

            float _progress;
            float _centerX;
            float _centerY;
            float _width;
            float _distortion;
            float _expand;

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
                float aspect = _ScreenParams.y / _ScreenParams.x;

                float2 center = float2(_centerX, _centerY);
                center.y = (center.y - 0.5) * aspect + 0.5;

                float2 tex_coord = i.uv;
                tex_coord.y = (tex_coord.y - 0.5) * aspect + 0.5;
                float dist = distance(tex_coord, center);

                if (_progress - _width <= dist && dist <= _progress + _width) {
                    float diff = dist - _progress;
                    float scale_diff = 1.0 - pow(abs(diff * _expand), _distortion);
                    float dt = diff * scale_diff;

                    float2 dir = normalize(tex_coord - center);

                    tex_coord += ((dir * dt) / (_progress * dist * 40.0));
                    fixed4 col = tex2D(_MainTex, float2(tex_coord.x, (tex_coord.y - 0.5) / aspect + 0.5));

                    col += (col * scale_diff) / (_progress * dist * 40.0);
                    return col;
                } else {
                    return tex2D(_MainTex, float2(tex_coord.x, (tex_coord.y - 0.5) / aspect + 0.5));
                }
            }
            ENDCG
        }
    }
}
