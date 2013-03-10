#define MaxLights 10
#include "SoftShadow.vsi"

float3 tint;

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 PrelightProjection;
float3 EyePosition;
float4x4 NormalMatrix;

float3 LightPosition[MaxLights];
//float3 LightAmbient[MaxLights];
float3 LightDiffuse[MaxLights];
float LightRange[MaxLights];
int NumLights;

float3 DirectionalDirection;
float3 DirectionalAmbient;
float3 DirectionalDiffuse;

int FogEnabled = 1;
float3 FogColor = float3(0.05, 0.045, 0.04);
float FogStart = -1000;
float FogEnd = 8000;

texture2D LightTexture;
sampler2D lightSampler = sampler_state
{
	texture = <LightTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

float4x4 ShadowMapView;
float4x4 ShadowMapProjection;

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

texture TextureMap0;
sampler TextureMapSampler0 = sampler_state
{
	Texture = <TextureMap0>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture TextureMap1;
sampler TextureMapSampler1 = sampler_state
{
	Texture = <TextureMap1>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture TextureMap2;
sampler TextureMapSampler2 = sampler_state
{
	Texture = <TextureMap2>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

texture TextureMap3;
sampler TextureMapSampler3 = sampler_state
{
	Texture = <TextureMap3>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float4 TexWeights : TEXCOORD1;
};

#include "Prelight/Shared.vsi"

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
	float4 TexWeights : TEXCOORD4;
	float4 PositionCopy : TEXCOORD5;
	float4 OriginalPosition : TEXCOORD6;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.ViewDirection = EyePosition - worldPosition.xyz;

	output.TexCoord = input.TexCoord;
    output.Normal = mul(input.Normal, NormalMatrix);
	output.WorldPosition = worldPosition.xyz;
	output.TexWeights = input.TexWeights;
	output.PositionCopy = mul(viewPosition, PrelightProjection);
	output.OriginalPosition = input.Position;

    return output;
}

float ComputeFogFactor(float d)
{
	return clamp((d - FogStart) / (FogEnd - FogStart), 0, .75) * FogEnabled;
}

float4 GetPositionFromLight(float4 position)
{
	float4x4 wvp = mul(mul(World, ShadowMapView), ShadowMapProjection);
	return mul(position, wvp);
}

float3 CalculateEnvironmentReflection(float3 normal, float3 directionFromEye) 
{
	return normalize(directionFromEye + 2 * normal * saturate(dot(-directionFromEye, normal)));
}

float3 CalculateSpecularBlinn(float3 normal, float3 directionToLight, float3 directionFromEye, float shininess)
{
	float3 h = normalize(directionToLight - directionFromEye);
	return pow(saturate(dot(h, normal)), shininess);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 normal = normalize(input.Normal);

	float3 directionFromEye = -normalize(input.ViewDirection);
	float MaterialShininess = 10;
	float normalizationFactor = normalizationFactor = ((MaterialShininess + 2.0) / 8.0);

    float4 color = tex2D(TextureMapSampler0, input.TexCoord) * input.TexWeights.x;
	color += tex2D(TextureMapSampler1, input.TexCoord) * input.TexWeights.y;
	color += tex2D(TextureMapSampler2, input.TexCoord) * input.TexWeights.z;
	color += tex2D(TextureMapSampler3, input.TexCoord) * input.TexWeights.w;
	
	float4 totalLight = float4(DirectionalAmbient, 1.0) * color;
	
	totalLight.rgb *= tint;
	
	float3 directionToLight = -normalize(DirectionalDirection);
	float selfShadow = saturate(4.0 * dot(normal, directionToLight));
	
	float specular = input.TexWeights.x * 0.2;
	float materialSpecular = float4(0.3, 0.3, 0.3, 1.0);
	float3 fresnel = materialSpecular + (float3(1.0, 1.0, 1.0) - materialSpecular) * pow(clamp(1.0 + dot(-directionFromEye, normal),
			0.0, 1.0), 5.0);

	totalLight.rgb += selfShadow * (DirectionalDiffuse * color.rgb * saturate(dot(normal, directionToLight))) +
			specular * fresnel * CalculateSpecularBlinn(normal, directionToLight, directionFromEye, MaterialShininess) * normalizationFactor;
	


	float4 lightingPosition = GetPositionFromLight(input.OriginalPosition);
	float2 shadowCoord = 0.5 * lightingPosition.xy / lightingPosition.w;
	
	shadowCoord += 0.5f;
	shadowCoord.y = 1.0f - shadowCoord.y;
	
	float dotProduct = dot(normal, directionToLight) / directionToLight;
	dotProduct += 1;
	dotProduct /= 2.0;
	

	//totalLight.rgb *= clamp(dotProduct, 0, 1);
	
	if (shadowCoord.x > 0 && shadowCoord.x < 1 && shadowCoord.y > 0 && shadowCoord.y < 1)
	{
		float ourDepth = 1 - (lightingPosition.z / lightingPosition.w);
		totalLight.rgb *= CalcShadowTermPCF(shadowMapSampler, ourDepth, shadowCoord);
		
		/*
		float shadowDepth = tex2D(shadowMapSampler, shadowCoord).r;
		if (shadowDepth + .003 > ourDepth)
		{
			totalLight.rgb *= .4;
		}
		*/
	}
	else
	{
		totalLight.g = totalLight.b = totalLight.r;
	}
	
	//totalLight.rgb += texCUBE(EnvironmentSampler, CalculateEnvironmentReflection(normal, directionFromEye) * float3(1,1,-1)) * fresnel * specular * 0.2;
	
	float2 texCoord = postProjToScreen(input.PositionCopy) + halfPixel();
	totalLight += tex2D(lightSampler, texCoord) * color;

	totalLight.rgb = lerp(totalLight.rgb, FogColor, ComputeFogFactor(length(input.ViewDirection)));

	return totalLight;
}


technique TerrainShading
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
