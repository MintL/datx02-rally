// This idea is fetched from chapter 27 in the book GPUGems3
// http://http.developer.nvidia.com/GPUGems3/gpugems3_ch27.html
#define MaxSamples 8

float4x4 PreviousViewProjection;
float4x4 ViewProjectionInverse;

int NumSamples = 4;

sampler2D sceneSampler : register(s0);
texture2D DepthTexture;
sampler2D depthSampler = sampler_state
{
	texture = <DepthTexture>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
};

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float zOverW = tex2D(depthSampler, texCoord);
	float4 H = float4(texCoord.x * 2 - 1, (1 - texCoord.y) * 2 - 1, zOverW, 1);
	float4 D = mul(H, ViewProjectionInverse);
	float4 worldPos = D / D.w;

	float4 currentPos = H;
	float4 previousPos = mul(worldPos, PreviousViewProjection);
	previousPos /= previousPos.w;
	float2 velocity = (currentPos - previousPos) / 50.f;

	// Blur
	float4 color = tex2D(sceneSampler, texCoord);
	texCoord += velocity;

	[unroll(MaxSamples)]
	for (int i = 1; i < NumSamples; i++, texCoord += velocity)
	{
		color += tex2D(sceneSampler, texCoord);
	}

	return color / NumSamples;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}