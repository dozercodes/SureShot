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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.Graphics.Geometry
{
    /// <summary>
    /// Custom vertex format with position and normal information
    /// </summary>
    public struct VertexPositionNormal : IVertexType
    {
        Vector3 pos;
        Vector3 normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            pos = position;
            this.normal = normal;
        }

        public readonly static VertexDeclaration VertexDeclaration =
            new VertexDeclaration (
                new VertexElement(0,VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float)*3,VertexElementFormat.Vector3,VertexElementUsage.Normal,0)               
            );

        public static bool operator !=(VertexPositionNormal left, VertexPositionNormal right)
        {
            return left.GetHashCode() != right.GetHashCode();
        }

        public static bool operator ==(VertexPositionNormal left, VertexPositionNormal right)
        {
            return left.GetHashCode() == right.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this == (VertexPositionNormal)obj;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get{return VertexDeclaration;}
        }

        /// <summary>
        /// Gets or sets the position of this vertex.
        /// </summary>
        public Vector3 Position { get { return pos; } set { pos = value; } }

        /// <summary>
        /// Gets or sets the normal of this vertex.
        /// </summary>
        public Vector3 Normal { get { return normal; } set { normal = value; } }

        /// <summary>
        /// Gets the size of this vertex structure in bytes.
        /// </summary>
        public static int SizeInBytes { get { return sizeof(float) * 6; } }

    }
}
