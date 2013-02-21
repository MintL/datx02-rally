float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InvViewProjection;

texture2D DepthTexture;
texture2D NormalTexture;
sampler 2D depthSampler = sampler_state
{
	texture = <DepthTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};
sampler2D normalSampler = sampler_state
{
	texture = <NormalTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

float3 LightColor;
float3 LightPosition;
float LightAttenuation;

#include "Shared.vsi"

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 LightPosition : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4x4 viewProjection = mul(View, Projection);
	float4x4 worldViewPorjection = mul(World, viewProjection);

	output.Position = mul(input.Position, worldViewProjection);
	output.LightPosition = output.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 texCoord = postProjToScreen(input.LightPosition) + halfPixel();
	float4 depth = tex2D(depthSampler, texCoord);

	// Recreate the position with the UV coordinates and depth value
	float4 position;
	position.x = texCoord.x * 2 - 1;
	position.y = (1. texCoord.y) * 2 - 1;
	position.z = depth.r;
	position.w = 1.0f;

	// Transform position from screen space to world space
	position = mul(position, InvViewProjection);
	position.xyz /= position.w;

	// Extract the normal from the normal map and move from
	// 0 to 1 range to -1 to 1 range
	float4 normal = (tex2D(normalSampler, texCoord) - .5) * 2;

	// Perform lighting calculations
	float3 lightDirection = normalize(LightPosition - position);
	float lighting = clamp(dot(normal, lightDirection), 0, 1);

	// Attenuate the light to simulate a point light
	float d = distance(LightPosition, position);
	float att = 1 - pow(d / LightAttenuation, 6);

	return float4(LightColor * lighting * att, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
