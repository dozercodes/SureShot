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
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;

namespace GoblinXNA.Graphics
{
    /// <summary>
    /// Material defines the surface properties of the model geometry such as its color, transparency,
    /// and shininess.
    /// </summary>
    public class Material
    {
        #region Member Fields
        protected float specularPower;

        protected Vector4 specularColor;
        protected Vector4 ambientColor;
        protected Vector4 emissiveColor;
        protected Vector4 diffuseColor;

        protected Texture2D texture;
        protected Effect internalEffect;

        protected bool hasChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a default material. See each properties for default values.
        /// </summary>
        public Material()
        {
            specularPower = 10.0f;

            specularColor = Vector4.Zero;
            ambientColor = Vector4.Zero;
            emissiveColor = Vector4.Zero;
            diffuseColor = new Vector4(0, 0, 0, 1);

            texture = null;
            internalEffect = null;
            hasChanged = false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the shininess of this material when highlighted with lights.
        /// The larger the specular power, the smaller the size of the specular highlight.
        /// The default value is 10.0f
        /// </summary>
        public virtual float SpecularPower
        {
            get { return specularPower; }
            set 
            {
                if (specularPower != value)
                {
                    specularPower = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the diffuse color of this material.
        /// The default value is Color.Black
        /// </summary>
        public virtual Vector4 Diffuse
        {
            get { return diffuseColor; }
            set 
            {
                if (!diffuseColor.Equals(value))
                {
                    diffuseColor = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ambient color of this material.
        /// The default value is Color.Black
        /// </summary>
        public virtual Vector4 Ambient
        {
            get { return ambientColor; }
            set 
            {
                if (!ambientColor.Equals(value))
                {
                    ambientColor = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the specular color of this material.
        /// The default value is Color.Black
        /// </summary>
        public virtual Vector4 Specular
        {
            get { return specularColor; }
            set 
            {
                if (!specularColor.Equals(value))
                {
                    specularColor = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the light this material emits. 
        /// The default value is Color.Black
        /// </summary>
        public virtual Vector4 Emissive
        {
            get { return emissiveColor; }
            set 
            {
                if (!emissiveColor.Equals(value))
                {
                    emissiveColor = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets whether this material contains texture information.
        /// </summary>
        public virtual bool HasTexture
        {
            get { return (texture != null); }
        }

        /// <summary>
        /// Gets or sets the texture applied to this material.
        /// </summary>
        public virtual Texture2D Texture
        {
            get { return texture; }
            set 
            { 
                texture = value;
                hasChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets whether there is a change in the material setting
        /// </summary>
        /// <remarks>
        /// Do not set this value in your application. It is set by the system.
        /// </remarks>
        public virtual bool HasChanged
        {
            get { return hasChanged; }
            set { hasChanged = value; }
        }

        /// <summary>
        /// Gets or sets the effect associated with model contents. Some model files include
        /// their own material information.
        /// </summary>
        /// <remarks>
        /// See XNA's reference manual for the details of an "Effect" class
        /// </remarks>
        public virtual Effect InternalEffect
        {
            get { return internalEffect; }
            set { internalEffect = value; }
        }
        #endregion

        #region Public Methods

        public virtual void Dispose()
        {
            if(texture != null)
                texture.Dispose();
        }

#if !WINDOWS_PHONE
        public virtual XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = xmlDoc.CreateElement(TypeDescriptor.GetClassName(this));

            xmlNode.SetAttribute("DiffuseColor", diffuseColor.ToString());
            xmlNode.SetAttribute("AmbientColor", ambientColor.ToString());
            xmlNode.SetAttribute("SpecularColor", specularColor.ToString());
            xmlNode.SetAttribute("SpecularPower", specularPower.ToString());
            xmlNode.SetAttribute("EmissiveColor", emissiveColor.ToString());

            if (texture != null)
                xmlNode.SetAttribute("TextureName", texture.Name);

            return xmlNode;
        }

        public virtual void Load(XmlElement xmlNode)
        {
            if (xmlNode.HasAttribute("DiffuseColor"))
                diffuseColor = Vector4Helper.FromString(xmlNode.GetAttribute("DiffuseColor"));
            if (xmlNode.HasAttribute("AmbientColor"))
                ambientColor = Vector4Helper.FromString(xmlNode.GetAttribute("AmbientColor"));
            if (xmlNode.HasAttribute("SpecularColor"))
                specularColor = Vector4Helper.FromString(xmlNode.GetAttribute("SpecularColor"));
            if (xmlNode.HasAttribute("SpecularPower"))
                specularPower = float.Parse(xmlNode.GetAttribute("SpecularPower"));
            if (xmlNode.HasAttribute("EmissiveColor"))
                emissiveColor = Vector4Helper.FromString(xmlNode.GetAttribute("EmissiveColor"));

            if (xmlNode.HasAttribute("TextureName"))
            {
                String textureName = xmlNode.GetAttribute("TextureName");
                texture = State.Content.Load<Texture2D>(textureName);
            }
        }
#endif

        #endregion
    }
}
