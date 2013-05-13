#include "../SoftShadow.vsi"
#include "../Prelight/Shared.vsi"

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 NormalMatrix;
float3 EyePosition;

float3 MaterialAmbient;
float3 MaterialDiffuse;
float3 MaterialSpecular;

float MaterialReflection;
float MaterialShininess;

float3 DirectionalLightDirection;
float3 DirectionalLightAmbient;
float3 DirectionalLightDiffuse;

texture2D DiffuseMap;
sampler2D DiffuseMapSampler = sampler_state
{
	texture = <DiffuseMap>;
	minfilter = linear;
	magfilter = linear;
	mipfilter = linear;
	AddressU = clamp;
	AddressV = clamp;
};

// Light Map

texture2D LightMap;
sampler2D LightMapSampler = sampler_state
{
	texture = <LightMap>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

// Environment Map

Texture EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
	texture = <EnvironmentMap>;
	magfilter = linear;
	minfilter = linear;
	mipfilter = linear;
	AddressU = mirror;
	AddressV = mirror;
};

// Shadow Map

float4x4 ShadowMapView;
float4x4 ShadowMapProjection;

texture2D ShadowMap;
sampler2D ShadowMapSampler = sampler_state
{
	texture = <ShadowMap>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
	AddressU = clamp;
	AddressV = clamp;
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
	float3 ViewDirection : TEXCOORD2; // World space
	float3 WorldPosition : TEXCOORD3; // World space
	float4 LightMapPosition : TEXCOORD4;
	float4 ModelPosition : TEXCOORD5; // Model space
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);

	output.TexCoord = input.TexCoord;
	output.Normal = mul(input.Normal, NormalMatrix);

	output.ViewDirection = EyePosition - worldPosition.xyz;
	output.WorldPosition = worldPosition.xyz;
	
	output.LightMapPosition = mul(viewPosition, PrelightProjection);
	output.ModelPosition = input.Position;
	
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
	float4x4 wvp = mul(mul(World, ShadowMapView), ShadowMapProjection);
	return mul(position, wvp);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 normal = normalize(input.Normal);
	float4 color = tex2D(DiffuseMapSampler, input.TexCoord);

	float3 directionFromEye = -normalize(input.ViewDirection);
	float normalizationFactor = ((MaterialShininess + 2.0) / 8.0);
	float4 totalLight = float4(MaterialAmbient * DirectionalLightAmbient, 1);

	float4 additionalLight = float4(0,0,0,0);

	// Light Map
	//float2 texCoord = postProjToScreen(input.LightMapPosition) + halfPixel();
	//additionalLight += tex2D(LightMapSampler, texCoord) * color;

	// Directional light
	float3 directionToLight = -normalize(DirectionalLightDirection);
	float selfShadow = saturate(4.0 * dot(normal, directionToLight));
	float3 fresnel = MaterialSpecular + (float3(1.0, 1.0, 1.0) - MaterialSpecular) * pow(clamp(1.0 + dot(-directionFromEye, normal),
			0.0, 1.0), 5.0);
	
	float3 reflection = CalculateEnvironmentReflection(normal, directionFromEye);
	float3 environmentMap = texCUBE(EnvironmentMapSampler, reflection) * fresnel * MaterialReflection;
	
	additionalLight += float4(selfShadow * 
						(MaterialDiffuse * DirectionalLightDiffuse * color.rgb * 
							CalculateDiffuse(normal, directionToLight) +
						DirectionalLightDiffuse * fresnel * 
							CalculateSpecularBlinn(normal, directionToLight, directionFromEye, MaterialShininess) * 
							normalizationFactor + environmentMap ), 1);
	
	additionalLight = saturate(additionalLight);
	
	totalLight += additionalLight;

	/*

	float4 lightingPosition = GetPositionFromLight(input.ModelPosition);
	float2 shadowCoord = 0.5 * lightingPosition.xy / lightingPosition.w;
	
	shadowCoord += 0.5f;
	shadowCoord.y = 1.0f - shadowCoord.y;

	if (shadowCoord.x > 0 && shadowCoord.x < 1 && shadowCoord.y > 0 && shadowCoord.y < 1)
	{
		float ourDepth = 1 - (lightingPosition.z / lightingPosition.w);
		//totalLight.rgb *= .2 + .8 * CalcShadowTermPCF(ShadowMapSampler, ourDepth, shadowCoord);


	}

	*/

	//if (totalLight.r == 0)
	// 	return tex2D(DiffuseMapSampler, input.TexCoord);

	return totalLight;
}

technique Technique1
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
