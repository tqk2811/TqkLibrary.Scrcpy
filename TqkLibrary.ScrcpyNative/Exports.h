#ifndef ScrcpyNativeExports_H
#define ScrcpyNativeExports_H

ScrcpyNativeExport bool LoadKey(BYTE* key, const int sizeInByte);
ScrcpyNativeExport Scrcpy* ScrcpyAlloc(LPCWSTR deviceId);
ScrcpyNativeExport void ScrcpyFree(Scrcpy* scrcpy);
ScrcpyNativeExport bool ScrcpyConnect(Scrcpy* scrcpy, LPCWSTR config, const ScrcpyNativeConfig& nativeConfig);
ScrcpyNativeExport void ScrcpyStop(Scrcpy* scrcpy);
ScrcpyNativeExport bool ScrcpyGetScreenSize(Scrcpy* scrcpy, int& w, int& y);
ScrcpyNativeExport bool ScrcpyControl(Scrcpy* scrcpy, const BYTE* command, const int sizeInByte);
ScrcpyNativeExport int ScrcpyGetScreenShot(Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte);
ScrcpyNativeExport int ScrcpyGetScreenBufferSize(Scrcpy* scrcpy);
#endif // !ScrcpyNativeExports_H
