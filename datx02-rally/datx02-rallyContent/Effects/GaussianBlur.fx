#define RADIUS 7
#define KERNEL_SIZE (RADIUS * 2 + 1)

sampler2D TextureSampler : register(s0);
float Weights[KERNEL_SIZE];
float2 Offsets[KERNEL_SIZE];

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = float4(0, 0, 0, 0);
	
	for (int i=0; i<KERNEL_SIZE; i++)
	{
		color += tex2D(TextureSampler, texCoord + Offsets[i]) * Weights[i];
	}
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
