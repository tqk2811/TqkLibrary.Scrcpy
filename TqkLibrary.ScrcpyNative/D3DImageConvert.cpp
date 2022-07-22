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
	this->m_renderTexture.Shutdown();
}
bool D3DImageConvert::Initialize(D3DClass* d3d) {
	return
		this->m_pixel.Initialize(d3d->GetDevice()) &&
		this->m_vertex.Initialize(d3d->GetDevice());
}

bool D3DImageConvert::Convert(D3DClass* d3d, InputTextureClass* input, const AVFrame* source) {
	if (source == NULL)
		return false;

	if ((source->format == AV_PIX_FMT_D3D11 && source->hw_frames_ctx != nullptr) || source->format == AV_PIX_FMT_YUV420P)
	{
		ComPtr<ID3D11DeviceContext> device_ctx = d3d->GetDeviceContext();
		ComPtr<ID3D11Device> device = d3d->GetDevice();

		if (this->m_renderTexture.Initialize(device.Get(), source->width, source->height))
		{
			device_ctx->ClearState();

			device_ctx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

			m_vertex.Set(device_ctx.Get());

			m_pixel.Set(device_ctx.Get(), input->GetLuminanceView(), input->GetChrominanceView());

			m_renderTexture.ClearRenderTarget(device_ctx.Get(), nullptr, 0, 0, 0, 0);
			m_renderTexture.SetRenderTarget(device_ctx.Get(), nullptr);
			m_renderTexture.SetViewPort(device_ctx.Get(), source->width, source->height);

			/*static FLOAT blendFactor[4] = { 0.f, 0.f, 0.f, 0.f };
			device_ctx->OMSetBlendState(nullptr, blendFactor, 0xffffffff);*/

			UINT x = (UINT)ceil(static_cast<FLOAT>(source->width) / 8);
			UINT y = (UINT)ceil(static_cast<FLOAT>(source->height) / 8);
			UINT z = 1;
			device_ctx->Dispatch(x, y, z);

			device_ctx->Draw(this->m_vertex.GetVertexCount(), 0);
			return true;
		}
	}
	return false;
}

bool D3DImageConvert::GetImage(D3DClass* d3d, const AVFrame* source, AVFrame* received) {

	return m_renderTexture.GetImage(d3d->GetDeviceContext(), source, received);
}