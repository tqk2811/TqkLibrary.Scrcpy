//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

// Per-pixel color data passed through the pixel shader.
struct PixelShaderInput
{
	float4 pos         : SV_POSITION;
	float2 texCoord    : TEXCOORD0;
};

Texture2D<float>  y_Channel   : t0;
Texture2D<float>  u_Channel   : t1;
Texture2D<float>  v_Channel   : t2;
SamplerState      defaultSampler     : s0;

// Derived from https://msdn.microsoft.com/en-us/library/windows/desktop/dd206750(v=vs.85).aspx
// Section: Converting 8-bit YUV to RGB888
static const float3x3 YUVtoRGBCoeffMatrix =
{
	1.164383f,  1.164383f, 1.164383f,
	0.000000f, -0.391762f, 2.017232f,
	1.596027f, -0.812968f, 0.000000f
};

float3 ConvertYUVtoRGB(float3 yuv)
{
	// Derived from https://msdn.microsoft.com/en-us/library/windows/desktop/dd206750(v=vs.85).aspx
	// Section: Converting 8-bit YUV to RGB888

	// These values are calculated from (16 / 255) and (128 / 255)
	yuv -= float3(0.062745f, 0.501960f, 0.501960f);
	yuv = mul(yuv, YUVtoRGBCoeffMatrix);
	yuv = saturate(yuv);//BGR
	return float3(yuv.z, yuv.y, yuv.x);//GRB
}


float4 PS_planar_rgba(PixelShaderInput input) : SV_TARGET
{
	float y = y_Channel.Sample(defaultSampler, input.texCoord);
	float u = u_Channel.Sample(defaultSampler, input.texCoord);
	float v = v_Channel.Sample(defaultSampler, input.texCoord);

	return float4(ConvertYUVtoRGB(float3(y, u, v)), 1.f);
}