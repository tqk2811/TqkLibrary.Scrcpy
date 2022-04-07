#ifndef MediaDecoder_H
#define MediaDecoder_H
class MediaDecoder
{
public:
	MediaDecoder(const AVCodec* codec, AVHWDeviceType type);
	~MediaDecoder();
	bool Init();

	
	bool Decode(const AVPacket* packet,AVFrame** received);
private:
	AVFrame* _decoding_frame = nullptr;
	AVCodecContext* _codec_ctx = nullptr;
	const AVCodec* _codec = nullptr;
	AVHWDeviceType _hwType;
	
};
#endif // !MediaDecoder_H