#ifndef MediaDecoder_H
#define MediaDecoder_H
class MediaDecoder
{
private:
	AVFrame* _decoding_frame = nullptr;
	AVFrame* _transfer_frame = nullptr;
	AVCodecContext* _codec_ctx = nullptr;
	const AVCodec* _codec = nullptr;
	AVHWDeviceType _hwType;

	NV12ToRgbShader* _d3d11Shader = nullptr;

	AVHWDeviceContext* GetHWDeviceContext();
	bool Init();
public:
	MediaDecoder(const AVCodec* codec, AVHWDeviceType type);
	~MediaDecoder();
	bool Decode(const AVPacket* packet,AVFrame** received);
};
#endif // !MediaDecoder_H