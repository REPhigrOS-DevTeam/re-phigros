// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/RuleImage"
{
    Properties
	{
		_MainTex ("Texture", 2D) = "white" {} //Main Texture, leave empty
		_TranTex ("Transition Texture", 2D) = "black" {} //Transition texture. Leave blank if using color
		_PatternTex ("Pattern Texture", 2d) = "white" {} //The pattern transition texture
		_Cutoff("Progress", Range (0, 1)) = 0 //Cut off slider
		_Factor("Edge Transition", Range(1, 10)) = 2.5 //factor of the edge transition
		_Color("Color", Color) = (0,0,0,0) //defaults to black
		[MaterialToggle] _UseColor("Use Color", Float) = 0 //color or texture toggle
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
			sampler2D _TranTex;
			sampler2D _PatternTex;
			float _Cutoff;
			fixed4 _Color;
			float _UseColor;
			half _Factor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 transit = tex2D(_PatternTex, i.uv);
				fixed4 tranTex = tex2D(_TranTex, i.uv);
				//checks the blue attribute of the pattern and cuts off in respect to that
				if(transit.r < _Cutoff){
					// if _UseColor is enabled, use color instead
					if(_UseColor > 0)
						return _Color;
					// _Use color is not enabled, use the _TranTex
					return tranTex;
				}
				if(_UseColor > 0)
					return _Color * _Cutoff * (1 - _Factor * (transit.r - _Cutoff));
				return tranTex * _Cutoff * (1 - _Factor * (transit.r - _Cutoff));
			}
			ENDCG
		}
	}
}
