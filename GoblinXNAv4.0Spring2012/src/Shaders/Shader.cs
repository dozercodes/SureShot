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
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;
using GoblinXNA.Graphics.ParticleEffects;
using GoblinXNA.SceneGraph;
using GoblinXNA.Helpers;

namespace GoblinXNA.Shaders
{
    /// <summary>
    /// A basic implementation of the IShader interface that doesn't use XNA's BasicEffect class.
    /// </summary>
    /// <remarks>
    /// It's recommended that a new shader class extends this base shader class.
    /// 
    /// None of the SetParameter(..) functions are implemented, so your extended shader class should
    /// implement the features your shader class supports.
    /// </remarks>
    public class Shader : IShader
    {
        #region Member Fields

        protected String shaderName;
        protected Effect effect;
        /// <summary>
        /// Defines some of the commonly used effect parameters in shader files.
        /// </summary>
        protected EffectParameter world,
            worldViewProj,
            viewProj,
            projection,
            viewInverse;

        protected Matrix lastUsedWorldViewProjMatrix;
        protected Matrix lastUsedViewProjMatrix;
        protected Matrix lastUsedProjMatrix;
        protected Matrix lastUsedViewInverseMatrix;

        #endregion

        #region Constructors

        public Shader(String shaderName)
        {
            if (State.Device == null)
                throw new GoblinException(
                    "GoblinXNA device is not initialized, can't create Shader.");

            Reload(shaderName);
        }

        #endregion

        #region Properties
        public virtual int MaxLights
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets whether this shader is valid to render.
        /// </summary>
        public bool Valid
        {
            get{ return effect != null; }
        }

        /// <summary>
        /// Gets the currently used effect that is loaded from a shader file.
        /// </summary>
        public Effect CurrentEffect
        {
            get{ return effect; }
        }

        public Material CurrentMaterial
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the effect technique from the technique name defined in the loaded shader file.
        /// </summary>
        /// <param name="techniqueName"></param>
        public EffectTechnique GetTechnique(String techniqueName)
        {
            return effect.Techniques[techniqueName];
        }

        /// <summary>
        /// Gets the number of techniques implemented in the loaded shader file.
        /// </summary>
        public int NumberOfTechniques
        {
            get{ return effect.Techniques.Count; } 
        }

        /// <summary>
        /// Sets an effect parameter for Matrix type.
        /// </summary>
        /// <param name="param">An effect parameter that contains Matrix type</param>
        /// <param name="lastUsedMatrix">Old Matrix value</param>
        /// <param name="newMatrix">New Matrix value</param>
        protected void SetValue(EffectParameter param,
            ref Matrix lastUsedMatrix, Matrix newMatrix)
        {
            lastUsedMatrix = newMatrix;
            param.SetValue(newMatrix);
        } 

        /// <summary>
        /// Sets an effect parameter for Vector3 type.
        /// </summary>
        /// <param name="param">An effect parameter that contains Matrix type</param>
        /// <param name="lastUsedVector">Last used vector</param>
        /// <param name="newVector">New vector</param>
        protected void SetValue(EffectParameter param,
            ref Vector3 lastUsedVector, Vector3 newVector)
        {
            if (param != null &&
                lastUsedVector != newVector)
            {
                lastUsedVector = newVector;
                param.SetValue(newVector);
            } 
        } 

        /// <summary>
        /// Sets an effect parameter for Color type.
        /// </summary>
        /// <param name="param">An effect parameter that contains Color type</param>
        /// <param name="lastUsedColor">Last used color</param>
        /// <param name="newColor">New color</param>
        protected void SetValue(EffectParameter param,
            ref Color lastUsedColor, Color newColor)
        {
            // Note: This check eats few % of the performance, but the color
            // often stays the change (around 50%).
            if (param != null &&
                //slower: lastUsedColor != newColor)
                lastUsedColor.PackedValue != newColor.PackedValue)
            {
                lastUsedColor = newColor;
                param.SetValue(newColor.ToVector4());
            } 
        } 

        /// <summary>
        /// Sets an effect parameter for float type.
        /// </summary>
        /// <param name="param">An effect parameter that contains float type</param>
        /// <param name="lastUsedValue">Last used value</param>
        /// <param name="newValue">New value</param>
        protected void SetValue(EffectParameter param,
            ref float lastUsedValue, float newValue)
        {
            if (param != null &&
                lastUsedValue != newValue)
            {
                lastUsedValue = newValue;
                param.SetValue(newValue);
            } 
        } 

        /// <summary>
        /// Sets an effect parameter for Xna.Framework.Graphics.Texture type.
        /// </summary>
        /// <param name="param">An effect parameter that contains 
        /// Xna.Framework.Graphics.Texture type</param>
        /// <param name="lastUsedValue">Last used value</param>
        /// <param name="newValue">New value</param>
        protected void SetValue(EffectParameter param,
            ref Texture lastUsedValue, Texture newValue)
        {
            if (param != null &&
                lastUsedValue != newValue)
            {
                lastUsedValue = newValue;
                param.SetValue(newValue);
            } 
        } 

        /// <summary>
        /// Gets or sets world projection matrix.
        /// </summary>
        protected Matrix WorldViewProjMatrix
        {
            get { return lastUsedWorldViewProjMatrix; } 
            set { SetValue(worldViewProj, ref lastUsedWorldViewProjMatrix, value); } 
        }

        /// <summary>
        /// Gets or sets view projection matrix.
        /// </summary>
        protected Matrix ViewProjMatrix
        {
            get { return lastUsedViewProjMatrix; } 
            set { SetValue(viewProj, ref lastUsedViewProjMatrix, value); } 
        }

        /// <summary>
        /// Gets or sets inverse view projection matrix.
        /// </summary>
        protected Matrix ViewInverseMatrix
        {
            get { return lastUsedViewInverseMatrix; }
            set { SetValue(viewInverse, ref lastUsedViewInverseMatrix, value); }
        }

        /// <summary>
        /// Gets or sets projection matrix.
        /// </summary>
        protected Matrix ProjectionMatrix
        {
            get { return lastUsedProjMatrix; }
            set { SetValue(projection, ref lastUsedProjMatrix, value); }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Reloads the shader file with the specified path.
        /// </summary>
        /// <param name="shaderName">The name of the shader/effect file</param>
        public void Reload(String shaderName)
        {
            this.shaderName = Path.GetFileNameWithoutExtension(shaderName);
            // Load shader
            effect = State.Content.Load<Effect>(
                Path.Combine(State.GetSettingVariable("ShaderDirectory"),
                this.shaderName)).Clone();

            // Reset and get all avialable parameters.
            // This is especially important for derived classes.
            ResetParameters();
            GetParameters();
        }

        /// <summary>
        /// Loads the effect parameters from the loaded shader file.
        /// </summary>
        protected virtual void GetParameters()
        {
        }

        /// <summary>
        /// Resets the values of effect parameters.
        /// </summary>
        protected virtual void ResetParameters()
        {
            lastUsedViewInverseMatrix = Matrix.Identity;
            lastUsedProjMatrix = Matrix.Identity;
            lastUsedViewProjMatrix = Matrix.Identity;
            lastUsedWorldViewProjMatrix = Matrix.Identity;
        }

        #endregion

        #region IShader Members

        /// <summary>
        /// This shader does not support material effect.
        /// </summary>
        /// <param name="material"></param>
        public virtual void SetParameters(Material material)
        {
        }

        /// <summary>
        /// This shader does not support lighting effect.
        /// </summary>
        /// <param name="globalLights"></param>
        /// <param name="localLights"></param>
        public virtual void SetParameters(List<LightNode> globalLights, List<LightNode> localLights)
        {
        }

        /// <summary>
        /// This shader does not support particle effect.
        /// </summary>
        /// <param name="particleEffect"></param>
        public virtual void SetParameters(ParticleEffect particleEffect)
        {
        }

        /// <summary>
        /// This shader does not support special camera effect.
        /// </summary>
        /// <param name="camera"></param>
        public virtual void SetParameters(CameraNode camera)
        {
        }

        /// <summary>
        /// This shader does not support environmental effect.
        /// </summary>
        /// <param name="environment"></param>
        public virtual void SetParameters(GoblinXNA.Graphics.Environment environment)
        {
        }

        public virtual void Render(ref Matrix worldMatrix, String techniqueName, 
            RenderHandler renderDelegate)
        {
            if (techniqueName == null)
                throw new GoblinException("techniqueName is null");
            if (renderDelegate == null)
                throw new GoblinException("renderDelegate is null");

            world.SetValue(worldMatrix);
            // Start shader
            effect.CurrentTechnique = effect.Techniques[techniqueName];

            // Render all passes (usually just one)
            //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            for (int num = 0; num < effect.CurrentTechnique.Passes.Count; num++)
            {
                EffectPass pass = effect.CurrentTechnique.Passes[num];

                pass.Apply();
                renderDelegate();
            }

        }

        public virtual void RenderEnd()
        {
        }

        public virtual void Dispose()
        {
            if (effect != null)
                effect.Dispose();
        }

        #endregion
    }
}
