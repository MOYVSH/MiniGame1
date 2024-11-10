// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/GerstnerWave" 
{
	Properties 
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_BumpTex ("Normal", 2D) = "white" {}
		_EdgeTex("Edge", 2D) = "white" {}
		_FoamTex("Foam", 2D) = "white" {}
		_RefractionTex("Refraction", 2D) = "white"{}

		_EdgeFadeout("Edge Fadeout",Range(0,1)) = 0.85

		_EdgeMoveSpeed("Edge Move Speed",float) = 1
		_EdgeBright("Edge Bright",Range(0,15)) = 1

		_FoamMoveSpeedX("Foam Move SpeedX",float) = 1
		_FoamMoveSpeedY("Foam Move SpeedY",float) = 1
		_FoamBright("Foam Bright",Range(0,15)) = 1

		_WaterConcentration("Water Concentration",Range(0.001,1.0)) = 0.6
		_RefractionFactor("Refraction Factor",Range(0,1.0)) = 0.1

		_BumpTexUVSpeedX("Bump Speed->X", float) = 1
		_BumpTexUVSpeedY("Bump Speed->Y", float) = 1

		//Gerstner
		_GAmplitude   ("Wave Amplitude", Vector) = (2 ,0, 0, 0)
		_GFrequency   ("Wave Frequency", Vector) = (1, 0, 0, 0)
		_GSteepness	  ("Wave Steepness", Vector) = (30, 0, 0, 0)
		_GSpeed		  ("Wave Speed", Vector)	 = (1, 0, 0, 0)
		_GDirectionAB ("Wave Direction", Vector) = (-0.1 ,-0.05, 0, 0)
		_GDirectionCD ("Wave Direction", Vector) = (0 ,0, 0, 0)	
	}

	SubShader 
	{  
        Pass 
		{  
			CGPROGRAM  
  
            #pragma vertex vert  
            #pragma fragment frag  
			#include "UnityCG.cginc"

            uniform sampler2D _MainTex; 
			uniform float4    _MainTex_ST;
			uniform sampler2D _BumpTex; 
			uniform float4    _BumpTex_ST;
			uniform sampler2D _EdgeTex;
			uniform sampler2D _FoamTex;
			uniform float4    _FoamTex_ST;
			uniform sampler2D _RefractionTex;

			uniform sampler2D _CameraDepthTexture;

			uniform float _BumpTexUVSpeedX;
			uniform float _BumpTexUVSpeedY;
			uniform float _EdgeMoveSpeed;
			uniform float _FoamMoveSpeedX;
			uniform float _FoamMoveSpeedY;

			uniform fixed _EdgeFadeout;
			uniform half  _EdgeBright;
			uniform half  _FoamBright;
			uniform fixed _WaterConcentration;
			uniform fixed _RefractionFactor;

			uniform float4 _GAmplitude;
			uniform float4 _GFrequency;
			uniform float4 _GSteepness; 									
			uniform float4 _GSpeed;					
			uniform float4 _GDirectionAB;		
			uniform float4 _GDirectionCD; 
              
            struct indata 
			{  
                float4 pos : POSITION;  
                float2 uv  : TEXCOORD0;  
            };  
       
            struct outdata 
			{  
                float4 pos		 : SV_POSITION;
				float4 screenPos : TEXCOORD0;  
                half2  main_uv	 : TEXCOORD1;  
				half2  foam_uv	 : TEXCOORD2; 
				float4 offset	 : TEXCOORD3;
				half3  normal	 : TEXCOORD4;
            }; 
  
  
			half3 GerstnerOffset4 (half2 xzVtx, half4 steepness, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD) 
			{
				half3 offsets;
		
				half4 AB = steepness.xxyy * amp.xxyy * dirAB.xyzw;
				half4 CD = steepness.zzww * amp.zzww * dirCD.xyzw;
		
				half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
				half4 TIME = _Time.yyyy * speed;
		
				half4 COS = cos (dotABCD + TIME);
				half4 SIN = sin (dotABCD + TIME);
		
				offsets.x = dot(COS, half4(AB.xz, CD.xz));
				offsets.z = dot(COS, half4(AB.yw, CD.yw));
				offsets.y = dot(SIN, amp);

				return offsets;			
			}	

			half3 GerstnerNormal4 (half2 xzVtx, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD) 
			{
				half3 nrml = half3(0,2.0,0);
		
				half4 AB = freq.xxyy * amp.xxyy * dirAB.xyzw;
				half4 CD = freq.zzww * amp.zzww * dirCD.xyzw;
		
				half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
				half4 TIME = _Time.yyyy * speed;
		
				half4 COS = cos (dotABCD + TIME);
		
				nrml.x -= dot(COS, half4(AB.xz, CD.xz));
				nrml.z -= dot(COS, half4(AB.yw, CD.yw));
		
				nrml = normalize (nrml);

				return nrml;			
			}	
	
			void Gerstner (	out half3 offs, out half3 nrml,
							 half3 vtx, half3 tileableVtx, 
							 half4 amplitude, half4 frequency, half4 steepness, 
							 half4 speed, half4 directionAB, half4 directionCD ) 
			{
				offs = GerstnerOffset4(tileableVtx.xz, steepness, amplitude, frequency, speed, directionAB, directionCD);
				nrml = GerstnerNormal4(tileableVtx.xz + offs.xz, amplitude, frequency, speed, directionAB, directionCD);							
			}

            outdata vert(indata i)
			{  
				half3 nrml;
				half3 offsets;

				Gerstner
				(
					offsets, nrml, i.pos.xyz, mul(unity_ObjectToWorld, (i.pos)).xzz,	// offsets, nrml will be written
					_GAmplitude,					 		// amplitude
					_GFrequency,				 			// frequency
					_GSteepness, 							// steepness
					_GSpeed,								// speed
					_GDirectionAB,							// direction # 1, 2
					_GDirectionCD							// direction # 3, 4
				);

				outdata o;
				o.normal = nrml;


				i.pos.xyz += offsets;
				o.pos = UnityObjectToClipPos (i.pos);

				o.screenPos  = ComputeScreenPos(o.pos);
				o.main_uv    = TRANSFORM_TEX(i.uv,_MainTex); 
				o.foam_uv	 = TRANSFORM_TEX(i.uv,_FoamTex);

				o.offset.xy = TRANSFORM_TEX(i.uv,_BumpTex);
				o.offset.zw = o.offset.xy;

				o.offset.xy += float2(_Time.x*_BumpTexUVSpeedX, 0);
				o.offset.zw += float2(0, _Time.x*_BumpTexUVSpeedY);

                return o;  
            }  
  
            half4 frag (outdata i) : COLOR 
			{ 
				float4 color;
				half3  offset1;
				half3  offset2;

				offset1 = UnpackNormal(tex2D(_BumpTex, i.offset.xy));
				offset2 = UnpackNormal(tex2D(_BumpTex, i.offset.zw));

				half3 dir = offset1 + offset2;
				color = tex2D(_MainTex, i.main_uv + dir.xy);

				float2 screen_uv   = i.screenPos.xy / i.screenPos.w;
				float  depth	   = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, screen_uv)));
				float  depthDiff   = abs(depth - i.screenPos.w);
				float  depthDiff01 = clamp(depthDiff * _EdgeFadeout, 0, 1);
				float  invertdepthDiff01_2 = (1 - depthDiff01); 
				invertdepthDiff01_2*=invertdepthDiff01_2;
				
				color += tex2D(_EdgeTex, half2(depthDiff01 + _Time.x*_EdgeMoveSpeed , 0)).r*_EdgeBright*invertdepthDiff01_2;
				color += tex2D(_FoamTex,i.foam_uv + _Time.x*half2(_FoamMoveSpeedX,_FoamMoveSpeedY) )*_FoamBright*invertdepthDiff01_2;

				fixed4 refractionColor;
				screen_uv += dir.xy * depthDiff01 * depthDiff01 * _RefractionFactor;
				refractionColor = tex2D(_RefractionTex, screen_uv );

				depthDiff = abs(LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, screen_uv)))- i.screenPos.w);

				color = lerp(refractionColor, color, clamp(depthDiff*_WaterConcentration,0,1));

				return color;  
            }  
            ENDCG  
        } 
	}
}
