/*
 * Tutorial
 * XNA Shader programming
 * www.gamecamp.no
 * 
 * by: Petri T. Wilhelmsen
 * e-mail: petriw@gmail.com
 * 
 * Feel free to ask me a question, give feedback or correct mistakes!
 * See Tutorial 2 for more information about this shader.
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
   AddressU  = Mirror;
   AddressV  = Mirror;
};


texture AlphaMap;
sampler AlphaMapSampler = sampler_state
{
   Texture = <AlphaMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Mirror;
   AddressV  = Mirror;
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
	Out.Tex = Tex*5;
	Out.L = normalize(vLightDirection);
	Out.N = normalize(mul(matInverseWorld, N));
	
	return Out;
}

float4 PixelShader(float2 Tex: TEXCOORD0,float3 L: TEXCOORD1, float3 N: TEXCOORD2) : COLOR
{
	// Calculate normal diffuse light.
	float4 Color = tex2D(ColorMapSampler, Tex);	
	float Ai = 0.01f;
	float4 Ac = float4(1.0, 1.0, 1.0, 1.0);
	float Di = 1.0f;
	float4 Dc = float4(1.0, 1.0, 1.0, 1.0);
	float Dd = saturate(dot(L,N));
	
	Color = (Ai*Ac*Color)+(Color*Di*Dd);
	Color.a = tex2D(AlphaMapSampler, Tex).r;
	
	return Color;
}

technique DiffuseShader
{
	pass P0
	{
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha; 
		
		Sampler[0] = (ColorMapSampler);
		Sampler[1] = (AlphaMapSampler);	
		
		VertexShader = compile vs_2_0 VertexShader();
		PixelShader = compile ps_2_0 PixelShader();
	}
}