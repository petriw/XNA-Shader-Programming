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

float TotalTime;

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

	// Just some random sin/cos equation to make things look random 
    float angle=(TotalTime%360)*2;
    float freqx = 0.4f+sin(TotalTime)*1.0f;
    float freqy = 1.0f+sin(TotalTime*1.3f)*2.0f;
    float freqz = 1.1f+sin(TotalTime*1.1f)*3.0f;
    float amp = 1.0f+sin(TotalTime*1.4)*10.0f;
    
    float f = sin(Normal.x*freqx + TotalTime) * sin(Normal.y*freqy + TotalTime) * sin(Normal.z*freqz + TotalTime);
    input.Position.z += Normal.z * freqz * amp * f;
    input.Position.x += Normal.x * freqx* amp * f;
    input.Position.y += Normal.y * freqy* amp * f;

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
