// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Water Face" 
{
	Properties
	{
		_Color		("Color",Color)		= (1,1,1,1)
		//_MainTex	("MainTex",2D)		    ="white"{}
		_MaxHeight	("maxH",float)		= 0
		_GlowWith	("_GlowWith",float) = 0.005
		_GlowColor	("GlowColor",Color) = (1,1,1,1)
	}
	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
			
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
		
			CGPROGRAM
			#pragma vertex	vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform float4		_Color;
			//uniform sampler2D	_MainTex;
			//uniform float4		_MainTex_ST;
			uniform float		_MaxHeight;
			uniform float		_GlowWith;
			uniform float4		_GlowColor;

			struct Input
			{
				float4 pos:POSITION;
				//float2 uv:TEXCOORD;
			};

			struct Output
			{
				float4 pos:POSITION;
				float4 wpos:TEXCOORD2;
				//float2 uv:TEXCOORD;
			};


			Output vert(Input i)
			{
				Output o;
				o.pos  = UnityObjectToClipPos(i.pos);
				o.wpos = mul(unity_ObjectToWorld,i.pos);
				//o.uv   = TRANSFORM_TEX(i.uv, _MainTex);
				return o;
			}

			half4 frag(Output o):COLOR
			{
				half4 c;
				c = _Color;
		
				if(o.wpos.y < _MaxHeight)
				{
					if(_GlowWith > 0)
					{
						float v;
						v = clamp((_MaxHeight - o.wpos.y)/_GlowWith,0,1);
						c = lerp(_GlowColor,_Color,v);
					}
				}
				else
				{
					discard;
				}
				return c;
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
