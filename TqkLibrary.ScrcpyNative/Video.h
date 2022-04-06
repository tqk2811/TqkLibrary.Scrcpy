#ifndef Video_H
#define Video_H

class Video
{
public:
	Video(SOCKET sock, AVHWDeviceType hwType);
	~Video();
	void Start();
	void Stop();
	bool Init();

	bool GetScreenSize(int& w, int& h);
	int GetScreenBufferSize();
	bool GetScreenShot(BYTE* buffer, const int sizeInByte, int w, int h, int lineSize);
private:
	std::string _deviceName{};
	bool _ishaveFrame{ false };
	ParsePacket* _parsePacket{ nullptr };
	MediaDecoder* _h264_mediaDecoder{ nullptr };
	SocketWrapper* _videoSock{ nullptr };
	BYTE* _videoBuffer{ nullptr };
	AVFrame* _tempFrame{ nullptr };
	std::mutex _mtx;
	void threadStart();

	bool _isStop = false;
	DWORD _threadId{ 0 };
	HANDLE _threadHandle{ INVALID_HANDLE_VALUE };
	static DWORD WINAPI MyThreadFunction(LPVOID lpParam);

	bool SwsScale(const AVFrame* frame, BYTE* buffer, const int sizeInByte, int w, int h, int lineSize, AVPixelFormat target);
};

#endif // !Video_H