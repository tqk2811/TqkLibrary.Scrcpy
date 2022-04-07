#include "pch.h"
#include "FrameConventer.h"
#include "Utils.h"

FrameConventer::FrameConventer() {

}
FrameConventer::~FrameConventer() {

}
bool FrameConventer::Convert(AVFrame* frame, BYTE* buff, const int sizeInByte, int w, int h, int lineSize) {
	if (frame == nullptr || buff == nullptr || sizeInByte <= 0)
		return false;

	if (w <= 0 || w % 2 != 0 || h <= 0 || h % 2 != 0 || lineSize < 0 || GetArgbBufferSize(w, h) != sizeInByte) {
		return false;
	}
	int err = 0;


	if (frame->hw_frames_ctx != nullptr) {
		//transfer from gpu to cpu
		AVHWFramesContext* hw_frames_ctx = (AVHWFramesContext*)frame->hw_frames_ctx->data;
		AVHWDeviceContext* hw_device_ctx = hw_frames_ctx->device_ctx;
		switch ((AVPixelFormat)hw_device_ctx->type)
		{
		//gpu hw
			
		case AVHWDeviceType::AV_HWDEVICE_TYPE_D3D11VA:
		{
			//NV12ToRgbShader shader(hw_device_ctx);
		}
		case AVHWDeviceType::AV_HWDEVICE_TYPE_CUDA:
		case AVHWDeviceType::AV_HWDEVICE_TYPE_DXVA2:
		{
			AVFrame transfer_frame{ 0 };
			err = av_hwframe_transfer_data(&transfer_frame, frame, 0);
			if (err < 0) {
				return false;
			}
			av_frame_unref(frame);
			av_frame_move_ref(frame, &transfer_frame);
			break;
		}
		
		//cpu hw
		case AVHWDeviceType::AV_HWDEVICE_TYPE_NONE:
		case AVHWDeviceType::AV_HWDEVICE_TYPE_QSV:
		default:
			break;
		}
	}


	switch (frame->format)
	{
	case AV_PIX_FMT_BGRA:
	{
		if (frame->linesize[0] == lineSize) {
			memcpy(buff, frame->data[0], sizeInByte);
			return true;
		}
		break;
	}
	case AV_PIX_FMT_YUV420P:
	case AV_PIX_FMT_NV12:
	{
		int linesizes[4]{ 0 };
		BYTE* const arr[1]{
			buff
		};

		err = av_image_fill_linesizes(linesizes, AV_PIX_FMT_BGRA, w);
		if (err < 0)
			return false;

		SwsContext* sws = sws_getContext(
			frame->width, frame->height, (AVPixelFormat)frame->format,
			w, h, AV_PIX_FMT_BGRA,
			SWS_FAST_BILINEAR, nullptr, nullptr, nullptr);

		if (sws == nullptr)
			return false;

		err = sws_scale(sws, frame->data, frame->linesize, 0, frame->height, arr, linesizes);
		sws_freeContext(sws);
		return err >= 0;
	}
	default:
		break;
	}

	return false;
}