#include "pch.h"
#include "PixelShader.h"
#include "VertexShader.h"
#include "Utils.h"
#include "NV12ToRgbShader.h"
#include <d3d11.h>
#include <directxmath.h>
#include <wrl/client.h>
//#include <dxgi.h>

using namespace Microsoft::WRL;
using namespace DirectX;
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

NV12ToRgbShader::NV12ToRgbShader() {
}

NV12ToRgbShader::~NV12ToRgbShader() {
	this->ReleaseSharedSurf();

	if (this->_d3d11_pixelShader != nullptr)
		this->_d3d11_pixelShader->Release();

	if (this->_d3d11_vertexShader != nullptr)
		this->_d3d11_vertexShader->Release();

	if (this->_d3d11_inputLayout != nullptr)
		this->_d3d11_inputLayout->Release();

	if (this->_d3d11_samplerState != nullptr)
		this->_d3d11_samplerState->Release();

	if (this->_d3d11_device != nullptr)
		this->_d3d11_device->Release();

	if (this->_d3d11_deviceCtx != nullptr)
		this->_d3d11_deviceCtx->Release();
}

bool NV12ToRgbShader::Init() {
	HRESULT hr;

	// Driver types supported
	D3D_DRIVER_TYPE DriverTypes[] =
	{
		D3D_DRIVER_TYPE_HARDWARE,
		D3D_DRIVER_TYPE_WARP,
		D3D_DRIVER_TYPE_REFERENCE,
	};
	UINT NumDriverTypes = ARRAYSIZE(DriverTypes);

	// Feature levels supported
	D3D_FEATURE_LEVEL FeatureLevels[] =
	{
		D3D_FEATURE_LEVEL_11_0,
		D3D_FEATURE_LEVEL_10_1,
		D3D_FEATURE_LEVEL_10_0,
		D3D_FEATURE_LEVEL_9_1
	};
	UINT NumFeatureLevels = ARRAYSIZE(FeatureLevels);
	D3D_FEATURE_LEVEL FeatureLevel;
	// This flag adds support for surfaces with a different color channel ordering
	// than the default. It is required for compatibility with Direct2D.
	UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;

	for (UINT DriverTypeIndex = 0; DriverTypeIndex < NumDriverTypes; ++DriverTypeIndex)
	{
		hr = D3D11CreateDevice(nullptr, DriverTypes[DriverTypeIndex], nullptr, creationFlags, FeatureLevels, NumFeatureLevels,
			D3D11_SDK_VERSION, &this->_d3d11_device, &FeatureLevel, &this->_d3d11_deviceCtx);
		if (SUCCEEDED(hr))
		{
			// Device creation succeeded, no need to loop anymore
			break;
		}
	}
	if (FAILED(hr))
		return false;

	D3D11_SAMPLER_DESC desc = CD3D11_SAMPLER_DESC(CD3D11_DEFAULT());
	hr = this->_d3d11_device->CreateSamplerState(&desc, &this->_d3d11_samplerState);
	if (FAILED(hr))
		return false;

	//VertexShader
	UINT Size = ARRAYSIZE(g_VS);
	hr = this->_d3d11_device->CreateVertexShader(g_VS, Size, nullptr, &this->_d3d11_vertexShader);
	if (FAILED(hr))
		return false;

	/*constexpr std::array<D3D11_INPUT_ELEMENT_DESC, 2> Layout =
	{ {
		{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		{ "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,    0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	} };*/
	std::array<D3D11_INPUT_ELEMENT_DESC, 2> Layout;
	Layout[0].SemanticName = "POSITION";
	Layout[0].SemanticIndex = 0;
	Layout[0].Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	Layout[0].InputSlot = 0;
	Layout[0].AlignedByteOffset = 0;
	Layout[0].InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA;
	Layout[0].InstanceDataStepRate = 0;	
	
	Layout[1].SemanticName = "TEXCOORD";
	Layout[1].SemanticIndex = 0;
	Layout[1].Format = DXGI_FORMAT_R32G32_FLOAT;
	Layout[1].InputSlot = 0;
	Layout[1].AlignedByteOffset = D3D11_APPEND_ALIGNED_ELEMENT;
	Layout[1].InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA;
	Layout[1].InstanceDataStepRate = 0;
	
	hr = this->_d3d11_device->CreateInputLayout(Layout.data(), Layout.size(), g_VS, Size, &this->_d3d11_inputLayout);
	if (FAILED(hr))
		return false;
	this->_d3d11_deviceCtx->IASetInputLayout(this->_d3d11_inputLayout);


	//PixelShader
	Size = ARRAYSIZE(g_PS);
	hr = this->_d3d11_device->CreatePixelShader(g_PS, Size, nullptr, &this->_d3d11_pixelShader);
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
	hr = this->_d3d11_device->CreateTexture2D(&texDesc_nv12, nullptr, &this->_texture_nv12);
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
	hr = this->_d3d11_device->CreateTexture2D(&texDesc_rgba, nullptr, &this->_texture_rgba_target);
	if (FAILED(hr))
		return false;


	D3D11_RENDER_TARGET_VIEW_DESC rtvDesc{};
	rtvDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	rtvDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
	rtvDesc.Texture2D.MipSlice = 0;
	hr = this->_d3d11_device->CreateRenderTargetView(this->_texture_rgba_target, &rtvDesc, &this->_renderTargetView);
	if (FAILED(hr))
		return false;

	//
	D3D11_SHADER_RESOURCE_VIEW_DESC const luminancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->_texture_nv12, D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = this->_d3d11_device->CreateShaderResourceView(this->_texture_nv12, &luminancePlaneDesc, &this->_luminanceView);
	if (FAILED(hr))
		return false;

	//
	D3D11_SHADER_RESOURCE_VIEW_DESC const chrominancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->_texture_nv12, D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8G8_UNORM);
	hr = this->_d3d11_device->CreateShaderResourceView(this->_texture_nv12, &chrominancePlaneDesc, &this->_chrominanceView);
	if (FAILED(hr))
		return false;

	this->_width = width;
	this->_height = height;

	return true;
}
void NV12ToRgbShader::ReleaseSharedSurf() {
	if (this->_luminanceView != nullptr) {
		this->_luminanceView->Release();
		this->_luminanceView = nullptr;
	}
	if (this->_chrominanceView != nullptr) {
		this->_chrominanceView->Release();
		this->_chrominanceView = nullptr;
	}
	if (this->_renderTargetView != nullptr) {
		this->_renderTargetView->Release();
		this->_renderTargetView = nullptr;
	}
	if (this->_texture_nv12 != nullptr) {
		this->_texture_nv12->Release();
		this->_texture_nv12 = nullptr;
	}
	if (this->_texture_rgba_target != nullptr) {
		this->_texture_rgba_target->Release();
		this->_texture_rgba_target = nullptr;
	}
	this->_width = 0;
	this->_height = 0;
}




//https://medium.com/swlh/streaming-video-with-ffmpeg-and-directx-11-7395fcb372c4
bool NV12ToRgbShader::Convert(const AVFrame* source, AVFrame** received) {
	HRESULT hr{ 0 };
	if (source == NULL)
		return false;
	if (source->format != AV_PIX_FMT_D3D11)
		return false;
	if (!source->hw_frames_ctx)
		return false;

	if (this->_d3d11_device == nullptr) {
		if (!Init())
			return false;
	}
	//init/reinit shader surface
	if (this->_width != source->width || this->_height != source->height) {
		this->ReleaseSharedSurf();
		if (!this->CreateSharedSurf(source->width, source->height))
			return false;
	}

	//bind/copy ffmpeg hw texture -> local d3d11 texture
	ComPtr<ID3D11Texture2D> texture = (ID3D11Texture2D*)source->data[0];
	const int texture_index = (int)source->data[1];
	this->_d3d11_deviceCtx->CopySubresourceRegion(
		this->_texture_nv12, 0, 0, 0, 0,
		texture.Get(), texture_index, nullptr
	);
	this->_d3d11_deviceCtx->CopyResource(this->_texture_nv12, texture.Get());


	//https://microsoft.github.io/DirectX-Specs/d3d/archive/D3D11_3_FunctionalSpec.htm


	// Rendering NV12 requires two resource views, which represent the luminance and chrominance channels of the YUV formatted texture.	
	std::array<ID3D11ShaderResourceView*, 2> const textureViews = {
		this->_luminanceView,
		this->_chrominanceView
	};
	this->_d3d11_deviceCtx->PSSetShaderResources(0, textureViews.size(), textureViews.data());
	FLOAT blendFactor[4] = { 0.f, 1.f, 0.f, 0.f };
	this->_d3d11_deviceCtx->OMSetBlendState(nullptr, blendFactor, 0xffffffff);
	this->_d3d11_deviceCtx->ClearRenderTargetView(this->_renderTargetView, blendFactor);
	this->_d3d11_deviceCtx->OMSetRenderTargets(1, &this->_renderTargetView, nullptr);
	this->_d3d11_deviceCtx->VSSetShader(this->_d3d11_vertexShader, nullptr, 0);
	this->_d3d11_deviceCtx->PSSetShader(this->_d3d11_pixelShader, nullptr, 0);
	this->_d3d11_deviceCtx->PSSetSamplers(0, 1, &this->_d3d11_samplerState);
	this->_d3d11_deviceCtx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

	D3D11_BUFFER_DESC BufferDesc;
	RtlZeroMemory(&BufferDesc, sizeof(BufferDesc));
	BufferDesc.Usage = D3D11_USAGE_DEFAULT;
	BufferDesc.ByteWidth = sizeof(VERTEX) * NUMVERTICES;
	BufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
	BufferDesc.CPUAccessFlags = 0;
	D3D11_SUBRESOURCE_DATA InitData;
	RtlZeroMemory(&InitData, sizeof(InitData));
	InitData.pSysMem = Vertices;

	ComPtr<ID3D11Buffer> VertexBuffer = nullptr;
	hr = this->_d3d11_device->CreateBuffer(&BufferDesc, &InitData, VertexBuffer.GetAddressOf());
	if (FAILED(hr))
		return false;

	UINT Stride = sizeof(VERTEX);
	UINT Offset = 0;
	_d3d11_deviceCtx->IASetVertexBuffers(0, 1, VertexBuffer.GetAddressOf(), &Stride, &Offset);


	// Draw quad.
	//this->_d3d11_deviceCtx->IASetInputLayout(nullptr);

	_d3d11_deviceCtx->Draw(NUMVERTICES, 0);

	//get draw result
	//make new texture allow cpu access
	ComPtr<ID3D11Texture2D> pInTexture2D = NULL;
	this->_renderTargetView->GetResource(reinterpret_cast<ID3D11Resource**>(pInTexture2D.GetAddressOf()));
	if (pInTexture2D == nullptr)
		return false;

	D3D11_TEXTURE2D_DESC desc2D;
	pInTexture2D->GetDesc(&desc2D);
	desc2D.BindFlags = 0;
	desc2D.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
	desc2D.Usage = D3D11_USAGE_STAGING;
	desc2D.MiscFlags = 0;


	ComPtr<ID3D11Texture2D> pOutTexture2D = NULL;
	hr = this->_d3d11_device->CreateTexture2D(&desc2D, NULL, pOutTexture2D.GetAddressOf());
	if (FAILED(hr))
		return false;

	this->_d3d11_deviceCtx->CopyResource(pOutTexture2D.Get(), pInTexture2D.Get());

	//get texture output
	D3D11_MAPPED_SUBRESOURCE ms;
	UINT uiSubResource = D3D11CalcSubresource(0, 0, 0);
	hr = _d3d11_deviceCtx->Map(pOutTexture2D.Get(), uiSubResource, D3D11_MAP_READ, 0, &ms);
	if (FAILED(hr))
		return false;

	int size = av_image_get_buffer_size(AVPixelFormat::AV_PIX_FMT_BGRA, source->width, source->height, 1);
	if (size == ms.DepthPitch)
	{
		//copy texture data to new frame
		AVFrame* frame = av_frame_alloc();
		if (frame != nullptr) {
			AVBufferRef* dataref = av_buffer_alloc(size);
			memcpy(dataref->data, ms.pData, size);
			if (avcheck(av_image_fill_arrays(
				frame->data, frame->linesize, dataref->data,
				AVPixelFormat::AV_PIX_FMT_BGRA, source->width, source->height, 1)))
			{
				av_frame_copy_props(frame, source);
				frame->buf[0] = dataref;
				frame->format = AVPixelFormat::AV_PIX_FMT_BGRA;
				frame->width = source->width;
				frame->height = source->height;
				frame->pts = source->pts;
				frame->pkt_dts = source->pkt_dts;
				frame->time_base = source->time_base;
				frame->pkt_duration = source->pkt_duration;
				frame->pkt_pos = source->pkt_pos;
			}
			else
			{
				av_buffer_unref(&dataref);
				av_frame_free(&frame);
			}
		}
		*received = frame;
	}
	else
	{

	}

	_d3d11_deviceCtx->Unmap(pOutTexture2D.Get(), 0);
	return *received != nullptr;
}

