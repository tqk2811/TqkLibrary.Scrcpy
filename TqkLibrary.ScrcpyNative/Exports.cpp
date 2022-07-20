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
bool ScrcpyConnect(Scrcpy* scrcpy, LPCWSTR config, const ScrcpyNativeConfig& nativeConfig) {
	if (scrcpy == nullptr || config == nullptr) return false;
	return scrcpy->Connect(config, nativeConfig);
}
void ScrcpyStop(Scrcpy* scrcpy) {
	if (scrcpy == nullptr) return;
	scrcpy->Stop();
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
	return scrcpy->GetScreenShot(buffer, sizeInByte, w, h, lineSize);
}
bool DoRender(Scrcpy* scrcpy, IUnknown* surface, bool isNewSurface) {
	if (scrcpy == nullptr || surface == nullptr) return false;
	return scrcpy->DoRender(surface, isNewSurface);
}

bool RegisterClipboardEvent(Scrcpy* scrcpy, ClipboardReceivedDelegate clipboardDelegate) {
	if (scrcpy == nullptr || clipboardDelegate == nullptr) return false;
	return scrcpy->RegisterClipboardEvent(clipboardDelegate);
}
bool RegisterClipboardAcknowledgementEvent(Scrcpy* scrcpy, ClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate) {
	if (scrcpy == nullptr || clipboardAcknowledgementDelegate == nullptr) return false;
	return scrcpy->RegisterClipboardAcknowledgementEvent(clipboardAcknowledgementDelegate);
}