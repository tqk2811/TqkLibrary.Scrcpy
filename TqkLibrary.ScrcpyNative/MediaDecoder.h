#ifndef MediaDecoder_H
#define MediaDecoder_H
class MediaDecoder
{
	friend Scrcpy;
public:
	MediaDecoder(const AVCodec* codec, const ScrcpyNativeConfig& nativeConfig);
	~MediaDecoder();
	bool Init();

	bool Decode(const AVPacket* packet);

	bool Convert(AVFrame* frame);
	bool GetFrameSize(int& w, int& h);
	bool IsNewFrame(INT64& pts);
	bool Draw(D3DImageView* view, IUnknown* surface, bool isNewSurface);

private:
	bool TransferNoHw(AVFrame* frame);
	bool FFmpegTransfer(AVFrame* frame);


	ScrcpyNativeConfig _nativeConfig{};
	AVFrame* _decoding_frame = nullptr;
	AVCodecContext* _codec_ctx = nullptr;
	const AVCodec* _codec = nullptr;
	AVHWDeviceType _hwType;



	D3DClass* m_d3d11{ nullptr };
	InputTextureClass* m_d3d11_input{ nullptr };

	D3DImageConvert* m_d3d11_convert = nullptr;

	std::mutex _mtx_frame;
	std::mutex _mtx_texture;
};
#endif // !MediaDecoder_H