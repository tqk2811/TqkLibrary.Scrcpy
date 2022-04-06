#include "pch.h"
#include "Video.h"
#include "libav.h"
#include "ParsePacket.h"
#include "MediaDecoder.h"
#define HEADER_SIZE 12
#define DEVICE_NAME_SIZE 64
#define NO_PTS UINT64_MAX
Video::Video(SOCKET sock, int buffSize, AVHWDeviceType hwType) {
	assert(buffSize > 0);
	this->_buffSize = buffSize;
	this->_videoSock = new SocketWrapper(sock);
	this->_videoBuffer = new BYTE[buffSize];
	const AVCodec* h264_decoder = avcodec_find_decoder(AV_CODEC_ID_H264);
	this->_parsePacket = new ParsePacket(h264_decoder);
	this->_h264_mediaDecoder = new MediaDecoder(h264_decoder, hwType);
}

Video::~Video() {
	delete this->_parsePacket;
	delete this->_h264_mediaDecoder;
	delete this->_videoSock;
	delete this->_videoBuffer;
	av_frame_unref(this->_tempFrame);
	av_frame_free(&this->_tempFrame);
	CloseHandle(this->_threadHandle);
}

void Video::Start() {
	//
	this->_threadHandle =
		CreateThread(
			NULL,                   // default security attributes
			0,                      // use default stack size  
			MyThreadFunction,       // thread function name
			this,					// argument to thread function 
			0,                      // use default creation flags 
			&_threadId);
}

void Video::Stop() {
	this->_videoSock->Stop();
	this->_isStop = true;
	if(this->_threadHandle != INVALID_HANDLE_VALUE) 
		WaitForSingleObject(this->_threadHandle, INFINITE);
}

bool Video::Init() {
	if (!this->_parsePacket->Init())
		return false;

	if (!this->_h264_mediaDecoder->Init())
		return false;
	
	return true;
}

DWORD WINAPI Video::MyThreadFunction(LPVOID lpParam) {
	((Video*)lpParam)->threadStart();
	return 0;
}

void Video::threadStart() {
	if (this->_videoSock->ReadAll(this->_videoBuffer, DEVICE_NAME_SIZE) != DEVICE_NAME_SIZE)//device name
		return;
	this->_deviceName.append(std::string((const char*)this->_videoBuffer, 64));


#if _DEBUG
	if (this->_videoSock->ReadAll(this->_videoBuffer, 2) != 2)//width
		return;
	int width = sc_read16be(this->_videoBuffer);

	if (this->_videoSock->ReadAll(this->_videoBuffer, 2) != 2)//height
		return;
	int height = sc_read16be(this->_videoBuffer);


	printf(this->_deviceName.c_str());
	printf("\r\n");
	printf(std::string("width:").append(std::to_string(width)).append("\r\n").c_str());
	printf(std::string("height:").append(std::to_string(height)).append("\r\n").c_str());
#else
	if (this->_videoSock->ReadAll(this->_videoBuffer, 2) != 2)//width
		return;
	if (this->_videoSock->ReadAll(this->_videoBuffer, 2) != 2)//height
		return;
#endif



	while (!this->_isStop)
	{
		if (this->_videoSock->ReadAll(this->_videoBuffer, HEADER_SIZE) != HEADER_SIZE)
			return;

		UINT64 pts = sc_read64be(this->_videoBuffer);
		INT32 len = sc_read32be(&this->_videoBuffer[8]);
		assert(len > 0 && len <= this->_buffSize);

		if (!((pts == NO_PTS || (pts & AV_NOPTS_VALUE) == 0) && len > 0))
			return;
		if (this->_videoSock->ReadAll(this->_videoBuffer, len) != len)
			return;
#if _DEBUG
		printf(std::string("pts:").append(std::to_string(pts)).append("  ,len:").append(std::to_string(len)).append("\r\n").c_str());
#endif

		AVPacket packet;
		int err = av_new_packet(&packet, len);
		if (err < 0)
			return;

		memcpy(packet.data, this->_videoBuffer, len);
		packet.pts = pts != NO_PTS ? (INT64)pts : AV_NOPTS_VALUE;

		if (this->_parsePacket->ParserPushPacket(&packet))
		{
			AVFrame* frame{ nullptr };
			if (this->_h264_mediaDecoder->Decode(&packet, &frame)) {

				//lock ref to frame
				_mtx.lock();
				av_frame_unref(this->_tempFrame);
				av_frame_free(&this->_tempFrame);
				this->_tempFrame = frame;
				_mtx.unlock();
				this->_ishaveFrame = true;
			}
		}
		av_packet_unref(&packet);
	}
}

bool Video::GetScreenSize(int& w, int& h) {
	if (!_ishaveFrame)
		return false;

	_mtx.lock();
	w = this->_tempFrame->width;
	h = this->_tempFrame->height;
	_mtx.unlock();

	return true;
}

int Video::GetScreenBufferSize() {
	if (!_ishaveFrame)
		return 0;
	_mtx.lock();
	int result = GetArgbBufferSize(this->_tempFrame->width, this->_tempFrame->height);
	_mtx.unlock();
	return result;
}

bool Video::GetScreenShot(BYTE* buffer, const int sizeInByte, int w, int h, int lineSize) {
	if (!_ishaveFrame)
		return false;

	AVFrame* clone_frame = nullptr;

	_mtx.lock();
	clone_frame = av_frame_clone(this->_tempFrame);
	_mtx.unlock();

	if (clone_frame == nullptr)
		return false;

	bool result = false;
	switch ((AVPixelFormat)clone_frame->format)
	{
	case AV_PIX_FMT_BGRA:
	{
		//check & copy to output
		if (w == clone_frame->width &&
			h == clone_frame->height &&
			lineSize == clone_frame->linesize[0] &&
			GetArgbBufferSize(w, h) == sizeInByte)
		{
			memcpy(buffer, clone_frame->data[0], lineSize);
			result = true;
			break;
		}
	}
	default:
	{
		result = SwsScale(clone_frame, buffer, sizeInByte, w, h, lineSize, AV_PIX_FMT_BGRA);// -> bitmap c# Format32bppArgb
	}
	}
	av_frame_unref(clone_frame);
	av_frame_free(&clone_frame);
	return result;
}

bool Video::SwsScale(const AVFrame* frame, BYTE* buffer, const int sizeInByte, int w, int h, int lineSize, AVPixelFormat target/* = AV_PIX_FMT_BGRA*/) {
	if (w <= 0 || w % 2 != 0) {
		return false;
	}
	if (h <= 0 || h % 2 != 0) {
		return false;
	}
	if (GetArgbBufferSize(w, h) != sizeInByte)
		return false;

	int linesizes[4]{ 0 };
	BYTE* const arr[1]{
		buffer
	};
	int err = av_image_fill_linesizes(linesizes, target, w);
	if (err < 0)
		return false;
	if (linesizes[0] != lineSize)
		return false;

	SwsContext* sws = sws_getContext(frame->width, frame->height, (AVPixelFormat)frame->format,
		w, h, target,
		SWS_FAST_BILINEAR, nullptr, nullptr, nullptr);
	if (sws == nullptr)
		return false;

	if (err >= 0) err = sws_scale(sws, frame->data, frame->linesize, 0, frame->height, arr, linesizes);
	sws_freeContext(sws);
	return err >= 0;
}