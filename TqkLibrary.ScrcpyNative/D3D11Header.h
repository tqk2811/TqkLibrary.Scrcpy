#ifndef _H_D3DClass_H_
#define _H_D3DClass_H_

#include <d3d11.h>
#include <directxmath.h>
#include <wrl/client.h>
#include <dxgi.h>

using namespace Microsoft::WRL;
using namespace DirectX;

typedef class D3DImageView;
typedef class D3DClass;

typedef class InputTextureYv12Class;
typedef class InputTextureNv12Class;

typedef class PixelShaderNv12ToBgraClass;
typedef class PixelShaderNv12ToRgbaClass;
typedef class PixelShaderYuv420ToBgraClass;

typedef class RenderTextureClass;
typedef class RenderTextureSurfaceClass;

typedef class VertexShaderClass;

#include "D3DClass.h"

#include "InputTextureYv12Class.h"
#include "InputTextureNv12Class.h"

#include "PixelShaderNv12ToRgbaClass.h"
#include "PixelShaderNv12ToBgraClass.h"
#include "PixelShaderYuv420ToBgraClass.h"

#include "RenderTextureClass.h"
#include "RenderTextureSurfaceClass.h"

#include "VertexShaderClass.h"

#include "D3DImageView.h"
#endif