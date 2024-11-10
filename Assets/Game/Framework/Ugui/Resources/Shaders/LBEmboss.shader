// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Effect/Emboss" 
{
	Properties 
	{
		_MainTex		("MainTex", 2D)			= "white" {}
		_SubTex			("Sub Texture",2D)		= "black" {}
		_OverlayTex		("OverlayTex",2D)		= "white" {}

		_MainTexSize	("MainTexSize", float)	= 256
		_SubTexSize		("SubTexSize", float)	= 256

		_OverlayIntensity("OverlayIntensity",float) = 1

		_Color			("Main Color",color)	= (1,1,1,1)
		_ShadowDirX		("Shadow Direction X",float) = 1 
		_ShadowDirY		("Shadow Direction Y",float) = -1
		_ShadowPower	("Shadow Power",float)		 = 2
		_EgdeSmooth		("Egde Smooth",float)		 = 4
		_SubTexDepth	("SubTex Depth",float)		 = 1
		_MaxBrightness	("Max Brightness",float)	 = 2		
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Lighting Off 
			ZWrite On
			Cull Off

			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4		_Color;
			float		_MainTexSize;
			float		_SubTexSize;
			sampler2D	_MainTex;
			sampler2D   _SubTex;
			sampler2D   _OverlayTex;

			float		_OverlayIntensity;
			float4		_MainTex_ST;
			float4		_SubTex_ST;
			float4		_OverlayTex_ST;

			float		_ShadowDirX;
			float		_ShadowDirY;
			float		_ShadowPower;
			float		_EgdeSmooth;
			float		_SubTexDepth;
			float		_MaxBrightness;

			struct indata 
			{
				float4 vertex    :POSITION;
				float2 texcoord	 :TEXCOORD0;
			};

			struct outdata 
			{
				float4 pos			 :POSITION;
				float2 uv_MainTex	 :TEXCOORD0;
				float2 uv_SubUvs	 :TEXCOORD1;
				float2 uv_OverlayUvs :TEXCOORD2;
			};

			outdata vert (indata v) 
			{
				outdata o;
				o.pos			 = UnityObjectToClipPos(v.vertex);
				o.uv_MainTex	 = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv_SubUvs		 = TRANSFORM_TEX(v.texcoord,_SubTex);
				o.uv_OverlayUvs	 = TRANSFORM_TEX(v.texcoord,_OverlayTex);

				return o;
			}

			half overlay(half s,half d)
			{
				return d>0.5?1-(1-s)*(1-d)*2:2*s*d;
			}

			half4 frag(outdata i):COLOR
			{
				//float
				half4  mainColor	   = tex2D (_MainTex, i.uv_MainTex);
				half   mainOffsetAlpha = tex2D (_MainTex, i.uv_MainTex + fixed2(_ShadowDirX,_ShadowDirY)/_MainTexSize).a;
			
				half4 subC1 = tex2D (_SubTex, i.uv_SubUvs);
				half4 subC2 = tex2D (_SubTex, i.uv_SubUvs + fixed2(0,1)/_SubTexSize);
				
				half4 overlayColor = tex2D(_OverlayTex,i.uv_OverlayUvs);
				
				mainColor *= _Color;

				overlayColor.r = overlay(overlayColor.r,mainColor.r);
				overlayColor.g = overlay(overlayColor.g,mainColor.g);
				overlayColor.b = overlay(overlayColor.b,mainColor.b);

				mainColor = half4(lerp(mainColor.rgb, overlayColor.rgb, (_OverlayIntensity)), mainColor.a);

				float maxDiff;
				float diff;
				float powv;

				maxDiff = (mainOffsetAlpha - mainColor.a);
				diff = subC1.a - subC2.a;
				if(abs(diff) > abs(maxDiff))
				   maxDiff = diff;

				powv = pow(mainColor.a,_EgdeSmooth) * _SubTexDepth;
				diff = subC1.a - subC2.a;
				diff *= powv;
				if(abs(diff) > abs(maxDiff))
				   maxDiff = diff;

				diff = subC1.r - subC2.r;
				diff *= powv;
				if(abs(diff) > abs(maxDiff))
				   maxDiff = diff;

				diff = subC1.g - subC2.g;
				diff *= powv;
				if(abs(diff) > abs(maxDiff))
				   maxDiff = diff;

				diff = subC1.b - subC2.b;
				diff *= powv;
				if(abs(diff) > abs(maxDiff))
				   maxDiff = diff;

				maxDiff = clamp(1 + maxDiff *_ShadowPower,0,_MaxBrightness);

				half4 retcolor;
				retcolor = half4(mainColor.rgb * maxDiff , mainColor.a);
				
				return retcolor;
			}
			ENDCG
		}
	} 
}