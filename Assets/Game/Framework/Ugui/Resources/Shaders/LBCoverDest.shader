// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/CoverDest" 
{
	Properties 
	{
		_MainTex("MainTex",2D) = "White"{}
		_Color	("Color",COLOR) = (1,1,1,0)
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

		Blend One Zero
		Cull Off 
		Lighting Off
		ZWrite Off
		ZTest Always 

		Pass
		{
			CGPROGRAM
			#pragma vertex   vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4	  _Color;
			struct _data
			{
				float4 pos:POSITION;
				float2 uvs:TEXCOORD0;
			};

			_data vert(_data i)
			{
				_data o;
				o.pos = UnityObjectToClipPos(i.pos);
				o.uvs = i.uvs;
				return o;
			}

			half4 frag(_data i):COLOR
			{
				return _Color * tex2D(_MainTex,i.uvs);
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
