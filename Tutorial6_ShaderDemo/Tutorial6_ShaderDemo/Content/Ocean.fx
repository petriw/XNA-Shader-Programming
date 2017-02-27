float4x4 matWorldViewProj;	
float4x4 matWorld;	
float4 vecLightDir;
float4 vecEye;
bool	bSpecular;
float	time;
float	A;

// diffuse color map texture
texture ColorMap;
sampler ColorMapSampler = sampler_state
{
   Texture = <ColorMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Wrap;
   AddressV  = Wrap;
};

// bumpmap texture
texture BumpMap;
sampler BumpMapSampler = sampler_state
{
   Texture = <BumpMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Wrap;
   AddressV  = Wrap;
};

texture EnvMap;
sampler EnvMapSampler = sampler_state
{
   Texture = <EnvMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Wrap;
   AddressV  = Wrap;
};


// definer output
struct VS_OUTPUT
{
    float4 Pos  : POSITION;
    float2 Tex : TEXCOORD0;
    float3 Light : TEXCOORD1;
    float3 View : TEXCOORD2;
};

// Vertex shader
VS_OUTPUT VS(float4 Pos : POSITION, float2 Tex : TEXCOORD, float3 Normal : NORMAL, float3 Tangent : TANGENT  )
{
    VS_OUTPUT Out = (VS_OUTPUT)0;     
    
    Pos.z += sin((time*8)+(Pos.y/4))/16;
     
    Out.Pos = mul(Pos, matWorldViewProj);	// posisjonen til vertex
    
    // worldspace -> tangent space
    float3x3 worldToTangentSpace;
    worldToTangentSpace[0] = mul(Tangent, matWorld);
    worldToTangentSpace[1] = mul(cross(Tangent, Normal), matWorld);
    worldToTangentSpace[2] = mul(Normal, matWorld);
        
    Out.Tex = Tex;
    
    float4 PosWorld = mul(Pos, matWorld);	
    
    Out.Light = mul(worldToTangentSpace, vecLightDir);
    Out.View = mul(worldToTangentSpace, vecEye - PosWorld);
    
   return Out;
}

// Pixel Shader
float4 PS(float2 Tex: TEXCOORD0, float3 Light : TEXCOORD1, float3 View : TEXCOORD2) : COLOR
{
    float4 Ambient = float4(0.2, 0.2, 0.2, 1.0);
    float4 ColorRef = tex2D(EnvMapSampler, Tex);
    
    Tex.y = Tex.y*10.0f + sin(time*3+10)/256;
    Tex.x = Tex.x*10.0f;
    
    // Hent fargen på gjeldende pixel  ( C )
    float4 Color = tex2D(ColorMapSampler, Tex);
    Color = Color;	//(Color + ColorRef)/2;
    
    Tex.y += (sin(time*3+10)/256)+(time/16);
    // hent bumpmap verdien til gjeldende pixel ( N )
    float3 Normal =(2 * (tex2D(BumpMapSampler, Tex))) - 1.0; 
    
    Tex.y -= ((sin(time*3+10)/256)+(time/16))*2;
    float3 Normal2 =(2 * (tex2D(BumpMapSampler, Tex))) - 1.0; 
    
    Normal = (Normal + Normal2)/2;
    
    Tex.y = Tex.y*10.0f + (time/10);
    Tex.x = Tex.x*10.0f + (time/10);
 	
 	// normaliser V og L
 	float3 ViewDir = normalize(View);
    float3 LightDir = normalize(Light);
        
	// D = [ N . V ]
    float Diffuse = saturate(dot(Normal, LightDir));
    
    float4 retColor;
	    
	// R = [ 2 * D * N - L ]
	float3 Reflect = normalize(2 * Diffuse * Normal - LightDir);  // R
 	
	// S = [ R . V ]
	float Specular = pow(saturate(dot(Reflect, ViewDir)), 100);

	// O = [ 0.2 * C + C * D + S ]
	if(bSpecular)
		retColor = A * Color + Color * Diffuse * 1.0f + Specular;	
	else
		retColor = A * Color + Color * Diffuse * 1.0f + Specular;
		

	retColor.a = 0.3f+Specular;
	return retColor;
}

technique OceanEffect
{
    pass P0
    {
        Sampler[0] = (ColorMapSampler);		
        Sampler[1] = (BumpMapSampler);
        Sampler[2] = (EnvMapSampler);
        
    
        // compile shaders
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}