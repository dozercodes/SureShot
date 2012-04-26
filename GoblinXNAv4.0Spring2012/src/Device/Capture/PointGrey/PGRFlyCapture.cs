//=============================================================================
// Copyright © 2006 Point Grey Research, Inc. All Rights Reserved.
// 
// This software is the confidential and proprietary information of Point
// Grey Research, Inc. ("Confidential Information").  You shall not
// disclose such Confidential Information and shall use it only in
// accordance with the terms of the license agreement you entered into
// with Point Grey Research, Inc. (PGR).
// 
// PGR MAKES NO REPRESENTATIONS OR WARRANTIES ABOUT THE SUITABILITY OF THE
// SOFTWARE, EITHER EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, OR NON-INFRINGEMENT. PGR SHALL NOT BE LIABLE FOR ANY DAMAGES
// SUFFERED BY LICENSEE AS A RESULT OF USING, MODIFYING OR DISTRIBUTING
// THIS SOFTWARE OR ITS DERIVATIVES.
//=============================================================================
//=============================================================================
//
// Modified by: Ohan Oda (ohan@cs.columbia.edu)
//
//==========================================================================


using System;
using System.Runtime.InteropServices;

namespace GoblinXNA.Device.Capture.PointGrey
{
    /// <summary>
    /// A camera driver class for Point Grey Research firefly/dragonfly cameras.
    /// (This class is provided by Point Grey Research)
    /// </summary>
	internal unsafe class PGRFlyCapture : IDisposable
    {
        #region Constants
        // Bitmap constant
		public const short DIB_RGB_COLORS = 0;

		// The maximum number of cameras on the bus.
		public const int _MAX_CAMS = 3;
        #endregion

        #region Variables
        private int flycapContext;
        private int cameraIndex;

        private PGRFlyModule.FlyCaptureInfo flycapInfo;
        private PGRFlyModule.FlyCaptureImage image;
        private PGRFlyModule.FlyCaptureImage flycapRGBImage;
        private PGRFlyModule.FlyCaptureCameraType cameraType;
        private PGRFlyModule.FlyCaptureCameraModel cameraModel;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a camera driver for Point Grey Research firefly/dragong fly cameras.
        /// </summary>
        public PGRFlyCapture()
        {
            flycapInfo = new PGRFlyModule.FlyCaptureInfo();
            image = new PGRFlyModule.FlyCaptureImage();
            flycapRGBImage = new PGRFlyModule.FlyCaptureImage();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The model of the camera
        /// </summary>
        /// <seealso cref="GoblinXNA.Device.Capture.PointGrey.PGRFlyModule"/>
        public PGRFlyModule.FlyCaptureCameraModel CameraModel
        {
            get { return cameraModel; }
        }

        /// <summary>
        /// The type of the camera (e.g., b&w or color)
        /// </summary>
        /// <seealso cref="GoblinXNA.Device.Capture.PGRFlyModule"/>
        public PGRFlyModule.FlyCaptureCameraType CameraType
        {
            get { return cameraType; }
        }
        #endregion

        /// <summary>
        /// Initializes the camera driver
        /// </summary>
        /// <param name="cameraIndex">If only one Point Grey camera is connected, then use '0'. 
        /// If more than one Point Grey cameras connected, then use between '0' and 'number of 
        /// Point Grey cameras connected - 1'</param>
        /// <param name="frameRate">The frame rate you desire</param>
        /// <param name="videoMode"></param>
        /// <param name="grayscale"></param>
        unsafe public void Initialize(int cameraIndex, PGRFlyModule.FlyCaptureFrameRate frameRate, 
            PGRFlyModule.FlyCaptureVideoMode videoMode, bool grayscale)
        {
            this.cameraIndex = cameraIndex;

            int flycapContext;
            int ret;
            // Create the context.
            ret = PGRFlyDllBridge.flycaptureCreateContext(&flycapContext);
            if (ret != 0)
                ReportError(ret, "flycaptureCreateContext");

            // Initialize the camera.
            ret = PGRFlyDllBridge.flycaptureInitialize(flycapContext, cameraIndex);
            if (ret != 0)
                ReportError(ret, "flycaptureInitialize");

            // Get the info for this camera.
            ret = PGRFlyDllBridge.flycaptureGetCameraInformation(flycapContext, ref flycapInfo);
            if (ret != 0)
                ReportError(ret, "flycaptureGetCameraInformation");

            if (flycapInfo.CameraType ==
                PGRFlyModule.FlyCaptureCameraType.FLYCAPTURE_BLACK_AND_WHITE && !grayscale)
                throw new GoblinException("This Point Grey camera is B&W, so you need to initialize " +
                    "the video capture device with grayscale");

            cameraType = flycapInfo.CameraType;
            cameraModel = flycapInfo.CameraModel;

            // Start FlyCapture.
            /*if (cameraModel == PGRFlyModule.FlyCaptureCameraModel.FLYCAPTURE_DRAGONFLY2)
                ret = PGRFlyDllBridge.flycaptureStart(flycapContext, 
                    PGRFlyModule.FlyCaptureVideoMode.FLYCAPTURE_VIDEOMODE_640x480RGB, frameRate);
            else*/
                ret = PGRFlyDllBridge.flycaptureStart(flycapContext, videoMode, frameRate);
            
            if (ret != 0)
                ReportError(ret, "flycaptureStart");

            this.flycapContext = flycapContext;
        }

        /// <summary>
        /// Grabs
        /// </summary>
        /// <param name="camImage"></param>
        /// <returns></returns>
        public PGRFlyModule.FlyCaptureImage GrabRGBImage(IntPtr camImage)
        {
            int ret;
            ret = PGRFlyDllBridge.flycaptureGrabImage2(flycapContext, ref image);
            if (ret != 0)
            {
                //ReportError(ret, "flycaptureGrabImage2");
                PGRFlyModule.FlyCaptureImage tmpImage = new PGRFlyModule.FlyCaptureImage();
                tmpImage.pData = null;
                return tmpImage;
            }

            if (cameraModel == PGRFlyModule.FlyCaptureCameraModel.FLYCAPTURE_DRAGONFLY2)
                return image;
            else
            {
                // Convert the image.
                flycapRGBImage.pData = (byte*)camImage;
                flycapRGBImage.pixelFormat = PGRFlyModule.FlyCapturePixelFormat.FLYCAPTURE_BGR;
                ret = PGRFlyDllBridge.flycaptureConvertImage(flycapContext, ref image, ref flycapRGBImage);
                if (ret != 0)
                    ReportError(ret, "flycaptureConvertImage");

                return flycapRGBImage;
            }
        }

		private void ReportError( int ret, string fname )
		{
            //throw new GoblinException(fname + " error: " + PGRFlyDllBridge.flycaptureErrorToString(ret));
		}

        #region IDisposable Members

        public void Dispose()
        {
            int ret;
            // Stop FlyCapture.
            ret = PGRFlyDllBridge.flycaptureStop(flycapContext);
            /*if (ret != 0)
                ReportError(ret, "flycaptureStop");*/

            // Destroy the context.
            ret = PGRFlyDllBridge.flycaptureDestroyContext(flycapContext);
            /*if(ret != 0)
                ReportError(ret, "flycaptureDestroyContext");*/
        }

        #endregion
    }
}



