#include "pch.h"
#include "PixelShaderNv12ToImage32Class.h"
#include "PixelShaderNv12_interleaveToRgba.h"
#include "PixelShaderNv12_planarToRgba.h"

#include "PixelShaderNv12_interleaveToBgra.h"
#include "PixelShaderNv12_planarToBgra.h"


PixelShaderNv12ToImage32Class::PixelShaderNv12ToImage32Class(Image32Format outputFormat) {
	this->m_outputFormat = outputFormat;
}

PixelShaderNv12ToImage32Class::~PixelShaderNv12ToImage32Class() {
	this->Shutdown();
}
bool PixelShaderNv12ToImage32Class::Initialize(ID3D11Device* d3d11_device, D3D11_FILTER filter) {
	if (this->m_d3d11_pixelShader_uvInterleave != nullptr && this->m_d3d11_pixelShader_uvPlanar != nullptr) 
		return true;

	HRESULT hr = 0;
	switch (this->m_outputFormat)
	{
	case Image32Format::BGRA:
	{
		UINT Size = ARRAYSIZE(g_PS_interleave_bgra);
		hr = d3d11_device->CreatePixelShader(g_PS_interleave_bgra, Size, nullptr, this->m_d3d11_pixelShader_uvInterleave.GetAddressOf());
		if (FAILED(hr))
			return false;

		Size = ARRAYSIZE(g_PS_planar_bgra);
		hr = d3d11_device->CreatePixelShader(g_PS_planar_bgra, Size, nullptr, this->m_d3d11_pixelShader_uvPlanar.GetAddressOf());
		if (FAILED(hr))
			return false;
		break;
	}
	case Image32Format::RGBA:
	{
		UINT Size = ARRAYSIZE(g_PS_interleave_rgba);
		hr = d3d11_device->CreatePixelShader(g_PS_interleave_rgba, Size, nullptr, this->m_d3d11_pixelShader_uvInterleave.GetAddressOf());
		if (FAILED(hr))
			return false;

		Size = ARRAYSIZE(g_PS_planar_rgba);
		hr = d3d11_device->CreatePixelShader(g_PS_planar_rgba, Size, nullptr, this->m_d3d11_pixelShader_uvPlanar.GetAddressOf());
		if (FAILED(hr))
			return false;
		break;
	}
	default: 
	{
		return FALSE;
	}
	}

	D3D11_SAMPLER_DESC desc = CD3D11_SAMPLER_DESC(CD3D11_DEFAULT());
	desc.Filter = filter;
	hr = d3d11_device->CreateSamplerState(&desc, this->m_d3d11_samplerState.GetAddressOf());
	if (FAILED(hr))
		return false;


	return true;
}
void PixelShaderNv12ToImage32Class::Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* y, ID3D11ShaderResourceView* uv) {

	d3d11_deviceCtx->PSSetShader(this->m_d3d11_pixelShader_uvInterleave.Get(), nullptr, 0);

	d3d11_deviceCtx->PSSetSamplers(0, 1, this->m_d3d11_samplerState.GetAddressOf());

	std::array<ID3D11ShaderResourceView*, 2> const textureViews = { y, uv };
	d3d11_deviceCtx->PSSetShaderResources(0, (UINT)textureViews.size(), textureViews.data());

}
void PixelShaderNv12ToImage32Class::Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* y, ID3D11ShaderResourceView* u, ID3D11ShaderResourceView* v) {

	d3d11_deviceCtx->PSSetShader(this->m_d3d11_pixelShader_uvPlanar.Get(), nullptr, 0);

	d3d11_deviceCtx->PSSetSamplers(0, 1, this->m_d3d11_samplerState.GetAddressOf());

	std::array<ID3D11ShaderResourceView*, 3> const textureViews = { y, u ,v };
	d3d11_deviceCtx->PSSetShaderResources(0, (UINT)textureViews.size(), textureViews.data());

}
void PixelShaderNv12ToImage32Class::Shutdown() {
	this->m_d3d11_pixelShader_uvInterleave.Reset();
	this->m_d3d11_pixelShader_uvPlanar.Reset();
	this->m_d3d11_samplerState.Reset();
}
