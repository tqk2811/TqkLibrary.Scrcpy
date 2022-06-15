#include "pch.h"
#include "FrameConventer.h"
#include "Utils.h"
#define Align 16
FrameConventer::FrameConventer() {

}
FrameConventer::~FrameConventer() {

}
bool FrameConventer::Convert(AVFrame* src_frame, BYTE* buff, const int sizeInByte, const int w, const int h, const int lineSize) {
	//test
	//buff[sizeInByte - 1] = 0;//success
	//buff[sizeInByte] = 0;//access violon

	if (src_frame == nullptr || buff == nullptr || sizeInByte <= 0)
		return false;

	if (w <= 0 || w % 2 != 0 || h <= 0 || h % 2 != 0 || lineSize < 0 || GetArgbBufferSize(w, h, Align) > sizeInByte) {
		return false;
	}
	int err = 0;

	switch (src_frame->format)
	{
	case AV_PIX_FMT_BGRA:
	{
		if (src_frame->linesize[0] > lineSize)
		{
			return false;
		}
		else if (src_frame->linesize[0] == lineSize) {
			memcpy(buff, src_frame->data[0], sizeInByte);
			return true;
		}
		else// src_frame->linesize[0] < lineSize
		{
			for (UINT64 i = 0; i < h; i++)
			{
				uint8_t* dst = buff + i * lineSize;
				uint8_t* src = src_frame->data[0] + i * src_frame->linesize[0];
				memcpy(dst, src, src_frame->linesize[0]);
			}
			return true;
		}
		break;
	}
	case AV_PIX_FMT_NV12:
	case AV_PIX_FMT_YUV420P:
	default:
	{
		int linesizes[8]{ 0 };
		BYTE* const arr[1]{
			buff
		};

		int fix_w = w + w % 16;
		err = av_image_fill_linesizes(linesizes, AV_PIX_FMT_BGRA, fix_w);
		if (!avcheck(err) || linesizes[0] != lineSize)
		{
			return false;
		}

		int buffer_size = av_image_get_buffer_size(AV_PIX_FMT_BGRA, fix_w, h, Align);
		if (buffer_size > sizeInByte)
		{
			return false;
		}

		SwsContext* sws = sws_getContext(
			src_frame->width, src_frame->height, (AVPixelFormat)src_frame->format,
			w, h, AV_PIX_FMT_BGRA,
			SWS_FAST_BILINEAR, nullptr, nullptr, nullptr);

		if (sws == nullptr)
			return false;

		int sws_h = sws_scale(sws, src_frame->data, src_frame->linesize, 0, src_frame->height, arr, linesizes);

		bool result = avcheck(sws_h);

		sws_freeContext(sws);

		return result;
	}
	}
	return false;
}