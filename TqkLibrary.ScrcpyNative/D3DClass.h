#ifndef D3DClass_H
#define D3DClass_H
class D3DClass
{
public:
	D3DClass();
	~D3DClass();

	bool Initialize();
	bool Initialize(const AVD3D11VADeviceContext* d3d11va_device_ctx);
	void Shutdown();

	ID3D11Device* GetDevice();
	ID3D11DeviceContext* GetDeviceContext();
private:
	ComPtr<ID3D11Device> m_device = nullptr;
	ComPtr<ID3D11DeviceContext> m_deviceContext = nullptr;
};
#endif // !D3DClass_H