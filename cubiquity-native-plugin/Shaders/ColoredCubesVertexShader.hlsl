
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
	uint4 EncodedPos : POSITION;
	uint QuantizedColor : COLOR;
};

//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
	float4 Pos : SV_POSITION;
	float4 WorldPos : TEXCOORD;
	float3 Color : COLOR;
};

static const uint MaxInOutValue = 255u;

static const uint RedMSB = 31u;
static const uint RedLSB = 27u;
static const uint GreenMSB = 26u;
static const uint GreenLSB = 21u;
static const uint BlueMSB = 20u;
static const uint BlueLSB = 16u;
static const uint AlphaMSB = 15u;
static const uint AlphaLSB = 12u;

static const uint NoOfRedBits = RedMSB - RedLSB + 1u;
static const uint NoOfGreenBits = GreenMSB - GreenLSB + 1u;
static const uint NoOfBlueBits = BlueMSB - BlueLSB + 1u;
static const uint NoOfAlphaBits = AlphaMSB - AlphaLSB + 1u;

static const uint RedScaleFactor = MaxInOutValue / ((0x01u << NoOfRedBits) - 1u);
static const uint GreenScaleFactor = MaxInOutValue / ((0x01u << NoOfGreenBits) - 1u);
static const uint BlueScaleFactor = MaxInOutValue / ((0x01u << NoOfBlueBits) - 1u);
static const uint AlphaScaleFactor = MaxInOutValue / ((0x01u << NoOfAlphaBits) - 1u);

//--------------------------------------------------------------------------------------
float3 DecodePosition(uint3 encodedPosition)
{
	return float3(encodedPosition) - 0.5;
}

//--------------------------------------------------------------------------------------
float3 DecodeColor(uint quantizedColor)
{
	quantizedColor >>= 16;
	float blue = (quantizedColor & 0x1Fu) * BlueScaleFactor;
	quantizedColor >>= 5;
	float green = (quantizedColor & 0x3Fu) * GreenScaleFactor;
	quantizedColor >>= 6;
	float red = (quantizedColor & 0x1Fu) * RedScaleFactor;

	float3 result = float3(red, green, blue);
	result *= (1.0 / 255.0);
	return result;
}

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_OUTPUT VS(VS_INPUT input)
{
	float3 pos = DecodePosition(input.EncodedPos.xyz);

	VS_OUTPUT output = (VS_OUTPUT)0;
	output.WorldPos = mul(float4(pos, 1), World);
	output.Pos = mul(output.WorldPos, View);
	output.Pos = mul(output.Pos, Projection);
	output.Color = DecodeColor(input.QuantizedColor);
	return output;
}
