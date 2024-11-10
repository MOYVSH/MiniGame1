// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/EnvironmentReflect" 
{
	Properties 
	{
		_Cube  ("Environment Map", Cube) = "white" {}
		_Color ("Main Color", Color)     = (1,1,1,1)
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
                float4 pos	  : POSITION;
				float3 normal : NORMAL; 
            }; 

			struct outdata 
			{  
                float4 pos : POSITION;
				float3 ref : TEXCOORD0; 
            }; 
  
            outdata vert( data i)
			{  
                outdata o;  
                o.pos   = UnityObjectToClipPos (i.pos);	
		
				fixed3 posWorld = mul(unity_ObjectToWorld, i.pos).xyz;
				fixed3 normalDirection = normalize(fixed3(mul(unity_ObjectToWorld,fixed4(i.normal, 0.0)).xyz));
				fixed3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);
				o.ref = reflect( -fixed4(viewDirection,0.0), fixed4(normalDirection,0.0) );
							
                return o;  
            }  
  
            half4 frag (outdata i) : COLOR 
			{ 
                return texCUBE(_Cube, i.ref) * _Color;  
            }  
            ENDCG  
        } 
	}
}
