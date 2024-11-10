// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/HueAdjust"
{
	Properties 
	{
		//使用计算好的色相 满饱和S 满亮度B 的颜色，提高shader计算速度
		_Color		("Hue Color",Color) = (1,0,0,1)
		_MainTex	("Texture",2D)		= "white"{}
		_Mask		("Mask",2D)			= "white"{}
		_Cutoff		("Alpha cutoff", Range (0.0,0.9))  = 0.5
		_HueFactor  ("Hue Factor",   Range (-1.0,1.0)) = 1.0
	}

	SubShader 
	{
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
		Lighting off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		// first pass:
		// render any pixels that are more than [_Cutoff] opaque
		Pass 
		{  
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 vertex	: POSITION;
				float2 texcoord : TEXCOORD0;
			};

			uniform sampler2D _Mask;
			uniform sampler2D _MainTex;
			uniform float4	  _MainTex_ST;
			
			uniform float	  _Cutoff;
			uniform float4	  _Color;
			uniform float	  _HueFactor;

			v2f vert (v2f v)
			{
				v2f o;
				o.vertex	= UnityObjectToClipPos(v.vertex);
				o.texcoord	= TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex, i.texcoord);
				col.a *= _Color.a;
				clip(col.a - _Cutoff);

				if(_HueFactor <= 0.0)
				{
					return col;	
				}
				else
				{
					half   max_c;
					half   min_c; 

					half  b,s;//HSB
					half4 newCol;

					_HueFactor *= tex2D(_Mask,i.texcoord).r;

					// 计算出HSB 中的饱和度 和 亮度
					// 改变色相 保留原图的[反转饱和度]和[亮度]
					max_c = max(max(col.r,col.g),col.b);
					min_c = min(min(col.r,col.g),col.b);
				
					b = max_c / 1.0;
					s = min_c / max_c;

					newCol.a   = col.a;
					newCol.rgb = _Color.rgb * b;
					max_c = max(max(newCol.r,newCol.g),newCol.b);
					newCol.rgb = lerp(newCol.rgb,max_c,s);

					return lerp(col,newCol,_HueFactor);
				}
			}
			ENDCG
		}

		//	 Second pass:
		//   render the semitransparent details.
		Pass 
		{
			Tags { "RequireOption" = "SoftVegetation" }
			ZWrite off
			Blend SrcAlpha OneMinusSrcAlpha
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			uniform sampler2D _Mask;
			uniform sampler2D _MainTex;
			uniform float4	  _MainTex_ST;
			uniform float	  _Cutoff;
			uniform float4	  _Color;
			uniform float	  _HueFactor;

			v2f vert (v2f v)
			{
				v2f o;
				o.vertex   = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex, i.texcoord);
				col.a *= _Color.a;
				clip(-(col.a - _Cutoff));

				if(_HueFactor <= 0.0)
				{
					return col;	
				}
				else
				{
					half   max_c;
					half   min_c; 

					half  b,s;//HSB
					half4 newCol;

					_HueFactor *= tex2D(_Mask,i.texcoord).r;

					// 计算出HSB 中的饱和度 和 亮度
					// 改变色相 保留原图的[反转饱和度]和[亮度]
					max_c = max(max(col.r,col.g),col.b);
					min_c = min(min(col.r,col.g),col.b);
				
					b = max_c / 1.0;
					s = min_c / max_c;

					newCol.a   = col.a;
					newCol.rgb = _Color.rgb * b;
					max_c = max(max(newCol.r,newCol.g),newCol.b);
					newCol.rgb = lerp(newCol.rgb,max_c,s);

					return lerp(col,newCol,_HueFactor);
				}
			}
			ENDCG
		}
	}
}
