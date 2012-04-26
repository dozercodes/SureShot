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
    /// A disk geometry primitive constructed with CustomMesh
    /// </summary>
    public class Disk : PrimitiveModel
    {
        #region Constructors

        /// <summary>
        /// Creates a disk or annulus on the y = 0 plane. The disk has a radius of outer, and 
        /// contains a concentric circular hole with a radius of inner. If inner is 0, 
        /// then no hole is generated. The disk is subdivided around the Y axis into radial slices 
        /// (like pizza slices)
        /// </summary>
        /// <param name="inner">Specifies the inner radius of the disk (may be 0).</param>
        /// <param name="outer">Specifies the outer radius of the disk. Must be larger than
        /// the 'inner' radius</param>
        /// <param name="slices">Specifies the number of subdivisions around the Y axis. Must be
        /// larger than 2.</param>
        /// <param name="twoSided">Specifies whether to render both front and back side</param>
        public Disk(float inner, float outer, int slices, bool twoSided)
            : base(CreateDisk(inner, outer, slices, 0, Math.PI * 2, twoSided))
        {
            customShapeParameters = inner + ", " + outer + ", " + slices + ", " + twoSided;
        }

        internal Disk(float inner, float outer, int slices, double start, double sweep, bool twoSided)
            : base(CreateDisk(inner, outer, slices, start, sweep, twoSided))
        {
            // these are encoded in PartialDisk class
            /*resourceName = "Disk";
            primitiveShapeParameters = inner + ", " + outer + ", " + slices + ", " + start + ", " +
                sweep + ", " + twoSided;*/
        }

        public Disk(params String[] xmlParams)
            : base(CreateDisk(float.Parse(xmlParams[0]), float.Parse(xmlParams[1]), 0, Math.PI * 2,
                int.Parse(xmlParams[2]), bool.Parse(xmlParams[3])))
        {
            customShapeParameters = xmlParams[0] + ", " + xmlParams[1] + ", " + xmlParams[2]
                + ", " + xmlParams[3];
        }

        #endregion

        #region Private Static Methods

        private static CustomMesh CreateDisk(float inner, float outer, int slices, 
            double start, double sweep, bool twoSided)
        {
            if (slices < 3)
                throw new ArgumentException("Cannot draw a disk with slices less than 3");
            if (inner < 0)
                throw new ArgumentException("Inner radius has to be greater than or equal to 0");
            if (outer <= 0)
                throw new ArgumentException("Outer radius has to be greater than 0");
            if (inner >= outer)
                throw new ArgumentException("Inner radius has to be less than outer radius");

            CustomMesh mesh = new CustomMesh();

            List<VertexPositionNormal> vertices = new List<VertexPositionNormal>();

            double angle = start;
            double incr = sweep / slices;
            float cos, sin;
            bool hasInner = (inner > 0);
            // Add top & bottom side vertices
            if (!hasInner)
            {
                VertexPositionNormal front = new VertexPositionNormal();
                front.Position = Vector3Helper.Get(0, 0, 0);
                front.Normal = Vector3Helper.Get(0, 1, 0);
                vertices.Add(front);

                if (twoSided)
                {
                    VertexPositionNormal back = new VertexPositionNormal();
                    back.Position = Vector3Helper.Get(0, 0, 0);
                    back.Normal = Vector3Helper.Get(0, -1, 0);
                    vertices.Add(back);
                }
            }

            // Add inner & outer vertices
            for (int i = 0; i <= slices; i++, angle += incr)
            {
                cos = (float)Math.Cos(angle);
                sin = (float)Math.Sin(angle);

                if (hasInner)
                {
                    VertexPositionNormal inside = new VertexPositionNormal();
                    inside.Position = Vector3Helper.Get(cos * inner, 0, sin * inner);
                    inside.Normal = Vector3Helper.Get(0, 1, 0);
                    vertices.Add(inside);

                    if (twoSided)
                        vertices.Add(new VertexPositionNormal(inside.Position, Vector3Helper.Get(0, -1, 0)));
                }

                VertexPositionNormal outside = new VertexPositionNormal();
                outside.Position = Vector3Helper.Get(cos * outer, 0, sin * outer);
                outside.Normal = Vector3Helper.Get(0, 1, 0);
                vertices.Add(outside);

                if (twoSided)
                    vertices.Add(new VertexPositionNormal(outside.Position, Vector3Helper.Get(0, -1, 0)));
            }

            mesh.VertexDeclaration = VertexPositionNormal.VertexDeclaration;

            mesh.VertexBuffer = new VertexBuffer(State.Device, typeof(VertexPositionNormal),
                vertices.Count, BufferUsage.None);
            mesh.VertexBuffer.SetData(vertices.ToArray());

            List<short> indices = new List<short>();

            // Create front side
            if (twoSided)
            {
                if (hasInner)
                {
                    for (int i = 0; i < vertices.Count - 2; i++)
                    {
                        indices.Add((short)(2 * i));
                        indices.Add((short)(2 * (i + 2 - (i+1)%2)));
                        indices.Add((short)(2 * (i + 2 - i%2)));

                        indices.Add((short)(2 * i - 1));
                        indices.Add((short)(2 * (i + 2 - (i + 1) % 2) - 1));
                        indices.Add((short)(2 * (i + 2 - i % 2) - 1));
                    }
                }
                else
                {
                    for (int i = 1; i < vertices.Count - 1; i++)
                    {
                        indices.Add((short)0);
                        indices.Add((short)(2 * i));
                        indices.Add((short)(2 * (i + 1)));

                        indices.Add((short)0);
                        indices.Add((short)(i + 3));
                        indices.Add((short)(i + 1));
                    }
                }
            }
            else
            {
                if (hasInner)
                {
                    for (int i = 0; i < vertices.Count - 2; i++)
                    {
                        indices.Add((short)i);
                        indices.Add((short)(i + 2 - (i+1)%2));
                        indices.Add((short)(i + 2 - i%2));
                    }
                }
                else
                {
                    for (int i = 1; i < vertices.Count - 1; i++)
                    {
                        indices.Add((short)0);
                        indices.Add((short)(i));
                        indices.Add((short)(i + 1));
                    }
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
