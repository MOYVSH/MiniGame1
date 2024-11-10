// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Effect/Distorted" 
{
	Properties 
	{
		_MainTex	("MainTex", 2D)		= "white" {}
		_NoiseTex	("NoiseTex", 2D)	= "white" {}     
		 
		_MoveSpeed	("MoveSpeed", range (0,1.5)) = 1        
		_MoveForce	("MoveForce", range (0,0.1)) = 0.1     
	}

	SubShader 
	{
		Pass 
		{
			Tags { "Queue"="Transparent+1" "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha   
			AlphaTest Greater .01  
			Cull Off 
			Lighting Off 
			ZWrite Off
   
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			uniform float	  _MoveSpeed; 
			uniform float	  _MoveForce;  

			uniform sampler2D _NoiseTex; 
			uniform sampler2D _MainTex;  
			uniform float4	  _NoiseTex_ST; 
			uniform float4	  _MainTex_ST;  

			struct inputdata 
			{
				float4 vertex  : POSITION; 
				float2 texcoord: TEXCOORD0; 
			};

			struct outputdata 
			{
				float4 vertex  : POSITION; 
				float2 uvmain  : TEXCOORD0; 
				float2 uvNoise : TEXCOORD1; 
			};

			outputdata vert (inputdata v)
			{
				outputdata o;

				o.vertex  = UnityObjectToClipPos(v.vertex);
				o.uvmain  = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uvNoise = TRANSFORM_TEX(v.texcoord, _NoiseTex);

				return o;
			}

			half4 frag( outputdata i ) : COLOR
			{
				 half4 offsetColor1 = tex2D(_NoiseTex, i.uvNoise  +  _Time.xz * _MoveSpeed);
				 half4 offsetColor2 = tex2D(_NoiseTex, i.uvNoise  -  _Time.yx * _MoveSpeed);
				
				 i.uvmain.x += ((offsetColor1.r + offsetColor2.r) - 1) * _MoveForce; 
				 i.uvmain.y += ((offsetColor1.g + offsetColor2.g) - 1) * _MoveForce;
		
				 return  tex2D(_MainTex, i.uvmain);
			}
			ENDCG
		}
	}
}