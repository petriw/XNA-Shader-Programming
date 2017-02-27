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
float4x4 matWorldViewProj;	// The projected and tranformed matrix
float4x4 matWorld;		// The world matrix
float4 vecLightDir;		// Light direction
float4 vecEye;			// Eye position
float4 vDiffuseColor;		// Diffuse color
float4 vSpecularColor;		// Specular color
float4 vAmbient;		// Ambient light


struct OUT
{
    float4 Pos  : POSITION; 	// Position
    float3 L : TEXCOORD0;   	// Light dir
    float3 N : TEXCOORD1;	// Normal
    float3 V : TEXCOORD2;	// View/Eye
};



OUT VS(float4 Pos : POSITION, float3 N : NORMAL)
{
    OUT Out = (OUT)0;      
    
    Out.Pos = mul(Pos, matWorldViewProj);	
    Out.N = mul(N, matWorld);				
    
    // Tranform Pos with matWorld in order to get the correct view vector.
    float4 PosWorld = mul(Pos, matWorld);	
    
    Out.L = vecLightDir;

    // Eye position - vertex position returns the view direction from eye to the vertex.
    Out.V = vecEye - PosWorld;
    
   return Out;
}


float4 PS(float3 L: TEXCOORD0, float3 N : TEXCOORD1, 
			float3 V : TEXCOORD2) : COLOR
{   
    // normalize our vectors.
    float3 Normal = normalize(N);
    float3 LightDir = normalize(L);
    float3 ViewDir = normalize(V);    
    
    // calculate diffuse light
    float Diff = saturate(dot(Normal, LightDir)); 
    
    // Create our reflection shader
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * Diff * Normal - LightDir);  

    // Calculate our specular light
    float Specular = pow(saturate(dot(Reflect, ViewDir)), 20); // R.V^n

    // return our final light equation
    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    return vAmbient + vDiffuseColor * Diff + vSpecularColor * Specular; 
}


technique SpecularLight
{
    pass P0
    {
        // compile shaders
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
