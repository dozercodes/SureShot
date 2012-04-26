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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;

namespace GoblinXNA.Graphics.Geometry
{
    /// <summary>
    /// A box geometry primitive with texture coordinates constructed with CustomMesh
    /// </summary>
    public class TexturedBox : PrimitiveModel
    {
        #region Constructors

        /// <summary>
        /// Create a box with the given dimensions. The texture coordinates are assigned
        /// using positional spherical mapping.
        /// </summary>
        /// <param name="xdim">X dimension</param>
        /// <param name="ydim">Y dimension</param>
        /// <param name="zdim">Z dimension</param>
        public TexturedBox(float xdim, float ydim, float zdim)
            : base(CreateBox(xdim, ydim, zdim))
        {
            customShapeParameters = xdim + ", " + ydim + ", " + zdim;
        }

        /// <summary>
        /// Creates a box whose edges are the given length. The texture coordinates are assigned
        /// using positional spherical mapping.
        /// </summary>
        /// <param name="length">Length of each side of the cube</param>
        public TexturedBox(float length)
            : base(CreateBox(length, length, length))
        {
            customShapeParameters = length + ", " + length + ", " + length;
        }

        /// <summary>
        /// Create a box with the given dimensions. The texture coordinates are assigned
        /// using positional spherical mapping.
        /// </summary>
        /// <param name="dimension">Dimension of the box geometry</param>
        public TexturedBox(Vector3 dimension)
            : base(CreateBox(dimension.X, dimension.Y, dimension.Z))
        {
            customShapeParameters = dimension.X + ", " + dimension.Y + ", " + dimension.Z;
        }

        /// <summary>
        /// Create a box from an XML input.
        /// </summary>
        /// <param name="xmlParams"></param>
        public TexturedBox(params String[] xmlParams)
            : base(CreateBox(float.Parse(xmlParams[0]), float.Parse(xmlParams[1]),
                float.Parse(xmlParams[2])))
        {
            customShapeParameters = xmlParams[0] + ", " + xmlParams[1] + ", " + xmlParams[2];
        }

        #endregion

        #region Private Static Method

        private static CustomMesh CreateBox(float xdim, float ydim, float zdim)
        {
            CustomMesh mesh = new CustomMesh();

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[24];
            Vector3 halfExtent = new Vector3();

            halfExtent.X = xdim / 2;
            halfExtent.Y = ydim / 2;
            halfExtent.Z = zdim / 2;

            Vector3 v0 = Vector3Helper.Get(-halfExtent.X, -halfExtent.Y, -halfExtent.Z);
            Vector3 v1 = Vector3Helper.Get(halfExtent.X, -halfExtent.Y, -halfExtent.Z);
            Vector3 v2 = Vector3Helper.Get(-halfExtent.X, halfExtent.Y, -halfExtent.Z);
            Vector3 v3 = Vector3Helper.Get(halfExtent.X, halfExtent.Y, -halfExtent.Z);
            Vector3 v4 = Vector3Helper.Get(halfExtent.X, halfExtent.Y, halfExtent.Z);
            Vector3 v5 = Vector3Helper.Get(-halfExtent.X, halfExtent.Y, halfExtent.Z);
            Vector3 v6 = Vector3Helper.Get(halfExtent.X, -halfExtent.Y, halfExtent.Z);
            Vector3 v7 = Vector3Helper.Get(-halfExtent.X, -halfExtent.Y, halfExtent.Z);

            Vector3 nZ = -Vector3.UnitZ;
            Vector3 pZ = Vector3.UnitZ;
            Vector3 nX = -Vector3.UnitX;
            Vector3 pX = Vector3.UnitX;
            Vector3 nY = -Vector3.UnitY;
            Vector3 pY = Vector3.UnitY;

            vertices[0].Position = v0; vertices[1].Position = v1;
            vertices[2].Position = v2; vertices[3].Position = v3;

            vertices[4].Position = v0; vertices[5].Position = v7;
            vertices[6].Position = v2; vertices[7].Position = v5;

            vertices[8].Position = v4; vertices[9].Position = v5;
            vertices[10].Position = v7; vertices[11].Position = v6;

            vertices[12].Position = v4; vertices[13].Position = v3;
            vertices[14].Position = v1; vertices[15].Position = v6;

            vertices[16].Position = v2; vertices[17].Position = v4;
            vertices[18].Position = v5; vertices[19].Position = v3;

            vertices[20].Position = v0; vertices[21].Position = v1;
            vertices[22].Position = v6; vertices[23].Position = v7;

            for (int i = 0; i < 4; i++)
                vertices[i].Normal = nZ;
            for (int i = 4; i < 8; i++)
                vertices[i].Normal = nX;
            for (int i = 8; i < 12; i++)
                vertices[i].Normal = pZ;
            for (int i = 12; i < 16; i++)
                vertices[i].Normal = pX;
            for (int i = 16; i < 20; i++)
                vertices[i].Normal = pY;
            for (int i = 20; i < 24; i++)
                vertices[i].Normal = nY;

            // Assign texture coordinates using positional spherical mapping
            Vector3 v;
            for (int i = 0; i < vertices.Length; ++i)
            {
                v = vertices[i].Position;
                v.Normalize();
                vertices[i].TextureCoordinate = new Vector2(
                    (float)Math.Asin(v.X) / MathHelper.Pi + 0.5f,
                    (float)Math.Asin(v.Y) / MathHelper.Pi + 0.5f);
            }

            mesh.VertexDeclaration = VertexPositionNormalTexture.VertexDeclaration;

            mesh.VertexBuffer = new VertexBuffer(State.Device, typeof(VertexPositionNormalTexture),
                24, BufferUsage.None);
            mesh.VertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            short[] indices = new short[36];

            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 2; indices[4] = 1; indices[5] = 3;
            indices[6] = 4; indices[7] = 6; indices[8] = 5;
            indices[9] = 6; indices[10] = 7; indices[11] = 5;
            indices[12] = 11; indices[13] = 10; indices[14] = 9;
            indices[15] = 11; indices[16] = 9; indices[17] = 8;
            indices[18] = 14; indices[19] = 15; indices[20] = 13;
            indices[21] = 15; indices[22] = 12; indices[23] = 13;
            indices[24] = 19; indices[25] = 17; indices[26] = 18;
            indices[27] = 19; indices[28] = 18; indices[29] = 16;
            indices[30] = 21; indices[31] = 20; indices[32] = 23;
            indices[33] = 21; indices[34] = 23; indices[35] = 22;

            mesh.IndexBuffer = new IndexBuffer(State.Device, typeof(short), 36,
                BufferUsage.None);
            mesh.IndexBuffer.SetData(indices);

            mesh.NumberOfVertices = 24;
            mesh.NumberOfPrimitives = 12;

            return mesh;
        }

        #endregion
    }
}
