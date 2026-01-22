#include "pch.h"
#include "InputTextureNv12Class.h"

InputTextureNv12Class::InputTextureNv12Class() {

}

InputTextureNv12Class::~InputTextureNv12Class() {
	this->Shutdown();
}

bool InputTextureNv12Class::Initialize(ID3D11Device* device, int width, int height) {
	assert(device != nullptr);

	if (width == this->m_width && height == this->m_height)
		return true;

	this->Shutdown();

	D3D11_TEXTURE2D_DESC texDesc_nv12;
	ZeroMemory(&texDesc_nv12, sizeof(texDesc_nv12));
	texDesc_nv12.Format = DXGI_FORMAT_NV12;
	texDesc_nv12.Width = width;
	texDesc_nv12.Height = height;
	texDesc_nv12.ArraySize = 1;
	texDesc_nv12.MipLevels = 1;
	texDesc_nv12.BindFlags = D3D11_BIND_SHADER_RESOURCE;
	texDesc_nv12.Usage = D3D11_USAGE_DEFAULT;// D3D11_USAGE_DYNAMIC;
	texDesc_nv12.CPUAccessFlags = 0;// D3D11_CPU_ACCESS_WRITE;
	texDesc_nv12.SampleDesc.Count = 1;
	texDesc_nv12.SampleDesc.Quality = 0;
	texDesc_nv12.MiscFlags = 0;
	HRESULT hr = device->CreateTexture2D(&texDesc_nv12, nullptr, this->m_texture_nv12.GetAddressOf());
	if (FAILED(hr))
		return false;


	D3D11_SHADER_RESOURCE_VIEW_DESC const luminancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_nv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_nv12.Get(), &luminancePlaneDesc, this->m_y_View_nv12.GetAddressOf());
	if (FAILED(hr))
		return false;

	//
	D3D11_SHADER_RESOURCE_VIEW_DESC const chrominancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_nv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8G8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_nv12.Get(), &chrominancePlaneDesc, this->m_uv_View.GetAddressOf());
	if (FAILED(hr))
		return false;





	D3D11_TEXTURE2D_DESC lumiDesc = { 0 };
	lumiDesc.Width = width;
	lumiDesc.Height = height;
	lumiDesc.MipLevels = 1;
	lumiDesc.ArraySize = 1;
	lumiDesc.Format = DXGI_FORMAT_R8_UNORM;
	lumiDesc.SampleDesc.Count = 1;
	lumiDesc.Usage = D3D11_USAGE_DYNAMIC;// Dynamic để CPU map/write
	lumiDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
	lumiDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	lumiDesc.MiscFlags = 0;
	hr = device->CreateTexture2D(&lumiDesc, nullptr, this->m_texture_y.GetAddressOf());
	if (FAILED(hr)) return false;

	D3D11_TEXTURE2D_DESC chromaDesc = { 0 };
	chromaDesc.Width = width / 2;
	chromaDesc.Height = height / 2;
	chromaDesc.MipLevels = 1;
	chromaDesc.ArraySize = 1;
	chromaDesc.Format = DXGI_FORMAT_R8_UNORM;
	chromaDesc.SampleDesc.Count = 1;
	chromaDesc.Usage = D3D11_USAGE_DYNAMIC;// Dynamic để CPU map/write
	chromaDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
	chromaDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	chromaDesc.MiscFlags = 0;
	hr = device->CreateTexture2D(&chromaDesc, nullptr, this->m_texture_u.GetAddressOf());
	if (FAILED(hr)) return false;
	hr = device->CreateTexture2D(&chromaDesc, nullptr, this->m_texture_v.GetAddressOf());
	if (FAILED(hr)) return false;

	D3D11_SHADER_RESOURCE_VIEW_DESC const yPlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_y.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_y.Get(), &yPlaneDesc, this->m_y_View_planar.GetAddressOf());
	if (FAILED(hr))
		return false;

	D3D11_SHADER_RESOURCE_VIEW_DESC const uPlaneDesc =
		CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_u.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_u.Get(), &uPlaneDesc, this->m_u_View.GetAddressOf());
	if (FAILED(hr)) return false;

	D3D11_SHADER_RESOURCE_VIEW_DESC const vPlaneDesc =
		CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_v.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_v.Get(), &vPlaneDesc, this->m_v_View.GetAddressOf());
	if (FAILED(hr)) return false;


	this->m_width = width;
	this->m_height = height;

	return true;
}

void InputTextureNv12Class::Shutdown() {
	m_texture_nv12.Reset();
	m_texture_y.Reset();
	m_texture_u.Reset();
	m_texture_v.Reset();

	m_y_View_nv12.Reset();
	m_y_View_planar.Reset();
	m_uv_View.Reset();
	m_u_View.Reset();
	m_v_View.Reset();
}

bool InputTextureNv12Class::Copy(ID3D11DeviceContext* device_ctx, const AVFrame* sourceFrame) {
	if (sourceFrame->format == AV_PIX_FMT_D3D11 && sourceFrame->hw_frames_ctx != nullptr)
	{
		ComPtr<ID3D11Texture2D> texture = (ID3D11Texture2D*)sourceFrame->data[0];
		const UINT64 texture_index = (UINT64)sourceFrame->data[1];

		D3D11_TEXTURE2D_DESC desc{ 0 };
		texture->GetDesc(&desc);

		D3D11_BOX box{ 0 };
		box.left = 0;
		box.right = sourceFrame->width;
		box.top = 0;
		box.bottom = sourceFrame->height;
		box.front = 0;
		box.back = 1;//https://docs.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-copysubresourceregion

		device_ctx->CopySubresourceRegion(
			this->m_texture_nv12.Get(), 0, 0, 0, 0,
			texture.Get(), (UINT32)texture_index, &box
		);
		this->m_isPlanar = FALSE;
		return true;
	}
	else if (sourceFrame->format == AV_PIX_FMT_YUV420P)
	{
		INT uv_height = sourceFrame->height / 2;
		INT64 y_size = sourceFrame->linesize[0] * sourceFrame->height;
		INT64 uv_size = sourceFrame->linesize[1] * uv_height;

		device_ctx->ClearState();
		HRESULT hr = 0;

		D3D11_MAPPED_SUBRESOURCE map;

		hr = device_ctx->Map(this->m_texture_y.Get(), 0, D3D11_MAP::D3D11_MAP_WRITE_DISCARD, 0, &map);
		if (FAILED(hr))
			return FALSE;
		if (sourceFrame->linesize[0] == map.RowPitch)
		{
			memcpy(map.pData, sourceFrame->data[0], y_size);
		}
		else
		{
			size_t lineSize = min(sourceFrame->linesize[0], map.RowPitch);
			for (int i = 0; i < sourceFrame->height; i++)
			{
				memcpy(
					(uint8_t*)map.pData + i * map.RowPitch,
					sourceFrame->data[0] + i * sourceFrame->linesize[0],
					lineSize
				);
			}
		}
		device_ctx->Unmap(this->m_texture_y.Get(), 0);
		ZeroMemory(&map, sizeof(D3D11_MAPPED_SUBRESOURCE));

		hr = device_ctx->Map(this->m_texture_u.Get(), 0, D3D11_MAP::D3D11_MAP_WRITE_DISCARD, 0, &map);
		if (FAILED(hr))
			return FALSE;
		if (sourceFrame->linesize[1] == map.RowPitch)
		{
			memcpy(map.pData, sourceFrame->data[1], uv_size);
		}
		else
		{
			size_t lineSize = min(sourceFrame->linesize[1], map.RowPitch);
			for (int i = 0; i < uv_height; i++)
			{
				memcpy(
					(uint8_t*)map.pData + i * map.RowPitch,
					sourceFrame->data[1] + i * sourceFrame->linesize[1],
					lineSize
				);
			}
		}
		device_ctx->Unmap(this->m_texture_u.Get(), 0);
		ZeroMemory(&map, sizeof(D3D11_MAPPED_SUBRESOURCE));

		hr = device_ctx->Map(this->m_texture_v.Get(), 0, D3D11_MAP::D3D11_MAP_WRITE_DISCARD, 0, &map);
		if (FAILED(hr))
			return FALSE;
		if (sourceFrame->linesize[2] == map.RowPitch)
		{
			memcpy(map.pData, sourceFrame->data[2], uv_size);
		}
		else
		{
			size_t lineSize = min(sourceFrame->linesize[2], map.RowPitch);
			for (int i = 0; i < uv_height; i++)
			{
				memcpy(
					(uint8_t*)map.pData + i * map.RowPitch,
					sourceFrame->data[2] + i * sourceFrame->linesize[2],
					lineSize
				);
			}
		}
		device_ctx->Unmap(this->m_texture_v.Get(), 0);
		ZeroMemory(&map, sizeof(D3D11_MAPPED_SUBRESOURCE));

		device_ctx->Flush();//force upload resoure from cpu -> gpu

		this->m_isPlanar = TRUE;
		return TRUE;
	}
	return false;
}