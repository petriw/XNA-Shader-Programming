// This combines the bloom texture with the original scene texture.
// BloomIntensity, OriginalIntensity, BloomSaturation and OriginalSaturation is used
// to control the blooming effect.
// This shader is based on the example in creators.xna.com, where I learned this technique.

// Our bloom texture
sampler BloomSampler : register(s0);

// Our original SceneTexture
texture ColorMap;

// Create a sampler for the ColorMap texture using lianear filtering and clamping
sampler ColorMapSampler = sampler_state
{
   Texture = <ColorMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};

// Controls the Intensity of the bloom texture
float BloomIntensity = 1.3;

// Controls the Intensity of the original scene texture
float OriginalIntensity = 1.0;

// Saturation amount on bloom
float BloomSaturation = 1.0;

// Saturation amount on original scene
float OriginalSaturation = 1.0;


float4 AdjustSaturation(float4 color, float saturation)
{
    // We define gray as the same color we used in the grayscale shader
    float grey = dot(color, float3(0.3, 0.59, 0.11));
    
    return lerp(grey, color, saturation);
}


float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
	// Get our bloom pixel from bloom texture
	float4 bloomColor = tex2D(BloomSampler, texCoord);

	// Get our original pixel from ColorMap
	float4 originalColor = tex2D(ColorMapSampler, texCoord);
    
    // Adjust color saturation and intensity based on the input variables to the shader
	bloomColor = AdjustSaturation(bloomColor, BloomSaturation) * BloomIntensity;
	originalColor = AdjustSaturation(originalColor, OriginalSaturation) * OriginalIntensity;
    
    // make the originalColor darker in very bright areas, avoiding these areas look burned-out
    originalColor *= (1 - saturate(bloomColor));
    
    // Combine the two images.
    return originalColor + bloomColor;
}


technique BloomCombine
{
    pass P0
    {
		// A post process shader only needs a pixel shader.
        PixelShader = compile ps_2_0 PixelShader();
    }
}
