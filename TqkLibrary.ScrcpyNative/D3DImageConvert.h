#ifndef _H_D3DImageConvert_H_
#define _H_D3DImageConvert_H_
class D3DImageConvert
{
	friend D3DImageView;
public:
	D3DImageConvert();
	~D3DImageConvert();

	bool Initialize(const AVD3D11VADeviceContext* d3d11va_device_ctx);
	bool Convert(const AVFrame* source);
	bool GetImage(const AVFrame* source, AVFrame* received);
	void Shutdown();
	bool IsNewFrame(UINT64* pts);
private:
	UINT64 m_currentPts;
	D3DClass m_d3d;
	VertexShaderClass m_vertex;
	PixelShaderNv12ToRgbaClass m_pixel;
	InputTextureClass m_input;
	RenderTextureClass m_renderTexture;
};
#endif
