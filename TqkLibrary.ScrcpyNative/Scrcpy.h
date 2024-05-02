#ifndef _H_Scrcpy_H_
#define _H_Scrcpy_H_
class Scrcpy
{
public:
	Scrcpy(LPCWSTR deviceId);
	~Scrcpy();
	bool Connect(const ScrcpyNativeConfig& nativeConfig);
	void Stop();

	bool ControlCommand(const BYTE* command, const int sizeInByte);
	bool GetScreenShot(BYTE* buffer, const int sizeInByte, const int w, const int h, const int lineSize);
	bool GetScreenSize(int& w, int& h);
	bool GetDeviceName(BYTE* buffer, int sizeInByte);
	bool IsHaveScrcpyInstance();

	bool RegisterClipboardEvent(ClipboardReceivedDelegate callback);
	bool RegisterClipboardAcknowledgementEvent(ClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate);
	bool RegisterDisconnectEvent(OnDisconnectDelegate onDisconnectDelegate);
	bool RegisterUhdiOutputEvent(UhdiOutputDelegate uhdiOutputDelegate);

	bool Draw(RenderTextureSurfaceClass* renderSurface, IUnknown* surface, bool isNewSurface, bool& isNewtargetView);
	void VideoDisconnectCallback();
	void ControlClipboardCallback(BYTE* buffer, int length);
	void ControlClipboardAcknowledgementCallback(UINT64 sequence);
	void UhdiOutputCallback(UINT16 id, UINT16 size, const BYTE* buff);

	LPCWSTR GetDeviceId();
	INT64 ReadAudioFrame(AVFrame* pFrame, INT64 last_pts);
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
	UhdiOutputDelegate _uhdiOutputDelegate{ nullptr };
	OnDisconnectDelegate disconnectCallback{ nullptr };
};

#endif