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
float4 vecLightDir;
float4 vecEye;

float A = 0.2f;

texture ColorMap;
sampler ColorMapSampler = sampler_state
{
   Texture = <ColorMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};

texture NormalMap;
sampler NormalMapSampler = sampler_state
{
   Texture = <NormalMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};

struct OUT
{
    float4 Pos  : POSITION;
    float2 Tex : TEXCOORD0;
    float3 Light : TEXCOORD1;
    float3 View : TEXCOORD2;
};


OUT VS(float4 Pos : POSITION, float2 Tex : TEXCOORD, float3 N : NORMAL, float3 T : TANGENT  )
{
    OUT Out = (OUT)0;      
    Out.Pos = mul(Pos, matWorldViewProj);	// transform Position
    
    float3x3 worldToTangentSpace;
    worldToTangentSpace[0] = mul(T, matWorld);
    worldToTangentSpace[1] = mul(cross(T, N), matWorld);
    worldToTangentSpace[2] = mul(N, matWorld);
        
    Out.Tex = Tex;
    
    float4 PosWorld = mul(Pos, matWorld);	
    
    Out.Light = mul(worldToTangentSpace, vecLightDir); 	// L
    Out.View = mul(worldToTangentSpace, vecEye - PosWorld);	// V
    
   return Out;
}


float4 PS(float2 Tex: TEXCOORD0, float3 L : TEXCOORD1, float3 V : TEXCOORD2) : COLOR
{
	// fargen på piksel, basert på teksturen som ligger i ColorMap
    float4 Color = tex2D(ColorMapSampler, Tex);	
    
    // Fargen på piksel, basert på teksturen som ligger i NormalMap. Denne forteller hvilken retning pikselen har sin "overflate" mot.
    float3 N =(2 * (tex2D(NormalMapSampler, Tex)))- 1.0; 
 	
 	
    float3 LightDir = normalize(L);	// L
    float3 ViewDir = normalize(V);	// V
        
	// diffuse
    float D = saturate(dot(N, LightDir)); 
    
    // reflection
    float3 R = normalize(2 * D * N - LightDir);  // R
 	
 	// specular
    // bruker normalmappen en alpha kan denne brukes til å redusere hvor mye den gitte pikelen skal reflektere
    float S = min(pow(saturate(dot(R, ViewDir)), 3), Color.w);
    
    bool spec = false;
    if(!spec)
		S = 0.0f;

    return A * Color + Color * D + S;	
}


technique NormalMapping
{
    pass P0
    {
        Sampler[0] = (ColorMapSampler);		
        Sampler[1] = (NormalMapSampler);		

        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
