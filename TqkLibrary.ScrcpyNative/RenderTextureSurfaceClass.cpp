#include "pch.h"
#include "RenderTextureSurfaceClass.h"


RenderTextureSurfaceClass::RenderTextureSurfaceClass() {

}

RenderTextureSurfaceClass::~RenderTextureSurfaceClass() {
	this->Shutdown();
}


bool RenderTextureSurfaceClass::Initialize(ID3D11Device* device, IUnknown* surface, bool isNewSurface, bool& isNewtargetView) {
	assert(device != nullptr);
	if (surface == nullptr)
		return false;

	if (isNewSurface || m_pRenderTargetView == nullptr)
	{
		this->Shutdown();



		ComPtr<IDXGIResource> pDXGIResource;
		HRESULT hr = surface->QueryInterface(__uuidof(IDXGIResource), (void**)pDXGIResource.GetAddressOf());
		if (FAILED(hr))
			false;

		HANDLE sharedHandle;
		hr = pDXGIResource->GetSharedHandle(&sharedHandle);
		if (FAILED(hr))
			false;

		//pDXGIResource->Release();


		ComPtr<IUnknown> tempResource11;
		hr = device->OpenSharedResource(sharedHandle, __uuidof(ID3D11Resource), (void**)tempResource11.GetAddressOf());
		if (FAILED(hr))
			false;

		ComPtr<ID3D11Texture2D> pOutputResource;
		hr = tempResource11->QueryInterface(__uuidof(ID3D11Texture2D), (void**)pOutputResource.GetAddressOf());
		if (FAILED(hr))
			false;
		//tempResource11->Release();



		D3D11_RENDER_TARGET_VIEW_DESC rtDesc;
		rtDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
		rtDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
		rtDesc.Texture2D.MipSlice = 0;

		hr = device->CreateRenderTargetView(pOutputResource.Get(), &rtDesc, m_pRenderTargetView.GetAddressOf());
		if (FAILED(hr))
			return false;


		D3D11_TEXTURE2D_DESC outputResourceDesc;
		pOutputResource->GetDesc(&outputResourceDesc);
		this->m_Width = outputResourceDesc.Width;
		this->m_Height = outputResourceDesc.Height;
		isNewtargetView = true;
		//pOutputResource->Release();
	}

	return true;
}
bool RenderTextureSurfaceClass::IsNewFrame(const AVFrame* frame) {
	bool isNewFrame = this->m_currentAVFrameAddress != frame || this->m_currentPts < frame->pts;
	if (isNewFrame) 
	{
		this->m_currentPts = frame->pts;
		this->m_currentAVFrameAddress = frame;
	}
	return isNewFrame;
}
void RenderTextureSurfaceClass::Shutdown() {
	this->m_pRenderTargetView.Reset();
	this->m_Width = 0;
	this->m_Height = 0;
	this->m_currentPts = 0;
}

void RenderTextureSurfaceClass::SetRenderTarget(ID3D11DeviceContext* deviceContext, ID3D11DepthStencilView* depthStencilView) {
	deviceContext->OMSetRenderTargets(1, this->m_pRenderTargetView.GetAddressOf(), depthStencilView);
}
void RenderTextureSurfaceClass::ClearRenderTarget(
	ID3D11DeviceContext* deviceContext, ID3D11DepthStencilView* depthStencilView,
	float red, float green, float blue, float alpha) {
	float color[4];
	// Setup the color to clear the buffer to.
	color[0] = red;
	color[1] = green;
	color[2] = blue;
	color[3] = alpha;

	// Clear the back buffer.
	deviceContext->ClearRenderTargetView(this->m_pRenderTargetView.Get(), color);
}
void RenderTextureSurfaceClass::SetViewPort(ID3D11DeviceContext* device_ctx) {
	D3D11_VIEWPORT VP;
	VP.Width = static_cast<FLOAT>(this->m_Width);
	VP.Height = static_cast<FLOAT>(this->m_Height);
	VP.MinDepth = 0.0f;
	VP.MaxDepth = 1.0f;
	VP.TopLeftX = 0;
	VP.TopLeftY = 0;
	device_ctx->RSSetViewports(1, &VP);
}
void RenderTextureSurfaceClass::SetViewPort(ID3D11DeviceContext* device_ctx, int width, int height) {
	D3D11_VIEWPORT VP;
	VP.Width = static_cast<FLOAT>(width);
	VP.Height = static_cast<FLOAT>(height);
	VP.MinDepth = 0.0f;
	VP.MaxDepth = 1.0f;
	VP.TopLeftX = 0;
	VP.TopLeftY = 0;
	device_ctx->RSSetViewports(1, &VP);
}