#include "pch.h"
#include "Utils.h"
#include "Scrcpy_pch.h"


Video::Video(Scrcpy* scrcpy, SOCKET sock, const ScrcpyNativeConfig& nativeConfig) {
	this->_scrcpy = scrcpy;
	this->_videoSock = new SocketWrapper(sock);
	this->_nativeConfig = nativeConfig;
}

Video::~Video() {
	if (this->_parsePacket)
		delete this->_parsePacket;
	if (this->_videoDecoder)
		delete this->_videoDecoder;
	if (this->_videoSock)
		delete this->_videoSock;
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
	this->_isStopMainLoop = true;
	if (this->_threadHandle != INVALID_HANDLE_VALUE)
		WaitForSingleObject(this->_threadHandle, INFINITE);
}

bool Video::Init() {
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
	video->_scrcpy->VideoDisconnectCallback();
	return 0;
}

bool Video::WaitForFirstFrame(DWORD timeout) {
	auto ret = WaitForSingleObject(this->_mtx_waitFirstFrame, timeout);
	return ret == WAIT_OBJECT_0;
}

void Video::threadStart() {
	this->_videoSock->ChangeBufferSize();

	AVCodecID codecId = this->_videoSock->ReadCodecId();
	const AVCodec* codec_decoder = avcodec_find_decoder(codecId);
	if (!codec_decoder)
		return;



	BYTE screenSize_buffer[8];
	if (this->_videoSock->ReadAll(screenSize_buffer, HEADER_SIZE) != HEADER_SIZE)
		return;
	uint32_t raw_codec_id = sc_read32be(screenSize_buffer);

#if _DEBUG
	uint32_t width = sc_read32be(screenSize_buffer);
	uint32_t height = sc_read32be(screenSize_buffer + 4);
	printf(std::string("width:").append(std::to_string(width)).append("\r\n").c_str());
	printf(std::string("height:").append(std::to_string(height)).append("\r\n").c_str());
#endif



	this->_parsePacket = new ParsePacket(codec_decoder);
	if (!this->_parsePacket->Init())
		return;

	this->_videoDecoder = new VideoDecoder(codec_decoder, this->_nativeConfig);
	if (!this->_videoDecoder->Init())
		return;

	while (!this->_isStopMainLoop)
	{
		AVPacket packet;
		if (!this->_videoSock->ReadPackage(&packet))
		{
			av_packet_unref(&packet);
			return;
		}

#if _DEBUG
		printf(std::string("Video pts:").append(std::to_string(packet.pts)).append("  ,len:").append(std::to_string(packet.size)).append("\r\n").c_str());
#endif

		if (this->_parsePacket->ParserPushPacket(&packet))
		{
			if (this->_videoDecoder->Decode(&packet))
			{
				if (!this->_ishaveFrame)
				{
					SetEvent(this->_mtx_waitFirstFrame);
					this->_ishaveFrame = true;
				}
			}
			else
			{
				av_packet_unref(&packet);
				return;
			}
		}
		av_packet_unref(&packet);
	}
}

bool Video::GetScreenSize(int& w, int& h) {
	if (!_ishaveFrame)
		return false;

	return this->_videoDecoder->GetFrameSize(w, h);
}

bool Video::GetCurrentRgbaFrame(AVFrame* frame) {
	if (!_ishaveFrame)
		return false;

	return this->_videoDecoder->Convert(frame);
}

bool Video::IsNewFrame(INT64& pts) {
	return this->_videoDecoder->IsNewFrame(pts);
}

bool Video::Draw(RenderTextureSurfaceClass* renderSurface, IUnknown* surface, bool isNewSurface, bool& isNewtargetView) {
	if (!this->_videoDecoder) return false;
	return this->_videoDecoder->Draw(
		renderSurface,
		surface,
		isNewSurface,
		isNewtargetView);
}