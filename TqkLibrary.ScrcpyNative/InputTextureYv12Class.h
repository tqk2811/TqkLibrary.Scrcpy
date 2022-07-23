#ifndef _H_InputTextureYv12Class_H_
#define _H_InputTextureYv12Class_H_
class InputTextureYv12Class
{
public:
	InputTextureYv12Class();
	~InputTextureYv12Class();


	bool Initialize(ID3D11Device* device, int width, int height);
	void Shutdown();

	bool Copy(ID3D11DeviceContext* device_ctx, const AVFrame* sourceFrame);

	ID3D11ShaderResourceView* GetYView();
	ID3D11ShaderResourceView* GetUView();
	ID3D11ShaderResourceView* GetVView();

	int Width();
	int Height();

private:
	int m_width{ 0 };
	int m_height{ 0 };

	ComPtr<ID3D11Texture2D> m_texture_yv12 = nullptr;

	ComPtr<ID3D11ShaderResourceView> m_yView = nullptr;
	ComPtr<ID3D11ShaderResourceView> m_uView = nullptr;
	ComPtr<ID3D11ShaderResourceView> m_vView = nullptr;
};
#endif

