#include "pch.h"
#include "InputTextureClass.h"

InputTextureClass::InputTextureClass() {

}

InputTextureClass::~InputTextureClass() {
	this->Shutdown();
}

bool InputTextureClass::Initialize(ID3D11Device* device, int width, int height) {
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
	HRESULT hr = device->CreateTexture2D(&texDesc_nv12, nullptr, this->m_texture_nv12.GetAddressOf());
	if (FAILED(hr))
		return false;

	D3D11_SHADER_RESOURCE_VIEW_DESC const luminancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_nv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_nv12.Get(), &luminancePlaneDesc, this->m_luminanceView.GetAddressOf());
	if (FAILED(hr))
		return false;

	//
	D3D11_SHADER_RESOURCE_VIEW_DESC const chrominancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_nv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8G8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_nv12.Get(), &chrominancePlaneDesc, this->m_chrominanceView.GetAddressOf());
	if (FAILED(hr))
		return false;

	this->m_width = width;
	this->m_height = height;

	return true;
}

void InputTextureClass::Shutdown() {
	m_texture_nv12.Reset();
	m_luminanceView.Reset();
	m_chrominanceView.Reset();
}
ID3D11ShaderResourceView* InputTextureClass::GetLuminanceView() {
	return this->m_luminanceView.Get();
}
ID3D11ShaderResourceView* InputTextureClass::GetChrominanceView() {
	return this->m_chrominanceView.Get();
}
bool InputTextureClass::Copy(ID3D11DeviceContext* device_ctx, const AVFrame* sourceFrame) {
	if (sourceFrame->format != AV_PIX_FMT_D3D11)
		return false;
	if (!sourceFrame->hw_frames_ctx)
		return false;

	ComPtr<ID3D11Texture2D> texture = (ID3D11Texture2D*)sourceFrame->data[0];

	const int texture_index = (int)sourceFrame->data[1];
	D3D11_BOX box{ 0 };
	box.left = 0;
	box.right = sourceFrame->width;
	box.top = 0;
	box.bottom = sourceFrame->height;
	box.front = 0;
	box.back = 1;//https://docs.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-copysubresourceregion

	device_ctx->CopySubresourceRegion(
		this->m_texture_nv12.Get(), 0, 0, 0, 0,
		texture.Get(), texture_index, &box
	);
}