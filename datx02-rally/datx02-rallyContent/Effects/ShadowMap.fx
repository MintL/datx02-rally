float4x4 World;
float4x4 View;
float4x4 Projection;

bool AlphaEnabled;

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
	float2 TexCoord : TEXCOORD;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Depth : TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Depth.xy = output.Position.zw;
	output.TexCoord = input.TexCoord;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 depth = float4(0, 0, 0, 1);
	// Depth is stored as distance from camera / far plane distance to get value between 0 and 1
	
	float alpha = 1;
	
	if (AlphaEnabled)
	{
		alpha = 1 - tex2D(AlphaMapSampler, input.TexCoord).r;

		if (alpha < .003)
			return 0;
	}

	depth.r = (1 - (input.Depth.x / input.Depth.y)) * alpha;
	depth.g = depth.b = depth.r;

	return depth;
}

technique Technique1
{
    pass Pass1
    {
		CullMode = none;

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
