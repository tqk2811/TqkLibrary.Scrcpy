#ifndef Scrcpy_H
#define Scrcpy_H
class Scrcpy
{
public:
	Scrcpy(LPCWSTR deviceId);
	~Scrcpy();
	bool Connect(LPCWSTR config, const ScrcpyNativeConfig& nativeConfig);
	void Stop();
private:
	std::wstring _deviceId;
	ProcessWrapper* _process{ nullptr };
	Video* _video{ nullptr };
	Control* _control{ nullptr };
	DWORD RunAdbProcess(LPCWSTR argument);
};

#endif