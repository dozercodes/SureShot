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
    public enum LightType
    {
        /// <summary>
        /// A point source is defined as a single point in space. The intensity of the light is
        /// attenuated with three attenuation coefficients. 
        /// </summary>
        Point,
        /// <summary>
        /// A directional source is described by the direction in which it is pointing, and 
        /// is useful for modeling a light source that is effectively infinitely far away from 
        /// the objects it illuminates.
        /// </summary>
        Directional,
        /// <summary>
        /// A spotlight is useful for creating dramatic localized lighting effects. It is defined 
        /// by its position, the direction in which it is pointing, and the width of the beam of 
        /// light it produces.
        /// </summary>
        SpotLight
    } 

    /// <summary>
    /// Light sources are used to illuminate the world.
    /// </summary>
    public class LightSource
    {
        #region Fields

        protected Vector3 position;
        protected Vector3 direction;
        protected LightType lightType;
        protected bool enabled;
        protected Vector4 diffuse;
        protected Vector4 specular;
        protected float attenuation0;
        protected float attenuation1;
        protected float attenuation2;
        protected float falloff;
        protected float innerConeAngle;
        protected float outerConeAngle;
        protected float range;

        protected Vector3 transformedPosition;
        protected Vector3 transformedDirection;

        protected bool hasChanged;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a light source with default configurations (see each field for the default values)
        /// </summary>
        public LightSource()
        {
            position = Vector3.Zero;
            direction = Vector3.Zero;
            lightType = LightType.Directional;
            enabled = true;
            diffuse = Color.Black.ToVector4();
            specular = Color.Black.ToVector4();
            attenuation0 = 1;
            attenuation1 = 0.1f;
            attenuation2 = 0.0f;
            falloff = 0.2f;
            innerConeAngle = (float)(0.2 * Math.PI);
            outerConeAngle = (float)(0.3 * Math.PI);
            range = 500;
            hasChanged = false;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="light"></param>
        public LightSource(LightSource light)
        {
            position = light.Position;
            direction = light.Direction;
            lightType = light.Type;
            enabled = light.Enabled;
            diffuse = light.Diffuse;
            specular = light.Specular;
            attenuation0 = light.Attenuation0;
            attenuation1 = light.Attenuation1;
            attenuation2 = light.Attenuation2;
            falloff = light.Falloff;
            innerConeAngle = light.InnerConeAngle;
            outerConeAngle = light.OuterConeAngle;
            range = light.Range;
            hasChanged = true;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the type of this light (Point, Directional, or SpotLight) 
        /// The default value is LightType.Directional.
        /// </summary>
        public LightType Type
        {
            get{ return lightType; }
            set
            {
                if (lightType != value)
                {
                    lightType = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this light source is enabled. The default value is true.
        /// </summary>
        public bool Enabled 
        {
            get{ return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the diffuse component of this light. The default value is Color.Black.
        /// </summary>
        public Vector4 Diffuse
        {
            get{ return diffuse; }
            set
            {
                if (!diffuse.Equals(value))
                {
                    diffuse = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the specular component of this light. The default value is Color.Black.
        /// </summary>
        public Vector4 Specular
        {
            get { return specular; }
            set 
            {
                if (!specular.Equals(value))
                {
                    specular = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the position of this light source. This property is used only for Point
        /// and SpotLight types. The default value is vector (0, 0, 0).
        /// </summary>
        public Vector3 Position
        {
            get{ return position; }
            set
            {
                if (!position.Equals(value))
                {
                    position = value;
                    transformedPosition = position;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets the transformed version of the Position property by the associated LightNode's
        /// WorldTransformation.
        /// </summary>
        public Vector3 TransformedPosition
        {
            get { return transformedPosition; }
            internal set { transformedPosition = value; }
        }

        /// <summary>
        /// Gets or sets the direction of this light source. This property is used
        /// only for Directional and SpotLight types. The default value is vector (0, 0, 0).
        /// </summary>
        public Vector3 Direction
        {
            get{ return direction; }
            set
            {
                if (!direction.Equals(value))
                {
                    direction = value;
                    transformedDirection = direction;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets the transformed version of the Direction property by the associated LightNode's
        /// WorldTransformation.
        /// </summary>
        public Vector3 TransformedDirection
        {
            get { return transformedDirection; }
            internal set { transformedDirection = value; }
        }

        /// <summary>
        /// Gets or sets the zero-th degree attenuation coefficient for estimating light energy attenuation. 
        /// This property is used only for Point and SpotLight types. The default value is 0.01f.
        /// </summary>
        public float Attenuation0
        {
            get { return attenuation0; }
            set 
            {
                if (attenuation0 != value)
                {
                    attenuation0 = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the first degree attenuation coefficient for estimating light energy attenuation. 
        /// This property is used only for Point and SpotLight types. The default value is 0.1f.
        /// </summary>
        public float Attenuation1
        {
            get { return attenuation1; }
            set 
            {
                if (attenuation1 != value)
                {
                    attenuation1 = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the second degree attenuation coefficient for estimating light energy attenuation. 
        /// This property is used only for Point and SpotLight types. The default value is 0.0f.
        /// </summary>
        public float Attenuation2
        {
            get { return attenuation2; }
            set 
            {
                if (attenuation2 != value)
                {
                    attenuation2 = value;
                    hasChanged = true;
                }
            }
        }

        public float Falloff
        {
            get { return falloff; }
            set 
            {
                if (falloff != value)
                {
                    falloff = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the inner radius of the spotlight where the light begins to be attenuated. 
        /// This property is used only for SpotLight type. The default value is 0.2f * PI (36 degrees).
        /// </summary>
        public float InnerConeAngle
        {
            get { return innerConeAngle; }
            set 
            {
                if (innerConeAngle != value)
                {
                    innerConeAngle = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the outer radius of the spotlight where the light intensity (ambient) is zero. 
        /// The default value is 0.3f * PI (54 degrees).
        /// </summary>
        public float OuterConeAngle
        {
            get { return outerConeAngle; }
            set
            {
                if (outerConeAngle != value)
                {
                    outerConeAngle = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the effective range of this light source. This property is used only for
        /// Point and SpotLight types. The default value is 500.
        /// </summary>
        public float Range
        {
            get { return range; }
            set 
            {
                if (range != value)
                {
                    range = value;
                    hasChanged = true;
                }
            }
        }

        internal bool HasChanged
        {
            get { return hasChanged; }
            set { hasChanged = value; }
        }

        #endregion

        #region Public Methods

#if !WINDOWS_PHONE
        public virtual XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = xmlDoc.CreateElement(TypeDescriptor.GetClassName(this));

            xmlNode.SetAttribute("LightType", lightType.ToString());
            xmlNode.SetAttribute("Enabled", enabled.ToString());
            xmlNode.SetAttribute("Diffuse", diffuse.ToString());
            xmlNode.SetAttribute("Specular", specular.ToString());

            if (lightType == LightType.Point)
            {
                xmlNode.SetAttribute("Position", position.ToString());
                xmlNode.SetAttribute("Attenuation0", attenuation0.ToString());
                xmlNode.SetAttribute("Attenuation1", attenuation1.ToString());
                xmlNode.SetAttribute("Attenuation2", attenuation2.ToString());
                xmlNode.SetAttribute("Range", range.ToString());
            }
            else if (lightType == LightType.Directional)
            {
                xmlNode.SetAttribute("Direction", direction.ToString());
            }
            else
            {
                xmlNode.SetAttribute("Position", position.ToString());
                xmlNode.SetAttribute("Direction", direction.ToString());
                xmlNode.SetAttribute("Attenuation0", attenuation0.ToString());
                xmlNode.SetAttribute("Attenuation1", attenuation1.ToString());
                xmlNode.SetAttribute("Attenuation2", attenuation2.ToString());
                xmlNode.SetAttribute("Range", range.ToString());
                xmlNode.SetAttribute("Falloff", falloff.ToString());
                xmlNode.SetAttribute("InnerConeAngle", innerConeAngle.ToString());
                xmlNode.SetAttribute("OuterConeAngle", outerConeAngle.ToString());
            }

            return xmlNode;
        }

        public virtual void Load(XmlElement xmlNode)
        {
            if (xmlNode.HasAttribute("LightType"))
                lightType = (LightType)Enum.Parse(typeof(LightType), xmlNode.GetAttribute("LightType"));
            if (xmlNode.HasAttribute("Enabled"))
                enabled = bool.Parse(xmlNode.GetAttribute("Enabled"));
            if (xmlNode.HasAttribute("Diffuse"))
                diffuse = Vector4Helper.FromString(xmlNode.GetAttribute("Diffuse"));
            if (xmlNode.HasAttribute("Specular"))
                specular = Vector4Helper.FromString(xmlNode.GetAttribute("Specular"));

            if (lightType == LightType.Point)
            {
                if (xmlNode.HasAttribute("Position"))
                    position = Vector3Helper.FromString(xmlNode.GetAttribute("Position"));
                if (xmlNode.HasAttribute("Attenuation0"))
                    attenuation0 = float.Parse(xmlNode.GetAttribute("Attenuation0"));
                if (xmlNode.HasAttribute("Attenuation1"))
                    attenuation1 = float.Parse(xmlNode.GetAttribute("Attenuation1"));
                if (xmlNode.HasAttribute("Attenuation2"))
                    attenuation2 = float.Parse(xmlNode.GetAttribute("Attenuation2"));
                if (xmlNode.HasAttribute("Range"))
                    range = float.Parse(xmlNode.GetAttribute("Range"));
            }
            else if (lightType == LightType.Directional)
            {
                if (xmlNode.HasAttribute("Direction"))
                    direction = Vector3Helper.FromString(xmlNode.GetAttribute("Direction"));
            }
            else
            {
                if (xmlNode.HasAttribute("Position"))
                    position = Vector3Helper.FromString(xmlNode.GetAttribute("Position"));
                if (xmlNode.HasAttribute("Direction"))
                    direction = Vector3Helper.FromString(xmlNode.GetAttribute("Direction"));
                if (xmlNode.HasAttribute("Attenuation0"))
                    attenuation0 = float.Parse(xmlNode.GetAttribute("Attenuation0"));
                if (xmlNode.HasAttribute("Attenuation1"))
                    attenuation1 = float.Parse(xmlNode.GetAttribute("Attenuation1"));
                if (xmlNode.HasAttribute("Attenuation2"))
                    attenuation2 = float.Parse(xmlNode.GetAttribute("Attenuation2"));
                if (xmlNode.HasAttribute("Range"))
                    range = float.Parse(xmlNode.GetAttribute("Range"));
                if (xmlNode.HasAttribute("Falloff"))
                    falloff = float.Parse(xmlNode.GetAttribute("Falloff"));
                if (xmlNode.HasAttribute("InnerConeAngle"))
                    innerConeAngle = float.Parse(xmlNode.GetAttribute("InnerConeAngle"));
                if (xmlNode.HasAttribute("OuterConeAngle"))
                    outerConeAngle = float.Parse(xmlNode.GetAttribute("OuterConeAngle"));
            }
        }

#endif

        #endregion
    }
}
