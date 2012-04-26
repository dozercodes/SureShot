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
using System.Linq;
using System.Text;

using GoblinXNA.UI;

using Microsoft.Devices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows;
using System.Windows.Media;

namespace GoblinXNA.Device.Capture
{
    public class PhoneCameraCapture : IVideoCapture
    {
        #region Member Fields

        private int videoDeviceID;

        private PhotoCamera camera;
        private VideoBrush videoBrush;

        private int cameraWidth;
        private int cameraHeight;
        private bool grayscale;
        private bool cameraInitialized;
        private bool cameraReady;

        private ImageFormat format;
        private IResizer resizer;

        private ImageReadyCallback imageReadyCallback;

        private byte[] luminance;
        private int[] color;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a video capture using Mango's phone camera.
        /// </summary>
        public PhoneCameraCapture(VideoBrush videoBrush)
        {
            this.videoBrush = videoBrush;
            cameraInitialized = false;
            cameraReady = false;

            videoDeviceID = -1;

            cameraWidth = 0;
            cameraHeight = 0;
            grayscale = false;
        }

        public PhoneCameraCapture() : this(null) { }

        #endregion

        #region Properties

        public int Width
        {
            get { return cameraWidth; }
        }

        public int Height
        {
            get { return cameraHeight; }
        }

        public int VideoDeviceID
        {
            get { return videoDeviceID; }
        }

        public bool GrayScale
        {
            get { return grayscale; }
        }

        public bool UseLuminance
        {
            get;
            set;
        }

        public bool Initialized
        {
            get { return cameraInitialized; }
        }

        public bool Ready
        {
            get { return cameraReady; }
        }

        public ImageFormat Format
        {
            get { return format; }
        }

        public IResizer MarkerTrackingImageResizer
        {
            get { return resizer; }
            set { resizer = value; }
        }

        public SpriteEffects RenderFormat
        {
            get { return SpriteEffects.None; }
        }

        public ImageReadyCallback CaptureCallback
        {
            set { imageReadyCallback = value; }
        }

        #endregion

        #region Public Methods

        public void InitVideoCapture(int videoDeviceID, FrameRate framerate, Resolution resolution, 
            ImageFormat format, bool grayscale)
        {
            if (cameraInitialized)
                return;

            this.videoDeviceID = videoDeviceID;
            this.format = format;
            this.grayscale = grayscale;

            switch (resolution)
            {
                case Resolution._160x120:
                    cameraWidth = 160;
                    cameraHeight = 120;
                    break;
                case Resolution._320x240:
                    cameraWidth = 320;
                    cameraHeight = 240;
                    break;
                case Resolution._640x480:
                    cameraWidth = 640;
                    cameraHeight = 480;
                    break;
                case Resolution._800x600:
                    cameraWidth = 800;
                    cameraHeight = 600;
                    break;
                case Resolution._1024x768:
                    cameraWidth = 1024;
                    cameraHeight = 768;
                    break;
                case Resolution._1280x1024:
                    cameraWidth = 1280;
                    cameraHeight = 1024;
                    break;
                case Resolution._1600x1200:
                    cameraWidth = 1600;
                    cameraHeight = 1200;
                    break;
            }

            camera = new PhotoCamera();

            camera.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(CameraInitialized);

            if(videoBrush == null)
                videoBrush = new VideoBrush();
            videoBrush.SetSource(camera);

            cameraInitialized = true;
        }

        public void GetImageTexture(int[] returnImage, byte[] imagePtr)
        {
            if (cameraInitialized && cameraReady)
            {
                if (returnImage != null)
                {
                    if (UseLuminance)
                    {
                        camera.GetPreviewBufferY(luminance);

                        for (int i = 0; i < returnImage.Length; ++i)
                        {
                            returnImage[i] = (int)(luminance[i] << 16 | luminance[i] << 8 | luminance[i]);
                        }
                    }
                    else
                    {
                        camera.GetPreviewBufferArgb32(returnImage);

                        byte r, g, b;
                        for (int i = 0; i < returnImage.Length; ++i)
                        {
                            r = (byte)(returnImage[i]);
                            g = (byte)(returnImage[i] >> 8);
                            b = (byte)(returnImage[i] >> 16);
                            returnImage[i] = (int)(r << 16 | g << 8 | b);
                        }
                    }
                }

                if (imagePtr != null)
                {
                    switch (format)
                    {
                        case ImageFormat.B8G8R8A8_32:
                            if (returnImage != null)
                                Buffer.BlockCopy(returnImage, 0, imagePtr, 0, imagePtr.Length);
                            else
                            {
                                try
                                {
                                    if (UseLuminance)
                                    {
                                        camera.GetPreviewBufferY(luminance);
                                        
                                        // Here, we assume the resizer is HalfResizer and instead of using
                                        // the HalfResizer to resize the pixel data, it's much faster to
                                        // do it here directly
                                        if (resizer != null)
                                        {
                                            int srcIndex = 0;
                                            int destIndex = 0;

                                            for (int j = 0; j < cameraHeight; j += 2)
                                            {
                                                for (int i = 0; i < cameraWidth; i += 2, destIndex += 4)
                                                {
                                                    imagePtr[destIndex] = imagePtr[destIndex + 1] =
                                                        imagePtr[destIndex + 2] = luminance[srcIndex];
                                                    srcIndex += 2;
                                                }

                                                srcIndex += cameraWidth;
                                            }
                                        }
                                        else
                                        {
                                            for (int i = 0, j = 0; i < imagePtr.Length; i += 4, ++j)
                                                imagePtr[i] = imagePtr[i + 1] = imagePtr[i + 2] = luminance[j];
                                        }
                                    }
                                    else
                                    {
                                        if (color == null)
                                            color = new int[imagePtr.Length / 4];

                                        camera.GetPreviewBufferArgb32(color);

                                        for (int i = 0; i < imagePtr.Length; i += 4)
                                        {
                                            imagePtr[i] = (byte)(color[i]);
                                            imagePtr[i + 1] = (byte)(color[i] >> 8);
                                            imagePtr[i + 2] = (byte)(color[i] >> 16);
                                        }
                                    }
                                }
                                catch (Exception exp) { }
                            }
                            break;
                        default:
                            throw new GoblinException("Currently " + format + " is not supported");
                    }
                }
            }
        }

        public void Dispose()
        {
            camera.Dispose();
        }

        public void Focus()
        {
            camera.Focus();
        }
        #endregion

        #region Private Methods

        private void CameraInitialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            cameraReady = e.Succeeded;
            if (cameraReady)
            {
                if (camera.PreviewResolution.Width != cameraWidth || camera.PreviewResolution.Height != cameraHeight)
                {
                    throw new GoblinException(cameraWidth + "x" + cameraHeight + " is not supported. The supported resolutions is: " +
                        camera.PreviewResolution.Width + "x" + camera.PreviewResolution.Height);
                }

                luminance = new byte[cameraWidth * cameraHeight];
            }
            else
            {
                Notifier.AddMessage(e.Exception.Message);
            }
        }

        #endregion
    }
}
