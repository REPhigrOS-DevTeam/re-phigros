Shader "Phira/glitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _power ("Power", Range(0.0, 1.0)) = 0.03
        _rate ("Rate", Range(0.0, 1.0)) = 0.6
        _speed ("Speed", Range(0.0, 10.0)) = 5.0
        _blockCount ("Block Count", Range(0.0, 100.0)) = 30.5
        _colorRate ("Color Rate", Range(0.0, 1.0)) = 0.01
    }

    SubShader
    {
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
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _power;
            float _rate;
            float _speed;
            float _blockCount;
            float _colorRate;

            float my_trunc(float x) {
              return x < 0.0? -floor(-x): floor(x);
            }

            float random(float seed) {
                float t = (543.2543 * sin(dot(float2(seed, seed), float2(3525.46, -54.3415))));
              return t - floor(t);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float enable_shift = float(random(my_trunc(_Time.y * _speed)) < _rate);

                float2 fixed_uv = i.uv;
                fixed_uv.x += (random((my_trunc(i.uv.y * _blockCount) / _blockCount) + _Time.y) - 0.5) * _power * 0.5 * enable_shift;

                fixed4 pixel_color = tex2D(_MainTex, fixed_uv);
                pixel_color.r = lerp(
                    pixel_color.r,
                    tex2D(_MainTex, fixed_uv + float2(_colorRate, 0.0)).r,
                    enable_shift
                );
                pixel_color.b = lerp(
                    pixel_color.b,
                    tex2D(_MainTex, fixed_uv + float2(-_colorRate, 0.0)).b,
                    enable_shift
                );
                return pixel_color;
            }
            ENDCG
        }
    }
}
