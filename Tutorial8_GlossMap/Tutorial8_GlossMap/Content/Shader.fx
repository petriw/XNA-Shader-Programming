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


// Global variables
// Can be accessed from outside the shader, using Effect->Parameters["key"] where key = variable name
float4x4 matWorldViewProj;	
float4x4 matWorld;	
float4 vecLightDir;
float4 vecEye;
float4 vDiffuseColor;
float4 vSpecularColor;
float4 vAmbient;


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

texture GlossMap;
sampler GlossMapSampler = sampler_state
{
   Texture = <GlossMap>;
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
    float3 L : TEXCOORD1;
    float3 N : TEXCOORD2;
    float3 V : TEXCOORD3;
};


OUT VS(float4 Pos : POSITION, float3 N : NORMAL, float2 Tex: TEXCOORD0)
{
    OUT Out = (OUT)0;      
    
    Out.Pos = mul(Pos, matWorldViewProj);	
    Out.N = mul(N, matWorld);				
    
    float4 PosWorld = mul(Pos, matWorld);	
    
    Out.L = vecLightDir;
    Out.V = vecEye - PosWorld;
    Out.Tex = Tex;
    
   return Out;
}


float4 PS(float2 Tex: TEXCOORD0, float3 L: TEXCOORD1, float3 N : TEXCOORD2, 
			float3 V : TEXCOORD3) : COLOR
{   
    float4 GlossMapColor = tex2D(GlossMapSampler, Tex);
    float4 ColorMapColor = tex2D(ColorMapSampler, Tex);
    float3 Normal = normalize(N);
    float3 LightDir = normalize(L);
    float3 ViewDir = normalize(V);    
    
    float Diff = saturate(dot(Normal, LightDir)); 
    
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * Diff * Normal - LightDir);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), 20); // R.V^n

    //The "magic" happens here. Use the glosscolor and multiply it with the specular. The gloss color
    // functions like the intensity of the specular highlight.
    Specular = Specular*GlossMapColor.x;

    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    return vAmbient + vDiffuseColor * ColorMapColor * Diff + vSpecularColor * Specular; 
}


technique SpecularLight
{
    pass P0
    {
		Sampler[0] = (GlossMapSampler);	
		Sampler[1] = (ColorMapSampler);	
		
        // compile shaders
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
