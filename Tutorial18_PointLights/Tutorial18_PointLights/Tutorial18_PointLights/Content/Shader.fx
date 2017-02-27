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
float4 LightColor2;
float4 LightColor3;
float3 vecLightPos;
float3 vecLightPos2;
float3 vecLightPos3;
float LightRange;
float LightRange2;
float LightRange3;

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
    float4 Light2 : TEXCOORD3;
    float4 Light3 : TEXCOORD4;
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
	float3 L2 = vecLightPos2 - PosWorld;
	float3 L3 = vecLightPos3 - PosWorld;

	// Transform light to tangent space
	Out.Light.xyz = normalize(mul(worldToTangentSpace, L)); 	// L, light
	Out.Light2.xyz = normalize(mul(worldToTangentSpace, L2)); 	// L2, light
	Out.Light3.xyz = normalize(mul(worldToTangentSpace, L3)); 	// L3, light

	// Add range to the light, attenuation
	Out.Light.w = saturate( 1 - dot(L / LightRange, L / LightRange));
	Out.Light2.w = saturate( 1 - dot(L2 / LightRange2, L2 / LightRange2));
	Out.Light3.w = saturate( 1 - dot(L3 / LightRange3, L3 / LightRange3));
    
    Out.View = mul(worldToTangentSpace, vecEye - PosWorld);	// V, view
    
   return Out;
}


float4 PS(float2 Tex: TEXCOORD0, float4 L : TEXCOORD1, float3 V : TEXCOORD2, float4 L2 : TEXCOORD3, float4 L3 : TEXCOORD4) : COLOR
{
    // Get the color from ColorMapSampler using the texture coordinates in Tex.
    float4 Color = tex2D(ColorMapSampler, Tex);	
    
    // Get the Color of the normal. The color describes the direction of the normal vector
    // and make it range from 0 to 1.
    float3 N =(2 * (tex2D(NormalMapSampler, Tex)))- 0.5; 
 	
 	// Get light direction/view from vertex shader output
    float3 LightDir = normalize(L.xyz);	// L
    float3 LightDir2 = normalize(L2.xyz);	// L2
    float3 LightDir3 = normalize(L3.xyz);	// L3
    float3 ViewDir = normalize(V);	// V
        
	// diffuse
	float D = saturate(dot(N, LightDir)); 
	float D2 = saturate(dot(N, LightDir2)); 
	float D3 = saturate(dot(N, LightDir3)); 

	// Self shadow - used to avoid light artifacts
	float Shadow = saturate(4.0 * LightDir.z);
	float Shadow2 = saturate(4.0 * LightDir2.z);
	float Shadow3 = saturate(4.0 * LightDir3.z);
	    
	// reflection
	float3 R = normalize(2 * D * N - LightDir);  // R
	float3 R2 = normalize(2 * D2 * N - LightDir2);  // R
	float3 R3 = normalize(2 * D3 * N - LightDir3);  // R

	// specular
	float S = min(pow(saturate(dot(R, ViewDir)), 3), Color.w);
	float S2 = min(pow(saturate(dot(R2, ViewDir)), 3), Color.w);
	float S3 = min(pow(saturate(dot(R3, ViewDir)), 3), Color.w);

	// calculate three point lights:
	// Ambient +  Shadow*((Diffuse + Specular)*Attenuation in L.w);
	float4 light1final = Shadow*((Color * D  * LightColor + S*LightColor) * (L.w));
	float4 light2final = Shadow2*((Color * D2  * LightColor2 + S2*LightColor2) * (L2.w));
	float4 light3final = Shadow3*((Color * D3  * LightColor3 + S3*LightColor3) * (L3.w));
	return 0.1 * Color + light1final + light2final + light3final;	
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
