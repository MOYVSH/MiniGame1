// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/SoftEdge_TopRender" 
{
	Properties 
	{
		_MainTex ("Main Tex"	, 2D)				= "White" {}
		_Color	 ("Color"		, Color)			= (1,1,1,1)
		_Cutoff  ("Alpha Cutoff", Range (0.0,0.9))	= 0.5
	}
	
	SubShader 
	{
		Tags { "Queue"="Transparent+100" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }

		Lighting off
		Cull	 off
		
		Blend Zero One
		// Pass 1. 清除重写深度
		Pass
		{
			ZWrite on
			ZTest  Greater	

			CGPROGRAM
			#pragma vertex   vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v1f
			{
				float4 pos : POSITION;
			};

			v1f vert(v1f i)
			{
				v1f o;
				o.pos = UnityObjectToClipPos(i.pos);
				return o;
			}

			half4 frag() : COLOR
			{
				return half4(0,0,0,0);
			}
			ENDCG
		}

		Blend SrcAlpha OneMinusSrcAlpha
		// Pass 2. 渲染期望不透明处
		Pass
		{
			ZWrite on
			ZTest  LEqual

			CGPROGRAM
			#pragma vertex   vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4    _MainTex_ST;

			uniform float	  _Cutoff;
			uniform float4	  _Color;

			struct v2f
			{
				float4  pos : POSITION;
				float2  uvs	: TEXCOORD0; 
			};

			v2f vert(v2f i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(i.pos);
				o.uvs = TRANSFORM_TEX(i.uvs,_MainTex);
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex,i.uvs); 
				clip(col.a - _Cutoff);
				return col * _Color;
			}
			ENDCG
		}

		// Pass 3. 渲染期望半透明处
		Pass
		{
			ZWrite off
			ZTest  LEqual

			CGPROGRAM
			#pragma vertex   vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4    _MainTex_ST;

			uniform float	  _Cutoff;
			uniform float4	  _Color;

			struct v2f
			{
				float4  pos : POSITION;
				float2  uvs	: TEXCOORD0; 
			};

			v2f vert(v2f i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(i.pos);
				o.uvs = TRANSFORM_TEX(i.uvs,_MainTex);
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex,i.uvs); 
				clip(_Cutoff - col.a);
				return col * _Color;
			}

			ENDCG
		}
	} 
}
