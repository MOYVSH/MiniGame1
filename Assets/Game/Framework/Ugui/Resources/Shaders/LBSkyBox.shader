// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/SkyBox" 
{
	Properties 
	{
		_Cube ("Environment Map", Cube) = "white" {}
		_Color ("Main Color", Color)    = (1,1,1,1)
	}

	SubShader 
	{  
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

        Pass 
		{  
			CGPROGRAM  
  
            #pragma vertex vert  
            #pragma fragment frag  
			#include "UnityCG.cginc"

            uniform samplerCUBE	_Cube; 
            uniform float4		_Color;   
              
            struct data 
			{  
                float4 pos : POSITION;
				float3 wpos : TEXCOORD0; 
            };  
  
            data vert( data i)
			{  
                data o;  
                o.pos  = UnityObjectToClipPos (i.pos);	
				o.wpos = mul(unity_ObjectToWorld,i.pos);			
                return o;  
            }  
  
            half4 frag (data i) : COLOR 
			{ 
                return texCUBE(_Cube, i.wpos - _WorldSpaceCameraPos) * _Color;  
            }  
            ENDCG  
        } 
	}
}
