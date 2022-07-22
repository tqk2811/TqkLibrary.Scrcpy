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

bool D3DImageView::Draw(D3DImageConvert* imgConvert, IUnknown* surface, bool isNewSurface) {
	assert(imgConvert != nullptr);

	if (surface == NULL)
		return false;

	ComPtr<ID3D11DeviceContext> device_ctx = imgConvert->m_d3d.GetDeviceContext();
	ComPtr<ID3D11Device> device = imgConvert->m_d3d.GetDevice();

	if (this->m_vertex.Initialize(device.Get()) &&
		this->m_pixel.Initialize(device.Get()) &&
		this->m_renderTextureSurface.Initialize(device.Get(), surface, isNewSurface)) {

		if (imgConvert->IsNewFrame(&this->m_currentPts) || isNewSurface)
		{
			device_ctx->ClearState();

			device_ctx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

			m_vertex.Set(device_ctx.Get());
			m_pixel.Set(device_ctx.Get(), imgConvert->m_renderTexture.GetRgbaResourceView());

			m_renderTextureSurface.ClearRenderTarget(device_ctx.Get(), nullptr, 0, 0, 0, 0);
			m_renderTextureSurface.SetRenderTarget(device_ctx.Get(), nullptr);
			m_renderTextureSurface.SetViewPort(device_ctx.Get(), m_renderTextureSurface.Width(), m_renderTextureSurface.Height());

			UINT x = (UINT)ceil(static_cast<FLOAT>(m_renderTextureSurface.Width()) / 8);
			UINT y = (UINT)ceil(static_cast<FLOAT>(m_renderTextureSurface.Height()) / 8);
			UINT z = 1;
			device_ctx->Dispatch(x, y, z);

			device_ctx->Draw(this->m_vertex.GetVertexCount(), 0);
		}
		return true;
	}
	return false;
}