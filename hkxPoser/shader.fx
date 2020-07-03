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
	float4x4 palette[80];
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
