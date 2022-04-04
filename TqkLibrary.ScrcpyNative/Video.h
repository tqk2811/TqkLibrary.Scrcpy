#ifndef Video_H
#define Video_H

class Video
{
public:
	Video(SOCKET sock, int buffSize, AVHWDeviceType hwType);
	~Video();
	void Start();
	void Stop();
	
	UINT16 GetWidth() { return _width; }
	UINT16 GetHeight() { return _height; }
private:
	int _buffSize{ 0 };
	UINT16 _width{ 0 };
	UINT16 _height{ 0 };
	std::string _deviceName{};
	
	ParsePacket* _parsePacket{ nullptr };
	MediaDecoder* _h264_mediaDecoder{ nullptr };
	SocketWrapper* _videoSock{ nullptr };
	BYTE* _videoBuffer{ nullptr };
	AVFrame _tempFrame{ 0 };
	std::mutex _mtx;
	void threadStart();
	
	bool _isStop = false;
	DWORD _threadId{ 0 };
	HANDLE _threadHandle{ 0 };
	static DWORD WINAPI MyThreadFunction(LPVOID lpParam);
};

#endif // !Video_H