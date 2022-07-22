#ifndef _H_D3DImageConvert_H_
#define _H_D3DImageConvert_H_
class D3DImageConvert
{
	friend MediaDecoder;
public:
	D3DImageConvert();
	~D3DImageConvert();

	bool Initialize(D3DClass* d3d);
	bool Convert(D3DClass* d3d, InputTextureClass* input, const AVFrame* source);
	bool GetImage(D3DClass* d3d, const AVFrame* source, AVFrame* received);
	void Shutdown();
private:
	VertexShaderClass m_vertex;
	PixelShaderNv12ToRgbaClass m_pixel;
	RenderTextureClass m_renderTexture;
};
#endif
