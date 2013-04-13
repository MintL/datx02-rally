#include "SoftShadow.vsi"
#include "Prelight/Shared.vsi"

float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;
float4x4 NormalMatrix;

float3 DirectionalDirection;
float3 DirectionalAmbient;
float3 DirectionalDiffuse;

float TerrainAmbientFactor = 3;
float MaterialShininess = 10;

int FogEnabled = 1;
float3 FogColor = float3(0.1, 0.1, 0.1);
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

texture2D RoadNormalMap;
sampler2D RoadNormalSampler = sampler_state
{
	texture = <RoadNormalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
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
	float3 Binormal : BINORMAL;
	float3 Tangent : TANGENT;
	float2 TexCoord : TEXCOORD0;
	float4 TexWeights : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float4 OriginalPosition : TEXCOORD2;
	float3 ViewDirection : TEXCOORD3;
	float3 WorldPosition : TEXCOORD4;
	float4 TexWeights : TEXCOORD5;
	float4 PositionCopy : TEXCOORD6;
	float3x3 WorldToTangentSpace : TEXCOORD7;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.OriginalPosition = input.Position;

	output.TexCoord = input.TexCoord;
    output.Normal = mul(input.Normal, NormalMatrix);

	output.WorldToTangentSpace[0] = mul(normalize(input.Tangent), World);
	output.WorldToTangentSpace[1] = mul(normalize(input.Binormal), World);
	output.WorldToTangentSpace[2] = mul(normalize(input.Normal), World);

	output.ViewDirection = EyePosition - worldPosition.xyz;

	output.WorldPosition = worldPosition.xyz;
	output.TexWeights = input.TexWeights;
	output.PositionCopy = mul(viewPosition, PrelightProjection);
	

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

float3 CalculateSpecularBlinn(float3 normal, float3 directionToLight, float3 directionFromEye, float shininess)
{
	float3 h = normalize(directionToLight - directionFromEye);
	return pow(saturate(dot(h, normal)), shininess);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 normal = normalize(input.Normal);

	float3 directionFromEye = -normalize(input.ViewDirection);
	float normalizationFactor = normalizationFactor = ((MaterialShininess + 2.0) / 8.0);

    float4 color = tex2D(TextureMapSampler0, input.TexCoord) * input.TexWeights.x;
	color += tex2D(TextureMapSampler1, input.TexCoord) * input.TexWeights.y;
	color += tex2D(TextureMapSampler2, input.TexCoord) * input.TexWeights.z;
	color += tex2D(TextureMapSampler3, input.TexCoord) * input.TexWeights.w;
	
	// Change the normal to the normal map if the pixel is on the road
	if (input.TexWeights.x > 0.5) {
		normal = 2.0 * (tex2D(RoadNormalSampler, input.TexCoord)) - 1.0;
		normal = normalize(mul(normal, input.WorldToTangentSpace));
	}

	float4 totalLight = float4(DirectionalAmbient, 1.0) * color * TerrainAmbientFactor;
	
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
	
	// Normal-shadowing
	//totalLight.rgb *= clamp(dotProduct, 0, 1);

	if (shadowCoord.x > 0 && shadowCoord.x < 1 && shadowCoord.y > 0 && shadowCoord.y < 1)
	{
		float ourDepth = 1 - (lightingPosition.z / lightingPosition.w);
		totalLight.rgb *= .4 + .6 * CalcShadowTermPCF(shadowMapSampler, ourDepth, shadowCoord);
		
		//float shadowDepth = tex2D(shadowMapSampler, shadowCoord).r;
		//if (shadowDepth - 0.01 > ourDepth || clamp(dotProduct, 0, 1) <= .5) //if (shadowDepth - 0.003 > ourDepth)
		//{
			//totalLight.rgb *= .2;
		//}
	}
	else
	{
		//totalLight.r *= 5;
	}
	
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
