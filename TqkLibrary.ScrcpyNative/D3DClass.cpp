#include "pch.h"
#include "D3DClass.h"

D3DClass::D3DClass() {

}

D3DClass::~D3DClass() {

}

bool D3DClass::Initialize(const AVD3D11VADeviceContext* d3d11va_device_ctx) {
	assert(d3d11va_device_ctx);
	this->m_device = d3d11va_device_ctx->device;
	this->m_deviceContext = d3d11va_device_ctx->device_context;
	return true;
}

bool D3DClass::Initialize() {
	HRESULT hr;

	// Driver types supported
	D3D_DRIVER_TYPE DriverTypes[] =
	{
		D3D_DRIVER_TYPE_HARDWARE,
		D3D_DRIVER_TYPE_WARP,
		D3D_DRIVER_TYPE_REFERENCE,
	};
	UINT NumDriverTypes = ARRAYSIZE(DriverTypes);

	// Feature levels supported
	D3D_FEATURE_LEVEL FeatureLevels[] =
	{
		D3D_FEATURE_LEVEL_11_0,
	};
	UINT NumFeatureLevels = ARRAYSIZE(FeatureLevels);
	D3D_FEATURE_LEVEL FeatureLevel;
	// This flag adds support for surfaces with a different color channel ordering
	// than the default. It is required for compatibility with Direct2D.
	UINT creationFlags =
		D3D11_CREATE_DEVICE_SINGLETHREADED |
		D3D11_CREATE_DEVICE_BGRA_SUPPORT;

	for (UINT DriverTypeIndex = 0; DriverTypeIndex < NumDriverTypes; ++DriverTypeIndex)
	{
		hr = D3D11CreateDevice(nullptr, DriverTypes[DriverTypeIndex], nullptr, creationFlags, FeatureLevels, NumFeatureLevels,
			D3D11_SDK_VERSION, this->m_device.GetAddressOf(), &FeatureLevel, this->m_deviceContext.GetAddressOf());
		if (SUCCEEDED(hr))
		{
			// Device creation succeeded, no need to loop anymore
			break;
		}
	}
	if (FAILED(hr))
		return false;

	return true;
}

ID3D11Device* D3DClass::GetDevice() {
	return this->m_device.Get();
}

ID3D11DeviceContext* D3DClass::GetDeviceContext() {
	return this->m_deviceContext.Get();
}

void D3DClass::Shutdown() {
	m_device.Reset();
	m_deviceContext.Reset();
}