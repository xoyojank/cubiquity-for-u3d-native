#include "D3D11VolumeRenderer.h"
#include "Utils.h"

extern ID3D11Device* g_D3D11Device;

typedef HRESULT(WINAPI *D3DCompileFunc)(
	const void* pSrcData,
	unsigned long SrcDataSize,
	const char* pFileName,
	const D3D10_SHADER_MACRO* pDefines,
	ID3D10Include* pInclude,
	const char* pEntrypoint,
	const char* pTarget,
	unsigned int Flags1,
	unsigned int Flags2,
	ID3D10Blob** ppCode,
	ID3D10Blob** ppErrorMsgs);

const char* VertexShaderFilePath[] = 
{
	"Cubiquity/Shaders/ColoredCubesVertexShader.cso",
	"Cubiquity/Shaders/TerrainVertexShader.cso"
};
const char* PixelShaderFilePath[] =
{
	"Cubiquity/Shaders/ColoredCubesPixelShader.cso",
	"Cubiquity/Shaders/TerrainPixelShader.cso"
};


D3D11_INPUT_ELEMENT_DESC ColorCubesInputElementDesc[] = {
	{ "POSITION", 0, DXGI_FORMAT_R8G8B8A8_UINT, 0, offsetof(CuColoredCubesVertex, encodedPosX), D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ "COLOR", 0, DXGI_FORMAT_R32_UINT, 0, offsetof(CuColoredCubesVertex, data), D3D11_INPUT_PER_VERTEX_DATA, 0 },
};
D3D11_INPUT_ELEMENT_DESC TerrainInputElementDesc[] = {
	{ "POSITION", 0, DXGI_FORMAT_R16G16B16A16_UINT, 0, offsetof(CuTerrainVertex, encodedPosX), D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ "COLOR", 0, DXGI_FORMAT_R8G8B8A8_UNORM, 0, offsetof(CuTerrainVertex, material0), D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ "COLOR", 1, DXGI_FORMAT_R8G8B8A8_UNORM, 0, offsetof(CuTerrainVertex, material4), D3D11_INPUT_PER_VERTEX_DATA, 0 },
};
const D3D11_INPUT_ELEMENT_DESC* InputElementDesc[] = { ColorCubesInputElementDesc, TerrainInputElementDesc };
const UINT NumOfInputElement[] = { 2, 3 };

//--------------------------------------------------------------------------------------
D3D11VolumeRenderer::D3D11VolumeRenderer()
: constantBuffer(nullptr)
, cbVSData(nullptr)
, currentVertexStride(0)
{
	for (int i = 0; i < 2; ++i)
	{
		this->vertexShader[i] = nullptr;
		this->pixelShader[i] = nullptr;
		this->inputLayout[i] = nullptr;
	}
}

bool D3D11VolumeRenderer::Setup(const std::string& assetPath)
{
	if (this->cbVSData != nullptr)
	{
		// has loaded
		return true;
	}
	this->cbVSData = (ConstantBufferVS*)_aligned_malloc(sizeof(ConstantBufferVS), 16);
	HRESULT hr;
	// constant buffer
	D3D11_BUFFER_DESC desc = { 0 };
	desc.Usage = D3D11_USAGE_DYNAMIC;
	desc.ByteWidth = sizeof(ConstantBufferVS);
	desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	hr = g_D3D11Device->CreateBuffer(&desc, NULL, &constantBuffer);
	V_RETURN(hr);
	// shaders & input layout
	std::vector<char> vsBuffer, psBuffer;
	for (int i = 0; i < 2; ++i)
	{
		LoadFileIntoBuffer(assetPath + VertexShaderFilePath[i], vsBuffer);
		LoadFileIntoBuffer(assetPath + PixelShaderFilePath[i], psBuffer);
		hr = g_D3D11Device->CreateVertexShader(vsBuffer.data(), vsBuffer.size(), nullptr, &this->vertexShader[i]);
		V_RETURN(hr);
		hr = g_D3D11Device->CreatePixelShader(psBuffer.data(), psBuffer.size(), nullptr, &this->pixelShader[i]);
		V_RETURN(hr);
		hr = g_D3D11Device->CreateInputLayout(InputElementDesc[i], NumOfInputElement[i], vsBuffer.data(), vsBuffer.size(), &this->inputLayout[i]);
		V_RETURN(hr);
	}

	return true;
}

void D3D11VolumeRenderer::Destroy()
{
	SAFE_RELEASE(this->constantBuffer);
	for (int i = 0; i < 2; ++i)
	{
		SAFE_RELEASE(this->vertexShader[i]);
		SAFE_RELEASE(this->pixelShader[i]);
		SAFE_RELEASE(this->inputLayout[i]);
	}
	_aligned_free(this->cbVSData);
}

void D3D11VolumeRenderer::UpdateMatrix(const XMFLOAT4X4& viewMatrix, const XMFLOAT4X4& projectionMatrix)
{
	if (this->cbVSData)
	{
		this->cbVSData->View = XMMatrixTranspose(XMLoadFloat4x4(&viewMatrix));
		this->cbVSData->Projection = XMMatrixTranspose(XMLoadFloat4x4(&projectionMatrix));
	}
}

void D3D11VolumeRenderer::RenderVolume(ID3D11DeviceContext* context, uint32_t volumeType, D3D11OctreeNode* rootNode)
{
	assert(volumeType == CU_COLORED_CUBES || volumeType == CU_TERRAIN);

	// shaders
	context->VSSetShader(this->vertexShader[volumeType], nullptr, 0);
	context->PSSetShader(this->pixelShader[volumeType], nullptr, 0);

	// vertex layout
	context->IASetInputLayout(this->inputLayout[volumeType]);
	context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
	switch (volumeType)
	{
	case CU_COLORED_CUBES:
		this->currentVertexStride = sizeof(CuColoredCubesVertex);
		break;
	case CU_TERRAIN:
		this->currentVertexStride = sizeof(CuTerrainVertex);
		break;
	}

	// draw
	this->RenderOctreeNode(context, rootNode);
}


void D3D11VolumeRenderer::RenderOctreeNode(ID3D11DeviceContext* context, D3D11OctreeNode* d3d11OctreeNode)
{
	if (d3d11OctreeNode->noOfIndices > 0 && d3d11OctreeNode->renderThisNode)
	{
		// update constant buffer
		XMMATRIX worldMatrix = XMMatrixTranslation(d3d11OctreeNode->posX, d3d11OctreeNode->posY, d3d11OctreeNode->posZ);
		this->cbVSData->World = XMMatrixTranspose(worldMatrix);
		D3D11_MAPPED_SUBRESOURCE mappedResource;
		HRESULT hr = context->Map(this->constantBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
		if (SUCCEEDED(hr))
		{
			memcpy(mappedResource.pData, this->cbVSData, sizeof(ConstantBufferVS));
			context->Unmap(this->constantBuffer, 0);
		}
		context->VSSetConstantBuffers(0, 1, &this->constantBuffer);

		// set vertex & index buffer
		UINT offset = 0;
		context->IASetVertexBuffers(0, 1, &d3d11OctreeNode->vertexBuffer, &this->currentVertexStride, &offset);
		context->IASetIndexBuffer(d3d11OctreeNode->indexBuffer, DXGI_FORMAT_R16_UINT, 0);

		context->DrawIndexed(d3d11OctreeNode->noOfIndices, 0, 0);
	}

	for (uint32_t z = 0; z < 2; z++)
	{
		for (uint32_t y = 0; y < 2; y++)
		{
			for (uint32_t x = 0; x < 2; x++)
			{
				if (d3d11OctreeNode->children[x][y][z])
				{
					RenderOctreeNode(context, d3d11OctreeNode->children[x][y][z]);
				}
			}
		}
	}
}

bool D3D11VolumeRenderer::LoadFileIntoBuffer(const std::string& fileName, std::vector<char>& buffer)
{
	FILE* fp;
	fopen_s(&fp, fileName.c_str(), "rb");
	if (fp)
	{
		fseek(fp, 0, SEEK_END);
		int size = ftell(fp);
		fseek(fp, 0, SEEK_SET);
		buffer.resize(size);
		fread(buffer.data(), size, 1, fp);
		fclose(fp);

		return true;
	}
	else
	{
		LogMessage("Failed to find: %s\n", fileName.c_str());
		return false;
	}
}

D3D11VolumeRenderer* D3D11VolumeRenderer::Instance()
{
	static D3D11VolumeRenderer s_instance;
	return &s_instance;
}
