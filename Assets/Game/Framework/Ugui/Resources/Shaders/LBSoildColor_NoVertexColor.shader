// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/SoildColor_NoVertexColor" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
	}

	SubShader 
	{  
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off 
		Cull Off

        Pass 
		{  
			CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  
			#include "UnityCG.cginc"
            uniform float4 _Color;  
            float4 vert(float4 pos : POSITION): POSITION
			{ 
                return UnityObjectToClipPos ( pos);   
            }  
  
            fixed4 frag (void) : COLOR 
			{ 
				return _Color;  
			}  
            ENDCG  
        } 
	}
}
