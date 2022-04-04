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

bool ScrcpyGetScreenSize(Scrcpy* scrcpy, int& w, int& y) {
	return true;
}

bool ScrcpyControl(Scrcpy* scrcpy, const BYTE* command, const int sizeInByte) {
	return true;
}

int ScrcpyGetScreenShot(Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte) {
	return 0;
}

int ScrcpyGetScreenBufferSize(Scrcpy* scrcpy) {
	return 0;
}
