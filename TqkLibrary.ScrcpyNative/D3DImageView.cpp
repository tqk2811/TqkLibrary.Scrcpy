#include "pch.h"
#include "D3DImageView.h"


D3DImageView::D3DImageView() {
}

D3DImageView::~D3DImageView() {
	this->Shutdown();
}

void D3DImageView::Shutdown() {
	this->m_vertex.Shutdown();
	this->m_pixel.Shutdown();
	this->m_renderTextureSurface.Shutdown();
}

bool D3DImageView::Draw(D3DClass* d3d, InputTextureClass* input, const AVFrame* source, IUnknown* surface, bool isNewSurface) {
	if (source == nullptr || surface == NULL || input == nullptr)
		return false;

	if ((source->format == AV_PIX_FMT_D3D11 && source->hw_frames_ctx != nullptr) || source->format == AV_PIX_FMT_YUV420P)
	{
		ComPtr<ID3D11DeviceContext> device_ctx = d3d->GetDeviceContext();
		ComPtr<ID3D11Device> device = d3d->GetDevice();

		if (this->m_vertex.Initialize(device.Get()) &&
			this->m_pixel.Initialize(device.Get()) &&
			this->m_renderTextureSurface.Initialize(device.Get(), surface, isNewSurface)) {

			bool isNewFrame = this->m_currentPts < source->pts;

			if ((isNewFrame || isNewSurface))
			{
				device_ctx->ClearState();

				device_ctx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

				m_vertex.Set(device_ctx.Get());
				m_pixel.Set(device_ctx.Get(), input->GetLuminanceView(), input->GetChrominanceView());

				//m_renderTextureSurface.ClearRenderTarget(device_ctx.Get(), nullptr, 0, 0, 0, 0);
				m_renderTextureSurface.SetRenderTarget(device_ctx.Get(), nullptr);
				m_renderTextureSurface.SetViewPort(device_ctx.Get(), m_renderTextureSurface.Width(), m_renderTextureSurface.Height());

				UINT x = (UINT)ceil(static_cast<FLOAT>(m_renderTextureSurface.Width()) / 8);
				UINT y = (UINT)ceil(static_cast<FLOAT>(m_renderTextureSurface.Height()) / 8);
				UINT z = 1;
				device_ctx->Dispatch(x, y, z);
				this->m_currentPts = source->pts;

				device_ctx->Draw(this->m_vertex.GetVertexCount(), 0);
			}
			return true;
		}
	}
	return false;
}