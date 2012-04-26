/************************************************************************************ 
 * Copyright (c) 2008-2011, Columbia University
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
    /// An implementation of IModel interface for models created with CustomMesh.
    /// </summary>
    public class PrimitiveModel : IModel, IPhysicsMeshProvider 
    {
        #region Fields

        protected bool useLighting;
        protected ShadowAttribute shadowAttribute;
        protected bool useVertexColor;

        protected CustomMesh customMesh;
        protected BoundingBox boundingBox;
        protected BoundingSphere boundingSphere;
        protected bool showBoundingBox;
        protected Matrix offsetTransform;
        protected bool offsetToOrigin;
        protected int triangleCount;

        protected IShader shader;
        protected String technique;
        protected List<IShader> afterEffectShaders;

        protected List<Vector3> vertices;
        protected List<int> indices;

        protected String shaderName;
        protected String customShapeParameters;

        #region Temporary Variables

        protected Matrix tmpMat1;
        protected Matrix tmpMat2;
        protected Vector3 tmpVec1;

        #endregion

        #endregion

        #region Constructors

        public PrimitiveModel() : this(null) { }

        /// <summary>
        /// Creates a model with VertexBuffer and IndexBuffer.
        /// </summary>
        /// <param name="customMesh">A mesh defined with VertexBuffer and IndexBuffer</param>
        public PrimitiveModel(CustomMesh customMesh)
        {
            offsetTransform = Matrix.Identity;
            offsetToOrigin = false;

            vertices = new List<Vector3>();
            indices = new List<int>();

            this.customMesh = customMesh;
            shader = new SimpleEffectShader();
            afterEffectShaders = new List<IShader>();

            customShapeParameters = "";
#if !WINDOWS_PHONE
            shaderName = TypeDescriptor.GetClassName(shader);
#endif

            useLighting = true;
            shadowAttribute = ShadowAttribute.None;
            showBoundingBox = false;
            useVertexColor = false;
            technique = "";

            if (customMesh != null)
            {
                CalculateMinimumBoundingBox();
                triangleCount = customMesh.NumberOfPrimitives;
            }
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
        /// Gets or sets how this model will be used when shadow mapping is in use. The default value
        /// is ShadowAttribute.None. 
        /// </summary>
        public virtual ShadowAttribute ShadowAttribute
        {
            get { return shadowAttribute; }
            set { shadowAttribute = value; }
        }
        
        /// <summary>
        /// Gets or sets whether to use the vertex color instead of material information to
        /// render this model.
        /// </summary>
        public virtual bool UseVertexColor
        {
            get { return useVertexColor; }
            set 
            { 
                useVertexColor = value;
                if (shader is SimpleEffectShader)
                    ((SimpleEffectShader)shader).UseVertexColor = value;
            }
        }

        public virtual IShader Shader
        {
            get { return shader; }
            set { shader = value; }
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

        public List<Vector3> Vertices
        {
            get { return vertices; }
        }

        public List<int> Indices
        {
            get { return indices; }
        }

        public PrimitiveType PrimitiveType
        {
            get{ return customMesh.PrimitiveType; }
        }

        /// <summary>
        /// Gets the mesh defined with VertexBuffer and IndexBuffer .
        /// </summary>
        public CustomMesh CustomMesh
        {
            get { return customMesh; }
        }

        /// <summary>
        /// Gets the minimum bounding box used for display and by the physics engine.
        /// </summary>
        public BoundingBox MinimumBoundingBox
        {
            get { return boundingBox; }
        }

        /// <summary>
        /// Gets the minimum bounding sphere that bounds this model mesh
        /// </summary>
        public BoundingSphere MinimumBoundingSphere
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
        public Matrix OffsetTransform
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
            set { showBoundingBox = value; }
        }

        /// <summary>
        /// Gets the triangle count of this model
        /// </summary>
        public int TriangleCount
        {
            get { return triangleCount; }
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

        /// <summary>
        /// Gets or sets the parameters needed to be passed to a class that contructs a primitive
        /// shape. 
        /// </summary>
        /// <remarks>
        /// This information is necessary for saving and loading scene graph from an XML file.
        /// </remarks>
        public virtual String CustomShapeParameters
        {
            get { return customShapeParameters; }
            set { customShapeParameters = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the minimum bounding box that fits this model.
        /// </summary>
        protected virtual void CalculateMinimumBoundingBox()
        {
            int stride = customMesh.VertexDeclaration.VertexStride;
            int numberv = customMesh.NumberOfVertices;
            byte[] data = new byte[stride * numberv];

            customMesh.VertexBuffer.GetData<byte>(data);

            for (int ndx = 0; ndx < data.Length; ndx += stride)
            {
                tmpVec1.X = BitConverter.ToSingle(data, ndx);
                tmpVec1.Y = BitConverter.ToSingle(data, ndx + 4);
                tmpVec1.Z = BitConverter.ToSingle(data, ndx + 8);
                vertices.Add(tmpVec1);
            }

            if (customMesh.IndexBuffer.BufferUsage == BufferUsage.None)
            {
                short[] tmpIndices = new short[customMesh.IndexBuffer.IndexCount];
                customMesh.IndexBuffer.GetData<short>(tmpIndices);
                int[] tmpIntIndices = new int[tmpIndices.Length];
                Array.Copy(tmpIndices, tmpIntIndices, tmpIndices.Length);
                indices.AddRange(tmpIntIndices);
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
                    offsetTransform.Translation = -(boundingBox.Min + boundingBox.Max) / 2;
            }
        }

        /// <summary>
        /// Copies only the geometry (Mesh, customMesh, AnimatedMesh, 
        /// MinimumBoundingBox, MinimumBoundingSphere, TriangleCount and Transforms)
        /// </summary>
        /// <param name="model">A source model from which to copy</param>
        public virtual void CopyGeometry(IModel model)
        {
            if (!(model is PrimitiveModel))
                return;

            PrimitiveModel srcModel = (PrimitiveModel)model;
            vertices.AddRange(((IPhysicsMeshProvider)model).Vertices);
            indices.AddRange(((IPhysicsMeshProvider)model).Indices);
            customMesh = srcModel.customMesh;

            triangleCount = srcModel.TriangleCount;
            boundingBox = srcModel.MinimumBoundingBox;
            boundingSphere = srcModel.MinimumBoundingSphere;
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
            if ((shader.CurrentMaterial != material) || material.HasChanged)
            {
                shader.SetParameters(material);

                foreach (IShader afterEffect in afterEffectShaders)
                    afterEffect.SetParameters(material);

                material.HasChanged = false;
            }

            shader.Render(
                ref renderMatrix,
                technique,
                SubmitGeometry);

            foreach (IShader afterEffect in afterEffectShaders)
            {
                afterEffect.Render(
                    ref renderMatrix,
                    technique,
                    ResubmitGeometry);
            }

            if (showBoundingBox)
                RenderBoundingBox(ref renderMatrix);
        }

        public virtual void PrepareShadows(ref Matrix renderMatrix)
        {
            if (!(shader is IShadowShader))
                return;

            ((IShadowShader)shader).ShadowMap.ComputeShadow(
                ref renderMatrix,
                SubmitGeometry);
        }

        /// <summary>
        /// Submits the vertex and index streams to the shader
        /// </summary>
        protected virtual void SubmitGeometry()
        {
            State.Device.SetVertexBuffer(customMesh.VertexBuffer);
            State.Device.Indices = customMesh.IndexBuffer;
            State.Device.DrawIndexedPrimitives(customMesh.PrimitiveType,
                0, 0, customMesh.NumberOfVertices, 0, customMesh.NumberOfPrimitives);
        }

        /// <summary>
        /// Submits the vertex and index streams to the shader without setting the index stream assuming
        /// it's already been set to the device
        /// </summary>
        protected virtual void ResubmitGeometry()
        {
            State.Device.DrawIndexedPrimitives(customMesh.PrimitiveType,
                0, 0, customMesh.NumberOfVertices, 0, customMesh.NumberOfPrimitives);
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

            if (shader != null)
                shader.Dispose();
            if (customMesh != null)
                customMesh.Dispose();
        }

#if !WINDOWS_PHONE
        public virtual XmlElement SaveModelCreationInfo(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = xmlDoc.CreateElement("ModelCreationInfo");

            if (customShapeParameters.Length == 0)
                throw new GoblinException("CustomShapeParameters must be specified");

            xmlNode.SetAttribute("CustomShapeParameters", customShapeParameters);

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
            if (xmlNode.HasAttribute("OffsetToOrigin"))
                offsetToOrigin = bool.Parse(xmlNode.GetAttribute("OffsetToOrigin"));
        }
#endif

        #endregion
    }
}
