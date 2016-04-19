#include "D3D11OctreeNode.h"
#include "Utils.h"
#include "RenderThreadCommand.h"

extern ID3D11Device* g_D3D11Device;


D3D11OctreeNode::D3D11OctreeNode(D3D11OctreeNode* _parent)
    : noOfIndices(0)
    , indexBuffer(nullptr)
    , vertexBuffer(nullptr)
    , posX(0)
    , posY(0)
    , posZ(0)
    , structureLastSynced(0)
    , propertiesLastSynced(0)
    , meshLastSynced(0)
    , nodeAndChildrenLastSynced(0)
    , renderThisNode(false)
    , parent(_parent)
    , height(0)
{
    memset(this->children, 0, sizeof(this->children));
}

D3D11OctreeNode::~D3D11OctreeNode()
{
    for (uint32_t z = 0; z < 2; z++)
    {
        for (uint32_t y = 0; y < 2; y++)
        {
            for (uint32_t x = 0; x < 2; x++)
            {
                SAFE_RELEASE(this->children[x][y][z]);
            }
        }
    }

    SAFE_RELEASE(this->vertexBuffer);
    SAFE_RELEASE(this->indexBuffer);
}

void D3D11OctreeNode::ProcessOctreeNode(uint32_t octreeNodeHandle, D3D11OctreeNode* d3d11OctreeNode)
{
    CuOctreeNode octreeNode;
    validate(cuGetOctreeNode(octreeNodeHandle, &octreeNode));

    if (octreeNode.nodeOrChildrenLastChanged > d3d11OctreeNode->nodeAndChildrenLastSynced)
    {
        if (octreeNode.propertiesLastChanged > d3d11OctreeNode->propertiesLastSynced)
        {
            //LogMessage("Resynced properties at %d\n", d3d11OctreeNode->propertiesLastSynced);
            d3d11OctreeNode->height = octreeNode.height;
            d3d11OctreeNode->renderThisNode = octreeNode.renderThisNode;
            cuGetCurrentTime(&(d3d11OctreeNode->propertiesLastSynced));
        }

        //std::cout << "updating" << std::endl;
        if (octreeNode.meshLastChanged > d3d11OctreeNode->meshLastSynced)
        {
            if (octreeNode.hasMesh == 1)
            {
                //std::cout << "Adding mesh for node height " << octreeNode.height;
                // These will point to the index and vertex data
                uint32_t noOfIndices;
                uint16_t* indices;
                uint16_t noOfVertices;
                void* vertices;

                // Get the index and vertex data
                validate(cuGetMesh(octreeNodeHandle, &noOfVertices, &vertices, &noOfIndices, &indices));
                uint32_t volumeType;
                validate(cuGetVolumeType(octreeNodeHandle, &volumeType));

                // Pass it to the D3D11 node.
                d3d11OctreeNode->posX = octreeNode.posX;
                d3d11OctreeNode->posY = octreeNode.posY;
                d3d11OctreeNode->posZ = octreeNode.posZ;
                d3d11OctreeNode->noOfIndices = noOfIndices;

                class CreateVBIBRenderCommand : public RenderCommand
                {
                public:
                    D3D11OctreeNode* d3d11OctreeNode;
                    uint32_t volumeType;
                    uint32_t noOfIndices;
                    std::vector<uint16_t> indices;
                    std::vector<uint8_t> vertexData;

                    virtual ~CreateVBIBRenderCommand()
                    {
                        d3d11OctreeNode->Release();
                    }

                    virtual void DoCommand() override
                    {
                        SAFE_RELEASE(d3d11OctreeNode->indexBuffer);
                        SAFE_RELEASE(d3d11OctreeNode->vertexBuffer);
                        HRESULT hr;
                        // create index buffer
                        D3D11_BUFFER_DESC bufferDesc = { 0 };
                        D3D11_SUBRESOURCE_DATA subResData = { 0 };
                        bufferDesc.Usage = D3D11_USAGE_DEFAULT;
                        bufferDesc.BindFlags = D3D11_BIND_INDEX_BUFFER;
                        bufferDesc.ByteWidth = sizeof(uint16_t)* noOfIndices;
                        subResData.pSysMem = indices.data();
                        hr = g_D3D11Device->CreateBuffer(&bufferDesc, &subResData, &d3d11OctreeNode->indexBuffer);
                        assert(SUCCEEDED(hr));
                        // create vertex buffer
                        bufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
                        bufferDesc.ByteWidth = vertexData.size();
                        subResData.pSysMem = vertexData.data();
                        hr = g_D3D11Device->CreateBuffer(&bufferDesc, &subResData, &d3d11OctreeNode->vertexBuffer);
                        assert(SUCCEEDED(hr));
                    }
                };
                d3d11OctreeNode->AddRef(); // release in command's destructor
                CreateVBIBRenderCommand* command = new CreateVBIBRenderCommand();
                command->d3d11OctreeNode = d3d11OctreeNode;
                command->volumeType = volumeType;
                command->noOfIndices = noOfIndices;
                command->indices.resize(noOfIndices);
                memcpy(command->indices.data(), indices, sizeof(uint16_t) * noOfIndices);
                size_t vertexDataSize = 0;
                if (volumeType == CU_COLORED_CUBES)
                    vertexDataSize = sizeof(CuColoredCubesVertex)* noOfVertices;
                else if (volumeType == CU_TERRAIN)
                    vertexDataSize = sizeof(CuTerrainVertex)* noOfVertices;
                else
                    __debugbreak();
                command->vertexData.resize(vertexDataSize);
                memcpy(command->vertexData.data(), vertices, vertexDataSize);

                RenderCommandProcessor::Instance()->SendCommand(command);
            }
            else
            {
                assert(d3d11OctreeNode->noOfIndices == 0);
            }

            cuGetCurrentTime(&(d3d11OctreeNode->meshLastSynced));
            //LogMessage("Resynced mesh at %d\n", d3d11OctreeNode->meshLastSynced);
        }

        if (octreeNode.structureLastChanged > d3d11OctreeNode->structureLastSynced)
        {
            for (uint32_t z = 0; z < 2; z++)
            {
                for (uint32_t y = 0; y < 2; y++)
                {
                    for (uint32_t x = 0; x < 2; x++)
                    {
                        if (octreeNode.childHandles[x][y][z] != 0xFFFFFFFF)
                        {
                            if (!d3d11OctreeNode->children[x][y][z])
                            {
                                d3d11OctreeNode->children[x][y][z] = new D3D11OctreeNode(d3d11OctreeNode);
                            }
                        }
                        else
                        {
                            SAFE_RELEASE(d3d11OctreeNode->children[x][y][z]);
                        }
                    }
                }
            }

            cuGetCurrentTime(&(d3d11OctreeNode->structureLastSynced));
            //LogMessage("Resynced structure at %d\n", d3d11OctreeNode->structureLastSynced);
        }

        for (uint32_t z = 0; z < 2; z++)
        {
            for (uint32_t y = 0; y < 2; y++)
            {
                for (uint32_t x = 0; x < 2; x++)
                {
                    if (octreeNode.childHandles[x][y][z] != 0xFFFFFFFF)
                    {
                        // Recursivly call the octree traversal
                        ProcessOctreeNode(octreeNode.childHandles[x][y][z], d3d11OctreeNode->children[x][y][z]);
                    }
                }
            }
        }

        cuGetCurrentTime(&(d3d11OctreeNode->nodeAndChildrenLastSynced));
    }
}
