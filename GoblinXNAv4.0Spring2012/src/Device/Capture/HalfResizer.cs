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
    /// An implementation of IResizer that resizes the image to half width and half height.
    /// </summary>
    public class HalfResizer : IResizer
    {
        #region Member Fields

        private float scale;

        #endregion

        #region Constructor

        public HalfResizer()
        {
            scale = 0.5f;
        }

        #endregion

        #region Properties

        public float ScalingFactor
        {
            get { return scale; }
        }

        #endregion

        #region Public Methods

#if WINDOWS

        public void ResizeImage(IntPtr origImagePtr, Vector2 origSize, 
            ref IntPtr resizedImagePtr, int bpp)
        {
            int newWidth = (int)(origSize.X * scale);
            int newHeight = (int)(origSize.Y * scale);
            int srcStride = (int)(origSize.X * bpp);
            int dstStride = (int)(newWidth * bpp);
            int incr = 2 * bpp;

            unsafe
            {
                byte* src = (byte*)origImagePtr;
                byte* dest = (byte*)resizedImagePtr;

                for (int i = 0; i < (int)origSize.Y; i += 2)
                {
                    for (int j = 0; j < srcStride; j += incr)
                    {
                        for (int k = 0; k < bpp; k++)
                            *(dest + k) = *(src + k);

                        src += incr;
                        dest += bpp;
                    }

                    src += srcStride;
                }
            }
        }
#else
        public void ResizeImage(byte[] origImage, Vector2 origSize,
            ref byte[] resizedImage, int bpp)
        {
            int newWidth = (int)(origSize.X * scale);
            int newHeight = (int)(origSize.Y * scale);
            int srcStride = (int)(origSize.X * bpp);
            int dstStride = (int)(newWidth * bpp);
            int incr = 2 * bpp;
            int srcIndex = 0;
            int destIndex = 0;

            for (int i = 0; i < (int)origSize.Y; i += 2)
            {
                for (int j = 0; j < srcStride; j += incr)
                {
                    for (int k = 0; k < bpp; k++)
                        resizedImage[destIndex + k] = origImage[srcIndex + k];

                    srcIndex += incr;
                    destIndex += bpp;
                }

                srcIndex += srcStride;
            }
        }
#endif
        #endregion
    }
}
