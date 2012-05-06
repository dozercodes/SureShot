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
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.Graphics
{
    /// <summary>
    /// A mesh defined using XNA's geometry primitives
    /// </summary>
    public class CustomMesh : IDisposable
    {
        #region Member Fields
        protected VertexBuffer vb;
        protected IndexBuffer ib;
        protected PrimitiveType type;
        protected VertexDeclaration decl;
        protected int numVertices;
        protected int numPrimitives;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a mesh defined using XNA's geometry primitives
        /// </summary>
        public CustomMesh()
        {
            type = PrimitiveType.TriangleList;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the VertexBuffer
        /// </summary>
        public VertexBuffer VertexBuffer
        {
            get { return vb; }
            set { vb = value; }
        }

        /// <summary>
        /// Gets or sets the IndexBuffer 
        /// </summary>
        public IndexBuffer IndexBuffer
        {
            get { return ib; }
            set { ib = value; }
        }

        /// <summary>
        /// Gets or sets primitive type used to render this mesh. 
        /// Default is PrimitiveType.TriangleList
        /// </summary>
        public PrimitiveType PrimitiveType
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Gets or sets the vertex declaration
        /// </summary>
        public VertexDeclaration VertexDeclaration
        {
            get { return decl; }
            set { decl = value; }
        }

        /// <summary>
        /// Gets or sets the number of vertices set in VertexBuffer
        /// </summary>
        public int NumberOfVertices
        {
            get { return numVertices; }
            set { numVertices = value; }
        }

        /// <summary>
        /// Gets or sets the number of primitive shapes defined with PrimitiveType
        /// </summary>
        public int NumberOfPrimitives
        {
            get { return numPrimitives; }
            set { numPrimitives = value; }
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (vb != null)
                vb.Dispose();

            if (ib != null)
                ib.Dispose();
        }

        #endregion
    }
}
