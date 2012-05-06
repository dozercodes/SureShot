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
using System.Xml;

using Microsoft.Xna.Framework;

using GoblinXNA.Graphics;
using GoblinXNA.Helpers;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A scene graph node that holds a light source.
    /// </summary>
    public class LightNode : Node
    {
        #region Member Fields

        protected LightSource lightSource;
        protected bool global;
        protected bool castShadows;
        protected Vector4 ambientLightColor;
        protected Matrix worldTransform;
        protected Matrix lightViewProjection;
        protected Matrix lightProjection;
        protected BoundingFrustum lightFrustum;

        protected bool hasChanged;

        protected Vector3 tmpVec1;
        protected Vector3 tmpVec2;
        protected Vector3 zero;
        protected Vector3 up;
        protected Matrix tmpMat1;
        protected Matrix tmpMat2;
        protected Matrix tmpMat3;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a node that can hold a light source.
        /// </summary>
        /// <param name="name">The name of this light node</param>
        public LightNode(String name)
            : base(name)
        {
            lightSource = new LightSource();
            global = true;
            castShadows = true;
            worldTransform = Matrix.Identity;
            lightViewProjection = Matrix.Identity;
            ambientLightColor = new Vector4(0, 0, 0, 1);
            hasChanged = true;

            lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 1000);
            lightFrustum = new BoundingFrustum(lightProjection);

            zero = Vector3.Zero;
            up = Vector3.Up;
        }
        /// <summary>
        /// Creates a light node with an empty name
        /// </summary>
        public LightNode() : this("") { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a light source associated with this node
        /// </summary>
        public LightSource LightSource
        {
            get { return lightSource; }
            set 
            { 
                lightSource = value;
                hasChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets whether this light is a global light source. If set to true, then no 
        /// matter where in the scene graph this light node exists, the light source associated
        /// with this node will affect all objects in the scene graph. If set to false, then this node's
        /// light source will affect only this node's siblings and siblings children. The default 
        /// value is true.
        /// </summary>
        public bool Global
        {
            get { return global; }
            set 
            {
                if (global != value)
                {
                    global = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this light should cast shadows when Scene.EnableShadowMapping is set to true.
        /// The default value is true. You should also set LightProjection property if you set this to true.
        /// </summary>
        /// <see cref="LightProjection"/>
        public bool CastShadows
        {
            get { return castShadows; }
            set
            {
                if (castShadows != value)
                {
                    castShadows = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ambient light color. The default value is (0.0f, 0.0f, 0.0f, 1.0f).
        /// </summary>
        public Vector4 AmbientLightColor
        {
            get { return ambientLightColor; }
            set 
            {
                if (!ambientLightColor.Equals(value))
                {
                    ambientLightColor = value;
                    hasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets the world transformation of this light node.
        /// </summary>
        public Matrix WorldTransformation
        {
            get { return worldTransform; }
            internal set
            {
                if (!worldTransform.Equals(value))
                {
                    worldTransform = value;
                    hasChanged = true;

                    // compute the transformed direction and position of the light source
                    if (lightSource.Type != LightType.Directional)
                    {
                        tmpVec1 = lightSource.Position;
                        Matrix.CreateTranslation(ref tmpVec1, out tmpMat2);
                        Matrix.Multiply(ref worldTransform, ref tmpMat2, out tmpMat3);

                        lightSource.TransformedPosition = tmpMat3.Translation;
                    }

                    if (lightSource.Type != LightType.Point)
                    {
                        tmpVec1 = lightSource.Direction;
                        Matrix.CreateTranslation(ref tmpVec1, out tmpMat1);
                        MatrixHelper.GetRotationMatrix(ref worldTransform, out tmpMat2);
                        Matrix.Multiply(ref tmpMat1, ref tmpMat2, out tmpMat3);

                        lightSource.TransformedDirection = tmpMat3.Translation;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the light projection used for casting shadows. If you set CastShadows property to true,
        /// you should also set this projection matrix.
        /// </summary>
        public Matrix LightProjection
        {
            get { return lightProjection; }
            set
            {
                lightProjection = value;
                lightFrustum = new BoundingFrustum(lightProjection);
            }
        }

        public Matrix LightViewProjection
        {
            get { return lightViewProjection; }
        }

        /// <summary>
        /// Indicates whether the light source or light node itself has changes to be reflected.
        /// </summary>
        internal bool HasChanged
        {
            get { return (hasChanged | lightSource.HasChanged); }
            set
            {
                hasChanged = value;
                lightSource.HasChanged = value;
            }
        }

        #endregion

        #region Public Methods

        public void ComputeLightViewProjection()
        {
            switch (lightSource.Type)
            {
                case LightType.Directional:
                    tmpVec1 = lightSource.TransformedDirection;
                    // Matrix with that will rotate in points the direction of the light
                    Matrix.CreateLookAt(ref zero, ref tmpVec1, ref up, out tmpMat1);

                    // Get the corners of the frustum
                    Vector3[] frustumCorners = lightFrustum.GetCorners();

                    // Transform the positions of the corners into the direction of the light
                    for (int i = 0; i < frustumCorners.Length; i++)
                    {
                        Vector3.Transform(ref frustumCorners[i], ref tmpMat1, out frustumCorners[i]);
                    }

                    // Find the smallest box around the points
                    BoundingBox lightBox = BoundingBox.CreateFromPoints(frustumCorners);

                    Vector3 boxSize = lightBox.Max - lightBox.Min;
                    Vector3 halfBoxSize = boxSize * 0.5f;

                    // The position of the light should be in the center of the back
                    // pannel of the box. 
                    tmpVec1 = lightBox.Min + halfBoxSize;
                    tmpVec1.Z = lightBox.Min.Z;

                    // We need the position back in world coordinates so we transform 
                    // the light position by the inverse of the lights rotation
                    Matrix.Invert(ref tmpMat1, out tmpMat2);
                    Vector3.Transform(ref tmpVec1, ref tmpMat2, out tmpVec1);

                    // Create the view matrix for the light
                    tmpVec2 = tmpVec1 + lightSource.TransformedDirection;
                    Matrix.CreateLookAt(ref tmpVec1, ref tmpVec2, ref up, out tmpMat1);

                    // Create the projection matrix for the light
                    // The projection is orthographic since we are using a directional light
                    Matrix.CreateOrthographic(boxSize.X, boxSize.Y, -boxSize.Z, boxSize.Z, out tmpMat2);

                    Matrix.Multiply(ref tmpMat1, ref tmpMat2, out lightViewProjection);
                    break;
                case LightType.Point:
                case LightType.SpotLight:
                    tmpVec1 = lightSource.TransformedPosition;
                    tmpVec2 = lightSource.TransformedDirection;
                    Matrix.CreateLookAt(ref tmpVec1, ref tmpVec2, ref up, out tmpMat1);

                    Matrix.Multiply(ref tmpMat1, ref lightProjection, out lightViewProjection);
                    break;
            }
        }

        #endregion

        #region Overriden Methods
        /// <summary>
        /// Clones this light node.
        /// </summary>
        /// <returns></returns>
        public override Node CloneNode()
        {
            LightNode clone = (LightNode)base.CloneNode();
            clone.Global = global;
            clone.CastShadows = castShadows;
            clone.AmbientLightColor = ambientLightColor;
            clone.LightSource = lightSource;

            return clone;
        }

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            xmlNode.SetAttribute("Global", global.ToString());
            xmlNode.SetAttribute("CastShadows", castShadows.ToString());
            xmlNode.SetAttribute("AmbientLightColor", ambientLightColor.ToString());

            xmlNode.AppendChild(lightSource.Save(xmlDoc));

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            if (xmlNode.HasAttribute("Global"))
                global = bool.Parse(xmlNode.GetAttribute("Global"));
            if (xmlNode.HasAttribute("CastShadows"))
                castShadows = bool.Parse(xmlNode.GetAttribute("CastShadows"));
            if (xmlNode.HasAttribute("AmbientLightColor"))
                ambientLightColor = Vector4Helper.FromString(xmlNode.GetAttribute("AmbientLightColor"));

            XmlElement lightSourceXml = (XmlElement)xmlNode.ChildNodes[0];
            lightSource = (LightSource)Activator.CreateInstance(
                Type.GetType(lightSourceXml.Name));
            lightSource.Load(lightSourceXml);
        }
#endif

        #endregion
    }
}
