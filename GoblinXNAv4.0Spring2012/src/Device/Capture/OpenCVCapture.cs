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
using System.Runtime.InteropServices;

using GoblinXNA.Device.Vision;
using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.Device.Capture
{
    public class OpenCVCapture : IVideoCapture
    {
        #region Member Fields

        private IntPtr capture;

        private int videoDeviceID;

        private int cameraWidth;
        private int cameraHeight;
        private int imageSize;
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
        /// Creates a video capture using the DirectShow library.
        /// </summary>
        public OpenCVCapture()
        {
            cameraInitialized = false;

            cameraWidth = 0;
            cameraHeight = 0;
            grayscale = false;

            failureCount = 0;

            imageReadyCallback = null;
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
                    cameraHeight = 1024;
                    break;
                case Resolution._1600x1200:
                    cameraWidth = 1600;
                    cameraHeight = 1200;
                    break;
            }

            capture = OpenCVWrapper.cvCaptureFromCAM(videoDeviceID);

            if (capture == IntPtr.Zero)
                throw new GoblinException("VideoDeviceID " + videoDeviceID + " is out of the range.");

            OpenCVWrapper.cvSetCaptureProperty(capture, OpenCVWrapper.CV_CAP_PROP_FRAME_WIDTH, cameraWidth);
            OpenCVWrapper.cvSetCaptureProperty(capture, OpenCVWrapper.CV_CAP_PROP_FRAME_HEIGHT, cameraHeight);

            double frame_rate = 0;
            switch (frameRate)
            {
                case FrameRate._15Hz: frame_rate = 15; break;
                case FrameRate._30Hz: frame_rate = 30; break;
                case FrameRate._50Hz: frame_rate = 50; break;
                case FrameRate._60Hz: frame_rate = 60; break;
                case FrameRate._120Hz: frame_rate = 120; break;
                case FrameRate._240Hz: frame_rate = 240; break;
            }

            OpenCVWrapper.cvSetCaptureProperty(capture, OpenCVWrapper.CV_CAP_PROP_FPS, frame_rate);

            // Grab the video image to see if resolution is correct
            if (OpenCVWrapper.cvGrabFrame(capture) != 0)
            {
                IntPtr ptr = OpenCVWrapper.cvRetrieveFrame(capture);

                OpenCVWrapper.IplImage videoImage = (OpenCVWrapper.IplImage)Marshal.PtrToStructure(ptr,
                    typeof(OpenCVWrapper.IplImage));

                if (videoImage.width != cameraWidth || videoImage.height != cameraHeight)
                    throw new GoblinException("Resolution " + cameraWidth + "x" + cameraHeight +
                        " is not supported");
            }

            cameraInitialized = true;
        }

        public void GetImageTexture(int[] returnImage, ref IntPtr imagePtr)
        {
            if (OpenCVWrapper.cvGrabFrame(capture) != 0)
            {
                failureCount = 0;

                IntPtr ptr = OpenCVWrapper.cvRetrieveFrame(capture);

                OpenCVWrapper.IplImage videoImage = (OpenCVWrapper.IplImage)Marshal.PtrToStructure(ptr,
                    typeof(OpenCVWrapper.IplImage));

                bool replaceBackground = false;
                if (imageReadyCallback != null)
                    replaceBackground = imageReadyCallback(ptr, returnImage);

                if (!replaceBackground && (returnImage != null))
                {
                    unsafe
                    {
                        byte* src = (byte*)videoImage.imageData;
                        int index = 0;
                        for (int i = 0; i < videoImage.height; i++)
                        {
                            for (int j = 0; j < videoImage.width * videoImage.nChannels; j += videoImage.nChannels)
                            {
                                returnImage[index++] = (int)((*(src) << 16) | (*(src + 1) << 8) | *(src + 2));
                                src += videoImage.nChannels;
                            }
                        }
                    }
                }

                if (imagePtr != IntPtr.Zero)
                    imagePtr = videoImage.imageData;
            }
            else
            {
                failureCount++;

                if (failureCount > FAILURE_THRESHOLD)
                {
                    throw new GoblinException("Video capture device ID: is used by " +
                        "other application, and can not be accessed");
                }
            }
        }

        public void Dispose()
        {
            if (capture != IntPtr.Zero)
                OpenCVWrapper.cvReleaseCapture(ref capture);
        }

        #endregion
    }
}
