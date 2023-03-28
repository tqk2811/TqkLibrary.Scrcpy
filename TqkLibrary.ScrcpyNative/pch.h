#pragma warning(disable: 26812)

#ifndef _H_ScrcpyNativePCH_H_
#define _H_ScrcpyNativePCH_H_

#ifdef TQKLIBRARYSCRCPYNATIVE_EXPORTS
#define ScrcpyNativeExport extern "C" __declspec( dllexport )
#else
#define ScrcpyNativeExport extern "C" __declspec( dllimport )
#endif

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

typedef struct ScrcpyNativeConfig ScrcpyNativeConfig;
#ifdef TQKLIBRARYSCRCPYNATIVE_EXPORTS
typedef class Scrcpy Scrcpy;
typedef class ScrcpyInstance ScrcpyInstance;

typedef class SocketWrapper SocketWrapper;
typedef class ProcessWrapper ProcessWrapper;

typedef class Control Control;
typedef class Video Video;
typedef class ParsePacket ParsePacket;
typedef class MediaDecoder MediaDecoder;
#include <windows.h>
#include <string>
#include <winsock2.h>
#include <mutex>
#include <array>
#include <assert.h>
#include <stdlib.h>

#include "libav.h"
#include "D3D11Header.h"

#include <ws2tcpip.h>

#include "ScrcpyNativeConfig.h"
#pragma comment(lib,"ws2_32.lib")

#endif //TQKLIBRARYSCRCPYNATIVE_EXPORTS

#include "Exports.h"

#endif //ScrcpyNativePCH_H
