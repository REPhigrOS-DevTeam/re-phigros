Shader "Phira/pixel"
{
    Properties {
        _size ("Pixel Size", Range(1, 100)) = 10
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Pass{
            Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        sampler2D _MainTex;
        float _size;

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

        fixed4 frag (v2f i) : SV_Target {
            float2 factor = _ScreenParams.xy / _size;
            float x = floor(i.uv.x * factor.x + 0.5) / factor.x;
            float y = floor(i.uv.y * factor.y + 0.5) / factor.y;
            return tex2D(_MainTex, float2(x, y));
        }
        ENDCG
        }
    }
    FallBack "Diffuse"
}
