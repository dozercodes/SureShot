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
    /// A scene graph node that holds the camera properties of the viewer.
    /// </summary>
    public class CameraNode : Node
    {
        #region Member Fields

        protected Camera camera;
        protected Matrix worldTransform;
        protected Matrix compoundViewMatrix;
        protected Matrix leftCompoundMatrix;
        protected Matrix rightCompoundMatrix;
        protected BoundingFrustum leftViewFrustum;
        protected BoundingFrustum rightViewFrustum;
        protected BoundingFrustum viewFrustum;
        protected bool isStereo;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a scene graph node that holds the camera properties of the viewer with a specified
        /// node name.
        /// </summary>
        /// <param name="name">The name of this camera node</param>
        /// <param name="camera">The actual camera properties associated with this node</param>
        public CameraNode(String name, Camera camera)
            : base(name)
        {
            this.camera = camera;
            
            worldTransform = Matrix.Identity;
            compoundViewMatrix = Matrix.Identity;
            leftCompoundMatrix = Matrix.Identity;
            rightCompoundMatrix = Matrix.Identity;

            if (camera != null)
            {
                if (camera is StereoCamera)
                    isStereo = true;
                else
                    isStereo = false;

                viewFrustum = new BoundingFrustum(camera.Projection);
                leftViewFrustum = new BoundingFrustum(camera.Projection);
                rightViewFrustum = new BoundingFrustum(camera.Projection);
            }
        }

        /// <summary>
        /// Creates a scene graph node that holds the camera properties of the viewer.
        /// </summary>
        /// <param name="camera"></param>
        public CameraNode(Camera camera) : this("", camera) { }

        public CameraNode() : this(null) { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the actual camera class associated with this node.
        /// </summary>
        public virtual Camera Camera
        {
            get { return camera; }
            set 
            { 
                camera = value;
                if (camera is StereoCamera)
                    isStereo = true;
                else
                    isStereo = false;
            }
        }

        /// <summary>
        /// Gets the view matrix of the mono (non-stereo) camera.
        /// </summary>
        public virtual Matrix CompoundViewMatrix
        {
            get { return compoundViewMatrix; }
            internal set
            {
                if (!compoundViewMatrix.Equals(value))
                {
                    compoundViewMatrix = value;
                    viewFrustum = new BoundingFrustum(value * camera.Projection);
                }
            }
        }

        /// <summary>
        /// Gets the world transformation of the viewer.
        /// </summary>
        public virtual Matrix WorldTransformation
        {
            get { return worldTransform; }
            internal set 
            {
                worldTransform = value;
            }
        }

        /// <summary>
        /// Gets the view matrix of the left eye of the stereo camera.
        /// </summary>
        /// <exception cref="GoblinException">If non-stereo camera is used</exception>
        public virtual Matrix LeftCompoundViewMatrix
        {
            get
            {
                if (!isStereo)
                    throw new GoblinException("Use CompoundViewMatrix for mono (non-stereo) camera");

                return leftCompoundMatrix;
            }
            internal set
            {
                if (!isStereo)
                    throw new GoblinException("Use CompoundViewMatrix for mono (non-stereo) camera");

                leftCompoundMatrix = value;
                leftViewFrustum = new BoundingFrustum(value * ((StereoCamera)camera).LeftProjection);
            }
        }

        /// <summary>
        /// Gets the view matrix of the right eye of the stereo camera.
        /// </summary>
        /// <exception cref="GoblinException">If non-stereo camera is used</exception>
        public virtual Matrix RightCompoundViewMatrix
        {
            get
            {
                if (!isStereo)
                    throw new GoblinException("Use CompoundViewMatrix for mono (non-stereo) camera");

                return rightCompoundMatrix;
            }
            internal set
            {
                if (!isStereo)
                    throw new GoblinException("Use CompoundViewMatrix for mono (non-stereo) camera");

                rightCompoundMatrix = value;
                rightViewFrustum = new BoundingFrustum(value * ((StereoCamera)camera).RightProjection);
            }
        }

        /// <summary>
        /// Gets whether the camera is a stereo camera.
        /// </summary>
        public virtual bool Stereo
        {
            get { return isStereo; }
        }

        /// <summary>
        /// Gets the camera frustum that is defined with top, bottom, left, right, near, and
        /// far clipping planes.
        /// </summary>
        public virtual BoundingFrustum BoundingFrustum
        {
            get { return viewFrustum; }
            internal set { viewFrustum = value;  }
        }

        /// <summary>
        /// Gets the camera frustum of the left eye for a stereo camera.
        /// </summary>
        /// <exception cref="GoblinException">If non-stereo camera is used</exception>
        public virtual BoundingFrustum LeftBoundingFrustum
        {
            get
            {
                if (!isStereo)
                    throw new GoblinException("Use BoundingFrustum for non-stereo camera");

                return leftViewFrustum;
            }
            internal set
            {
                if (!isStereo)
                    throw new GoblinException("Use BoundingFrustum for non-stereo camera");

                leftViewFrustum = value;
            }
        }

        /// <summary>
        /// Gets the camera frustum of the right eye for a stereo camera.
        /// </summary>
        /// <exception cref="GoblinException">If non-stereo camera is used</exception>
        public virtual BoundingFrustum RightBoundingFrustum
        {
            get
            {
                if (!isStereo)
                    throw new GoblinException("Use BoundingFrustum for non-stereo camera");

                return rightViewFrustum;
            }
            internal set
            {
                if (!isStereo)
                    throw new GoblinException("Use BoundingFrustum for non-stereo camera");

                rightViewFrustum = value;
            }
        }
        #endregion

        #region Overriden Methods

        public override Node CloneNode()
        {
            CameraNode node = (CameraNode) base.CloneNode();
            if (isStereo)
            {
                node.Camera = new StereoCamera((StereoCamera)camera);
                node.LeftCompoundViewMatrix = leftCompoundMatrix;
                node.RightCompoundViewMatrix = rightCompoundMatrix;
            }
            else
            {
                node.Camera = new Camera(camera);
                node.CompoundViewMatrix = compoundViewMatrix;
                node.WorldTransformation = worldTransform;
            }

            return node;
        }

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            xmlNode.SetAttribute("Stereo", "" + isStereo);

            xmlNode.AppendChild(camera.Save(xmlDoc));

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            if (xmlNode.HasAttribute("Stereo"))
                isStereo = bool.Parse(xmlNode.GetAttribute("Stereo"));

            camera = (Camera)Activator.CreateInstance(Type.GetType(xmlNode.FirstChild.Name));
            camera.Load((XmlElement)xmlNode.FirstChild);

            viewFrustum = new BoundingFrustum(camera.Projection);
            leftViewFrustum = new BoundingFrustum(camera.Projection);
            rightViewFrustum = new BoundingFrustum(camera.Projection);
        }
#endif

        #endregion
    }
}
