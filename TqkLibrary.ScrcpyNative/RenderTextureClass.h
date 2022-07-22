#ifndef RenderTextureClass_H
#define RenderTextureClass_H
class RenderTextureClass
{
public:
	RenderTextureClass();
	~RenderTextureClass();

	bool Initialize(ID3D11Device* device, int textureWidth, int textureHeight);
	void Shutdown();

	void SetRenderTarget(ID3D11DeviceContext*, ID3D11DepthStencilView*);
	void ClearRenderTarget(ID3D11DeviceContext*, ID3D11DepthStencilView*, float, float, float, float);
	void SetViewPort(ID3D11DeviceContext* device_ctx, int width, int height);
	bool GetImage(ID3D11DeviceContext* deviceContext, const AVFrame* source, AVFrame* received);

private:
	int m_textureWidth{ 0 };
	int m_textureHeight{ 0 };
	ComPtr<ID3D11Texture2D> m_renderTargetTexture = nullptr;
	ComPtr<ID3D11Texture2D> m_renderTargetTexture_copy = nullptr;
	ComPtr<ID3D11RenderTargetView> m_renderTargetView = nullptr;
};
#endif

