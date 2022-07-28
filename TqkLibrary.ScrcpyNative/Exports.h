#ifndef ScrcpyNativeExports_H
#define ScrcpyNativeExports_H

ScrcpyNativeExport BYTE FFmpegHWSupport(BYTE bHWSupport);
ScrcpyNativeExport bool ClearKey();
ScrcpyNativeExport bool AddKey(BYTE* key, const int sizeInByte);
ScrcpyNativeExport Scrcpy* ScrcpyAlloc(LPCWSTR deviceId);
ScrcpyNativeExport void ScrcpyFree(Scrcpy* scrcpy);
ScrcpyNativeExport bool ScrcpyConnect(Scrcpy* scrcpy, LPCWSTR config, const ScrcpyNativeConfig& nativeConfig);
ScrcpyNativeExport void ScrcpyStop(Scrcpy* scrcpy);
ScrcpyNativeExport bool ScrcpyGetScreenSize(Scrcpy* scrcpy, int& w, int& h);
ScrcpyNativeExport bool ScrcpyControlCommand(Scrcpy* scrcpy, const BYTE* command, const int sizeInByte);
ScrcpyNativeExport bool ScrcpyGetScreenShot(Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte, const int w, const int h, const int lineSize);

ScrcpyNativeExport RenderTextureSurfaceClass* D3DImageViewAlloc();
ScrcpyNativeExport void D3DImageViewFree(RenderTextureSurfaceClass* renderSurface);
ScrcpyNativeExport bool D3DImageViewRender(RenderTextureSurfaceClass* renderSurface, Scrcpy* scrcpy, IUnknown* surface, bool isNewSurface, bool& isNewtargetView);

typedef bool (*ClipboardReceivedDelegate)(BYTE* buffer, int length);
typedef bool (*ClipboardAcknowledgementDelegate)(UINT64 sequence);
typedef bool (*OnDisconnectDelegate)();
ScrcpyNativeExport bool RegisterClipboardEvent(Scrcpy* scrcpy, ClipboardReceivedDelegate clipboardDelegate);
ScrcpyNativeExport bool RegisterClipboardAcknowledgementEvent(Scrcpy* scrcpy, ClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate);
ScrcpyNativeExport bool RegisterDisconnectEvent(Scrcpy* scrcpy, OnDisconnectDelegate onDisconnectDelegate);
#endif // !ScrcpyNativeExports_H
