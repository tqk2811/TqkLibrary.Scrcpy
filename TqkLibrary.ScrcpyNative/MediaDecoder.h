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
	AVFrame* Decode(const AVPacket* packet);
};
#endif // !MediaDecoder_H



