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
 * Author: Ohan Oda (ohan@cs.columbia.edu)
 *         Steve Henderson (henderso@cs.columbia.edu)
 *          
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture;

using GoblinXNA;
using GoblinXNA.Graphics;
#if !WINDOWS_PHONE
using GoblinXNA.Graphics.ParticleEffects;
#endif
using GoblinXNA.SceneGraph;
using GoblinXNA.Shaders;
using GoblinXNA.Helpers;

namespace Tutorial14___Skinned_Model_Animation
{
    /// <summary>
    /// An implementation of the IShader interface that works with SkinnedEffect
    /// </summary>
    public class SkinnedModelShader : IShader, IAlphaBlendable
    {
        #region Member Fields

        private SkinnedEffect skinnedEffect;
        private List<LightSource> lightSources;
        private Vector3 ambientLight;
        private float[] originalAlphas;
        private bool originalSet;
        private int alphaIndexer;
        private SkinnedEffect internalEffect;
        private bool lightsChanged;

        #region Temporary Variables

        private Matrix tmpMat1;
        private Matrix tmpMat2;
        private Matrix tmpMat3;
        private Vector3 tmpVec1;

        #endregion

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a simple shader to render 3D meshes using the BasicEffect class.
        /// </summary>
        /// <exception cref="GoblinException"></exception>
        public SkinnedModelShader()
        {
            if (!State.Initialized)
                throw new GoblinException("Goblin XNA needs to be initialized first using State.InitGoblin(..)");

            skinnedEffect = new SkinnedEffect(State.Device);
            lightSources = new List<LightSource>();
            ambientLight = Vector3.Zero;

            originalSet = false;
            alphaIndexer = 0;
        }
        #endregion

        #region Properties
        public int MaxLights
        {
            get { return 3; }
        }

        public Effect CurrentEffect
        {
            get { return skinnedEffect; }
        }

        public Material CurrentMaterial
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether to prefer using per-pixel lighting if applicable.
        /// </summary>
        public bool PreferPerPixelLighting
        {
            get { return skinnedEffect.PreferPerPixelLighting; }
            set { skinnedEffect.PreferPerPixelLighting = value; }
        }
        #endregion

        #region IAlphaBlendable implementations
        public void SetOriginalAlphas(ModelEffectCollection effectCollection)
        {
            if (originalSet)
                return;

            originalAlphas = new float[effectCollection.Count];

            for (int i = 0; i < effectCollection.Count; i++)
                if(effectCollection[i] is SkinnedEffect)
                    originalAlphas[i] = ((SkinnedEffect)effectCollection[i]).Alpha;

            originalSet = true;
        }
        #endregion

        #region IShader implementations
        public void SetParameters(Material material)
        {
            if (material.InternalEffect != null)
            {
                if (material.InternalEffect is SkinnedEffect)
                {
                    internalEffect = (SkinnedEffect)material.InternalEffect;
                    internalEffect.Alpha = originalAlphas[alphaIndexer] * material.Diffuse.W;

                    internalEffect.PreferPerPixelLighting = skinnedEffect.PreferPerPixelLighting;
                    internalEffect.AmbientLightColor = skinnedEffect.AmbientLightColor;

                    if (lightsChanged)
                    {
                        if (lightSources.Count > 0)
                        {
                            DirectionalLight[] lights = {internalEffect.DirectionalLight0,
                            internalEffect.DirectionalLight1, internalEffect.DirectionalLight2};

                            int numLightSource = lightSources.Count;
                            for (int i = 0; i < numLightSource; i++)
                            {
                                lights[i].Enabled = true;
                                lights[i].DiffuseColor = Vector3Helper.GetVector3(lightSources[i].Diffuse);
                                lights[i].Direction = lightSources[i].Direction;
                                lights[i].SpecularColor = Vector3Helper.GetVector3(lightSources[i].Specular);
                            }
                        }
                    }

                    alphaIndexer = (alphaIndexer + 1) % originalAlphas.Length;
                }
                else
                    Log.Write("Passed internal effect is not BasicEffect, so we can not apply the " +
                        "effect to this shader", Log.LogLevel.Warning);
            }
            else
            {
                skinnedEffect.Alpha = material.Diffuse.W;
                skinnedEffect.DiffuseColor = Vector3Helper.GetVector3(material.Diffuse);
                skinnedEffect.SpecularColor = Vector3Helper.GetVector3(material.Specular);
                skinnedEffect.EmissiveColor = Vector3Helper.GetVector3(material.Emissive);
                skinnedEffect.SpecularPower = material.SpecularPower;
                skinnedEffect.Texture = material.Texture;
            }
        }

        public void SetParameters(List<LightNode> globalLights, List<LightNode> localLights)
        {
            bool ambientSet = false;
            ClearBasicEffectLights();
            lightSources.Clear();
            LightNode lNode = null;
            Vector4 ambientLightColor = new Vector4(0, 0, 0, 1);

            // traverse the local lights in reverse order in order to get closest light sources
            // in the scene graph
            for (int i = localLights.Count - 1; i >= 0; i--)
            {
                lNode = localLights[i];
                // only set the ambient light color if it's not set yet and not the default color (0, 0, 0, 1)
                if (!ambientSet && (!lNode.AmbientLightColor.Equals(ambientLightColor)))
                {
                    ambientLightColor = lNode.AmbientLightColor;
                    ambientSet = true;
                }

                if (lightSources.Count < MaxLights)
                {
                    // skip the light source if not enabled or not a directional light
                    if (!lNode.LightSource.Enabled || (lNode.LightSource.Type != LightType.Directional))
                        continue;

                    LightSource source = new LightSource();
                    source.Diffuse = lNode.LightSource.Diffuse;

                    tmpVec1 = lNode.LightSource.Direction;
                    Matrix.CreateTranslation(ref tmpVec1, out tmpMat1);
                    tmpMat2 = lNode.WorldTransformation;
                    MatrixHelper.GetRotationMatrix(ref tmpMat2, out tmpMat2);
                    Matrix.Multiply(ref tmpMat1, ref tmpMat2, out tmpMat3);

                    source.Direction = tmpMat3.Translation;
                    source.Specular = lNode.LightSource.Specular;

                    lightSources.Add(source);

                    // If there are already 3 lights, then skip other lights
                    if (lightSources.Count >= MaxLights)
                        break;
                }
            }

            // Next, traverse the global lights in normal order
            for (int i = 0; i < globalLights.Count; i++)
            {
                lNode = globalLights[i];
                // only set the ambient light color if it's not set yet and not the default color (0, 0, 0, 1)
                if (!ambientSet && (!lNode.AmbientLightColor.Equals(ambientLightColor)))
                {
                    ambientLightColor = lNode.AmbientLightColor;
                    ambientSet = true;
                }

                if (lightSources.Count < MaxLights)
                {
                    // skip the light source if not enabled or not a directional light
                    if (!lNode.LightSource.Enabled || (lNode.LightSource.Type != LightType.Directional))
                        continue;

                    LightSource source = new LightSource();
                    source.Diffuse = lNode.LightSource.Diffuse;

                    tmpVec1 = lNode.LightSource.Direction;
                    Matrix.CreateTranslation(ref tmpVec1, out tmpMat1);
                    tmpMat2 = lNode.WorldTransformation;
                    MatrixHelper.GetRotationMatrix(ref tmpMat2, out tmpMat2);
                    Matrix.Multiply(ref tmpMat1, ref tmpMat2, out tmpMat3);

                    source.Direction = tmpMat3.Translation;
                    source.Specular = lNode.LightSource.Specular;

                    lightSources.Add(source);

                    // If there are already 3 lights, then skip other lights
                    if (lightSources.Count >= MaxLights)
                        break;
                }
            }
            
            ambientLight = Vector3Helper.GetVector3(ambientLightColor);

            if (lightSources.Count > 0)
            {
                DirectionalLight[] lights = {skinnedEffect.DirectionalLight0,
                    skinnedEffect.DirectionalLight1, skinnedEffect.DirectionalLight2};

                int numLightSource = lightSources.Count;
                for (int i = 0; i < numLightSource; i++)
                {
                    lights[i].Enabled = true;
                    lights[i].DiffuseColor = Vector3Helper.GetVector3(lightSources[i].Diffuse);
                    lights[i].Direction = lightSources[i].Direction;
                    lights[i].SpecularColor = Vector3Helper.GetVector3(lightSources[i].Specular);
                }
            }

            skinnedEffect.AmbientLightColor = ambientLight;

            lightsChanged = true;
        }

        /// <summary>
        /// This shader does not support special camera effect.
        /// </summary>
        /// <param name="camera"></param>
        public virtual void SetParameters(CameraNode camera)
        {
        }

        public virtual void SetParameters(GoblinXNA.Graphics.Environment environment)
        {
            skinnedEffect.FogEnabled = environment.FogEnabled;
            if (environment.FogEnabled)
            {
                skinnedEffect.FogStart = environment.FogStartDistance;
                skinnedEffect.FogEnd = environment.FogEndDistance;
                skinnedEffect.FogColor = Vector3Helper.GetVector3(environment.FogColor);
            }
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// This shader does not support particle effect.
        /// </summary>
        /// <param name="particleEffect"></param>
        public void SetParameters(ParticleEffect particleEffect)
        {
        }
#endif

        public void Render(ref Matrix worldMatrix, string techniqueName, RenderHandler renderDelegate)
        {
            SkinnedEffect effect = (internalEffect != null) ? internalEffect : skinnedEffect;
            effect.View = State.ViewMatrix;
            effect.Projection = State.ProjectionMatrix;
            effect.World = worldMatrix;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                renderDelegate();
            }
        }

        public void RenderEnd()
        {
            if (lightsChanged)
                lightsChanged = false;
        }

        public void Dispose()
        {
            if (skinnedEffect != null)
                skinnedEffect.Dispose();
            if (internalEffect != null)
                internalEffect.Dispose();
        }

        #endregion

        #region Private Method

        private void ClearBasicEffectLights()
        {
            skinnedEffect.DirectionalLight0.Enabled = false;
            skinnedEffect.DirectionalLight1.Enabled = false;
            skinnedEffect.DirectionalLight2.Enabled = false;
        }

        #endregion

        #region Public Methods

        public void UpdateBones(Matrix[] updatedBones)
        {
            internalEffect.SetBoneTransforms(updatedBones);
        }

        #endregion
    }
}
