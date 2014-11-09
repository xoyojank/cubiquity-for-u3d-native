Shader "VisualizeLOD"
{
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert addshadow

		float _height;

		struct Input
		{
			float4 color : COLOR;
			float3 worldPos : POSITION;
			float3 volumeNormal;
		};
		
		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			
			o.volumeNormal = v.normal;
		}

		// From http://www.chilliant.com/rgb2hsv.html
		float3 HUEtoRGB(in float H)
		  {
			float R = abs(H * 6 - 3) - 1;
			float G = 2 - abs(H * 6 - 2);
			float B = 2 - abs(H * 6 - 4);
			return saturate(float3(R,G,B));
		  }

		float3 HSVtoRGB(in float3 HSV)
		  {
			float3 RGB = HUEtoRGB(HSV.x);
			return ((RGB - 1) * HSV.y + 1) * HSV.z;
		  }

		void surf (Input IN, inout SurfaceOutput o)
		{		
			IN.volumeNormal = normalize(IN.volumeNormal);

			float golden_ratio_conjugate = 0.618033988749895;
			float hue = frac(_height * golden_ratio_conjugate);

			o.Albedo = HSVtoRGB(float3(hue, 0.5, 0.95));

			/*if(_height < 0.5)
			{
				o.Albedo = float3(1.0, 0.0, 0.0);
			}
			else if(_height < 1.5)
			{
				o.Albedo = float3(0.0, 1.0, 0.0);
			}
			else if(_height < 2.5)
			{
				o.Albedo = float3(0.0, 0.0, 1.0);
			}
			else
			{
				o.Albedo = float3(1.0, 1.0, 1.0);
			}*/
			
			o.Alpha = 1.0;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
