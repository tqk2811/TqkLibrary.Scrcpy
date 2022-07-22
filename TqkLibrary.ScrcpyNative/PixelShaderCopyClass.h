#ifndef _H_PixelShaderCopyClass_H_
#define _H_PixelShaderCopyClass_H_

class PixelShaderCopyClass
{
public:
	PixelShaderCopyClass();
	~PixelShaderCopyClass();

	bool Initialize(ID3D11Device* d3d11_device);
	void Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* rgba);
	void Shutdown();

private:
	ComPtr<ID3D11PixelShader> m_d3d11_pixelShader = nullptr;
	ComPtr<ID3D11SamplerState> m_d3d11_samplerState = nullptr;
};

#endif // !_H_PixelShaderCopyClass_H_



