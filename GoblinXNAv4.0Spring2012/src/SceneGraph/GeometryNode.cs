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
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;
using GoblinXNA.Physics;
using GoblinXNA.Network;
using GoblinXNA.Helpers;

namespace GoblinXNA.SceneGraph
{
    /// <summary>
    /// A scene graph node that holds the model geometry, physical properties, etc.
    /// </summary>
    public class GeometryNode : BranchNode
    {
        #region Field Members

        protected IModel model;
        protected bool isRendered;
        private bool shouldRender;
        protected Material material;
        protected List<LightNode> illuminationLights;
        /// <summary>
        /// Indicates whether the local lights need to be updated in its shader
        /// </summary>
        protected bool needsToUpdateLocalLights; 
        protected Matrix worldTransform;
        protected Matrix markerTransform;
        private bool markerTransformSet;

        protected IPhysicsObject physicsProperties;

        protected BoundingSphere boundingVolume;
        protected bool showBoundingVolume;

        protected bool addToPhysicsEngine;
        protected bool isOccluder;

        protected bool physicsStateChanged;
        protected bool occlusionStateChanged;

        protected bool ignoreDepth;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a geometry node that contains the actual geometry information required for
        /// rendering, physics simulation, and networking with a specified node name.
        /// </summary>
        /// <param name="name">
        /// The name of this geometry node (has to be unique for correct networking behavior)
        /// </param>
        public GeometryNode(String name)
            : base(name)
        {
            model = null;
            worldTransform = Matrix.Identity;
            markerTransform = Matrix.Identity;
            material = new Material();
            illuminationLights = new List<LightNode>();
            needsToUpdateLocalLights = false;
            isOccluder = false;
            addToPhysicsEngine = false;
            markerTransformSet = false;

            physicsProperties = new PhysicsObject(this);

            boundingVolume = new BoundingSphere();
            showBoundingVolume = false;
            ignoreDepth = false;

            shouldRender = false;
            physicsStateChanged = false;
            occlusionStateChanged = false;
        }

        /// <summary>
        /// Creates a geometry node that contains the actual geometry information required for
        /// rendering, physics simulation, and networking.
        /// </summary>
        public GeometryNode() : this("") { }

        #endregion

        #region Properties

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                //physicsStateChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the actual model used for rendering and physics simulation.
        /// </summary>
        public virtual IModel Model
        {
            get { return model; }
            set 
            { 
                model = value;
                if (model != null)
                {
                    physicsProperties.Model = model;
                    if (model is IPhysicsMeshProvider)
                        physicsProperties.MeshProvider = (IPhysicsMeshProvider)model;
                    boundingVolume.Radius = model.MinimumBoundingSphere.Radius;

                    // needs to update the lighting settings of the shader used for the model
                    needsToUpdateLocalLights = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the material properties of this model.
        /// </summary>
        public virtual Material Material
        {
            get { return material; }
            set { material = value; }
        }

        /// <summary>
        /// Gets or sets the list of local light sources that will be used for illumination.
        /// </summary>
        internal List<LightNode> LocalLights
        {
            get { return illuminationLights; }
            set { illuminationLights = value; }
        }

        internal bool NeedsToUpdateLocalLights
        {
            get { return needsToUpdateLocalLights; }
            set { needsToUpdateLocalLights = value; }
        }

        /// <summary>
        /// Gets or sets whether to add this geometry node to the physics engine.
        /// </summary>
        public virtual bool AddToPhysicsEngine
        {
            get { return addToPhysicsEngine; }
            set 
            {
                if (addToPhysicsEngine != value)
                {
                    addToPhysicsEngine = value;
                    physicsStateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether AddToPhysicsEngine property has been changed.
        /// </summary>
        internal bool PhysicsStateChanged
        {
            get { return physicsStateChanged; }
            set { physicsStateChanged = value; }
        }

        /// <summary>
        /// Gets or sets whether IsOccluder property is changed. 
        /// </summary>
        internal bool OcclusionStateChanged
        {
            get { return occlusionStateChanged; }
            set { occlusionStateChanged = value; }
        }

        /// <summary>
        /// Gets or sets the physics properties associated with this geometry node.
        /// </summary>
        public virtual IPhysicsObject Physics
        {
            get { return physicsProperties; }
            set 
            { 
                physicsProperties = value;
                physicsProperties.Model = model;
                if (model is IPhysicsMeshProvider)
                    physicsProperties.MeshProvider = (IPhysicsMeshProvider)model;
            }
        }

        /// <summary>
        /// Gets the transformation of the model.
        /// </summary>
        public virtual Matrix WorldTransformation
        {
            get { return worldTransform; }
            set { worldTransform = value; }
        }

        /// <summary>
        /// Gets the transform updated by a marker. This information is valid only when
        /// at least one of its successor is a MarkerNode.
        /// </summary>
        public virtual Matrix MarkerTransform
        {
            get { return markerTransform; }
            internal set 
            { 
                markerTransform = value;
                if(!markerTransform.Equals(Matrix.Identity))
                    markerTransformSet = true;
            }
        }

        /// <summary>
        /// Gets whether the MarkerTransform property is valid (non Identity matrix)
        /// </summary>
        public bool MarkerTransformSet
        {
            get { return markerTransformSet; }
        }

        /// <summary>
        /// Gets or sets whether this node is added to rendering routine
        /// </summary>
        internal bool IsRendered
        {
            get { return isRendered; }
            set 
            { 
                isRendered = value;
                //physicsStateChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets whether this node should be rendered
        /// </summary>
        internal bool ShouldRender
        {
            get { return shouldRender; }
            set { shouldRender = value; }
        }

        /// <summary>
        /// Gets or sets whether this node is used as an occluder that occludes any object
        /// that is rendered behind this object
        /// </summary>
        public virtual bool IsOccluder
        {
            get { return isOccluder; }
            set 
            {
                if (isOccluder != value)
                {
                    isOccluder = value;
                    occlusionStateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets a sphere that encloses the contents of all the nodes below the current one.
        /// </summary>
        /// <remarks>
        /// This node itself is not included in this bounding sphere
        /// </remarks>
        public virtual BoundingSphere BoundingVolume
        {
            get { return boundingVolume; }
            internal set { boundingVolume = value; }
        }

        /// <summary>
        /// Gets or sets if the bounding volume (sphere) should be displayed.
        /// (Currently not implemented)
        /// </summary>
        public virtual bool ShowBoundingVolume
        {
            get { return showBoundingVolume; }
            set { showBoundingVolume = value; }
        }

        /// <summary>
        /// Gets or sets whether to ignore the depth buffer for drawing this geometry.
        /// Default value is false.
        /// </summary>
        /// <remarks>
        /// Anything other than the drawing (e.g., physics simulation) will still take the 
        /// depth into consideration even if this is set to true.
        /// </remarks>
        public virtual bool IgnoreDepth
        {
            get { return ignoreDepth; }
            set { ignoreDepth = value; }
        }

        /// <summary>
        /// Indicates whether to render this geometry regardless of whether the bounding sphere of the 
        /// associated model is within the view frustum of the main camera. The default value is false.
        /// </summary>
        /// <remarks>
        /// This property is ignored if Scene.EnableFrustumCulling is set to false.
        /// </remarks>
        public virtual bool AlwaysRender
        {
            get;
            set;
        }

        #endregion

        #region Override Methods

        public override Node CloneNode()
        {
            throw new GoblinException("You should not clone Geometry node");
        }

#if !WINDOWS_PHONE
        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            xmlNode.SetAttribute("Occluder", isOccluder.ToString());
            xmlNode.SetAttribute("IgnoreDepth", ignoreDepth.ToString());
            xmlNode.SetAttribute("AddToPhysicsEngine", addToPhysicsEngine.ToString());

            if (model != null)
            {
                xmlNode.AppendChild(model.SaveModelCreationInfo(xmlDoc));
                xmlNode.AppendChild(model.Save(xmlDoc));
            }

            xmlNode.AppendChild(material.Save(xmlDoc));
            xmlNode.AppendChild(physicsProperties.Save(xmlDoc));

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            if (xmlNode.HasAttribute("Occluder"))
                isOccluder = bool.Parse(xmlNode.GetAttribute("Occluder"));
            if (xmlNode.HasAttribute("IgnoreDepth"))
                ignoreDepth = bool.Parse(xmlNode.GetAttribute("IgnoreDepth"));
            if (xmlNode.HasAttribute("AddToPhysicsEngine"))
                AddToPhysicsEngine = bool.Parse(xmlNode.GetAttribute("AddToPhysicsEngine"));

            int i = 0;
            if (xmlNode.ChildNodes[i].Name.Equals("ModelCreationInfo"))
            {
                XmlElement modelInfo = (XmlElement)xmlNode.ChildNodes[i];
                if (modelInfo.HasAttribute("ResourceName"))
                {
                    if (!modelInfo.HasAttribute("ModelLoaderName"))
                        throw new GoblinException("ModelLoaderName attribute is required if " +
                            "ResourceName attribute is specified");

                    String assetName = Path.ChangeExtension(modelInfo.GetAttribute("ResourceName"), null);
                    IModelLoader loader = (IModelLoader)Activator.CreateInstance(Type.GetType(
                        modelInfo.GetAttribute("ModelLoaderName")));
                    model = (IModel)loader.Load("", assetName);
                }
                else
                {
                    if (!modelInfo.HasAttribute("CustomShapeParameters"))
                        throw new GoblinException("CustomShapeParameters attribute must be " +
                            "specified if ResourceName is not specified");

                    String[] primShapeParams = modelInfo.GetAttribute("CustomShapeParameters").Split(',');
                    model = (IModel)Activator.CreateInstance(Type.GetType(xmlNode.ChildNodes[i + 1].Name),
                        primShapeParams);
                }

                model.Load((XmlElement)xmlNode.ChildNodes[i + 1]);
                i += 2;
            }

            material = (Material)Activator.CreateInstance(Type.GetType(xmlNode.ChildNodes[i].Name));
            material.Load((XmlElement)xmlNode.ChildNodes[i]);

            physicsProperties = (IPhysicsObject)Activator.CreateInstance(Type.GetType(
                xmlNode.ChildNodes[i + 1].Name));
            physicsProperties.Load((XmlElement)xmlNode.ChildNodes[i + 1]);
            physicsProperties.Container = this;
            if(model is IPhysicsMeshProvider)
                physicsProperties.MeshProvider = (IPhysicsMeshProvider)model;
            physicsProperties.Model = model;
        }
#endif

        public override void Dispose()
        {
            base.Dispose();
            material.Dispose();
            if(model != null)
                model.Dispose();
        }

        #endregion
    }
}
