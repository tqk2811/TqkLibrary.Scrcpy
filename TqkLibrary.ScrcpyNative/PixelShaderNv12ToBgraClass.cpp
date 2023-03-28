#include "pch.h"
#include "PixelShaderNv12ToBgra.h"
#include "PixelShaderNv12ToBgraClass.h"

PixelShaderNv12ToBgraClass::PixelShaderNv12ToBgraClass() {

}

PixelShaderNv12ToBgraClass::~PixelShaderNv12ToBgraClass() {
	this->Shutdown();
}
bool PixelShaderNv12ToBgraClass::Initialize(ID3D11Device* d3d11_device, D3D11_FILTER filter) {
	if (this->m_d3d11_pixelShader != nullptr) return true;

	UINT Size = ARRAYSIZE(g_PS);
	HRESULT hr = d3d11_device->CreatePixelShader(g_PS, Size, nullptr, this->m_d3d11_pixelShader.GetAddressOf());
	if (FAILED(hr))
		return false;

	D3D11_SAMPLER_DESC desc = CD3D11_SAMPLER_DESC(CD3D11_DEFAULT());
	desc.Filter = filter;
	hr = d3d11_device->CreateSamplerState(&desc, this->m_d3d11_samplerState.GetAddressOf());
	if (FAILED(hr))
		return false;


	return true;
}
void PixelShaderNv12ToBgraClass::Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* luminance, ID3D11ShaderResourceView* chrominance) {

	d3d11_deviceCtx->PSSetShader(this->m_d3d11_pixelShader.Get(), nullptr, 0);

	d3d11_deviceCtx->PSSetSamplers(0, 1, this->m_d3d11_samplerState.GetAddressOf());

	std::array<ID3D11ShaderResourceView*, 2> const textureViews = { luminance, chrominance };
	d3d11_deviceCtx->PSSetShaderResources(0, (UINT32)textureViews.size(), textureViews.data());

}
void PixelShaderNv12ToBgraClass::Shutdown() {
	this->m_d3d11_pixelShader.Reset();
	this->m_d3d11_samplerState.Reset();
}
