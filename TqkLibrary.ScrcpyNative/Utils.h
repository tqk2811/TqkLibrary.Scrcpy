#ifndef _H_Utils_H_
#define _H_Utils_H_

static inline void arlet(const char* s) {
	::MessageBoxA(NULL, (LPCSTR)s, (LPCSTR)"ShaderErr", MB_OK);
}

//#define avcheck(x) if(x < 0){\
//	char buffer[1024];\
//	av_strerror(err, buffer, 1024);\
//	::MessageBoxA(NULL, (LPCSTR)buffer, (LPCSTR)"Libav", MB_OK);\
//	return false;\
//}


static inline bool avcheck(int err) {
	if (err < 0) {
		char buffer[1024];
		av_strerror(err, buffer, 1024);
		printf("avcheck: %d %s", err, buffer);
#if _DEBUG
		::MessageBoxA(NULL, (LPCSTR)buffer, (LPCSTR)"Libav", MB_OK);
#endif

		return false;
	}
	return true;
}



static inline uint16_t sc_read16be(const uint8_t* buf) {
	return (buf[0] << 8) | buf[1];
}

static inline uint32_t sc_read32be(const uint8_t* buf) {
	return ((uint32_t)buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
}

static inline uint64_t sc_read64be(const uint8_t* buf) {
	uint32_t msb = sc_read32be(buf);
	uint32_t lsb = sc_read32be(&buf[4]);
	return ((uint64_t)msb << 32) | lsb;
}

static inline int GetArgbBufferSize(const int width, const int height, const int align) {
	return av_image_get_buffer_size(AVPixelFormat::AV_PIX_FMT_BGRA, width, height, align);
}

static inline bool IsHwSupport(AVHWDeviceType hwType) {
	AVHWDeviceType find = AVHWDeviceType::AV_HWDEVICE_TYPE_NONE;
	while (true) {
		find = av_hwdevice_iterate_types(find);
		if (find == AV_HWDEVICE_TYPE_NONE) return false;
		if (find == hwType) return true;
	}
}

static inline enum AVCodecID sc_demuxer_to_avcodec_id(uint32_t codec_id) {
#define SC_CODEC_ID_H264 UINT32_C(0x68323634) // "h264" in ASCII
#define SC_CODEC_ID_H265 UINT32_C(0x68323635) // "h265" in ASCII
#define SC_CODEC_ID_AV1 UINT32_C(0x00617631) // "av1" in ASCII
#define SC_CODEC_ID_OPUS UINT32_C(0x6f707573) // "opus" in ASCII
#define SC_CODEC_ID_AAC UINT32_C(0x00616163) // "aac in ASCII"
#define SC_CODEC_ID_RAW UINT32_C(0x00726177) // "raw" in ASCII
	switch (codec_id) {
	case SC_CODEC_ID_H264:
		return AV_CODEC_ID_H264;
	case SC_CODEC_ID_H265:
		return AV_CODEC_ID_HEVC;
	case SC_CODEC_ID_AV1:
		return AV_CODEC_ID_AV1;
	case SC_CODEC_ID_OPUS:
		return AV_CODEC_ID_OPUS;
	case SC_CODEC_ID_AAC:
		return AV_CODEC_ID_AAC;
	case SC_CODEC_ID_RAW:
		return AV_CODEC_ID_PCM_S16LE;
	default:
		return AV_CODEC_ID_NONE;
	}
}
#endif // !Utils_H
