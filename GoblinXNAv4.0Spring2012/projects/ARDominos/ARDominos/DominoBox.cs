/************************************************************************************ 
 * Copyright (c) 2008-2009, Columbia University
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA;
using GoblinXNA.Graphics;
using Model = GoblinXNA.Graphics.Model;

namespace ARDominos
{
    /// <summary>
    /// A box geometry primitive constructed with PrimitiveMesh representing a domino model
    /// </summary>
    public class DominoBox : PrimitiveModel
    {
        /// <summary>
        /// Create a domino box model with the given dimension
        /// </summary>
        /// <param name="dimension">Dimension of the box geometry</param>
        /// <param name="textCoord">The lower-left corner of the texture coordinates</param>
        public DominoBox(Vector3 dimension, Vector2 textCoord)
            : base(CreateBox(dimension, textCoord))
        {
        }

        private static CustomMesh CreateBox(Vector3 dimension, Vector2 textCoord)
        {
            CustomMesh mesh = new CustomMesh();

            // Even though we only need 8 vertices to create a cube/box geometry, because
            // XNA uses vertex normals instead of face normals, we need 3 duplicate vertices
            // on each of the 8 corners of the cube/box geometry, so that each of them
            // can have different normals depending on the face the vertex belongs to.
            // Also, because we want to texture map as well as illuminate the model with
            // lights, we use a vertex structure that contains position, normal, and texture 
            // coornidate information.
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[24];
            Vector3 halfExtent = dimension / 2;

            // Create 8 vertex positions that will be assigned to those 24 vertices
            Vector3 v0 = new Vector3(-halfExtent.X, -halfExtent.Y, -halfExtent.Z);
            Vector3 v1 = new Vector3(halfExtent.X, -halfExtent.Y, -halfExtent.Z);
            Vector3 v2 = new Vector3(-halfExtent.X, halfExtent.Y, -halfExtent.Z);
            Vector3 v3 = new Vector3(halfExtent.X, halfExtent.Y, -halfExtent.Z);
            Vector3 v4 = new Vector3(halfExtent.X, halfExtent.Y, halfExtent.Z);
            Vector3 v5 = new Vector3(-halfExtent.X, halfExtent.Y, halfExtent.Z);
            Vector3 v6 = new Vector3(halfExtent.X, -halfExtent.Y, halfExtent.Z);
            Vector3 v7 = new Vector3(-halfExtent.X, -halfExtent.Y, halfExtent.Z);

            // Since there are 6 different face directions for a cube/box geometry,
            // we create 6 normal vectors to assign to the faces
            Vector3 nZ = new Vector3(0, 0, -1);
            Vector3 pZ = new Vector3(0, 0, 1);
            Vector3 nX = new Vector3(-1, 0, 0);
            Vector3 pX = new Vector3(1, 0, 0);
            Vector3 nY = new Vector3(0, -1, 0);
            Vector3 pY = new Vector3(0, 1, 0);

            Vector2 t0 = new Vector2(0, 0);
            Vector2 t1 = new Vector2(textCoord.X, 0);
            Vector2 t2 = new Vector2(0, textCoord.Y);
            Vector2 t3 = new Vector2(textCoord.X, textCoord.Y);

            // Assign the 8 positions to the 24 vertices
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

            // Assign the texture coordinates to only those faces we care about texture mapping
            vertices[4].TextureCoordinate = t3; vertices[5].TextureCoordinate = t1;
            vertices[6].TextureCoordinate = t2; vertices[7].TextureCoordinate = t0;

            vertices[12].TextureCoordinate = t1; vertices[13].TextureCoordinate = t3;
            vertices[14].TextureCoordinate = t2; vertices[15].TextureCoordinate = t0;

            // Assign the appropriate normal vectors to each of the triangle faces
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

            mesh.VertexDeclaration = VertexPositionNormalTexture.VertexDeclaration;

            mesh.VertexBuffer = new VertexBuffer(State.Device,
                typeof(VertexPositionNormalTexture), 24, BufferUsage.None);
            mesh.VertexBuffer.SetData(vertices);

            // Since there are 12 triangle faces (6 rectangle faces, and each rectangle face contains
            // 2 triangle faces), we need 36 indices (12 triangles * 3 vertices/triangle)
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
    }
}
