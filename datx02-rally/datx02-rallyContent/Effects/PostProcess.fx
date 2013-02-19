texture2D ColorMap;
sampler2D ColorMapSampler = sampler_state
{
	Texture = <ColorMap>;
};

struct PixelShaderInput
{
    float2 TexCoord : TEXCOORD0;

};

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
    float4 srcColor = tex2D(ColorMapSampler, input.TexCoord);

    return srcColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
