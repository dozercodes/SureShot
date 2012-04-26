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
using Microsoft.Xna.Framework.Input;

using GoblinXNA.Device.Generic;

namespace GoblinXNA.UI
{
    /// <summary>
    /// A GUI mapper class that maps UI texture and events from an external GUI library 
    /// (e.g., Windows Forms or Java Swing)
    /// </summary>
    internal interface IGUIMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        void KeyEventHandler(Keys key, KeyModifier modifier);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mouseLocation"></param>
        /// <param name="button"></param>
        void MouseEventHandler(Point mouseLocation, int button);

        /// <summary>
        /// Gets an array of uint data of this 2D GUI
        /// </summary>
        uint[] GUITexture { get; }

        /// <summary>
        /// Gets the width of this GUI in pixels
        /// </summary>
        int GUIWidth { get; }

        /// <summary>
        /// Gets the height of this GUI in pixels
        /// </summary>
        int GUIHeight { get; }

        String GUIName { get; }

        /// <summary>
        /// Gets the texture format of this GUI
        /// </summary>
        /// <remarks>The texture format has to match whatever the texture data GUITexture
        /// method returns</remarks>
        /// <seealso cref="GUITexture"/>
        SurfaceFormat TextureFormat { get; }

        /// <summary>
        /// Gets the drawing scale factor (e.g., if you want it to be draw 2 times smaller than the
        /// original GUI texture, pass new Vector2(0.5f, 0.5f))
        /// </summary>
        /// <returns></returns>
        Vector2 DrawingScaleFactor { get; }

        //GUI2Dto3DMapper.MouseInput GetMappedMouseInput();
    }
}
