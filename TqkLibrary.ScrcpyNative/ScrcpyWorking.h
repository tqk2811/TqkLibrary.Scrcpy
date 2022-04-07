#ifndef ScrcpyWorking_H
#define ScrcpyWorking_H
class ScrcpyWorking
{
	friend Scrcpy;
public:
	ScrcpyWorking(const Scrcpy* scrcpy, LPCWSTR config, const ScrcpyNativeConfig& nativeConfig);
	~ScrcpyWorking();
	bool Start();
private:
	//function	
	DWORD RunAdbProcess(LPCWSTR argument);

	//const
	const Scrcpy* _scrcpy{ nullptr };//don't delete
	std::wstring _config;
	ScrcpyNativeConfig _nativeConfig{ };
	
	//need release
	SOCKET _listenSock{ INVALID_SOCKET };
	ProcessWrapper* _process{ nullptr };
	Video* _video{ nullptr };
	Control* _control{ nullptr };
};
#endif