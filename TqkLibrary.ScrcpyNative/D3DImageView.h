#ifndef _H_D3DImageView_H_
#define _H_D3DImageView_H_
class D3DImageView
{
public:
	D3DImageView();
	~D3DImageView();

	void Shutdown();
	bool IsNewFrame(INT64 pts);

	RenderTextureSurfaceClass m_renderTextureSurface;
private:
	INT64 m_currentPts{ 0 };
};
#endif