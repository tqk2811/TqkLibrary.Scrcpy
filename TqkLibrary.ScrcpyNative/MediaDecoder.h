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

public:
	MediaDecoder(const AVCodec* codec, AVHWDeviceType type);
	~MediaDecoder();
	bool Init();
	bool Decode(const AVPacket* packet,[OUT] AVFrame** received);
	AVHWDeviceContext* GetHWDeviceContext();
};
#endif // !MediaDecoder_H