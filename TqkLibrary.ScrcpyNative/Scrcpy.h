#ifndef Scrcpy_H
#define Scrcpy_H
class Scrcpy
{
public:
	Scrcpy(LPCWSTR deviceId);
	~Scrcpy();
	bool Connect(LPCWSTR config, const ScrcpyNativeConfig& nativeConfig);
private:
	std::wstring _deviceId;
	SOCKET _video{ INVALID_SOCKET };
	SOCKET _control{ INVALID_SOCKET };
	ProcessWrapper* _process{ nullptr };
	void RunAdbProcess(LPCWSTR argument);
};

#endif