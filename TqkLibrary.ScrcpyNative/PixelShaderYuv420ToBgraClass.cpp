#include "pch.h"
#include "PixelShaderYuv420ToBgraClass.h"
#include "PixelShaderYuv420ToBgra.h"

PixelShaderYuv420ToBgraClass::PixelShaderYuv420ToBgraClass() {

}
PixelShaderYuv420ToBgraClass::~PixelShaderYuv420ToBgraClass() {
	this->Shutdown();
}

bool PixelShaderYuv420ToBgraClass::Initialize(ID3D11Device* d3d11_device) {
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
void PixelShaderYuv420ToBgraClass::Set(
	ID3D11DeviceContext* d3d11_deviceCtx,
	ID3D11ShaderResourceView* y,
	ID3D11ShaderResourceView* u,
	ID3D11ShaderResourceView* v) {

	d3d11_deviceCtx->PSSetShader(this->m_d3d11_pixelShader.Get(), nullptr, 0);

	d3d11_deviceCtx->PSSetSamplers(0, 1, this->m_d3d11_samplerState.GetAddressOf());

	std::array<ID3D11ShaderResourceView*, 3> const textureViews = { y, u, v };
	d3d11_deviceCtx->PSSetShaderResources(0, textureViews.size(), textureViews.data());
}

void PixelShaderYuv420ToBgraClass::Shutdown() {
	this->m_d3d11_pixelShader.Reset();
	this->m_d3d11_samplerState.Reset();
}
