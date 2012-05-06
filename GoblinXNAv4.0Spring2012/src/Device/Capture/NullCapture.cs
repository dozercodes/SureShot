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
using System.Linq;
using System.Text;
using System.IO;

using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.Device.Capture
{
    /// <summary>
    /// Creates a dummy capture device that streams a static image.
    /// </summary>
    public class NullCapture : IVideoCapture
    {
        #region Member Fields

        private int videoDeviceID;

        private string imageFilename;

        private int cameraWidth;
        private int cameraHeight;
        private bool grayscale;
        private bool cameraInitialized;

        private ImageFormat format;
        private IResizer resizer;

        private ImageReadyCallback imageReadyCallback;

        private Texture2D imageTexture;
        private int[] intImage;
#if !WINDOWS
        private byte[] byteImage;
#endif

        private bool isImageAlreadyProcessed;
        private bool processing;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a video capture using a series of static images.
        /// </summary>
        public NullCapture()
        {
            cameraInitialized = false;

            videoDeviceID = -1;

            imageFilename = "";

            cameraWidth = 0;
            cameraHeight = 0;
            grayscale = false;
            isImageAlreadyProcessed = false;
        }

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

        public bool Initialized
        {
            get { return cameraInitialized; }
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

        /// <summary>
        /// Gets or sets whether the static image is already processed by the marker tracker.
        /// </summary>
        public bool IsImageAlreadyProcessed
        {
            get { return isImageAlreadyProcessed; }
            set { isImageAlreadyProcessed = value; }
        }

        /// <summary>
        /// Gets or sets the static image used for tracking. JPEG, GIF, and BMP formats are
        /// supported.
        /// </summary>
        /// <remarks>
        /// You need to set this value if you want to perform marker tracking using a
        /// static image instead of a live video stream. 
        /// </remarks>
        /// <exception cref="GoblinException">If the image format is not supported.</exception>
        public String StaticImageFile
        {
            get { return imageFilename; }
            set
            {
                if (!value.Equals(imageFilename))
                {
                    imageFilename = value;

                    if (Path.GetExtension(imageFilename).Length > 0)
#if WINDOWS
                        imageTexture = Texture2D.FromStream(State.Device, new FileStream(imageFilename, FileMode.Open, FileAccess.Read));
#else
                        imageTexture = Texture2D.FromStream(State.Device, TitleContainer.OpenStream(imageFilename));
#endif
                    else
                        imageTexture = State.Content.Load<Texture2D>(imageFilename);

                    while (processing) { }

                    cameraWidth = imageTexture.Width;
                    cameraHeight = imageTexture.Height;

                    intImage = new int[cameraWidth * cameraHeight];

                    imageTexture.GetData<int>(intImage);
#if !WINDOWS
                    byteImage = new byte[cameraWidth * cameraHeight * 4];
                    CopyIntArrayToByteArray();
#endif

                    isImageAlreadyProcessed = false;
                }
            }
        }

        /// <summary>
        /// Sets the image data for tracking directly.
        /// </summary>
        public int[] ImageData
        {
            set
            {
                while (processing) { }

                if (intImage == null || intImage.Length != value.Length)
                    intImage = new int[value.Length];

                Buffer.BlockCopy(value, 0, intImage, 0, value.Length * 4);

                if (intImage.Length == 320 * 240)
                {
                    cameraWidth = 320;
                    cameraHeight = 240;
                }
                else if (intImage.Length == 640 * 480)
                {
                    cameraWidth = 640;
                    cameraHeight = 480;
                }
                else if (intImage.Length == 800 * 600)
                {
                    cameraWidth = 800;
                    cameraHeight = 600;
                }
                else if (intImage.Length == 1024 * 768)
                {
                    cameraWidth = 1024;
                    cameraHeight = 768;
                }
                else if (intImage.Length == 160 * 120)
                {
                    cameraWidth = 160;
                    cameraHeight = 120;
                }
                else
                    throw new GoblinException("Unsupported image dimension for size: " + intImage.Length);
            }
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

            cameraInitialized = true;
        }

#if WINDOWS
        public void GetImageTexture(int[] returnImage, ref IntPtr imagePtr)
        {
            if (intImage != null)
            {
                processing = true;

                if(returnImage != null)
                    Buffer.BlockCopy(intImage, 0, returnImage, 0, intImage.Length * 4);

                if (imagePtr != IntPtr.Zero)
                    CopyIntArrayToIntPtr(intImage, ref imagePtr);

                if (imageReadyCallback != null)
                    imageReadyCallback(imagePtr, returnImage);

                processing = false;
            }
        }
#else
        public void GetImageTexture(int[] returnImage, byte[] imagePtr)
        {
            if (imageFilename.Length > 0)
            {
                if(returnImage != null)
                    Buffer.BlockCopy(intImage, 0, returnImage, 0, intImage.Length * 4);
                if(imagePtr != null)
                    Buffer.BlockCopy(byteImage, 0, imagePtr, 0, byteImage.Length);

                if (imageReadyCallback != null)
                    imageReadyCallback(imagePtr, returnImage);
            }
        }
#endif

        public void Dispose()
        {
            if(imageTexture != null)
                imageTexture.Dispose();
        }

        #endregion

        #region Private Methods
#if WINDOWS
        private void CopyIntArrayToIntPtr(int[] imageData, ref IntPtr imagePtr)
        {
            byte R, G, B, A;
            byte r, g, b;
            int color;

            unsafe
            {
                byte* dst = (byte*)imagePtr;

                switch (format)
                {
                    case ImageFormat.GRAYSCALE_8:
                        for (int i = 0; i < cameraHeight; i++)
                        {
                            for (int j = 0; j < cameraWidth; j++)
                            {
                                color = imageData[i * cameraWidth + j];
                                *(dst + j) = (byte)(0.3 * ((byte)(color >> 16)) + 0.59 * ((byte)(color >> 8)) +
                                        +0.11 * ((byte)color));
                            }

                            dst += cameraWidth;
                        }
                        break;
                    case ImageFormat.R5G6B5_16:
                        for (int i = 0; i < cameraHeight; i++)
                        {
                            for (int j = 0; j < cameraWidth * 2; j += 2)
                            {
                                color = imageData[i * cameraWidth + j / 2];
                                r = (byte)(color >> 16);
                                g = (byte)(color >> 8);
                                b = (byte)(color);
                                *(dst + j) = (byte)((r & 0xF8) | (g >> 5));
                                *(dst + j + 1) = (byte)(((g & 0x1C) << 3) |  ((b & 0xF8) >> 3));
                            }

                            dst += cameraWidth * 2;
                        }
                        break;
                    case ImageFormat.B8G8R8_24:
                    case ImageFormat.R8G8B8_24:
                        if (format == ImageFormat.R8G8B8_24)
                        {
                            R = 0; G = 1; B = 1;
                        }
                        else
                        {
                            R = 2; G = 1; B = 0;
                        }

                        for (int i = 0; i < cameraHeight; i++)
                        {
                            for (int j = 0; j < cameraWidth * 3; j += 3)
                            {
                                color = imageData[i * cameraWidth + j / 3];
                                *(dst + j + R) = (byte)(color >> 16);
                                *(dst + j + G) = (byte)(color >> 8);
                                *(dst + j + B) = (byte)(color);
                            }

                            dst += cameraWidth * 3;
                        }
                        break;
                    case ImageFormat.A8B8G8R8_32:
                    case ImageFormat.B8G8R8A8_32:
                    case ImageFormat.R8G8B8A8_32:
                        if (format == ImageFormat.A8B8G8R8_32)
                        {
                            A = 0; B = 1; G = 2; R = 3;
                        }
                        else if (format == ImageFormat.B8G8R8A8_32)
                        {
                            B = 0; G = 1; R = 2; A = 3;
                        }
                        else
                        {
                            R = 0; G = 1; B = 2; A = 3;
                        }

                        for (int i = 0; i < cameraHeight; i++)
                        {
                            for (int j = 0; j < cameraWidth * 4; j += 4)
                            {
                                color = imageData[i * cameraWidth + j / 4];
                                //*(dst + j + A) = (byte)255;
                                *(dst + j + R) = (byte)(color >> 16);
                                *(dst + j + G) = (byte)(color >> 8);
                                *(dst + j + B) = (byte)(color);
                            }

                            dst += cameraWidth * 4;
                        }
                        break;
                }
            }
        }
#else
        private void CopyIntArrayToByteArray()
        {
            byte R, G, B, A;
            int color;

            switch (format)
            {
                case ImageFormat.GRAYSCALE_8:
                    break;
                case ImageFormat.R5G6B5_16:
                    break;
                case ImageFormat.B8G8R8_24:
                case ImageFormat.R8G8B8_24:
                    break;
                case ImageFormat.A8B8G8R8_32:
                case ImageFormat.B8G8R8A8_32:
                case ImageFormat.R8G8B8A8_32:
                    if (format == ImageFormat.A8B8G8R8_32)
                    {
                        A = 0; B = 1; G = 2; R = 3;
                    }
                    else if (format == ImageFormat.B8G8R8A8_32)
                    {
                        B = 0; G = 1; R = 2; A = 3;
                    }
                    else
                    {
                        R = 0; G = 1; B = 2; A = 3;
                    }

                    for (int i = 0, j = 0; i < intImage.Length; ++i, j += 4)
                    {
                        color = intImage[i];
                        byteImage[j + R] = (byte)(color >> 16);
                        byteImage[j + G] = (byte)(color >> 8);
                        byteImage[j + B] = (byte)(color);
                    }
                    break;
            }
        }
#endif
        #endregion
    }
}
