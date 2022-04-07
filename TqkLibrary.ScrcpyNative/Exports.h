#ifndef ScrcpyNativeExports_H
#define ScrcpyNativeExports_H

ScrcpyNativeExport bool LoadKey(BYTE* key, int sizeInByte);
ScrcpyNativeExport Scrcpy* ScrcpyAlloc(LPCWSTR deviceId);
ScrcpyNativeExport void ScrcpyFree(Scrcpy* scrcpy);
ScrcpyNativeExport bool ScrcpyConnect(Scrcpy* scrcpy, LPCWSTR config, const ScrcpyNativeConfig& nativeConfig);
ScrcpyNativeExport void ScrcpyStop(Scrcpy* scrcpy);
ScrcpyNativeExport bool ScrcpyGetScreenSize(Scrcpy* scrcpy, int& w, int& h);
ScrcpyNativeExport bool ScrcpyControlCommand(Scrcpy* scrcpy, BYTE* command, int sizeInByte);
ScrcpyNativeExport bool ScrcpyGetScreenShot(Scrcpy* scrcpy, BYTE* buffer, int sizeInByte, int w, int h, int lineSize);
#endif // !ScrcpyNativeExports_H
