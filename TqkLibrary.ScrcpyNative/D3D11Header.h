#ifndef _H_D3D11Header_H_
#define _H_D3D11Header_H_

#include <d3d11.h>
#include <directxmath.h>
#include <wrl/client.h>
#include <dxgi.h>

using namespace Microsoft::WRL;
using namespace DirectX;

typedef class D3DClass D3DClass;

typedef class InputTextureNv12Class InputTextureNv12Class;

typedef class PixelShaderNv12ToBgraClass PixelShaderNv12ToBgraClass;
typedef class PixelShaderNv12ToRgbaClass PixelShaderNv12ToRgbaClass;

typedef class RenderTextureClass RenderTextureClass;
typedef class RenderTextureSurfaceClass RenderTextureSurfaceClass;

typedef class VertexShaderClass VertexShaderClass;

#include "D3DClass.h"

#include "InputTextureNv12Class.h"

#include "PixelShaderNv12ToRgbaClass.h"
#include "PixelShaderNv12ToBgraClass.h"

#include "RenderTextureClass.h"
#include "RenderTextureSurfaceClass.h"

#include "VertexShaderClass.h"
#endif