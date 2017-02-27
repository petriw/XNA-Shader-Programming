/*
 * Tutorial
 * XNA Shader programming
 * www.gamecamp.no
 * 
 * by: Petri T. Wilhelmsen
 * e-mail: petriw@gmail.com
 * 
 * Feel free to ask me a question, give feedback or correct mistakes!
 */


// Global variables
float Du = 1.0f;
float C = 12.0f;


// This will use the texture bound to the object( like from the sprite batch ).
sampler ColorMapSampler : register(s0);

texture D1M;
sampler D1MSampler = sampler_state
{
   Texture = <D1M>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};

texture D2M;
sampler D2MSampler = sampler_state
{
   Texture = <D2M>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};

texture BGScene;
sampler BGSceneSampler = sampler_state
{
   Texture = <BGScene>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};

texture Scene;
sampler SceneSampler = sampler_state
{
   Texture = <Scene>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};


// Transmittance
float4 PixelShader(float2 Tex: TEXCOORD0) : COLOR
{
	float4 Color=tex2D(SceneSampler, Tex);
	float4 BGColor=tex2D(BGSceneSampler, Tex);
	float depth1=tex2D(D1MSampler, Tex).r;
	
    return Color;
}

technique PostProcess
{
	pass P0
	{
		// A post process shader only needs a pixel shader.
		PixelShader = compile ps_2_0 PixelShader();
	}
}