#ifndef _H_PixelShaderNv12ToBgraClass_H_
#define _H_PixelShaderNv12ToBgraClass_H_

class PixelShaderNv12ToBgraClass
{
public:
	PixelShaderNv12ToBgraClass();
	~PixelShaderNv12ToBgraClass();

	bool Initialize(ID3D11Device* d3d11_device);
	void Set(ID3D11DeviceContext* d3d11_deviceCtx, ID3D11ShaderResourceView* luminance, ID3D11ShaderResourceView* chrominance);
	void Shutdown();

private:
	ComPtr<ID3D11PixelShader> m_d3d11_pixelShader = nullptr;
	ComPtr<ID3D11SamplerState> m_d3d11_samplerState = nullptr;
};

#endif // !_H_PixelShaderCopyClass_H_



