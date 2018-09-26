// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
struct VS_IN
{
	float3 position : POSITION;
	float2 texcoord : TEXCOORD;
	float4 weights : BLENDWEIGHT;
	uint4 indices : BLENDINDICES;
};

struct PS_IN
{
	float4 position : SV_Position;
	float2 texcoord : TEXCOORD;
};

// update by camera
// for VS
cbuffer cb_camera : register(b0)
{
	float4x4 wvp;
}

// update by submesh
// for VS
cbuffer cb_submesh : register(b1)
{
	float4x4 palette[40];
}

// update by mesh
// for PS
cbuffer cb_mesh : register(b2)
{
	unsigned int SLSF1;
	unsigned int SLSF2;
	unsigned int unknown2;
	unsigned int unknown3;
}

Texture2D albedoMap;
SamplerState albedoSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

PS_IN VS( VS_IN input )
{
	PS_IN output;

	float4 inp = float4(input.position, 1);

	float4x4 mat = palette[input.indices.x] * input.weights.x +
		palette[input.indices.y] * input.weights.y +
		palette[input.indices.z] * input.weights.z +
		palette[input.indices.w] * input.weights.w;
	float4 p = mul(mat, inp);
	output.position = mul(wvp, p);
	output.texcoord = input.texcoord;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	bool model_space_normals = SLSF1 & (1<<12);

	float4 albedo = albedoMap.Sample( albedoSampler, input.texcoord );
	clip(albedo.a - 0.25); // alpha test

	if (model_space_normals)
		albedo += albedo;

	return albedo;
}
