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
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;

namespace GoblinXNA.Graphics.Geometry
{
    /// <summary>
    /// A capsule geometry primitive constructed with CustomMesh
    /// </summary>
    public class Capsule : PrimitiveModel
    {
        #region Constructors

        /// <summary>
        /// Creates a capsule (a cylinder with hemisphere caps) oriented along the Y axis. 
        /// If the height is less than radius * 2, then the height is clamped to radius * 2.
        /// </summary>
        /// <param name="radius">The radius of the hemisphere caps</param>
        /// <param name="height">The height of the capsule (including the hemisphere caps).
        /// If less than radius * 2, them clamped to radius * 2</param>
        /// <param name="slices">The number of subdivisions around the Y axis (similar to lines 
        /// of longitude). This has to be greater than 4 and less than 101.</param>
        public Capsule(float radius, float height, int slices) : 
            base(CreateCapsule(radius, height, slices))
        {
            customShapeParameters = radius + ", " + height + ", " + slices;
        }

        public Capsule(params String[] xmlParams)
            : base(CreateCapsule(float.Parse(xmlParams[0]), float.Parse(xmlParams[1]),
                int.Parse(xmlParams[2])))
        {
            customShapeParameters = xmlParams[0] + ", " + xmlParams[1] + ", " + xmlParams[2];
        }

        #endregion

        #region Private Static Methods

        private static CustomMesh CreateCapsule(float radius, float height, int slices)
        {
            if(slices < 5)
                throw new ArgumentException("Cannot draw a capsule with slices less than 5");
            if (slices > 100)
                throw new ArgumentException("Cannot draw a capsule with slices greater than 100");
            if (radius <= 0)
                throw new ArgumentException("radius should be greater than zero");
            if (height <= 0)
                throw new ArgumentException("height should be greater than zero");

            if(height < radius * 2)
                height = radius * 2;

            CustomMesh mesh = new CustomMesh();

            List<VertexPositionNormal> vertices = new List<VertexPositionNormal>();

            float cylinderHeight = height - radius * 2;
            Vector3 topCenter = Vector3Helper.Get(0, cylinderHeight / 2, 0);
            Vector3 bottomCenter = Vector3Helper.Get(0, -cylinderHeight / 2, 0);

            double thai = 0, theta = 0;
            double thaiIncr = Math.PI / slices;
            double thetaIncr = Math.PI * 2 / slices;
            int countB, countA;
            // Add top hemisphere vertices
            for (countA = 0, thai = thaiIncr; thai < Math.PI / 2; thai += thaiIncr, countA++)
            {
                for (countB = 0, theta = 0; countB < slices; theta += thetaIncr, countB++)
                {
                    VertexPositionNormal vert = new VertexPositionNormal();
                    vert.Position = Vector3Helper.Get((float)(radius * Math.Sin(thai) * Math.Cos(theta)),
                        (float)(radius * Math.Cos(thai)) + cylinderHeight / 2, 
                        (float)(radius * Math.Sin(thai) * Math.Sin(theta)));
                    vert.Normal = Vector3.Normalize(vert.Position - topCenter);
                    vertices.Add(vert);
                }
            }

            // Add bottom hemisphere vertices
            for (thai = Math.PI / 2; thai < Math.PI - thaiIncr / 2; thai += thaiIncr, countA++)
            {
                for (countB = 0, theta = 0; countB < slices; theta += thetaIncr, countB++)
                {
                    VertexPositionNormal vert = new VertexPositionNormal();
                    vert.Position = Vector3Helper.Get((float)(radius * Math.Sin(thai) * Math.Cos(theta)),
                        (float)(radius * Math.Cos(thai)) - cylinderHeight / 2,
                        (float)(radius * Math.Sin(thai) * Math.Sin(theta)));
                    vert.Normal = Vector3.Normalize(vert.Position - bottomCenter);
                    vertices.Add(vert);
                }
            }

            // Add north pole vertex
            vertices.Add(new VertexPositionNormal(Vector3Helper.Get(0, radius + cylinderHeight / 2, 0),
                Vector3Helper.Get(0, 1, 0)));
            // Add south pole vertex
            vertices.Add(new VertexPositionNormal(Vector3Helper.Get(0, -radius - cylinderHeight / 2, 0),
                Vector3Helper.Get(0, -1, 0)));

            mesh.VertexDeclaration = VertexPositionNormal.VertexDeclaration;

            mesh.VertexBuffer = new VertexBuffer(State.Device, typeof(VertexPositionNormal),
                vertices.Count, BufferUsage.None);
            mesh.VertexBuffer.SetData(vertices.ToArray());

            List<short> indices = new List<short>();

            // Create the north and south pole area mesh
            for (int i = 0, j = (countA - 1) * slices; i < slices - 1; i++, j++)
            {
                indices.Add((short)(vertices.Count - 2));
                indices.Add((short)i);
                indices.Add((short)(i + 1));

                indices.Add((short)(vertices.Count - 1));
                indices.Add((short)(j + 1));
                indices.Add((short)j);
            }
            indices.Add((short)(vertices.Count - 2));
            indices.Add((short)(slices - 1));
            indices.Add((short)0);

            indices.Add((short)(vertices.Count - 1));
            indices.Add((short)((countA - 1) * slices));
            indices.Add((short)(vertices.Count - 3));

            // Create side of the hemispheres
            for (int i = 0; i < countA - 1; i++)
            {
                for (int j = 0; j < slices - 1; j++)
                {
                    indices.Add((short)(i * slices + j));
                    indices.Add((short)((i + 1) * slices + j));
                    indices.Add((short)((i + 1) * slices + j + 1));

                    indices.Add((short)(i * slices + j));
                    indices.Add((short)((i + 1) * slices + j + 1));
                    indices.Add((short)(i * slices + j + 1));
                }

                indices.Add((short)((i + 1) * slices - 1));
                indices.Add((short)((i + 2) * slices - 1));
                indices.Add((short)((i + 1) * slices));

                indices.Add((short)((i + 1) * slices - 1));
                indices.Add((short)((i + 1) * slices));
                indices.Add((short)(i * slices ));
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
