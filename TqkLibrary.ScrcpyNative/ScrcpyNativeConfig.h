#ifndef _H_ScrcpyNativeConfig_H_
#define _H_ScrcpyNativeConfig_H_
struct ScrcpyNativeConfig {
	BYTE HwType;//AVHWDeviceType
	bool IsControl;
	bool IsUseD3D11ForUiRender;
	bool IsUseD3D11ForConvert;
	bool IsAudio;
	bool IsVideo;
	INT32 ConnectionTimeout;
	D3D11_FILTER Filter;
	UINT32 GpuThreadX;
	UINT32 GpuThreadY;
	BOOL IsForceUiGpuFlush;
};
#endif // !ScrcpyNativeConfig_H

