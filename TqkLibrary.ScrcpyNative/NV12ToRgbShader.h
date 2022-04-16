#ifndef NV12ToRgbShader_H
#define NV12ToRgbShader_H
class NV12ToRgbShader
{
public:
	NV12ToRgbShader();
	~NV12ToRgbShader();

	bool Init();

	bool Convert(const AVFrame* source, AVFrame** received);
private:

	ID3D11DeviceContext* _d3d11_deviceCtx{ NULL };
	ID3D11Device* _d3d11_device{ NULL };
	ID3D11SamplerState* _d3d11_samplerState{ NULL };
	//shader
	ID3D11PixelShader* _d3d11_pixelShader{ NULL };
	ID3D11InputLayout* _d3d11_inputLayout{ NULL };
	ID3D11VertexShader* _d3d11_vertexShader{ NULL };



	//SharedSurf
	ID3D11Texture2D* _texture_nv12{ nullptr };
	ID3D11ShaderResourceView* _luminanceView{ nullptr };
	ID3D11ShaderResourceView* _chrominanceView{ nullptr };
	ID3D11RenderTargetView* _renderTargetView{ nullptr };
	ID3D11Texture2D* _texture_rgba_target{ nullptr };
	uint32_t _width{ 0 };
	uint32_t _height{ 0 };
	bool CreateSharedSurf(int width, int height);
	void ReleaseSharedSurf();
};
#endif // !NV12ToRgbShader_H



