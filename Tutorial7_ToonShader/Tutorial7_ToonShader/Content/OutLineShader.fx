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

// Use these two variables to set the outline properties( thickness and threshold )
float Thickness = 1.5f;
float Threshold = 0.2f;

float getGray(float4 c)
{
    return(dot(c.rgb,((0.33333).xxx)));
}

float4 PixelShader(float2 Tex: TEXCOORD0) : COLOR
{
	float4 Color = tex2D(ColorMapSampler, Tex);	
	float2 QuadScreenSize = float2(800,600);

	float2 ox = float2(Thickness/QuadScreenSize.x,0.0);
    float2 oy = float2(0.0,Thickness/QuadScreenSize.y);
    float2 uv = Tex.xy;
    float2 PP = uv - oy;
    float4 CC = tex2D(ColorMapSampler,PP-ox); float g00 = getGray(CC);
    CC = tex2D(ColorMapSampler,PP);    float g01 = getGray(CC);
    CC = tex2D(ColorMapSampler,PP+ox); float g02 = getGray(CC);
    PP = uv;
    CC = tex2D(ColorMapSampler,PP-ox); float g10 = getGray(CC);
    CC = tex2D(ColorMapSampler,PP);    float g11 = getGray(CC);
    CC = tex2D(ColorMapSampler,PP+ox); float g12 = getGray(CC);
    PP = uv + oy;
    CC = tex2D(ColorMapSampler,PP-ox); float g20 = getGray(CC);
    CC = tex2D(ColorMapSampler,PP);    float g21 = getGray(CC);
    CC = tex2D(ColorMapSampler,PP+ox); float g22 = getGray(CC);
    float K00 = -1;
    float K01 = -2;
    float K02 = -1;
    float K10 = 0;
    float K11 = 0;
    float K12 = 0;
    float K20 = 1;
    float K21 = 2;
    float K22 = 1;
    float sx = 0;
    float sy = 0;
    sx += g00 * K00;
    sx += g01 * K01;
    sx += g02 * K02;
    sx += g10 * K10;
    sx += g11 * K11;
    sx += g12 * K12;
    sx += g20 * K20;
    sx += g21 * K21;
    sx += g22 * K22; 
    sy += g00 * K00;
    sy += g01 * K10;
    sy += g02 * K20;
    sy += g10 * K01;
    sy += g11 * K11;
    sy += g12 * K21;
    sy += g20 * K02;
    sy += g21 * K12;
    sy += g22 * K22; 
    float dist = sqrt(sx*sx+sy*sy);
    float result = 1;
    if (dist>Threshold) { result = 0; }
    
    // The scene will be in black and white, so to render
    // everything normaly, except for the edges, bultiply the
    // edge texture with the scenecolor
    return Color*result.xxxx;

}

technique PostOutline
{
	pass P0
	{
		PixelShader = compile ps_2_0 PixelShader();
	}
}