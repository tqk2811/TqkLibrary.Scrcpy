#ifndef _H_VideoDecoder_H_
#define _H_VideoDecoder_H_
class VideoDecoder
{
public:
	VideoDecoder(const AVCodec* codec, const ScrcpyNativeConfig& nativeConfig);
	~VideoDecoder();
	bool Init();

	bool Decode(const AVPacket* packet);

	bool Convert(AVFrame* frame);
	bool GetFrameSize(int& w, int& h);
	bool IsNewFrame(INT64& pts);
	bool Draw(RenderTextureSurfaceClass* renderSurface, IUnknown* surface, bool isNewSurface, bool& isNewtargetView);

private:
	bool TransferNoHw(AVFrame* frame);
	bool FFmpegTransfer(AVFrame* frame);
	bool Nv12Convert(AVFrame* frame);

	ScrcpyNativeConfig _nativeConfig{};
	AVFrame* _decoding_frame{ nullptr };
	AVCodecContext* _codec_ctx{ nullptr };
	const AVCodec* _codec{ nullptr };
	AVHWDeviceType _hwType{ AV_HWDEVICE_TYPE_NONE };





	D3DClass* m_d3d11{ nullptr };
	VertexShaderClass* m_vertex{ nullptr };

	InputTextureNv12Class* m_d3d11_inputNv12{ nullptr };

	PixelShaderNv12ToImage32Class* m_d3d11_pixel_Nv12ToRgba{ nullptr };//get screen shot

	PixelShaderNv12ToImage32Class* m_d3d11_pixel_Nv12ToBgra{ nullptr };//video render

	RenderTextureClass* m_d3d11_renderTexture{ nullptr };

	std::mutex _mtx_frame;
};
#endif // !MediaDecoder_H