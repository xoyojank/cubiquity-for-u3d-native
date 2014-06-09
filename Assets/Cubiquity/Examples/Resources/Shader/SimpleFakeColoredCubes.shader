Shader "SimpleFakeColoredCubes"
{
	Properties
	{
		_NormalMap ("Normal map", 2D) = "bump" {}
		_NoiseStrength ("Noise strength", Range (0.0,0.5)) = 0.1
		
		//_CubePosition ("Cube Position", Vector) = (0.0, 1.0, 0.0, 1.0)
		//_CubeColor ("Cube Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		CGPROGRAM
		
		// We include this file so we can access functionality such as noise from the colored cubes shader.
		// Using the relative path here is slightly error-prone, but it seems a Unity bug prevents us from using 
		// the absolute path: http://forum.unity3d.com/threads/custom-cginc-relative-to-assets-folder-in-dx11.163271/
		#include "../../../Resources/Shaders/ColoredCubesUtilities.cginc"
		
		#pragma surface surf Lambert vertex:vert addshadow
		#pragma target 3.0
		#pragma glsl		

		sampler2D _NormalMap;
		float _NoiseStrength;
		
		float4 _CubeColor;
		float4 _CubePosition;

		struct Input
		{
	        float2 uv_NormalMap;
	        float4 localPosition;
      	};
      	
      	void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			
			o.localPosition = v.vertex;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
			
			// Add noise - we use volume space to prevent noise scrolling if the volume moves.
			float noise = positionBasedNoise(float4(_CubePosition.xyz, _NoiseStrength));
			
			o.Albedo = _CubeColor.rgb + float3(noise, noise, noise);
			//o.Albedo = _CubePosition.xyz;
			o.Alpha = 1.0;
        	//o.Normal = normalFromNormalMap;
		}
		ENDCG
	} 
}
