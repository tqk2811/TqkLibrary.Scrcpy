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

	avcodec_close(_codec_ctx);
	avcodec_free_context(&_codec_ctx);
	if (this->_decoding_frame != NULL) av_frame_free(&_decoding_frame);
	if (this->m_d3d11 != nullptr) delete this->m_d3d11;
	if (this->m_d3d11_input != nullptr) delete this->m_d3d11_input;
}


bool MediaDecoder::Init() {
	this->_codec_ctx = avcodec_alloc_context3(this->_codec);
	if (this->_codec_ctx == NULL)
		return false;

	this->_decoding_frame = av_frame_alloc();
	if (this->_decoding_frame == NULL)
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

				this->m_d3d11 = new D3DClass();
				if (!this->m_d3d11->Initialize(d3d11va_device_ctx))
					return false;
				this->m_d3d11_input = new InputTextureClass();

				this->m_d3d11_convert = new D3DImageConvert();
				if (!this->m_d3d11_convert->Initialize(this->m_d3d11))
					return false;
			}
			break;
		}
		default:
			break;
		}
	}
	return true;
}

bool MediaDecoder::Decode(const AVPacket* packet) {
	if (packet == nullptr)
		return false;

	bool result = false;

	if (avcheck(avcodec_send_packet(_codec_ctx, packet)))
	{
		_mtx_frame.lock();

		av_frame_unref(_decoding_frame);
		result = avcheck(avcodec_receive_frame(_codec_ctx, _decoding_frame));

		_mtx_frame.unlock();

		if (result && this->m_d3d11_input != nullptr)
		{
			_mtx_texture.lock();

#if _DEBUG
			auto start(std::chrono::high_resolution_clock::now());
#endif

			if (this->m_d3d11_input->Initialize(this->m_d3d11->GetDevice(), _decoding_frame->width, _decoding_frame->height))
			{
				this->m_d3d11_input->Copy(this->m_d3d11->GetDeviceContext(), _decoding_frame);
			}
			_mtx_texture.unlock();

#if _DEBUG
			auto finish(std::chrono::high_resolution_clock::now());
			auto r = std::chrono::duration_cast<std::chrono::milliseconds>(finish - start);
			std::wstring text(L"Copy to Texture: ");
			text.append(std::to_wstring(r.count()));
			wprintf(text.c_str());
#endif
		}
	}

	return result;
}

bool MediaDecoder::Convert(AVFrame* frame) {
	bool result = false;

	_mtx_frame.lock();

	if (_decoding_frame->hw_frames_ctx == nullptr)
	{
		if (this->m_d3d11_convert != nullptr &&
			_decoding_frame->format == AVPixelFormat::AV_PIX_FMT_YUV420P)
		{
			_mtx_frame.unlock();


			_mtx_texture.lock();

			result =
				this->m_d3d11_convert->Convert(this->m_d3d11, this->m_d3d11_input, _decoding_frame) &&
				this->m_d3d11_convert->GetImage(this->m_d3d11, _decoding_frame, frame);

			_mtx_texture.unlock();
		}
		else
		{
			result = this->TransferNoHw(frame);
			_mtx_frame.unlock();
		}
	}
	else
	{
		switch (this->_hwType)
		{
		case AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA:
		{
			if (this->m_d3d11_convert != nullptr) {

				_mtx_texture.lock();
				result =
					this->m_d3d11_convert->Convert(this->m_d3d11, this->m_d3d11_input, _decoding_frame) &&
					this->m_d3d11_convert->GetImage(this->m_d3d11, _decoding_frame, frame);
				_mtx_texture.unlock();
			}
			else
			{
				result = this->FFmpegTransfer(frame);
			}
			break;
		}
		case AVHWDeviceType::AV_HWDEVICE_TYPE_CUDA://nvidia 
		case AVHWDeviceType::AV_HWDEVICE_TYPE_DXVA2://DirectX Video Acceleration by microsoft
		case AVHWDeviceType::AV_HWDEVICE_TYPE_VDPAU://nvidia api
		case AVHWDeviceType::AV_HWDEVICE_TYPE_VAAPI://by intel
		case AVHWDeviceType::AV_HWDEVICE_TYPE_OPENCL://by amd
		case AVHWDeviceType::AV_HWDEVICE_TYPE_VULKAN://gpu
		{
			result = this->FFmpegTransfer(frame);
			break;
		}
		//case AVHWDeviceType::AV_HWDEVICE_TYPE_DRM://Digital rights management video
		//case AVHWDeviceType::AV_HWDEVICE_TYPE_MEDIACODEC://android
		//case AVHWDeviceType::AV_HWDEVICE_TYPE_VIDEOTOOLBOX://apple api

		case AVHWDeviceType::AV_HWDEVICE_TYPE_NONE://cpu
		case AVHWDeviceType::AV_HWDEVICE_TYPE_QSV://cpu
		default:
		{
			result = this->TransferNoHw(frame);
			break;
		}
		}

		_mtx_frame.unlock();
	}

	return result;
}

bool MediaDecoder::IsNewFrame(INT64& pts) {
	_mtx_frame.lock();

	bool result = _decoding_frame->pts > pts;
	if (result) pts = _decoding_frame->pts;

	_mtx_frame.unlock();

	return result;
}

bool MediaDecoder::GetFrameSize(int& w, int& h) {
	if (_decoding_frame == nullptr)
		return false;

	_mtx_frame.lock();

	w = _decoding_frame->width;
	h = _decoding_frame->height;

	_mtx_frame.unlock();

	return true;
}

bool MediaDecoder::TransferNoHw(AVFrame* frame) {
	av_frame_move_ref(frame, _decoding_frame);
	return true;
}

bool MediaDecoder::FFmpegTransfer(AVFrame* frame) {
	av_frame_unref(frame);
	bool result = avcheck(av_hwframe_transfer_data(frame, _decoding_frame, 0));
	return result;
}

bool MediaDecoder::Draw(D3DImageView* view, IUnknown* surface, bool isNewSurface) {
	assert(view != nullptr);

	bool result = false;

	if (this->_nativeConfig.IsUseD3D11Shader &&
		this->m_d3d11 != nullptr &&
		this->m_d3d11_convert != nullptr)
	{
		_mtx_texture.lock();

		result = view->Draw(
			this->m_d3d11,
			this->m_d3d11_input,
			this->_decoding_frame,
			surface,
			isNewSurface);

		_mtx_texture.unlock();
	}


	return result;
}