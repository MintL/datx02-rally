#define MaxLights 10

float4x4 World;
float4x4 View;
float4x4 Projection;
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

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
	float4 TexWeights : TEXCOORD4;
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

    return output;
}

float ComputeFogFactor(float d)
{
	return clamp((d - FogStart) / (FogEnd - FogStart), 0, .75) * FogEnabled;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 normal = normalize(input.Normal);

    float4 color = tex2D(TextureMapSampler0, input.TexCoord) * input.TexWeights.x;
	color += tex2D(TextureMapSampler1, input.TexCoord) * input.TexWeights.y;
	color += tex2D(TextureMapSampler2, input.TexCoord) * input.TexWeights.z;
	color += tex2D(TextureMapSampler3, input.TexCoord) * input.TexWeights.w;
	
	//float4 totalLight = float4(DirectionalAmbient, 1.0) * color;
	float4 totalLight = color;

	// point lights
	for (int i = 0; i < NumLights; i++)
	{
		float3 L = LightPosition[i] - input.WorldPosition;
		float3 directionToLight = normalize(L);

		// point light
		float attenuation = saturate(1 - dot(L / LightRange[i], L / LightRange[i])); 

		// Frazier threshold self shadowing
		float selfShadow = saturate(4.0 * dot(normal, directionToLight));

		totalLight.rgb +=
			attenuation * (LightDiffuse[i] * saturate(dot(normal, directionToLight)));
	}
	float3 directionToLight = -normalize(DirectionalDirection);
	float selfShadow = saturate(4.0 * dot(normal, directionToLight));
	
	totalLight.rgb += selfShadow * (DirectionalDiffuse * color.rgb * saturate(dot(normal, directionToLight)));
	
	totalLight.rgb = lerp(totalLight.rgb, FogColor, ComputeFogFactor(length(input.ViewDirection)));

	return float4(DirectionalAmbient, 1.0) * totalLight;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
