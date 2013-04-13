#define MaxLights 10
#include "SoftShadow.vsi"
#include "Prelight/Shared.vsi"

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 NormalMatrix;

float3 EyePosition;

bool MaterialUnshaded;
float3 MaterialAmbient;
float3 MaterialDiffuse;
float3 MaterialSpecular;
float MaterialShininess;

float3 LightPosition[MaxLights];
//float3 LightAmbient[MaxLights];
float3 LightDiffuse[MaxLights];
float LightRange[MaxLights];
int NumLights;

float3 DirectionalLightDirection;
float3 DirectionalLightAmbient;
float3 DirectionalLightDiffuse;

texture2D DiffuseMap;
sampler2D diffuseMapSampler = sampler_state
{
	texture = <DiffuseMap>;
	minfilter = linear;
	magfilter = linear;
	mipfilter = linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture2D LightTexture;
sampler2D lightSampler = sampler_state
{
	texture = <LightTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

float MaterialReflection;
Texture EnvironmentMap;
samplerCUBE EnvironmentSampler = sampler_state
{
	texture = <EnvironmentMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

float4x4 LightView;
float4x4 LightProjection;
texture2D ShadowMap;
sampler2D shadowMapSampler = sampler_state
{
	texture = <ShadowMap>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
	float4 PositionCopy : TEXCOORD4;
	float4 OriginalPosition : TEXCOORD5;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
	output.ViewDirection = EyePosition - worldPosition.xyz;
	output.WorldPosition = worldPosition.xyz;
	
	output.TexCoord = input.TexCoord;

	output.Normal = mul(input.Normal, NormalMatrix);
	
	output.PositionCopy = mul(viewPosition, PrelightProjection);
	output.OriginalPosition = input.Position;
	
    return output;
}

float3 CalculateDiffuse(float3 normal, float3 directionToLight)
{
	return saturate(dot(normal, directionToLight));
}

float3 CalculateSpecularBlinn(float3 normal, float3 directionToLight, float3 directionFromEye, float shininess)
{
	float3 h = normalize(directionToLight - directionFromEye);
	return pow(saturate(dot(h, normal)), shininess);
}

float3 CalculateEnvironmentReflection(float3 normal, float3 directionFromEye) 
{
	return normalize(directionFromEye + 2 * normal * saturate(dot(-directionFromEye, normal)));
}

float4 GetPositionFromLight(float4 position)
{
	float4x4 wvp = mul(mul(World, LightView), LightProjection);
	return mul(position, wvp);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 normal = normalize(input.Normal);
	float4 color = tex2D(diffuseMapSampler, input.TexCoord);

	float3 directionFromEye = -normalize(input.ViewDirection);
	float normalizationFactor = ((MaterialShininess + 2.0) / 8.0);
	float4 totalLight = float4(MaterialAmbient * DirectionalLightAmbient, 1);

	if (MaterialUnshaded) 
	{
		return float4(MaterialAmbient, 1);
	}

	// Prelighting
	float2 texCoord = postProjToScreen(input.PositionCopy) + halfPixel();
	totalLight += tex2D(lightSampler, texCoord) * color;

	// Directional light
	float3 directionToLight = -normalize(DirectionalLightDirection);
	float selfShadow = saturate(4.0 * dot(normal, directionToLight));
	float3 fresnel = MaterialSpecular + (float3(1.0, 1.0, 1.0) - MaterialSpecular) * pow(clamp(1.0 + dot(-directionFromEye, normal),
			0.0, 1.0), 5.0);

	float3 reflection = CalculateEnvironmentReflection(normal, directionFromEye);
	totalLight += float4(selfShadow * (DirectionalLightDiffuse * color.rgb * CalculateDiffuse(normal, directionToLight) +
					DirectionalLightDiffuse * fresnel * CalculateSpecularBlinn(normal, directionToLight, directionFromEye, MaterialShininess) * normalizationFactor +
					texCUBE(EnvironmentSampler, reflection * float3(1,1,-1)) * fresnel * MaterialReflection), 1);
	
	totalLight = saturate(totalLight);

	float4 lightingPosition = GetPositionFromLight(input.OriginalPosition);
	float2 shadowCoord = 0.5 * lightingPosition.xy / lightingPosition.w;
	
	/*
	shadowCoord += 0.5f;
	shadowCoord.y = 1.0f - shadowCoord.y;

	if (shadowCoord.x > 0 && shadowCoord.x < 1 && shadowCoord.y > 0 && shadowCoord.y < 1)
	{
		float ourDepth = 1 - (lightingPosition.z / lightingPosition.w);
		totalLight.rgb *= .4 + .6 * CalcShadowTermPCF(shadowMapSampler, ourDepth, shadowCoord);
	}
	*/

	return totalLight;
}


technique CarShading
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
