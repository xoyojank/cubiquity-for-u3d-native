Shader "SimpleFakeColoredCubes"
{
	Properties
	{
		_NormalMap ("Normal map", 2D) = "bump" {}
		_NoiseStrength ("Noise strength", Range (0.0,0.5)) = 0.1
		
		_CubeColor ("Cube Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		CGPROGRAM
		#pragma surface surf Lambert		

		float4 _CubeColor;
		sampler2D _NormalMap;

		struct Input
		{
	        float2 uv_NormalMap;
      	};

		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
			
			o.Albedo = _CubeColor.rgb;
			o.Alpha = 1.0;
        	//o.Normal = normalFromNormalMap;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
