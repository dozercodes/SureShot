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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.Shaders;
using GoblinXNA.SceneGraph;
using GoblinXNA.Helpers;

namespace GoblinXNA.Shaders
{
    /// <summary>
    /// An implementaion of normal map (a.k.a bump mapping) shader.
    /// </summary>
    public class NormalMapShader : Shader
    {
        #region Members

        private EffectParameter view,
            diffuseColor,
            specularColor,
            lightDirection,
            lightColor,
            ambientLightColor,
            shinniness,
            diffuseTexture,
            normalMapTexture,
            fresnelBias,
            fresnelPower,
            reflectAmount,
            envMap;

        private Vector3 tmpVec1;

        private Vector4 lColor;
        private Vector4 lAmbiColor;
        
        #endregion

        #region Constructor

        public NormalMapShader()
            : base("NormalMapping")
        {
        }

        #endregion

        #region Private Methods

        private void GetMinimumParameters()
        {
            world = effect.Parameters["World"];
            viewProj = effect.Parameters["ViewProjection"];
            viewInverse = effect.Parameters["ViewInverse"];

            lightDirection = effect.Parameters["LightDirection"];
            lightColor = effect.Parameters["LightColor"];
            ambientLightColor = effect.Parameters["AmbientLightColor"];
        }

        #endregion

        #region Overriden Methods

        protected override void GetParameters()
        {
            world = effect.Parameters["World"];
            viewProj = effect.Parameters["ViewProjection"];
            viewInverse = effect.Parameters["ViewInverse"];

            diffuseColor = effect.Parameters["DiffuseColor"];
            specularColor = effect.Parameters["SpecularColor"];

            lightDirection = effect.Parameters["LightDirection"];
            lightColor = effect.Parameters["LightColor"];
            ambientLightColor = effect.Parameters["AmbientLightColor"];

            shinniness = effect.Parameters["Shininess"];

            diffuseTexture = effect.Parameters["Texture"];
            normalMapTexture = effect.Parameters["NormalMap"];

            fresnelBias = effect.Parameters["FresnelBias"];
            fresnelPower = effect.Parameters["FresnelPower"];
            reflectAmount = effect.Parameters["ReflectionAmount"];
            envMap = effect.Parameters["ReflectionCubeTexture"];
        }

        public override void SetParameters(Material material)
        {
            if (material.InternalEffect != null)
            {
                effect = material.InternalEffect;
                GetMinimumParameters();

                lightDirection.SetValue(-tmpVec1);
                lightColor.SetValue(lColor);
                ambientLightColor.SetValue(lAmbiColor);
            }
            else
            {
                diffuseColor.SetValue(material.Diffuse);
                specularColor.SetValue(material.Specular);
                shinniness.SetValue(material.SpecularPower);

                diffuseTexture.SetValue(material.Texture);
                
                if (material is NormalMapMaterial)
                {
                    NormalMapMaterial normMap = (NormalMapMaterial)material;
                    
                    normalMapTexture.SetValue(normMap.NormalMapTexture);
                    fresnelBias.SetValue(normMap.FresnelBias);
                    fresnelPower.SetValue(normMap.FresnelPower);
                    reflectAmount.SetValue(normMap.ReflectionAmount);
                    envMap.SetValue(normMap.EnvironmentMapTexture);
                }
            }
        }

        public override void SetParameters(List<LightNode> globalLights, List<LightNode> localLights)
        {
            if ((globalLights.Count + localLights.Count) == 0)
            {
                lightDirection.SetValue(Vector3.Zero);
                return;
            }

            LightNode lNode = null;
            if (globalLights.Count >= 1)
                lNode = globalLights[0];
            else
                lNode = localLights[localLights.Count - 1];

            tmpVec1 = lNode.LightSource.TransformedDirection;
            tmpVec1.Normalize();
            lightDirection.SetValue(-tmpVec1);

            lAmbiColor = lNode.AmbientLightColor;
            ambientLightColor.SetValue(lAmbiColor);

            lColor = lNode.LightSource.Diffuse;
            lightColor.SetValue(lColor);
        }

        public override void Render(ref Matrix worldMatrix, string techniqueName,
            RenderHandler renderDelegate)
        {
            viewProj.SetValue(State.ViewProjectionMatrix);
            viewInverse.SetValue(State.ViewInverseMatrix);

            if (techniqueName.Length == 0)
                techniqueName = "NormalMapOnly";

            base.Render(ref worldMatrix, techniqueName, renderDelegate);
        }

        #endregion
    }
}
