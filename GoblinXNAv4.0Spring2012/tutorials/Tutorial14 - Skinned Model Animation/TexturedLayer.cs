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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA;
using GoblinXNA.Graphics;

using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Helpers;

namespace Tutorial14___Skinned_Model_Animation
{
    public class TexturedLayer : PrimitiveModel
    {
        public TexturedLayer(Vector2 dimension)
            : base(CreateLayer(dimension))
        {
        }

        private static CustomMesh CreateLayer(Vector2 dimension)
        {
            CustomMesh mesh = new CustomMesh();

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
            Vector2 halfExtent = dimension / 2;

            Vector3 v0 = Vector3Helper.Get(-halfExtent.X, 0, -halfExtent.Y);
            Vector3 v1 = Vector3Helper.Get(-halfExtent.X, 0, halfExtent.Y);
            Vector3 v2 = Vector3Helper.Get(halfExtent.X, 0, halfExtent.Y);
            Vector3 v3 = Vector3Helper.Get(halfExtent.X, 0, -halfExtent.Y);

            vertices[0].Position = v0;
            vertices[1].Position = v1;
            vertices[2].Position = v2;
            vertices[3].Position = v3;

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = Vector3.UnitY;

            vertices[1].TextureCoordinate = new Vector2(0, 1);
            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[2].TextureCoordinate = new Vector2(1, 1);
            vertices[3].TextureCoordinate = new Vector2(1, 0);

            mesh.VertexDeclaration = VertexPositionNormalTexture.VertexDeclaration;

            mesh.VertexBuffer = new VertexBuffer(State.Device,
                typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None);
            mesh.VertexBuffer.SetData(vertices);

            short[] indices = new short[6];

            indices[0] = 0; indices[1] = 2; indices[2] = 1;
            indices[3] = 2; indices[4] = 0; indices[5] = 3;

            mesh.IndexBuffer = new IndexBuffer(State.Device, typeof(short), 6,
                BufferUsage.None);
            mesh.IndexBuffer.SetData(indices);

            mesh.NumberOfVertices = 4;
            mesh.NumberOfPrimitives = 2;

            return mesh;
        }
    }
}
