#include "pch.h"
#include "Scrcpy_pch.h"

#define DeleteHeap(v) if(v != nullptr) { delete v; v = nullptr; } 

AudioDecoder::AudioDecoder(const AVCodec* codec, const ScrcpyNativeConfig& nativeConfig) {
	this->_codec = codec;
	this->_nativeConfig = nativeConfig;
}

AudioDecoder::~AudioDecoder() {
	avcodec_close(_codec_ctx);
	avcodec_free_context(&_codec_ctx);
	if (this->_decoding_frame != NULL) av_frame_free(&_decoding_frame);
}

bool AudioDecoder::Init() {
	this->_codec_ctx = avcodec_alloc_context3(this->_codec);
	if (this->_codec_ctx == NULL)
		return FALSE;

	this->_codec_ctx->flags |= AV_CODEC_FLAG_LOW_DELAY;
	this->_codec_ctx->ch_layout.nb_channels = 2;
	this->_codec_ctx->sample_rate = 48000;

	this->_decoding_frame = av_frame_alloc();
	if (this->_decoding_frame == NULL)
		return FALSE;

	if (!avcheck(avcodec_open2(this->_codec_ctx, this->_codec, nullptr))) {
		return FALSE;
	}
	return TRUE;
}

bool AudioDecoder::Decode(const AVPacket* packet) {
	if (packet == nullptr || this->_decoding_frame == nullptr)
		return false;

	bool result = false;

#if _DEBUG
	auto start(std::chrono::high_resolution_clock::now());
	INT64 pts = 0;
#endif
	if (avcheck(avcodec_send_packet(_codec_ctx, packet)))
	{
		_mtx_frame.lock();//lock read frame

		av_frame_unref(_decoding_frame);
		result = avcheck(avcodec_receive_frame(_codec_ctx, _decoding_frame));

#if _DEBUG
		pts = _decoding_frame->pts;
#endif
		_mtx_frame.unlock();
	}
#if _DEBUG
	auto finish(std::chrono::high_resolution_clock::now());
	auto r = std::chrono::duration_cast<std::chrono::microseconds>(finish - start);
	std::wstring text(L"AudioDecoder: ");
	text.append(std::to_wstring(r.count()));
	text.append(L"us, pts: ");
	text.append(std::to_wstring(pts));
	text.append(L"\r");
	OutputDebugString(text.c_str());
#endif

	return result;
}

INT64 AudioDecoder::ReadAudioFrame(AVFrame* pFrame, INT64 last_pts)
{
	if (this->_decoding_frame == nullptr || pFrame == nullptr)
		return -1;

	av_frame_unref(pFrame);
	
	INT64 result = -1;
	_mtx_frame.lock();//lock read frame
	if (_decoding_frame->pts > last_pts)
	{
		av_frame_copy_props(pFrame, _decoding_frame);
		av_frame_ref(pFrame, _decoding_frame);
		result = _decoding_frame->pts;
	}
	_mtx_frame.unlock();
	return result;
}