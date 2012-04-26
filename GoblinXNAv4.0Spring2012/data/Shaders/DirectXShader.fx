//structs shared between all effects
struct Light 
{
    float4 color;
    float3 position;
    float3 direction;
    float falloff;
    float range;
    float attenuation0;
    float attenuation1;
    float attenuation2;
    float innerConeAngle;
    float outerConeAngle;
    int type; // 0: directional; 1: point; 2: spot;
    
};

Light lights[12];
Light light;
int numberOfLights;


//shared scene parameters
shared float4x4 viewProjection;
shared float3 cameraPosition;
shared float4 ambientLightColor;


//the world paramter is not shared because it will
//change on every Draw() call
float4x4 world;
float4x4 worldForNormal;


//these material paramters are set per effect instance
float4 emissiveColor;
float4 diffuseColor;
float4 normalMapColor;
float4 specularColor;
float specularPower;
float specularIntensity;


///////////////////////////////////////////////////////////////////
///////////////////////    Shared    //////////////////////////////
///////////////////////////////////////////////////////////////////


//This function calculates the diffuse and specular effect of a single light
//on a pixel given the world position, normal, and material properties
float4 CalculateSinglePointLight(Light light, float3 worldPosition, float3 worldNormal, 
                            float4 specularColor, float4 compoundDiffuseColor)
{    
	float3 lightVector = light.position - worldPosition;
	float lightDist = length(lightVector);
	float3 directionToLight = normalize(lightVector);
    
	float distanceAttenuation;                            
	distanceAttenuation = 1 / (light.attenuation0 + (light.attenuation1 + light.attenuation2 * lightDist) * lightDist);
		
     //calculate the intensity of the light with exponential falloff
     float baseIntensity = pow(saturate((light.range - lightDist) / light.range),
                                 light.falloff);
     
     baseIntensity *= distanceAttenuation;
     baseIntensity  = saturate(baseIntensity);
     
     float diffuseIntensity = saturate( dot(directionToLight, worldNormal));
     float4 diffuse = diffuseIntensity * light.color * compoundDiffuseColor;

     //calculate Phong components per-pixel
     float3 reflectionVector = normalize(reflect(directionToLight, worldNormal));
     float3 directionToCamera = normalize(cameraPosition - worldPosition);
     
     //calculate specular component
     float4 specular = saturate(light.color * specularColor *
                       pow(saturate(dot(reflectionVector, directionToCamera)), 
                           specularPower));
                           
     return  baseIntensity * (diffuse + specular);
}

float4 CalculateSingleDirectionalLight(Light light, float3 worldPosition, float3 worldNormal, 
									  float4 specularColor, float4 compoundDiffuseColor )
{
     float3 lightVector = -light.direction;
     float3 directionToLight = normalize(lightVector);
     
     float diffuseIntensity = saturate( dot(directionToLight, worldNormal));
     float4 diffuse = diffuseIntensity * light.color * compoundDiffuseColor;

     //calculate Phong components per-pixel
     float3 reflectionVector = normalize(reflect(-directionToLight, worldNormal));
     float3 directionToCamera = normalize(cameraPosition - worldPosition);
     
     //calculate specular component
     float4 specular = saturate(light.color * specularColor *
                       pow(saturate(dot(reflectionVector, directionToCamera)), 
                           specularPower));
                           
     return  diffuse + specular;
}

float4 CalculateSingleSpotLight(Light light, float3 worldPosition, float3 worldNormal, 
                            float4 specularColor, float4 compoundDiffuseColor)
{
    float3 lightVector = light.position - worldPosition;
	float lightDist = length(lightVector);
	float3 directionToLight = normalize(lightVector);
    
	float distanceAttenuation;                            
	distanceAttenuation = 1 / (light.attenuation0 + (light.attenuation1 + light.attenuation2 * lightDist) * lightDist);
     
     float innerCos = cos(light.innerConeAngle / 2);
     float outerCos = cos(light.outerConeAngle / 2);
     float lightDirCos = dot(-directionToLight, normalize(light.direction));
     
     float coneAttenuation;
     if (lightDirCos > innerCos)
     {
		coneAttenuation = 1;
     }
     else if (lightDirCos < outerCos)
     {
		coneAttenuation = 0;
     }
     else
     {
		coneAttenuation = pow((lightDirCos - outerCos) / (innerCos - outerCos), light.falloff);
     }
     
     float diffuseIntensity = saturate( dot(directionToLight, worldNormal));
     float4 diffuse = diffuseIntensity * light.color * compoundDiffuseColor;

     //calculate Phong components per-pixel
     float3 reflectionVector = normalize(reflect(-directionToLight, worldNormal));
     float3 directionToCamera = normalize(cameraPosition - worldPosition);
     
     //calculate specular component
     float4 specular = saturate(light.color * specularColor *
                       pow(saturate(dot(reflectionVector, directionToCamera)), 
                           specularPower));
                           
	//return coneAttenuation;
     return  distanceAttenuation * coneAttenuation * (diffuse + specular);
}

/////////////////////////////////////////////////////////////////////////////////
/////////////////////////     No Texture     ////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////

struct VertexShaderOutput
{
     float4 Position : POSITION;
     float3 WorldNormal : TEXCOORD1;
     float3 WorldPosition : TEXCOORD2;
};

struct PixelShaderInput
{
     float3 WorldNormal : TEXCOORD1;
     float3 WorldPosition : TEXCOORD2;
};

//This function transforms the model to projection space and set up
//interpolators used by the pixel shader
VertexShaderOutput BasicVS(
     float3 position : POSITION,
     float3 normal : NORMAL)
{
     VertexShaderOutput output;

     //generate the world-view-projection matrix
     float4x4 wvp = mul(world, viewProjection);
     
     //transform the input position to the output
     output.Position = mul(float4(position, 1.0), wvp);

     output.WorldNormal =  mul(normal, worldForNormal);
     output.WorldNormal = normalize(output.WorldNormal);
     float4 worldPosition =  mul(float4(position, 1.0), world);
     output.WorldPosition = worldPosition / worldPosition.w;

     return output;
}

//The Ambient pixel shader simply adds an ambient color to the
//back buffer while outputting depth information.
float4 AmbientPS(PixelShaderInput input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;

	float4 color = ambientLightColor * compoundDiffuseColor + emissiveColor;
	color.a = diffuseColor.a;
	return color;
}


float4 MultipleDirectionalLightsPS(PixelShaderInput input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	
    float4 color = 0;
	int i = 0;
	
    for (; i< numberOfLights; i++)
    {    
		color += CalculateSingleDirectionalLight(lights[i], 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);
	}
	
	color.a = diffuseColor.a;
	return color;	
}

float4 MultiplePointLightsPS(PixelShaderInput input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;

    float4 color = 0;
	int i = 0;
	
    for (; i< numberOfLights; i++)
    {    
		color += CalculateSinglePointLight(lights[i], 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);
	}
	
	color.a = diffuseColor.a;
	return color;	
}

float4 MultipleSpotLightsPS(PixelShaderInput input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	
    float4 color = 0;
	int i = 0;
	
    for (; i< numberOfLights; i++)
    {    
		color += CalculateSingleSpotLight(lights[i], 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);
	}
	
	color.a = diffuseColor.a;
	return color;	
}

float4 SingleDirectionalLightsPS(PixelShaderInput input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	
    float4 color = 0;
	int i = 0;

	color += CalculateSingleDirectionalLight(light, 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);
	
	color.a = diffuseColor.a;
	return color;	
}

float4 SinglePointLightsPS(PixelShaderInput input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;

    float4 color = 0;
	int i = 0;
		
	color += CalculateSinglePointLight(light, 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);

	color.a = diffuseColor.a;
	return color;	
}

float4 SingleSpotLightsPS(PixelShaderInput input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;

    float4 color = 0;
	int i = 0;
	 
	color += CalculateSingleSpotLight(light, 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);

	color.a = diffuseColor.a;
	return color;	
}


technique GeneralLighting
{
    pass Ambient
    {
        VertexShader = compile vs_2_0 BasicVS();
        PixelShader = compile ps_2_0 AmbientPS();
    }

    pass MultipleDirectionalLight
    {
        VertexShader = compile vs_3_0 BasicVS();
        PixelShader = compile ps_3_0 MultipleDirectionalLightsPS();
    }
    
    pass MultiplePointLight
    {
        VertexShader = compile vs_3_0 BasicVS();
        PixelShader = compile ps_3_0 MultiplePointLightsPS();
    }
    pass MultipleSpotLight
    {
        VertexShader = compile vs_3_0 BasicVS();
        PixelShader = compile ps_3_0 MultipleSpotLightsPS();
    }
    pass SingleDirectionalLight
    {
        VertexShader = compile vs_2_0 BasicVS();
        PixelShader = compile ps_2_0 SingleDirectionalLightsPS();
    }
    
    pass SinglePointLight
    {
        VertexShader = compile vs_2_0 BasicVS();
        PixelShader = compile ps_2_0 SinglePointLightsPS();
    }
    pass SingleSpotLight
    {
        VertexShader = compile vs_2_0 BasicVS();
        PixelShader = compile ps_2_0 SingleSpotLightsPS();
    }
}

///////////////////////////////////////////////////////////////////
///////////////////////    Textured    ////////////////////////////
/////////////////////////////////////////////////////////////////// 

sampler diffuseSampler;

//texture parameters can be used to set states in the 
//effect state pass code
texture2D diffuseTexture;

struct VertexShaderOutputWithTexture
{
     float4 Position : POSITION;
     float2 TexCoords : TEXCOORD0;
     float3 WorldNormal : TEXCOORD1;
     float3 WorldPosition : TEXCOORD2;
};

struct PixelShaderInputWithTexture
{
     float2 TexCoords : TEXCOORD0;
     float3 WorldNormal : TEXCOORD1;
     float3 WorldPosition : TEXCOORD2;
};

//This function transforms the model to projection space and set up
//interpolators used by the pixel shader
VertexShaderOutputWithTexture BasicWithTextureVS(
     float3 position : POSITION,
     float3 normal : NORMAL,
	 float2 texCoord : TEXCOORD0)
{
     VertexShaderOutputWithTexture output;

     //generate the world-view-projection matrix
     float4x4 wvp = mul(world, viewProjection);
     
     //transform the input position to the output
     output.Position = mul(float4(position, 1.0), wvp);

     output.WorldNormal =  mul(normal, worldForNormal);
     output.WorldNormal = normalize(output.WorldNormal);
     float4 worldPosition =  mul(float4(position, 1.0), world);
     output.WorldPosition = worldPosition / worldPosition.w;
     
     //copy the tex coords to the interpolator
     output.TexCoords = texCoord;
	 
     return output;
}

//The Ambient pixel shader simply adds an ambient color to the
//back buffer while outputting depth information.
float4 AmbientWithTexturePS(PixelShaderInputWithTexture input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	compoundDiffuseColor *= tex2D(diffuseSampler, input.TexCoords);

	float4 color = ambientLightColor * compoundDiffuseColor + emissiveColor;
	color.a = diffuseColor.a;
	return color;
}

float4 MultipleDirectionalLightsWithTexturePS(PixelShaderInputWithTexture input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	compoundDiffuseColor *= tex2D(diffuseSampler, input.TexCoords);
	
    float4 color = 0;
	int i = 0;
	
    for (; i< numberOfLights; i++)
    {    
		color += CalculateSingleDirectionalLight(lights[i], 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);
	}
	
	color.a = diffuseColor.a;
	return color;	
}

float4 MultiplePointLightsWithTexturePS(PixelShaderInputWithTexture input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	compoundDiffuseColor *= tex2D(diffuseSampler, input.TexCoords);

    float4 color = 0;
	int i = 0;
	
    for (; i< numberOfLights; i++)
    {    
		color += CalculateSinglePointLight(lights[i], 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);
	}
	
	color.a = diffuseColor.a;
	return color;	
}

float4 MultipleSpotLightsWithTexturePS(PixelShaderInputWithTexture input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	compoundDiffuseColor *= tex2D(diffuseSampler, input.TexCoords);
	
    float4 color = 0;
	int i = 0;
	
    for (; i< numberOfLights; i++)
    {    
		color += CalculateSingleSpotLight(lights[i], 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);
	}
	
	color.a = diffuseColor.a;
	return color;	
}

float4 SingleDirectionalLightsWithTexturePS(PixelShaderInputWithTexture input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	compoundDiffuseColor *= tex2D(diffuseSampler, input.TexCoords);
	
    float4 color = 0;
	int i = 0;

	color += CalculateSingleDirectionalLight(light, 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);
	
	color.a = diffuseColor.a;
	return color;	
}

float4 SinglePointLightsWithTexturePS(PixelShaderInputWithTexture input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	compoundDiffuseColor *= tex2D(diffuseSampler, input.TexCoords);

    float4 color = 0;
	int i = 0;
		
	color += CalculateSinglePointLight(light, 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);

	color.a = diffuseColor.a;
	return color;	
}

float4 SingleSpotLightsWithTexturePS(PixelShaderInputWithTexture input) : COLOR
{
	float4 compoundDiffuseColor = diffuseColor;
	compoundDiffuseColor *= tex2D(diffuseSampler, input.TexCoords);

    float4 color = 0;
	int i = 0;
	 
	color += CalculateSingleSpotLight(light, 
						 input.WorldPosition, input.WorldNormal,
						specularColor, compoundDiffuseColor);

	color.a = diffuseColor.a;
	return color;	
}

technique GeneralLightingWithTexture
{
    pass Ambient
    {
        //set sampler states
        MagFilter[0] = LINEAR;
        MinFilter[0] = LINEAR;
        MipFilter[0] = LINEAR;
        AddressU[0] = WRAP;
        AddressV[0] = WRAP;
        MagFilter[1] = LINEAR;
        MinFilter[1] = LINEAR;
        MipFilter[1] = LINEAR;
        AddressU[1] = WRAP;
        AddressV[1] = WRAP;
        
        //set texture states (notice the '<' , '>' brackets)
        //as the texture state assigns a reference
        Texture[0] = <diffuseTexture>;
       
        
        VertexShader = compile vs_2_0 BasicWithTextureVS();
        PixelShader = compile ps_2_0 AmbientWithTexturePS();
    }

    pass MultipleDirectionalLight
    {
        VertexShader = compile vs_3_0 BasicWithTextureVS();
       PixelShader = compile ps_3_0 MultipleDirectionalLightsWithTexturePS();
    }
    
    pass MultiplePointLight
    {
        VertexShader = compile vs_3_0 BasicWithTextureVS();
        PixelShader = compile ps_3_0 MultiplePointLightsWithTexturePS();
    }
    pass MultipleSpotLight
    {
        VertexShader = compile vs_3_0 BasicWithTextureVS();
        PixelShader = compile ps_3_0 MultipleSpotLightsWithTexturePS();
    }
    pass SingleDirectionalLight
    {
        VertexShader = compile vs_2_0 BasicWithTextureVS();
       PixelShader = compile ps_2_0 SingleDirectionalLightsWithTexturePS();
    }
    
    pass SinglePointLight
    {
        VertexShader = compile vs_2_0 BasicWithTextureVS();
        PixelShader = compile ps_2_0 SinglePointLightsWithTexturePS();
    }
    pass SingleSpotLight
    {
        VertexShader = compile vs_2_0 BasicWithTextureVS();
        PixelShader = compile ps_2_0 SingleSpotLightsWithTexturePS();
    }
}