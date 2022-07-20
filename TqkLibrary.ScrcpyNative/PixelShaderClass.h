#ifndef PixelShaderClass_H
#define PixelShaderClass_H
class PixelShaderClass
{
public:
	PixelShaderClass();
	~PixelShaderClass();

	bool Initialize(ID3D11Device* d3d11_device);
	void Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* luminance, ID3D11ShaderResourceView* chrominance);
	void Shutdown();

private:
	ComPtr<ID3D11PixelShader> m_d3d11_pixelShader = nullptr;
	ComPtr<ID3D11SamplerState> m_d3d11_samplerState = nullptr;
};
#endif // !PixelShaderClass_H