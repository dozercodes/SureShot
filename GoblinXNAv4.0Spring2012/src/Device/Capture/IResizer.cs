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

namespace GoblinXNA.Device.Capture
{
    /// <summary>
    /// An interface for resizing an image.
    /// </summary>
    public interface IResizer
    {
        /// <summary>
        /// Gets the scaling factor applied to the original image.
        /// </summary>
        float ScalingFactor { get; }

#if WINDOWS
        /// <summary>
        /// Resizes the original image and store the resized image in the given memory address pointed by
        /// resizedImagePtr.
        /// </summary>
        /// <param name="origImagePtr">The pointer that contains the address of the original image.</param>
        /// <param name="origSize">The size of the original image.</param>
        /// <param name="resizedImagePtr">The pointer that contains the address of the resized image.</param>
        /// <param name="bpp">Bits per pixel</param>
        void ResizeImage(IntPtr origImagePtr, Vector2 origSize, ref IntPtr resizedImagePtr, int bpp);
#else
        /// <summary>
        /// Resizes the original image and store the resized image in the given memory address pointed by
        /// resizedImagePtr.
        /// </summary>
        /// <param name="origImage">The pixel byte array of the original image.</param>
        /// <param name="origSize">The size of the original image.</param>
        /// <param name="resizedImage">The pixel byte array of the resized image.</param>
        /// <param name="bpp">Bits per pixel</param>
        void ResizeImage(byte[] origImage, Vector2 origSize, ref byte[] resizedImage, int bpp);
#endif
    }
}
