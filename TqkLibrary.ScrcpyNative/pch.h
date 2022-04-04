#pragma warning(disable: 26812)

#ifndef ScrcpyNativePCH_H
#define ScrcpyNativePCH_H

#ifdef TQKLIBRARYSCRCPYNATIVE_EXPORTS
#define ScrcpyNativeExport extern "C" __declspec( dllexport )
#else
#define ScrcpyNativeExport extern "C" __declspec( dllimport )
#endif

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
typedef class Scrcpy;
typedef struct ScrcpyNativeConfig;

#ifdef TQKLIBRARYSCRCPYNATIVE_EXPORTS
typedef class SocketWrapper;
typedef class Control;
typedef class Video;
typedef class ParsePacket;
typedef class MediaDecoder;
#include <windows.h>
#include <string>
#include <winsock2.h>
#include <mutex>
#include <assert.h>
#include <stdlib.h>
#include <d3d11.h>
#include "libav.h"
enum Orientations : int
{
	Auto = -1,
	Natural = 0,
	Counterclockwise90 = 1,
	/// <summary>
	/// 180°
	/// </summary>
	Flip = 2,
	Clockwise90 = 3
};
struct ScrcpyNativeConfig {
	Orientations Orientation;//int
	AVHWDeviceType HwType;//int
	int PacketBufferLength;
	bool ForceAdbForward;
	bool IsControl;
};

#include "ProcessWrapper.h"
#include "SocketWrapper.h"
#include "Utils.h"
#include "Video.h"
#include "Control.h"
#include "Scrcpy.h"
#endif //TQKLIBRARYSCRCPYNATIVE_EXPORTS

#include "Exports.h"

#endif //ScrcpyNativePCH_H
