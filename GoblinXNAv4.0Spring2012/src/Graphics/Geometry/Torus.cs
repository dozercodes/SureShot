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
    /// A torus geometry primitive constructed with CustomMesh
    /// </summary>
    public class Torus : PrimitiveModel
    {
        #region Constructors

        /// <summary>
        /// Creates a torus of the given inner and outer radius around the Y axis centered around 
        /// the origin. The torus is subdivided around the Y axis into slices and around the
        /// torus tubes into stacks.
        /// </summary>
        /// <param name="inner">The inner radius of the torus. This has to be greater than or
        /// equal to 0 and less than outer radius.</param>
        /// <param name="outer">The outer radius of the torus. This has to be greater than 0
        /// and larger than the inner radius</param>
        /// <param name="slices">Specifies the number of subdivisions around the Y axis.
        /// This has to be greater than 4 and greater than 101.</param>
        /// <param name="stacks">Specifies the number of subdivisions around the torus tube. 
        /// This has to be greater than 4 and greater than 101.</param>
        public Torus(float inner, float outer, int slices, int stacks)
            : base(CreateTorus(inner, outer, slices, stacks))
        {
            customShapeParameters = inner + ", " + outer + ", " + slices + ", " + stacks;
        }

        public Torus(params String[] xmlParams)
            : base(CreateTorus(float.Parse(xmlParams[0]), float.Parse(xmlParams[1]),
                int.Parse(xmlParams[2]), int.Parse(xmlParams[3])))
        {
            customShapeParameters = xmlParams[0] + ", " + xmlParams[1] + ", " + xmlParams[2]
                + ", " + xmlParams[3];
        }

        #endregion

        #region Private Static Methods

        private static CustomMesh CreateTorus(float inner, float outer, int slices, int stacks)
        {
            if (slices < 5)
                throw new ArgumentException("Cannot draw a torus with slices less than 5");
            if (slices > 100)
                throw new ArgumentException("Cannot draw a torus with slices greater than 100");
            if (stacks < 5)
                throw new ArgumentException("Cannot draw a torus with stacks less than 5");
            if (stacks > 100)
                throw new ArgumentException("Cannot draw a torus with stacks greater than 100");
            if (inner < 0)
                throw new ArgumentException("Inner radius has to be greater than or equal to 0");
            if (outer <= 0)
                throw new ArgumentException("Outer radius has to be greater than 0");
            if (inner >= outer)
                throw new ArgumentException("Inner radius has to be less than outer radius");

            CustomMesh mesh = new CustomMesh();

            List<VertexPositionNormal> vertices = new List<VertexPositionNormal>();

            // Add the vertices
            double thetaIncr = Math.PI * 2 / slices;
            double thaiIncr = Math.PI * 2 / stacks;
            int countA, countB = 0;
            double theta, thai;
            float c = (outer + inner) / 2;
            float a = (outer - inner) / 2;
            double common;
            Vector3 tubeCenter;
            for (countA = 0, theta = 0; countA < slices; theta += thetaIncr, countA++)
            {
                tubeCenter = Vector3Helper.Get((float)(c * Math.Cos(theta)), 0, (float)(c * Math.Sin(theta)));
                for (countB = 0, thai = 0; countB < stacks; thai += thaiIncr, countB++)
                {
                    VertexPositionNormal vert = new VertexPositionNormal();
                    common = (c + a * Math.Cos(thai));
                    vert.Position = Vector3Helper.Get((float)(common * Math.Cos(theta)),
                        (float)(a * Math.Sin(thai)), (float)(common * Math.Sin(theta)));
                    vert.Normal = Vector3.Normalize(vert.Position - tubeCenter);
                    vertices.Add(vert);
                }
            }

            mesh.VertexDeclaration = VertexPositionNormal.VertexDeclaration;

            mesh.VertexBuffer = new VertexBuffer(State.Device, typeof(VertexPositionNormal),
                vertices.Count, BufferUsage.None);
            mesh.VertexBuffer.SetData(vertices.ToArray());

            List<short> indices = new List<short>();

            for (int i = 0; i < slices - 1; i++)
            {
                for (int j = 0; j < stacks - 1; j++)
                {
                    indices.Add((short)(i * stacks + j));
                    indices.Add((short)((i + 1) * stacks + j));
                    indices.Add((short)(i * stacks + j + 1));

                    indices.Add((short)(i * stacks + j + 1));
                    indices.Add((short)((i + 1) * stacks + j));
                    indices.Add((short)((i + 1) * stacks + j + 1));
                }

                indices.Add((short)((i + 1) * stacks - 1));
                indices.Add((short)((i + 2) * stacks - 1));
                indices.Add((short)(i * stacks));

                indices.Add((short)(i * stacks));
                indices.Add((short)((i + 2) * stacks - 1));
                indices.Add((short)((i + 1) * stacks));
            }

            for (int j = 0; j < stacks - 1; j++)
            {
                indices.Add((short)((slices - 1) * stacks + j));
                indices.Add((short)j);
                indices.Add((short)((slices - 1) * stacks + j + 1));

                indices.Add((short)((slices - 1) * stacks + j + 1));
                indices.Add((short)j);
                indices.Add((short)(j + 1));
            }
            indices.Add((short)(slices * stacks - 1));
            indices.Add((short)(slices - 1));
            indices.Add((short)((slices - 1) * stacks));

            indices.Add((short)((slices - 1) * stacks));
            indices.Add((short)(slices - 1));
            indices.Add((short)0);

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
