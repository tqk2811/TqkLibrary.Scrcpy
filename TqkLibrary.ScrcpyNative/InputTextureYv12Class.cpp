#include "pch.h"
#include "InputTextureYv12Class.h"


InputTextureYv12Class::InputTextureYv12Class() {

}
InputTextureYv12Class::~InputTextureYv12Class() {
	this->Shutdown();
}


bool InputTextureYv12Class::Initialize(ID3D11Device* device, int width, int height) {
	assert(device != nullptr);

	if (width != this->m_width || height != this->m_height) {
		this->Shutdown();
	}
	else
	{
		return true;
	}

	D3D11_TEXTURE2D_DESC texDesc_nv12;
	ZeroMemory(&texDesc_nv12, sizeof(texDesc_nv12));
	texDesc_nv12.Format = DXGI_FORMAT_420_OPAQUE;
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
	HRESULT hr = device->CreateTexture2D(&texDesc_nv12, nullptr, this->m_texture_yv12.GetAddressOf());
	if (FAILED(hr))
		return false;

	D3D11_SHADER_RESOURCE_VIEW_DESC const yPlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_yv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_yv12.Get(), &yPlaneDesc, this->m_yView.GetAddressOf());
	if (FAILED(hr))
		return false;

	//
	D3D11_SHADER_RESOURCE_VIEW_DESC const vPlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_yv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_yv12.Get(), &vPlaneDesc, this->m_vView.GetAddressOf());
	if (FAILED(hr))
		return false;

	D3D11_SHADER_RESOURCE_VIEW_DESC const uPlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_yv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_yv12.Get(), &uPlaneDesc, this->m_uView.GetAddressOf());
	if (FAILED(hr))
		return false;

	this->m_width = width;
	this->m_height = height;

	return true;
}


void InputTextureYv12Class::Shutdown() {
	this->m_texture_yv12.Reset();
	this->m_yView.Reset();
	this->m_uView.Reset();
	this->m_vView.Reset();
}

bool InputTextureYv12Class::Copy(ID3D11DeviceContext* device_ctx, const AVFrame* sourceFrame) {
	if (sourceFrame->format == AV_PIX_FMT_YUV420P)
	{
		D3D11_MAPPED_SUBRESOURCE ms;
		HRESULT hr = device_ctx->Map(this->m_texture_yv12.Get(), 0, D3D11_MAP::D3D11_MAP_WRITE_DISCARD, 0, &ms);
		if (FAILED(hr))
			return false;

		UINT64 ySize = sourceFrame->linesize[0] * sourceFrame->height;
		UINT64 uSize = sourceFrame->linesize[1] * sourceFrame->height / 2;
		UINT64 vSize = uSize;

		UINT64 totalSize = ySize + uSize + vSize;

		assert(ms.DepthPitch == totalSize || ms.DepthPitch == 0);
		assert(sourceFrame->linesize[1] == sourceFrame->linesize[2]);
		assert(ms.RowPitch == sourceFrame->linesize[0]);

		// YYYYYYYY......VVVVVV....UUUUUU.... 
		memcpy(ms.pData, sourceFrame->data[0], ySize);//y
		memcpy((void*)((UINT64)ms.pData + ySize), sourceFrame->data[2], vSize);//v
		memcpy((void*)((UINT64)ms.pData + ySize + vSize), sourceFrame->data[1], uSize);//u

		device_ctx->Unmap(this->m_texture_yv12.Get(), 0);
		return true;
	}
	return false;
}
int InputTextureYv12Class::Width() {
	return this->m_width;
}

int InputTextureYv12Class::Height() {
	return this->m_height;
}

ID3D11ShaderResourceView* InputTextureYv12Class::GetYView() {
	return this->m_yView.Get();
}
ID3D11ShaderResourceView* InputTextureYv12Class::GetUView() {
	return this->m_uView.Get();
}
ID3D11ShaderResourceView* InputTextureYv12Class::GetVView() {
	return this->m_vView.Get();
}