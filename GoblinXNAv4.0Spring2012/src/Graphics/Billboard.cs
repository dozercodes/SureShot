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
using System.ComponentModel;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Shaders;

namespace GoblinXNA.Graphics
{
    public class Billboard : PrimitiveModel
    {
        private Matrix tmpMat1;

        public Billboard(Vector2 dimension) :
            base()
        {
            shader = new AlphaTestShader();
#if !WINDOWS_PHONE
            shaderName = TypeDescriptor.GetClassName(shader);
#endif

            CreateBillboardMesh(dimension);

            CalculateMinimumBoundingBox();
            triangleCount = customMesh.NumberOfPrimitives;
        }

        private void CreateBillboardMesh(Vector2 dimension)
        {
            this.customMesh = new CustomMesh();

            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            float halfX = dimension.X / 2;
            float halfY = dimension.Y / 2;
            vertices[0].Position = new Vector3(-halfX, halfY, 0);
            vertices[1].Position = new Vector3(halfX, halfY, 0);
            vertices[2].Position = new Vector3(halfX, -halfY, 0);
            vertices[3].Position = new Vector3(-halfX, -halfY, 0);

            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].TextureCoordinate = new Vector2(1, 0);
            vertices[2].TextureCoordinate = new Vector2(1, 1);
            vertices[3].TextureCoordinate = new Vector2(0, 1);

            this.customMesh.VertexDeclaration = VertexPositionTexture.VertexDeclaration;

            this.customMesh.VertexBuffer = new VertexBuffer(State.Device,
                typeof(VertexPositionTexture), 4, BufferUsage.None);
            this.customMesh.VertexBuffer.SetData(vertices);

            short[] indices = new short[6];

            indices[0] = 0; indices[1] = 1; indices[2] = 3;
            indices[3] = 2; indices[4] = 3; indices[5] = 1;

            this.customMesh.IndexBuffer = new IndexBuffer(State.Device, typeof(short), 6,
                BufferUsage.None);
            this.customMesh.IndexBuffer.SetData(indices);

            this.customMesh.NumberOfVertices = 4;
            this.customMesh.NumberOfPrimitives = 2;
        }

        public override void Render(ref Matrix renderMatrix, Material material)
        {
            if ((shader.CurrentMaterial != material) || material.HasChanged)
            {
                shader.SetParameters(material);

                foreach (IShader afterEffect in afterEffectShaders)
                    afterEffect.SetParameters(material);

                material.HasChanged = false;
            }

            tmpMat1 = Matrix.CreateBillboard(State.CameraTransform.Translation,
                renderMatrix.Translation,
                State.ViewMatrix.Up,
                State.ViewMatrix.Forward);
            tmpMat1.Translation = renderMatrix.Translation;

            shader.Render(
                ref tmpMat1,
                technique,
                SubmitGeometry);

            foreach (IShader afterEffect in afterEffectShaders)
            {
                afterEffect.Render(
                    ref tmpMat1,
                    technique,
                    ResubmitGeometry);
            }

            if (showBoundingBox)
                RenderBoundingBox(ref tmpMat1);
        }
    }
}
