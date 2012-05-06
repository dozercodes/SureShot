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
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;
using GoblinXNA.Device.Capture.PointGrey;

namespace GoblinXNA.Device.Capture
{
    /// <summary>
    /// A video capture class that uses the Point Grey FlyCapture library. This is for Point
    /// Grey cameras.
    /// </summary>
    public class PointGreyCapture_1_7 : IVideoCapture
    {
        #region Member Fields

        /// <summary>
        /// Video capture class for Point Grey API
        /// </summary>
        private PGRFlyCapture flyCapture;
        private PGRFlyModule.FlyCaptureVideoMode flyVideoMode;

        private int videoDeviceID;

        private int cameraWidth;
        private int cameraHeight;
        private bool grayscale;
        private bool cameraInitialized;
        private Resolution resolution;
        private FrameRate frameRate;
        private ImageFormat format;
        private IResizer resizer;

        private ImageReadyCallback imageReadyCallback;

        /// <summary>
        /// Used to count the number of times it failed to capture an image
        /// If it fails more than certain times, it will assume that the video
        /// capture device can not be accessed
        /// </summary>
        private int failureCount;

        private const int FAILURE_THRESHOLD = 100;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a video capture using the Point Grey FlyCapture library.
        /// </summary>
        public PointGreyCapture_1_7()
        {
            cameraInitialized = false;
            videoDeviceID = -1;
            flyVideoMode = PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_ANY;

            cameraWidth = 0;
            cameraHeight = 0;
            grayscale = false;

            failureCount = 0;
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
        /// Gets the camera model of a Point Grey camera.
        /// </summary>
        public PGRFlyModule.FlyCaptureCameraModel PGRCameraModel
        {
            get { return flyCapture.CameraModel; }
        }

        /// <summary>
        /// Gets the camera type (e.g., b&w, color) of a Point Grey camera
        /// </summary>
        public PGRFlyModule.FlyCaptureCameraType PGRCameraType
        {
            get { return flyCapture.CameraType; }
        }

        /// <summary>
        /// Sets the video mode of a Point Grey camera
        /// </summary>
        public PGRFlyModule.FlyCaptureVideoMode PGRVideoMode
        {
            set { flyVideoMode = value; }
        }

        #endregion

        #region Public Methods

        public void InitVideoCapture(int videoDeviceID, FrameRate framerate, Resolution resolution,
            ImageFormat format, bool grayscale)
        {
            if (cameraInitialized)
                return;

            this.resolution = resolution;
            this.grayscale = grayscale;
            this.frameRate = framerate;
            this.videoDeviceID = videoDeviceID;
            this.format = format;

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
                    cameraHeight = 960;
                    break;
                case Resolution._1600x1200:
                    cameraWidth = 1600;
                    cameraHeight = 1200;
                    break;
            }

            flyCapture = new PGRFlyCapture();

            PGRFlyModule.FlyCaptureFrameRate flyFrameRate =
                PGRFlyModule.FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_ANY;

            switch (frameRate)
            {
                case FrameRate._15Hz:
                    flyFrameRate = PGRFlyModule.FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_15;
                    break;
                case FrameRate._30Hz:
                    flyFrameRate = PGRFlyModule.FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_30;
                    break;
                case FrameRate._50Hz:
                    flyFrameRate = PGRFlyModule.FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_50;
                    break;
                case FrameRate._60Hz:
                    flyFrameRate = PGRFlyModule.FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_60;
                    break;
                case FrameRate._120Hz:
                    flyFrameRate = PGRFlyModule.FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_120;
                    break;
                case FrameRate._240Hz:
                    flyFrameRate = PGRFlyModule.FlyCaptureFrameRate.FLYCAPTURE_FRAMERATE_240;
                    break;
            }

            if (flyVideoMode.Equals(PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_ANY))
            {
                switch (resolution)
                {
                    case Resolution._160x120:
                        flyVideoMode = PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_160x120YUV444;
                        break;
                    case Resolution._320x240:
                        flyVideoMode = PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_320x240YUV422;
                        break;
                    case Resolution._640x480:
                        flyVideoMode = PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_640x480Y8;
                        break;
                    case Resolution._800x600:
                        flyVideoMode = PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_800x600Y8;
                        break;
                    case Resolution._1024x768:
                        flyVideoMode = PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_1024x768Y8;
                        break;
                    case Resolution._1280x1024:
                        flyVideoMode = PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_1280x960Y8;
                        break;
                    case Resolution._1600x1200:
                        flyVideoMode = PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_1600x1200Y8;
                        break;
                }
            }

            flyCapture.Initialize(videoDeviceID, flyFrameRate, flyVideoMode, grayscale);

            cameraInitialized = true;
        }

        public void GetImageTexture(int[] returnImage, ref IntPtr imagePtr)
        {
            PGRFlyModule.FlyCaptureImage flyImage = GetPGRFlyImage(imagePtr);

            int B = 0, G = 1, R = 2;
            if (PGRCameraModel == PGRFlyModule.FlyCaptureCameraModel.FLYCAPTURE_DRAGONFLY2)
            {
                B = 2;
                R = 0;
            }

            unsafe
            {
                if (flyImage.pData != null)
                {
                    failureCount = 0;
                    if (imagePtr != IntPtr.Zero)
                    {
                        switch (format)
                        {
                            case ImageFormat.R5G6B5_16:
                                throw new GoblinException(format.ToString() + " is not supported");
                                break;
                            case ImageFormat.B8G8R8_24:
                            case ImageFormat.R8G8B8_24:
                                imagePtr = (IntPtr)flyImage.pData;
                                break;
                            case ImageFormat.A8B8G8R8_32:
                            case ImageFormat.B8G8R8A8_32:
                            case ImageFormat.R8G8B8A8_32:
                                throw new GoblinException(format.ToString() + " is not supported");
                                break;
                        }
                    }

                    bool replaceBackground = false;
                    if (imageReadyCallback != null)
                        replaceBackground = imageReadyCallback(imagePtr, returnImage);

                    if (!replaceBackground && (returnImage != null))
                    {
                        int index = 0;
                        for (int i = 0; i < flyImage.iRows; i++)
                        {
                            for (int j = 0; j < flyImage.iRowInc; j += 3)
                            {
                                returnImage[i * flyImage.iCols + j / 3] =
                                    (*(flyImage.pData + index + j + R) << 16) |
                                    (*(flyImage.pData + index + j + G) << 8) |
                                    *(flyImage.pData + index + j + B);
                            }
                            index += flyImage.iRowInc;
                        }
                    }
                }
                else
                {
                    Log.Write("Failed to capture image", Log.LogLevel.Log);
                    failureCount++;

                    if (failureCount > FAILURE_THRESHOLD)
                    {
                        throw new GoblinException("Video capture device id:" + videoDeviceID +
                            " is used by other application, and can not be accessed");
                    }
                }
            }
        }

        /// <summary>
        /// Gets a FlyCaptureImage object returned by the PGRFly library.
        /// </summary>
        /// <param name="camImage"></param>
        /// <returns></returns>
        public PGRFlyModule.FlyCaptureImage GetPGRFlyImage(IntPtr camImage)
        {
            return flyCapture.GrabRGBImage(camImage);
        }

        public void Dispose()
        {
            if (flyCapture != null)
                flyCapture.Dispose();
        }

        #endregion
    }
}
