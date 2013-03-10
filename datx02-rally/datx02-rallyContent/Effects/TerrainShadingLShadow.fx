
float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DirectionalDirection;
float3 DirectionalAmbient;
float3 DirectionalDiffuse;

float4x4 LightView;
float4x4 LightProjection;

texture2D ShadowMap;
sampler2D shadowMapSampler = sampler_state
{
	texture = <ShadowMap>;
	minfilter = point;
	magfilter = point;
	mipfilter = point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture TextureMap;
sampler TextureMapSampler = sampler_state
{
	Texture = <TextureMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float4 OriginalPosition : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition,  View);
    output.Position = mul(viewPosition, Projection);

	output.TexCoord = input.TexCoord;

	output.WorldPosition = worldPosition.xyz;
	output.OriginalPosition = input.Position;

    return output;
}

float4 GetPositionFromLight(float4 position)
{
	float4x4 wvp = mul(mul(World, LightView), LightProjection);
	return mul(position, wvp);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

    float4 color = tex2D(TextureMapSampler, input.TexCoord);
	
	float4 totalLight = float4(DirectionalAmbient, 1.0) * color;
	
	float4 lightingPosition = GetPositionFromLight(input.OriginalPosition);
	float2 shadowCoord = 0.5 * lightingPosition.xy / lightingPosition.w;
	
	shadowCoord += 0.5f;
	shadowCoord.y = 1.0f - shadowCoord.y;
	
	if (shadowCoord.x > 0 && shadowCoord.x < 1 && shadowCoord.y > 0 && shadowCoord.y < 1)
	{
		float ourDepth = 1 - (lightingPosition.z / lightingPosition.w);
		//totalLight.rgb *= CalcShadowTermPCF(shadowMapSampler, ourDepth, shadowCoord);

		float shadowDepth = tex2D(shadowMapSampler, shadowCoord).r;
		if (shadowDepth + .003 > ourDepth)
		{
			totalLight.rgb *= .4;
		}
	}
	else
	{
		totalLight.r *= .3;
		totalLight.g *= .3;
		totalLight.b *= 3;
	}
	
	return totalLight;
}


technique TerrainShading
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
