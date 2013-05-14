float4x4 World;
float4x4 View;
float4x4 Projection;

float ElapsedTime;

texture SkyboxTexture;
sampler SkyboxSampler = sampler_state
{
	Texture = <SkyboxTexture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

int FogEnabled = 1;
float3 FogColor = float3(0.1, 0.1, 0.1);
float FogFactor = 0.8;

struct VertexShaderInput
{
    float3 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Coordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    //float4 worldPosition = mul(input.Position, World);
    //float4 viewPosition = mul(worldPosition, View);
    //output.Position = mul(viewPosition, Projection);

    // TODO: add your vertex shader code here.

	// Calculate rotation. Using a float3 result, so translation is ignored
    float3 rotatedPosition = mul(input.Position, View);           
    // Calculate projection, moving all vertices to the far clip plane 
    // (w and z both 1.0)
    output.Position = mul(float4(rotatedPosition, 1), Projection).xyww;    

    output.Coordinates = input.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // grab the pixel color value from the skybox cube map
    float4 skyBoxColor = texCUBE(SkyboxSampler, input.Coordinates);

	float size = 1.3;
	float amount = 3.2 + (sin(1.3 * ElapsedTime) + 1) * size; // amount will be in range [1..1+size]
	skyBoxColor = skyBoxColor * amount;

	skyBoxColor.rgb = lerp(skyBoxColor.rgb, FogColor, FogEnabled * FogFactor);

	return skyBoxColor;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.
		
        // We're drawing the inside of a model
        CullMode = None;  
        // We don't want it to obscure objects with a Z < 1
        //ZWriteEnable = false;

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
