
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
	float4 Pos : SV_POSITION;
	float4 WorldPos : TEXCOORD;
	float3 Color : COLOR;
};

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(VS_OUTPUT input) : SV_Target
{
	float3 worldNormal = normalize(cross(ddy(input.WorldPos.xyz), ddx(input.WorldPos.xyz)));
	worldNormal *= -1.0; // Not sure why we have to invert this... to be checked.

	// Basic lighting calculation for overhead light.
	float ambient = 0.3;
	float diffuse = 0.7;
	float3 lightDir = normalize(float3(0.2, 0.8, 0.4));
	float nDotL = saturate(dot(normalize(worldNormal), lightDir));
	float lightIntensity = ambient + diffuse * nDotL;

	return float4(input.Color * lightIntensity, 1.0);
}
