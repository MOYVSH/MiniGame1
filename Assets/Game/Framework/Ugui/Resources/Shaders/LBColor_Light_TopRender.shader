// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Color_Light_TopRender" 
{
	Properties 
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
	}

	SubShader 
	{  
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend Zero One
		// Pass 1. Çå³ýÉî¶È
		Pass
		{
			Cull off
			ZTest Greater	

			CGPROGRAM
			#pragma vertex   vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 vert(float4 pos : POSITION):POSITION
			{
				return UnityObjectToClipPos(pos);
			}

			fixed4 frag() : COLOR
			{
				return fixed4(0,0,0,0);
			}
			ENDCG
		}

		Blend SrcAlpha OneMinusSrcAlpha
        Pass 
		{  
			ZTest LEqual

			CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  
			#include "UnityCG.cginc"

            uniform sampler2D _MainTex; 
			uniform float4    _MainTex_ST; 
            uniform half4     _Color; 
			uniform fixed4	  _LightColor0;  
              
            struct in_data 
			{  
                float4 pos		: POSITION; 
				fixed3 normal 	: NORMAL; 
                fixed2 uv		: TEXCOORD0;  
            };  
  
			struct out_data 
			{  
                float4 pos		: POSITION;
                fixed2 uv		: TEXCOORD0;
				fixed4 col		: COLOR0;
            };  

            out_data vert( in_data i)
			{  
                out_data o;  
                o.pos = UnityObjectToClipPos (i.pos); 
				o.uv  = TRANSFORM_TEX(i.uv,_MainTex); 

				float3 posWorld = mul(unity_ObjectToWorld, i.pos).xyz;
				fixed3 normalDirection = normalize(fixed3(mul(unity_ObjectToWorld,fixed4(i.normal, 0.0)).xyz));
    			fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				fixed3 diffuseReflection = _LightColor0.rgb * max(0.0, dot(normalDirection, lightDirection));
			
				o.col = fixed4((UNITY_LIGHTMODEL_AMBIENT.rgb + diffuseReflection), 1.0);

                return o;  
            }  
  
            half4 frag (out_data i) : COLOR 
			{ 
				half4 texCol = tex2D(_MainTex, i.uv);
				return fixed4(texCol.rgb * i.col.rgb * _Color.rgb * 2, _Color.a * texCol.a);
            }  
            ENDCG  
        } 
	}
	Fallback "Diffuse"
}
