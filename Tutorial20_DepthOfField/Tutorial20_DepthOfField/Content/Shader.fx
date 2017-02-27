/*
 * Tutorial
 * XNA Shader programming
 * www.gamecamp.no
 * 
 * by: Petri T. Wilhelmsen
 * e-mail: petriw@gmail.com
 * 
 * Feel free to ask me a question, give feedback or correct mistakes!
 * See Tutorial 3 for more information about this shader.
 */



///////////////////////////////////////////////////////////
// Specular light shader

// Global variables
// Can be accessed from outside the shader, using Effect->Parameters["key"] where key = variable name
float4x4	matWorldViewProj;
float4x4	matInverseWorld;
float4		vLightDirection;
float4		vecLightDir;
float4		vecEye;
float4		vDiffuseColor;
float4		vSpecularColor;
float4		vAmbient;


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


struct OUT
{
	float4 Pos	: POSITION;
	float2 Tex	: TEXCOORD0;
	float3 L	: TEXCOORD1;
	float3 N	: TEXCOORD2;
	float3 V	: TEXCOORD3;
};

OUT VertexShader( float4 Pos: POSITION, float2 Tex : TEXCOORD, float3 N: NORMAL )
{
	OUT Out = (OUT) 0;
	Out.Pos = mul(Pos, matWorldViewProj);
	Out.Tex = Tex;
	Out.L = normalize(vLightDirection);
	Out.N = normalize(mul(matInverseWorld, N));
	Out.V = vecEye - Pos;
	
	return Out;
}

float4 PixelShader(float2 Tex: TEXCOORD0,float3 L: TEXCOORD1, float3 N: TEXCOORD2, float3 V: TEXCOORD3) : COLOR
{
	float3 ViewDir = normalize(V); 
	
	// Calculate normal diffuse light.
	float4 Color = tex2D(ColorMapSampler, Tex);	
	float Diff = saturate(dot(L, N)); 

	float3 Reflect = normalize(2 * Diff * N - L);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), 32); // R.V^n

    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    return Color*vAmbient + Color*vDiffuseColor * Diff + vSpecularColor * Specular; 

}

///////////////////////////////////////////////////////////
// Depth texture shader

struct OUT_DEPTH
{
	float4 Position : POSITION;
	float Distance : TEXCOORD0;
};

OUT_DEPTH RenderDepthMapVS(float4 vPos: POSITION)
{
	OUT_DEPTH Out;
	// Translate the vertex using matWorldViewProj.
	Out.Position = mul(vPos, matWorldViewProj);
	// Get the distance of the vertex between near and far clipping plane in matWorldViewProj.
	Out.Distance.x = 1-(Out.Position.z/Out.Position.w);	
	
	return Out;
}

float4 RenderDepthMapPS( OUT_DEPTH In ) : COLOR
{ 
    return float4(In.Distance.x,0,0,1);
}

technique EnvironmentShader
{
	pass P0
	{
		VertexShader = compile vs_2_0 VertexShader();
		PixelShader = compile ps_2_0 PixelShader();
	}
}


technique DepthMapShader
{
	pass P0
	{
		ZEnable = TRUE;
		ZWriteEnable = TRUE;
		AlphaBlendEnable = FALSE;
		
        VertexShader = compile vs_2_0 RenderDepthMapVS();
        PixelShader  = compile ps_2_0 RenderDepthMapPS();
	
	}
}