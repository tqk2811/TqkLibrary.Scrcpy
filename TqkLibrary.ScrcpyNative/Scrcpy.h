#ifndef Scrcpy_H
#define Scrcpy_H
class Scrcpy
{
	friend ScrcpyInstance;
	friend Control;
public:
	Scrcpy(LPCWSTR deviceId);
	~Scrcpy();
	bool Connect(LPCWSTR config, const ScrcpyNativeConfig& nativeConfig);
	void Stop();

	bool ControlCommand(const BYTE* command, const int sizeInByte);
	bool GetScreenShot(BYTE* buffer, const int sizeInByte, const int w, const int h, const int lineSize);
	bool GetScreenSize(int& w, int& h);
	bool RegisterClipboardEvent(ClipboardReceivedDelegate callback);
	bool RegisterClipboardAcknowledgementEvent(ClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate);

	bool Draw(D3DImageView* d3d_imgView, IUnknown* surface, bool isNewSurface);
private:
	//const
	std::wstring _deviceId;
	std::mutex _mutex;

	AVFrame cache;
	//need release
	ScrcpyInstance* _scrcpyInstance{ nullptr };
	ClipboardReceivedDelegate clipboardCallback{ nullptr };
	ClipboardAcknowledgementDelegate clipboardAcknowledgementCallback{ nullptr };
};

#endif