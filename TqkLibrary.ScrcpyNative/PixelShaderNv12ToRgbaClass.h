#ifndef _H_PixelShaderNv12ToRgbaClass_H_
#define _H_PixelShaderNv12ToRgbaClass_H_
class PixelShaderNv12ToRgbaClass
{
public:
	PixelShaderNv12ToRgbaClass();
	~PixelShaderNv12ToRgbaClass();

	bool Initialize(ID3D11Device* d3d11_device);
	void Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* luminance, ID3D11ShaderResourceView* chrominance);
	void Shutdown();

private:
	ComPtr<ID3D11PixelShader> m_d3d11_pixelShader = nullptr;
	ComPtr<ID3D11SamplerState> m_d3d11_samplerState = nullptr;
};
#endif // !_H_PixelShaderNv12ToRgbaClass_H_