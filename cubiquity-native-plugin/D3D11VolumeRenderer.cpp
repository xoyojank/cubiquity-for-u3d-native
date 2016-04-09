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

const char* ShaderFilePath[] = 
{
	"Cubiquity/Shaders/ColoredCubes.fx",
	"Cubiquity/Shaders/TerrainVoxel.fx"
};

D3D11_INPUT_ELEMENT_DESC ColorCubesInputElementDesc[] = {
	{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ "COLOR", 0, DXGI_FORMAT_R8G8B8A8_UNORM, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};
D3D11_INPUT_ELEMENT_DESC TerrainInputElementDesc[] = {
	{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ "COLOR", 0, DXGI_FORMAT_R8G8B8A8_UNORM, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};
const D3D11_INPUT_ELEMENT_DESC* InputElementDesc[] = { ColorCubesInputElementDesc, TerrainInputElementDesc };

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
	this->cbVSData = (ConstantBufferVS*)_aligned_malloc(sizeof(ConstantBufferVS), 16);
	HRESULT hr;
	// constant buffer
	D3D11_BUFFER_DESC desc;
	memset(&desc, 0, sizeof(desc));
	desc.Usage = D3D11_USAGE_DEFAULT;
	desc.ByteWidth = sizeof(ConstantBufferVS);
	desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	hr = g_D3D11Device->CreateBuffer(&desc, NULL, &constantBuffer);
	V_RETURN(hr);
	// shaders & input layout
	std::vector<char> buffer;
	for (int i = 0; i < 2; ++i)
	{
		LoadFileIntoBuffer(assetPath + ShaderFilePath[i], buffer);
		CompileShaderFromString(buffer, InputElementDesc[i], &this->vertexShader[i], &this->pixelShader[i], &this->inputLayout[i]);
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
		this->cbVSData->View = XMLoadFloat4x4(&viewMatrix);
		this->cbVSData->Projection = XMLoadFloat4x4(&projectionMatrix);
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
		// update contant buffer
		this->cbVSData->World = XMMatrixTranslation(d3d11OctreeNode->posX, d3d11OctreeNode->posY, d3d11OctreeNode->posZ);
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
		fread(&buffer[0], size, 1, fp);
		fclose(fp);

		return true;
	}
	else
	{
		return false;
	}
}

void D3D11VolumeRenderer::CompileShaderFromString(std::vector<char> &buffer, const D3D11_INPUT_ELEMENT_DESC* desc,
	ID3D11VertexShader** outVS, ID3D11PixelShader** outPS, ID3D11InputLayout** outIL)
{
	HMODULE compiler = LoadLibraryA("D3DCompiler_43.dll");
	if (compiler == NULL)
	{
		// Try compiler from Windows 8 SDK
		compiler = LoadLibraryA("D3DCompiler_46.dll");
	}
	if (compiler)
	{
		ID3D10Blob* vsBlob = NULL;
		ID3D10Blob* psBlob = NULL;

		D3DCompileFunc compileFunc = (D3DCompileFunc)GetProcAddress(compiler, "D3DCompile");
		if (compileFunc)
		{
			HRESULT hr;
			hr = compileFunc(buffer.data(), buffer.size(), NULL, NULL, NULL, "VS", "vs_4_0", 0, 0, &vsBlob, NULL);
			if (SUCCEEDED(hr))
			{
				g_D3D11Device->CreateVertexShader(vsBlob->GetBufferPointer(), vsBlob->GetBufferSize(), NULL, outVS);
			}

			hr = compileFunc(buffer.data(), buffer.size(), NULL, NULL, NULL, "PS", "ps_4_0", 0, 0, &psBlob, NULL);
			if (SUCCEEDED(hr))
			{
				g_D3D11Device->CreatePixelShader(psBlob->GetBufferPointer(), psBlob->GetBufferSize(), NULL, outPS);
			}
		}

		// input layout
		if (*outVS && vsBlob)
		{
			g_D3D11Device->CreateInputLayout(desc, 2, vsBlob->GetBufferPointer(), vsBlob->GetBufferSize(), outIL);
		}

		SAFE_RELEASE(vsBlob);
		SAFE_RELEASE(psBlob);

		FreeLibrary(compiler);
	}
	else
	{
		DebugLog("D3D11: HLSL shader compiler not found, will not render anything\n");
	}
}
