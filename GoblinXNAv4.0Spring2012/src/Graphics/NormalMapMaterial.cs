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

using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;
using System.Xml;

namespace GoblinXNA.Graphics
{
    /// <summary>
    /// An extension of the default material for normal map shading.
    /// </summary>
    public class NormalMapMaterial : Material
    {
        #region Member Fields

        protected Texture2D normalMapTexture;
        protected float fresnelBias;
        protected float fresnelPower;
        protected float reflectAmount;
        protected TextureCube envMap;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a normal map material.
        /// </summary>
        public NormalMapMaterial()
            : base()
        {
            fresnelBias = 0.5f;
            fresnelPower = 1.5f;
            reflectAmount = 1.0f;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the texture that contains normal map.
        /// </summary>
        public Texture2D NormalMapTexture
        {
            get { return normalMapTexture; }
            set
            {
                normalMapTexture = value;
                hasChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets fresnel bias for reflection from environment map.
        /// </summary>
        public float FresnelBias
        {
            get { return fresnelBias; }
            set
            {
                fresnelBias = value;
                hasChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the fresnel power for reflection from environment map.
        /// </summary>
        public float FresnelPower
        {
            get { return fresnelPower; }
            set
            {
                fresnelPower = value;
                hasChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the reflection amount from the environment map.
        /// </summary>
        public float ReflectionAmount
        {
            get { return reflectAmount; }
            set
            {
                reflectAmount = value;
                hasChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the texture used for environment mapping.
        /// </summary>
        public TextureCube EnvironmentMapTexture
        {
            get { return envMap; }
            set
            {
                envMap = value;
                hasChanged = true;
            }
        }

        #endregion

        #region Overriden Methods

        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            if (normalMapTexture != null)
                xmlNode.SetAttribute("NormalMapTextureName", normalMapTexture.Name);

            xmlNode.SetAttribute("FresnelBias", fresnelBias.ToString());
            xmlNode.SetAttribute("FresnelPower", fresnelPower.ToString());
            xmlNode.SetAttribute("ReflectionAmount", reflectAmount.ToString());

            if (envMap != null)
                xmlNode.SetAttribute("EnvironmentMapTextureName", envMap.Name);

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            if (xmlNode.HasAttribute("NormalMapTextureName"))
            {
                String normalMapTextureName = xmlNode.GetAttribute("NormalMapTextureName");
                normalMapTexture = State.Content.Load<Texture2D>(normalMapTextureName);
            }

            if (xmlNode.HasAttribute("FresnelBias"))
                fresnelBias = float.Parse(xmlNode.GetAttribute("FresnelBias"));
            if (xmlNode.HasAttribute("FresnelPower"))
                fresnelPower = float.Parse(xmlNode.GetAttribute("FresnelPower"));
            if (xmlNode.HasAttribute("ReflectionAmount"))
                reflectAmount = float.Parse(xmlNode.GetAttribute("ReflectionAmount"));

            if (xmlNode.HasAttribute("EnvironmentMapTextureName"))
            {
                String envMapName = xmlNode.GetAttribute("EnvironmentMapTextureName");
                envMap = State.Content.Load<TextureCube>(envMapName);
            }
        }

        #endregion
    }
}
