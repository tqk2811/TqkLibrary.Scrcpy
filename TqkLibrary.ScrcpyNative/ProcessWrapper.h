#ifndef Process_H
#define Process_H
class ProcessWrapper
{
public:
	ProcessWrapper(wchar_t* args);
	~ProcessWrapper();

	bool WaitForExit(DWORD timeout = INFINITE);
	bool GetExitCodeProcess(LPDWORD lpExitCode);
	DWORD GetExitCode();
	/*int WriteStdIn(const BYTE* buff, int buffSize);
	int ReadStdOut(BYTE* buff, int buffSize);
	int ReadStdErr(BYTE* buff, int buffSize);*/
private:
	bool _isCreateSuccess{ false };
	STARTUPINFO _si{ 0 };
	PROCESS_INFORMATION _pi{ 0 };


};
#endif // !Process_H



