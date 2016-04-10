#pragma once
#include "D3D11OctreeNode.h"
#include <string>
#include <vector>
#include <DirectXMath.h>
#include "Unity/IUnityInterface.h"

using namespace DirectX;

class D3D11CubiquityRenderer
{
public:
	static D3D11CubiquityRenderer* Instance();

	bool Setup(const std::string& assetPath);

	void Destroy();

	void UpdateVolume(uint32_t volumeHandle, const XMFLOAT4X4& viewMatrix, const XMFLOAT4X4& projectionMatrix);
	void RenderVolume(ID3D11DeviceContext* context, uint32_t volumeType, D3D11OctreeNode* rootNode);

	void RenderTestVolume(ID3D11DeviceContext* context, uint32_t volumeType);

public:
	static bool LoadFileIntoBuffer(const std::string& fileName, std::vector<char>& buffer);

private:
	D3D11CubiquityRenderer();
	void RenderOctreeNode(ID3D11DeviceContext* context, D3D11OctreeNode* d3d11OctreeNode);

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
	std::string assetPath;
};


extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UpdateVolume(uint32_t volumeHandle, float viewMatrix[], float projectionMatrix[]);
