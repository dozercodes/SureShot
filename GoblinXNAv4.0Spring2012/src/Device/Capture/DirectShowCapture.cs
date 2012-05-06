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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Xna.Framework.Graphics;

// Reference for the DirectShow Library for C# originally from
// http://www.codeproject.com/cs/media/directxcapture.asp
// Update of this original library with capability of capture individual frame from
// http://www.codeproject.com/cs/media/DirXVidStrm.asp?df=100&forumid=73014&exp=0&select=1780522
using DirectX.Capture;
using DCapture = DirectX.Capture.Capture;

namespace GoblinXNA.Device.Capture
{
    /// <summary>
    /// A video capture class that uses the DirectShow library. This implementation is slower than
    /// the other DirectShowCapture implementation, but this implementation works for any cameras
    /// under 64-bit OS.
    /// </summary>
    public class DirectShowCapture : IVideoCapture
    {
        #region Member Fields

        /// <summary>
        /// Video capture class for DirectShow
        /// </summary>
        private DCapture capture;

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
        private String selectedVideoDeviceName;

        /// <summary>
        /// Used to count the number of times it failed to capture an image
        /// If it fails more than certain times, it will assume that the video
        /// capture device can not be accessed
        /// </summary>
        private int failureCount;

        private const int FAILURE_THRESHOLD = 1000;

        /// <summary>
        /// A temporary panel for grabbing the video frame from DirectShow interface
        /// </summary>
        private Panel tmpPanel;
        private Bitmap tmpBitmap;

        private ImageReadyCallback imageReadyCallback;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a video capture using the DirectShow library.
        /// </summary>
        public DirectShowCapture()
        {
            cameraInitialized = false;
            videoDeviceID = -1;

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

        /// <summary>
        /// Sets the callback function to be called when a new image becomes ready.
        /// </summary>
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

            Filters filters = null;
            Filter videoDevice, audioDevice = null;
            try
            {
                filters = new Filters();
            }
            catch (Exception exp)
            {
                throw new GoblinException("No video capturing devices are found");
            }

            try
            {
                videoDevice = (videoDeviceID >= 0) ? filters.VideoInputDevices[videoDeviceID] : null;
            }
            catch (Exception exp)
            {
                String suggestion = "Try the following device IDs:";
                for(int i = 0; i < filters.VideoInputDevices.Count; i++)
                {
                    suggestion += " " + i + ":" + filters.VideoInputDevices[i].Name + ", ";
                }
                throw new GoblinException("VideoDeviceID " + videoDeviceID + " is out of the range. "
                    + suggestion);
            }

            selectedVideoDeviceName = filters.VideoInputDevices[videoDeviceID].Name;
            
            capture = new DCapture(videoDevice, audioDevice);

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

            if (videoDevice != null)
            {
                // Using MPEG compressor
                //capture.VideoCompressor = filters.VideoCompressors[2]; 
                capture.FrameRate = frame_rate;
                try
                {
                    capture.FrameSize = new Size(cameraWidth, cameraHeight);
                }
                catch(Exception exp)
                {
                    throw new GoblinException("Resolution._" + cameraWidth + "x" + cameraHeight +
                        " is not supported for " + selectedVideoDeviceName + 
                        ". Maximum resolution supported is " + 
                        capture.VideoCaps.MaxFrameSize);
                }
            }

            if (capture.FrameSize.Width != cameraWidth || capture.FrameSize.Height != cameraHeight)
                throw new GoblinException("Failed to set the resolution to " + cameraWidth + "x" + cameraHeight);

            tmpPanel = new Panel();
            tmpPanel.Size = new Size(cameraWidth, cameraHeight);

            try
            {
                capture.PreviewWindow = tmpPanel;
            }
            catch (Exception exp)
            {
                throw new GoblinException("Specified framerate or/and resolution is/are not supported " +
                    "for " + selectedVideoDeviceName);
            }

            capture.FrameEvent2 += new DCapture.HeFrame(CaptureDone);
            capture.GrapImg();

            cameraInitialized = true;
        }

        public void GetImageTexture(int[] returnImage, ref IntPtr imagePtr)
        {
            Bitmap image = GetBitmapImage();

            if (image != null)
            {
                failureCount = 0;
                BitmapData data = image.LockBits(
                    new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, image.PixelFormat);

                // convert the Bitmap pixel format, that is right to left and
                // bottom to top, to artag pixel format, that is right to left and
                // top to bottom
                if ((imagePtr != IntPtr.Zero) && (returnImage != null))
                    ReadBmpData(data, imagePtr, returnImage, image.Width, image.Height);
                else if (imagePtr != IntPtr.Zero)
                    ReadBmpData(data, imagePtr, image.Width, image.Height);
                else if (returnImage != null)
                    ReadBmpData(data, returnImage, image.Width, image.Height);

                image.UnlockBits(data);

                if(imageReadyCallback != null)
                    imageReadyCallback(imagePtr, returnImage);
            }
            else
            {
                failureCount++;

                if (failureCount > FAILURE_THRESHOLD)
                {
                    throw new GoblinException("Video capture device ID: " + videoDeviceID + ", Name: " + 
                        selectedVideoDeviceName + " is used by " +
                        "other application, and can not be accessed");
                }
            }
        }

        /// <summary>
        /// Displays video capture device property information on a Windows Form object.
        /// </summary>
        /// <param name="frmOwner">The owner to hold this property form</param>
        public void ShowVideoCaptureDeviceProperties(System.Windows.Forms.Form frmOwner)
        {
            capture.PropertyPages[0].Show(frmOwner);
        }

        /// <summary>
        /// Displayes video capture pin information on a Windows Form object.
        /// </summary>
        /// <param name="frmOwner"></param>
        public void ShowVideoCapturePin(System.Windows.Forms.Form frmOwner)
        {
            capture.PropertyPages[1].Show(frmOwner);
        }

        /// <summary>
        /// Starts video recording, and saves the recorded video in a given file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>If previous video capturing has not been stopped, then false is returned. Otherwise, true.</returns>
        public bool StartVideoCapturing(String filename)
        {
            if (!capture.Stopped)
                return false;

            capture.Filename = filename;

            if (!capture.Cued)
                capture.Cue();

            capture.Start();

            return true;
        }

        /// <summary>
        /// Stops the video capturing.
        /// </summary>
        public void StopVideoCapturing()
        {
            if (!capture.Stopped)
                capture.Stop();
        }

        /// <summary>
        /// Gets a Bitmap image object returned by the DirectShow library.
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmapImage()
        {
            if (capture == null)
                return null;

            return tmpBitmap;
        }

        public void Dispose()
        {
            if (capture != null)
                capture.Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// A helper function that extracts the image pixels stored in Bitmap instance to an array
        /// of integers as well as copy them to the memory location pointed by 'cam_image'.
        /// </summary>
        /// <param name="bmpDataSource"></param>
        /// <param name="cam_image"></param>
        /// <param name="imageData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void ReadBmpData(BitmapData bmpDataSource, IntPtr cam_image, int[] imageData, int width,
            int height)
        {
            unsafe
            {
                byte* src = (byte*)bmpDataSource.Scan0;
                byte* dst = (byte*)cam_image;
                int R = 0, G = 0, B = 0, A = 0;
                switch (format)
                {
                    case ImageFormat.GRAYSCALE_8:
                        break;
                    case ImageFormat.R5G6B5_16:
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0, k = 0; j < width * 3; j += 3, k += 2)
                            {
                                *(dst + k) = (byte)((*(src + j) & 0xF8) | (*(src + j + 1) >> 5));
                                *(dst + k + 1) = (byte)(((*(src + j + 1) & 0x1C) << 3) |
                                    ((*(src + j + 2) & 0xF8) >> 3));

                                imageData[i * width + j / 3] = (*(src + j + 2) << 16) |
                                    (*(src + j + 1) << 8) | *(src + j);
                            }

                            src -= (width * 3);
                            dst += (width * 2);
                        }
                        break;
                    case ImageFormat.B8G8R8_24:
                    case ImageFormat.R8G8B8_24:
                        if (format == ImageFormat.B8G8R8_24)
                        {
                            R = 2; G = 1; B = 0;
                        }
                        else
                        {
                            R = 0; G = 1; B = 2;
                        }

                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width * 3; j += 3)
                            {
                                *(dst + j) = *(src + j + R);
                                *(dst + j + 1) = *(src + j + G);
                                *(dst + j + 2) = *(src + j + B);

                                imageData[i * width + j / 3] = (*(src + j) << 16) |
                                    (*(src + j + 1) << 8) | *(src + j + 2);
                            }

                            src -= (width * 3);
                            dst += (width * 3);
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

                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0, k = 0; j < width * 3; j += 3, k += 4)
                            {
                                *(dst + k + R) = *(src + j);
                                *(dst + k + G) = *(src + j + 1);
                                *(dst + k + B) = *(src + j + 2);
                                *(dst + k + A) = (byte)255;

                                imageData[i * width + j / 3] = (*(src + j + 2) << 16) |
                                    (*(src + j + 1) << 8) | *(src + j);
                            }

                            src -= (width * 3);
                            dst += (width * 4);
                        }
                        break;
                }
            }
        }

        private void ReadBmpData(BitmapData bmpDataSource, int[] imageData, int width, int height)
        {
            unsafe
            {
                byte* src = (byte*)bmpDataSource.Scan0;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width * 3; j += 3)
                    {
                        imageData[i * width + j / 3] = (*(src + j) << 16) |
                            (*(src + j + 1) << 8) | *(src + j + 2);
                    }
                    src -= (width * 3);
                }
            }
        }

        private void ReadBmpData(BitmapData bmpDataSource, IntPtr cam_image, int width, int height)
        {
            unsafe
            {
                byte* src = (byte*)bmpDataSource.Scan0;
                byte* dst = (byte*)cam_image;
                int R = 0, G = 0, B = 0, A = 0;
                switch (format)
                {
                    case ImageFormat.R5G6B5_16:
                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0, k = 0; j < width * 3; j += 3, k += 2)
                            {
                                *(dst + k) = (byte)((*(src + j) & 0xF8) | (*(src + j + 1) >> 5));
                                *(dst + k + 1) = (byte)(((*(src + j + 1) & 0x1C) << 3) |
                                    ((*(src + j + 2) & 0xF8) >> 3));
                            }

                            src -= (width * 3);
                            dst += (width * 2);
                        }
                        break;
                    case ImageFormat.B8G8R8_24:
                    case ImageFormat.R8G8B8_24:
                        if (format == ImageFormat.B8G8R8_24)
                        {
                            R = 2; G = 1; B = 0;
                        }
                        else
                        {
                            R = 0; G = 1; B = 2;
                        }

                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width * 3; j += 3)
                            {
                                *(dst + j) = *(src + j + R);
                                *(dst + j + 1) = *(src + j + G);
                                *(dst + j + 2) = *(src + j + B);
                            }

                            src -= (width * 3);
                            dst += (width * 3);
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

                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0, k = 0; j < width * 3; j += 3, k += 4)
                            {
                                *(dst + k + R) = *(src + j);
                                *(dst + k + G) = *(src + j + 1);
                                *(dst + k + B) = *(src + j + 2);
                                *(dst + k + A) = (byte)255;
                            }

                            src -= (width * 3);
                            dst += (width * 4);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Assigns the video image returned by the DirectShow library to a temporary Bitmap holder.
        /// </summary>
        /// <param name="e"></param>
        private void CaptureDone(System.Drawing.Bitmap e)
        {
            this.tmpBitmap = e;
        }

        #endregion
    }
}
