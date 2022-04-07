#ifndef Control_H
#define Control_H

class Control
{
public:
	Control(SOCKET sock);
	~Control();
	void Start();
	void Stop();

	bool ControlCommand(const BYTE* command, const int sizeInByte);
private:
	SocketWrapper* _sockControl{ nullptr };
	BYTE* _buffer{ nullptr };
	void threadStart();

	bool _isStop = false;
	DWORD _threadId{ 0 };
	HANDLE _threadHandle{ INVALID_HANDLE_VALUE };
	static DWORD WINAPI MyThreadFunction(LPVOID lpParam);
};

#endif // !Control_H

