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
    public class AlphaTestShader : IShader
    {
        #region Member Fields

        private AlphaTestEffect alphaEffect;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a simple shader to render 3D meshes using the BasicEffect class.
        /// </summary>
        /// <exception cref="GoblinException"></exception>
        public AlphaTestShader()
        {
            if (!State.Initialized)
                throw new GoblinException("Goblin XNA needs to be initialized first using State.InitGoblin(..)");

            alphaEffect = new AlphaTestEffect(State.Device);
        }
        #endregion

        #region Properties
        public int MaxLights
        {
            get { return 0; }
        }

        public Effect CurrentEffect
        {
            get { return alphaEffect; }
        }

        public Material CurrentMaterial
        {
            get;
            set;
        }

        public bool UseVertexColor
        {
            get { return alphaEffect.VertexColorEnabled; }
            set { alphaEffect.VertexColorEnabled = value; }
        }

        public CompareFunction AlphaFunction
        {
            get { return alphaEffect.AlphaFunction; }
            set { alphaEffect.AlphaFunction = value; }
        }

        public int ReferenceAlpha
        {
            get { return alphaEffect.ReferenceAlpha; }
            set { alphaEffect.ReferenceAlpha = value; }
        }
        #endregion

        #region IShader implementations
        public void SetParameters(Material material)
        {
            if (material.InternalEffect != null)
            {
                if (material.InternalEffect is BasicEffect)
                {
                    BasicEffect be = (BasicEffect)material.InternalEffect;

                    alphaEffect.Alpha = be.Alpha;
                    alphaEffect.DiffuseColor = be.DiffuseColor;
                    alphaEffect.Texture = be.Texture;
                    alphaEffect.VertexColorEnabled = be.VertexColorEnabled;
                }
                else
                    Log.Write("Passed internal effect is not BasicEffect, so we can not apply the " +
                        "effect to this shader", Log.LogLevel.Warning);
            }
            else
            {
                alphaEffect.Alpha = material.Diffuse.W;
                alphaEffect.DiffuseColor = Vector3Helper.GetVector3(material.Diffuse);
                alphaEffect.Texture = material.Texture;
            }
        }

        public void SetParameters(List<LightNode> globalLights, List<LightNode> localLights)
        {
            // AlphaTestEffect does not use Lighting information
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
            alphaEffect.FogEnabled = environment.FogEnabled;
            if (environment.FogEnabled)
            {
                alphaEffect.FogStart = environment.FogStartDistance;
                alphaEffect.FogEnd = environment.FogEndDistance;
                alphaEffect.FogColor = Vector3Helper.GetVector3(environment.FogColor);
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
            alphaEffect.View = State.ViewMatrix;
            alphaEffect.Projection = State.ProjectionMatrix;
            alphaEffect.World = worldMatrix;

            foreach (EffectPass pass in alphaEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                renderDelegate();
            }
        }

        public void RenderEnd()
        {
        }

        public void Dispose()
        {
            if (alphaEffect != null)
                alphaEffect.Dispose();
        }

        #endregion
    }
}
