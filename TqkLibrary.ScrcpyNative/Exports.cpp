#include "pch.h"
#include "Exports.h"

bool LoadKey(const BYTE* key, const int sizeInByte) {
	return true;
}

Scrcpy* ScrcpyAlloc(LPCWSTR deviceId) {
	return new Scrcpy(deviceId);
}

void ScrcpyFree(const Scrcpy* scrcpy) {
	delete scrcpy;
}

bool ScrcpyConnect(const Scrcpy* scrcpy, LPCWSTR config, const ScrcpyNativeConfig nativeConfig) {
	return true;
}

bool ScrcpyGetScreenSize(const Scrcpy* scrcpy, int& w, int& y) {
	return true;
}

bool ScrcpyControl(const Scrcpy* scrcpy, const BYTE* command, const int sizeInByte) {
	return true;
}

int ScrcpyGetScreenShot(const Scrcpy* scrcpy, BYTE* buffer, const int sizeInByte) {
	return 0;
}

int ScrcpyGetScreenBufferSize(const Scrcpy* scrcpy) {
	return 0;
}
