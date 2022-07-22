#ifndef Video_H
#define Video_H

class Video
{
	friend Scrcpy;
public:
	Video(SOCKET sock, const ScrcpyNativeConfig& nativeConfig);
	~Video();
	void Start();
	void Stop();
	bool Init();
	bool WaitForFirstFrame(DWORD timeout);

	bool GetScreenSize(int& w, int& h);
	bool RefCurrentFrame(AVFrame* frame);
	bool DoRender(IUnknown* surface, bool isNewSurface);
private:
	std::string _deviceName{};
	bool _ishaveFrame{ false };
	ParsePacket* _parsePacket{ nullptr };
	MediaDecoder* _h264_mediaDecoder{ nullptr };
	SocketWrapper* _videoSock{ nullptr };
	BYTE* _videoBuffer{ nullptr };
	AVFrame* _frame{ nullptr };
	AVFrame* _temp_frame{ nullptr };
	std::mutex _mtx_frame;
	HANDLE _mtx_waitFirstFrame{ INVALID_HANDLE_VALUE };

	void threadStart();

	bool _isStop = false;
	DWORD _threadId{ 0 };
	HANDLE _threadHandle{ INVALID_HANDLE_VALUE };
	static DWORD WINAPI MyThreadFunction(LPVOID lpParam);
};

#endif // !Video_H