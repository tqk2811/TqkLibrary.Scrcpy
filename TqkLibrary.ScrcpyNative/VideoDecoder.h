#ifndef _H_VideoDecoder_H_
#define _H_VideoDecoder_H_
class VideoDecoder
{
	friend Scrcpy;
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
	AVFrame* _decoding_frame = nullptr;
	AVCodecContext* _codec_ctx = nullptr;
	const AVCodec* _codec = nullptr;
	AVHWDeviceType _hwType;





	D3DClass* m_d3d11;
	VertexShaderClass* m_vertex;

	InputTextureNv12Class* m_d3d11_inputNv12;

	PixelShaderNv12ToRgbaClass* m_d3d11_pixel_Nv12ToRgba;//get screen shot

	PixelShaderNv12ToBgraClass* m_d3d11_pixel_Nv12ToBgra;//video render

	RenderTextureClass* m_d3d11_renderTexture;

	std::mutex _mtx_frame;
};
#endif // !MediaDecoder_H