#ifndef _H_PixelShaderYuv420ToBgraClass_H_
#define _H_PixelShaderYuv420ToBgraClass_H_
class PixelShaderYuv420ToBgraClass
{
public:
	PixelShaderYuv420ToBgraClass();
	~PixelShaderYuv420ToBgraClass();

	bool Initialize(ID3D11Device* d3d11_device);
	void Set(
		ID3D11DeviceContext* d3d11_deviceCtx, 
		ID3D11ShaderResourceView* y, 
		ID3D11ShaderResourceView* u,
		ID3D11ShaderResourceView* v);

	void Shutdown();

private:
	ComPtr<ID3D11PixelShader> m_d3d11_pixelShader = nullptr;
	ComPtr<ID3D11SamplerState> m_d3d11_samplerState = nullptr;
};
#endif // !_H_PixelShaderYuv420ToBgraClass_H_

