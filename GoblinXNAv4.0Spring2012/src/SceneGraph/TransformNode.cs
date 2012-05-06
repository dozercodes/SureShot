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

using GoblinXNA.Helpers;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A scene graph node that defines a transformation.
    /// </summary>
    public class TransformNode : BranchNode
    {
        #region Member Fields

        protected Vector3 postTranslation;
        protected Quaternion rotation;
        protected Vector3 scaling;
        protected Vector3 preTranslation;
        protected Matrix worldTransformation;
        protected Matrix composedTransform;
        protected bool isWorldTransformationDirty;
        protected bool isReadOnly;
        protected bool useUserDefinedTransform;
        protected bool userDefinedTransformChanged;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a scene graph node that defines the transformation of its children.
        /// </summary>
        /// <param name="name">The name of this transform node</param>
        /// <param name="preTranslation">Pre-translation component of this transform that will be applied
        /// before scaling and rotation</param>
        /// <param name="postTranslation">Post-translation component of this transform that will be applied
        /// after scaling and rotation</param>
        /// <param name="rotation">Rotation component of this transform in quaternion</param>
        /// <param name="scaling">Scaling component of this transform</param>
        public TransformNode(String name, Vector3 preTranslation, Vector3 postTranslation, 
            Quaternion rotation, Vector3 scaling)
            : base(name)
        {
            this.preTranslation = preTranslation;
            this.postTranslation = postTranslation;
            this.rotation = rotation;
            this.scaling = scaling;
            worldTransformation = Matrix.Identity;
            isWorldTransformationDirty = true;
            useUserDefinedTransform = false;
            userDefinedTransformChanged = false;
            isReadOnly = false;
        }

        /// <summary>
        /// Creates a scene graph node that defines the transform of its children with no pre-translation.
        /// </summary>
        /// <param name="name">The name of this transform node</param>
        /// <param name="postTranslation">Post-translation component of this transform that will be applied
        /// after scaling and rotation</param>
        /// <param name="rotation">Rotation component of this transform in quaternion</param>
        /// <param name="scaling">Scaling component of this transform</param>
        public TransformNode(String name, Vector3 postTranslation, Quaternion rotation, Vector3 scaling)
            :
            this(name, Vector3.Zero, postTranslation, rotation, scaling) { }

        /// <summary>
        /// Creates a scene graph node that defines the transform of its children with scaling of 1
        /// in each dimension and no pre-translation.
        /// </summary>
        /// <param name="name">The name of this transform node</param>
        /// <param name="postTranslation">Post-translation component of this transform that will be applied
        /// after scaling and rotation</param>
        /// <param name="rotation">Rotation component of this transform in quaternion</param>
        public TransformNode(String name, Vector3 postTranslation, Quaternion rotation)
            :
            this(name, postTranslation, rotation, Vector3.One) { }

        /// <summary>
        /// Creates a scene graph node that defines the transform of its children with scaling of 1
        /// in each dimension and no pre-translation.
        /// </summary>
        /// <param name="postTranslation">Post-translation component of this transform that will be applied
        /// after scaling and rotation</param>
        /// <param name="rotation">Rotation component of this transform in quaternion</param>
        public TransformNode(Vector3 postTranslation, Quaternion rotation) : this("", postTranslation, rotation) { }

        /// <summary>
        /// Creates a scene graph node that defines the transform of its children with scaling of 1
        /// in each dimension, no rotation, and empty name.
        /// </summary>
        /// <param name="postTranslation">Post-translation component of this transform that will be applied
        /// after scaling and rotation</param>
        public TransformNode(Vector3 postTranslation) : this(postTranslation, Quaternion.Identity) { }

        /// <summary>
        /// Creates a scene graph node that defines the transform of its children with scaling of 1
        /// in each dimension, no rotation, and no translation.
        /// </summary>
        /// <param name="name">The name of this transform node</param>
        public TransformNode(String name) : this(name, new Vector3(), Quaternion.Identity) { }

        /// <summary>
        /// Creates a scene graph node that defines the transform of its children with scaling of 1
        /// in each dimension, no rotation, no translation, and no empty name.
        /// </summary>
        public TransformNode() : this("") { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the post-translation component that will be applied after rotation and scaling.
        /// 
        /// If WorldTransformation matrix is set directly after setting this property, then
        /// the value set for this property will not affect the transformation of this node.
        /// </summary>
        /// <remarks>
        /// If ReadOnly is set to true, then you can't set this property. Synonym of PostTranslation property.
        /// </remarks>
        /// <seealso cref="ReadOnly"/>
        /// <seealso cref="WorldTransformation"/>
        /// <exception cref="GoblinException"></exception>
        public Vector3 Translation
        {
            get
            {
                return postTranslation;
            }
            set
            {
                if (isReadOnly)
                    throw new GoblinException("This TransformNode is read only, " + 
                        "setting Translation is not allowed");

                if (!postTranslation.Equals(value))
                    isWorldTransformationDirty = true;
                postTranslation = value;
                useUserDefinedTransform = false;
            }
        }

        /// <summary>
        /// Gets or sets the post-translation component that will be applied after rotation and scaling.
        /// </summary>
        /// <remarks>
        /// Synonym of Translation property.
        /// </remarks>
        public Vector3 PostTranslation
        {
            get { return Translation; }
            set { Translation = value; }
        }

        /// <summary>
        /// Gets or sets the pre-translation component that will be applied before rotation and scaling.
        /// 
        /// If WorldTransformation matrix is set directly after setting this property, then
        /// the value set for this property will not affect the transformation of this node.
        /// </summary>
        /// <remarks>
        /// If ReadOnly is set to true, then you can't set this property.
        /// </remarks>
        public Vector3 PreTranslation
        {
            get{return preTranslation;}
            set
            {
                if (isReadOnly)
                    throw new GoblinException("This TransformNode is read only, " +
                        "setting PreTranslation is not allowed");

                if (!preTranslation.Equals(value))
                    isWorldTransformationDirty = true;
                preTranslation = value;
                useUserDefinedTransform = false;
            }
        }

        /// <summary>
        /// Gets or sets the rotation component.
        /// 
        /// If WorldTransformation matrix is set directly after setting this property, then
        /// the value set for this property will not affect the transformation of this node.
        /// </summary>
        /// <remarks>
        /// If ReadOnly is set to true, then you can't set this property.
        /// </remarks>
        /// <seealso cref="ReadOnly"/>
        /// <seealso cref="WorldTransformation"/>
        /// <exception cref="GoblinException"></exception>
        public Quaternion Rotation
        {
            get { return rotation; }
            set
            {
                if (isReadOnly)
                    throw new GoblinException("This TransformNode is read only, " +
                        "setting Rotation is not allowed");

                if (!rotation.Equals(value))
                    isWorldTransformationDirty = true;
                rotation = value;
                useUserDefinedTransform = false;
            }
        }

        /// <summary>
        /// Gets or sets the scale component.
        /// 
        /// If WorldTransformation matrix is set directly after setting this property, then
        /// the value set for this property will not affect the transformation of this node.
        /// </summary>
        /// <remarks>
        /// If ReadOnly is set to true, then you can't set this property.
        /// </remarks>
        /// <seealso cref="ReadOnly"/>
        /// <seealso cref="WorldTransformation"/>
        /// <exception cref="GoblinException"></exception>
        public Vector3 Scale
        {
            get { return scaling; }
            set
            {
                if (isReadOnly)
                    throw new GoblinException("This TransformNode is read only, " +
                        "setting Scale is not allowed");

                if (!scaling.Equals(value))
                    isWorldTransformationDirty = true;
                scaling = value;
                useUserDefinedTransform = false;
            }
        }

        /// <summary>
        /// Gets or sets the transformation matrix. If you set this matrix directly, then whatever
        /// you set on Translation, Rotation, and Scale properties previoulsy will be ignored, and instead, this
        /// matrix value will be used to define the transformation of this node.
        /// 
        /// However, if you set any of the Translation, Rotation, and Scale properties after setting
        /// this matrix, then the composed matrix value from these three properties will be used.
        /// </summary>
        /// <remarks>
        /// If ReadOnly is set to true, then you can't set this property.
        /// </remarks>
        /// <seealso cref="ReadOnly"/>
        /// <seealso cref="Scale"/>
        /// <seealso cref="Translation"/>
        /// <seealso cref="Rotation"/>
        /// <exception cref="GoblinException"></exception>
        public Matrix WorldTransformation
        {
            get 
            {
                if (useUserDefinedTransform)
                    return worldTransformation;
                else
                    return composedTransform;
            }
            set 
            {
                if (isReadOnly)
                    throw new GoblinException("This TransformNode is read only, " +
                        "setting WorldTransformation is not allowed");

                useUserDefinedTransform = true;
                worldTransformation = value;
                userDefinedTransformChanged = true;
                isWorldTransformationDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets whether the user directly modified the transformation matrix instead using the
        /// composed one from Translation, Rotation, and Scale properties.
        /// </summary>
        internal bool UseUserDefinedTransform
        {
            get { return useUserDefinedTransform; }
            set { useUserDefinedTransform = value; }
        }

        /// <summary>
        /// Gets or sets whether the transformation matrix set directly by the user has been
        /// modified from the last one.
        /// </summary>
        internal bool UserDefinedTransformChanged
        {
            get { return userDefinedTransformChanged; }
            set { userDefinedTransformChanged = value; }
        }

        /// <summary>
        /// Gets the matrix composed from each individual transformation properties
        /// (Translation, Rotation, and Scale).
        /// </summary>
        internal Matrix ComposedTransform
        {
            get { return composedTransform; }
            set { composedTransform = value; }
        }

        /// <summary>
        /// Gets or sets whether the transformation has been modified from the previous one.
        /// </summary>
        internal bool IsWorldTransformationDirty
        {
            get { return isWorldTransformationDirty; }
            set { isWorldTransformationDirty = value; }
        }

        /// <summary>
        /// Gets or sets whether this transform node is read only. If true, then you can not
        /// set any of the Translation, Rotation, Scale, and WorldTransformation properties.
        /// </summary>
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }

        #endregion

        #region Override Properties

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                if (value)
                    isWorldTransformationDirty = true;
            }
        }

        #endregion

        #region Override Methods

        public override Node CloneNode()
        {
            TransformNode clone = (TransformNode)base.CloneNode();
            clone.PostTranslation = postTranslation;
            clone.PreTranslation = preTranslation;
            clone.Rotation = rotation;
            clone.Scale = scaling;
            clone.WorldTransformation = WorldTransformation;
            clone.UseUserDefinedTransform = useUserDefinedTransform;
            clone.ComposedTransform = composedTransform;
            clone.IsWorldTransformationDirty = isWorldTransformationDirty;
            clone.IsReadOnly = isReadOnly;

            return clone;
        }

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            xmlNode.SetAttribute("Readonly", isReadOnly.ToString());
            xmlNode.SetAttribute("UseUserDefinedTransform", useUserDefinedTransform.ToString());

            if (useUserDefinedTransform)
            {
                xmlNode.SetAttribute("WorldTransform", worldTransformation.ToString());
            }
            else
            {
                xmlNode.SetAttribute("PreTranslation", preTranslation.ToString());
                xmlNode.SetAttribute("Scale", scaling.ToString());
                xmlNode.SetAttribute("PostTranslation", postTranslation.ToString());
                xmlNode.SetAttribute("Rotation", rotation.ToString());
            }

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            if (xmlNode.HasAttribute("Readonly"))
                isReadOnly = bool.Parse(xmlNode.GetAttribute("Readonly"));
            if (xmlNode.HasAttribute("UseUserDefinedTransform"))
                useUserDefinedTransform = bool.Parse(xmlNode.GetAttribute("UseUserDefinedTransform"));

            if (useUserDefinedTransform)
            {
                if (xmlNode.HasAttribute("WorldTransform"))
                    worldTransformation = MatrixHelper.FromString(xmlNode.GetAttribute("WorldTransform"));
            }
            else
            {
                if(xmlNode.HasAttribute("PreTranslation"))
                    preTranslation = Vector3Helper.FromString(xmlNode.GetAttribute("PreTranslation"));
                if (xmlNode.HasAttribute("Scale"))
                    scaling = Vector3Helper.FromString(xmlNode.GetAttribute("Scale"));
                if (xmlNode.HasAttribute("PostTranslation"))
                    postTranslation = Vector3Helper.FromString(xmlNode.GetAttribute("PostTranslation"));
                if (xmlNode.HasAttribute("Rotation"))
                {
                    Vector4 vec4 = Vector4Helper.FromString(xmlNode.GetAttribute("Rotation"));
                    rotation = new Quaternion(vec4.X, vec4.Y, vec4.Z, vec4.W);
                }
            }
        }
#endif

        #endregion
    }
}
