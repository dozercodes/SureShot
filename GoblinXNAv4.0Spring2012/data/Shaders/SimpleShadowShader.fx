//-----------------------------------------------------------------------------
// SimpleShadowShader.fx
//
// Copyright (c) 2008-2011, Columbia University
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Columbia University nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY COLUMBIA UNIVERSITY ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// 
// ===========================================================================
// Author: Janessa Det (jwd2126@columbia.edu)
//         Ohan Oda (ohan@cs.columbia.edu)
// 
//-----------------------------------------------------------------------------

float4x4 World;
float4x4 ViewProjection;
float4x4 LightViewProj;
float3 CameraPosition;

float4 DiffuseColor;
float4 SpecularColor;
float SpecularPower;
bool HasTexture;
bool IsOccluder;

int LightType;
bool CastShadows;
float4 DiffuseLightColor;
float4 SpecularLightColor;
float4 AmbientLightColor;
float3 LightDirection;
float3 LightPosition;
float Atten0;
float Atten1;
float Atten2;
float LightConeAngle;
float LightDecay;

bool FirstPass;

texture Texture : register(s0);
sampler TextureSampler = sampler_state
{
    Texture = (Texture);
};

texture LastLayer : register(s1);
sampler LastLayerSampler = sampler_state
{
    Texture = <LastLayer>;
};

texture ShadowsOnly : register(s2);
sampler ShadowsSampler = sampler_state
{
    Texture = <ShadowsOnly>;
};

//-----------------------------------------------------------------------------
// Technique-specific Data Structures
//-----------------------------------------------------------------------------

struct DrawShadows_VSIn
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct DrawWithShadowMap_VSOut
{
    float4 Position : POSITION0;
	float4 Position2D   : TEXCOORD0;
    float3 Normal   : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
    float4 WorldPos : TEXCOORD3;
	float4 LightViewPosition : TEXCOORD4;
};


//-----------------------------------------------------------------------------
// Helper Methods
//-----------------------------------------------------------------------------

// Decay check helper method for Spot Light
 bool isInCone(float3 spotDir, float3 lightDir)
 {
	float angle = dot(normalize(-spotDir),normalize(lightDir));
	return angle > LightConeAngle;
 }


//-----------------------------------------------------------------------------
// Render Scene With Shadows Shaders
//-----------------------------------------------------------------------------

// Draws the model with shadows
DrawWithShadowMap_VSOut DrawModel_VertexShader(DrawShadows_VSIn input)
{
    DrawWithShadowMap_VSOut Output;

    float4x4 WorldViewProj = mul(World, ViewProjection);
    
    // Transform the models verticies and normal
    Output.Position = mul(input.Position, WorldViewProj);
	Output.Position2D = Output.Position;
    Output.Normal =  normalize(mul(input.Normal, World));
    Output.TexCoord = input.TexCoord;
	Output.LightViewPosition = mul(input.Position, LightViewProj);
    
    // Save the vertices postion in world space
    Output.WorldPos = mul(input.Position, World);
    
    return Output;
}

// Determines the depth of the pixel for the model and checks to see 
// if it is in shadow or not
float4 DrawWithShadowMap_PixelShader(DrawWithShadowMap_VSOut input) : COLOR
{
	float2 ProjectedTexCoords;
	ProjectedTexCoords[0] = input.Position2D.x/input.Position2D.w/2.0f +0.5f;
    ProjectedTexCoords[1] = -input.Position2D.y/input.Position2D.w/2.0f +0.5f;
	float4 shadowColor = (CastShadows) ? tex2D(ShadowsSampler, ProjectedTexCoords) : float4(1,1,1,1);
	float4 diffuse;

	if(IsOccluder)
		diffuse = float4(0, 0, 0, 1 - shadowColor.r);
	else
	{
		// default values pertain to Directional Light settings
		float3 direction = LightDirection;
		float atten = 1.0f;
		float lightPower = 1;
		bool isLit = true;

		// Calculate attributes according to Light Type
		// Point Light and Spot Light
		if(LightType == 0 || LightType == 2) {
			direction = LightPosition - input.WorldPos;

			float dist = pow(input.LightViewPosition.x,2) + pow(input.LightViewPosition.y,2) + pow(input.LightViewPosition.z,2);
			dist = sqrt(dist);
			atten = 1/( Atten0 + Atten1 * dist + Atten2 * dist * dist);
		}
		// Spot Light specific extra parameters (cone angle, delay)
		if(LightType == 2) {
			if(isInCone(LightDirection, direction)) {
				lightPower = pow(dot(normalize(LightDirection),normalize(direction)), LightDecay);
			}
			else {
				lightPower = 0;
			}
		}

		float3 reflectionVector = normalize(reflect(-direction, input.Normal));
		float3 directionToCamera = normalize(CameraPosition - input.WorldPos);
     
		//calculate specular component
		float4 specular = saturate(SpecularLightColor * SpecularColor *
						  pow(saturate(dot(reflectionVector, directionToCamera)), 
							   SpecularPower));

		// Color of the model
		float4 diffuseColor = DiffuseColor;
		if(HasTexture)
			diffuseColor *= tex2D(TextureSampler, input.TexCoord);

		// Intensity based on the direction of the light
		float diffuseIntensity = saturate(dot(direction, input.Normal)) * lightPower * shadowColor.r * atten;
		// Final diffuse color with ambient color added
		diffuse = (diffuseIntensity * DiffuseLightColor + AmbientLightColor) * (diffuseColor + specular);
	}

	float4 lastLayerColor = tex2D(LastLayerSampler, ProjectedTexCoords);

	float4 color;
	if(FirstPass) {
		color = diffuse;
	}
	else {
		color = normalize(diffuse + lastLayerColor) * 2;
	}	

    return color;
}


//-----------------------------------------------------------------------------
// Technique Declarations
//-----------------------------------------------------------------------------

// Technique for drawing with shadows
technique DrawWithShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 DrawModel_VertexShader();
        PixelShader = compile ps_3_0 DrawWithShadowMap_PixelShader();
    }
}
