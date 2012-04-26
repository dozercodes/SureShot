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
using System.Text;
using System.Runtime.InteropServices;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

// NyARToolkit library from http://nyatla.jp/nyartoolkit/wiki/index.php
using jp.nyatla.nyartoolkit.cs;
using jp.nyatla.nyartoolkit.cs.core;
using jp.nyatla.nyartoolkit.cs.detector;

using GoblinXNA.Helpers;
using GoblinXNA.Device.Capture;

using GoblinXNA.Helpers;
using GoblinXNA.Device.Capture;

namespace GoblinXNA.Device.Vision.Marker
{
    /// <summary>
    /// A marker tracker implementation using NyARToolkit 3.0.0
    /// </summary>
    public class NyARToolkitTracker : IMarkerTracker
    {
        /// <summary>
        /// An enum that defines how the resulting transform should be computed from all visible markers.
        /// </summary>
        /// <remarks>
        /// For Custom method, you must set the ComputeTransform delegate property.
        /// </remarks>
        public enum ComputationMethod { Average, BestConfidence, WeightedAverage, Custom }

        /// <summary>
        /// Computes the resulting transform based on the visible markers' transforms and their confidence values.
        /// </summary>
        /// <param name="transforms">The list of visible markers' transforms</param>
        /// <param name="confidences">The list of visible markers' confidence values</param>
        /// <param name="result">The computed transform</param>
        public delegate void ComputeMultiMarkeTransform(List<Matrix> transforms, List<float> confidences, out Matrix result);

        #region Member Fields

        private NyARParam param;
        private MarkerDetector multiDetector;
        private DsBGRX32Raster raster;
        private Dictionary<int, MarkerInfo> markerInfoMap;
        private List<MarkerInfo> markerInfoList;
        private List<NyARCode> codes;
        private List<double> pattSizes;
        private Object lastMarkerID;

        /// <summary>
        /// The object ID that we are working with
        /// </summary>
        private Matrix camProjMat;

        private bool initialized;
        private bool started;

        private String configFilename;
        private bool continuousMode;
        private int threshold;

        private float zNearPlane;
        private float zFarPlane;

        private Matrix tmpMat1;
        private Matrix tmpMat2;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an ARTag marker tracker.
        /// </summary>
        public NyARToolkitTracker()
        {
            configFilename = "";
            camProjMat = Matrix.Identity;

            initialized = false;
            markerInfoList = new List<MarkerInfo>();
            markerInfoMap = new Dictionary<int, MarkerInfo>();
            codes = new List<NyARCode>();
            pattSizes = new List<double>();

            zNearPlane = 10;
            zFarPlane = 2000;

            EnableTracking = true;

            tmpMat1 = Matrix.Identity;
            tmpMat2 = Matrix.Identity;
        }

        #endregion

        #region Properties

        public bool Initialized
        {
            get { return initialized; }
        }

        public Matrix CameraProjection
        {
            get { return camProjMat; }
        }

        /// <summary>
        /// Gets or sets the near clipping plane used to compute CameraProjection.
        /// The default value is 10.
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
        /// The default value is 2000.
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

        /// <summary>
        /// Gets or sets whether to enable the tracking.
        /// </summary>
        public bool EnableTracking
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the function used to compute the transform of marker array using all visible markers.
        /// </summary>
        public ComputeMultiMarkeTransform ComputeTransform
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initilizes the marker tracker with a set of configuration parameters.
        /// Five parameters are expected: int imgWidth, int imgHeight, String cameraFilename, 
        /// int threshold, bool continuousMode
        /// </summary>
        /// <param name="configs">A set of configuration parameters</param>
        public void InitTracker(params Object[] configs)
        {
            if (!(configs.Length == 3 || configs.Length == 5))
                throw new MarkerException(GetInitTrackerUsage());

            int img_width = 0;
            int img_height = 0;
            try
            {
                img_width = (int)configs[0];
                img_height = (int)configs[1];
                configFilename = (String)configs[2];
                if (configs.Length == 5)
                {
                    threshold = (int)configs[3];
                    continuousMode = (bool)configs[4];
                }
                else
                {
                    threshold = 100;
                    continuousMode = false;
                }
            }
            catch (Exception)
            {
                throw new MarkerException(GetInitTrackerUsage());
            }

            raster = new DsBGRX32Raster(img_width, img_height);

            param = new NyARParam();
            param.loadARParam(TitleContainer.OpenStream(configFilename));
            param.changeScreenSize(img_width, img_height);

            camProjMat = GetProjectionMatrix(param, zNearPlane, zFarPlane);

            initialized = true;
        }

        /// <summary>
        /// Associates a marker with an identifier so that the identifier can be used to find this
        /// marker after processing the image. 
        /// </summary>
        /// <param name="markerConfigs">A set of parameters that identifies a maker. (e.g., for
        /// ARTag, this parameter would be the marker array name or marker ID)</param>
        /// <returns>An identifier for this marker object</returns>
        public Object AssociateMarker(params Object[] markerConfigs)
        {
            // make sure we are initialized
            if (!initialized)
                throw new MarkerException("ARToolkitTracker is not initialized. Call InitTracker(...)");

            if (!(markerConfigs.Length == 2 || markerConfigs.Length == 5))
                throw new MarkerException(GetAssocMarkerUsage());

            MarkerInfo markerInfo = new MarkerInfo();

            if (markerConfigs.Length == 2)
            {
                string arrayName = "";
                ComputationMethod method = ComputationMethod.Average;

                try
                {
                    arrayName = (String)markerConfigs[0];
                    method = (ComputationMethod)markerConfigs[1];
                }
                catch (Exception)
                {
                    throw new MarkerException(GetAssocMarkerUsage());
                }

                ParseArray(arrayName, ref markerInfo);

                markerInfo.Method = method;
            }
            else
            {
                int pattWidth = 0, pattHeight = 0;
                float pattSize = 0, conf = 0;
                String pattName = "";

                try
                {
                    pattName = (String)markerConfigs[0];
                    pattWidth = (int)markerConfigs[1];
                    pattHeight = (int)markerConfigs[2];
                    pattSize = (float)markerConfigs[3];
                    conf = (float)markerConfigs[4];
                }
                catch (Exception)
                {
                    throw new MarkerException(GetAssocMarkerUsage());
                }

                NyARCode code = new NyARCode(pattWidth, pattHeight);
                code.loadARPatt(new System.IO.StreamReader(TitleContainer.OpenStream(pattName)));
                codes.Add(code);
                pattSizes.Add(pattSize);

                PatternInfo info = new PatternInfo();
                info.ConfidenceThreshold = conf;

                int id = codes.Count - 1;
                markerInfo.PatternInfos.Add(id, info);
                markerInfo.RelativeTransforms.Add(id, Matrix.Identity);
                markerInfo.Method = ComputationMethod.Average;

                markerInfoMap.Add(id, markerInfo);
            }

            markerInfoList.Add(markerInfo);

            // reinitialize the multi marker detector if the programmer adds new marker node
            // after the initialization phase
            if (started)
            {
                multiDetector = new MarkerDetector(param, codes.ToArray(), pattSizes.ToArray(),
                    codes.Count, raster.getBufferType());
                multiDetector.setContinueMode(continuousMode);
            }

            return markerInfo;
        }

#if WINDOWS
        /// <summary>
        /// Processes the video image captured from an initialized video capture device. 
        /// </summary>
        /// <param name="captureDevice">An initialized video capture device</param>
        public void ProcessImage(IVideoCapture captureDevice, IntPtr imagePtr)
        {
            if (captureDevice.Format != ImageFormat.B8G8R8A8_32)
                throw new MarkerException("Only ImageFormat.B8G8R8A8_32 format is acceptable for NyARToolkitTracker");

            // initialize the detector right before the image processing
            if (!started)
            {
                multiDetector = new MarkerDetector(param, codes.ToArray(), pattSizes.ToArray(),
                    codes.Count, raster.getBufferType());
                multiDetector.setContinueMode(continuousMode);
                started = true;
            }

            raster.SetBuffer(imagePtr);

            UpdateMarkerTransforms();
        }
#else
        public void ProcessImage(IVideoCapture captureDevice, byte[] imagePtr)
        {
            if (captureDevice.Format != ImageFormat.B8G8R8A8_32)
                throw new MarkerException("Only ImageFormat.B8G8R8A8_32 format is acceptable for NyARToolkitTracker");

            // initialize the detector right before the image processing
            if (!started)
            {
                multiDetector = new MarkerDetector(param, codes.ToArray(), pattSizes.ToArray(),
                    codes.Count, raster.getBufferType());
                multiDetector.setContinueMode(continuousMode);
                started = true;
            }

            raster.SetBuffer(imagePtr);

            UpdateMarkerTransforms();
        }
#endif

        /// <summary>
        /// Checks whether a marker identified by 'markerID' is found in the processed image
        /// after calling ProcessImage(...) method.
        /// </summary>
        /// <param name="markerID">An ID associated with a marker returned from AssociateMarker(...)
        /// method.</param>
        /// <returns>A boolean value representing whether a marker was found</returns>
        public bool FindMarker(Object markerID)
        {
            lastMarkerID = markerID;

            return ((MarkerInfo)lastMarkerID).IsFound;
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
            return ((MarkerInfo)lastMarkerID).Transform;
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
            return "Usage: InitTracker(int imgWidth, int imgHeight, String cameraFilename) or " +
                "InitTracker(int imgWidthInitTracker(int imgWidth, int imgHeight, String cameraFilename, " +
                "int threshold, bool continuousMode)";
        }

        private String GetAssocMarkerUsage()
        {
            return "Usage: AssociateMarker(String arrayFilename, ComputationMethod method) or " +
                "AssociateMarker(String patternFilename, int pattWidth, int pattHeight, " +
                "float pattSize, float confidenceThreshold)";
        }

        private void UpdateMarkerTransforms()
        {
            // Reset all marker patterns to not found
            foreach (MarkerInfo markerInfo in markerInfoList)
            {
                markerInfo.IsFound = false;
                foreach (PatternInfo patternInfo in markerInfo.PatternInfos.Values)
                    patternInfo.IsFound = false;
            }

            int numMarkerFound = multiDetector.detectMarkerLite(raster, threshold);
            NyARTransMatResult result = new NyARTransMatResult();

            for (int i = 0; i < numMarkerFound; ++i)
            {
                int id = multiDetector.getARCodeIndex(i);

                MarkerInfo markerInfo = markerInfoMap[id];

                if (multiDetector.getConfidence(i) >= markerInfo.PatternInfos[id].ConfidenceThreshold)
                {
                    try
                    {
                        markerInfo.IsFound = true;
                        markerInfo.PatternInfos[id].IsFound = true;
                        markerInfo.PatternInfos[id].Confidence = (float)multiDetector.getConfidence(i);
                        multiDetector.getTransmationMatrix(i, result);
                        markerInfo.PatternInfos[id].Transform = (new Matrix(
                            (float)result.m00, (float)result.m10, (float)result.m20, (float)result.m30,
                            (float)result.m01, (float)result.m11, (float)result.m21, (float)result.m31,
                            (float)result.m02, (float)result.m12, (float)result.m22, (float)result.m32,
                            (float)result.m03, (float)result.m13, (float)result.m23, (float)result.m33)) *
                             Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.Pi);
                    }
                    catch (NyARException exp)
                    {
                        Log.Write(exp.Message);

                        markerInfo.PatternInfos[id].IsFound = false;
                        markerInfo.PatternInfos[id].Confidence = 0;
                    }
                }
            }

            // Compute the final transformation of each marker info
            foreach (MarkerInfo markerInfo in markerInfoList)
            {
                if (markerInfo.IsFound)
                {
                    Matrix resultMat = MatrixHelper.Empty;

                    List<Matrix> transforms = null;
                    List<float> weights = null;

                    switch (markerInfo.Method)
                    {
                        case ComputationMethod.Average:
                            int count = 0;
                            foreach (int id in markerInfo.PatternInfos.Keys)
                            {
                                PatternInfo patternInfo = markerInfo.PatternInfos[id];
                                if (patternInfo.IsFound)
                                {
                                    tmpMat1 = markerInfo.RelativeTransforms[id];
                                    Matrix.Multiply(ref tmpMat1, ref patternInfo.Transform, out tmpMat2);
                                    Matrix.Add(ref resultMat, ref tmpMat2, out resultMat);
                                    count++;
                                }
                            }

                            resultMat /= count;
                            break;
                        case ComputationMethod.BestConfidence:
                            float bestConfidence = 0;
                            foreach (int id in markerInfo.PatternInfos.Keys)
                            {
                                PatternInfo patternInfo = markerInfo.PatternInfos[id];
                                if (patternInfo.IsFound && patternInfo.Confidence > bestConfidence)
                                {
                                    tmpMat1 = markerInfo.RelativeTransforms[id];
                                    Matrix.Multiply(ref tmpMat1, ref patternInfo.Transform, out resultMat);
                                    bestConfidence = patternInfo.Confidence;
                                }
                            }
                            break;
                        case ComputationMethod.WeightedAverage:
                            transforms = new List<Matrix>();
                            weights = new List<float>();
                            float weightSum = 0;

                            foreach (int id in markerInfo.PatternInfos.Keys)
                            {
                                PatternInfo patternInfo = markerInfo.PatternInfos[id];
                                if (patternInfo.IsFound)
                                {
                                    Matrix transform = markerInfo.RelativeTransforms[id];
                                    Matrix.Multiply(ref transform, ref patternInfo.Transform, out transform);
                                    transforms.Add(transform);
                                    weights.Add(patternInfo.Confidence);
                                    weightSum += patternInfo.Confidence;
                                }
                            }

                            for (int i = 0; i < weights.Count; ++i)
                                weights[i] /= weightSum;

                            for (int i = 0; i < transforms.Count; ++i)
                            {
                                tmpMat1 = transforms[i];
                                Matrix.Multiply(ref tmpMat1, weights[i], out tmpMat2);
                                Matrix.Add(ref resultMat, ref tmpMat2, out resultMat);
                            }

                            break;
                        case ComputationMethod.Custom:
                            if (ComputeTransform == null)
                                throw new MarkerException("For ComputationMethod.Custom method, you must set " +
                                    "the ComputeTransform property");

                            transforms = new List<Matrix>();
                            weights = new List<float>();

                            foreach (int id in markerInfo.PatternInfos.Keys)
                            {
                                PatternInfo patternInfo = markerInfo.PatternInfos[id];
                                if (patternInfo.IsFound)
                                {
                                    Matrix transform = markerInfo.RelativeTransforms[id];
                                    Matrix.Multiply(ref transform, ref patternInfo.Transform, out transform);
                                    transforms.Add(transform);
                                    weights.Add(patternInfo.Confidence);
                                }
                            }

                            ComputeTransform(transforms, weights, out resultMat);

                            break;
                    }

                    markerInfo.Transform = resultMat;
                }
            }
        }

        private void ParseArray(string arrayName, ref MarkerInfo markerInfo)
        {
            XElement markerArray = XElement.Load(@"" + arrayName);

            int pattWidth = 0, pattHeight = 0;
            float pattSize = 0, conf = 0;
            String pattName = "";
            Vector3 upperLeftCorner = Vector3.Zero;
            string[] tmp = null;

            foreach (XElement markerElement in markerArray.Elements("marker"))
            {
                try
                {
                    pattName = markerElement.Attribute("patternName").Value;
                    pattWidth = int.Parse(markerElement.Attribute("patternWidth").Value);
                    pattHeight = int.Parse(markerElement.Attribute("patternHeight").Value);
                    pattSize = float.Parse(markerElement.Attribute("patternSize").Value);
                    conf = float.Parse(markerElement.Attribute("confidence").Value);
                    tmp = markerElement.Attribute("upperLeftCorner").Value.Split(',');
                    upperLeftCorner.X = -float.Parse(tmp[0]);
                    upperLeftCorner.Y = float.Parse(tmp[1]);
                }
                catch (Exception exp)
                {
                    throw new MarkerException("Wrong marker array format: " + exp.Message);
                }

                NyARCode code = new NyARCode(pattWidth, pattHeight);
                code.loadARPatt(new System.IO.StreamReader(TitleContainer.OpenStream(pattName)));
                codes.Add(code);
                pattSizes.Add(pattSize);

                PatternInfo info = new PatternInfo();
                info.ConfidenceThreshold = conf;

                int id = codes.Count - 1;
                markerInfo.PatternInfos.Add(id, info);
                markerInfo.RelativeTransforms.Add(id, Matrix.CreateTranslation(upperLeftCorner));

                markerInfoMap.Add(id, markerInfo);
            }
        }

        private Matrix GetProjectionMatrix(NyARParam i_arparam, float near, float far)
        {
            NyARMat trans_mat = new NyARMat(3, 4);
            NyARMat icpara_mat = new NyARMat(3, 4);
            double[,] p = new double[3, 3], q = new double[4, 4];
            int width, height;
            int i, j;

            NyARIntSize size = i_arparam.getScreenSize();
            width = size.w;
            height = size.h;

            i_arparam.getPerspectiveProjectionMatrix().decompMat(icpara_mat, trans_mat);

            double[][] icpara = icpara_mat.getArray();
            double[][] trans = trans_mat.getArray();

            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    p[i, j] = icpara[i][j] / icpara[2][2];
                }
            }

            q[0, 0] = (2.0 * p[0, 0] / (width));
            q[0, 1] = (2.0 * p[0, 1] / (width));
            q[0, 2] = ((2.0 * p[0, 2] / (width)) - 1.0);
            q[0, 3] = 0.0;

            q[1, 0] = 0.0;
            q[1, 1] = (2.0 * p[1, 1] / (height));
            q[1, 2] = ((2.0 * p[1, 2] / (height)) - 1.0);
            q[1, 3] = 0.0;

            q[2, 0] = 0.0;
            q[2, 1] = 0.0;
            q[2, 2] = (far + near) / (far - near);
            q[2, 3] = -2.0 * far * near / (far - near);

            q[3, 0] = 0.0;
            q[3, 1] = 0.0;
            q[3, 2] = 1.0;
            q[3, 3] = 0.0;

            Matrix mat = Matrix.Identity;
            mat.M11 = (float)(q[0, 0] * trans[0][0] + q[0, 1] * trans[1][0] + q[0, 2] * trans[2][0]);
            mat.M12 = (float)(q[1, 0] * trans[0][0] + q[1, 1] * trans[1][0] + q[1, 2] * trans[2][0]);
            mat.M13 = (float)(q[2, 0] * trans[0][0] + q[2, 1] * trans[1][0] + q[2, 2] * trans[2][0]);
            mat.M14 = (float)(q[3, 0] * trans[0][0] + q[3, 1] * trans[1][0] + q[3, 2] * trans[2][0]);
            mat.M21 = (float)(q[0, 1] * trans[0][1] + q[0, 1] * trans[1][1] + q[0, 2] * trans[2][1]);
            mat.M22 = (float)(q[1, 1] * trans[0][1] + q[1, 1] * trans[1][1] + q[1, 2] * trans[2][1]);
            mat.M23 = (float)(q[2, 1] * trans[0][1] + q[2, 1] * trans[1][1] + q[2, 2] * trans[2][1]);
            mat.M24 = (float)(q[3, 1] * trans[0][1] + q[3, 1] * trans[1][1] + q[3, 2] * trans[2][1]);
            mat.M31 = (float)(q[0, 2] * trans[0][2] + q[0, 1] * trans[1][2] + q[0, 2] * trans[2][2]);
            mat.M32 = (float)(q[1, 2] * trans[0][2] + q[1, 1] * trans[1][2] + q[1, 2] * trans[2][2]);
            mat.M33 = -(float)(q[2, 2] * trans[0][2] + q[2, 1] * trans[1][2] + q[2, 2] * trans[2][2]);
            mat.M34 = -(float)(q[3, 2] * trans[0][2] + q[3, 1] * trans[1][2] + q[3, 2] * trans[2][2]);
            mat.M41 = (float)(q[0, 3] * trans[0][3] + q[0, 1] * trans[1][3] + q[0, 2] * trans[2][3] + q[0, 3]);
            mat.M42 = (float)(q[1, 3] * trans[0][3] + q[1, 1] * trans[1][3] + q[1, 2] * trans[2][3] + q[1, 3]);
            mat.M43 = (float)(q[2, 3] * trans[0][3] + q[2, 1] * trans[1][3] + q[2, 2] * trans[2][3] + q[2, 3]);
            mat.M44 = (float)(q[3, 3] * trans[0][3] + q[3, 1] * trans[1][3] + q[3, 2] * trans[2][3] + q[3, 3]);

            return mat;
        }

        #endregion

        #region Private Class

        private class MarkerInfo
        {
            public Dictionary<int, PatternInfo> PatternInfos;
            public Dictionary<int, Matrix> RelativeTransforms;
            public ComputationMethod Method;
            public bool IsFound;
            public Matrix Transform;

            public MarkerInfo()
            {
                PatternInfos = new Dictionary<int, PatternInfo>();
                RelativeTransforms = new Dictionary<int, Matrix>();
                IsFound = false;
                Transform = Matrix.Identity;
            }
        }

        private class PatternInfo
        {
            public float ConfidenceThreshold;
            public Matrix Transform;
            public bool IsFound;
            public float Confidence;

            public PatternInfo()
            {
                Transform = Matrix.Identity;
                IsFound = false;
            }
        }

        private class DsBGRX32Raster : NyARRgbRaster
        {
            public DsBGRX32Raster(int i_width, int i_height)
                : base(i_width, i_height, NyARBufferType.BYTE1D_B8G8R8X8_32)
            {
            }

#if WINDOWS
            public void SetBuffer(IntPtr buf)
            {
                Marshal.Copy(buf, (byte[])this._buf, 0, ((byte[])this._buf).Length);
            }
#else
            public void SetBuffer(byte[] buf)
            {
                this._buf = buf;
            }
#endif
        }

        #endregion
    }
}
