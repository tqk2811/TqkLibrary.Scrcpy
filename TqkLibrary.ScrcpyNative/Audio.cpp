#include "pch.h"
#include "Scrcpy_pch.h"

Audio::Audio(Scrcpy* scrcpy, SOCKET sock, const ScrcpyNativeConfig& nativeConfig) {
	this->_scrcpy = scrcpy;
	this->_audioSock = new SocketWrapper(sock);
}
Audio::~Audio() {
	delete this->_audioSock;
	CloseHandle(this->_threadHandle);
}
void Audio::Start() {
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
void Audio::Stop() {
	this->_audioSock->Stop();
	this->_isStopMainLoop = true;
	if (this->_threadHandle != INVALID_HANDLE_VALUE)
		WaitForSingleObject(this->_threadHandle, INFINITE);
}
bool Audio::Init() {
	return true;
}

DWORD Audio::MyThreadFunction(LPVOID lpParam) {
	Audio* audio = (Audio*)lpParam;
	audio->threadStart();
	audio->_isStopped = true;
	return 0;
}
void Audio::threadStart() {
	this->_audioSock->ChangeBufferSize();

	BYTE codec_buffer[4];

	if (this->_audioSock->ReadAll(codec_buffer, 4) != 4)
		return;
	uint32_t raw_codec_id = sc_read32be(codec_buffer);

	if (raw_codec_id == 0 ||	//stream explicitly disabled by the device
		raw_codec_id == 1)		//stream configuration error on the device
		return;

	AVCodecID codecId = sc_demuxer_to_avcodec_id(raw_codec_id);
	const AVCodec* codec_decoder = avcodec_find_decoder(codecId);
	if (!codec_decoder)
		return;

	this->_parsePacket = new ParsePacket(codec_decoder);
	if (!this->_parsePacket->Init())
		return;


	while (!this->_isStopMainLoop)
	{
		AVPacket packet;
		if (!this->_audioSock->ReadPackage(&packet))
		{
			av_packet_unref(&packet);
			return;
		}

#if _DEBUG
		printf(std::string("Audio pts:").append(std::to_string(packet.pts)).append("  ,len:").append(std::to_string(packet.size)).append("\r\n").c_str());
#endif

		if (this->_parsePacket->ParserPushPacket(&packet))
		{
			//if (this->_videoDecoder->Decode(&packet))
			//{
			//	if (!this->_ishaveFrame)
			//	{
			//		SetEvent(this->_mtx_waitFirstFrame);
			//		this->_ishaveFrame = true;
			//	}
			//}
			//else
			//{
			//	av_packet_unref(&packet);
			//	return;
			//}
		}
		av_packet_unref(&packet);
	}
}