float4x4 World;
float4x4 View;
float4x4 Projection;

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

};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Texture : TEXCOORD0;

};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Texture = input.Texture;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 Color = tex2D(ColorMapSampler, input.Texture);	

	Color.a = 1 - tex2D(AlphaMapSampler, input.Texture).r;
	Color.rgb = Color.rgb * Color.a;
	if (Color.a < 0.01) discard;
	
	return Color;
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
