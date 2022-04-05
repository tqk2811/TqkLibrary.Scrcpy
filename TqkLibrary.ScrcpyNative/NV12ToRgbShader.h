#ifndef NV12ToRgbShader_H
#define NV12ToRgbShader_H
class NV12ToRgbShader
{
public:
	NV12ToRgbShader(AVHWDeviceContext* deviceContext);
	~NV12ToRgbShader();

	bool Init();

	bool Convert(const AVFrame* source, AVFrame** received);
private:
	AVHWDeviceContext* _avhw_deviceCtx{ NULL };
	AVD3D11VADeviceContext* _av_d3d11_vaDeviceCtx{ NULL };
	ID3D11DeviceContext* _d3d11_deviceCtx{ NULL };
	ID3D11Device* _d3d11_device{ NULL };

	ID3D11PixelShader* _d3d11_pixelShader{ NULL };

	bool InitShader();
};
#endif // !NV12ToRgbShader_H



