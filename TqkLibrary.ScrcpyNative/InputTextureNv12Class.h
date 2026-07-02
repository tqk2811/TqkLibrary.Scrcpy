#ifndef _H_InputTextureNv12Class_H_
#define _H_InputTextureNv12Class_H_
#include <map>
class InputTextureNv12Class
{
public:
	InputTextureNv12Class();
	~InputTextureNv12Class();


	bool Initialize(ID3D11Device* device, int width, int height);
	void Shutdown();

	// zeroCopy: sample the decoder's shader-readable pool textures directly (no per-frame copy).
	// Requires the pool to have been created with D3D11_BIND_SHADER_RESOURCE; falls back to a copy
	// into m_texture_nv12 when zeroCopy is false or the per-slice SRVs can't be created.
	bool Copy(ID3D11Device* device, ID3D11DeviceContext* device_ctx, const AVFrame* sourceFrame, bool zeroCopy);

	bool IsPlanar() const { return m_isPlanar; }
	ID3D11ShaderResourceView* Get_Y_View() {
		if (m_isZeroCopy) {
			return m_current_y_view;
		}
		else if (m_isPlanar) {
			return m_y_View_planar.Get();
		}
		else {
			return m_y_View_nv12.Get();
		}
	}
	ID3D11ShaderResourceView* Get_UV_View() { return m_isZeroCopy ? m_current_uv_view : m_uv_View.Get(); }
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

	// Zero-copy path: SRVs viewing individual slices of the decoder pool texture, cached by
	// (texture, slice) and reused across frames. m_current_* point at the active frame's slice.
	bool GetOrCreateSliceViews(ID3D11Device* device, ID3D11Texture2D* texture, UINT slice);

	bool m_isZeroCopy = false;
	ID3D11ShaderResourceView* m_current_y_view = nullptr;
	ID3D11ShaderResourceView* m_current_uv_view = nullptr;
	std::map<std::pair<ID3D11Texture2D*, UINT>, std::pair<ComPtr<ID3D11ShaderResourceView>, ComPtr<ID3D11ShaderResourceView>>> m_slice_views;
};
#endif // !_InputClass_H_


