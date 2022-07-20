#include "pch.h"
#include "RenderTextureClass.h"
#include "Utils.h"

RenderTextureClass::RenderTextureClass() {
}

RenderTextureClass::~RenderTextureClass() {
	this->Shutdown();
}

bool RenderTextureClass::Initialize(ID3D11Device* device, int textureWidth, int textureHeight)
{
	HRESULT hr;

	if (this->m_textureWidth == textureWidth && this->m_textureHeight == textureHeight)
		return true;
	
	this->Shutdown();

	D3D11_TEXTURE2D_DESC textureDesc;
	// Initialize the render target texture description.
	ZeroMemory(&textureDesc, sizeof(textureDesc));
	// Setup the render target texture description.
	textureDesc.Width = textureWidth;
	textureDesc.Height = textureHeight;
	textureDesc.MipLevels = 1;
	textureDesc.ArraySize = 1;
	textureDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	textureDesc.SampleDesc.Count = 1;
	textureDesc.SampleDesc.Quality = 0;
	textureDesc.Usage = D3D11_USAGE_DEFAULT;
	textureDesc.BindFlags = D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE;
	textureDesc.CPUAccessFlags = 0;
	textureDesc.MiscFlags = 0;

	// Create the render target texture.
	hr = device->CreateTexture2D(&textureDesc, NULL, m_renderTargetTexture.GetAddressOf());
	if (FAILED(hr))
		return false;


	textureDesc.BindFlags = 0;
	textureDesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
	textureDesc.Usage = D3D11_USAGE_STAGING;//cpu read
	textureDesc.MiscFlags = 0;
	hr = device->CreateTexture2D(&textureDesc, nullptr, this->m_renderTargetTexture_copy.GetAddressOf());
	if (FAILED(hr))
		return false;



	D3D11_RENDER_TARGET_VIEW_DESC renderTargetViewDesc;
	// Setup the description of the render target view.
	renderTargetViewDesc.Format = textureDesc.Format;
	renderTargetViewDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
	renderTargetViewDesc.Texture2D.MipSlice = 0;
	// Create the render target view.
	hr = device->CreateRenderTargetView(m_renderTargetTexture.Get(), &renderTargetViewDesc, m_renderTargetView.GetAddressOf());
	if (FAILED(hr))
		return false;

	this->m_textureWidth = textureWidth;
	this->m_textureHeight = textureHeight;

	return true;
}

void RenderTextureClass::Shutdown()
{
	m_renderTargetView.Reset();
	m_renderTargetTexture.Reset();
	m_renderTargetTexture_copy.Reset();
	return;
}

void RenderTextureClass::SetRenderTarget(ID3D11DeviceContext* deviceContext, ID3D11DepthStencilView* depthStencilView)
{
	// Bind the render target view and depth stencil buffer to the output render pipeline.
	deviceContext->OMSetRenderTargets(1, &m_renderTargetView, depthStencilView);
	return;
}
void RenderTextureClass::SetViewPort(ID3D11DeviceContext* device_ctx, int width, int height) {
	D3D11_VIEWPORT VP;
	VP.Width = static_cast<FLOAT>(width);
	VP.Height = static_cast<FLOAT>(height);
	VP.MinDepth = 0.0f;
	VP.MaxDepth = 1.0f;
	VP.TopLeftX = 0;
	VP.TopLeftY = 0;
	device_ctx->RSSetViewports(1, &VP);
}

void RenderTextureClass::ClearRenderTarget(
	ID3D11DeviceContext* deviceContext, ID3D11DepthStencilView* depthStencilView,
	float red, float green, float blue, float alpha) {

	float color[4];
	// Setup the color to clear the buffer to.
	color[0] = red;
	color[1] = green;
	color[2] = blue;
	color[3] = alpha;

	// Clear the back buffer.
	deviceContext->ClearRenderTargetView(m_renderTargetView.Get(), color);

	// Clear the depth buffer.
	deviceContext->ClearDepthStencilView(depthStencilView, D3D11_CLEAR_DEPTH, 1.0f, 0);
}

bool RenderTextureClass::GetImage(ID3D11DeviceContext* deviceContext, const AVFrame* source, AVFrame* received) {
	bool result = false;

	deviceContext->CopyResource(this->m_renderTargetTexture_copy.Get(), this->m_renderTargetTexture.Get());

	D3D11_MAPPED_SUBRESOURCE ms;
	HRESULT hr = deviceContext->Map(this->m_renderTargetTexture_copy.Get(), 0, D3D11_MAP_READ, 0, &ms);
	if (FAILED(hr))
		return false;

	int size = av_image_get_buffer_size(AVPixelFormat::AV_PIX_FMT_BGRA, source->width, source->height, 1);
	if ((size <= ms.DepthPitch) && (ms.RowPitch * source->height == ms.DepthPitch))
	{
		av_frame_unref(received);
		AVBufferRef* dataref = av_buffer_alloc(size);
		if (dataref != nullptr &&
			avcheck(av_image_fill_arrays(
				received->data, received->linesize, dataref->data,
				AVPixelFormat::AV_PIX_FMT_BGRA, source->width, source->height, 1)))
		{
			av_frame_copy_props(received, source);
			received->format = AVPixelFormat::AV_PIX_FMT_BGRA;
			received->width = source->width;
			received->height = source->height;
			received->pts = source->pts;
			received->pkt_dts = source->pkt_dts;
			received->time_base = source->time_base;
			received->pkt_duration = source->pkt_duration;
			received->pkt_pos = source->pkt_pos;

			received->buf[0] = dataref;
			if (received->linesize[0] == ms.RowPitch)
			{
				memcpy(dataref->data, ms.pData, ms.DepthPitch);
				result = true;
			}
			else if (received->linesize[0] < ms.RowPitch)
			{
				for (UINT64 i = 0; i < source->height; i++)
				{
					uint8_t* dst = dataref->data + i * received->linesize[0];
					uint8_t* src = (uint8_t*)ms.pData + i * ms.RowPitch;
					memcpy(dst, src, received->linesize[0]);
				}
				result = true;
			}
		}
		else
		{
			av_buffer_unref(&dataref);
		}
	}
	deviceContext->Unmap(this->m_renderTargetTexture_copy.Get(), 0);
	return result;
}