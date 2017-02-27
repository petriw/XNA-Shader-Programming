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
float4x4	matWorldViewProj;
float4x4	matInverseWorld;
float4		vLightDirection;

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

// The texture that contains the celmap
texture CelMap;
sampler2D CelMapSampler = sampler_state
{
	Texture	  = <CelMap>;
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
};

struct OUT
{
	float4 Pos	: POSITION;
	float2 Tex	: TEXCOORD0;
	float3 L	: TEXCOORD1;
	float3 N	: TEXCOORD2;
};

OUT VertexShader( float4 Pos: POSITION, float2 Tex : TEXCOORD, float3 N: NORMAL )
{
	OUT Out = (OUT) 0;
	Out.Pos = mul(Pos, matWorldViewProj);
	Out.Tex = Tex;
	Out.L = normalize(vLightDirection);
	Out.N = normalize(mul(matInverseWorld, N));
	
	return Out;
}

float4 PixelShader(float2 Tex: TEXCOORD0,float3 L: TEXCOORD1, float3 N: TEXCOORD2) : COLOR
{
	// Calculate normal diffuse light but use Tex.x as color in stead.
	float4 Color = tex2D(ColorMapSampler, Tex);	
	float Ai = 0.8f;
	float4 Ac = float4(0.075, 0.075, 0.2, 1.0);
	float Di = 1.0f;
	float4 Dc = float4(1.0, 1.0, 1.0, 1.0);
	
	Tex.y = 0.0f;
	Tex.x = saturate(dot(L, N));
	
	float4 CelColor = tex2D(CelMapSampler, Tex);	
	
	return (Ai*Ac*Color)+(Color*Di*CelColor);
}

technique ToonShader
{
	pass P0
	{
		Sampler[0] = (ColorMapSampler);	
		Sampler[1] = (CelMapSampler);	
		
		VertexShader = compile vs_2_0 VertexShader();
		PixelShader = compile ps_2_0 PixelShader();
	}
}