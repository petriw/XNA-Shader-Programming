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

// The blur amount( how far away from our texel will we look up neighbour texels? )
float BlurDistance = 0.003f;

// This will use the texture bound to the object( like from the sprite batch ).
sampler ColorMapSampler : register(s0);

float4 PixelShader(float2 Tex: TEXCOORD0) : COLOR
{
	float4 Color;
	
	// Get the texel from ColorMapSampler using a modified texture coordinate. This
	// gets the texels at the neighbour texels and adds it to Color.
	Color  = tex2D( ColorMapSampler, float2(Tex.x+BlurDistance, Tex.y+BlurDistance));
	Color += tex2D( ColorMapSampler, float2(Tex.x-BlurDistance, Tex.y-BlurDistance));
	Color += tex2D( ColorMapSampler, float2(Tex.x+BlurDistance, Tex.y-BlurDistance));
	Color += tex2D( ColorMapSampler, float2(Tex.x-BlurDistance, Tex.y+BlurDistance));

	// We need to devide the color with the amount of times we added
	// a color to it, in this case 4, to get the avg. color
	Color = Color / 4;	
	
	// returned the blured color
    return Color;
}

technique PostProcessBlur
{
	pass P0
	{
		// A post process shader only needs a pixel shader.
		PixelShader = compile ps_2_0 PixelShader();
	}
}