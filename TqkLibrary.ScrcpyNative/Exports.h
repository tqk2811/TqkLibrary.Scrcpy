#ifndef _H_ScrcpyNativeExports_H_
#define _H_ScrcpyNativeExports_H_

ScrcpyNativeExport BYTE FFmpegHWSupport(BYTE bHWSupport);
ScrcpyNativeExport Scrcpy* ScrcpyAlloc(LPCWSTR deviceId);
ScrcpyNativeExport void ScrcpyFree(Scrcpy* scrcpy);
ScrcpyNativeExport bool ScrcpyConnect(Scrcpy* scrcpy, const ScrcpyNativeConfig& nativeConfig);
ScrcpyNativeExport void ScrcpyStop(Scrcpy* scrcpy);
ScrcpyNativeExport bool IsHaveScrcpyInstance(Scrcpy* scrcpy);
ScrcpyNativeExport bool ScrcpyGetScreenSize(Scrcpy* scrcpy, int& w, int& h);
ScrcpyNativeExport bool ScrcpyControlCommand(Scrcpy* scrcpy, const BYTE* command, const int sizeInByte);
ScrcpyNativeExport bool ScrcpyGetScreenShot(Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte, const int w, const int h, const int lineSize);
ScrcpyNativeExport bool ScrcpyGetDeviceName(Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte);
ScrcpyNativeExport INT64 ScrcpyReadAudioFrame(Scrcpy* scrcpy, AVFrame* pFrame, INT64 last_pts, DWORD waitFrameTime);

ScrcpyNativeExport RenderTextureSurfaceClass* D3DImageViewAlloc();
ScrcpyNativeExport void D3DImageViewFree(RenderTextureSurfaceClass* renderSurface);
ScrcpyNativeExport bool D3DImageViewRender(RenderTextureSurfaceClass* renderSurface, Scrcpy* scrcpy, IUnknown* surface, bool isNewSurface, bool& isNewtargetView);

typedef bool (*ClipboardReceivedDelegate)(BYTE* buffer, int length);
typedef bool (*ClipboardAcknowledgementDelegate)(UINT64 sequence);
typedef bool (*OnDisconnectDelegate)();
typedef bool (*UhdiOutputDelegate)(UINT16 id, UINT16 size, const BYTE* buff);

ScrcpyNativeExport bool RegisterClipboardEvent(Scrcpy* scrcpy, ClipboardReceivedDelegate clipboardDelegate);
ScrcpyNativeExport bool RegisterClipboardAcknowledgementEvent(Scrcpy* scrcpy, ClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate);
ScrcpyNativeExport bool RegisterDisconnectEvent(Scrcpy* scrcpy, OnDisconnectDelegate onDisconnectDelegate);
ScrcpyNativeExport bool RegisterUhdiOutputEvent(Scrcpy* scrcpy, UhdiOutputDelegate uhdiOutputDelegate);
#endif // !ScrcpyNativeExports_H
