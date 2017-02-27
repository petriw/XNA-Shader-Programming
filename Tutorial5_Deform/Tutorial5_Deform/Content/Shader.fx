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

float g_fTime;


struct OUT
{
    float4 Pos  : POSITION;
    float3 L : TEXCOORD0;
    float3 N : TEXCOORD1;
    float3 V : TEXCOORD2;
};


OUT VS(float4 Pos : POSITION, float3 N : NORMAL)
{
    
    OUT Out = (OUT)0;      
   
   
    // Just some random sin/cos equation to make things look random 
    float angle=(g_fTime%360)*2;
    float freqx = 1.0f+sin(g_fTime)*4.0f;
    float freqy = 1.0f+sin(g_fTime*1.3f)*4.0f;
    float freqz = 1.0f+sin(g_fTime*1.1f)*4.0f;
    float amp = 1.0f+sin(g_fTime*1.4)*30.0f;
    
    float f = sin(N.x*freqx + g_fTime) * sin(N.y*freqy + g_fTime) * sin(N.z*freqz + g_fTime);
    Pos.z += N.z * freqz * amp * f;
    Pos.x += N.x * freqx* amp * f;
    Pos.y += N.y * freqy* amp * f;
    
    Out.Pos = mul(Pos, matWorldViewProj);	
    Out.N = mul(N, matWorld);				
    
    float4 PosWorld = mul(Pos, matWorld);	
    
    Out.L = vecLightDir;
    Out.V = vecEye - PosWorld;
    
   return Out;
}


float4 PS(float3 L: TEXCOORD0, float3 N : TEXCOORD1, 
			float3 V : TEXCOORD2) : COLOR
{   
    float3 Normal = normalize(N);
    float3 LightDir = normalize(L);
    float3 ViewDir = normalize(V);    
    
    float Diff = saturate(dot(Normal, LightDir)); 
    
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * Diff * Normal - LightDir);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), 15); // R.V^n

    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    return vAmbient + vDiffuseColor * Diff + vSpecularColor * Specular; 
}


technique DeformObjects
{
    pass P0
    {
        // compile shaders
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
