#ifndef _H_ScrcpyWorking_H_
#define _H_ScrcpyWorking_H_
class ScrcpyInstance
{
	friend Scrcpy;
public:
	ScrcpyInstance(Scrcpy* scrcpy, const ScrcpyNativeConfig& nativeConfig);
	~ScrcpyInstance();
	bool Connect(SOCKET videoSock, SOCKET audioSock, SOCKET controlSock);

private:
	Scrcpy* _scrcpy{ nullptr };//don't delete
	ScrcpyNativeConfig _nativeConfig{ };
	int _physicalScreenW{ -1 };
	int _physicalScreenH{ -1 };
	bool _wsa_isStartUp = false;

	//need release
	Video* _video{ nullptr };
	Audio* _audio{ nullptr };
	Control* _control{ nullptr };
};
#endif
