float4x4 World;
float4x4 WorldInverseTranspose;
float4x4 WorldViewProjection;
float4x4 ViewInverse;

float Inflate = 0.1;

float3 GlowColor = {1.0f, 0.9f, 0.3f};

float GlowExponential = 1.3;

struct GlowShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoords : TEXCOORD0;
};

struct GlowShaderOutput
{
	float4 Position	: POSITION0;
    float3 WorldNormal	: TEXCOORD0;
    float3 WorldView	: TEXCOORD1;
};

GlowShaderOutput GlowVertexShader( GlowShaderInput input )
{
    GlowShaderOutput output;
    
    output.WorldNormal = mul(input.Normal,WorldInverseTranspose).xyz;
    float4 Po = float4(input.Position.xyz,1);
    Po += (Inflate*normalize(float4(input.Normal.xyz,0))); // the balloon effect
    float4 Pw = mul(Po,World);
    output.WorldView = normalize(ViewInverse[3].xyz - Pw.xyz);
    output.Position = mul(Po,WorldViewProjection);            

    return output;
}

float4 GlowPixelShader( GlowShaderOutput input ) : COLOR0
{
	float3 Nn = normalize(input.WorldNormal);
    float3 Vn = normalize(input.WorldView);
    float edge = 1.0 - dot(Nn,Vn);
    edge = pow(edge,GlowExponential);
    float3 result = edge * GlowColor.rgb;
    return float4(result,edge);
}

technique Glow
{
    pass GlowPass 
    {
        VertexShader = compile vs_1_1 GlowVertexShader();
        PixelShader = compile ps_2_0 GlowPixelShader();
    }
}