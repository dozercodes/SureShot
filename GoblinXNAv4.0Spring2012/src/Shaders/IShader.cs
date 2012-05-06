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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;
#if !WINDOWS_PHONE
using GoblinXNA.Graphics.ParticleEffects;
#endif
using GoblinXNA.SceneGraph;

namespace GoblinXNA.Shaders
{
    /// <summary>
    /// A delegate function used to provide the actual rendering code.
    /// </summary>
    public delegate void RenderHandler();

    /// <summary>
    /// An interface for a shader class. 
    /// </summary>
    /// <remarks>
    /// A shader class should either extend an existing IShader implementation
    /// or implement this class. However, function body can be empty for any of the 
    /// SetParameter(...) methods if your shader implementation doesn't support
    /// the specific property.
    /// </remarks>
    public interface IShader
    {
        /// <summary>
        /// Gets the maximum number of lights this shader can handle.
        /// </summary>
        int MaxLights { get; }

        /// <summary>
        /// Sets the material properties to be applied for the rendering.
        /// </summary>
        /// <param name="material">The material properties of a 3D object</param>
        void SetParameters(Material material);

        /// <summary>
        /// Sets the lighting effect to be applied for the rendering.
        /// </summary>
        /// <param name="globalLights">A list of global light nodes</param>
        /// <param name="localLights">A list of local light nodes</param>
        void SetParameters(List<LightNode> globalLights, List<LightNode> localLights);

#if !WINDOWS_PHONE
        /// <summary>
        /// Sets the particle effect to be applied for the rendering.
        /// </summary>
        /// <param name="particleEffect">The particle effect properties</param>
        void SetParameters(ParticleEffect particleEffect);
#endif

        /// <summary>
        /// Sets the special camera effect to be applied for the rendering.
        /// </summary>
        /// <remarks>
        /// For example, this method can be used to implement a fish-eye camera effect
        /// </remarks>
        /// <param name="camera">The camera properties</param>
        void SetParameters(CameraNode camera);

        /// <summary>
        /// Sets the environmental effect (e.g., fog) to be applied for the rendering.
        /// </summary>
        /// <param name="environment">The environment properties</param>
        void SetParameters(GoblinXNA.Graphics.Environment environment);

        /// <summary>
        /// Gets the current effect class used for this shader
        /// </summary>
        Effect CurrentEffect { get; }

        /// <summary>
        /// Gets or sets the current material used for this shader
        /// </summary>
        Material CurrentMaterial { get; }

        /// <summary>
        /// Renders a 3D mesh provided in the renderDelegate function with the specified
        /// world transformation and shader technique name.
        /// </summary>
        /// <param name="worldMatrix">The world transformation of the mesh to be rendered</param>
        /// <param name="techniqueName">The name of the shader technique to use</param>
        /// <param name="renderDelegate">A delegate function which contains the mesh preparation</param>
        void Render(ref Matrix worldMatrix, String techniqueName, RenderHandler renderDelegate);

        /// <summary>
        /// This function is called after rendering each IModel.
        /// </summary>
        void RenderEnd();

        /// <summary>
        /// Disposes this shader.
        /// </summary>
        void Dispose();
    }
}
