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
 * Author: Nicolas Dedual (dedual@cs.columbia.edu)
 * 
 *************************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA;
using GoblinXNA.Helpers;
using GoblinXNA.UI.UI3D;
using GoblinXNA.Graphics.Geometry;
 
// 3D Text rendering library from http://nuclexframework.codeplex.com/
// Nuclex.Fonts used here is modified a little bit to better suite Goblin
// framework
 
using Nuclex.Fonts;


namespace GoblinXNA.Graphics
{
    /// <summary>
    /// A 3D text primitive using Nuclex.Fonts library
    /// </summary>
    public class Text3D : PrimitiveModel
    {
        #region Fields

        private VectorFont basicTextVectorFont;
        private String basicTextToRender;
        private Text basicTextIn3D;
        private UI3DRenderer.Text3DStyle styleToUse;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a 3D text with the font name (the asset name of the spritefont file) and the text to render
        /// </summary>
        /// <param name="fontToUse">String of the font type to use (the asset name of the spritefont file)</param>
        /// <param name="textToRender">String of the text to render</param>
        public Text3D(String fontToUse, String textToRender):
            this(fontToUse, textToRender, UI3DRenderer.Text3DStyle.Fill)
        {
        }

        /// <summary>
        /// Create a 3D text with the font name (the asset name of the spritefont file), the text to render,
        /// and the mesh style
        /// </summary>
        /// <param name="fontToUse">String of the font type to use (the asset name of the spritefont file)</param>
        /// <param name="textToRender">String of the text to render</param>
        /// <param name="styleToUse">Type of rendering to use in how we create the 3D mesh</param>
        /// 
        public Text3D(String fontToUse, String textToRender, UI3DRenderer.Text3DStyle styleToUse)
            : base()
        {
            basicTextToRender = textToRender;
            basicTextVectorFont = State.Content.Load<VectorFont>(fontToUse);
            this.styleToUse = styleToUse;

            CreateText();
        }

        /// <summary>
        /// Create a 3D text with the font, the text to render, and the mesh style
        /// </summary>
        /// <param name="fontToUse">Font type to use</param>
        /// <param name="textToRender">String of the text to render</param>
        /// <param name="styleToUse">Type of rendering to use in how we create the 3D mesh</param>
        public Text3D(VectorFont fontToUse, String textToRender, UI3DRenderer.Text3DStyle styleToUse)
            : base()
        {
            basicTextToRender = textToRender;
            basicTextVectorFont = fontToUse;
            this.styleToUse = styleToUse;

            CreateText();
        }

        #endregion

        #region Private Static Method

        private void CreateShape()
        {
            switch (styleToUse)
            {
                case UI3DRenderer.Text3DStyle.Outline:
                    basicTextIn3D = basicTextVectorFont.Outline(basicTextToRender);
                    break;
                case UI3DRenderer.Text3DStyle.Fill:
                    basicTextIn3D = basicTextVectorFont.Fill(basicTextToRender);
                    break;
                case UI3DRenderer.Text3DStyle.Extrude:
                    basicTextIn3D = basicTextVectorFont.Extrude(basicTextToRender);
                    break;
            }
        }

        /// <summary>
        /// Actually builds the text
        /// </summary>
        /// <returns> The custom mesh that has all of the geometry information</returns>
        private void CreateText()
        {
            customMesh = new CustomMesh();

            CreateShape();

            foreach (VertexPositionNormalTexture aVertex in basicTextIn3D.Vertices)
            {
                vertices.Add(aVertex.Position);
            }

            if(styleToUse == UI3DRenderer.Text3DStyle.Fill)
                for (int i = 0; i < basicTextIn3D.Vertices.Length; ++i)
                    basicTextIn3D.Vertices[i].Normal = -basicTextIn3D.Vertices[i].Normal;

            foreach (short index in basicTextIn3D.Indices)
            {
                indices.Add((int)index);
            }

            customMesh.VertexDeclaration = VertexPositionNormalTexture.VertexDeclaration;

            customMesh.VertexBuffer = new VertexBuffer(State.Device, typeof(VertexPositionNormalTexture),
                basicTextIn3D.Vertices.Length, BufferUsage.None);

            customMesh.VertexBuffer.SetData(basicTextIn3D.Vertices);

            customMesh.IndexBuffer = new IndexBuffer(State.Device, typeof(short), basicTextIn3D.Indices.Length,
               BufferUsage.None);
            customMesh.IndexBuffer.SetData(basicTextIn3D.Indices);

            customMesh.NumberOfVertices = basicTextIn3D.Vertices.Length;
            customMesh.NumberOfPrimitives = basicTextIn3D.Indices.Length / 3;
            customMesh.PrimitiveType = basicTextIn3D.PrimitiveType;

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
            }

            triangleCount = indices.Count / 3;
        }

        public override void Render(ref Matrix renderMatrix, Material material)
        {
            RasterizerState origState = State.Device.RasterizerState;
            State.Device.RasterizerState = RasterizerState.CullNone;
            base.Render(ref renderMatrix, material);
            State.Device.RasterizerState = origState;
        }

        #endregion
    }
}
