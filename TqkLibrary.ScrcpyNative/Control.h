#ifndef Control_H
#define Control_H

class Control
{
public:
	Control(SOCKET sock);
	~Control();
	void Start();
	void Stop();
private:
	SocketWrapper* _sockControl{ nullptr };
	BYTE* _buffer{ nullptr };
	void threadStart();

	bool _isStop = false;
	DWORD _threadId{ 0 };
	HANDLE _threadHandle{ 0 };
	static DWORD WINAPI MyThreadFunction(LPVOID lpParam);
};

#endif // !Control_H

