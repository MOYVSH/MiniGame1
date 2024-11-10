Shader "LBShader/Unlit Transparent with depth" {
	Properties {
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Geometry+1"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite on
			Fog { Mode Off }

			Blend SrcAlpha OneMinusSrcAlpha
			
			SetTexture [_MainTex]
			{
				constantColor [_Color]
				combine texture * constant
			}
		}
	}

	FallBack "Diffuse"
}
