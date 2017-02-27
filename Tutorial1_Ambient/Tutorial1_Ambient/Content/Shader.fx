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


// Define the data our shader will return. This can be named to whatever you want.
struct OUT
{
	// We need to transform the position of every vertex, and store it in the
	// POSITION register
	float4 Pos: POSITION;
};

// Our vertex shader
OUT VertexShader( float4 Pos: POSITION )
{
	// Define an instance of our OUT structure.
	OUT Out = (OUT) 0;

	// Transfom our vertex position from the input to this shader
	// with matWorldViewProj).
	Out.Pos = mul(Pos, matWorldViewProj);

	// return our instance of the Out structure.
	return Out;
}

// Our pixel shader, returns a float4 and stores it in the COLOR register.
float4 PixelShader() : COLOR
{
	// Set ambient intensity to 80%
	float Ai = 0.8f;

	// Set the color of our ambient light
	float4 Ac = float4(0.85, 0.75, 0.2, 1.0);
	
	// return our color to the COLOR register
	return Ai * Ac;
}


// Crate our AmbientLight technique
technique AmbientLight
{
	// We only need one pass in this shader, and I named it to P0, can be anything you want
	pass P0
	{
		// Compile our vertex shader with vs.1.1 and the function VertexShader()
		VertexShader = compile vs_1_1 VertexShader();

		// Compile our pixel shader with vs.1.1 and the function PixelShader()
		PixelShader = compile ps_1_1 PixelShader();
	}
}