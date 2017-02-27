// XNA 4.0 Shader Programming #2 - Diffuse light

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Light related
float4 AmbientColor;
float AmbientIntensity;

float3 LightDirection;
float4 DiffuseColor;
float DiffuseIntensity;

float4 SpecularColor;
float3 EyePosition;


// The input for the VertexShader
struct VertexShaderInput
{
    float4 Position : POSITION0;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 View : TEXCOORD1;
};

// The VertexShader.
VertexShaderOutput VertexShaderFunction(VertexShaderInput input,float3 Normal : NORMAL)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	float3 normal = normalize(mul(Normal, World));
	output.Normal = normal;
	output.View = normalize(float4(EyePosition,1.0) - worldPosition);

    return output;
}

// The Pixel Shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 normal = float4(input.Normal, 1.0);
	float4 diffuse = saturate(dot(-LightDirection,normal));
	float4 reflect = normalize(2*diffuse*normal-float4(LightDirection,1.0));
	float4 specular = pow(saturate(dot(reflect,input.View)),32);

    return AmbientColor*AmbientIntensity+DiffuseIntensity*DiffuseColor*diffuse+SpecularColor*specular;
}

// Our Techinique
technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
