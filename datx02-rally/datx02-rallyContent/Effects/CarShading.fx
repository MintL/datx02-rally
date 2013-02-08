#define MaxLights 10

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 NormalMatrix;

float3 EyePosition;

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

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL;
    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 ViewDirection : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.ViewDirection = EyePosition - worldPosition.xyz;
	output.WorldPosition = worldPosition.xyz;

	output.Normal = mul(input.Normal, NormalMatrix);

    return output;
}

float3 CalculateDiffuse(float3 normal, float3 directionToLight)
{
	return saturate(dot(normal, directionToLight));
}

float3 CalculateSpecular(float3 normal, float3 directionToLight, float3 directionFromEye, float shininess)
{
	float3 reflect = -directionToLight + normal * (2 * dot(normal, directionToLight));
	return pow(saturate(dot(reflect, directionFromEye)), shininess);
}

float3 CalculateSpecularBlinn(float3 normal, float3 directionToLight, float3 directionFromEye, float shininess)
{
	float3 h = normalize(directionToLight - directionFromEye);
	return pow(saturate(dot(h, normal)), shininess);
}

float3 CalculateEnvironmentReflection(float3 normal, float3 directionFromEye) 
{
	//return reflect(directionFromEye, normal);
	return normalize(reflect(directionFromEye, normal));
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 normal = normalize(input.Normal);
	float3 directionFromEye = -normalize(input.ViewDirection);
	float normalizationFactor = ((MaterialShininess + 2.0) / 8.0);
	float3 totalLight = MaterialAmbient * DirectionalLightAmbient;

	for (int i = 0; i < NumLights; i++)
	{
		float3 L = LightPosition[i] - input.WorldPosition;
		float3 directionToLight = normalize(L);

		// point light
		float attenuation = saturate(1 - dot(L / LightRange[i], L / LightRange[i])); 

		// Frazier threshold self shadowing
		float selfShadow = saturate(4.0 * dot(normal, directionToLight));

		// Fresnel
		float3 fresnel = MaterialSpecular + (float3(1.0, 1.0, 1.0) - MaterialSpecular) * pow(clamp(1.0 + dot(-directionFromEye, normal),
			0.0, 1.0), 5.0);

		//totalLight +=
		//	attenuation * selfShadow *
		//	(LightDiffuse[i] * MaterialDiffuse * CalculateDiffuse(normal, directionToLight) + 
		//	LightDiffuse[i] * fresnel * CalculateSpecularBlinn(normal, directionToLight, directionFromEye, MaterialShininess) * normalizationFactor);
	}

	// Directional light
	float3 directionToLight = -normalize(DirectionalLightDirection);
	float selfShadow = saturate(4.0 * dot(normal, directionToLight));
	float3 fresnel = MaterialSpecular + (float3(1.0, 1.0, 1.0) - MaterialSpecular) * pow(clamp(1.0 + dot(-directionFromEye, normal),
			0.0, 1.0), 5.0);
			
	
	//float3 reflection = normalize(2 * saturate(dot(directionFromEye, normal)) * normal - directionFromEye);
	totalLight += //selfShadow * (DirectionalLightDiffuse * MaterialDiffuse * CalculateDiffuse(normal, directionToLight) +
					//DirectionalLightDiffuse * fresnel * CalculateSpecularBlinn(normal, directionToLight, directionFromEye, MaterialShininess) * normalizationFactor +
					texCUBE(EnvironmentSampler, CalculateEnvironmentReflection(normal, directionFromEye));// * MaterialReflection * fresnel);

	return float4(saturate(totalLight), 1.0);
}

technique CarShading
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
