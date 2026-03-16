#ifndef _H_ScrcpyWorking_H_
#define _H_ScrcpyWorking_H_
class ScrcpyInstance
{
	friend Scrcpy;
public:
	ScrcpyInstance(Scrcpy* scrcpy, const ScrcpyNativeConfig& nativeConfig);
	~ScrcpyInstance();
	bool Start();


private:
	bool _wsa_isStartUp = false;
	//function
	DWORD RunAdbProcess(LPCWSTR argument);

	//const
	Scrcpy* _scrcpy{ nullptr };//don't delete
	ScrcpyNativeConfig _nativeConfig{ };
	std::string _deviceName;
	int _physicalScreenW{ -1 };
	int _physicalScreenH{ -1 };
	
	//need release
	SOCKET _listenSock{ INVALID_SOCKET };
	ProcessWrapper* _process{ nullptr };
	Video* _video{ nullptr };
	Audio* _audio{ nullptr };
	Control* _control{ nullptr };
};
#endif