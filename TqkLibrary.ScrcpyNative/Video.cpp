#include "pch.h"
#include "Video.h"
#include "libav.h"
#include "ParsePacket.h"
#include "MediaDecoder.h"
#include "SocketWrapper.h"
#include "Utils.h"
#include "Scrcpy.h"
#define PACKET_BUFFER_SIZE 1 << 18//256k
#define HEADER_SIZE 12
#define DEVICE_NAME_SIZE 64
#define NO_PTS UINT64_MAX

#define SC_PACKET_FLAG_CONFIG    (UINT64_C(1) << 63)
#define SC_PACKET_FLAG_KEY_FRAME (UINT64_C(1) << 62)
#define SC_PACKET_PTS_MASK (SC_PACKET_FLAG_KEY_FRAME - 1)

Video::Video(const Scrcpy* scrcpy, SOCKET sock, const ScrcpyNativeConfig& nativeConfig) {
	this->_scrcpy = scrcpy;
	this->_videoSock = new SocketWrapper(sock);
	this->_videoBuffer = new BYTE[DEVICE_NAME_SIZE];
	const AVCodec* h264_decoder = avcodec_find_decoder(AV_CODEC_ID_H264);
	this->_parsePacket = new ParsePacket(h264_decoder);
	this->_h264_mediaDecoder = new MediaDecoder(h264_decoder, nativeConfig);
}

Video::~Video() {
	delete this->_parsePacket;
	delete this->_h264_mediaDecoder;
	delete this->_videoSock;
	delete this->_videoBuffer;
	CloseHandle(this->_threadHandle);
	CloseHandle(this->_mtx_waitFirstFrame);
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

	//first bool is true for manual reset else auto reset, second bool is initially signaled
	this->_mtx_waitFirstFrame = CreateEvent(NULL, TRUE, FALSE, NULL);
	if (this->_mtx_waitFirstFrame == INVALID_HANDLE_VALUE)
		return false;

	return this->_videoSock->ChangeBlockMode(true);
}

DWORD WINAPI Video::MyThreadFunction(LPVOID lpParam) {
	Video* video = (Video*)lpParam;
	video->threadStart();
	video->_isStopped = true;
	video->_scrcpy->disconnectCallback();
	return 0;
}

bool Video::WaitForFirstFrame(DWORD timeout) {
	auto ret = WaitForSingleObject(this->_mtx_waitFirstFrame, timeout);
	return ret == WAIT_OBJECT_0;
}

void Video::threadStart() {
	this->_videoSock->ChangeBufferSize(PACKET_BUFFER_SIZE);

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

		UINT64 pts_flags = sc_read64be(this->_videoBuffer);
		INT32 len = sc_read32be(&this->_videoBuffer[8]);

		AVPacket packet;
		if (!avcheck(av_new_packet(&packet, len))) {
			return;
		}

		if (this->_videoSock->ReadAll(packet.data, len) != len)
		{
			av_packet_unref(&packet);
			return;
		}
		if (pts_flags & SC_PACKET_FLAG_CONFIG) {
			packet.pts = AV_NOPTS_VALUE;
		}
		else {
			packet.pts = pts_flags & SC_PACKET_PTS_MASK;
		}

		if (pts_flags & SC_PACKET_FLAG_KEY_FRAME) {
			packet.flags |= AV_PKT_FLAG_KEY;
		}

		packet.dts = packet.pts;


#if _DEBUG
		//printf(std::string("pts:").append(std::to_string(pts)).append("  ,len:").append(std::to_string(len)).append("\r\n").c_str());
#endif

		if (this->_parsePacket->ParserPushPacket(&packet))
		{
			if (this->_h264_mediaDecoder->Decode(&packet))
			{
				if (!this->_ishaveFrame)
				{
					SetEvent(this->_mtx_waitFirstFrame);
					this->_ishaveFrame = true;
				}
			}
			else
			{
				return;
			}
		}
		av_packet_unref(&packet);
	}
}

bool Video::GetScreenSize(int& w, int& h) {
	if (!_ishaveFrame)
		return false;

	return this->_h264_mediaDecoder->GetFrameSize(w, h);
}

bool Video::GetCurrentRgbaFrame(AVFrame* frame) {
	if (!_ishaveFrame)
		return false;

	return this->_h264_mediaDecoder->Convert(frame);
}