#ifndef _H_D3DClass_H_
#define _H_D3DClass_H_

#include <d3d11.h>
#include <directxmath.h>
#include <wrl/client.h>
#include <dxgi.h>

using namespace Microsoft::WRL;
using namespace DirectX;

typedef class D3DImageConvert;
typedef class D3DImageView;

typedef class D3DClass;
typedef class InputTextureClass;
typedef class PixelShaderCopyClass;
typedef class PixelShaderNv12ToRgbaClass;
typedef class RenderTextureClass;
typedef class RenderTextureSurfaceClass;
typedef class VertexShaderClass;

#include "D3DClass.h"
#include "InputTextureClass.h"
#include "PixelShaderNv12ToRgbaClass.h"
#include "PixelShaderCopyClass.h"
#include "RenderTextureClass.h"
#include "RenderTextureSurfaceClass.h"
#include "VertexShaderClass.h"

#include "D3DImageConvert.h"
#include "D3DImageView.h"
#endif