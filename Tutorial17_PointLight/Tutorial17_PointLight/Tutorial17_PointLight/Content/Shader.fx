/*
 * Tutorial
 * XNA Shader programming
 * www.gamecamp.no
 * 
 * by: Petri T. Wilhelmsen
 * e-mail: petriw@gmail.com
 * 
 * Feel free to ask me a question, give feedback or correct mistakes!
 * 
 */


float4x4 matWorldViewProj;	
float4x4 matWorld;	
float4 vecEye;
float4 LightColor;
float3 vecLightPos;
float LightRange;

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
    float4 Light : TEXCOORD1;
    float3 View : TEXCOORD2;
};

OUT VS(float4 Pos : POSITION, float2 Tex : TEXCOORD, float3 N : NORMAL, float3 T : TANGENT, float3 B : BINORMAL)
{
    OUT Out = (OUT)0;      
    Out.Pos = mul(Pos, matWorldViewProj);	// transform Position
    
    // Create tangent space to get normal and light to the same space.
    float3x3 worldToTangentSpace;
    worldToTangentSpace[0] = mul(normalize(T), matWorld);
	worldToTangentSpace[1] = mul(normalize(B), matWorld);
	worldToTangentSpace[2] = mul(normalize(N), matWorld);
        
    // Just pass textures trough
    Out.Tex = Tex;
    
    // Tranform position to world space
    float3 PosWorld = mul(Pos, matWorld);	
    
	// calculate distance to light in world space
	float3 L = vecLightPos - PosWorld;

	// Transform light to tangent space
	Out.Light.xyz = normalize(mul(worldToTangentSpace, L)); 	// L, light

	// Add range to the light, attenuation
	Out.Light.w = saturate( 1 - dot(L / LightRange, L / LightRange));
    
    Out.View = mul(worldToTangentSpace, vecEye - PosWorld);	// V, view
    
   return Out;
}


float4 PS(float2 Tex: TEXCOORD0, float4 L : TEXCOORD1, float3 V : TEXCOORD2) : COLOR
{
    // Get the color from ColorMapSampler using the texture coordinates in Tex.
    float4 Color = tex2D(ColorMapSampler, Tex);	
    
    // Get the Color of the normal. The color describes the direction of the normal vector
    // and make it range from 0 to 1.
    float3 N = (2.0 * (tex2D(NormalMapSampler, Tex))) - 1.0;
 	
 	// Get light direction/view from vertex shader output
    float3 LightDir = normalize(L.xyz);	// L
    float3 ViewDir = normalize(V);	// V
        
	// diffuse
    float D = saturate(dot(N, LightDir));
    
    // Self shadow - used to avoid light artifacts
    float Shadow = saturate(4.0 * LightDir.z);
        
    // reflection
    float3 R = normalize(2 * D * N - LightDir);  // R
 	
    // specular
    float S = min(pow(saturate(dot(R, ViewDir)), 3), Color.w);

    // calculate point light:
    // Ambient +  Shadow*((Diffuse + Specular)*Attenuation in L.w);
    return 0.2 * Color + Shadow*((Color * D  * LightColor + S*LightColor) * (L.w));	
}

technique PointLight
{
    pass P0
    {
        // Setup samplers
        Sampler[0] = (ColorMapSampler);
        Sampler[1] = (NormalMapSampler);		

        // Compile shaders. Pixelshader is 2.0
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
