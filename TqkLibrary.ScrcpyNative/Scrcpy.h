#ifndef Scrcpy_H
#define Scrcpy_H
class Scrcpy
{
public:
	Scrcpy(LPCWSTR deviceId);
	~Scrcpy();
	bool Connect(LPCWSTR config, const ScrcpyNativeConfig& nativeConfig);
	void Stop();

	bool ControlCommand(const BYTE* command, const int sizeInByte);
	int GetScreenBufferSize();
	bool GetScreenShot(BYTE* buffer, const int sizeInByte, int w, int h, int lineSize);
	bool GetScreenSize(int& w, int& h);
private:
	std::wstring _deviceId;
	std::mutex _controlMutext;
	std::mutex _videoMutext;

	ProcessWrapper* _process{ nullptr };
	Video* _video{ nullptr };
	Control* _control{ nullptr };

	DWORD RunAdbProcess(LPCWSTR argument);
};

#endif