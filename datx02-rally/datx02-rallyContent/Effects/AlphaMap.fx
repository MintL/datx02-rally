#include "Prelight/Shared.vsi"

float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;

float3 DirectionalDirection;
float3 DirectionalAmbient;
float3 DirectionalDiffuse;

texture2D LightTexture;
sampler2D lightSampler = sampler_state
{
	texture = <LightTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

texture ColorMap;
sampler ColorMapSampler = sampler_state
{
	Texture = <ColorMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

texture AlphaMap;
sampler AlphaMapSampler = sampler_state
{
	Texture = <AlphaMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

texture NormalMap;
sampler NormalMapSampler = sampler_state
{
	Texture = <NormalMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD;
	float3 Normal : NORMAL;
	float3 Binormal : BINORMAL;
	float3 Tangent : TANGENT;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float4 PositionCopy : TEXCOORD1;
	float3x3 WorldToTangentSpace : TEXCOORD2;
	
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord;
	output.WorldToTangentSpace[0] = mul(normalize(input.Tangent), World);
	output.WorldToTangentSpace[1] = mul(normalize(input.Binormal), World);
	output.WorldToTangentSpace[2] = mul(normalize(input.Normal), World);

	output.PositionCopy = mul(viewPosition, PrelightProjection);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(ColorMapSampler, input.TexCoord);
	float4 totalLight = float4(DirectionalAmbient, 1.0) * color;
	totalLight.a = 1 - tex2D(AlphaMapSampler, input.TexCoord).r;
	clip(totalLight.a - 0.7843f);

	float3 normal = 2.0 * (tex2D(NormalMapSampler, input.TexCoord)) - 1.0;
	normal = normalize(mul(normal, input.WorldToTangentSpace));
	float3 directionToLight = -normalize(DirectionalDirection);
	float selfShadow = saturate(4.0 * dot(normal, directionToLight));

	totalLight.rgb += selfShadow * (DirectionalDiffuse * color.rgb * saturate(dot(normal, directionToLight)));
	
	float2 texCoord = postProjToScreen(input.PositionCopy) + halfPixel();
	totalLight += tex2D(lightSampler, texCoord) * color;

	return totalLight;
}

technique Technique1
{
    pass Pass1
    {
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
