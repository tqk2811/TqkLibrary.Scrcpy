#ifndef _H_ScrcpyNativeConfig_H_
#define _H_ScrcpyNativeConfig_H_
struct ScrcpyNativeConfig {
	BYTE HwType;//AVHWDeviceType
	bool ForceAdbForward;
	bool IsControl;
	bool IsUseD3D11ForUiRender;
	bool IsUseD3D11ForConvert;
	bool IsAudio;
	LPCWSTR AdbPath;
	LPCWSTR ScrcpyServerPath;
	LPCWSTR ConfigureArguments;
	INT32 ConnectionTimeout;
	D3D11_FILTER Filter;
	INT32 SCID;
};
#endif // !ScrcpyNativeConfig_H

