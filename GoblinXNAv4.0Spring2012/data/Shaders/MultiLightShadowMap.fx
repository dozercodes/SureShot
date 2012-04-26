//-----------------------------------------------------------------------------
// MultiLightShadowMap.fx
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

float DepthBias;

bool FirstPass;

float TexelW;
float TexelH;

float Gauss[11];

texture ShadowsOnly : register(s0);
sampler ShadowsSampler = sampler_state 
{
    Texture = <ShadowsOnly>;
};

texture ShadowMap : register(s1);
sampler ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
};

//-----------------------------------------------------------------------------
// Technique-specific Data Structures
//-----------------------------------------------------------------------------

struct GenerateShadows_VSIn
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct GenerateShadowsOnly_VSOut
{
    float4 Position : POSITION0;
    float3 Normal   : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float4 WorldPos : TEXCOORD2;
	float4 LightViewPosition : TEXCOORD3;
};

struct Gaussian_VSOut
{
    float4 Position	: POSITION;
    float4 Position2D : TEXCOORD0;
};

struct CreateShadowMap_VSOut
{
    float4 Position : POSITION;
    float Depth     : TEXCOORD0;
};

//-----------------------------------------------------------------------------
// Create Shadow Map Shaders
//-----------------------------------------------------------------------------

// Transforms the model into light space an renders out the depth of the object
CreateShadowMap_VSOut CreateShadowMap_VertexShader(float4 Position: POSITION)
{
    CreateShadowMap_VSOut Out;
    Out.Position = mul(Position, mul(World, LightViewProj)); 
    Out.Depth = Out.Position.z / Out.Position.w;    
    return Out;
}

// Saves the depth value out to the 32bit floating point texture
float4 CreateShadowMap_PixelShader(CreateShadowMap_VSOut input) : COLOR
{ 
    return float4(input.Depth, 0, 0, 0);
}

//-----------------------------------------------------------------------------
// Generate Shadows Shaders (with PCF)
//-----------------------------------------------------------------------------

// Draws the model with shadows
GenerateShadowsOnly_VSOut GenerateShadows_VertexShader(GenerateShadows_VSIn input)
{
    GenerateShadowsOnly_VSOut Output;

    float4x4 WorldViewProj = mul(World, ViewProjection);
    
    // Transform the models verticies and normal
    Output.Position = mul(input.Position, WorldViewProj);
    Output.Normal =  normalize(mul(input.Normal, World));
    Output.TexCoord = input.TexCoord;
	Output.LightViewPosition = mul(input.Position, LightViewProj);
    
    // Save the vertices postion in world space
    Output.WorldPos = mul(input.Position, World);
    
    return Output;
}

// Determines the depth of the pixel for the model and checks to see 
// if it is in shadow or not
float4 GenerateShadows_PixelShader(GenerateShadowsOnly_VSOut input) : COLOR
{    
    // Find the position of this pixel in light space
    float4 lightingPosition = mul(input.WorldPos, LightViewProj);
    
    // Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / 
                            lightingPosition.w + float2( 0.5, 0.5 );
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

	float2 coords[9];
	float shadowAvg = 0.0f;

	// 1 2 3
	// 4 0 5
	// 6 7 8
	coords[0] = ShadowTexCoord;
	coords[1] = ShadowTexCoord + float2(-TexelW, -TexelW);
	coords[2] = ShadowTexCoord + float2(0.0f, -TexelW);
	coords[3] = ShadowTexCoord + float2(TexelW, -TexelW);
	coords[4] = ShadowTexCoord + float2(-TexelW, 0.0f);
	coords[5] = ShadowTexCoord + float2(TexelW, 0.0f);
	coords[6] = ShadowTexCoord + float2(-TexelW, TexelW);
	coords[7] = ShadowTexCoord + float2(0.0f, TexelW);
	coords[8] = ShadowTexCoord + float2(TexelW, TexelW);

	for(int i=0; i<9; i++) {
		// Get the current depth stored in the shadow map
		float shadowdepth = tex2D(ShadowMapSampler, coords[i]).r;    
    
		// Calculate the current pixel depth
		// The bias is used to prevent folating point errors that occur when
		// the pixel of the occluder is being drawn
		float ourdepth = (lightingPosition.z / lightingPosition.w) - DepthBias;
    
		// Check to see if this pixel is in front or behind the value in the shadow map
		if (shadowdepth > ourdepth)
		{
			// Shadow the pixel
			shadowAvg += 1.0f;
		};
	}
    
    return shadowAvg / 9.0f;
}

//-----------------------------------------------------------------------------
// Gaussian Blur Shaders
//-----------------------------------------------------------------------------

Gaussian_VSOut Gaussian_VertexShader(float4 Position: POSITION)
{
    Gaussian_VSOut Out;

	float4x4 WorldViewProj = mul(World, ViewProjection);

	Out.Position = mul(Position, WorldViewProj);
    Out.Position2D = Out.Position;

    return Out;
}

float4 Gaussian_PixelShader_H(Gaussian_VSOut input) : COLOR
{ 
    float2 ProjectedShadowCoords;
	ProjectedShadowCoords[0] = input.Position2D.x/input.Position2D.w/2.0f +0.5f;
    ProjectedShadowCoords[1] = -input.Position2D.y/input.Position2D.w/2.0f +0.5f;

    float sum = 0.0f;
    int rad = 3;

    float xindex;
    
		for(int i=-rad; i<=rad; i++)
		{
			xindex = ProjectedShadowCoords[0]+float(i)*TexelW;

			sum += Gauss[i+rad] * tex2D(ShadowsSampler, float2(xindex, ProjectedShadowCoords[1])).r;
		}

    return sum;
}

float4 Gaussian_PixelShader_V(Gaussian_VSOut input) : COLOR
{ 
    float2 ProjectedShadowCoords;
	ProjectedShadowCoords[0] = input.Position2D.x/input.Position2D.w/2.0f +0.5f;
    ProjectedShadowCoords[1] = -input.Position2D.y/input.Position2D.w/2.0f +0.5f;

    float sum = 0.0f;
    int rad = 3;

    float yindex;
    
		for(int i=-rad; i<=rad; i++)
		{
			yindex = ProjectedShadowCoords[1]+float(i)*TexelH;

			sum += Gauss[i+rad] * tex2D(ShadowsSampler, float2(ProjectedShadowCoords[0], yindex)).r;
		}

    return sum;
}

//-----------------------------------------------------------------------------
// Technique Declarations
//-----------------------------------------------------------------------------

// Technique for creating the shadow map
technique CreateShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 CreateShadowMap_VertexShader();
        PixelShader = compile ps_2_0 CreateShadowMap_PixelShader();
    }
}

// Technique for generating shadows
technique GenerateShadows
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 GenerateShadows_VertexShader();
        PixelShader = compile ps_2_0 GenerateShadows_PixelShader();
    }
}

// Techniqus for horizontal and vertical Gaussian filters
technique ApplyGaussianH
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 Gaussian_VertexShader();
        PixelShader = compile ps_2_0 Gaussian_PixelShader_H();
    }
}
technique ApplyGaussianV
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 Gaussian_VertexShader();
        PixelShader = compile ps_2_0 Gaussian_PixelShader_V();
    }
}