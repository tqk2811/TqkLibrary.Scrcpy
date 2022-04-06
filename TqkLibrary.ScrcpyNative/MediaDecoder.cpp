#include "pch.h"
#include "libav.h"
#include "MediaDecoder.h"
#include "NV12ToRgbShader.h"

MediaDecoder::MediaDecoder(const AVCodec* codec, AVHWDeviceType type) {
	this->_codec = codec;
	this->_hwType = type;
}

MediaDecoder::~MediaDecoder() {
	avcodec_close(_codec_ctx);
	avcodec_free_context(&_codec_ctx);
	av_frame_free(&_decoding_frame);
	av_frame_free(&_transfer_frame);
	if (this->_d3d11Shader != nullptr)
		delete this->_d3d11Shader;
}


bool MediaDecoder::Init() {
	this->_codec_ctx = avcodec_alloc_context3(this->_codec);
	if (this->_codec_ctx == NULL)
		return false;

	this->_decoding_frame = av_frame_alloc();
	if (this->_decoding_frame == NULL)
		return false;

	int err = avcodec_open2(this->_codec_ctx, this->_codec, nullptr);
	if (err < 0)
		return false;

	if (this->_hwType != AVHWDeviceType::AV_HWDEVICE_TYPE_NONE)
	{
		if (!avcheck(av_hwdevice_ctx_create(
			&_codec_ctx->hw_device_ctx,
			this->_hwType,
			nullptr,
			nullptr,
			0))) return false;

		_transfer_frame = av_frame_alloc();
		if (_transfer_frame == NULL) return false;

		switch (this->_hwType) 
		{
		case AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA:
		{
			this->_d3d11Shader = new NV12ToRgbShader(GetHWDeviceContext());
			break;
		}
		default: break;
		}
	}
	return true;
}

AVHWDeviceContext* MediaDecoder::GetHWDeviceContext() {
	if (this->_hwType != AVHWDeviceType::AV_HWDEVICE_TYPE_NONE)
		return (AVHWDeviceContext*)_codec_ctx->hw_device_ctx->data;
	else return NULL;
}

bool MediaDecoder::Decode(const AVPacket* packet, AVFrame** frame) {
	if (!avcheck(avcodec_send_packet(_codec_ctx, packet))) return false;
	av_frame_unref(_decoding_frame);
	av_frame_unref(_transfer_frame);
	if (!avcheck(avcodec_receive_frame(_codec_ctx, _decoding_frame))) return false;

	switch (_hwType)
	{
	case AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA:
	{
		return this->_d3d11Shader->Convert(_decoding_frame, frame);
	}
	case AVHWDeviceType::AV_HWDEVICE_TYPE_CUDA:
	case AVHWDeviceType::AV_HWDEVICE_TYPE_DXVA2:
		if (!avcheck(av_hwframe_transfer_data(_transfer_frame, _decoding_frame, 0))) return false;
		*frame = av_frame_clone(_transfer_frame);
		return *frame != nullptr;

	case AVHWDeviceType::AV_HWDEVICE_TYPE_NONE:
	case AVHWDeviceType::AV_HWDEVICE_TYPE_QSV:
	default: // not tested
		*frame = av_frame_clone(_decoding_frame);
		return *frame != nullptr;
	}
}