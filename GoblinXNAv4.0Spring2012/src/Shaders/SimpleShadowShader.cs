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
using GoblinXNA.Graphics.ParticleEffects;
using GoblinXNA.SceneGraph;
using GoblinXNA.Helpers;

namespace GoblinXNA.Shaders
{
    public class SimpleShadowShader : IShader, IShadowShader
    {
        #region Member Fields

        protected Effect effect;

        protected EffectParameter 
            world,
            viewProj,
            lightViewProj,
            lightType,
            cameraPosition,
            castShadows,
            diffuseColor,
            specularColor,
            lightDirection,
            diffuseLightColor,
            specularLightColor,
            ambientLightColor,
            specularPower,
            hasTexture,
            diffuseTexture,
            lightPosition,
            atten0,
            atten1,
            atten2,
            lightConeAngle,
            lightDecay,
            firstPass,
            lastLayer,
            shadowsOnly,
            isOccluder;

        protected List<LightNode> lights;

        #endregion

        #region Constructors

        public SimpleShadowShader(IShadowMap shadowMap)
        {
            this.ShadowMap = shadowMap;
            if (State.Device == null)
                throw new GoblinException(
                    "GoblinXNA device is not initialized, can't create Shader.");

            effect = State.Content.Load<Effect>(
                System.IO.Path.Combine(State.GetSettingVariable("ShaderDirectory"),
                "SimpleShadowShader"));

            lights = new List<LightNode>();

            GetParameters();
        }

        #endregion

        #region Properties

        public virtual int MaxLights
        {
            get { return 10; }
        }

        public Effect CurrentEffect
        {
            get{ return effect; }
        }

        public Material CurrentMaterial
        {
            get;
            set;
        }

        public IShadowMap ShadowMap
        {
            get;
            set;
        }

        public RenderTarget2D LastLayer
        {
            get;
            set;
        }

        public ShadowAttribute Attribute
        {
            get;
            set;
        }

        public bool IsOccluder
        {
            get;
            set;
        }

        public int LightIndex
        {
            get;
            set;
        }

        public int ShadowLightIndex
        {
            get;
            set;
        }

        #endregion

        #region Private Methods

        private void GetParameters()
        {
            world = effect.Parameters["World"];
            viewProj = effect.Parameters["ViewProjection"];
            lightViewProj = effect.Parameters["LightViewProj"];
            cameraPosition = effect.Parameters["CameraPosition"];

            diffuseColor = effect.Parameters["DiffuseColor"];
            specularColor = effect.Parameters["SpecularColor"];
            specularPower = effect.Parameters["SpecularPower"];
            isOccluder = effect.Parameters["IsOccluder"];

            lightType = effect.Parameters["LightType"];
            castShadows = effect.Parameters["CastShadows"];
            diffuseLightColor = effect.Parameters["DiffuseLightColor"];
            specularLightColor = effect.Parameters["SpecularLightColor"];
            ambientLightColor = effect.Parameters["AmbientLightColor"];
            lightDirection = effect.Parameters["LightDirection"];
            lightPosition = effect.Parameters["LightPosition"];
            atten0 = effect.Parameters["Atten0"];
            atten1 = effect.Parameters["Atten1"];
            atten2 = effect.Parameters["Atten2"];
            lightConeAngle = effect.Parameters["LightConeAngle"];
            lightDecay = effect.Parameters["LightDecay"];

            firstPass = effect.Parameters["FirstPass"];

            hasTexture = effect.Parameters["HasTexture"];
            diffuseTexture = effect.Parameters["Texture"];
            lastLayer = effect.Parameters["LastLayer"];
            shadowsOnly = effect.Parameters["ShadowsOnly"];
        }

        #endregion

        #region IShader Members

        public virtual void SetParameters(Material material)
        {
            if (material.InternalEffect != null)
            {
                if (material.InternalEffect is BasicEffect)
                {
                    BasicEffect be = (BasicEffect)material.InternalEffect;

                    hasTexture.SetValue(be.TextureEnabled);
                    if(be.TextureEnabled)
                        diffuseTexture.SetValue(be.Texture);

                    Vector4 diffuse = new Vector4(be.DiffuseColor, be.Alpha);
                    diffuseColor.SetValue(diffuse);
                    Vector4 specular = new Vector4(be.SpecularColor, 1);
                    specularColor.SetValue(specular);
                    specularPower.SetValue(be.SpecularPower);
                }
                else
                    Log.Write("Passed internal effect is not BasicEffect, so we can not apply the " +
                        "effect to this shader", Log.LogLevel.Warning);
            }
            else
            {
                hasTexture.SetValue(material.HasTexture);
                if (material.HasTexture)
                    diffuseTexture.SetValue(material.Texture);

                diffuseColor.SetValue(material.Diffuse);
                specularColor.SetValue(material.Specular);
                specularPower.SetValue(material.SpecularPower);
            }
        }

        public virtual void SetParameters(List<LightNode> globalLights, List<LightNode> localLights)
        {
            lights = globalLights;
        }

        /// <summary>
        /// This shader does not support particle effect.
        /// </summary>
        /// <param name="particleEffect"></param>
        public virtual void SetParameters(ParticleEffect particleEffect)
        {
            // nothing to do
        }

        /// <summary>
        /// This shader does not support special camera effect.
        /// </summary>
        /// <param name="camera"></param>
        public virtual void SetParameters(CameraNode camera)
        {
            cameraPosition.SetValue(camera.WorldTransformation.Translation);
        }

        /// <summary>
        /// This shader does not support environmental effect.
        /// </summary>
        /// <param name="environment"></param>
        public virtual void SetParameters(GoblinXNA.Graphics.Environment environment)
        {
            // nothing to do
        }

        public virtual void Render(ref Matrix worldMatrix, String techniqueName, 
            RenderHandler renderDelegate)
        {
            if (String.IsNullOrEmpty(techniqueName))
                techniqueName = "DrawWithShadowMap";
            if (renderDelegate == null)
                throw new GoblinException("renderDelegate is null");

            State.Device.SamplerStates[1] = SamplerState.LinearWrap;
            State.Device.SamplerStates[2] = SamplerState.PointClamp;

            // Start shader
            effect.CurrentTechnique = effect.Techniques[techniqueName];

            world.SetValue(worldMatrix);
            viewProj.SetValue(State.ViewProjectionMatrix);
            castShadows.SetValue(lights[LightIndex].CastShadows);
            isOccluder.SetValue(IsOccluder);

            firstPass.SetValue(LastLayer == null);
            if (LastLayer != null)
                lastLayer.SetValue(LastLayer);

            if (ShadowLightIndex >= 0)
            {
                if (this.Attribute == ShadowAttribute.ReceiveCast)
                    shadowsOnly.SetValue(ShadowMap.OccluderRenderTargets[ShadowLightIndex]);
                else if (this.Attribute == ShadowAttribute.ReceiveOnly)
                    shadowsOnly.SetValue(ShadowMap.ShadowRenderTargets[ShadowLightIndex]);
            }

            if (!IsOccluder)
            {
                lightViewProj.SetValue(lights[LightIndex].LightViewProjection);
                lightType.SetValue((int)lights[LightIndex].LightSource.Type);

                diffuseLightColor.SetValue(lights[LightIndex].LightSource.Diffuse);
                specularLightColor.SetValue(lights[LightIndex].LightSource.Specular);
                ambientLightColor.SetValue(lights[LightIndex].AmbientLightColor);

                if (lights[LightIndex].LightSource.Type != LightType.Directional)
                {
                    lightPosition.SetValue(lights[LightIndex].LightSource.TransformedPosition);

                    atten0.SetValue(lights[LightIndex].LightSource.Attenuation0);
                    atten1.SetValue(lights[LightIndex].LightSource.Attenuation1);
                    atten2.SetValue(lights[LightIndex].LightSource.Attenuation2);
                }
                else
                    lightDirection.SetValue(-lights[LightIndex].LightSource.TransformedDirection);
                if (lights[LightIndex].LightSource.Type == LightType.SpotLight)
                {
                    lightConeAngle.SetValue(lights[LightIndex].LightSource.OuterConeAngle);
                    lightDecay.SetValue(lights[LightIndex].LightSource.Falloff);
                    lightDirection.SetValue(lights[LightIndex].LightSource.TransformedDirection);
                }
            }

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
