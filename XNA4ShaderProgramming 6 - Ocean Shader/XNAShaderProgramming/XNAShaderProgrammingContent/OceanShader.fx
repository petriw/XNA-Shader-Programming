// XNA 4.0 Shader Programming #4 - Normal Mapping

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

texture2D ColorMap;
sampler2D ColorMapSampler = sampler_state
{
	Texture = <ColorMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state
{
	Texture = <NormalMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

// The input for the VertexShader
struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : NORMAL0;
	float3 Binormal : BINORMAL0;
	float3 Tangent : TANGENT0;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 View : TEXCOORD1;
	float3x3 WorldToTangentSpace : TEXCOORD2;
};

// The VertexShader.
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	input.Position.z += sin((TotalTime*16)+(input.Position.y/1))/16;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;

	output.WorldToTangentSpace[0] = mul(normalize(input.Tangent), World);
	output.WorldToTangentSpace[1] = mul(normalize(input.Binormal), World);
	output.WorldToTangentSpace[2] = mul(normalize(input.Normal), World);
	
	output.View = normalize(float4(EyePosition,1.0) - worldPosition);

    return output;
}

// The Pixel Shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	input.TexCoord.x = input.TexCoord.x*20.0f + sin(TotalTime*3+10)/256;
	input.TexCoord.y = input.TexCoord.y*20.0f;

	float4 color = tex2D(ColorMapSampler, input.TexCoord);

	input.TexCoord.y += (sin(TotalTime*3+10)/256)+(TotalTime/16);
	float3 normalMap = 2.0 *(tex2D(NormalMapSampler, input.TexCoord)) - 1.0;

	input.TexCoord.y -= ((sin(TotalTime*3+10)/256)+(TotalTime/16))*2;
    float3 normalMap2 =(2 * (tex2D(NormalMapSampler, input.TexCoord))) - 1.0; 

	normalMap = (normalMap + normalMap2) / 2;

	normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
	float4 normal = float4(normalMap,1.0);

	float4 diffuse = saturate(dot(-LightDirection,normal));
	float4 reflect = normalize(2*diffuse*normal-float4(LightDirection,1.0));
	float4 specular = pow(saturate(dot(reflect,input.View)),28);

    float4 finalColor =  color * AmbientIntensity + 
			color * DiffuseIntensity * diffuse + 
			specular*250;

	finalColor.a = 0.3f;
	return finalColor;
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
