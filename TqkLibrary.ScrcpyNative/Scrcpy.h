#ifndef _H_Scrcpy_H_
#define _H_Scrcpy_H_
class Scrcpy
{
	friend ScrcpyInstance;
public:
	Scrcpy(LPCWSTR deviceId);
	~Scrcpy();
	bool Connect(const ScrcpyNativeConfig& nativeConfig);
	void Stop();

	bool ControlCommand(const BYTE* command, const int sizeInByte);
	bool GetScreenShot(BYTE* buffer, const int sizeInByte, const int w, const int h, const int lineSize);
	bool GetScreenSize(int& w, int& h);

	bool RegisterClipboardEvent(ClipboardReceivedDelegate callback);
	bool RegisterClipboardAcknowledgementEvent(ClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate);
	bool RegisterDisconnectEvent(OnDisconnectDelegate onDisconnectDelegate);

	bool Draw(RenderTextureSurfaceClass* renderSurface, IUnknown* surface, bool isNewSurface, bool& isNewtargetView);
	void VideoDisconnectCallback();
	void ControlClipboardCallback(BYTE* buffer, int length);
	void ControlClipboardAcknowledgementCallback(UINT64 sequence);
private:
	//const
	std::wstring _deviceId;
	std::mutex _mutex;
	std::mutex _mutex_instance;

	AVFrame cache;
	//need release
	ScrcpyInstance* _scrcpyInstance{ nullptr };
	ClipboardReceivedDelegate clipboardCallback{ nullptr };
	ClipboardAcknowledgementDelegate clipboardAcknowledgementCallback{ nullptr };

	OnDisconnectDelegate disconnectCallback{ nullptr };
};

#endif