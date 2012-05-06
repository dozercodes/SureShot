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

using GoblinXNA.Shaders;
using GoblinXNA.Graphics;
using GoblinXNA.Helpers;
using GoblinXNA.Physics;

namespace GoblinXNA.Graphics
{
    /// <summary>
    /// This implementation is suitable for DirectX models and primitive shapes.
    /// </summary>
    public class Model : IModel, IPhysicsMeshProvider 
    {
        #region Fields

        protected bool useLighting;
        protected ShadowAttribute shadowAttribute;

        protected ModelMeshCollection mesh;

        protected BoundingBox boundingBox;
        protected BoundingSphere boundingSphere;
        protected bool showBoundingBox;
        protected Matrix offsetTransform;
        protected bool offsetToOrigin;
        protected int triangleCount;

        protected IShader shader;
        protected String technique;
        protected List<IShader> afterEffectShaders;
        protected Matrix[] transforms;
        protected Dictionary<string, Matrix> animationTransforms;
        protected List<Vector3> vertices;
        protected List<int> indices;
        protected bool useInternalMaterials;

        protected String modelLoaderName;
        protected String resourceName;
        protected String shaderName;

        protected bool boundingBoxCalculated;
        protected Matrix prevRenderMatrix;

        protected ModelMeshPart curPart;

        #region Temporary Variables

        protected Matrix tmpMat1;
        protected Matrix tmpMat2;
        protected Vector3 tmpVec1;

        #endregion

        #endregion

        #region Constructors

        public Model() : this(null, null) { }

        /// <summary>
        /// Creates a model with information loaded from a model file
        /// </summary>
        /// <param name="transforms">Transforms applied to each model meshes</param>
        /// <param name="mesh">A collection of model meshes</param>
        public Model(Matrix[] transforms, ModelMeshCollection mesh)
        {
            this.transforms = transforms;
            this.mesh = mesh;

            offsetTransform = Matrix.Identity;
            offsetToOrigin = false;

            vertices = new List<Vector3>();
            indices = new List<int>();

            shader = new SimpleEffectShader();
            technique = "";
            afterEffectShaders = new List<IShader>();

            animationTransforms = new Dictionary<string, Matrix>();

            resourceName = "";
#if !WINDOWS_PHONE
            shaderName = TypeDescriptor.GetClassName(shader);
#endif
            modelLoaderName = "";

            useInternalMaterials = false;
            useLighting = true;
            shadowAttribute = ShadowAttribute.None;
            showBoundingBox = false;
            boundingBoxCalculated = false;

            CalculateTriangleCount();
            CalculateMinimumBoundingSphere();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Flag reflecting whether lighting should be used when rendering this model. 
        /// The default value is true.
        /// </summary>
        public virtual bool UseLighting
        {
            get { return useLighting; }
            set { useLighting = value; }
        }

        /// <summary>
        /// Gets or sets whether to use the material setting set inside the model file.
        /// The default value is false.
        /// </summary>
        public virtual bool UseInternalMaterials
        {
            get { return useInternalMaterials; }
            set
            { 
                useInternalMaterials = value;

                if (shader is IAlphaBlendable)
                    foreach (ModelMesh modelMesh in this.mesh)
                        ((IAlphaBlendable)shader).SetOriginalAlphas(modelMesh.Effects);
            }
        }

        /// <summary>
        /// Gets or sets whether the model's internal texture or material contains
        /// transparency. Make sure to set this to true if your model contains transparency,
        /// otherwise, everything will be drawn in opaque color by default for optimization
        /// purpose.
        /// </summary>
        /// <remarks>Effective only if UseInternalMaterials property is set to true</remarks>
        /// <see cref="UseInternalMaterials"/>
        public virtual bool ContainsTransparency
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets how this model will be used when shadow mapping is in use. The default value
        /// is ShadowAttribute.None. 
        /// </summary>
        public virtual ShadowAttribute ShadowAttribute
        {
            get { return shadowAttribute; }
            set { shadowAttribute = value; }
        }

        public virtual IShader Shader
        {
            get { return shader; }
            set 
            { shader = value; }
        }

        public virtual String ShaderTechnique
        {
            get { return technique; }
            set { technique = value; }
        }

        public virtual List<IShader> AfterEffectShaders
        {
            get { return afterEffectShaders; }
            set { afterEffectShaders = value; }
        }

        public virtual ModelMeshCollection Mesh
        {
            get { return mesh; }
        }

        public virtual List<Vector3> Vertices
        {
            get { return vertices; }
        }

        public virtual List<int> Indices
        {
            get { return indices; }
        }

        public virtual PrimitiveType PrimitiveType
        {
            get { return PrimitiveType.TriangleList; }
        }

        /// <summary>
        /// Gets the minimum bounding box used for display and by the physics engine.
        /// </summary>
        public virtual BoundingBox MinimumBoundingBox
        {
            get 
            {
                if (!boundingBoxCalculated)
                    CalculateMinimumBoundingBox();

                return boundingBox; 
            }
        }

        /// <summary>
        /// Gets the minimum bounding sphere that bounds this model mesh
        /// </summary>
        public virtual BoundingSphere MinimumBoundingSphere
        {
            get { return boundingSphere; }
        }

        /// <summary>
        /// Gets the offset transformation from the origin of the world coordinate.
        /// </summary>
        /// <remarks>
        /// If not provided, this transform will be calculated based on the following equation:
        /// OffsetTranslation.Translation = (MinimumBoundingBox.Min + MinimumBoundingBox.Max) / 2.
        /// In this case, no rotation offset will be calculated.
        /// </remarks>
        public virtual Matrix OffsetTransform
        {
            get { return offsetTransform; }
            internal set { offsetTransform = value; }
        }
        
        /// <summary>
        /// Gets or sets whether to relocate the model to the origin. Each model has its 
        /// position stored in the model file, but if you want to relocate the model to the 
        /// origin instead of locating it based on the position stored in the file, you should
        /// set this to true. The default value is false.
        /// </summary>
        public virtual bool OffsetToOrigin
        {
            get { return offsetToOrigin; }
            set { offsetToOrigin = value; }
        }

        /// <summary>
        /// Gets or sets whether to draw the minimum bounding box around the model.
        /// The default value is false.
        /// </summary>
        public virtual bool ShowBoundingBox
        {
            get { return showBoundingBox; }
            set 
            {
                showBoundingBox = value; 

                if (!boundingBoxCalculated)
                    CalculateMinimumBoundingBox();
            }
        }

        /// <summary>
        /// Gets the triangle count of this model
        /// </summary>
        public virtual int TriangleCount
        {
            get { return triangleCount; }
        }

        /// <summary>
        /// Gets or sets the name of the resource (asset name) used to create this model. 
        /// This name should not contain any extensions.
        /// </summary>
        /// <remarks>
        /// This information is necessary for saving and loading scene graph from an XML file.
        /// </remarks>
        /// <see cref="ModelLoaderName"/>
        public virtual String ResourceName
        {
            get { return resourceName; }
            set { resourceName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the model loader if this model was loaded using a specific
        /// model loader.
        /// </summary>
        /// <remarks>
        /// This information is necessary for saving and loading scene graph from an XML file if
        /// the ResourceName is specified.
        /// </remarks>
        public virtual String ModelLoaderName
        {
            get { return modelLoaderName; }
            set { modelLoaderName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the shader used to illuminate this model. The default value 
        /// is SimpleEffectShader.
        /// </summary>
        /// <remarks>
        /// This information is necessary for saving and loading scene graph from an XML file.
        /// </remarks>
        public virtual String ShaderName
        {
            get { return shaderName; }
            set { shaderName = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the minimum bounding sphere used for visibility testing against view frustum.
        /// </summary>
        protected virtual void CalculateMinimumBoundingSphere()
        {
            bool needTransform = false;

            foreach (ModelMesh modelMesh in mesh)
            {
                if(transforms != null)
                    needTransform = !transforms[modelMesh.ParentBone.Index].Equals(Matrix.Identity);

                if (needTransform)
                {
                    tmpMat1 = Matrix.CreateTranslation(modelMesh.BoundingSphere.Center);
                    Matrix.Multiply(ref tmpMat1, ref transforms[modelMesh.ParentBone.Index],
                        out tmpMat2);
                    BoundingSphere bSphere = new BoundingSphere(tmpMat2.Translation,
                        modelMesh.BoundingSphere.Radius);

                    if (boundingSphere.Radius == 0)
                        boundingSphere = bSphere;
                    else
                        BoundingSphere.CreateMerged(ref boundingSphere, ref bSphere,
                            out boundingSphere);
                }
                else
                {
                    if (boundingSphere.Radius == 0)
                        boundingSphere = modelMesh.BoundingSphere;
                    else
                        boundingSphere = BoundingSphere.CreateMerged(boundingSphere, 
                            modelMesh.BoundingSphere);
                }
            }
        }

        /// <summary>
        /// Calcuates the triangle count of this model.
        /// </summary>
        protected virtual void CalculateTriangleCount()
        {
            triangleCount = 0;

            foreach (ModelMesh modelMesh in mesh)
            {
                foreach (ModelMeshPart part in modelMesh.MeshParts)
                {
                    triangleCount += part.PrimitiveCount;
                }
            }
        }

        /// <summary>
        /// Calculates the minimum bounding box that fits this model.
        /// </summary>
        protected virtual void CalculateMinimumBoundingBox()
        {
            bool needTransform = false;
            int baseVertex = 0;

            foreach (ModelMesh modelMesh in mesh)
            {
                if(transforms != null)
                    needTransform = !transforms[modelMesh.ParentBone.Index].Equals(Matrix.Identity);

                foreach (ModelMeshPart part in modelMesh.MeshParts)
                {
                    baseVertex = vertices.Count;

                    Vector3[] data = new Vector3[part.NumVertices];

                    // Read the format of the vertex buffer  
                    VertexDeclaration declaration = part.VertexBuffer.VertexDeclaration;
                    VertexElement[] vertexElements = declaration.GetVertexElements();
                    // Find the element that holds the position  
                    VertexElement vertexPosition = new VertexElement();
                    foreach (VertexElement vert in vertexElements)
                    {
                        if (vert.VertexElementUsage == VertexElementUsage.Position &&
                            vert.VertexElementFormat == VertexElementFormat.Vector3)
                        {
                            vertexPosition = vert;
                            // There should only be one  
                            break;
                        }
                    }

                    // Check the position element found is valid  
                    if (vertexPosition == null ||
                        vertexPosition.VertexElementUsage != VertexElementUsage.Position ||
                        vertexPosition.VertexElementFormat != VertexElementFormat.Vector3)
                    {
                        throw new Exception("Model uses unsupported vertex format!");
                    }

                    part.VertexBuffer.GetData<Vector3>(part.VertexOffset * declaration.VertexStride + vertexPosition.Offset,
                        data, 0, part.NumVertices, declaration.VertexStride);

                    if (needTransform)
                    {
                        Matrix transform = transforms[modelMesh.ParentBone.Index];
                        for (int ndx = 0; ndx < data.Length; ndx++)
                        {
                            Vector3.Transform(ref data[ndx], ref transform, out data[ndx]);
                        }
                    }

                    vertices.AddRange(data);

                    int[] tmpIndices = new int[part.PrimitiveCount * 3];

                    if (part.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
                    {
                        ushort[] tmp = new ushort[part.PrimitiveCount * 3];
                        part.IndexBuffer.GetData<ushort>(part.StartIndex * 2, tmp, 0,
                            tmp.Length);
                        Array.Copy(tmp, 0, tmpIndices, 0, tmpIndices.Length);
                    }
                    else
                        part.IndexBuffer.GetData<int>(part.StartIndex * 2, tmpIndices, 0,
                            tmpIndices.Length);

                    if (baseVertex != 0)
                        for (int i = 0; i < tmpIndices.Length; i++)
                            tmpIndices[i] += baseVertex;

                    indices.AddRange(tmpIndices);
                }
            }

            if (vertices.Count == 0)
            {
                throw new GoblinException("Corrupted model vertices. Failed to calculate MBB.");
            }
            else
            {
                boundingBox = BoundingBox.CreateFromPoints(vertices);
                boundingSphere = BoundingSphere.CreateFromPoints(vertices);
                if(offsetTransform.Equals(Matrix.Identity))
                    offsetTransform.Translation = (boundingBox.Min + boundingBox.Max) / 2;
                boundingBoxCalculated = true;
            }
        }

        /// <summary>
        /// Copies only the geometry (Mesh, AnimatedMesh, 
        /// MinimumBoundingBox, MinimumBoundingSphere, TriangleCount and Transforms)
        /// </summary>
        /// <param name="model">A source model from which to copy</param>
        public virtual void CopyGeometry(IModel model)
        {
            if (!(model is Model))
                return;

            Model srcModel = (Model)model;
            vertices.AddRange(((IPhysicsMeshProvider)model).Vertices);
            indices.AddRange(((IPhysicsMeshProvider)model).Indices);

            triangleCount = srcModel.TriangleCount;
            boundingBox = srcModel.MinimumBoundingBox;
            boundingSphere = srcModel.MinimumBoundingSphere;
            UseInternalMaterials = srcModel.UseInternalMaterials;
        }

        /// <summary>
        /// Updates the transforms of any animated meshes.
        /// </summary>
        /// <param name="meshName"></param>
        /// <param name="animationTransform"></param>
        public virtual void UpdateAnimationTransforms(string meshName, Matrix animationTransform)
        {
            if (animationTransforms.ContainsKey(meshName))
                animationTransforms[meshName] = animationTransform;
            else
                animationTransforms.Add(meshName, animationTransform);
        }

        /// <summary>
        /// Renders the model itself as well as the minimum bounding box if showBoundingBox
        /// is true. By default, SimpleEffectShader is used to render the model.
        /// </summary>
        /// <remarks>
        /// This function is called automatically to render the model, so do not call this method
        /// </remarks>
        /// <param name="material">Material properties of this model</param>
        /// <param name="renderMatrix">Transform of this model</param>
        public virtual void Render(ref Matrix renderMatrix, Material material)
        {
            if (!UseInternalMaterials)
            {
                material.InternalEffect = null;
                if ((shader.CurrentMaterial != material) || material.HasChanged)
                {
                    shader.SetParameters(material);

                    foreach (IShader afterEffect in afterEffectShaders)
                        afterEffect.SetParameters(material);

                    material.HasChanged = false;
                }
            }

            BlendState origState = null;
            if (ContainsTransparency)
            {
                origState = State.Device.BlendState;
                State.AlphaBlendingEnabled = true;
            }

            // Render the actual model
            foreach (ModelMesh modelMesh in this.mesh)
            {
                Matrix.Multiply(ref transforms[modelMesh.ParentBone.Index], ref renderMatrix, out tmpMat1);
                if (animationTransforms.Count > 0 && animationTransforms.ContainsKey(modelMesh.Name))
                    tmpMat1 = animationTransforms[modelMesh.Name] * tmpMat1;

                foreach (ModelMeshPart part in modelMesh.MeshParts)
                {
                    if (UseInternalMaterials)
                    {
                        material.InternalEffect = part.Effect;
                        shader.SetParameters(material);
                    }

                    String techniqueName = technique;
                    if (String.IsNullOrEmpty(techniqueName))
                        techniqueName = part.Effect.CurrentTechnique.Name;
                    curPart = part;
                    shader.Render(
                        ref tmpMat1,
                        techniqueName,
                        SubmitGeometry);

                    foreach (IShader afterEffect in afterEffectShaders)
                    {
                        if (UseInternalMaterials)
                            afterEffect.SetParameters(material);

                        afterEffect.Render(
                            ref tmpMat1,
                            "",
                            ResubmitGeometry);
                    }
                }
            }

            shader.RenderEnd();
            foreach (IShader afterEffect in afterEffectShaders)
                afterEffect.RenderEnd();

            if (ContainsTransparency)
                State.Device.BlendState = origState;

            if (showBoundingBox)
                RenderBoundingBox(ref renderMatrix);
        }

        public virtual void PrepareShadows(ref Matrix renderMatrix)
        {
            if (!(shader is IShadowShader))
                return;

            foreach (ModelMesh modelMesh in this.mesh)
            {
                Matrix.Multiply(ref transforms[modelMesh.ParentBone.Index], ref renderMatrix, out tmpMat1);
                if (animationTransforms.Count > 0 && animationTransforms.ContainsKey(modelMesh.Name))
                    tmpMat1 = animationTransforms[modelMesh.Name] * tmpMat1;

                foreach (ModelMeshPart part in modelMesh.MeshParts)
                {
                    curPart = part;

                    ((IShadowShader)shader).ShadowMap.ComputeShadow(
                        ref tmpMat1,
                        SubmitGeometry);
                }
            }
        }

        /// <summary>
        /// Submits the vertex and index streams to the shader
        /// </summary>
        protected virtual void SubmitGeometry()
        {
            State.Device.SetVertexBuffer(curPart.VertexBuffer);
            State.Device.Indices = curPart.IndexBuffer;
            State.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                curPart.VertexOffset, 0, curPart.NumVertices, curPart.StartIndex, curPart.PrimitiveCount);
        }

        /// <summary>
        /// Submits the vertex and index streams to the shader without setting the index stream assuming
        /// it's already been set to the device
        /// </summary>
        protected virtual void ResubmitGeometry()
        {
            State.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                curPart.VertexOffset, 0, curPart.NumVertices, curPart.StartIndex, curPart.PrimitiveCount);
        }

        protected virtual void RenderBoundingBox(ref Matrix renderMatrix)
        {
            Vector3[] corners = boundingBox.GetCorners();
            for (int i = 0; i < corners.Length; i++)
                Vector3.Transform(ref corners[i], ref renderMatrix, out corners[i]);

            DebugShapeRenderer.AddBoundingBox(corners, State.BoundingBoxColor, 0);
        }

        /// <summary>
        /// Disposes of model contents.
        /// </summary>
        public virtual void Dispose()
        {
            vertices.Clear();
            mesh = null;

            if (shader != null)
                shader.Dispose();
        }

#if !WINDOWS_PHONE
        public virtual XmlElement SaveModelCreationInfo(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = xmlDoc.CreateElement("ModelCreationInfo");

            if (resourceName.Length == 0)
                throw new GoblinException("ResourceName must be specified in order to " +
                    "save this model information to an XML file");

            xmlNode.SetAttribute("ResourceName", resourceName);
            if (modelLoaderName.Length == 0)
                throw new GoblinException("ModelLoaderName must be specified");

            xmlNode.SetAttribute("ModelLoaderName", modelLoaderName);

            xmlNode.SetAttribute("ShaderName", shaderName);
            if (technique.Length > 0)
                xmlNode.SetAttribute("ShaderTechniqueName", technique);

            return xmlNode;
        }

        public virtual XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = xmlDoc.CreateElement(TypeDescriptor.GetClassName(this));

            xmlNode.SetAttribute("UseLighting", useLighting.ToString());
            xmlNode.SetAttribute("ShadowAttribute", shadowAttribute.ToString());
            xmlNode.SetAttribute("ShowBoundingBox", showBoundingBox.ToString());
            xmlNode.SetAttribute("UseInternalMaterials", useInternalMaterials.ToString());
            xmlNode.SetAttribute("OffsetToOrigin", offsetToOrigin.ToString());

            return xmlNode;
        }

        public virtual void Load(XmlElement xmlNode)
        {
            if (xmlNode.HasAttribute("UseLighting"))
                useLighting = bool.Parse(xmlNode.GetAttribute("UseLighting"));
            if (xmlNode.HasAttribute("ShadowAttribute"))
                shadowAttribute = (ShadowAttribute)Enum.Parse(typeof(ShadowAttribute),
                    xmlNode.GetAttribute("CastShadow"));
            if (xmlNode.HasAttribute("ShowBoundingBox"))
                showBoundingBox = bool.Parse(xmlNode.GetAttribute("ShowBoundingBox"));
            if (xmlNode.HasAttribute("UseInternalMaterials"))
                useInternalMaterials = bool.Parse(xmlNode.GetAttribute("UseInternalMaterials"));
            if (xmlNode.HasAttribute("OffsetToOrigin"))
                offsetToOrigin = bool.Parse(xmlNode.GetAttribute("OffsetToOrigin"));
        }
#endif

        #endregion
    }
}
