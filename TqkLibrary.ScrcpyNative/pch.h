#pragma warning(disable: 26812)

#ifndef ScrcpyNativePCH_H
#define ScrcpyNativePCH_H

#ifdef TQKLIBRARYSCRCPYNATIVE_EXPORTS
#define ScrcpyNativeExport extern "C" __declspec( dllexport )
#else
#define ScrcpyNativeExport extern "C" __declspec( dllimport )
#endif

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

#ifdef TQKLIBRARYSCRCPYNATIVE_EXPORTS
typedef class Scrcpy;
typedef class ScrcpyWorking;
typedef struct ScrcpyNativeConfig;

typedef class SocketWrapper;
typedef class ProcessWrapper;

typedef class Control;
typedef class Video;
typedef class ParsePacket;
typedef class MediaDecoder;
typedef class NV12ToRgbShader;
#include <windows.h>
#include <string>
#include <winsock2.h>
#include <mutex>
#include <array>
#include <assert.h>
#include <stdlib.h>
#include "libav.h"
#include <ws2tcpip.h>

#include <d3d11.h>
#include <directxmath.h>
#include <wrl/client.h>
#include <dxgi.h>

#pragma comment(lib,"ws2_32.lib")
using namespace Microsoft::WRL;
using namespace DirectX;
//enum Orientations : int
//{
//	Auto = -1,
//	Natural = 0,
//	Counterclockwise90 = 1,
//	/// <summary>
//	/// 180°
//	/// </summary>
//	Flip = 2,
//	Clockwise90 = 3
//};
struct ScrcpyNativeConfig {
	BYTE HwType;//AVHWDeviceType
	bool ForceAdbForward;
	bool IsControl;
	INT32 ConnectionTimeout;
};
extern bool IsCudaSupport;
//#include "ProcessWrapper.h"
//#include "SocketWrapper.h"
//#include "Utils.h"
//#include "ScrcpyWorking.h"
//#include "Video.h"
//#include "Control.h"
//#include "Scrcpy.h"
#endif //TQKLIBRARYSCRCPYNATIVE_EXPORTS

#include "Exports.h"

#endif //ScrcpyNativePCH_H
