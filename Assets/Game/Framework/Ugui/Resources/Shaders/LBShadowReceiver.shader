Shader "LBShader/ShadowReceiver" 
{
	SubShader
	{
		pass
		{
			Tags{ "LightMode" = "ForwardBase" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma multi_compile_fwdbase
			#pragma vertex   vert  
			#pragma fragment frag  
			
			#include "UnityCG.cginc"  
			#include "Lighting.cginc"  
			#include "autolight.cginc" 

			struct v2f
			{
				float4 pos	 : POSITION;
				float4 color : COLOR;
				LIGHTING_COORDS(0, 1)
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos   = UnityObjectToClipPos(v.vertex);
				o.color = _LightColor0 * saturate(dot(normalize(mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz), normalize(_WorldSpaceLightPos0))) + UNITY_LIGHTMODEL_AMBIENT;
				TRANSFER_VERTEX_TO_FRAGMENT(o);  
				return o;
			}

			fixed4 frag(v2f v) : COLOR
			{
				return fixed4(0, 0, 0, 1.0 - LIGHT_ATTENUATION(v));
			}
			ENDCG
		}
	}
	fallback "Diffuse"
}