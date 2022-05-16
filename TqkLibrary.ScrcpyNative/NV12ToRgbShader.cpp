#include "pch.h"
#include "PixelShader.h"
#include "VertexShader.h"
#include "Utils.h"
#include "NV12ToRgbShader.h"
#include <math.h>

#define NUMVERTICES 6

typedef struct _VERTEX
{
	DirectX::XMFLOAT3 Pos;
	DirectX::XMFLOAT2 TexCoord;
} VERTEX;

// Vertices for drawing whole texture
VERTEX Vertices[NUMVERTICES] =
{
	{XMFLOAT3(-1.0f, -1.0f, 0), XMFLOAT2(0.0f, 1.0f)},
	{XMFLOAT3(-1.0f, 1.0f, 0), XMFLOAT2(0.0f, 0.0f)},
	{XMFLOAT3(1.0f, -1.0f, 0), XMFLOAT2(1.0f, 1.0f)},
	{XMFLOAT3(1.0f, -1.0f, 0), XMFLOAT2(1.0f, 1.0f)},
	{XMFLOAT3(-1.0f, 1.0f, 0), XMFLOAT2(0.0f, 0.0f)},
	{XMFLOAT3(1.0f, 1.0f, 0), XMFLOAT2(1.0f, 0.0f)},
};

FLOAT blendFactor[4] = { 0.f, 0.f, 0.f, 0.f };

NV12ToRgbShader::NV12ToRgbShader(const AVD3D11VADeviceContext* d3d11va_device_ctx) {	
	this->_d3d11_device = d3d11va_device_ctx->device;
	this->_d3d11_deviceCtx = d3d11va_device_ctx->device_context;
}

NV12ToRgbShader::~NV12ToRgbShader() {
	this->ReleaseSharedSurf();
	_d3d11_vertexBuffer.Reset();
	_d3d11_pixelShader.Reset();
	_d3d11_inputLayout.Reset();
	_d3d11_vertexShader.Reset();
	_d3d11_samplerState.Reset();
}

bool NV12ToRgbShader::Init() {
	HRESULT hr;
	
	//SamplerState
	D3D11_SAMPLER_DESC desc = CD3D11_SAMPLER_DESC(CD3D11_DEFAULT());
	hr = this->_d3d11_device->CreateSamplerState(&desc, this->_d3d11_samplerState.GetAddressOf());
	if (FAILED(hr))
		return false;

	//VertexShader
	UINT Size = ARRAYSIZE(g_VS);
	hr = this->_d3d11_device->CreateVertexShader(g_VS, Size, nullptr, this->_d3d11_vertexShader.GetAddressOf());
	if (FAILED(hr))
		return false;
	constexpr std::array<D3D11_INPUT_ELEMENT_DESC, 2> Layout =
	{ {
		{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		{ "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,    0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	} };

	hr = this->_d3d11_device->CreateInputLayout(Layout.data(), Layout.size(), g_VS, Size, this->_d3d11_inputLayout.GetAddressOf());
	if (FAILED(hr))
		return false;


	//PixelShader
	Size = ARRAYSIZE(g_PS);
	hr = this->_d3d11_device->CreatePixelShader(g_PS, Size, nullptr, this->_d3d11_pixelShader.GetAddressOf());
	if (FAILED(hr))
		return false;

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
	hr = this->_d3d11_device->CreateBuffer(&BufferDesc, &InitData, this->_d3d11_vertexBuffer.GetAddressOf());
	if (FAILED(hr))
		return false;

	return true;
}

bool NV12ToRgbShader::CreateSharedSurf(int width, int height) {
	//
	HRESULT hr{ 0 };

	D3D11_TEXTURE2D_DESC texDesc_nv12;
	ZeroMemory(&texDesc_nv12, sizeof(texDesc_nv12));
	texDesc_nv12.Format = DXGI_FORMAT_NV12;
	texDesc_nv12.Width = width;
	texDesc_nv12.Height = height;
	texDesc_nv12.ArraySize = 1;
	texDesc_nv12.MipLevels = 1;
	texDesc_nv12.BindFlags = D3D11_BIND_SHADER_RESOURCE;
	texDesc_nv12.Usage = D3D11_USAGE_DYNAMIC;
	texDesc_nv12.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	texDesc_nv12.SampleDesc.Count = 1;
	texDesc_nv12.SampleDesc.Quality = 0;
	texDesc_nv12.MiscFlags = 0;
	hr = this->_d3d11_device->CreateTexture2D(&texDesc_nv12, nullptr, this->_texture_nv12.GetAddressOf());
	if (FAILED(hr))
		return false;

	//
	D3D11_SHADER_RESOURCE_VIEW_DESC const luminancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->_texture_nv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = this->_d3d11_device->CreateShaderResourceView(this->_texture_nv12.Get(), &luminancePlaneDesc, this->_luminanceView.GetAddressOf());
	if (FAILED(hr))
		return false;

	//
	D3D11_SHADER_RESOURCE_VIEW_DESC const chrominancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->_texture_nv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8G8_UNORM);
	hr = this->_d3d11_device->CreateShaderResourceView(this->_texture_nv12.Get(), &chrominancePlaneDesc, this->_chrominanceView.GetAddressOf());
	if (FAILED(hr))
		return false;


	D3D11_TEXTURE2D_DESC texDesc_rgba;
	ZeroMemory(&texDesc_rgba, sizeof(texDesc_rgba));
	texDesc_rgba.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	texDesc_rgba.Width = width;
	texDesc_rgba.Height = height;
	texDesc_rgba.ArraySize = 1;
	texDesc_rgba.MipLevels = 1;
	texDesc_rgba.BindFlags = D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE;
	texDesc_rgba.Usage = D3D11_USAGE_DEFAULT;
	texDesc_rgba.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
	texDesc_rgba.SampleDesc.Count = 1;
	texDesc_rgba.SampleDesc.Quality = 0;
	texDesc_rgba.MiscFlags = 0;
	hr = this->_d3d11_device->CreateTexture2D(&texDesc_rgba, nullptr, this->_texture_rgba_target.GetAddressOf());
	if (FAILED(hr))
		return false;

	texDesc_rgba.BindFlags = 0;
	texDesc_rgba.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
	texDesc_rgba.Usage = D3D11_USAGE_STAGING;//cpu read
	texDesc_rgba.MiscFlags = 0;
	hr = this->_d3d11_device->CreateTexture2D(&texDesc_rgba, nullptr, this->_texture_rgba_copy.GetAddressOf());
	if (FAILED(hr))
		return false;


	D3D11_RENDER_TARGET_VIEW_DESC rtvDesc{};
	rtvDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	rtvDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
	rtvDesc.Texture2D.MipSlice = 0;
	hr = this->_d3d11_device->CreateRenderTargetView(this->_texture_rgba_target.Get(), &rtvDesc, this->_renderTargetView.GetAddressOf());
	if (FAILED(hr))
		return false;

	return true;
}

void NV12ToRgbShader::ReleaseSharedSurf() {
	this->_d3d11_deviceCtx->ClearState();
	this->_texture_nv12.Reset();
	this->_luminanceView.Reset();
	this->_chrominanceView.Reset();
	this->_texture_rgba_target.Reset();
	this->_texture_rgba_copy.Reset();
	this->_renderTargetView.Reset();
	this->_width = 0;
	this->_height = 0;
}

void NV12ToRgbShader::DeviceCtxSet(int width, int height) {
	//init set
	this->_d3d11_deviceCtx->IASetInputLayout(this->_d3d11_inputLayout.Get());
	this->_d3d11_deviceCtx->OMSetBlendState(nullptr, blendFactor, 0xffffffff);
	//this->_d3d11_deviceCtx->ClearRenderTargetView(this->_renderTargetView.Get(), blendFactor);
	this->_d3d11_deviceCtx->VSSetShader(this->_d3d11_vertexShader.Get(), nullptr, 0);
	this->_d3d11_deviceCtx->PSSetShader(this->_d3d11_pixelShader.Get(), nullptr, 0);
	this->_d3d11_deviceCtx->PSSetSamplers(0, 1, this->_d3d11_samplerState.GetAddressOf());
	UINT Stride = sizeof(VERTEX);
	UINT Offset = 0;
	this->_d3d11_deviceCtx->IASetVertexBuffers(0, 1, this->_d3d11_vertexBuffer.GetAddressOf(), &Stride, &Offset);
	this->_d3d11_deviceCtx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

	//SharedSurf	
	std::array<ID3D11ShaderResourceView*, 2> const textureViews = {
		this->_luminanceView.Get(),
		this->_chrominanceView.Get()
	};
	this->_d3d11_deviceCtx->PSSetShaderResources(0, textureViews.size(), textureViews.data());
	this->_d3d11_deviceCtx->OMSetRenderTargets(1, this->_renderTargetView.GetAddressOf(), nullptr);

	D3D11_VIEWPORT VP;
	VP.Width = static_cast<FLOAT>(width);
	VP.Height = static_cast<FLOAT>(height);
	VP.MinDepth = 0.0f;
	VP.MaxDepth = 1.0f;
	VP.TopLeftX = 0;
	VP.TopLeftY = 0;
	this->_d3d11_deviceCtx->RSSetViewports(1, &VP);

	UINT x = (UINT)ceil(width * 1.0 / 8);
	UINT y = (UINT)ceil(height * 1.0 / 8);
	UINT z = 1;
	this->_d3d11_deviceCtx->Dispatch(x, y, z);
	this->_width = width;
	this->_height = height;
}

//https://medium.com/swlh/streaming-video-with-ffmpeg-and-directx-11-7395fcb372c4
bool NV12ToRgbShader::Convert(const AVFrame* source, AVFrame* received) {
	HRESULT hr{ 0 };
	if (source == NULL || received == NULL)
		return false;
	if (source->format != AV_PIX_FMT_D3D11)
		return false;
	if (!source->hw_frames_ctx)
		return false;
	
	//init/reinit shader surface
	if (this->_width != source->width || this->_height != source->height) {
		this->ReleaseSharedSurf();
		if (this->CreateSharedSurf(source->width, source->height))
		{
			//this->DeviceCtxSet(source->width, source->height);
		}
		else
		{
			return false;
		}
	}	
	this->DeviceCtxSet(source->width, source->height);
	
	ComPtr<ID3D11Texture2D> texture = (ID3D11Texture2D*)source->data[0];
	const int texture_index = (int)source->data[1];

	//bind/copy ffmpeg hw texture -> local d3d11 texture
	this->_d3d11_deviceCtx->CopySubresourceRegion(
		this->_texture_nv12.Get(), 0, 0, 0, 0,
		texture.Get(), texture_index, nullptr
	);

	this->_d3d11_deviceCtx->Draw(NUMVERTICES, 0);

	//render target view only 1 sub resource https://docs.microsoft.com/en-us/windows/win32/direct3d11/overviews-direct3d-11-resources-subresources
#define CopySubResource 0
	this->_d3d11_deviceCtx->CopyResource(this->_texture_rgba_copy.Get(), this->_texture_rgba_target.Get());

	//get texture output
	D3D11_MAPPED_SUBRESOURCE ms;
	hr = this->_d3d11_deviceCtx->Map(this->_texture_rgba_copy.Get(), CopySubResource, D3D11_MAP_READ, 0, &ms);
	if (FAILED(hr))
		return false;

	bool result = this->CopyMapResource(ms, source, received);

	this->_d3d11_deviceCtx->Unmap(this->_texture_rgba_copy.Get(), 0);

	return result;
}

bool NV12ToRgbShader::CopyMapResource(const D3D11_MAPPED_SUBRESOURCE& ms, const AVFrame* source, AVFrame* received) {
	bool result = false;
	int size = av_image_get_buffer_size(AVPixelFormat::AV_PIX_FMT_BGRA, source->width, source->height, 1);
	if (size <= ms.DepthPitch)
	{
		av_frame_unref(received);
		AVBufferRef* dataref = av_buffer_alloc(size);
		if (dataref != nullptr &&
			avcheck(av_image_fill_arrays(
				received->data, received->linesize, dataref->data,
				AVPixelFormat::AV_PIX_FMT_BGRA, source->width, source->height, 1)))
		{
			av_frame_copy_props(received, source);
			received->format = AVPixelFormat::AV_PIX_FMT_BGRA;
			received->width = source->width;
			received->height = source->height;
			received->pts = source->pts;
			received->pkt_dts = source->pkt_dts;
			received->time_base = source->time_base;
			received->pkt_duration = source->pkt_duration;
			received->pkt_pos = source->pkt_pos;

			received->buf[0] = dataref;
			if (received->linesize[0] == ms.RowPitch)
			{
				memcpy(dataref->data, ms.pData, ms.DepthPitch);
			}
			else
			{
				for (UINT64 i = 0; i < source->height; i++)
				{
					uint8_t* dst = dataref->data + i * received->linesize[0];
					uint8_t* src = (uint8_t*)ms.pData + i * ms.RowPitch;
					memcpy(dst, src, received->linesize[0]);
				}
			}
			result = true;
		}
		else
		{
			av_buffer_unref(&dataref);
		}
	}
	return result;
}