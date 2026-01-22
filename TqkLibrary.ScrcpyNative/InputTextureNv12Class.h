#ifndef _H_InputTextureNv12Class_H_
#define _H_InputTextureNv12Class_H_
class InputTextureNv12Class
{
public:
	InputTextureNv12Class();
	~InputTextureNv12Class();


	bool Initialize(ID3D11Device* device, int width, int height);
	void Shutdown();

	bool Copy(ID3D11DeviceContext* device_ctx, const AVFrame* sourceFrame);

	bool IsPlanar() const { return m_isPlanar; }
	ID3D11ShaderResourceView* Get_Y_View() {
		if (m_isPlanar) {
			return m_y_View_planar.Get();
		}
		else {
			return m_y_View_nv12.Get();
		}
	}
	ID3D11ShaderResourceView* Get_UV_View() { return m_uv_View.Get(); }
	ID3D11ShaderResourceView* Get_U_View() { return m_u_View.Get(); }
	ID3D11ShaderResourceView* Get_V_View() { return m_v_View.Get(); }

	int Width() { return m_width; }
	int Height() { return m_height; }

private:
	int m_width{ 0 };
	int m_height{ 0 };

	ComPtr<ID3D11Texture2D> m_texture_nv12 = nullptr;

	ComPtr<ID3D11Texture2D> m_texture_y = nullptr;
	ComPtr<ID3D11Texture2D> m_texture_u = nullptr;
	ComPtr<ID3D11Texture2D> m_texture_v = nullptr;

	bool m_isPlanar = false;

	ComPtr<ID3D11ShaderResourceView> m_y_View_nv12 = nullptr;
	ComPtr<ID3D11ShaderResourceView> m_y_View_planar = nullptr;
	ComPtr<ID3D11ShaderResourceView> m_uv_View = nullptr;
	ComPtr<ID3D11ShaderResourceView> m_u_View = nullptr;
	ComPtr<ID3D11ShaderResourceView> m_v_View = nullptr;
};
#endif // !_InputClass_H_


