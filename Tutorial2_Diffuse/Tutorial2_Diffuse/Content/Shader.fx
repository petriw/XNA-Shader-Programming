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
float4x4	matInverseWorld; // For calculating normals
float4		vLightDirection; // Our lights direction

// The OUT structure our Vertex Shader will use.
struct OUT
{
	float4 Pos: POSITION; // Store transformed position  here
	float3 L:	TEXCOORD0; // Store the normalized light direction here
	float3 N:	TEXCOORD1; // Store the transformed and normalized normal here
};

// Our vertex shader, takes the vertex position and vertex normal as input
OUT VertexShader( float4 Pos: POSITION, float3 N: NORMAL )
{
	OUT Out = (OUT) 0;
	Out.Pos = mul(Pos, matWorldViewProj);

	// normalize(a) returns a normalized version of a.
	// in this case, a = vLightDirection
	Out.L = normalize(vLightDirection);

	// transform our normal with matInverseWorld, and normalize it
	Out.N = normalize(mul(matInverseWorld, N));
	
	return Out;
}


// Our pixelshader. Needs the light direction and normal from the vertex shader.
float4 PixelShader(float3 L: TEXCOORD0, float3 N: TEXCOORD1) : COLOR
{
	// Ambient light
	float Ai = 0.8f;
	float4 Ac = float4(0.075, 0.075, 0.2, 1.0);
	
	// Diffuse light
	float Di = 1.0f;
	float4 Dc = float4(1.0, 1.0, 1.0, 1.0);
	
	// return Ambient light * diffuse light. See tutorial if
	// you dont understand this formula
	return Ai * Ac + Di * Dc * saturate(dot(L, N));
}

// our technique
technique DiffuseLight
{
	pass P0
	{
		VertexShader = compile vs_1_1 VertexShader();
		PixelShader = compile ps_1_1 PixelShader();
	}
}