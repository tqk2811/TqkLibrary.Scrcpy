#ifndef _H_Control_H_
#define _H_Control_H_

class Control
{
public:
	Control(Scrcpy* scrcpy, SOCKET sock);
	~Control();
	void Start();
	void Stop();

	bool ControlCommand(const BYTE* command, const int sizeInByte);
private:
	//no need delete
	Scrcpy* scrcpy{ nullptr };

	//need delete
	SocketWrapper* _sockControl{ nullptr };
	BYTE* _buffer{ nullptr };
	
	//need close
	HANDLE _threadHandle{ INVALID_HANDLE_VALUE };

	
	DWORD _threadId{ 0 };
	void threadStart();
	bool _isStop = false;
	static DWORD WINAPI MyThreadFunction(LPVOID lpParam);
};

#endif // !Control_H

