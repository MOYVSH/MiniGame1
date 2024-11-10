// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Fluid Wave" 
{
	Properties 
	{
		_Color ("Color", Color) = (1.0,1.0,1.0,1.0)
		_Smooth ("Smooth Value", Range(1.0,10.0)) = 6.0
	}
	
	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		AlphaTest Greater .01
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off 
		ZWrite On

		Pass 
		{ 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			uniform float4 _Color;
			uniform float _Smooth;
			
			struct vertexInput 
			{
				float4 vertex : POSITION;
				float4 col	  : COLOR;
				float3 normal : NORMAL;
			};

			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
			};
			
			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				float3 viewDirection = normalize(mul(unity_WorldToObject,float4(_WorldSpaceCameraPos.xyz,1.0f)) -  v.vertex);
				float factor = dot(viewDirection,v.normal);
				
				factor += 1.0f;
				factor /= 2.0f;
				factor = pow(factor,_Smooth);
				factor += 1.0f;

				o.col = _Color * factor;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			float4 frag(vertexOutput i) : COLOR
			{
				i.col.a = _Color.a;
				return i.col; 
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	
}