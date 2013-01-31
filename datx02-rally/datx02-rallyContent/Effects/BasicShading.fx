float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 NormalMatrix;

float3 EyePosition;

float3 MaterialAmbient;
float3 MaterialDiffuse;
float3 MaterialSpecular;
float MaterialShininess;

float3 LightPosition;
float3 LightAmbient;
float3 LightDiffuse;
float LightRange;

struct VertexShaderInput
{
    float4 Position : POSITION0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 View : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float3 Normal : NORMAL)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.View = EyePosition - worldPosition.xyz;
	output.WorldPosition = worldPosition.xyz;

	output.Normal = mul(Normal, NormalMatrix);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 normal = normalize(input.Normal);
	float3 L = LightPosition - input.WorldPosition;
	float3 directionToLight = normalize(L);
	float3 directionFromEye = -normalize(input.View);

	float3 diffuse = saturate(dot(normal, directionToLight));
	
	float3 reflect = -directionToLight + normal * (2 * dot(normal, directionToLight));
	float3 specular = pow(saturate(dot(reflect, directionFromEye)), MaterialShininess);
	float normalizationFactor = ((MaterialShininess + 2.0) / 8.0);

	// point light
	float attenuation = saturate(1 - dot(L / LightRange, L / LightRange)); 
	// Frazier threshold selfshadowing
	float selfShadow = saturate(4.0 * dot(normal, directionToLight));

	// Fresnel
	float3 fresnel = MaterialSpecular + (float3(1.0, 1.0, 1.0) - MaterialSpecular) * pow(clamp(1.0 + dot(-directionFromEye, normal),
		0.0, 1.0), 5.0);

	//TODO: Multiple lights
    float3 shading = LightAmbient * MaterialAmbient + 
		attenuation * selfShadow *
		(LightDiffuse * MaterialDiffuse * diffuse + LightDiffuse * fresnel * specular * normalizationFactor);

	return float4(shading, 1.0);
}

technique BasicShading
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
