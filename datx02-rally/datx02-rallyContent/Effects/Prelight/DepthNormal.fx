float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Depth : TEXCOORD0;
	float3 Normal : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.Normal = mul(input.Normal, World);

	// Position's z and w correspond to the distance from camera and distance of the far plane respectively
	output.Depth.xy = output.Position.zw;

    return output;
}

struct PixelShaderOutput
{
	float4 Normal : COLOR0;
	float4 Depth : COLOR1;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;

	output.Depth = float4(0, 0, 0, 1);
	// Depth is stored as distance from camera / far plane distance to get value between 0 and 1
	output.Depth.r = 1-input.Depth.x / (input.Depth.y);

	// Normal map simply stores x, y, and z of normal shifted from (-1 to 1) range to (0 to 1) range
	output.Normal.xyz = (normalize(input.Normal).xyz / 2) + .5;
	
	// The rest to compile
	//output.Depth.a = 1;
	output.Normal.a = 1;

	return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
