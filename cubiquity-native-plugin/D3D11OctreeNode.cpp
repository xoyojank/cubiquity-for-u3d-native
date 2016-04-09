#include "D3D11OctreeNode.h"
#include "Utils.h"

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
				SAFE_DELETE(this->children[x][y][z]);
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
			LogMessage("Resynced properties at %s\n", d3d11OctreeNode->propertiesLastSynced);
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

				HRESULT hr;
				// create index buffer;
				D3D11_BUFFER_DESC bufferDesc;
				ZeroMemory(&bufferDesc, sizeof(bufferDesc));
				bufferDesc.Usage = D3D11_USAGE_DEFAULT;
				bufferDesc.ByteWidth = sizeof(uint16_t)* noOfIndices;
				bufferDesc.BindFlags = D3D11_BIND_INDEX_BUFFER;
				bufferDesc.CPUAccessFlags = 0;
				D3D11_SUBRESOURCE_DATA subResData;
				ZeroMemory(&subResData, sizeof(subResData));
				subResData.pSysMem = indices;
				hr = g_D3D11Device->CreateBuffer(&bufferDesc, &subResData, &d3d11OctreeNode->indexBuffer);
				assert(SUCCEEDED(hr));

				// create vertex buffer
				bufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
				if (volumeType == CU_COLORED_CUBES)
				{
					bufferDesc.ByteWidth = sizeof(CuColoredCubesVertex)* noOfVertices;
					subResData.pSysMem = vertices;
					hr = g_D3D11Device->CreateBuffer(&bufferDesc, &subResData, &d3d11OctreeNode->vertexBuffer);
					assert(SUCCEEDED(hr));
				}
				else if (volumeType == CU_TERRAIN)
				{
					bufferDesc.ByteWidth = sizeof(CuTerrainVertex)* noOfVertices;
					subResData.pSysMem = vertices;
					hr = g_D3D11Device->CreateBuffer(&bufferDesc, &subResData, &d3d11OctreeNode->vertexBuffer);
					assert(SUCCEEDED(hr));
				}
			}
			else
			{
				assert(d3d11OctreeNode->noOfIndices == 0);
			}

			cuGetCurrentTime(&(d3d11OctreeNode->meshLastSynced));
			LogMessage("Resynced mesh at %s\n", d3d11OctreeNode->meshLastSynced);
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
							if (d3d11OctreeNode->children[x][y][z])
							{
								delete d3d11OctreeNode->children[x][y][z];
								d3d11OctreeNode->children[x][y][z] = nullptr;
							}
						}
					}
				}
			}

			cuGetCurrentTime(&(d3d11OctreeNode->structureLastSynced));
			LogMessage("Resynced structure at %s\n", d3d11OctreeNode->structureLastSynced);
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
