// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/RuleImage"
{
    Properties
	{
		_MainTex ("Texture", 2D) = "white" {} //Main Texture, leave empty
		_PatternTex ("Pattern Texture", 2d) = "white" {} //The pattern transition texture
		_Cutoff("Progress", Range (0, 1)) = 0 //Cut off slider
		_Factor("Edge Transition", Range(0, 1)) = 0.1 //factor of the edge transition
		_Color("Color", Color) = (0,0,0,0) //defaults to black
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "RenderQueue"="Transparent" }
		
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		//Enable alpha blend
		Blend SrcAlpha OneMinusSrcAlpha
		
		
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
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			//sampler2D _TranTex;
			sampler2D _PatternTex;
			fixed _Cutoff;
			fixed4 _Color;
			fixed _Factor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed cast_value(fixed value, fixed oldmin, fixed oldmax, fixed newmin, fixed newmax)
			{
				value = clamp(oldmin, oldmax, value);
				return (value-oldmin)/(oldmax-oldmin)*(newmax-newmin)+newmin;
			}

			
			fixed4 frag(v2f i) : SV_Target
    		{
    		    _Cutoff = 1 - cast_value(_Cutoff, 0.1, 0.9, 0.0, 1.0);
    			
    			if (_Cutoff <= 0.1) _Factor = cast_value(_Cutoff, 0.0, 0.1, 0.0, _Factor);
    			else if (_Cutoff >= 0.9) _Factor = cast_value(_Cutoff, 0.9, 1.0, _Factor, 0.0);

    		    fixed4 transit = tex2D(_PatternTex, i.uv);
    		    //fixed4 tranTex = tex2D(_TranTex, i.uv);
    			
    		    if(_Cutoff < 0.005) return _Color; //Edge cut

    			fixed fmin = _Cutoff - _Factor;
    			fixed fmax = _Cutoff + _Factor;

    			fixed pct = (transit.r - fmin) / (fmax - fmin);
    			fixed res = smoothstep(0, 1, pct);
    			return _Color * res;
    		}
			ENDCG
		}
	}
}
