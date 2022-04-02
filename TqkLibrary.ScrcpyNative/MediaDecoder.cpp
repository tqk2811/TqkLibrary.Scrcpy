#include "pch.h"
#include "libav.h"
#include "MediaDecoder.h"

MediaDecoder::MediaDecoder(const AVCodec* codec, AVHWDeviceType type) {
	this->_codec = codec;
	this->_hwType = type;
	_codec_ctx = avcodec_alloc_context3(codec);
	assert(_codec_ctx != nullptr);
	if (type != AVHWDeviceType::AV_HWDEVICE_TYPE_NONE)
	{
		assert(av_hwdevice_ctx_create(&_codec_ctx->hw_device_ctx, type, nullptr, nullptr, 0) >= 0);
		_transfer_frame = av_frame_alloc();
		assert(_transfer_frame != nullptr);
	}
	assert(avcodec_open2(_codec_ctx, codec, nullptr) >= 0);
}

MediaDecoder::~MediaDecoder() {
	avcodec_close(_codec_ctx);
	avcodec_free_context(&_codec_ctx);
	av_frame_free(&_decoding_frame);
	av_frame_free(&_transfer_frame);
}

AVFrame* MediaDecoder::Decode(const AVPacket* packet) {
	assert(avcodec_send_packet(_codec_ctx, packet) >= 0);
	av_frame_unref(_decoding_frame);
	av_frame_unref(_transfer_frame);
	assert(avcodec_receive_frame(_codec_ctx, _decoding_frame) >= 0);

	switch (_hwType)
	{
	case AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA:
	case AVHWDeviceType::AV_HWDEVICE_TYPE_CUDA:
	case AVHWDeviceType::AV_HWDEVICE_TYPE_DXVA2:
		assert(av_hwframe_transfer_data(_transfer_frame, _decoding_frame, 0) >= 0);
		return av_frame_clone(_transfer_frame);

	case AVHWDeviceType::AV_HWDEVICE_TYPE_NONE:
	case AVHWDeviceType::AV_HWDEVICE_TYPE_QSV:
		return av_frame_clone(_decoding_frame);

	default: return av_frame_clone(_decoding_frame);// not tested
	}
}