// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Effect/OutLine" 
{
	Properties 
	{
		_AlphaEnhance ("AlphaEnhance", range(0,5)) = 1
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (0.0, 0.03)) = .005
		_MainTex ("Base (RGB)", 2D) = "white" { }
	}

	SubShader 
	{
		Tags { "Queue" = "Transparent+1001" }

		// Draw Mesh.
		Pass 
		{
			Stencil 
			{
                Ref 100
                Pass replace
            }

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _MainTex;
			uniform float4    _MainTex_ST;
			uniform float     _AlphaEnhance;

			struct p_u 
			{
				float4 pos : POSITION;
				float2 uv  :TEXCOORD0;
			};

			p_u vert(p_u i) 
			{
				p_u o;
				o.pos = UnityObjectToClipPos(i.pos);
				o.uv  = TRANSFORM_TEX(i.uv,_MainTex);
				return o;
			}

			half4 frag(p_u i) :COLOR 
			{
				half4 h4;
				h4    = tex2D(_MainTex,i.uv);
				h4.a *=_AlphaEnhance;
				return h4;
			}
			ENDCG
		}

		// Render Out Line.
		Pass 
		{	
			Stencil 
			{
                Ref 100
                Comp NotEqual
            }

			Blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			
			uniform float _Outline;
			uniform float4 _OutlineColor;

			struct appdata 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct v2f 
			{
				float4 pos : POSITION;
				float4 color : COLOR;
			};


			v2f vert(appdata v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				float2 offset = TransformViewToProjection(norm.xy);
				o.pos.xy += offset * o.pos.z * _Outline;
				o.color = _OutlineColor;
				return o;
			}

			half4 frag(v2f i) :COLOR 
			{
				return i.color;
			}
			ENDCG
		}
		

	}
	Fallback "Diffuse"
}