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
	default:
	{
		int linesizes[4]{ 0 };
		BYTE* const arr[1]{
			buff
		};

		if (!avcheck(av_image_fill_linesizes(linesizes, AV_PIX_FMT_BGRA, w))) {
			return false;
		}
		
		if (IsCudaSupport) {
			
		}


		SwsContext* sws = sws_getContext(
			frame->width, frame->height, (AVPixelFormat)frame->format,
			w, h, AV_PIX_FMT_BGRA,
			SWS_FAST_BILINEAR, nullptr, nullptr, nullptr);

		if (sws == nullptr)
			return false;

		bool result = avcheck(sws_scale(sws, frame->data, frame->linesize, 0, frame->height, arr, linesizes));
		
		sws_freeContext(sws);
		
		return result;
	}
	}

	return false;
}