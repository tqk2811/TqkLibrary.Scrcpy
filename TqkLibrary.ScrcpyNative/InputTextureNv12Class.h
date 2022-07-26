#ifndef _InputTextureNv12Class_H_
#define _InputTextureNv12Class_H_
class InputTextureNv12Class
{
public:
	InputTextureNv12Class();
	~InputTextureNv12Class();


	bool Initialize(ID3D11Device* device, int width, int height);
	void Shutdown();

	bool Copy(ID3D11DeviceContext* device_ctx, const AVFrame* sourceFrame);

	ID3D11ShaderResourceView* GetLuminanceView();
	ID3D11ShaderResourceView* GetChrominanceView();

	int Width();
	int Height();

private:
	int m_width{ 0 };
	int m_height{ 0 };

	ComPtr<ID3D11Texture2D> m_texture_nv12 = nullptr;
	ComPtr<ID3D11Texture2D> m_texture_nv12_cache = nullptr;

	ComPtr<ID3D11ShaderResourceView> m_luminanceView = nullptr;
	ComPtr<ID3D11ShaderResourceView> m_chrominanceView = nullptr;
};
#endif // !_InputClass_H_


