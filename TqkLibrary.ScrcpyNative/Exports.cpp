#include "pch.h"
#include "Exports.h"
#include "Scrcpy.h"

BYTE FFmpegHWSupport(BYTE bHWSupport)
{
	return (BYTE)av_hwdevice_iterate_types((AVHWDeviceType)bHWSupport);
}
Scrcpy* ScrcpyAlloc() {
	return new Scrcpy();
}
void ScrcpyFree(Scrcpy* scrcpy) {
	if (scrcpy != nullptr) delete scrcpy;
}
bool ScrcpyConnect(Scrcpy* scrcpy, const ScrcpyNativeConfig& nativeConfig, SOCKET videoSock, SOCKET audioSock, SOCKET controlSock) {
	if (scrcpy == nullptr) return false;
	return scrcpy->Connect(nativeConfig, videoSock, audioSock, controlSock);
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
bool ScrcpyGetScreenShot(Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte, const int w, const int h, const int lineSize, const INT32 swsFlag) {
	if (scrcpy == nullptr || buffer == nullptr) return false;
	return scrcpy->GetScreenShot(buffer, sizeInByte, w, h, lineSize, swsFlag);
}
INT64 ScrcpyReadAudioFrame(Scrcpy* scrcpy, AVFrame* pFrame, INT64 last_pts, DWORD waitFrameTime) {
	if (scrcpy == nullptr || pFrame == nullptr) return -1;
	return scrcpy->ReadAudioFrame(pFrame, last_pts, waitFrameTime);
}
INT64 ScrcpyReadAudioRaw(Scrcpy* scrcpy, BYTE* buffer, INT32 bufferSize, INT32 outNbChannels, INT32 outSampleRate, INT32 outSampleFmt, INT64 last_pts, DWORD waitFrameTime, INT32* outBytesWritten) {
	if (scrcpy == nullptr || buffer == nullptr || outBytesWritten == nullptr) return -1;
	return scrcpy->ReadAudioRaw(buffer, bufferSize, outNbChannels, outSampleRate, outSampleFmt, last_pts, waitFrameTime, outBytesWritten);
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