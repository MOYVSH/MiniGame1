// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Color" 
{
	Properties 
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
	}

	SubShader 
	{  
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off 
		ZWrite On
		Cull Off

        Pass 
		{  
			CGPROGRAM  
  
            #pragma vertex vert  
            #pragma fragment frag  
			#include "UnityCG.cginc"

            uniform sampler2D _MainTex; 
			uniform float4    _MainTex_ST; 
            uniform float4    _Color;   
              
            struct data 
			{  
                float4 pos : POSITION;  
                float2 uv  : TEXCOORD0;  
            };  
  
            data vert( data i)
			{  
                data o;  
                o.pos = UnityObjectToClipPos (i.pos); 
				o.uv  = TRANSFORM_TEX(i.uv,_MainTex); 
                return o;  
            }  
  
            half4 frag (data i) : COLOR 
			{ 
                return tex2D( _MainTex, i.uv) * _Color;  
            }  
            ENDCG  
        } 
	}
}
