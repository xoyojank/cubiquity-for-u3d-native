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

    void UpdateCamera(const XMFLOAT4X4& viewMatrix, const XMFLOAT4X4& projectionMatrix);
    bool UpdateVolume(uint32_t volumeHandle, D3D11OctreeNode* rootOctreeNode, const XMFLOAT4X4& worldMatrix);
    void RenderVolume(ID3D11DeviceContext* context, uint32_t volumeType, D3D11OctreeNode* rootNode);

    void RenderDefaultVolume(ID3D11DeviceContext* context);

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

public:
    uint32_t defaultVolumeHandle;
    D3D11OctreeNode* defaultRootOctreeNode;
	XMFLOAT4X4 defaultVolumeWorldMatrix;
};


extern "C" void CUBIQUITYC_API UpdateCamera(float viewMatrix[], float projectionMatrix[]);
extern "C" bool CUBIQUITYC_API UpdateVolume(uint32_t volumeHandle, PVOID rootOctreeNode, float worldMatrix[]);

extern "C" PVOID CUBIQUITYC_API CreateOctreeNode();
extern "C" void CUBIQUITYC_API DestroyOctreeNode(PVOID octreeNode);
