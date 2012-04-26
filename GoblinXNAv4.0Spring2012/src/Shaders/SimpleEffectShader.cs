/************************************************************************************ 
 * Copyright (c) 2008-2011, Columbia University
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
 * 
 *************************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;
using GoblinXNA.Helpers;
#if !WINDOWS_PHONE
using GoblinXNA.Graphics.ParticleEffects;
#endif
using GoblinXNA.SceneGraph;

namespace GoblinXNA.Shaders
{
    /// <summary>
    /// An implementation of a simple shader that uses the BasicEffect class.
    /// </summary>
    /// <remarks>
    /// Since BasicEffect class can only include upto three light sources, if more than three light
    /// sources are passed to this class, then the local light sources precede the global light sources.
    /// Both the global and local light nodes are added in the order of encounter in the preorder
    /// tree-traversal of the scene graph. For local lights, the last light node is the closest light node
    /// in the scene graph, so the light sources are added in the reverse order. If there are less than
    /// three local light sources, then global light sources are added in the normal order.
    /// </remarks>
    public class SimpleEffectShader : IShader, IAlphaBlendable
    {
        #region Member Fields

        private BasicEffect basicEffect;
        private List<LightSource> lightSources;
        private Vector3 ambientLight;
        private Dictionary<Effect, float> originalAlphas;
        private BasicEffect internalEffect;
        private bool lightsChanged;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a simple shader to render 3D meshes using the BasicEffect class.
        /// </summary>
        /// <exception cref="GoblinException"></exception>
        public SimpleEffectShader()
        {
            if (!State.Initialized)
                throw new GoblinException("Goblin XNA needs to be initialized first using State.InitGoblin(..)");

            basicEffect = new BasicEffect(State.Device);
            lightSources = new List<LightSource>();
            ambientLight = Vector3.Zero;
            originalAlphas = new Dictionary<Effect, float>();
        }
        #endregion

        #region Properties
        public int MaxLights
        {
            get { return 3; }
        }

        public Effect CurrentEffect
        {
            get { return basicEffect; }
        }

        public Material CurrentMaterial
        {
            get;
            set;
        }

        public bool UseVertexColor
        {
            get { return basicEffect.VertexColorEnabled; }
            set { basicEffect.VertexColorEnabled = value; }
        }

        /// <summary>
        /// Indicates whether to prefer using per-pixel lighting if applicable.
        /// </summary>
        public bool PreferPerPixelLighting
        {
            get { return basicEffect.PreferPerPixelLighting; }
            set { basicEffect.PreferPerPixelLighting = value; }
        }
        #endregion

        #region IAlphaBlendable implementations
        public void SetOriginalAlphas(ModelEffectCollection effectCollection)
        {
            for (int i = 0; i < effectCollection.Count; i++)
                if ((effectCollection[i] is BasicEffect) &&
                    !originalAlphas.ContainsKey(effectCollection[i]))
                    originalAlphas.Add(effectCollection[i], ((BasicEffect)effectCollection[i]).Alpha);
        }
        #endregion

        #region IShader implementations
        public void SetParameters(Material material)
        {
            if (material.InternalEffect != null)
            {
                if (material.InternalEffect is BasicEffect)
                {
                    internalEffect = (BasicEffect)material.InternalEffect;
                    internalEffect.Alpha = originalAlphas[internalEffect] * material.Diffuse.W;

                    if (lightsChanged)
                    {
                        internalEffect.LightingEnabled = basicEffect.LightingEnabled;
                        internalEffect.PreferPerPixelLighting = basicEffect.PreferPerPixelLighting;
                        internalEffect.AmbientLightColor = basicEffect.AmbientLightColor;
                        if (basicEffect.LightingEnabled)
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
                            for (int i = numLightSource; i < MaxLights; i++)
                                lights[i].Enabled = false;
                        }
                    }
                }
                else
                    Log.Write("Passed internal effect is not BasicEffect, so we can not apply the " +
                        "effect to this shader", Log.LogLevel.Warning);
            }
            else
            {
                basicEffect.Alpha = material.Diffuse.W;
                basicEffect.DiffuseColor = Vector3Helper.GetVector3(material.Diffuse);
                basicEffect.SpecularColor = Vector3Helper.GetVector3(material.Specular);
                basicEffect.EmissiveColor = Vector3Helper.GetVector3(material.Emissive);
                basicEffect.SpecularPower = material.SpecularPower;
                basicEffect.TextureEnabled = material.HasTexture;
                basicEffect.Texture = material.Texture;
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
                    source.Direction = lNode.LightSource.TransformedDirection;
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
                    source.Direction = lNode.LightSource.TransformedDirection;
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
                DirectionalLight[] lights = {basicEffect.DirectionalLight0,
                    basicEffect.DirectionalLight1, basicEffect.DirectionalLight2};

                bool atLeastOneLight = false;
                int numLightSource = lightSources.Count;
                for (int i = 0; i < numLightSource; i++)
                {
                    lights[i].Enabled = true;
                    lights[i].DiffuseColor = Vector3Helper.GetVector3(lightSources[i].Diffuse);
                    lights[i].Direction = lightSources[i].Direction;
                    lights[i].SpecularColor = Vector3Helper.GetVector3(lightSources[i].Specular);
                    atLeastOneLight = true;
                }

                basicEffect.LightingEnabled = atLeastOneLight;
            }

            basicEffect.AmbientLightColor = ambientLight;
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
            basicEffect.FogEnabled = environment.FogEnabled;
            if (environment.FogEnabled)
            {
                basicEffect.FogStart = environment.FogStartDistance;
                basicEffect.FogEnd = environment.FogEndDistance;
                basicEffect.FogColor = Vector3Helper.GetVector3(environment.FogColor);
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
            BasicEffect effect = (internalEffect != null) ? internalEffect : basicEffect;
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
            if (basicEffect != null)
                basicEffect.Dispose();
            if (internalEffect != null)
                internalEffect.Dispose();
        }

        #endregion

        #region Private Method

        private void ClearBasicEffectLights()
        {
            basicEffect.DirectionalLight0.Enabled = false;
            basicEffect.DirectionalLight1.Enabled = false;
            basicEffect.DirectionalLight2.Enabled = false;
        }

        #endregion
    }
}
