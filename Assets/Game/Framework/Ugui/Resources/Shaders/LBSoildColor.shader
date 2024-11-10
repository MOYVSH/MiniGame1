// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LBShader/SoildColor" 
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
			
			struct v_data
			{
				float4 pos : POSITION;
				fixed4 col : COLOR0;
			};

            v_data vert(v_data i)
			{  
				v_data o;
				o.pos = UnityObjectToClipPos ( i.pos);
				o.col = i.col; 
                return o;   
            }  
  
            fixed4 frag (v_data i) : COLOR 
			{ 
                return _Color * i.col;  
            }  
            ENDCG  
        } 
	}
}
