#include "pch.h"
#include "NV12ToRgbShader.h"
//https://medium.com/swlh/streaming-video-with-ffmpeg-and-directx-11-7395fcb372c4
//https://gist.github.com/RomiTT/9c05d36fe339b899793a3252297a5624#file-yuv_bt2020_to_rgb-hlsl
const char* shaderStr =
"struct PixelShaderInput\
{\
	min16float4 pos         : SV_POSITION;\
	min16float2 texCoord    : TEXCOORD0;\
};\
\
Texture2D<float>  luminanceChannel   : t0;\
Texture2D<float2> chrominanceChannel : t1;\
SamplerState      defaultSampler     : s0;\
\
// Derived from https://msdn.microsoft.com/en-us/library/windows/desktop/dd206750(v=vs.85).aspx\
// Section: Converting 8-bit YUV to RGB888\
static const float3x3 YUVtoRGBCoeffMatrix =\
{\
	1.164383f,  1.164383f, 1.164383f,\
	0.000000f, -0.391762f, 2.017232f,\
	1.596027f, -0.812968f, 0.000000f\
};\
\
float3 ConvertYUVtoRGB(float3 yuv)\
{\
	// Derived from https://msdn.microsoft.com/en-us/library/windows/desktop/dd206750(v=vs.85).aspx\
	// Section: Converting 8-bit YUV to RGB888\
\
	// These values are calculated from (16 / 255) and (128 / 255)\
	yuv -= float3(0.062745f, 0.501960f, 0.501960f);\
	yuv = mul(yuv, YUVtoRGBCoeffMatrix);\
\
	return saturate(yuv);\
}\
\
min16float4 PS(PixelShaderInput input) : SV_TARGET\
{\
	float y = luminanceChannel.Sample(defaultSampler, input.texCoord);\
	float2 uv = chrominanceChannel.Sample(defaultSampler, input.texCoord);\
\
	return min16float4(ConvertYUVtoRGB(float3(y, uv)), 1.f);\
}";


NV12ToRgbShader::NV12ToRgbShader(AVHWDeviceContext* deviceContext) {
	if (deviceContext != nullptr &&
		deviceContext->type == AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA) {

		this->_avhw_deviceCtx = deviceContext;
		//This struct is allocated as AVHWDeviceContext.hwctx
		this->_av_d3d11_vaDeviceCtx = (AVD3D11VADeviceContext*)deviceContext->hwctx;
		this->_d3d11_deviceCtx = this->_av_d3d11_vaDeviceCtx->device_context;
		this->_d3d11_device = _av_d3d11_vaDeviceCtx->device;
	}
}

NV12ToRgbShader::~NV12ToRgbShader() {
	if (this->_d3d11_pixelShader != nullptr) this->_d3d11_pixelShader->Release();
}

bool NV12ToRgbShader::Init() {
	if (!InitShader()) return false;
}

bool NV12ToRgbShader::InitShader() {
	if (this->_d3d11_device == nullptr) return false;

	HRESULT hr = this->_d3d11_device->CreatePixelShader(
		shaderStr,
		strlen(shaderStr),
		NULL,
		&this->_d3d11_pixelShader);

	if (FAILED(hr)) return false;

	return true;
}

//https://medium.com/swlh/streaming-video-with-ffmpeg-and-directx-11-7395fcb372c4
bool NV12ToRgbShader::Convert(const AVFrame* source, AVFrame** received) {
	if (source == NULL) return false;
	if (source->format != AV_PIX_FMT_D3D11) return false;
	if (!source->hw_frames_ctx) return false;
	if (this->_avhw_deviceCtx == nullptr) return false;

	//For hwaccel-format frames, this should be a reference to the AVHWFramesContext describing the frame.
	AVHWFramesContext* frameCtx = (AVHWFramesContext*)source->hw_frames_ctx->data;
	if (frameCtx->device_ctx != _avhw_deviceCtx) return false;

	//process frame
	//This struct is allocated as AVHWFramesContext.hwctx
	AVD3D11VAFramesContext* d3dFrameCtx = (AVD3D11VAFramesContext*)frameCtx->hwctx;
	
	ID3D11Texture2D* texture = d3dFrameCtx->texture;
	D3D11_TEXTURE2D_DESC textureDesc{ 0 };
	texture->GetDesc(&textureDesc);
	HRESULT hr = 0;
	
	/*D3D11_MAPPED_SUBRESOURCE ms{ 0 };
	HRESULT hr = _d3d11_deviceCtx->Map(texture, 0, D3D11_MAP_READ, 0, &ms);
	if (FAILED(hr)) return false;*/


	// DXGI_FORMAT_R8_UNORM for NV12 luminance channel
	/*D3D11_SHADER_RESOURCE_VIEW_DESC luminance_desc = CD3D11_SHADER_RESOURCE_VIEW_DESC(texture, D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8_UNORM);
	ID3D11ShaderResourceView* m_luminance_shader_resource_view{ nullptr };
	hr = _d3d11_device->CreateShaderResourceView(texture, &luminance_desc, &m_luminance_shader_resource_view);
	if (FAILED(hr)) return false;*/

	// DXGI_FORMAT_R8G8_UNORM for NV12 chrominance channel
	D3D11_SHADER_RESOURCE_VIEW_DESC chrominance_desc = CD3D11_SHADER_RESOURCE_VIEW_DESC(texture, D3D11_SRV_DIMENSION_TEXTURE2D, DXGI_FORMAT_R8G8_UNORM);
	ID3D11ShaderResourceView* m_chrominance_shader_resource_view{ nullptr };
	hr = _d3d11_device->CreateShaderResourceView(texture, &chrominance_desc, &m_chrominance_shader_resource_view);
	if (FAILED(hr)) return false;

	//_d3d11_deviceCtx->PSSetShaderResources(0, 1, &m_luminance_shader_resource_view);
	_d3d11_deviceCtx->PSSetShaderResources(1, 1, &m_chrominance_shader_resource_view);

	IDXGIResource* dxgi_resource{ nullptr };
	hr = texture->QueryInterface(__uuidof(IDXGIResource), (void**)&dxgi_resource);
	if (FAILED(hr)) return false;

	HANDLE shared_handle{ nullptr };
	hr = dxgi_resource->GetSharedHandle(&shared_handle);
	if (FAILED(hr)) return false;

	hr = _d3d11_device->OpenSharedResource(shared_handle, __uuidof(ID3D11Texture2D), reinterpret_cast<void**>(&texture));
	if (FAILED(hr)) return false;

	AVFrame* temp_received = av_frame_alloc();

	ID3D11Texture2D* new_texture = (ID3D11Texture2D*)*temp_received->data[0];
	const int texture_index = temp_received->data[1][0];
	_d3d11_deviceCtx->CopySubresourceRegion(texture, 0, 0, 0, 0, new_texture, texture_index, nullptr);

	return true;
}

