#ifndef _H_D3DImageView_H_
#define _H_D3DImageView_H_
class D3DImageView
{
public:
	D3DImageView();
	~D3DImageView();

	void Shutdown();

	bool Draw(D3DImageConvert* imgConvert, IUnknown* surface, bool isNewSurface);

private:
	UINT64 m_currentPts;
	VertexShaderClass m_vertex;
	PixelShaderCopyClass m_pixel;
	RenderTextureSurfaceClass m_renderTextureSurface;
};
#endif