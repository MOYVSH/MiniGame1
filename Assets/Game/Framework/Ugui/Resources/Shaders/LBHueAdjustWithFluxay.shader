// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/HueAdjustWithFluxay"
{
	Properties 
	{
		//使用计算好的色相 满饱和S 满亮度B 的颜色，提高shader计算速度
		_Color		("Hue Color",Color) = (1,0,0,1)
		_MainTex	("Texture",2D)		= "white"{}
		_Mask		("Mask",2D)			= "white"{}
		_Cutoff		("Alpha cutoff", Range (0.0,0.9))  = 0.5
		_HueFactor  ("Hue Factor",   Range (-1.0,1.0)) = 1.0


		_FluxaySmooth	  ("=Fluxay Smooth=",		Range (0.1,5.0))  = 1.0
		_FluxayHighlight  ("=Fluxay Highlight=",	Range (0.1,5.0))  = 1.0


		_FluxayX	  ("[Open Model-X-Fluxay]", Range(-1.0,1.0)) = 1.0
		_FluxayXWidth (" ------FluxayWidth",	Range(0.01,1.0)) = 0.1
		_FluxayXSpeed (" ------FluxaySpeed",    float) = 1
		_FluxayXRange (" ------FluxayInterval", Range(0.1,10.0)) = 1

		_FluxayY	  ("[Open Model-Y-Fluxay]", Range(-1.0,1.0)) = 1.0
		_FluxayYWidth (" ------FluxayWidth",	Range(0.01,1.0)) = 0.1
		_FluxayYSpeed (" ------FluxaySpeed",    float) = 1
		_FluxayYRange (" ------FluxayInterval", Range(0.1,10.0)) = 1

		_FluxayZ	  ("[Open Model-Z-Fluxay]", Range(-1.0,1.0)) = 1.0
		_FluxayZWidth (" ------FluxayWidth",	Range(0.01,1.0)) = 0.1
		_FluxayZSpeed (" ------FluxaySpeed",    float) = 1
		_FluxayZRange (" ------FluxayInterval", Range(0.1,10.0)) = 1
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
			#pragma target 3.0
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 vertex	: POSITION;
				float2 texcoord : TEXCOORD0;
				float4 localPos : TEXCOORD2;
				float3 fluxayPos: TEXCOORD3;
			};

			uniform sampler2D _Mask;
			uniform sampler2D _MainTex;
			uniform float4	  _MainTex_ST;
			
			uniform float	  _Cutoff;
			uniform float4	  _Color;
			uniform float	  _HueFactor;

			uniform float	  _FluxayX;
			uniform float	  _FluxayXWidth;
			uniform float	  _FluxayXSpeed;
			uniform float	  _FluxayXRange;

			uniform float	  _FluxayY;
			uniform float	  _FluxayYWidth;
			uniform float	  _FluxayYSpeed;
			uniform float	  _FluxayYRange;

			uniform float	  _FluxayZ;
			uniform float	  _FluxayZWidth;
			uniform float	  _FluxayZSpeed;
			uniform float	  _FluxayZRange;

			uniform float	  _FluxaySmooth;
			uniform float	  _FluxayHighlight;

			v2f vert (v2f v)
			{
				v2f o;
				o.vertex	  = UnityObjectToClipPos(v.vertex);
				o.texcoord	  = TRANSFORM_TEX(v.texcoord, _MainTex);

				o.localPos	  = v.vertex;
				o.fluxayPos.x = fmod(_Time.y*_FluxayXSpeed,_FluxayXRange*2.0) - _FluxayXRange;
				o.fluxayPos.y = fmod(_Time.y*_FluxayYSpeed,_FluxayYRange*2.0) - _FluxayYRange;
				o.fluxayPos.z = fmod(_Time.y*_FluxayZSpeed,_FluxayZRange*2.0) - _FluxayZRange;

				return o;
			}
		
			half calcFluxayFactor(float isOpen,float diff,float width)
			{
				half ret = 1.0;
				if(isOpen > 0.0 && diff < width)
					ret += pow((width - diff)/width,_FluxaySmooth)*_FluxayHighlight;
				return ret;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex, i.texcoord);
				col.a *= _Color.a;
				clip(col.a - _Cutoff);
				
				//------ Fluxay ----------

				col.rgb *= calcFluxayFactor(_FluxayX,abs(i.localPos.x - i.fluxayPos.x),_FluxayXWidth);
				col.rgb *= calcFluxayFactor(_FluxayY,abs(i.localPos.y - i.fluxayPos.y),_FluxayYWidth);
				col.rgb *= calcFluxayFactor(_FluxayZ,abs(i.localPos.z - i.fluxayPos.z),_FluxayZWidth);

				//------ End Fluxay ------


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
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			struct v2f 
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 localPos : TEXCOORD2;
				float3 fluxayPos: TEXCOORD3;
			};

			uniform sampler2D _Mask;
			uniform sampler2D _MainTex;
			uniform float4	  _MainTex_ST;
			uniform float	  _Cutoff;
			uniform float4	  _Color;
			uniform float	  _HueFactor;

			uniform float	  _FluxayX;
			uniform float	  _FluxayXWidth;
			uniform float	  _FluxayXSpeed;
			uniform float	  _FluxayXRange;

			uniform float	  _FluxayY;
			uniform float	  _FluxayYWidth;
			uniform float	  _FluxayYSpeed;
			uniform float	  _FluxayYRange;

			uniform float	  _FluxayZ;
			uniform float	  _FluxayZWidth;
			uniform float	  _FluxayZSpeed;
			uniform float	  _FluxayZRange;

			uniform float	  _FluxaySmooth;
			uniform float	  _FluxayHighlight;

			v2f vert (v2f v)
			{
				v2f o;
				o.vertex      = UnityObjectToClipPos(v.vertex);
				o.texcoord	  = TRANSFORM_TEX(v.texcoord, _MainTex);

				o.localPos	  = v.vertex;
				o.fluxayPos.x = fmod(_Time.y*_FluxayXSpeed,_FluxayXRange*2.0) - _FluxayXRange;
				o.fluxayPos.y = fmod(_Time.y*_FluxayYSpeed,_FluxayYRange*2.0) - _FluxayYRange;
				o.fluxayPos.z = fmod(_Time.y*_FluxayZSpeed,_FluxayZRange*2.0) - _FluxayZRange;
				
				return o;
			}
			
			half calcFluxayFactor(float isOpen,float diff,float width)
			{
				half ret = 1.0;
				if(isOpen > 0.0 && diff < width)
					ret += pow((width - diff)/width,_FluxaySmooth)*_FluxayHighlight;
				return ret;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex, i.texcoord);
				col.a *= _Color.a;
				clip(-(col.a - _Cutoff));
				
				//------ Fluxay ----------

				col.rgb *= calcFluxayFactor(_FluxayX,abs(i.localPos.x - i.fluxayPos.x),_FluxayXWidth);
				col.rgb *= calcFluxayFactor(_FluxayY,abs(i.localPos.y - i.fluxayPos.y),_FluxayYWidth);
				col.rgb *= calcFluxayFactor(_FluxayZ,abs(i.localPos.z - i.fluxayPos.z),_FluxayZWidth);

				//------ End Fluxay ------

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
