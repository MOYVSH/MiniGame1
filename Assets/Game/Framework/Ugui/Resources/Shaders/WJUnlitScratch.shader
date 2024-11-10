// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Scratch" 
{
	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off 
		ZWrite Off
		Cull Off
		Pass 
		{ 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			struct v_data 
			{
				float4 pos : POSITION;
				float4 col : COLOR;
			};
			
			v_data vert(v_data i)
			{
				v_data o;
				o.col = i.col;
				o.pos = UnityObjectToClipPos(i.pos);
				return o;
			}
			
			float4 frag(v_data i) : COLOR
			{
				return i.col; 
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	
}