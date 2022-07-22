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

ScrcpyNativeExport D3DImageView* D3DImageViewAlloc();
ScrcpyNativeExport void D3DImageViewFree(D3DImageView* d3dView);
ScrcpyNativeExport bool D3DImageViewRender(D3DImageView* d3dView, Scrcpy* scrcpy, IUnknown* surface, bool isNewSurface);

typedef bool (*ClipboardReceivedDelegate)(BYTE* buffer, int length);
typedef bool (*ClipboardAcknowledgementDelegate)(UINT64 sequence);
ScrcpyNativeExport bool RegisterClipboardEvent(Scrcpy* scrcpy, ClipboardReceivedDelegate clipboardDelegate);
ScrcpyNativeExport bool RegisterClipboardAcknowledgementEvent(Scrcpy* scrcpy, ClipboardAcknowledgementDelegate clipboardAcknowledgementDelegate);
#endif // !ScrcpyNativeExports_H
