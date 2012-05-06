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

using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics;

namespace GoblinXNA.Shaders
{
    public interface IShadowMap
    {
        /// <summary>
        /// Gets the maximum number of lights this shader can handle.
        /// </summary>
        int MaxLights { get; }

        /// <summary>
        /// Gets the current effect class used for this shader
        /// </summary>
        Effect CurrentEffect { get; }

        /// <summary>
        /// Gets the shadow render targets
        /// </summary>
        RenderTarget2D[] ShadowRenderTargets { get; }

        /// <summary>
        /// Gets the occluder render targets
        /// </summary>
        RenderTarget2D[] OccluderRenderTargets { get; }

        /// <summary>
        /// Sets the lighting effect to be applied for rendering shadows.
        /// </summary>
        /// <param name="globalLights">A list of global light nodes</param>
        void SetParameters(List<LightNode> globalLights);

        /// <summary>
        /// Prepares the render targets that will be used by IShader to render shadows
        /// </summary>
        /// <param name="backgroundGeometries"></param>
        /// <param name="occluderGeometries"></param>
        void PrepareRenderTargets(List<GeometryNode> occluderGeometries,
            List<GeometryNode> backgroundGeometries);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="renderHandler"></param>
        void ComputeShadow(ref Matrix matrix, RenderHandler renderHandler);

        /// <summary>
        /// Disposes this shader.
        /// </summary>
        void Dispose();
    }
}
