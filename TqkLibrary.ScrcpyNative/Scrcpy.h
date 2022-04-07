#ifndef Scrcpy_H
#define Scrcpy_H
class Scrcpy
{
	friend ScrcpyWorking;
public:
	Scrcpy(LPCWSTR deviceId);
	~Scrcpy();
	bool Connect(LPCWSTR config, const ScrcpyNativeConfig& nativeConfig);
	void Stop();

	bool ControlCommand(const BYTE* command, const int sizeInByte);
	bool GetScreenShot(BYTE* buffer, const int sizeInByte, int w, int h, int lineSize);
	bool GetScreenSize(int& w, int& h);
private:
	//const
	std::wstring _deviceId;
	std::mutex _mutex;
	

	//need release
	ScrcpyWorking* _scrcpyWorking{ nullptr };
};

#endif