#include "pch.h"
#include "D3D11Header.h"

D3DImageConvert::D3DImageConvert() {

}
D3DImageConvert::~D3DImageConvert() {
	this->Shutdown();
}
void D3DImageConvert::Shutdown() {
	this->m_vertex.Shutdown();
	this->m_pixel.Shutdown();
	this->m_input.Shutdown();
	this->m_renderTexture.Shutdown();
	this->m_d3d.Shutdown();
}
bool D3DImageConvert::Initialize(const AVD3D11VADeviceContext* d3d11va_device_ctx) {
	return this->m_d3d.Initialize(d3d11va_device_ctx) &&
		this->m_pixel.Initialize(this->m_d3d.GetDevice()) &&
		this->m_vertex.Initialize(this->m_d3d.GetDevice());
}

bool D3DImageConvert::Convert(const AVFrame* source) {
	if (source == NULL)
		return false;
	if (source->format != AV_PIX_FMT_D3D11)
		return false;
	if (!source->hw_frames_ctx)
		return false;

	ComPtr<ID3D11DeviceContext> device_ctx = this->m_d3d.GetDeviceContext();
	ComPtr<ID3D11Device> device = this->m_d3d.GetDevice();

	if (this->m_input.Initialize(device.Get(), source->width, source->height) &&
		this->m_input.Copy(device_ctx.Get(), source) &&
		this->m_renderTexture.Initialize(device.Get(), source->width, source->height))
	{
		device_ctx->ClearState();

		m_pixel.Set(device_ctx.Get(), m_input.GetLuminanceView(), m_input.GetChrominanceView());
		m_vertex.Set(device_ctx.Get());
		m_renderTexture.ClearRenderTarget(device_ctx.Get(), nullptr, 0, 0, 0, 0);
		m_renderTexture.SetRenderTarget(device_ctx.Get(), nullptr);
		m_renderTexture.SetViewPort(device_ctx.Get(), source->width, source->height);

		device_ctx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

		/*static FLOAT blendFactor[4] = { 0.f, 0.f, 0.f, 0.f };
		device_ctx->OMSetBlendState(nullptr, blendFactor, 0xffffffff);*/

		UINT x = (UINT)ceil(static_cast<FLOAT>(source->width) / 8);
		UINT y = (UINT)ceil(static_cast<FLOAT>(source->height) / 8);
		UINT z = 1;
		device_ctx->Dispatch(x, y, z);

		device_ctx->Draw(NUMVERTICES, 0);
		return true;
	}
	return false;
}

bool D3DImageConvert::GetImage(const AVFrame* source, AVFrame* received) {

	return m_renderTexture.GetImage(this->m_d3d.GetDeviceContext(), source, received);
}