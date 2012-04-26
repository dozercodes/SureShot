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
    /// A cylinder geometry primitive constructed with CustomMesh
    /// </summary>
    public class Cylinder : PrimitiveModel
    {
        #region Constructors

        /// <summary>
        /// Creates a cylinder (actually a truncated cone) oriented along the Y axis. The base of the cylinder 
        /// is placed at Y = -height/2, and the top at height/2 = height. A cylinder is subdivided around 
        /// the Y axis into slices.
        /// </summary>
        /// <param name="bottom">Specifies the radius of the cylinder at y = -height/2.</param>
        /// <param name="top">Specifies the radius of the cylinder at y = height/2.</param>
        /// <param name="height">Specifies the height of the cylinder.</param>
        /// <param name="slices">
        /// Specifies the number of subdivisions around the Y axis. Should be greater than or equal to 3.
        /// </param>
        public Cylinder(float bottom, float top, float height, int slices)
            : base(CreateCylinder(bottom, top, height, slices))
        {
            customShapeParameters = bottom + ", " + top + ", " + height + ", " + slices;
        }

        public Cylinder(params String[] xmlParams)
            : base(CreateCylinder(float.Parse(xmlParams[0]), float.Parse(xmlParams[1]), 
                float.Parse(xmlParams[2]), int.Parse(xmlParams[3])))
        {
            customShapeParameters = xmlParams[0] + ", " + xmlParams[1] + ", " + xmlParams[2] + ", "
                + xmlParams[3];
        }

        #endregion

        #region Private Static Methods

        private static CustomMesh CreateCylinder(float bottom, float top, float height, int slices)
        {
            if(slices < 3)
                throw new ArgumentException("Cannot draw a cylinder with slices less than 3");
            if (top < 0)
                throw new ArgumentException("Top has to be a positive natural number");
            if (bottom <= 0)
                throw new ArgumentException("Bottom has to be greater than zero");
            if (height <= 0)
                throw new ArgumentException("Height should be greater than zero");

            CustomMesh mesh = new CustomMesh();

            List<VertexPositionNormal> vertices = new List<VertexPositionNormal>();

            // Add top center vertex
            VertexPositionNormal topCenter = new VertexPositionNormal();
            topCenter.Position = Vector3Helper.Get(0, height / 2, 0);
            topCenter.Normal = Vector3Helper.Get(0, 1, 0);

            vertices.Add(topCenter);

            // Add bottom center vertex
            VertexPositionNormal bottomCenter = new VertexPositionNormal();
            bottomCenter.Position = Vector3Helper.Get(0, -height / 2, 0);
            bottomCenter.Normal = Vector3Helper.Get(0, -1, 0);

            vertices.Add(bottomCenter);

            double angle = 0;
            double incr = Math.PI * 2 / slices;
            float cos, sin;
            bool hasTop = (top > 0);
            Vector3 u, v;
            Matrix mat;
            bool tilted = (top != bottom);
            float rotAngle = (float)Math.Atan(bottom / height);
            Vector3 down = -Vector3.UnitY;
            if (hasTop)
            {
                // Add top & bottom side vertices
                for (int i = 0; i <= slices; i++, angle += incr)
                {
                    cos = (float)Math.Cos(angle);
                    sin = (float)Math.Sin(angle);

                    VertexPositionNormal topSide = new VertexPositionNormal();
                    topSide.Position = Vector3Helper.Get(cos * top, height / 2, sin * top);
                    topSide.Normal = Vector3.Normalize(topSide.Position - topCenter.Position);

                    VertexPositionNormal topSide2 = new VertexPositionNormal();
                    topSide2.Position = Vector3Helper.Get(cos * top, height / 2, sin * top);
                    topSide2.Normal = topCenter.Normal;

                    // Add bottom side vertices
                    VertexPositionNormal bottomSide = new VertexPositionNormal();
                    bottomSide.Position = Vector3Helper.Get(cos * bottom, -height / 2, sin * bottom);
                    bottomSide.Normal = Vector3.Normalize(bottomSide.Position - bottomCenter.Position);

                    VertexPositionNormal bottomSide2 = new VertexPositionNormal();
                    bottomSide2.Position = Vector3Helper.Get(cos * bottom, -height / 2, sin * bottom);
                    bottomSide2.Normal = bottomCenter.Normal;

                    if (tilted)
                    {
                        v = topSide.Normal;
                        u = Vector3.Cross(v, down);
                        mat = Matrix.CreateTranslation(v) * Matrix.CreateFromAxisAngle(u, -rotAngle);
                        topSide.Normal = bottomSide.Normal = Vector3.Normalize(mat.Translation);
                    }

                    vertices.Add(topSide);
                    vertices.Add(topSide2);
                    vertices.Add(bottomSide);
                    vertices.Add(bottomSide2);
                }
            }
            else
            {
                // Add top & bottom side vertices
                for (int i = 0; i <= slices; i++, angle += incr)
                {
                    cos = (float)Math.Cos(angle);
                    sin = (float)Math.Sin(angle);

                    VertexPositionNormal topSide = new VertexPositionNormal();
                    topSide.Position = topCenter.Position;

                    // Add bottom side vertices
                    VertexPositionNormal bottomSide = new VertexPositionNormal();
                    bottomSide.Position = Vector3Helper.Get(cos * bottom, -height / 2, sin * bottom);
                    bottomSide.Normal = Vector3.Normalize(bottomSide.Position - bottomCenter.Position);

                    VertexPositionNormal bottomSide2 = new VertexPositionNormal();
                    bottomSide2.Position = Vector3Helper.Get(cos * bottom, -height / 2, sin * bottom);
                    bottomSide2.Normal = bottomCenter.Normal;

                    v = bottomSide.Normal;
                    u = Vector3.Cross(v, down);
                    mat = Matrix.CreateTranslation(v) * Matrix.CreateFromAxisAngle(u, rotAngle);
                    topSide.Normal = bottomSide.Normal = Vector3.Normalize(mat.Translation);

                    vertices.Add(topSide);
                    vertices.Add(bottomSide);
                    vertices.Add(bottomSide2);
                }
            }

            mesh.VertexDeclaration = VertexPositionNormal.VertexDeclaration;

            mesh.VertexBuffer = new VertexBuffer(State.Device, typeof(VertexPositionNormal),
                vertices.Count, BufferUsage.None);
            mesh.VertexBuffer.SetData(vertices.ToArray());

            List<short> indices = new List<short>();

            if (hasTop)
            {
                // Create top & bottom circle 
                for (int i = 2; i < vertices.Count - 7; i += 4)
                {
                    indices.Add(0);
                    indices.Add((short)(i + 1));
                    indices.Add((short)(i + 5));

                    indices.Add(1);
                    indices.Add((short)(i + 7));
                    indices.Add((short)(i + 3));
                }

                // Create side
                for (int i = 2; i < vertices.Count - 7; i += 4)
                {
                    indices.Add((short)i);
                    indices.Add((short)(i + 2));
                    indices.Add((short)(i + 4));

                    indices.Add((short)(i + 4));
                    indices.Add((short)(i + 2));
                    indices.Add((short)(i + 6));
                }
            }
            else
            {
                // Create bottom circle 
                for (int i = 2; i < vertices.Count - 5; i += 3)
                {
                    indices.Add(1);
                    indices.Add((short)(i + 5));
                    indices.Add((short)(i + 2));
                }

                // Create side
                for (int i = 2; i < vertices.Count - 5; i += 3)
                {
                    indices.Add((short)i);
                    indices.Add((short)(i + 1));
                    indices.Add((short)(i + 4));
                }
            }

            mesh.IndexBuffer = new IndexBuffer(State.Device, typeof(short), indices.Count,
                BufferUsage.None);
            mesh.IndexBuffer.SetData(indices.ToArray());

            mesh.NumberOfVertices = vertices.Count;
            mesh.NumberOfPrimitives = indices.Count / 3;

            return mesh;
        }

        #endregion
    }
}
