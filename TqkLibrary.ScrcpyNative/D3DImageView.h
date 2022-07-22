#ifndef _H_D3DImageView_H_
#define _H_D3DImageView_H_
class D3DImageView
{
public:
	D3DImageView();
	~D3DImageView();

	void Shutdown();

	bool Draw(D3DClass* d3d, InputTextureClass* input, const AVFrame* source, IUnknown* surface, bool isNewSurface);

private:
	UINT64 m_currentPts{ 0 };
	VertexShaderClass m_vertex;
	PixelShaderNv12ToBgraClass m_pixel;
	RenderTextureSurfaceClass m_renderTextureSurface;
};
#endif