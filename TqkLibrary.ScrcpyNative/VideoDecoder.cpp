#include "pch.h"
#include "Scrcpy_pch.h"

#define DeleteHeap(v) if(v != nullptr) { delete v; v = nullptr; } 


VideoDecoder::VideoDecoder(const AVCodec* codec, const ScrcpyNativeConfig& nativeConfig) {
	this->_codec = codec;
	this->_nativeConfig = nativeConfig;
	this->_hwType = (AVHWDeviceType)nativeConfig.HwType;
}

VideoDecoder::~VideoDecoder() {

	avcodec_close(_codec_ctx);
	avcodec_free_context(&_codec_ctx);
	if (this->_decoding_frame != NULL) av_frame_free(&_decoding_frame);
	DeleteHeap(this->m_vertex);
	DeleteHeap(this->m_d3d11_inputNv12);
	DeleteHeap(this->m_d3d11_pixel_Nv12ToRgba);
	DeleteHeap(this->m_d3d11_pixel_Nv12ToBgra);
	DeleteHeap(this->m_d3d11_renderTexture);
	DeleteHeap(this->m_d3d11);
}


bool VideoDecoder::Init() {
	this->_codec_ctx = avcodec_alloc_context3(this->_codec);
	if (this->_codec_ctx == NULL)
		return FALSE;

	this->_decoding_frame = av_frame_alloc();
	if (this->_decoding_frame == NULL)
		return FALSE;

	if (!avcheck(avcodec_open2(this->_codec_ctx, this->_codec, nullptr))) {
		return FALSE;
	}


	if (this->_hwType != AVHWDeviceType::AV_HWDEVICE_TYPE_NONE)
	{
		if (!avcheck(av_hwdevice_ctx_create(
			&_codec_ctx->hw_device_ctx,
			this->_hwType,
			nullptr,
			nullptr,
			0)))
			return FALSE;
	}

	if (this->_nativeConfig.IsUseD3D11ForUiRender)
	{
		switch (this->_hwType)
		{
		case AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA:
		{
			AVHWDeviceContext* hw_device_ctx = reinterpret_cast<AVHWDeviceContext*>(this->_codec_ctx->hw_device_ctx->data);
			AVD3D11VADeviceContext* d3d11va_device_ctx = reinterpret_cast<AVD3D11VADeviceContext*>(hw_device_ctx->hwctx);

			this->m_d3d11 = new D3DClass();
			if (!this->m_d3d11->Initialize(d3d11va_device_ctx))
				return false;

			this->m_vertex = new VertexShaderClass();
			if (!this->m_vertex->Initialize(this->m_d3d11->GetDevice()))
				return false;

			m_d3d11_inputNv12 = new InputTextureNv12Class();
			m_d3d11_pixel_Nv12ToRgba = new PixelShaderNv12ToRgbaClass();
			m_d3d11_pixel_Nv12ToBgra = new PixelShaderNv12ToBgraClass();
			m_d3d11_renderTexture = new RenderTextureClass();

			break;
		}
		case AVHWDeviceType::AV_HWDEVICE_TYPE_NONE:
		{
			this->m_d3d11 = new D3DClass();
			if (!this->m_d3d11->Initialize())
				return false;

			this->m_vertex = new VertexShaderClass();
			if (!this->m_vertex->Initialize(this->m_d3d11->GetDevice()))
				return false;

			m_d3d11_inputNv12 = new InputTextureNv12Class();
			m_d3d11_pixel_Nv12ToRgba = new PixelShaderNv12ToRgbaClass();
			m_d3d11_pixel_Nv12ToBgra = new PixelShaderNv12ToBgraClass();
			m_d3d11_renderTexture = new RenderTextureClass();
			break;
		}
		default:
			return FALSE;
		}
	}

	return TRUE;
}

bool VideoDecoder::Decode(const AVPacket* packet) {
	if (packet == nullptr)
		return false;

	bool result = false;

#if _DEBUG
	auto start(std::chrono::high_resolution_clock::now());
#endif
	if (avcheck(avcodec_send_packet(_codec_ctx, packet)))
	{
		_mtx_frame.lock();//lock read frame
		av_frame_unref(_decoding_frame);
		result = avcheck(avcodec_receive_frame(_codec_ctx, _decoding_frame));

		if (result)
		{
			if (_nativeConfig.IsUseD3D11ForUiRender &&
				((_decoding_frame->format == AV_PIX_FMT_D3D11 && _decoding_frame->hw_frames_ctx != nullptr) ||
					_decoding_frame->format == AV_PIX_FMT_YUV420P))//on AV_PIX_FMT_D3D11 false or AV_HWDEVICE_TYPE_NONE
			{
				if (this->m_d3d11_inputNv12->Initialize(this->m_d3d11->GetDevice(), _decoding_frame->width, _decoding_frame->height))
				{
					result = this->m_d3d11_inputNv12->Copy(this->m_d3d11->GetDeviceContext(), _decoding_frame);
				}
			}
		}
		_mtx_frame.unlock();
	}
#if _DEBUG
	auto finish(std::chrono::high_resolution_clock::now());
	auto r = std::chrono::duration_cast<std::chrono::microseconds>(finish - start);
	std::wstring text(L"VideoDecoder: ");
	text.append(std::to_wstring(r.count()));
	text.append(L"us");
	text.append(L"\r");
	OutputDebugString(text.c_str());
#endif

	return result;
}

bool VideoDecoder::Convert(AVFrame* frame) {
	bool result = false;

	_mtx_frame.lock();

	if (_decoding_frame->hw_frames_ctx == nullptr)//HW failed
	{
		if (_decoding_frame->format == AVPixelFormat::AV_PIX_FMT_YUV420P && _nativeConfig.IsUseD3D11ForUiRender)// -> m_d3d11_inputYv12
		{
			if (_nativeConfig.IsUseD3D11ForConvert)
			{
				result = Nv12Convert(frame);
			}
			else 
			{
				result = this->TransferNoHw(frame);
			}
		}
		else//other hw
		{
			result = this->TransferNoHw(frame);
		}
	}
	else//HW success
	{
		switch (this->_hwType)
		{
		case AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA:
		{
			result = Nv12Convert(frame);
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
	}

	_mtx_frame.unlock();
	return result;
}

bool VideoDecoder::Nv12Convert(AVFrame* frame) {
	ComPtr<ID3D11DeviceContext> device_ctx = this->m_d3d11->GetDeviceContext();
	ComPtr<ID3D11Device> device = this->m_d3d11->GetDevice();

	if (this->m_d3d11_renderTexture->Initialize(device.Get(), this->m_d3d11_inputNv12->Width(), this->m_d3d11_inputNv12->Height()) &&
		this->m_d3d11_pixel_Nv12ToRgba->Initialize(device.Get(), this->_nativeConfig.Filter))
	{
		device_ctx->ClearState();

		device_ctx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

		this->m_vertex->Set(device_ctx.Get());

		this->m_d3d11_pixel_Nv12ToRgba->Set(
			device_ctx.Get(),
			this->m_d3d11_inputNv12->GetLuminanceView(),
			this->m_d3d11_inputNv12->GetChrominanceView());

		this->m_d3d11_renderTexture->SetRenderTarget(device_ctx.Get(), nullptr);
		this->m_d3d11_renderTexture->SetViewPort(device_ctx.Get(), this->m_d3d11_inputNv12->Width(), this->m_d3d11_inputNv12->Height());

		/*static FLOAT blendFactor[4] = { 0.f, 0.f, 0.f, 0.f };
		device_ctx->OMSetBlendState(nullptr, blendFactor, 0xffffffff);*/

		D3D_FEATURE_LEVEL feature_level = device->GetFeatureLevel();
		if (feature_level >= D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_10_0)
		{
			UINT x = (UINT)ceil(static_cast<FLOAT>(this->m_d3d11_inputNv12->Width()) / 8);
			UINT y = (UINT)ceil(static_cast<FLOAT>(this->m_d3d11_inputNv12->Height()) / 8);
			UINT z = 1;
			device_ctx->Dispatch(x, y, z);
		}

		this->m_d3d11_renderTexture->ClearRenderTarget(device_ctx.Get(), nullptr, 0, 0, 0, 0);
		device_ctx->Draw(this->m_vertex->GetVertexCount(), 0);

		device_ctx->Flush();
		return this->m_d3d11_renderTexture->GetImage(device_ctx.Get(), _decoding_frame, frame);
	}
	return false;
}

bool VideoDecoder::IsNewFrame(INT64& pts) {
	_mtx_frame.lock();

	bool result = _decoding_frame->pts > pts;
	if (result) pts = _decoding_frame->pts;

	_mtx_frame.unlock();

	return result;
}

bool VideoDecoder::GetFrameSize(int& w, int& h) {
	if (_decoding_frame == nullptr)
		return false;

	_mtx_frame.lock();

	w = _decoding_frame->width;
	h = _decoding_frame->height;

	_mtx_frame.unlock();

	return true;
}

bool VideoDecoder::TransferNoHw(AVFrame* frame) {
	av_frame_ref(frame, _decoding_frame);
	return true;
}

bool VideoDecoder::FFmpegTransfer(AVFrame* frame) {
	av_frame_unref(frame);
	bool result = avcheck(av_hwframe_transfer_data(frame, _decoding_frame, 0));
	return result;
}

bool VideoDecoder::Draw(RenderTextureSurfaceClass* renderSurface, IUnknown* surface, bool isNewSurface, bool& isNewtargetView) {
	assert(renderSurface != nullptr);

	bool result = false;

	if (this->_nativeConfig.IsUseD3D11ForUiRender)
	{
#if _DEBUG
		auto start(std::chrono::high_resolution_clock::now());
#endif
		_mtx_frame.lock();

		ComPtr<ID3D11DeviceContext> device_ctx = this->m_d3d11->GetDeviceContext();
		ComPtr<ID3D11Device> device = this->m_d3d11->GetDevice();

		if (renderSurface->Initialize(device.Get(), surface, isNewSurface, isNewtargetView) &&
			this->m_d3d11_pixel_Nv12ToBgra->Initialize(device.Get(), this->_nativeConfig.Filter) &&
			this->m_vertex->Initialize(device.Get()))
		{
			bool isNewFrame = renderSurface->IsNewFrame(_decoding_frame);

			if (isNewFrame || isNewSurface)
			{
				device_ctx->ClearState();

				device_ctx->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

				this->m_vertex->Set(device_ctx.Get());

				this->m_d3d11_pixel_Nv12ToBgra->Set(
					device_ctx.Get(),
					this->m_d3d11_inputNv12->GetLuminanceView(),
					this->m_d3d11_inputNv12->GetChrominanceView());

				renderSurface->SetRenderTarget(device_ctx.Get(), nullptr);
				renderSurface->SetViewPort(device_ctx.Get());

				D3D_FEATURE_LEVEL feature_level = device->GetFeatureLevel();
				if (feature_level >= D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_10_0)
				{
					UINT x = (UINT)ceil(static_cast<FLOAT>(renderSurface->Width()) / 8);
					UINT y = (UINT)ceil(static_cast<FLOAT>(renderSurface->Height()) / 8);
					UINT z = 1;
					device_ctx->Dispatch(x, y, z);
				}

				//view->m_renderTextureSurface.ClearRenderTarget(device_ctx.Get(), nullptr, 0, 0, 0, 0);
				device_ctx->Draw(this->m_vertex->GetVertexCount(), 0);
				device_ctx->Flush();
			}
			result = true;
		}
		_mtx_frame.unlock();

#if _DEBUG
		auto finish(std::chrono::high_resolution_clock::now());
		auto r = std::chrono::duration_cast<std::chrono::microseconds>(finish - start);
		std::wstring text(L"Draw surface: ");
		text.append(std::to_wstring(r.count()));
		text.append(L"us");
		text.append(L"\r");
		OutputDebugString(text.c_str());
#endif
	}

	return result;
}