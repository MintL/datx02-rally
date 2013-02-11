float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;

float3 DirectionalDirection;
float3 DirectionalAmbient;
float3 DirectionalDiffuse;

int FogEnabled = 1;
float3 FogColor = float3(0.1, 0.1, 0.1);
float FogStart = 200;
float FogEnd = 8000;

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

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD;
	float3 Normal : NORMAL;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.ViewDirection = EyePosition - worldPosition.xyz;

	output.TexCoord = input.TexCoord;
    output.Normal = input.Normal;

    return output;
}

float ComputeFogFactor(float d)
{
	return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogEnabled;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = tex2D(ColorMapSampler, input.TexCoord);
	float4 totalLight = float4(DirectionalAmbient, 1.0) * color;

	float3 normal = normalize(input.Normal);
	float3 directionToLight = -normalize(DirectionalDirection);
	float selfShadow = saturate(4.0 * dot(normal, directionToLight));

	totalLight.rgb += selfShadow * (DirectionalDiffuse * color.rgb * saturate(dot(normal, directionToLight)));
	
	totalLight.rgb = lerp(totalLight.rgb, FogColor, ComputeFogFactor(length(input.ViewDirection)));

	return totalLight;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
