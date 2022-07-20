#pragma warning(disable: 26812)

#ifndef ScrcpyNativePCH_H
#define ScrcpyNativePCH_H

#ifdef TQKLIBRARYSCRCPYNATIVE_EXPORTS
#define ScrcpyNativeExport extern "C" __declspec( dllexport )
#else
#define ScrcpyNativeExport extern "C" __declspec( dllimport )
#endif

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

typedef struct ScrcpyNativeConfig;
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
#include "D3D11Header.h"

#include <ws2tcpip.h>

#include "ScrcpyNativeConfig.h"
#pragma comment(lib,"ws2_32.lib")

#endif //TQKLIBRARYSCRCPYNATIVE_EXPORTS

#include "Exports.h"

#endif //ScrcpyNativePCH_H
