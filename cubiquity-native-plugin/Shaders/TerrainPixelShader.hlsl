
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR;
	float3 Normal : TEXCOORD0;
	float4 Weights : TEXCOORD1;
};

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(VS_OUTPUT input) : SV_Target
{

	// Basic lighting calculation for overhead light.
	float ambient = 0.3;
	float diffuse = 0.7;
	float3 lightDir = float3(0.0, 1.0, 0.0);
	float nDotL = saturate(dot(normalize(input.Normal), lightDir));
	float lightIntensity = ambient + diffuse * nDotL;

	return float4(input.Color.rgb * lightIntensity, 1.0);
}
