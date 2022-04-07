#include "pch.h"
#include "libav.h"
#include "MediaDecoder.h"
#include "NV12ToRgbShader.h"
#include "Utils.h"

MediaDecoder::MediaDecoder(const AVCodec* codec, AVHWDeviceType type) {
	this->_codec = codec;
	this->_hwType = type;
}

MediaDecoder::~MediaDecoder() {
	avcodec_close(_codec_ctx);
	avcodec_free_context(&_codec_ctx);
	av_frame_free(&_decoding_frame);
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
			0))) 
			return false;
	}
	return true;
}

bool MediaDecoder::Decode(const AVPacket* packet, AVFrame** frame) {
	if (!avcheck(avcodec_send_packet(_codec_ctx, packet))) 
		return false;
	av_frame_unref(_decoding_frame);
	if (!avcheck(avcodec_receive_frame(_codec_ctx, _decoding_frame))) 
		return false;
	*frame = av_frame_clone(_decoding_frame);
	return true;
}