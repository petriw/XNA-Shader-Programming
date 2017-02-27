/*
 * Tutorial
 * XNA Shader programming
 * www.gamecamp.no
 * 
 * by: Petri T. Wilhelmsen
 * e-mail: petriw@gmail.com
 * 
 * Feel free to ask me a question, give feedback or correct mistakes!
 * This shader is mostly based on the shader "post edgeDetect" from nVidias Shader library:
 * http://developer.download.nvidia.com/shaderlibrary/webpages/shader_library.html
 */


// Global variables
// This will use the texture bound to the object( like from the sprite batch ).
sampler ColorMapSampler : register(s0);

// Grayscale
float4 PixelShader(float2 Tex: TEXCOORD0) : COLOR
{
	float4 Color = tex2D(ColorMapSampler, Tex);	
	
	//Color.rgb = (Color.r + Color.g + Color.b)/3;
	Color.rgb = dot(Color.rgb, float3(0.3, 0.59, 0.11));

	// Keep our alphachannel at 1.
	Color.a = 1.0f;
		
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