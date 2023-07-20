Shader "Phira/radialBlur"
{
    Properties
    {
        _centerX ("Center X", Range(0.0, 1.0)) = 0.5
        _centerY ("Center Y", Range(0.0, 1.0)) = 0.5
        _power ("Power", Range(0.0, 1.0)) = 0.01
        _sampleCount ("Sample Count", Range(1, 64)) = 6
        _MainTex ("Texture", 2D) = "white" {}
    }
 
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            float _centerX;
            float _centerY;
            float _power;
            float _sampleCount;
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                float2 direction = i.uv - float2(_centerX, _centerY);
                float3 c = float3(0.0, 0.0, 0.0);
                float f = 1.0 / _sampleCount;
                float2 screen_uv = i.uv / 2.0 + float2(0.5, 0.5);
                for (float j = 0.0; j < 64.0; ++j)
                {
                    if (j >= _sampleCount) break;
                    c += tex2D(_MainTex, i.uv - _power * direction * j).rgb * f;
                }
                return fixed4(c, 1.0);
            }
            ENDCG
        }
    }
}
