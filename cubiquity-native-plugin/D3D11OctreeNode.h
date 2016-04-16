#pragma once
#include "CubiquityC.h"
#include <d3d11.h>
#include "Unity/IUnityInterface.h"

class D3D11OctreeNode
{
    volatile long refCount;
    ~D3D11OctreeNode();
public:
    uint32_t noOfIndices;
    ID3D11Buffer* indexBuffer;
    ID3D11Buffer* vertexBuffer;
    CRITICAL_SECTION bufferLock;

    int32_t posX;
    int32_t posY;
    int32_t posZ;

    uint32_t structureLastSynced;
    uint32_t propertiesLastSynced;
    uint32_t meshLastSynced;
    uint32_t nodeAndChildrenLastSynced;

    uint32_t renderThisNode;

    D3D11OctreeNode* parent;
    D3D11OctreeNode* children[2][2][2];

    uint8_t height;

public:
    D3D11OctreeNode(D3D11OctreeNode* _parent);

    void AddRef();
    void Release();

    static void ProcessOctreeNode(uint32_t octreeNodeHandle, D3D11OctreeNode* d3d11OctreeNode);
};

inline void D3D11OctreeNode::AddRef()
{
    _InterlockedIncrement(&this->refCount);
}

inline void D3D11OctreeNode::Release()
{
    if (0 == _InterlockedDecrement(&this->refCount))
    {
        delete this;
    }
}
