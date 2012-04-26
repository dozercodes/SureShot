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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;

using GoblinXNA.Helpers;
using GoblinXNA.Device.Capture;
using ImageFormat = GoblinXNA.Device.Capture.ImageFormat;

namespace GoblinXNA.Device.Vision.Marker
{
    /// <summary>
    /// A marker tracker implementation using the ALVAR (http://virtual.vtt.fi/virtual/proj2/multimedia/) 
    /// library developed by VTT.
    /// </summary>
    public class ALVARMarkerTracker : IMarkerTracker
    {
        #region Member Fields

        private Dictionary<int, Matrix> detectedMarkers;
        private Dictionary<String, Matrix> detectedMultiMarkers;

        private List<int> singleMarkerIDs;
        private IntPtr singleMarkerIDsPtr;
        private List<String> multiMarkerIDs;
        private int multiMarkerID;

        private Matrix lastMarkerMatrix;
        private double max_marker_error;
        private double max_track_error;

        private Matrix camProjMat;

        private double cameraFovX;
        private double cameraFovY;

        private bool initialized;

        private String configFilename;

        private float zNearPlane;
        private float zFarPlane;

        private int colorChannel;

        private int[] ids;
        private double[] poseMats;
        private IntPtr idPtr;
        private IntPtr posePtr;
        private int prevMarkerNum;

        private int[] multiIDs;
        private double[] multiPoseMats;
        private double[] multiErrors;
        private IntPtr multiIdPtr;
        private IntPtr multiPosePtr;
        private IntPtr multiHideTexturePtr;
        private IntPtr multiErrorPtr;

        private int detectorID;
        private int cameraID;
        private bool detectAdditional;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an ALVAR marker tracker.
        /// </summary>
        public ALVARMarkerTracker()
        {
            configFilename = "";

            lastMarkerMatrix = Matrix.Identity;
            max_marker_error = 0.08;
            max_track_error = 0.2;

            cameraFovX = 0;
            cameraFovY = 0;

            camProjMat = Matrix.Identity;
            initialized = false;

            zNearPlane = 0.1f;
            zFarPlane = 1000;

            detectedMarkers = new Dictionary<int, Matrix>();
            detectedMultiMarkers = new Dictionary<string, Matrix>();

            singleMarkerIDs = new List<int>();
            singleMarkerIDsPtr = IntPtr.Zero;
            multiMarkerIDs = new List<string>();
            multiMarkerID = 0;

            colorChannel = 0;

            ids = null;
            poseMats = null;
            prevMarkerNum = 0;
            idPtr = IntPtr.Zero;
            posePtr = IntPtr.Zero;

            multiIDs = null;
            multiPoseMats = null;
            multiErrors = null;
            multiIdPtr = IntPtr.Zero;
            multiPosePtr = IntPtr.Zero;
            multiHideTexturePtr = IntPtr.Zero;
            multiErrorPtr = IntPtr.Zero;

            detectAdditional = false;
            detectorID = -1;
            cameraID = -1;

            EnableTracking = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the camera's horizontal field of view in radians.
        /// </summary>
        public double CameraFovX
        {
            get { return cameraFovX; }
        }

        /// <summary>
        /// Gets the camera's vertical field of view in radians.
        /// </summary>
        public double CameraFovY
        {
            get { return cameraFovY; }
        }

        /// <summary>
        /// Default value is 0.08.
        /// </summary>
        public double MaxMarkerError
        {
            get { return max_marker_error; }
            set { max_marker_error = value; }
        }

        /// <summary>
        /// Default value is 0.2.
        /// </summary>
        public double MaxTrackError
        {
            get { return max_track_error; }
            set { max_track_error = value; }
        }

        /// <summary>
        /// Default value is false.
        /// </summary>
        public bool DetectAdditional
        {
            get { return detectAdditional; }
            set { detectAdditional = value; }
        }

        public bool Initialized
        {
            get { return initialized; }
        }

        public Matrix CameraProjection
        {
            get
            {
                return camProjMat;
            }
        }

        /// <summary>
        /// Gets or sets the near clipping plane used to compute CameraProjection.
        /// The default value is 0.1f.
        /// </summary>
        /// <remarks>
        /// This property should be set before calling InitTracker(...).
        /// </remarks>
        public float ZNearPlane
        {
            get { return zNearPlane; }
            set
            {
                if (initialized)
                    throw new MarkerException("You need to set this property before initialization");

                zNearPlane = value;
            }
        }

        /// <summary>
        /// Gets or sets the far clipping plane used to compute CameraProjection.
        /// The default value is 1000.
        /// </summary>
        /// <remarks>
        /// This property should be set before calling InitTracker(...).
        /// </remarks>
        public float ZFarPlane
        {
            get { return zFarPlane; }
            set
            {
                if (initialized)
                    throw new MarkerException("You need to set this property before initialization");

                zFarPlane = value;
            }
        }

        public bool EnableTracking
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current marker detector ID. The current detector ID specifies which marker
        /// detector to use when more than one detector is added in the case you need more than one 
        /// instance of marker detector. For instance, if you plan to track markers from different video
        /// capture devices at the same time (e.g., stereo mode with two physical cameras), then you will
        /// need two marker detector instances for proper tracking (ALVAR makes assumptions based on history 
        /// information for performing tracking on images with sharp angle or far away, so if you pass images
        /// from different physical location sequentially, the tracking will get very messy).
        /// 
        /// Make sure to add a new marker detector before setting this property, and use 
        /// ALVARDllBridge.alvar_add_marker_detector(...) function to add a new marker detector. The detector
        /// ID is the return value of this function if no error occurs.
        /// </summary>
        /// <remarks>
        /// Generally, you only need one instance of marker detector, so unless you add and need more than
        /// one instances of marker detector, it's best not to change this property.
        /// </remarks>
        public int DetectorID
        {
            get { return detectorID; }
            set { detectorID = value; }
        }

        /// <summary>
        /// Gets or sets the current camera ID used by the marker detector. A camera in ALVAR contains
        /// lens intrinsic parameters and distortion information, so if you use different physical capture
        /// devices for tracking, then you should add additional camera. Make sure to add a new camera before
        /// setting this property, and use ALVARDllBridge.alvar_add_camera(...) function to add a new camera.
        /// </summary>
        /// <remarks>
        /// Generally, you only need one camera, so unless you add and need more than one instance of camera,
        /// it's best not to change this property. The camera ID is the return value of this function if 
        /// no error occurs.
        /// </remarks>
        /// <see cref="ALVARDllBridge.alvar_add_camera"/>
        public int CameraID
        {
            get { return cameraID; }
            set { cameraID = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initilizes the marker tracker with a set of configuration parameters.
        /// </summary>
        /// <param name="configs">
        /// There are two ways to pass the parameters. One way is to pass in the order of
        /// (int imageWidth, int imageHeight, String cameraCalibFilename, double markerSize,
        /// int markerRes, double margin), and the other way is (int imageWidth, int imageHeight, 
        /// String cameraCalibFilename, double markerSize).
        /// </param>
        public void InitTracker(params Object[] configs)
        {
            if (!(configs.Length == 4 || configs.Length == 6))
                throw new MarkerException(GetInitTrackerUsage());

            int markerRes = 5;
            double markerSize = 1, margin = 2;
            int img_width = 0;
            int img_height = 0;
            if (configs.Length == 4)
            {
                try
                {
                    img_width = (int)configs[0];
                    img_height = (int)configs[1];
                    configFilename = (String)configs[2];
                    markerSize = Double.Parse(configs[3].ToString());
                }
                catch (Exception)
                {
                    throw new MarkerException(GetInitTrackerUsage());
                }
            }
            else
            {
                try
                {
                    img_width = (int)configs[0];
                    img_height = (int)configs[1];
                    configFilename = (String)configs[2];
                    markerSize = Double.Parse(configs[3].ToString());
                    markerRes = (int)configs[4];
                    margin = Double.Parse(configs[5].ToString());
                }
                catch (Exception)
                {
                    throw new MarkerException(GetInitTrackerUsage());
                }
            }

            ALVARDllBridge.alvar_init();

            int ret = ALVARDllBridge.alvar_add_camera(configFilename, img_width, img_height);
            if (ret < 0)
                throw new MarkerException("Camera calibration file is either not specified or not found");

            cameraID = ret;

            double[] projMat = new double[16];
            ALVARDllBridge.alvar_get_camera_params(cameraID, projMat, ref cameraFovX, ref cameraFovY, zFarPlane, zNearPlane);
            camProjMat = new Matrix(
                (float)projMat[0], (float)projMat[1], (float)projMat[2], (float)projMat[3],
                (float)projMat[4], (float)projMat[5], (float)projMat[6], (float)projMat[7],
                (float)projMat[8], (float)projMat[9], (float)projMat[10], (float)projMat[11],
                (float)projMat[12], (float)projMat[13], (float)projMat[14], (float)projMat[15]);

            detectorID = ALVARDllBridge.alvar_add_marker_detector(markerSize, markerRes, margin);

            initialized = true;
        }

        /// <summary>
        /// Associates a marker with an identifier so that the identifier can be used to find this
        /// marker after processing the image. 
        /// </summary>
        /// <param name="markerConfigs">There are three ways to pass the parameters; (int markerID),
        /// (int markerID, double markerSize), or (String multiMarkerConfig). </param>
        /// <returns>An identifier for this marker object</returns>
        public Object AssociateMarker(params Object[] markerConfigs)
        {
            // make sure we are initialized
            if (!initialized)
                throw new MarkerException("ALVARMarkerTracker is not initialized. Call InitTracker(...)");

            if (!(markerConfigs.Length == 1 || markerConfigs.Length == 2))
                throw new MarkerException(GetAssocMarkerUsage());

            Object id = null;

            if (markerConfigs.Length == 1)
            {
                if (markerConfigs[0] is string)
                {
                    String markerConfigName = (String)markerConfigs[0];
                    if (markerConfigName.Equals(""))
                        throw new MarkerException(GetAssocMarkerUsage());
                    else
                    {
                        ALVARDllBridge.alvar_add_multi_marker(markerConfigName);
                        id = markerConfigName;
                    }

                    multiMarkerIDs.Add((String)id);
                    multiMarkerID++;

                    multiIdPtr = Marshal.AllocHGlobal(multiMarkerIDs.Count * sizeof(int));
                    multiPosePtr = Marshal.AllocHGlobal(multiMarkerIDs.Count * 16 * sizeof(double));
                    multiErrorPtr = Marshal.AllocHGlobal(multiMarkerIDs.Count * sizeof(double));

                    multiIDs = new int[multiMarkerIDs.Count];
                    multiPoseMats = new double[multiMarkerIDs.Count * 16];
                    multiErrors = new double[multiMarkerIDs.Count];
                }
                else if (markerConfigs[0] is int)
                {
                    id = markerConfigs[0];
                    int markerID = (int)markerConfigs[0];
                    singleMarkerIDs.Add(markerID);

                    singleMarkerIDsPtr = Marshal.AllocHGlobal(singleMarkerIDs.Count * sizeof(int));
                    unsafe
                    {
                        int* dest = (int*)singleMarkerIDsPtr;
                        for (int i = 0; i < singleMarkerIDs.Count; i++)
                            *(dest + i) = singleMarkerIDs[i];
                    }
                }
                else
                    throw new MarkerException(GetAssocMarkerUsage());
            }
            else
            {
                try
                {
                    if (markerConfigs[0] is int)
                    {
                        id = markerConfigs[0];
                        int markerID = (int)markerConfigs[0];
                        double markerSize = Double.Parse(markerConfigs[1].ToString());
                        ALVARDllBridge.alvar_set_marker_size(detectorID, markerID, markerSize);
                        singleMarkerIDs.Add(markerID);

                        singleMarkerIDsPtr = Marshal.AllocHGlobal(singleMarkerIDs.Count * sizeof(int));
                        unsafe
                        {
                            int* dest = (int*)singleMarkerIDsPtr;
                            for (int i = 0; i < singleMarkerIDs.Count; i++)
                                *(dest + i) = singleMarkerIDs[i];
                        }
                    }
                    else
                    {
                        String markerConfigName = (String)markerConfigs[0];
                        if (markerConfigName.Equals(""))
                            throw new MarkerException(GetAssocMarkerUsage());
                        else
                        {
                            ALVARDllBridge.alvar_add_multi_marker(markerConfigName);
                            id = markerConfigName;
                        }

                        multiMarkerIDs.Add((String)id);
                        multiMarkerID++;

                        multiIdPtr = Marshal.AllocHGlobal(multiMarkerIDs.Count * sizeof(int));
                        multiPosePtr = Marshal.AllocHGlobal(multiMarkerIDs.Count * 16 * sizeof(double));
                        multiErrorPtr = Marshal.AllocHGlobal(multiMarkerIDs.Count * sizeof(double));

                        multiIDs = new int[multiMarkerIDs.Count];
                        multiPoseMats = new double[multiMarkerIDs.Count * 16];
                        multiErrors = new double[multiMarkerIDs.Count];
                    }
                }
                catch (Exception)
                {
                    throw new MarkerException(GetAssocMarkerUsage());
                }
            }

            return id;
        }

        /// <summary>
        /// Processes the video image captured from an initialized video capture device. 
        /// </summary>
        /// <param name="captureDevice">An initialized video capture device</param>
        public void ProcessImage(IVideoCapture captureDevice, IntPtr imagePtr)
        {
            String channelSeq = "";
            int nChannles = 1;
            switch(captureDevice.Format)
            {
                case ImageFormat.R5G6B5_16:
                case ImageFormat.R8G8B8_24:
                    channelSeq = "RGB";
                    nChannles = 3;
                    break;
                case ImageFormat.R8G8B8A8_32:
                    channelSeq = "RGBA";
                    nChannles = 4;
                    break;
                case ImageFormat.B8G8R8_24:
                    channelSeq = "BGR";
                    nChannles = 3;
                    break;
                case ImageFormat.B8G8R8A8_32:
                    channelSeq = "BGRA";
                    nChannles = 4;
                    break;
                case ImageFormat.A8B8G8R8_32:
                    channelSeq = "ARGB";
                    nChannles = 4;
                    break;
            }

            int interestedMarkerNums = singleMarkerIDs.Count;
            int foundMarkerNums = 0;

            ALVARDllBridge.alvar_detect_marker(detectorID, cameraID, nChannles, channelSeq, channelSeq, 
                imagePtr, singleMarkerIDsPtr, ref foundMarkerNums, ref interestedMarkerNums,
                max_marker_error, max_track_error);

            Process(interestedMarkerNums, foundMarkerNums);
        }

        /// <summary>
        /// Checks whether a marker identified by 'markerID' is found in the processed image
        /// after calling ProcessImage(...) method.
        /// </summary>
        /// <param name="markerID">An ID associated with a marker returned from AssociateMarker(...)
        /// method.</param>
        /// <returns>A boolean value representing whether a marker was found</returns>
        public bool FindMarker(Object markerID)
        {
            bool found = false;
            if (markerID is int)
            {
                if (detectedMarkers.Count == 0)
                    return false;

                int id = (int)markerID;
                found = detectedMarkers.ContainsKey(id);
                if (found)
                {
                    lastMarkerMatrix = detectedMarkers[id];
                }
            }
            else
            {
                if (detectedMultiMarkers.Count == 0)
                    return false;

                String id = (String)markerID;
                found = detectedMultiMarkers.ContainsKey(id);
                if (found)
                {
                    lastMarkerMatrix = detectedMultiMarkers[id];
                }
            }

            return found;
        }

        /// <summary>
        /// Gets the pose transformation of the found marker after calling the FindMarker(...) method.
        /// </summary>
        /// <remarks>
        /// This method should be called if and only if FindMarker(...) method returned true for
        /// the marker you're looking for. 
        /// </remarks>
        /// <returns>The pose transformation of a found marker</returns>
        public Matrix GetMarkerTransform()
        {
            return lastMarkerMatrix;
        }

        /// <summary>
        /// Disposes this marker tracker.
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose
        }

        #endregion

        #region Private Methods

        private String GetInitTrackerUsage()
        {
            return "Usage: InitTracker(int imgWidth, int imgHeight, String cameraCalibFilename, " +
                "double markerSize, int markerRes, double margin) or InitTracker(int imgWidth, " +
                "int imgHeight, String cameraCalibFilename, double markerSize)";
        }

        private String GetAssocMarkerUsage()
        {
            return "Usage: AssociateMarker(int markerID) or AssociateMarker(int markerID, " +
                "double markerSize) or AssociateMarker(String multiMarkerConfig)";
        }

        private void Process(int interestedMarkerNums, int foundMarkerNums)
        {
            detectedMarkers.Clear();
            detectedMultiMarkers.Clear();

            if (foundMarkerNums <= 0)
                return;

            int id = 0;
            if (interestedMarkerNums > 0)
            {
                if (prevMarkerNum != interestedMarkerNums)
                {
                    ids = new int[interestedMarkerNums];
                    poseMats = new double[interestedMarkerNums * 16];
                    idPtr = Marshal.AllocHGlobal(interestedMarkerNums * sizeof(int));
                    posePtr = Marshal.AllocHGlobal(interestedMarkerNums * 16 * sizeof(double));
                }

                ALVARDllBridge.alvar_get_poses(detectorID, idPtr, posePtr);

                prevMarkerNum = interestedMarkerNums;

                Marshal.Copy(idPtr, ids, 0, interestedMarkerNums);
                Marshal.Copy(posePtr, poseMats, 0, interestedMarkerNums * 16);

                for (int i = 0; i < interestedMarkerNums; i++)
                {
                    id = ids[i];

                    // If same marker ID exists, then we ignore the 2nd one
                    if (detectedMarkers.ContainsKey(id))
                    {
                        // do nothing
                    }
                    else
                    {
                        int index = i * 16;
                        Matrix mat = new Matrix(
                            (float)poseMats[index], (float)poseMats[index + 1], (float)poseMats[index + 2], (float)poseMats[index + 3],
                            (float)poseMats[index + 4], (float)poseMats[index + 5], (float)poseMats[index + 6], (float)poseMats[index + 7],
                            (float)poseMats[index + 8], (float)poseMats[index + 9], (float)poseMats[index + 10], (float)poseMats[index + 11],
                            (float)poseMats[index + 12], (float)poseMats[index + 13], (float)poseMats[index + 14], (float)poseMats[index + 15]);
                        detectedMarkers.Add(id, mat);
                    }
                }
            }

            if (multiMarkerIDs.Count == 0)
                return;

            double error = -1;

            ALVARDllBridge.alvar_get_multi_marker_poses(detectorID, cameraID, detectAdditional, 
                multiIdPtr, multiPosePtr, multiErrorPtr);

            Marshal.Copy(multiIdPtr, multiIDs, 0, multiMarkerIDs.Count);
            Marshal.Copy(multiPosePtr, multiPoseMats, 0, multiMarkerIDs.Count * 16);
            Marshal.Copy(multiErrorPtr, multiErrors, 0, multiMarkerIDs.Count);

            for (int i = 0; i < multiMarkerIDs.Count; i++)
            {
                id = multiIDs[i];
                error = multiErrors[i];

                if (error == -1)
                    continue;

                int index = i * 16;
                Matrix mat = new Matrix(
                    (float)multiPoseMats[index], (float)multiPoseMats[index + 1], (float)multiPoseMats[index + 2], (float)multiPoseMats[index + 3],
                    (float)multiPoseMats[index + 4], (float)multiPoseMats[index + 5], (float)multiPoseMats[index + 6], (float)multiPoseMats[index + 7],
                    (float)multiPoseMats[index + 8], (float)multiPoseMats[index + 9], (float)multiPoseMats[index + 10], (float)multiPoseMats[index + 11],
                    (float)multiPoseMats[index + 12], (float)multiPoseMats[index + 13], (float)multiPoseMats[index + 14], (float)multiPoseMats[index + 15]);
                detectedMultiMarkers.Add(multiMarkerIDs[i], mat);
            }
        }

        #endregion
    }
}
