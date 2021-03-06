float4x4 WorldViewProjection;
float4x4 InvViewProjection;

texture2D NormalDepthTexture;
sampler2D normalDepthSampler = sampler_state
{
	texture = <NormalDepthTexture>;
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

	output.Position = mul(input.Position, WorldViewProjection);
	output.LightPosition = output.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 texCoord = postProjToScreen(input.LightPosition) + halfPixel();
	float4 normalDepth = tex2D(normalDepthSampler, texCoord);

	// Recreate the position with the UV coordinates and depth value
	float4 position;
	position.x = texCoord.x * 2 - 1;
	position.y = (1 - texCoord.y) * 2 - 1;
	position.z = 1 - normalDepth.a;
	position.w = 1.0f;

	// Transform position from screen space to world space
	position = mul(position, InvViewProjection);
	position.xyz /= position.w;
	position.w = 1;

	// Extract the normal from the normal map and move from
	// 0 to 1 range to -1 to 1 range
	float3 normal = normalize((normalDepth.xyz - .5) * 2);

	// Perform lighting calculations
	float3 lightDirection = LightPosition - position;
	float att = saturate(1 - dot(lightDirection / LightAttenuation, lightDirection / LightAttenuation));

	lightDirection = normalize(lightDirection);
	float lighting = clamp(dot(normal, lightDirection), 0, 1);


	// Attenuate the light to simulate a point light
	//float d = distance(LightPosition, position);
	

	return float4(LightColor * lighting * att, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
