sampler2D TextureSampler : register(s0);
texture2D ColorMap;
sampler2D ColorMapSampler = sampler_state
{
	Texture = <ColorMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float Threshold = 0.8;
float BloomIntensity = 1.4;
float OriginalIntensity = 1.0;
float BloomSaturation = 1.0;
float OriginalSaturation = 1.0;

float4 BloomFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, texCoord);

    return saturate((color - Threshold) / (1 - Threshold));
}

technique Bloom
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 BloomFunction();
    }
}

float4 AdjustSaturation(float4 color, float saturation)
{
	float grey = dot(color, float3(0.3, 0.59, 0.11));
	return lerp(grey, color, saturation);
}

float4 BloomCombineFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 bloomColor = tex2D(TextureSampler, texCoord);
	float4 originalColor = tex2D(ColorMapSampler, texCoord);

	bloomColor = AdjustSaturation(bloomColor, BloomSaturation) * BloomIntensity;
	originalColor = AdjustSaturation(originalColor, OriginalSaturation) * OriginalIntensity;

	originalColor *= (1 - saturate(bloomColor));

	return originalColor + bloomColor;
}

technique BloomCombine
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 BloomCombineFunction();
	}
}
