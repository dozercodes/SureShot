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
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;

namespace GoblinXNA.Graphics.Geometry
{
    /// <summary>
    /// A quad/plane geometry primitive with texture coordinates constructed with CustomMesh
    /// </summary>
    public class TexturedPlane : PrimitiveModel
    {
        #region Constructors

        /// <summary>
        /// Create a plane with the given dimensions and texture coordinates.
        /// </summary>
        /// <param name="xdim">X dimension</param>
        /// <param name="ydim">Y dimension</param>
        /// <param name="texCoordX"></param>
        /// <param name="texCoordY"></param>
        /// <param name="texCoordWidth"></param>
        /// <param name="texCoordHeight"></param>
        public TexturedPlane(float xdim, float ydim, float texCoordX, float texCoordY,
            float texCoordWidth, float texCoordHeight)
            : base()
        {
            CreatePlane(xdim, ydim, texCoordX, texCoordY, texCoordWidth, texCoordHeight);
            customShapeParameters = xdim + ", " + ydim + ", " + texCoordX + ", " + texCoordY + ", "
                + texCoordWidth + ", " + texCoordHeight;

            triangleCount = customMesh.NumberOfPrimitives;
        }

        /// <summary>
        /// Create a plane with the given dimensions.
        /// </summary>
        /// <param name="xdim"></param>
        /// <param name="ydim"></param>
        public TexturedPlane(float xdim, float ydim)
            : this(xdim, ydim, 1)
        {
        }

        /// <summary>
        /// Create a plane with the given dimensions and texture scale factor.
        /// </summary>
        /// <param name="xdim"></param>
        /// <param name="ydim"></param>
        /// <param name="texCoordScale"></param>
        public TexturedPlane(float xdim, float ydim, float texCoordScale)
            : this(xdim, ydim, (1 - texCoordScale) / 2, (1 - texCoordScale) / 2, texCoordScale, texCoordScale)
        {
        }

        /// <summary>
        /// Create a plane from an XML input.
        /// </summary>
        /// <param name="xmlParams"></param>
        public TexturedPlane(params String[] xmlParams)
            : this(float.Parse(xmlParams[0]), float.Parse(xmlParams[1]), float.Parse(xmlParams[2]),
            float.Parse(xmlParams[3]), float.Parse(xmlParams[4]), float.Parse(xmlParams[5]))
        {
        }

        #endregion

        #region Private Static Method

        private void CreatePlane(float xdim, float ydim, float texCoordX, float texCoordY,
            float texCoordWidth, float texCoordHeight)
        {
            customMesh = new CustomMesh();

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
            Vector3 halfExtent = new Vector3();

            halfExtent.X = xdim / 2;
            halfExtent.Y = ydim / 2;

            Vector3 v0 = Vector3Helper.Get(-halfExtent.X, 0, -halfExtent.Y);
            Vector3 v1 = Vector3Helper.Get(halfExtent.X, 0, -halfExtent.Y);
            Vector3 v2 = Vector3Helper.Get(-halfExtent.X, 0, halfExtent.Y);
            Vector3 v3 = Vector3Helper.Get(halfExtent.X, 0, halfExtent.Y);

            this.vertices.Add(v0);
            this.vertices.Add(v1);
            this.vertices.Add(v2);
            this.vertices.Add(v3);

            Vector3 pY = Vector3.UnitY;

            vertices[0].Position = v0; vertices[1].Position = v1;
            vertices[2].Position = v2; vertices[3].Position = v3;

            vertices[0].TextureCoordinate = new Vector2(texCoordX, texCoordY);
            vertices[1].TextureCoordinate = new Vector2(texCoordX + texCoordWidth, texCoordY);
            vertices[2].TextureCoordinate = new Vector2(texCoordX, texCoordY + texCoordHeight);
            vertices[3].TextureCoordinate = new Vector2(texCoordX + texCoordWidth, texCoordY + texCoordHeight);

            for (int i = 0; i < 4; i++)
                vertices[i].Normal = pY;

            customMesh.VertexDeclaration = VertexPositionNormalTexture.VertexDeclaration;

            customMesh.VertexBuffer = new VertexBuffer(State.Device, typeof(VertexPositionNormalTexture),
                4, BufferUsage.None);
            customMesh.VertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            short[] indices = new short[6];

            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 2; indices[4] = 1; indices[5] = 3;

            for (int i = 0; i < indices.Length; ++i)
                this.indices.Add(indices[i]);

            customMesh.IndexBuffer = new IndexBuffer(State.Device, typeof(short), 6,
                BufferUsage.None);
            customMesh.IndexBuffer.SetData(indices);

            customMesh.NumberOfVertices = 4;
            customMesh.NumberOfPrimitives = 2;

            if (this.vertices.Count == 0)
            {
                throw new GoblinException("Corrupted model vertices. Failed to calculate MBB.");
            }
            else
            {
                boundingBox = BoundingBox.CreateFromPoints(this.vertices);
                boundingSphere = BoundingSphere.CreateFromPoints(this.vertices);
                if (offsetTransform.Equals(Matrix.Identity))
                    offsetTransform.Translation = (boundingBox.Min + boundingBox.Max) / 2;
            }
        }

        #endregion
    }
}

