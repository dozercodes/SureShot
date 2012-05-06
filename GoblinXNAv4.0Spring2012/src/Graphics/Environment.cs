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

using GoblinXNA.Shaders;

namespace GoblinXNA.Graphics
{
    /// <summary>
    /// Defines the properties of environmental effects such as fog. This class can be passed to 
    /// IShader.SetParameters(IEnvironment) for rendering environmental effects.
    /// </summary>
    public class Environment
    {
        #region Member Fields
        protected float fogStartDistance;
        protected float fogEndDistance;
        protected bool fogEnabled;
        protected Vector4 fogColor;
        protected IShader shader;
        protected Matrix tmpMat1;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an environment object.
        /// </summary>
        public Environment()
        {
            fogStartDistance = -1;
            fogEndDistance = -1;
            fogEnabled = false;
            fogColor = Color.White.ToVector4();
            tmpMat1 = Matrix.Identity;
            shader = new SimpleEffectShader();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the start distance of fog. The default value is -1.
        /// </summary>
        public float FogStartDistance
        {
            get { return fogStartDistance; }
            set { fogStartDistance = value; }
        }

        /// <summary>
        /// Gets or sets the ending distance of fog. The default value is -1.
        /// </summary>
        public float FogEndDistance
        {
            get { return fogEndDistance; }
            set { fogEndDistance = value; }
        }

        /// <summary>
        /// Gets or sets whether the fog is enabled
        /// </summary>
        public bool FogEnabled
        {
            get { return fogEnabled; }
            set { fogEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the fog color in this environment. The default is (1, 1, 1, 1) which is opaque white.
        /// </summary>
        public Vector4 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; }
        }

        /// <summary>
        /// Gets or sets the shader to use for rendering this environment
        /// </summary>
        public IShader Shader
        {
            get { return shader; }
            set { shader = value; }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Renders this environment
        /// </summary>
        public void Render()
        {
            shader.SetParameters(this);
            shader.Render(ref tmpMat1, "", null);
        }
        #endregion
    }
}
