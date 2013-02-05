float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 NormalMatrix;

float3 DirectionalDirection;
float3 DirectionalAmbient;
float3 DirectionalDiffuse;

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

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 Texture : TEXCOORD;
	float3 Normal : NORMAL;

};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Texture : TEXCOORD0;
	float3 Normal : TEXCOORD1;

};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Texture = input.Texture;
	output.Normal = mul(input.Normal, NormalMatrix);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(ColorMapSampler, input.Texture);
	float4 totalLight = float4(DirectionalAmbient, 1.0) * color;
	totalLight.a = 1 - tex2D(AlphaMapSampler, input.Texture).r;

	float3 normal = normalize(input.Normal);
	float3 directionToLight = -normalize(DirectionalDirection);
	float selfShadow = saturate(4.0 * dot(normal, directionToLight));

	totalLight.rgb += selfShadow * (DirectionalDiffuse * color.rgb * saturate(dot(normal, directionToLight)));
	
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
