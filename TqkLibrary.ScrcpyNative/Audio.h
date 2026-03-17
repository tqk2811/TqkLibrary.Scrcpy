#ifndef _H_Audio_H_
#define _H_Audio_H_
class Audio
{
public:
	Audio(Scrcpy* scrcpy, SOCKET sock, const ScrcpyNativeConfig& nativeConfig);
	~Audio();
	void Start();
	void Stop();
	bool Init();

	INT64 ReadAudioFrame(AVFrame* pFrame, INT64 last_pts);
	INT64 ReadAudioRaw(BYTE* buffer, INT32 bufferSize, INT32 outNbChannels, INT32 outSampleRate, AVSampleFormat outSampleFmt, INT64 last_pts, INT32* outBytesWritten);
	HANDLE GetWaitHanlde();
	void SetNotifyDisconnect(bool notify);

private:
	ScrcpyNativeConfig _nativeConfig{};
	Scrcpy* _scrcpy;




	bool _isStopped = false;
	bool _notifyDisconnect = false;

	//need delete
	AudioDecoder* _audioDecoder{ nullptr };
	SocketWrapper* _audioSock{ nullptr };
	HANDLE _mtx_waitNextFrame{ INVALID_HANDLE_VALUE };



	//thread object
	void threadStart();

	bool _isStopMainLoop = false;
	DWORD _threadId{ 0 };
	HANDLE _threadHandle{ INVALID_HANDLE_VALUE };
	static DWORD WINAPI MyThreadFunction(LPVOID lpParam);
};
#endif

