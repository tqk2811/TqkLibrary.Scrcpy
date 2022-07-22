#include "pch.h"
#include "PixelShadeRgbaCopy.h"
#include "PixelShaderCopyClass.h"

PixelShaderCopyClass::PixelShaderCopyClass() {

}

PixelShaderCopyClass::~PixelShaderCopyClass() {
	this->Shutdown();
}
bool PixelShaderCopyClass::Initialize(ID3D11Device* d3d11_device) {
	if (this->m_d3d11_pixelShader != nullptr) return true;

	UINT Size = ARRAYSIZE(g_PS);
	HRESULT hr = d3d11_device->CreatePixelShader(g_PS, Size, nullptr, this->m_d3d11_pixelShader.GetAddressOf());
	if (FAILED(hr))
		return false;

	D3D11_SAMPLER_DESC desc = CD3D11_SAMPLER_DESC(CD3D11_DEFAULT());
	hr = d3d11_device->CreateSamplerState(&desc, this->m_d3d11_samplerState.GetAddressOf());
	if (FAILED(hr))
		return false;


	return true;
}
void PixelShaderCopyClass::Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* rgba) {

	d3d11_deviceCtx->PSSetShader(this->m_d3d11_pixelShader.Get(), nullptr, 0);

	d3d11_deviceCtx->PSSetSamplers(0, 1, this->m_d3d11_samplerState.GetAddressOf());

	std::array<ID3D11ShaderResourceView*, 1> const textureViews = { rgba };
	d3d11_deviceCtx->PSSetShaderResources(0, textureViews.size(), textureViews.data());

}
void PixelShaderCopyClass::Shutdown() {
	this->m_d3d11_pixelShader.Reset();
	this->m_d3d11_samplerState.Reset();
}
