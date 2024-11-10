// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/Unlit Transparent Gray" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_Gray ("Gray Value", Range(0.0,1.0)) = 0.0
		_Brightness ("Brightness Value", Range(1.0,2.0)) = 1.0
		_Flash ("Flash Value", Range(0.0,1.0)) = 0.0
		_FlashFrequency ("FlashFrequency Value", Range(1.0,20.0)) = 1.0
	}

	Category
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater .01
		ColorMask RGB
		Cull Off 
		Lighting Off 
		ZWrite On
		
		BindChannels 
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
		// ---- Fragment program cards
		SubShader 
		{
			Pass 
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_particles
				
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed4 _Color;
				
				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
				
				float4 _MainTex_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}

				sampler2D _CameraDepthTexture;
				float _Gray;
				float _Brightness;
				float _Flash;
				float _FlashFrequency;
				
				fixed4 frag (v2f i) : COLOR
				{	
					float4 color;
					float v;
					color = tex2D(_MainTex, i.texcoord);
					v = _Gray * (color.r*0.299 + color.g * 0.587 + color.b * 0.114)/3.0;
					color.r = v  + color.r * (1.0f - _Gray);
					color.g = v + color.g * (1.0f - _Gray);
					color.b = v + color.b * (1.0f - _Gray);
					if(_Flash >= 0.5f)
					{
						v = sin(_Brightness + _Time.y * _FlashFrequency)/2.0f;
						v += 1.5f;
					}
					else
					{
						v = _Brightness;
					}
					return v*_Color * color;
				}
				ENDCG 
			}
		} 	
	}
}
