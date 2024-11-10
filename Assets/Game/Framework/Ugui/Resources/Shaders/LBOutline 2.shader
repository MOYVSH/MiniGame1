// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Effect/OutLine 2" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (0.0, 0.01)) = .005
	
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

		// Draw Mesh.
		Pass 
		{
			Stencil 
			{
                Ref 100
                Pass replace
            }

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite off

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			
			uniform float4 _Color;

			struct appdata 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct outappdata 
			{
				float4 vertex : POSITION;
				float  alpha : TEXCOORD;
			};

			outappdata vert(appdata i)
			{
				outappdata o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.alpha  = 1-saturate(dot(normalize(mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1)) - i.vertex),normalize(i.normal)));
				return o;
			}

			half4 frag(outappdata o) :COLOR 
			{
				return half4(_Color.rgb,o.alpha*_Color.a);
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
			ZWrite off

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

			float4 vert(appdata v) : POSITION
			{
				float4 pos;
				pos = UnityObjectToClipPos(v.vertex);
				float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				float2 offset = TransformViewToProjection(norm.xy);
				pos.xy += offset * pos.z * _Outline;
				return pos;
			}

			half4 frag() : COLOR 
			{
				return _OutlineColor;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}