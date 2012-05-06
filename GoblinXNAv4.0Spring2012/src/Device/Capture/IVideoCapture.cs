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
 * Authors: Ohan Oda (ohan@cs.columbia.edu) 
 * 
 *************************************************************************************/


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA;

namespace GoblinXNA.Device.Capture
{
    #region Enums
    /// <summary>
    /// The resolution of the camera. In the format of _[width]x[height].
    /// </summary>
    public enum Resolution{ 
        _160x120, 
        _320x240, 
        _640x480, 
        _800x600, 
        _1024x768, 
        _1280x1024, 
        _1600x1200 
    };

    /// <summary>
    /// The framerate of the camera
    /// </summary>
    public enum FrameRate
    {
        _15Hz,
        _30Hz,
        _50Hz,
        _60Hz,
        _120Hz,
        _240Hz
    };

    /// <summary>
    /// The format of the image that will be passed to the marker tracker.
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// 8-bit Grayscale format.
        /// </summary>
        GRAYSCALE_8,

        /// <summary>
        /// 16-bit RGB format. 5 bits for R, 6 bits for G, and 5 bits for B channel.
        /// </summary>
        R5G6B5_16,

        /// <summary>
        /// 24-bit RGB format. 8 bits for each R, G, and B channel.
        /// </summary>
        R8G8B8_24,

        /// <summary>
        /// 24-bit BGR format. 8 bits for each B, G, and R channel.
        /// </summary>
        B8G8R8_24,

        /// <summary>
        /// 32-bit ABGR format. 8 bits for each A (alpha), B, G, and R channel.
        /// </summary>
        A8B8G8R8_32,

        /// <summary>
        /// 32-bit RGBA format. 8 bits for each R, G, B, and A (alpha) channel.
        /// </summary>
        R8G8B8A8_32,

        /// <summary>
        /// 32-bit BGRA format. 8 bits for each B, G, R, and A (alpha) channel.
        /// </summary>
        B8G8R8A8_32
    };
    #endregion

    #region Delegates

#if WINDOWS
    public delegate bool ImageReadyCallback(IntPtr image, int[] background);
#else
    public delegate bool ImageReadyCallback(byte[] image, int[] background);
#endif

    #endregion

    /// <summary>
    /// A video capture interface for accessing cameras. Any video decoding class should implement this interface.
    /// </summary>
    public interface IVideoCapture
    {
        /// <summary>
        /// Gets the camera width in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the camera height in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the video device ID.
        /// </summary>
        int VideoDeviceID { get; }

        /// <summary>
        /// Gets whether to use grayscale.
        /// </summary>
        bool GrayScale { get; }

        /// <summary>
        /// Gets the image pointer format.
        /// </summary>
        ImageFormat Format { get; }

        /// <summary>
        /// Gets or sets the image resizer for the image pointer passed to the marker tracking. You can pass
        /// a different resolution to the marker tracking process from the resolution of the rendered video image
        /// by setting this resizer. If not set, which is by default, then the same resolution from the rendered image
        /// is used for the marker tracking. 
        /// </summary>
        IResizer MarkerTrackingImageResizer { get; set; }

        /// <summary>
        /// Sets the callback function to be called when a new image becomes ready.
        /// </summary>
        ImageReadyCallback CaptureCallback { set; }

        /// <summary>
        /// Gets the information whether certain image operation needs to be applied to the rendered
        /// video image on the background.
        /// </summary>
        SpriteEffects RenderFormat { get; }

        /// <summary>
        /// Gets whether the device is initialized.
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// Initializes the video capture device with the specific video and audio device ID,
        /// desired frame rate and image resolution, and whether to use grayscaled image rather 
        /// than color image. 
        /// </summary>
        /// <remarks>
        /// If the camera supports either only color or grayscale, then the grayscale parameter 
        /// does not affect the output
        /// </remarks>
        /// <param name="videoDeviceID">The actual video device ID assigned by the OS. It's usually 
        /// determined in the order of time that they were plugged in to the computer. For example, 
        /// the first video capture device plugged into the computer is assigned ID of 0, and the next 
        /// one is assigned ID of 1. If you're using the cameras embedded on a laptop or other mobile PC, 
        /// usually the front camera is assigned ID of 0, and the back camera is assigned ID of 1.</param>
        /// <param name="frameRate">The desired framerate to use</param>
        /// <param name="resolution">The resolution of the live video image to use. Some resolution is
        /// not supported by certain cameras, and an exception will be thrown in that case</param>
        /// <param name="format">The format of how the ImagePtr property, which will be passed to
        /// the marker tracker, will be stored (e.g., ARTag uses R8G8B8_24 format)</param>
        /// <param name="grayscale">Indicates whether to use grayscale mode. If the camera only supports 
        /// black & white, then this must be set to false. Otherwise, an exception will be thrown</param>
        void InitVideoCapture(int videoDeviceID, FrameRate frameRate, Resolution resolution, 
            ImageFormat format, bool grayscale);

#if WINDOWS
        /// <summary>
        /// Gets an array of video image pixels in Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr32 
        /// format. The size is CameraWidth * CameraHeight.
        /// </summary>
        /// <param name="imagePtr">The pointer where to copy the video image so that the
        /// marker tracker library can use it to process the image and detect marker transformations.
        /// Pass IntPtr.Zero if you don't need to get back the pointer.</param>
        /// <param name="returnImage">An array of int in which the video pixels are copied to. Pass null
        /// if you don't need the int[] image.</param>
        /// <returns></returns>
        void GetImageTexture(int[] returnImage, ref IntPtr imagePtr);
#else
        /// <summary>
        /// Gets an array of video image pixels in Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr32 
        /// format. The size is CameraWidth * CameraHeight.
        /// </summary>
        /// <param name="imagePtr">A byte array where to copy the video image so that the
        /// marker tracker library can use it to process the image and detect marker transformations.
        /// Pass null if you don't need to get back the image.</param>
        /// <param name="returnImage">An array of int in which the video pixels are copied to. Pass null
        /// if you don't need the int[] image.</param>
        /// <returns></returns>
        void GetImageTexture(int[] returnImage, byte[] imagePtr);
#endif

        /// <summary>
        /// Disposes the video capture device.
        /// </summary>
        void Dispose();
    }
}
