float4x4 World;
float4x4 ViewProjection;
float4x4 ViewInverse;

// ***** Light properties *****
float3 LightDirection;
float4 LightColor;
float4 AmbientLightColor;
// ****************************

// ***** material properties *****
float4 DiffuseColor;
float4 SpecularColor;

// output from phong specular will be scaled by this amount
float Shininess;

float FresnelBias = 0.5f;
float FresnelPower = 1.5f;
float ReflectionAmount = 1.0f;

// *******************************

texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state
{
    Texture = <NormalMap>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

texture2D Texture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

texture ReflectionCubeTexture;
samplerCUBE ReflectionCubeTextureSampler = sampler_state
{
    Texture = <ReflectionCubeTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

float3x3 ComputeTangentMatrix(float3 tangent, float3 normal)
{
    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace;
    worldToTangentSpace[0] =
        //left handed: mul(cross(tangent, normal), World);
        mul(cross(normal, tangent), World);
    worldToTangentSpace[1] = mul(tangent, World);
    worldToTangentSpace[2] = mul(normal, World);
    return worldToTangentSpace;
}

float3 CalcNormalVector(float3 nor)
{
    return normalize(mul(nor, (float3x3)World));
}

float4 TransformPosition(float3 pos)
{
    return mul(mul(float4(pos.xyz, 1), World), ViewProjection);
}

float3 GetWorldPos(float3 pos)
{
    return mul(float4(pos, 1), World).xyz;
}

float3 GetCameraPos()
{
    return ViewInverse[3].xyz;
}

struct VertexInput
{
    float3 pos      : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 normal   : NORMAL0;
    float3 tangent    : TANGENT0;
};

struct VertexOutput
{
    float4 pos          : POSITION0;
    float2 texCoord		: TEXCOORD0;
    float3 lightVec     : TEXCOORD1;
    float3 viewVec      : TEXCOORD2;
};

struct VertexOutput_WithReflection
{
    float4 pos          : POSITION0;
    float2 texCoord     : TEXCOORD0;
    float3 lightVec     : TEXCOORD1;
    float3 viewVec      : TEXCOORD2;
    float3 cubeTexCoord : TEXCOORD3;
};

VertexOutput VS_NormalMap(VertexInput In)
{
    VertexOutput Out; 
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.texCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();
    float3 worldVertPos = GetWorldPos(In.pos);

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    // For ps_2_0 we don't need to clamp form 0 to 1
    Out.lightVec = normalize(mul(worldToTangentSpace, LightDirection));
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    // And pass everything to the pixel shader
    return Out;
}

float4 PS_NormalMap(VertexOutput In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, In.texCoord);
    float3 normalVector = (2.0 * tex2D(NormalMapSampler, In.texCoord).agb) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Additionally normalize the vectors
    float3 lightVector = In.lightVec;
    float3 viewVector = normalize(In.viewVec);
    // For ps_2_0 we don't need to unpack the vectors to -1 - 1

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    // Specular factor
    float3 reflect = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(reflect, viewVector)), Shininess);

    float4 color = diffuseTexture * (AmbientLightColor +
        bump * (DiffuseColor + spec * SpecularColor));
    color.a = DiffuseColor.a * diffuseTexture.a;
    return color;
}

Technique NormalMapOnly
{
    Pass P0
    {
        VertexShader = compile vs_2_0 VS_NormalMap();
        PixelShader = compile ps_2_0 PS_NormalMap();
    }
}

VertexOutput_WithReflection
    VS_NormalMapWithReflection(VertexInput In)
{
    VertexOutput_WithReflection Out;
    
    float4 worldVertPos = mul(float4(In.pos.xyz, 1), World);
    Out.pos = mul(worldVertPos, ViewProjection);
    
    // Copy texture coordinates for diffuse and normal maps
    Out.texCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    // For ps_2_0 we don't need to clamp form 0 to 1
    Out.lightVec = normalize(mul(worldToTangentSpace, LightDirection));
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    float3 normal = CalcNormalVector(In.normal);
    float3 viewVec = normalize(worldEyePos - worldVertPos);
    float3 R = reflect(-viewVec, normal);
    Out.cubeTexCoord = R;
    
    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function
float4 PS_NormalMapWithReflection(VertexOutput_WithReflection In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, In.texCoord);
    float3 normalVector = (2.0 * tex2D(NormalMapSampler, In.texCoord).agb) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Additionally normalize the vectors
    float3 lightVector = normalize(In.lightVec);
    float3 viewVector = normalize(In.viewVec);
    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    // Specular factor
    float3 ref = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(ref, viewVector)), Shininess);

    // Darken down bump factor on back faces
    float3 ambDiffColor = AmbientLightColor + bump * DiffuseColor;
    float4 color;
    color.rgb = diffuseTexture * ambDiffColor +
        bump * spec * SpecularColor * diffuseTexture.a;
    
    // Reflection
    half3 R = reflect(-viewVector, normalVector);
    R = float3(R.x, R.z, R.y);
    half4 reflColor = texCUBE(ReflectionCubeTextureSampler, R);
    
    // Fresnel
    float3 E = -viewVector;
    float facing = 1.0 - max(dot(E, -normalVector), 0);
    float fresnel = FresnelBias + (1.0-FresnelBias)*pow(facing, FresnelPower);
    color.rgb += reflColor * ReflectionAmount * fresnel * 1.5;
    
    // Apply color
    color.a = diffuseTexture.a * DiffuseColor.a;
    return color;
}

technique NormalMapWithReflection
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS_NormalMapWithReflection();
        PixelShader  = compile ps_2_0 PS_NormalMapWithReflection();
    }
}