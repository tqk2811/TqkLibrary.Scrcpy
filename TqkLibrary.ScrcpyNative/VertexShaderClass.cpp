#include "pch.h"
#include "VertexShaderClass.h"
#include "VertexShader.h"

#define NUMVERTICES 6
typedef struct _VERTEX
{
	DirectX::XMFLOAT3 Pos;
	DirectX::XMFLOAT2 TexCoord;
} VERTEX;


VertexShaderClass::VertexShaderClass() {

}

VertexShaderClass::~VertexShaderClass() {
	this->Shutdown();
}

bool VertexShaderClass::Initialize(ID3D11Device* d3d11_device) {
	if (this->m_d3d11_vertexShader != nullptr) return true;

	UINT Size = ARRAYSIZE(g_VS);
	HRESULT hr = d3d11_device->CreateVertexShader(g_VS, Size, nullptr, this->m_d3d11_vertexShader.GetAddressOf());
	if (FAILED(hr))
		return false;


	constexpr std::array<D3D11_INPUT_ELEMENT_DESC, 2> Layout =
	{ {
		{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		{ "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,    0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	} };

	hr = d3d11_device->CreateInputLayout(Layout.data(), (UINT)Layout.size(), g_VS, Size, this->m_d3d11_inputLayout.GetAddressOf());
	if (FAILED(hr))
		return false;



	static VERTEX Vertices[NUMVERTICES] =
	{
		{XMFLOAT3(-1.0f, -1.0f, 0), XMFLOAT2(0.0f, 1.0f)},
		{XMFLOAT3(-1.0f, 1.0f, 0), XMFLOAT2(0.0f, 0.0f)},
		{XMFLOAT3(1.0f, -1.0f, 0), XMFLOAT2(1.0f, 1.0f)},
		{XMFLOAT3(1.0f, -1.0f, 0), XMFLOAT2(1.0f, 1.0f)},
		{XMFLOAT3(-1.0f, 1.0f, 0), XMFLOAT2(0.0f, 0.0f)},
		{XMFLOAT3(1.0f, 1.0f, 0), XMFLOAT2(1.0f, 0.0f)},
	};

	//VertexBuffer
	D3D11_BUFFER_DESC BufferDesc;
	RtlZeroMemory(&BufferDesc, sizeof(BufferDesc));
	BufferDesc.Usage = D3D11_USAGE_DEFAULT;
	BufferDesc.ByteWidth = sizeof(VERTEX) * NUMVERTICES;
	BufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
	BufferDesc.CPUAccessFlags = 0;
	D3D11_SUBRESOURCE_DATA InitData;
	RtlZeroMemory(&InitData, sizeof(InitData));
	InitData.pSysMem = Vertices;
	hr = d3d11_device->CreateBuffer(&BufferDesc, &InitData, this->m_d3d11_vertexBuffer.GetAddressOf());
	if (FAILED(hr))
		return false;


	return true;
}
void VertexShaderClass::Set(ID3D11DeviceContext* d3d11_deviceCtx) {
	d3d11_deviceCtx->VSSetShader(this->m_d3d11_vertexShader.Get(), nullptr, 0);
	d3d11_deviceCtx->IASetInputLayout(this->m_d3d11_inputLayout.Get());
	UINT Stride = sizeof(VERTEX);
	UINT Offset = 0;
	d3d11_deviceCtx->IASetVertexBuffers(0, 1, this->m_d3d11_vertexBuffer.GetAddressOf(), &Stride, &Offset);
}
UINT VertexShaderClass::GetVertexCount() {
	return NUMVERTICES;
}
void VertexShaderClass::Shutdown() {
	this->m_d3d11_vertexShader.Reset();
	this->m_d3d11_inputLayout.Reset();
	this->m_d3d11_vertexBuffer.Reset();
}