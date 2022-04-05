#include "pch.h"
#include "Exports.h"

bool LoadKey(const BYTE* key, const int sizeInByte) {
	return true;
}

Scrcpy* ScrcpyAlloc(LPCWSTR deviceId) {
	return new Scrcpy(deviceId);
}

void ScrcpyFree(Scrcpy* scrcpy) {
	delete scrcpy;
}
bool ScrcpyConnect(Scrcpy* scrcpy, LPCWSTR config, const ScrcpyNativeConfig& nativeConfig) {
	return scrcpy->Connect(config, nativeConfig);
}
void ScrcpyStop(Scrcpy* scrcpy) {
	scrcpy->Stop();
}
bool ScrcpyGetScreenSize(Scrcpy* scrcpy, int& w, int& h) {
	return scrcpy->GetScreenSize(w, h);
}

bool ScrcpyControlCommand(Scrcpy* scrcpy, const BYTE* command, const int sizeInByte) {
	return scrcpy->ControlCommand(command, sizeInByte);
}

bool ScrcpyGetScreenShot(Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte, int w, int h, int lineSize) {
	return scrcpy->GetScreenShot(buffer, sizeInByte, w, h, lineSize);
}

int ScrcpyGetScreenBufferSize(Scrcpy* scrcpy) {
	return scrcpy->GetScreenBufferSize();
}
