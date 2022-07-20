#ifndef _InputTextureClass_H_
#define _InputTextureClass_H_
class InputTextureClass
{
public:
	InputTextureClass();
	~InputTextureClass();


	bool Initialize(ID3D11Device* device, int width, int height);
	void Shutdown();

	bool Copy(ID3D11DeviceContext* device_ctx, const AVFrame* sourceFrame);

	ID3D11ShaderResourceView* GetLuminanceView();
	ID3D11ShaderResourceView* GetChrominanceView();

private:
	int m_width{ 0 };
	int m_height{ 0 };

	ComPtr<ID3D11Texture2D> m_texture_nv12 = nullptr;

	ComPtr<ID3D11ShaderResourceView> m_luminanceView = nullptr;
	ComPtr<ID3D11ShaderResourceView> m_chrominanceView = nullptr;
};
#endif // !_InputClass_H_


