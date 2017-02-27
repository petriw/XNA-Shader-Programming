float4x4 matWorldViewProj;	
float4x4 matWorld;	
float4 vecLightDir;
float4 vecEye;


// Define our texture
texture ColorMap;
// Create a sampler for the ColorMap texture using leanear filtering and clamping
sampler ColorMapSampler = sampler_state
{
   Texture = <ColorMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};

// Define our normal map
texture NormalMap;
// Create a sampler for the NormalMap texture using leanear filtering and clamping
sampler NormalMapSampler = sampler_state
{
   Texture = <NormalMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};

// OUT structure
struct OUT
{
    float4 Pos  : POSITION;
    float2 Tex : TEXCOORD0;
    float3 Light : TEXCOORD1;
    float3 View : TEXCOORD2;
};


OUT VS(float4 Pos : POSITION, float2 Tex : TEXCOORD, float3 N : NORMAL, float3 T : TANGENT, float3 B : BINORMAL)
{
	OUT Out = (OUT)0; 
	Out.Pos = mul(Pos, matWorldViewProj); // transform Position

	// Create tangent space to get normal and light to the same space.
	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(normalize(T), matWorld);
	worldToTangentSpace[1] = mul(normalize(B), matWorld);
	worldToTangentSpace[2] = mul(normalize(N), matWorld);

	// Just pass textures trough
	Out.Tex = Tex;

	float4 PosWorld = mul(Pos, matWorld); 

	// Pass out light and view directions, pre-normalized
	Out.Light = normalize(mul(worldToTangentSpace, vecLightDir));
	Out.View = normalize(mul(worldToTangentSpace, vecEye - PosWorld));

	return Out;
}



float4 PS(float2 Tex: TEXCOORD0, float3 L : TEXCOORD1, float3 V : TEXCOORD2) : COLOR
{
	// Get the color from ColorMapSampler using the texture coordinates in Tex.
	float4 Color = tex2D(ColorMapSampler, Tex); 

	// Get the Color of the normal. The color describes the direction of the normal vector
	// and make it range from 0 to 1.
	float3 N = (2.0 * (tex2D(NormalMapSampler, Tex))) - 1.0;

	// diffuse
	float D = saturate(dot(N, L)); 

	// reflection
	float3 R = normalize(2 * D * N - L);

	// specular
	float S = pow(saturate(dot(R, V)), 2);

	// calculate light (ambient + diffuse + specular)
	const float4 Ambient = float4(0.0, 0.0, 0.0, 1.0);
	return Color*Ambient + Color * D + Color*S; 
}



technique NormalMapping
{
    pass P0
    {
        // Setup samplers
        //Sampler[0] = (ColorMapSampler);
        //Sampler[1] = (NormalMapSampler);		

        // Compile shaders. Pixelshader is 2.0
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
