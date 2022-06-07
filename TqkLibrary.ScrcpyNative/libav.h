#ifndef libav_H
#define libav_H
#include <d3d11.h>
#include <directxmath.h>
#include <wrl/client.h>
#include <dxgi.h>
extern "C" {
#include <libavcodec/avcodec.h>
#include <libavdevice/avdevice.h>
#include <libavformat/avformat.h>
#include <libswscale/swscale.h>

#include <libavutil/avutil.h>
#include <libavutil/imgutils.h>
#include <libavutil/hwcontext.h>
#include <libavutil/hwcontext_d3d11va.h>
}
#endif // libav_H
