#include "pch.h"
//#include "libav.h"
//#include "D3D11Header.h"
//
//#include "NV12ToRgbShader.h"
#include "Utils.h"

#include "MediaDecoder.h"


MediaDecoder::MediaDecoder(const AVCodec* codec, const ScrcpyNativeConfig& nativeConfig) {
	this->_codec = codec;
	this->_nativeConfig = nativeConfig;
	this->_hwType = (AVHWDeviceType)nativeConfig.HwType;
}

MediaDecoder::~MediaDecoder() {
	if (this->_d3d11_shader != NULL)
		delete this->_d3d11_shader;
	avcodec_close(_codec_ctx);
	avcodec_free_context(&_codec_ctx);
	if (this->_decoding_frame != NULL) av_frame_free(&_decoding_frame);
	if (this->_transfer_frame != NULL) av_frame_free(&_transfer_frame);
}


bool MediaDecoder::Init() {
	this->_codec_ctx = avcodec_alloc_context3(this->_codec);
	if (this->_codec_ctx == NULL)
		return false;

	this->_decoding_frame = av_frame_alloc();
	if (this->_decoding_frame == NULL)
		return false;

	this->_transfer_frame = av_frame_alloc();
	if (this->_transfer_frame == NULL)
		return false;

	if (!avcheck(avcodec_open2(this->_codec_ctx, this->_codec, nullptr))) {
		return false;
	}


	if (this->_hwType != AVHWDeviceType::AV_HWDEVICE_TYPE_NONE)
	{
		if (!avcheck(av_hwdevice_ctx_create(
			&_codec_ctx->hw_device_ctx,
			this->_hwType,
			nullptr,
			nullptr,
			0)))
			return false;

		switch (this->_hwType)
		{
		case AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA:
		{
			if (this->_nativeConfig.IsUseD3D11Shader) {
				AVHWDeviceContext* hw_device_ctx = reinterpret_cast<AVHWDeviceContext*>(this->_codec_ctx->hw_device_ctx->data);
				AVD3D11VADeviceContext* d3d11va_device_ctx = reinterpret_cast<AVD3D11VADeviceContext*>(hw_device_ctx->hwctx);

				this->m_d3d11_convert = new D3DImageConvert();
				if (this->m_d3d11_convert == nullptr)
					return false;

				if (!this->m_d3d11_convert->Initialize(d3d11va_device_ctx))
					return false;


				/*this->_d3d11_shader = new NV12ToRgbShader(d3d11va_device_ctx);
				if (this->_d3d11_shader == nullptr)
					return false;

				if (!this->_d3d11_shader->Init())
					return false;*/
			}
			break;
		}
		default:
			break;
		}
	}
	return true;
}

bool MediaDecoder::Decode(const AVPacket* packet, AVFrame* frame) {
	if (packet == nullptr || frame == nullptr)
		return false;
	if (!avcheck(avcodec_send_packet(_codec_ctx, packet)))
		return false;
	av_frame_unref(_decoding_frame);
	av_frame_unref(_transfer_frame);
	if (!avcheck(avcodec_receive_frame(_codec_ctx, _decoding_frame)))
		return false;
	if (_decoding_frame->hw_frames_ctx == nullptr)
	{
		return this->TransferNoHw(frame);
	}
	else
	{
		switch (this->_hwType)
		{
		case AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA:
		{
			if (this->m_d3d11_convert != nullptr) {
				bool result = this->m_d3d11_convert->Convert(_decoding_frame) && this->m_d3d11_convert->GetImage(_decoding_frame, frame);
				av_frame_unref(_decoding_frame);
				return result;
			}

			/*if (this->_d3d11_shader != nullptr) {
				bool result = this->_d3d11_shader->Convert(_decoding_frame, frame);
				av_frame_unref(_decoding_frame);
				return result;
			}*/
			return this->FFmpegTransfer(frame);
		}
		case AVHWDeviceType::AV_HWDEVICE_TYPE_CUDA://nvidia 
		case AVHWDeviceType::AV_HWDEVICE_TYPE_DXVA2://DirectX Video Acceleration by microsoft
		case AVHWDeviceType::AV_HWDEVICE_TYPE_VDPAU://nvidia api
		case AVHWDeviceType::AV_HWDEVICE_TYPE_VAAPI://by intel
		case AVHWDeviceType::AV_HWDEVICE_TYPE_OPENCL://by amd
		case AVHWDeviceType::AV_HWDEVICE_TYPE_VULKAN://gpu
		{
			return this->FFmpegTransfer(frame);
		}
		//case AVHWDeviceType::AV_HWDEVICE_TYPE_DRM://Digital rights management video
		//case AVHWDeviceType::AV_HWDEVICE_TYPE_MEDIACODEC://android
		//case AVHWDeviceType::AV_HWDEVICE_TYPE_VIDEOTOOLBOX://apple api

		case AVHWDeviceType::AV_HWDEVICE_TYPE_NONE://cpu
		case AVHWDeviceType::AV_HWDEVICE_TYPE_QSV://cpu
		default:
		{
			return this->TransferNoHw(frame);
		}
		}
	}
}

bool MediaDecoder::TransferNoHw(AVFrame* frame) {
	av_frame_move_ref(frame, _decoding_frame);
	return true;
}
bool MediaDecoder::FFmpegTransfer(AVFrame* frame) {
	bool result = avcheck(av_hwframe_transfer_data(_transfer_frame, _decoding_frame, 0));
	av_frame_unref(_decoding_frame);
	if (result)
		av_frame_move_ref(frame, _transfer_frame);
	return result;
}

bool MediaDecoder::DoRender(IUnknown* surface, bool isNewSurface) {
	if (this->_nativeConfig.IsUseD3D11Shader)
	{
		//return this->_d3d11_shader->DoRender(surface, isNewSurface);
	}
	return false;
}