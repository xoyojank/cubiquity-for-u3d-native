#pragma once
#include "D3D11OctreeNode.h"
#include <string>
#include <vector>
#include <DirectXMath.h>

using namespace DirectX;

class D3D11VolumeRenderer
{
public:
	D3D11VolumeRenderer();

	bool Setup(const std::string& assetPath);

	void Destroy();

	void UpdateMatrix(const XMFLOAT4X4& viewMatrix, const XMFLOAT4X4& projectionMatrix);
	void RenderVolume(ID3D11DeviceContext* context, uint32_t volumeType, D3D11OctreeNode* rootNode);

private:
	void RenderOctreeNode(ID3D11DeviceContext* context, D3D11OctreeNode* d3d11OctreeNode);

	static bool LoadFileIntoBuffer(const std::string& fileName, std::vector<char>& buffer);

	static void CompileShaderFromString(std::vector<char> &buffer, const D3D11_INPUT_ELEMENT_DESC* desc,
		ID3D11VertexShader** outVS, ID3D11PixelShader** outPS, ID3D11InputLayout** outIL);

private:
	struct ConstantBufferVS
	{
		DirectX::XMMATRIX World;
		DirectX::XMMATRIX View;
		DirectX::XMMATRIX Projection;
	};
	ConstantBufferVS* cbVSData;
	ID3D11Buffer* constantBuffer;
	ID3D11VertexShader* vertexShader[2];
	ID3D11PixelShader* pixelShader[2];
	ID3D11InputLayout* inputLayout[2];

private:
	UINT currentVertexStride;
};