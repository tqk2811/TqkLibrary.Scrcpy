#include "pch.h"
#include "Exports.h"
#include "Scrcpy.h"

BYTE FFmpegHWSupport(BYTE bHWSupport)
{
	return (BYTE)av_hwdevice_iterate_types((AVHWDeviceType)bHWSupport);
}
bool ClearKey() {
	return true;
}
bool AddKey(const BYTE* key, const int sizeInByte) {
	return true;
}
Scrcpy* ScrcpyAlloc(LPCWSTR deviceId) {
	return new Scrcpy(deviceId);
}
void ScrcpyFree(Scrcpy* scrcpy) {
	if (scrcpy != nullptr) delete scrcpy;
}
bool ScrcpyConnect(Scrcpy* scrcpy, const ScrcpyNativeConfig& nativeConfig) {
	if (scrcpy == nullptr) return false;
	return scrcpy->Connect(nativeConfig);
}
void ScrcpyStop(Scrcpy* scrcpy) {
	if (scrcpy == nullptr) return;
	scrcpy->Stop();
}
bool IsHaveScrcpyInstance(Scrcpy* scrcpy) {
	if (scrcpy == nullptr) return false;
	return scrcpy->IsHaveScrcpyInstance();
}
bool ScrcpyGetScreenSize(Scrcpy* scrcpy, int& w, int& h) {
	if (scrcpy == nullptr) return false;
	return scrcpy->GetScreenSize(w, h);
}
bool ScrcpyControlCommand(Scrcpy* scrcpy, const BYTE* command, const int sizeInByte) {
	if (scrcpy == nullptr || command == nullptr) return false;
	return scrcpy->ControlCommand(command, sizeInByte);
}
bool ScrcpyGetScreenShot(Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte, const int w, const int h, const int lineSize) {
	if (scrcpy == nullptr || buffer == nullptr) return false;
#ifdef Scrcpy_ScreenShot
	return scrcpy->GetScreenShot(buffer, sizeInByte, w, h, lineSize);
#else 
	return false;
#endif // Scrcpy_ScreenShot
}
bool ScrcpyGetDeviceName(Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte) {
	if (scrcpy == nullptr || buffer == nullptr) return false;
	return scrcpy->GetDeviceName(buffer, sizeInByte);
}
INT64 ScrcpyReadAudioFrame(Scrcpy* scrcpy, AVFrame* pFrame, INT64 last_pts) {
	if (scrcpy == nullptr || pFrame == nullptr) return -1;
	return scrcpy->ReadAudioFrame(pFrame, last_pts);
}

RenderTextureSurfaceClass* D3DImageViewAlloc() {
	return new RenderTextureSurfaceClass();
}
void D3DImageViewFree(RenderTextureSurfaceClass* renderSurface) {
	delete renderSurface;
}
bool D3DImageViewRender(RenderTextureSurfaceClass* renderSurface, Scrcpy* scrcpy, IUnknown* surface, bool isNewSurface, bool& isNewtargetView) {
	if (renderSurface == nullptr || scrcpy == nullptr || surface == nullptr)
		return false;
	return scrcpy->Draw(renderSurface, surface, isNewSurface, isNewtargetView);
}




bool RegisterClipboardEvent(Scrcpy* scrcpy, ClipboardReceivedDelegate clipboardDelegate) {
	if (scrcpy == nullptr || clipboardDelegate == nullptr) return false;
	return scrcpy->RegisterClipboardEvent(clipboardDelegate);
}
bool RegisterClipboardAcknowledgementEvent(Scrcpy* scrcpy, ClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate) {
	if (scrcpy == nullptr || clipboardAcknowledgementDelegate == nullptr) return false;
	return scrcpy->RegisterClipboardAcknowledgementEvent(clipboardAcknowledgementDelegate);
}

bool RegisterDisconnectEvent(Scrcpy* scrcpy, OnDisconnectDelegate onDisconnectDelegate) {
	if (scrcpy == nullptr || onDisconnectDelegate == nullptr) return false;
	return scrcpy->RegisterDisconnectEvent(onDisconnectDelegate);
}
bool RegisterUhdiOutputEvent(Scrcpy* scrcpy, UhdiOutputDelegate uhdiOutputDelegate) {
	if (scrcpy == nullptr || uhdiOutputDelegate == nullptr) return false;
	return scrcpy->RegisterUhdiOutputEvent(uhdiOutputDelegate);
}