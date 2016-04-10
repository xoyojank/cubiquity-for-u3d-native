
//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer ConstantBuffer : register(b0)
{
	matrix World;
	matrix View;
	matrix Projection;
}

//--------------------------------------------------------------------------------------
struct VS_INPUT
{
	uint4 EncodedPosAndNormal : POSITION;
	float4 Color : COLOR0;
	float4 Weights : COLOR1;
};

//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR;
	float3 Normal : TEXCOORD0;
	float4 Weights : TEXCOORD1;
};

//--------------------------------------------------------------------------------------
float3 DecodePosition(uint3 encodedPosition)
{
	return float3(encodedPosition) / 256.0;
}

//--------------------------------------------------------------------------------------
// Returns +/- 1
float2 SignNotZero(float2 v)
{
	return float2((v.x >= 0.0) ? +1.0 : -1.0, (v.y >= 0.0) ? +1.0 : -1.0);
}

//--------------------------------------------------------------------------------------
float3 DecodeNormal(uint encodedNormal)
{
	//Get the encoded bytes of the normal
	uint encodedX = (encodedNormal >> 8u) & 0xFFu;
	uint encodedY = (encodedNormal)& 0xFFu;

	// Map to range [-1.0, +1.0]
	float2 e = float2(encodedX, encodedY);
	e = e * float2(1.0 / 127.5, 1.0 / 127.5);
	e = e - float2(1.0, 1.0);

	// Now decode normal using listing 2 of http://jcgt.org/published/0003/02/01/
	float3 v = float3(e.xy, 1.0 - abs(e.x) - abs(e.y));
	if (v.z < 0) v.xy = (1.0 - abs(v.yx)) * SignNotZero(v.xy);
	v = normalize(v);
	return v;
}

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_OUTPUT VS(VS_INPUT input)
{
	float3 pos = DecodePosition(input.EncodedPosAndNormal.xyz);
	float3 normal = DecodeNormal(input.EncodedPosAndNormal.w);

	VS_OUTPUT output = (VS_OUTPUT)0;
	output.Pos = mul(pos, World);
	output.Pos = mul(output.Pos, View);
	output.Pos = mul(output.Pos, Projection);
	output.Normal = normal;// Valid if we don't scale or rotate our volume.
	output.Color = input.Color;
	output.Weights = input.Weights;

	return output;
}
