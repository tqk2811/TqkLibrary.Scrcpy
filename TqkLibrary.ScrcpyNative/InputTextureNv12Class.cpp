#include "pch.h"
#include "InputTextureNv12Class.h"
//
//  420ToNv12.c
//  420ToNv12
//
//  Created by Hank Lee on 5/31/15.
//  Copyright (c) 2015 Hank Lee. All rights reserved.
//
extern "C" {
	//#define SSE
#ifdef SSE
#include <stdint.h>
	typedef char __attribute__((vector_size(8)))    v8qi;
	int planar_to_interleave(
		uint32_t		uv_size,
		uint64_t* u_et_v,
		const uint64_t* u,
		const uint64_t* v
	)
	{
		int i;
		v8qi* res;

		res = (v8qi*)u_et_v;

		for (i = 0; i < wxh / 32; i++)
		{
			res[0] = __builtin_ia32_punpcklbw((v8qi)u[i], (v8qi)v[i]);
			res[1] = __builtin_ia32_punpckhbw((v8qi)u[i], (v8qi)v[i]);

			res += 2;
		}

		return 0;
	}
#else
	int planar_to_interleave
	(
		uint32_t		uv_size,
		uint8_t* u_et_v,
		const uint8_t* u,
		const uint8_t* v
	)
	{
		int i;
		int size = uv_size / 2;
		for (i = 0; i < size; i++)
		{
			uint8_t u_data = u[i];  // fetch u data
			uint8_t v_data = v[i];  // fetch v data

			u_et_v[2 * i] = u_data;   // write u data
			u_et_v[2 * i + 1] = v_data;   // write v data
		}

		return 0;
	}
#endif
}

InputTextureNv12Class::InputTextureNv12Class() {

}

InputTextureNv12Class::~InputTextureNv12Class() {
	this->Shutdown();
}

bool InputTextureNv12Class::Initialize(ID3D11Device* device, int width, int height) {
	assert(device != nullptr);

	if (width != this->m_width || height != this->m_height) {
		this->Shutdown();
	}
	else
	{
		return true;
	}

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

	texDesc_nv12.Usage = D3D11_USAGE_DYNAMIC;
	texDesc_nv12.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	hr = device->CreateTexture2D(&texDesc_nv12, nullptr, this->m_texture_nv12_cache.GetAddressOf());
	if (FAILED(hr))
		return false;


	D3D11_SHADER_RESOURCE_VIEW_DESC const luminancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_nv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_nv12.Get(), &luminancePlaneDesc, this->m_luminanceView.GetAddressOf());
	if (FAILED(hr))
		return false;

	//
	D3D11_SHADER_RESOURCE_VIEW_DESC const chrominancePlaneDesc
		= CD3D11_SHADER_RESOURCE_VIEW_DESC(this->m_texture_nv12.Get(), D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8G8_UNORM);
	hr = device->CreateShaderResourceView(this->m_texture_nv12.Get(), &chrominancePlaneDesc, this->m_chrominanceView.GetAddressOf());
	if (FAILED(hr))
		return false;

	this->m_width = width;
	this->m_height = height;

	return true;
}

void InputTextureNv12Class::Shutdown() {
	m_texture_nv12.Reset();
	m_texture_nv12_cache.Reset();
	m_luminanceView.Reset();
	m_chrominanceView.Reset();
}

ID3D11ShaderResourceView* InputTextureNv12Class::GetLuminanceView() {
	return this->m_luminanceView.Get();
}

ID3D11ShaderResourceView* InputTextureNv12Class::GetChrominanceView() {
	return this->m_chrominanceView.Get();
}

int InputTextureNv12Class::Width() {
	return this->m_width;
}

int InputTextureNv12Class::Height() {
	return this->m_height;
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
		return true;
	}
	else if (sourceFrame->format == AV_PIX_FMT_YUV420P)
	{
		/*this->m_texture_nv12_cache->SetEvictionPriority(DXGI_RESOURCE_PRIORITY_MAXIMUM);
		this->m_texture_nv12->SetEvictionPriority(DXGI_RESOURCE_PRIORITY_MAXIMUM);*/
		device_ctx->ClearState();

		D3D11_MAPPED_SUBRESOURCE map;
		device_ctx->Map(this->m_texture_nv12_cache.Get(), 0, D3D11_MAP::D3D11_MAP_WRITE_DISCARD, 0, &map);

		INT64 y_size = sourceFrame->linesize[0] * sourceFrame->height;
		INT64 uv_size = sourceFrame->linesize[1] * sourceFrame->height;//linesize = 1/2 width, height / 2, * 2 u and v
		INT64 totalSize = y_size + uv_size;

		//assert(map.DepthPitch == 0 || map.DepthPitch == totalSize);
		assert(sourceFrame->linesize[1] == sourceFrame->linesize[2]);

		//std::wstring f(L"sourceFrame linesize: ");
		//f.append(std::to_wstring(sourceFrame->linesize[0]));
		//f.append(L", TotalSize:");
		//f.append(std::to_wstring(totalSize));
		//f.append(L", W:");
		//f.append(std::to_wstring(sourceFrame->width));
		//f.append(L", H:");
		//f.append(std::to_wstring(sourceFrame->height));
		//std::wstring s(L"D3D11_MAPPED_SUBRESOURCE.RowPitch");
		//s.append(std::to_wstring(map.RowPitch));
		//s.append(L", DepthPitch:");
		//s.append(std::to_wstring(map.DepthPitch));
		//MessageBox(NULL, f.c_str(), s.c_str(), 0);

		bool result = false;
		if (
			sourceFrame->linesize[0] == map.RowPitch &&
			(map.DepthPitch == 0 || map.DepthPitch == totalSize))
		{
			memcpy(map.pData, sourceFrame->data[0], y_size);
			planar_to_interleave((UINT32)uv_size, (uint8_t*)((UINT64)map.pData + y_size), sourceFrame->data[1], sourceFrame->data[2]);
			result = true;
		}
		else if (
			(UINT)sourceFrame->linesize[0] < map.RowPitch &&
			map.DepthPitch == map.RowPitch * 3 * sourceFrame->height / 2)
		{
			for (int row = 0; row < sourceFrame->height; row++)
			{
				memcpy(
					(uint8_t*)((UINT64)map.pData + (map.RowPitch * row)),
					sourceFrame->data[0] + (sourceFrame->linesize[0] * row),
					sourceFrame->linesize[0]);
			}

			uint8_t* start_uv = (uint8_t*)map.pData + map.RowPitch * sourceFrame->height;
			int uv_rowSizeCopy = sourceFrame->linesize[1] + sourceFrame->linesize[2];
			int uv_height = sourceFrame->height / 2;
			for (int row = 0; row < uv_height; row++)
			{
				planar_to_interleave(
					uv_rowSizeCopy,
					start_uv + map.RowPitch * row,
					sourceFrame->data[1] + (sourceFrame->linesize[1] * row),
					sourceFrame->data[2] + (sourceFrame->linesize[2] * row));
			}
			result = true;
		}
		else
		{
#if _DEBUG
			MessageBox(NULL, L"Failed", L"", 0);
#endif
		}

		device_ctx->Unmap(this->m_texture_nv12_cache.Get(), 0);

		device_ctx->Flush();//force upload resoure from cpu -> gpu in m_texture_nv12_cache

		device_ctx->CopyResource(this->m_texture_nv12.Get(), this->m_texture_nv12_cache.Get());
		return result;
	}
	return false;
}