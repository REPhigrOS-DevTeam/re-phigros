Shader "Unlit/FrameAnimation"
{
Properties
	{
		_Color("Color Tint",Color) = (1,1,1,1)
		_SequenceTexture ("Sequence Texture", 2D) = "white" {}
		_RowAmount("Row Amount",Int) = 8
		_ColAmount("Col Amount",Int) = 8
		_FPS("FPS",Int) = 30

	}
		SubShader
		{
			//由于序列帧图像通常是透明背景，所以需要设置pass的相关状态，以渲染透明效果
			//半透明“标配”
			Tags { "Queue" = "Transparent" "IgnoreProject" = "True" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"}

			Pass
			{
				Tags{"LightMode" = "UniversalForward"}
				//关闭深度写入
				ZWrite off
			//开启并设置混合模式
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			fixed4 _Color;
			sampler2D _MainTex;
		    float4 _MainTex_ST;
			float _HorizontalAmount;
			float _VerticalAmount;
			float _Speed;

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

			sampler2D _SequenceTexture;
			float4 _SequenceTexture_ST;
			int _RowAmount;
			int _ColAmount;
			int _FPS;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _SequenceTexture);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//获取当前帧的索引：时间 * 帧率 % 序列帧个数
				float frameIndex = floor(_Time.y * _FPS) % (_RowAmount * _ColAmount);
				//转换成行列索引
				float rowIndex = floor(frameIndex / _RowAmount);
				float colIndex = frameIndex - rowIndex * _RowAmount;

				//计算当前帧的开始UV坐标:我们计算行列是从左上角开始的，而UV是从左下角开始的，Y坐标需要转换一下
				half2 uvFrameStart = half2(colIndex / _ColAmount, 1.0 - rowIndex / _RowAmount);
				//在帧内的UV偏移量
				half2 uvOffset = half2(i.uv.x / _ColAmount, i.uv.y / _RowAmount);
				//最终UV坐标
				half2 uv = uvFrameStart + uvOffset;

				fixed4 c = tex2D(_SequenceTexture,uv);

				return c;
			}
			ENDCG
		}
	}
			FallBack "Transparent/VertexLit"
}