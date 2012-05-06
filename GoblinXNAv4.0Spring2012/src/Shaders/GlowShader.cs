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
using GoblinXNA.SceneGraph;
using GoblinXNA.Helpers;

namespace GoblinXNA.Shaders
{
    /// <summary>
    /// A shader that implements a simple glow effect around the given geometry.
    /// </summary>
    public class GlowShader : Shader
    {
        #region Members

        private EffectParameter 
            worldInverseTranspose,
            inflate,
            glowColor,
            glowExponential;

        private Vector3 color;
        private float flate;
        private float expo;
        
        #endregion

        #region Constructor

        public GlowShader()
            : base("Glow")
        {
            color = glowColor.GetValueVector3();
            flate = inflate.GetValueSingle();
            expo = glowExponential.GetValueSingle();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the color of the glow effect. Default color is yellow.
        /// </summary>
        public Vector3 GlowColor
        {
            get { return color; }
            set
            {
                color = value;
                glowColor.SetValue(color);
            }
        }

        /// <summary>
        /// Gets or sets the exponential of the glow. Default value is 1.3.
        /// </summary>
        public float GlowExponential
        {
            get { return expo; }
            set
            {
                expo = value;
                glowExponential.SetValue(expo);
            }
        }

        /// <summary>
        /// Gets or sets the size of the glow. Default value is 0.1.
        /// </summary>
        public float Inflate
        {
            get { return flate; }
            set
            {
                flate = value;
                inflate.SetValue(flate);
            }
        }

        #endregion

        #region Overriden Properties

        public override int MaxLights
        {
            get { return 1; }
        }

        #endregion

        #region Overriden Methods

        protected override void GetParameters()
        {
            world = effect.Parameters["World"];
            worldInverseTranspose = effect.Parameters["WorldInverseTranspose"];
            worldViewProj = effect.Parameters["WorldViewProjection"];
            viewInverse = effect.Parameters["ViewInverse"];

            inflate = effect.Parameters["Inflate"];
            glowColor = effect.Parameters["GlowColor"];
            glowExponential = effect.Parameters["GlowExponential"];
        }

        public override void Render(ref Matrix worldMatrix, string techniqueName, 
            RenderHandler renderDelegate)
        {
            worldInverseTranspose.SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));
            worldViewProj.SetValue(worldMatrix * State.ViewProjectionMatrix);
            viewInverse.SetValue(Matrix.Invert(State.ViewMatrix));

            base.Render(ref worldMatrix, "Glow", renderDelegate);
        }

        #endregion
    }
}
