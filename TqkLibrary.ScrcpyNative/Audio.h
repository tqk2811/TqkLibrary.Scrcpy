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

private:
	Scrcpy* _scrcpy;

	bool _isStopped = false;



	ParsePacket* _parsePacket{ nullptr };
	SocketWrapper* _audioSock{ nullptr };





	void threadStart();

	bool _isStopMainLoop = false;
	DWORD _threadId{ 0 };
	HANDLE _threadHandle{ INVALID_HANDLE_VALUE };
	static DWORD WINAPI MyThreadFunction(LPVOID lpParam);
};
#endif

