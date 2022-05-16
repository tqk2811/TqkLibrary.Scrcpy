#ifndef NV12ToRgbShader_H
#define NV12ToRgbShader_H
class NV12ToRgbShader
{
public:
	NV12ToRgbShader(const AVD3D11VADeviceContext* d3d11va_device_ctx);
	~NV12ToRgbShader();

	bool Init();

	bool Convert(const AVFrame* source, AVFrame* received);
private:
	//init
	ComPtr<ID3D11DeviceContext> _d3d11_deviceCtx = nullptr;
	ComPtr<ID3D11Device> _d3d11_device = nullptr;
	ComPtr<ID3D11SamplerState> _d3d11_samplerState = nullptr;
	ComPtr<ID3D11VertexShader> _d3d11_vertexShader = nullptr;
	ComPtr<ID3D11InputLayout> _d3d11_inputLayout = nullptr;
	ComPtr<ID3D11PixelShader> _d3d11_pixelShader = nullptr;
	ComPtr<ID3D11Buffer> _d3d11_vertexBuffer = nullptr;


	void DeviceCtxSet(int width, int height);

	

	//SharedSurf
	
	ComPtr<ID3D11Texture2D> _texture_nv12 = nullptr;
	ComPtr<ID3D11ShaderResourceView> _luminanceView = nullptr;
	ComPtr<ID3D11ShaderResourceView> _chrominanceView = nullptr;
	ComPtr<ID3D11RenderTargetView> _renderTargetView = nullptr;
	ComPtr<ID3D11Texture2D> _texture_rgba_target = nullptr;
	ComPtr<ID3D11Texture2D> _texture_rgba_copy = nullptr;
	uint32_t _width{ 0 };
	uint32_t _height{ 0 };
	bool CreateSharedSurf(int width, int height);
	void ReleaseSharedSurf();
	bool CopyMapResource(const D3D11_MAPPED_SUBRESOURCE& ms, const AVFrame* source, AVFrame* received);
};
#endif // !NV12ToRgbShader_H



