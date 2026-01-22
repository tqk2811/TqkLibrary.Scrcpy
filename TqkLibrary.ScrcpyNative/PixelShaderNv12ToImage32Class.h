#ifndef _H_PixelShaderNv12ToImage32Class_H_
#define _H_PixelShaderNv12ToImage32Class_H_
enum Image32Format
{
	RGBA,
	BGRA,
};
class PixelShaderNv12ToImage32Class
{
public:
	PixelShaderNv12ToImage32Class(Image32Format outputFormat);
	~PixelShaderNv12ToImage32Class();

	bool Initialize(ID3D11Device* d3d11_device, D3D11_FILTER filter);
	void Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* y, ID3D11ShaderResourceView* uv);
	void Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* y, ID3D11ShaderResourceView* u, ID3D11ShaderResourceView* v);
	void Shutdown();

private:
	Image32Format m_outputFormat;

	ComPtr<ID3D11PixelShader> m_d3d11_pixelShader_uvInterleave = nullptr;
	ComPtr<ID3D11PixelShader> m_d3d11_pixelShader_uvPlanar = nullptr;
	ComPtr<ID3D11SamplerState> m_d3d11_samplerState = nullptr;
};
#endif // !_H_PixelShaderNv12ToImage32Class_H_