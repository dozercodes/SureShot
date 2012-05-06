/************************************************************************************ 
 * Copyright (c) 2008-2012, Columbia University
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Columbia University nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY COLUMBIA UNIVERSITY ''AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * 
 * ===================================================================================
 * Author: Janessa Det (jwd2126@columbia.edu)
 *         Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using System.IO;

namespace GoblinXNA.Shaders
{
    public class MultiLightShadowMap : IShadowMap
    {
        #region Member Fields

        // The size of the shadow map
        // The larger the size the more detail we will have for our entire scene
        private const int SHADOW_MAP_SIZE = 2048;

        // Gaussian blur radius
        private const int BLUR_RADIUS = 3;

        // The shadow map render target
        private RenderTarget2D shadowMapRenderTarget;
        // The shadow render targets
        private RenderTarget2D[] occluderRenderTargets;
        private RenderTarget2D[] shadowRenderTargets;
        private RenderTarget2D interimOccluderRenderTarget;
        private RenderTarget2D interimShadowRenderTarget;

        private Effect shadowEffect;

        private int textureWidth;
        private int textureHeight;

        private EffectParameter
            world,
            viewProj,
            lightViewProj,
            depthBias,
            shadowsOnly,
            shadowMap;

        // container for generated Gausseian coefficients
        private float[] gaussCoeff;

        private bool applyGaussianBlur;
        private float directionalDepthBias;
        private float pointDepthBias;
        private float spotDepthBias;

        private bool isStereo;

        private List<LightNode> lights;
        private List<GeometryNode> backgroundGeometries;
        private List<GeometryNode> occluderGeometries;

        private Matrix tmpMat1;
        private Matrix tmpMat2;
        private Matrix tmpMat3;

        #endregion

        #region Constructors

        public MultiLightShadowMap() : this(3.0, false) { }

        public MultiLightShadowMap(bool isStereo) : this(3.0, isStereo) { }

        public MultiLightShadowMap(double sigma, bool isStereo)
        {
            if (State.Device.GraphicsProfile != GraphicsProfile.HiDef)
                throw new GoblinException("You need to use HiDef profile in order to use this shadow map");

            this.isStereo = isStereo;

            applyGaussianBlur = true;
            directionalDepthBias = 0.002f;
            pointDepthBias = 0.0001f;
            spotDepthBias = 0.0001f;
            lights = new List<LightNode>();

            // Create floating point render targets
            shadowMapRenderTarget = new RenderTarget2D(State.Device,
                                                    SHADOW_MAP_SIZE,
                                                    SHADOW_MAP_SIZE,
                                                    false,
                                                    SurfaceFormat.Single,
                                                    DepthFormat.Depth24);

            PresentationParameters pp = State.Device.PresentationParameters;

            textureWidth = pp.BackBufferWidth;
            textureHeight = pp.BackBufferHeight;

            if (isStereo)
                textureWidth /= 2;

            interimOccluderRenderTarget = new RenderTarget2D(State.Device,
                                                    textureWidth,
                                                    textureHeight,
                                                    false,
                                                    SurfaceFormat.Single,
                                                    DepthFormat.Depth24);

            interimShadowRenderTarget = new RenderTarget2D(State.Device,
                                                    textureWidth,
                                                    textureHeight,
                                                    false,
                                                    SurfaceFormat.Single,
                                                    DepthFormat.Depth24);

            shadowEffect = State.Content.Load<Effect>(
                System.IO.Path.Combine(State.GetSettingVariable("ShaderDirectory"),
                "MultiLightShadowMap"));

            shadowEffect.Parameters["TexelW"].SetValue(1 / (float)textureWidth);
            shadowEffect.Parameters["TexelH"].SetValue(1 / (float)textureHeight);

            world = shadowEffect.Parameters["World"];
            viewProj = shadowEffect.Parameters["ViewProjection"];
            lightViewProj = shadowEffect.Parameters["LightViewProj"];
            depthBias = shadowEffect.Parameters["DepthBias"];
            shadowMap = shadowEffect.Parameters["ShadowMap"];
            shadowsOnly = shadowEffect.Parameters["ShadowsOnly"];

            GenerateGaussianCoeff(sigma);

            tmpMat1 = Matrix.Identity;
            tmpMat2 = Matrix.Identity;
            tmpMat3 = Matrix.Identity;
        }

        #endregion

        #region Properties

        public int MaxLights
        {
            get { return 10; }
        }

        public Effect CurrentEffect
        {
            get { return shadowEffect; }
        }

        /// <summary>
        /// Indicates whether to apply gaussian blur to the shadow in order to have soft edges.
        /// The default value is true. Set this to false if performance is slow.
        /// </summary>
        public bool ApplyGaussianBlur
        {
            get { return applyGaussianBlur; }
            set { applyGaussianBlur = value; }
        }

        /// <summary>
        /// The default value is 0.002f
        /// </summary>
        /// <remarks>
        /// Custom depth bias values may need to be tweaked for different meshes
        /// </remarks>
        public float DirectionalLightDepthBias
        {
            get { return directionalDepthBias; }
            set { directionalDepthBias = value; }
        }

        /// <summary>
        /// The default value is 0.0001f
        /// </summary>
        /// <remarks>
        /// Custom depth bias values may need to be tweaked for different meshes
        /// </remarks>
        public float PointLightDepthBias
        {
            get { return pointDepthBias; }
            set { pointDepthBias = value; }
        }

        /// <summary>
        /// The default value is 0.0001f
        /// </summary>
        /// <remarks>
        /// Custom depth bias values may need to be tweaked for different meshes
        /// </remarks>
        public float SpotLightDepthBias
        {
            get { return spotDepthBias; }
            set { spotDepthBias = value; }
        }

        public RenderTarget2D[] ShadowRenderTargets
        {
            get { return shadowRenderTargets; }
        }

        public RenderTarget2D[] OccluderRenderTargets
        {
            get { return occluderRenderTargets; }
        }

        #endregion

        #region Public Methods

        public void SetParameters(List<LightNode> shadowLights)
        {
            lights = shadowLights;

            if (lights.Count > 0)
            {
                if (shadowRenderTargets == null || shadowRenderTargets.Length != shadowLights.Count)
                {
                    if (shadowRenderTargets != null)
                    {
                        for (int i = 0; i < shadowRenderTargets.Length; ++i)
                        {
                            shadowRenderTargets[i].Dispose();
                            occluderRenderTargets[i].Dispose();
                        }
                    }

                    shadowRenderTargets = new RenderTarget2D[lights.Count];
                    occluderRenderTargets = new RenderTarget2D[lights.Count];

                    for (int i = 0; i < shadowRenderTargets.Length; ++i)
                    {

                        shadowRenderTargets[i] = new RenderTarget2D(State.Device,
                                                    textureWidth,
                                                    textureHeight,
                                                    false,
                                                    SurfaceFormat.Single,
                                                    DepthFormat.Depth24);

                        occluderRenderTargets[i] = new RenderTarget2D(State.Device,
                                                    textureWidth,
                                                    textureHeight,
                                                    false,
                                                    SurfaceFormat.Single,
                                                    DepthFormat.Depth24);
                    }
                }
            }
        }

        public void PrepareRenderTargets(List<GeometryNode> occluderGeometries, 
            List<GeometryNode> backgroundGeometries)
        {
            this.occluderGeometries = occluderGeometries;
            this.backgroundGeometries = backgroundGeometries;

            BlendState oldBlendState = State.Device.BlendState;
            DepthStencilState oldDepthState = State.Device.DepthStencilState;

            State.Device.BlendState = BlendState.Opaque;
            State.Device.DepthStencilState = DepthStencilState.Default;

            viewProj.SetValue(State.ViewProjectionMatrix);

            for (int i = 0; i < lights.Count; i++)
            {
                // Render the scene to the shadow map
                CreateShadowMap(i);

                // Draw shadows only for processing
                GenerateShadows(i);

                if (applyGaussianBlur)
                {
                    // Do Gaussian Blur
                    ApplyGaussianH(i);
                    ApplyGaussianV(i);
                }

                /*State.Device.SetRenderTarget(null);
                occluderRenderTargets[i].SaveAsJpeg(
                    new FileStream("occluder" + i + ".jpg",
                    FileMode.Create, FileAccess.Write),
                    occluderRenderTargets[i].Width, occluderRenderTargets[i].Height);

                shadowRenderTargets[i].SaveAsJpeg(
                    new FileStream("shadow" + i + ".jpg",
                    FileMode.Create, FileAccess.Write),
                    shadowRenderTargets[i].Width, shadowRenderTargets[i].Height);*/
            }

            State.Device.BlendState = oldBlendState;
            State.Device.DepthStencilState = oldDepthState;
        }

        public void ComputeShadow(ref Matrix mat, RenderHandler renderHandler)
        {
            world.SetValue(mat);

            for (int num = 0; num < shadowEffect.CurrentTechnique.Passes.Count; num++)
            {
                EffectPass pass = shadowEffect.CurrentTechnique.Passes[num];

                pass.Apply();
                renderHandler();
            }
        }

        public void Dispose()
        {
            shadowMapRenderTarget.Dispose();
            interimShadowRenderTarget.Dispose();
            interimOccluderRenderTarget.Dispose();
            if (shadowRenderTargets != null)
                for (int i = 0; i < shadowRenderTargets.Length; ++i)
                    shadowRenderTargets[i].Dispose();
            if (occluderRenderTargets != null)
                for (int i = 0; i < occluderRenderTargets.Length; ++i)
                    occluderRenderTargets[i].Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Pre-generate Gaussian coefficient values
        /// </summary>
        private void GenerateGaussianCoeff(double sigma)
        {
            gaussCoeff = new float[2 * BLUR_RADIUS + 1];
            int r = BLUR_RADIUS;

            float gSum = 0.0f;
            for (int i = -r; i <= r; i++)
            {
                gaussCoeff[i + r] = (float)(1.0 / (Math.Sqrt(2.0 * Math.PI) * sigma) * Math.Exp(-Math.Pow(i, 2.0) / 
                    (2.0 * Math.Pow(sigma, 2.0))));
                gSum += gaussCoeff[i + r];
            }

            // normalize to 1
            for (int k = -r; k <= r; k++)
            {
                gaussCoeff[k + r] /= gSum;
            }

            shadowEffect.Parameters["Gauss"].SetValue(gaussCoeff);
        }

        /// <summary>
        /// Renders the scene to the floating point render target then 
        /// sets the texture for use when drawing the scene.
        /// </summary>
        private void CreateShadowMap(int lightIndex)
        {
            // Set our render target to our floating point render target
            State.Device.SetRenderTarget(shadowMapRenderTarget);

            // Clear the render target to white or all 1's
            // We set the clear to white since that represents the 
            // furthest the object could be away
            State.Device.Clear(Color.White);

            // Draw any occluders 
            for (int i = 0; i < occluderGeometries.Count; i++)
            {
                PrepareShadows(lightIndex, occluderGeometries[i], "CreateShadowMap", true);
            }
        }

        /// <summary>
        /// Draws shadows only using the shadow map
        /// </summary>
        private void GenerateShadows(int lightIndex)
        {
            State.Device.SetRenderTarget(shadowRenderTargets[lightIndex]);
            State.Device.Clear(Color.White);

            State.Device.SamplerStates[0] = SamplerState.PointClamp;

            for (int i = 0; i < backgroundGeometries.Count; i++)
            {
                PrepareShadows(lightIndex, backgroundGeometries[i], "GenerateShadows", false);
            }

            State.Device.SetRenderTarget(occluderRenderTargets[lightIndex]);
            State.Device.Clear(Color.White);

            for (int i = 0; i < occluderGeometries.Count; i++)
            {
                PrepareShadows(lightIndex, occluderGeometries[i], "GenerateShadows", true);
            }
        }

        /// <summary>
        /// Apply Gaussian blur to shadows (horizontal)
        /// </summary>
        private void ApplyGaussianH(int lightIndex)
        {
            State.Device.SetRenderTarget(interimShadowRenderTarget);
            State.Device.Clear(Color.White);

            for (int i = 0; i < backgroundGeometries.Count; i++)
            {
                PrepareShadows(lightIndex, backgroundGeometries[i], "ApplyGaussianH", false);
            }

            State.Device.SetRenderTarget(interimOccluderRenderTarget);
            State.Device.Clear(Color.White);

            for (int i = 0; i < occluderGeometries.Count; i++)
            {
                PrepareShadows(lightIndex, occluderGeometries[i], "ApplyGaussianH", true);
            }
        }
        /// <summary>
        /// Apply Gaussian blur to shadows (vertical)
        /// </summary>
        private void ApplyGaussianV(int lightIndex)
        {
            State.Device.SetRenderTarget(shadowRenderTargets[lightIndex]);
            State.Device.Clear(Color.White);

            for (int i = 0; i < backgroundGeometries.Count; i++)
            {
                PrepareShadows(lightIndex, backgroundGeometries[i], "ApplyGaussianV", false);
            }

            State.Device.SetRenderTarget(occluderRenderTargets[lightIndex]);
            State.Device.Clear(Color.White);

            for (int i = 0; i < occluderGeometries.Count; i++)
            {
                PrepareShadows(lightIndex, occluderGeometries[i], "ApplyGaussianV", true);
            }
        }

        /// <summary>
        /// Helper function to draw shadows with effect
        /// </summary>
        /// <param name="lightIndex"></param>
        /// <param name="geometry">The model to draw</param>
        /// <param name="technique">The technique to use</param>
        /// <param name="occluder"></param>
        private void PrepareShadows(int lightIndex, GeometryNode geometry, string technique, bool occluder)
        {
            LightNode light = lights[lightIndex];

            // Set the current values for the effect
            shadowEffect.CurrentTechnique = shadowEffect.Techniques[technique];

            if (technique == "GenerateShadows")
            {
                shadowMap.SetValue(shadowMapRenderTarget);
            }
            else if (technique == "ApplyGaussianH")
            {
                if (occluder)
                    shadowsOnly.SetValue(occluderRenderTargets[lightIndex]);
                else
                    shadowsOnly.SetValue(shadowRenderTargets[lightIndex]);
            }
            else if (technique == "ApplyGaussianV")
            {
                if (occluder)
                    shadowsOnly.SetValue(interimOccluderRenderTarget);
                else
                    shadowsOnly.SetValue(interimShadowRenderTarget);
            }

            lightViewProj.SetValue(light.LightViewProjection);

            // Light specific values
            if (light.LightSource.Type == LightType.Directional)
                depthBias.SetValue(directionalDepthBias);
            else if (light.LightSource.Type == LightType.Point)
                depthBias.SetValue(pointDepthBias);
            else
                depthBias.SetValue(spotDepthBias);

            tmpMat1 = geometry.WorldTransformation;
            if (geometry.MarkerTransformSet)
            {
                tmpMat2 = geometry.MarkerTransform;
                Matrix.Multiply(ref tmpMat1, ref tmpMat2, out tmpMat3);
                geometry.Model.PrepareShadows(ref tmpMat3);
            }
            else
                geometry.Model.PrepareShadows(ref tmpMat1); 
        }

        #endregion
    }
}
