#include "pch.h"
#include <d3d11.h>
#include "NV12ToRgbShader.h"
//https://gist.github.com/RomiTT/9c05d36fe339b899793a3252297a5624#file-yuv_bt2020_to_rgb-hlsl
const char* shaderStr = 
"cbuffer PS_CONSTANT_BUFFER : register(b0)\
{\
	float Opacity;\
	float ignoreA;\
	float ignoreB;\
	float ignoreC;\
};\
\
Texture2D shaderTextureY;\
Texture2D shaderTextureUV;\
SamplerState SampleType;\
\
struct PS_INPUT\
{\
	float4 Position   : SV_POSITION;\
	float2 Texture    : TEXCOORD0;\
};\
\
float4 PS(PS_INPUT In) : SV_TARGET\
{\
	float3 yuv;\
	float4 rgba;\
	yuv.x = shaderTextureY.Sample(SampleType, In.Texture).x;\
	yuv.yz = shaderTextureUV.Sample(SampleType, In.Texture).xy;\
	yuv.x = 1.164383561643836 * (yuv.x - 0.0625);\
	yuv.y = yuv.y - 0.5;\
	yuv.z = yuv.z - 0.5;\
	rgba.x = yuv.x + 1.792741071428571 * yuv.z;\
	rgba.y = yuv.x - 0.532909328559444 * yuv.z - 0.21324861427373 * yuv.y;\
	rgba.z = yuv.x + 2.112401785714286 * yuv.y;\
	rgba.x = saturate(1.661 * rgba.x - 0.588 * rgba.y - 0.073 * rgba.z);\
	rgba.y = saturate(-0.125 * rgba.x + 1.133 * rgba.y - 0.008 * rgba.z);\
	rgba.z = saturate(-0.018 * rgba.x - 0.101 * rgba.y + 1.119 * rgba.z);\
	rgba.a = Opacity;\
	return rgba;\
}";


NV12ToRgbShader::NV12ToRgbShader() {
	
}

NV12ToRgbShader::~NV12ToRgbShader() {

}

bool NV12ToRgbShader::Init() {
	if (!InitShader()) return false;


}

bool NV12ToRgbShader::Convert(const AVFrame* frame, BYTE* buff, int buffSize) {
	if (frame == NULL) return false;
	if (buff == NULL) return false;
	if (frame->format != AV_PIX_FMT_NV12) return false;
	if (!frame->hw_frames_ctx) return false;

	//For hwaccel-format frames, this should be a reference to the AVHWFramesContext describing the frame.
	AVHWFramesContext* frameCtx = (AVHWFramesContext*)frame->hw_frames_ctx->data;
	if (!frameCtx->device_ctx || frameCtx->device_ctx->type != AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA) return false;

	//This struct is allocated as AVHWDeviceContext.hwctx
	AVD3D11VADeviceContext* deviceCtx = (AVD3D11VADeviceContext*)frameCtx->device_ctx->hwctx;

	//This struct is allocated as AVHWFramesContext.hwctx
	AVD3D11VAFramesContext* d3dFrameCtx = (AVD3D11VAFramesContext*)frameCtx->hwctx;
	ID3D11Texture2D* texture = d3dFrameCtx->texture;
	D3D11_TEXTURE2D_DESC textureDesc{ 0 };
	texture->GetDesc(&textureDesc);
	
	ID3D11PixelShader* shader = NULL;
	HRESULT hr = deviceCtx->device->CreatePixelShader(shaderStr, strlen(shaderStr), NULL, &shader);
	if (!SUCCEEDED(hr)) return false;

	//textureDesc.
	//shader->SetPrivateData()







	return true;
}


bool NV12ToRgbShader::InitShader() {

}