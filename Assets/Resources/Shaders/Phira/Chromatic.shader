Shader "Phira/chromatic" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _sampleCount ("Sample Count", Range(1, 64)) = 3
        _power ("Power", Range(0.01, 1)) = 0.01
    }

    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

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
            float _sampleCount;
            float _power;

            float3 chromatic_slice(float t) {
              float3 res = float3(1.0 - t, 1.0 - abs(t - 1.0), t - 1.0);
              return max(res, 0.0);
            }

            fixed4 frag (v2f i) : SV_Target {
              float3 sum = float3(0.0, 0.0, 0.0);
              float3 c = float3(0.0, 0.0, 0.0);
              float2 offset = (i.uv - float2(0.5, 0.5)) * float2(1, -1);
              int sample_count = int(_sampleCount);
              for (int j = 0; j < 64; ++j) {
                if (j >= sample_count) break;
                float t = 2.0 * float(j) / float(sample_count - 1); // range 0.0->2.0
                float3 slice = float3(1.0 - t, 1.0 - abs(t - 1.0), t - 1.0);
                slice = max(slice, 0.0);
                sum += slice;
                float2 slice_offset = (t - 1.0) * _power * .5 * offset;
                c += slice * tex2D(_MainTex, i.uv + slice_offset).rgb;
              }
              return fixed4(c / sum, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}