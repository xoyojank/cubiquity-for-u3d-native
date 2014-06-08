Shader "FakeColoredCubes"
{
	Properties
	{
		_CubeColor ("Cube Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_NormalMap ("Normal map", 2D) = "bump" {}
		_NormalMapScaleFactor ("Normal map scale factor", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert addshadow
		#pragma target 3.0
		#pragma glsl
		
		// Scripts can flip the computed normal by setting this to '-1.0f'.
		float normalMultiplier;

		float4 _CubeColor;
		sampler2D _NormalMap;
		float _NormalMapScaleFactor;

		struct Input
		{
			float4 color : COLOR;
			float4 volumePos;
		};
		
		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			
			// Unity can't cope with the idea that we're peforming lighting without having per-vertex
			// normals. We specify dummy ones here to avoid having to use up vertex buffer space for them.
			//v.normal = float3 (0.0f, 0.0f, 1.0f);
			//v.tangent = float4 (1.0f, 0.0f, 0.0f, 1.0f);     
			
			// Volume-space position is use for adding noise.
			o.volumePos = v.vertex;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			// Compute the surface normal in the fragment shader. I believe the orientation of the vector is different
	      	// between render systems as some are left-handed and some are right handed (even though Unity corrects for
	      	// other handiness differences internally). With these render system tests the normals are correct on Windows
	      	// regardless of which render system is in use.
#if SHADER_API_D3D9 || SHADER_API_D3D11 || SHADER_API_D3D11_9X || SHADER_API_XBOX360
	      	float3 volumeNormal = normalize(cross(ddx(IN.volumePos.xyz), ddy(IN.volumePos.xyz)));
#else
			float3 volumeNormal = -normalize(cross(ddx(IN.volumePos.xyz), ddy(IN.volumePos.xyz)));
#endif

			normalMultiplier = 1.0; //-normalMultiplier; //HACK!!
	
			// Despite our render system checks above, we have seen that the normals are still backwards in Linux
	      	// standalone builds. The reason is not currently clear, but the 'normalMultiplier' allow scripts to
	      	// flip the normal if required by setting the multiplier to '-1.0f'.
			volumeNormal *= normalMultiplier;
			
			// This fixes inaccuracies/rounding errors which can otherwise occur
			volumeNormal = floor(volumeNormal + float3(0.5, 0.5, 0.5));	
			
			// Because we know our normal is pointing along one of the three main axes we can trivially compute a tangent space.
			float3 volumeTangent = volumeNormal.yzx;
	    	float3 volumeBinormal = volumeNormal.zxy;
	    	
	    	// And from our tangent space we can now compute texture coordinates.
	    	float2 texCoords = float2(dot(IN.volumePos.xyz, volumeTangent), dot(IN.volumePos.xyz, volumeBinormal));
	    	texCoords = texCoords - float2(0.5, 0.5);  // Required because integer positions are at the center of the voxel.
	    	texCoords = texCoords / _NormalMapScaleFactor;
	    	
	    	// Get the normal from the normal map (we no longer need the normal we calculated earlier).
	    	float3 normalFromNormalMap = UnpackNormal(tex2D(_NormalMap, texCoords));
	    	
	    	// Move the normal into the correct space. I do wonder whether some of this is unnecessary, and actually reduces to something trivial...
	    	float3x3 volumeToTangentMatrix = float3x3(
	    		volumeTangent.x, volumeBinormal.x, volumeNormal.x, 
	    		volumeTangent.y, volumeBinormal.y, volumeNormal.y, 
	    		volumeTangent.z, volumeBinormal.z, volumeNormal.z);
			normalFromNormalMap = mul(volumeToTangentMatrix, normalFromNormalMap); //HACK - this shouldn't be commented?!!
			
			o.Albedo = _CubeColor.rgb;
			o.Alpha = 1.0;
        	o.Normal = normalFromNormalMap;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
