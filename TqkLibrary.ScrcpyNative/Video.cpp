#include "pch.h"
#include "Video.h"
#include "libav.h"
#include "ParsePacket.h"
#include "MediaDecoder.h"
#include "SocketWrapper.h"
#include "Utils.h"

#define HEADER_SIZE 12
#define DEVICE_NAME_SIZE 64
#define NO_PTS UINT64_MAX
Video::Video(SOCKET sock, AVHWDeviceType hwType) {
	this->_videoSock = new SocketWrapper(sock);
	this->_videoBuffer = new BYTE[DEVICE_NAME_SIZE];
	const AVCodec* h264_decoder = avcodec_find_decoder(AV_CODEC_ID_H264);
	this->_parsePacket = new ParsePacket(h264_decoder);
	this->_h264_mediaDecoder = new MediaDecoder(h264_decoder, hwType);
}

Video::~Video() {
	delete this->_parsePacket;
	delete this->_h264_mediaDecoder;
	delete this->_videoSock;
	delete this->_videoBuffer;
	av_frame_free(&this->_frame);
	av_frame_free(&this->_temp_frame);
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
	if (this->_threadHandle != INVALID_HANDLE_VALUE)
		WaitForSingleObject(this->_threadHandle, INFINITE);
}

bool Video::Init() {
	if (!this->_parsePacket->Init())
		return false;

	if (!this->_h264_mediaDecoder->Init())
		return false;

	this->_frame = av_frame_alloc();
	this->_temp_frame = av_frame_alloc();

	return this->_frame != nullptr && this->_temp_frame != nullptr;
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

		if (!((pts == NO_PTS || (pts & AV_NOPTS_VALUE) == 0) && len > 0))
			return;

		AVPacket packet;
		if (!avcheck(av_new_packet(&packet, len))) {
			return;
		}

		if (this->_videoSock->ReadAll(packet.data, len) != len)
		{
			av_packet_unref(&packet);
			return;
		}

#if _DEBUG
		printf(std::string("pts:").append(std::to_string(pts)).append("  ,len:").append(std::to_string(len)).append("\r\n").c_str());
#endif
		packet.pts = pts != NO_PTS ? (INT64)pts : AV_NOPTS_VALUE;

		if (this->_parsePacket->ParserPushPacket(&packet))
		{
			av_frame_unref(this->_temp_frame);
			if (this->_h264_mediaDecoder->Decode(&packet, this->_temp_frame)) 
			{
				//lock ref to frame
				_mtx.lock();
				av_frame_unref(this->_frame);
				av_frame_move_ref(this->_frame, this->_temp_frame);
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
	w = this->_frame->width;
	h = this->_frame->height;
	_mtx.unlock();

	return true;
}

bool Video::RefCurrentFrame(AVFrame* frame) {
	if (!_ishaveFrame)
		return false;

	_mtx.lock();
	int result = av_frame_ref(frame, this->_frame);
	_mtx.unlock();

	return result >= 0;
}