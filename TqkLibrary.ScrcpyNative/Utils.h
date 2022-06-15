#ifndef Utils_H
#define Utils_H

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
		//::MessageBoxA(NULL, (LPCSTR)buffer, (LPCSTR)"Libav", MB_OK);
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

#endif // !Utils_H
