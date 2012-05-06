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
 * Author: Semih Energin (se2302@columbia.edu) 
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using GoblinXNA.Device.Capture;
using GoblinXNA.Helpers;
using Microsoft.Xna.Framework;
using nyartoolkit.four.core;
using nyartoolkit.four.markersystem;
using NyARException = jp.nyatla.nyartoolkit.cs.NyARException;

namespace GoblinXNA.Device.Vision.Marker
{
    /// <summary>
    /// A marker tracker implementation using NyARToolkit 4.0.1 for ID-based markers
    /// </summary>
    public class NyARToolkitIdTracker: IMarkerTracker
    {
        /// <summary>
        /// An enum that defines how the resulting transform should be computed from all visible markers.
        /// </summary>
        /// <remarks>
        /// For Custom method, you must set the ComputeTransform delegate property.
        /// </remarks>
        public enum ComputationMethod { Average }

        /// <summary>
        /// Computes the resulting transform based on the visible markers' transforms and their confidence values.
        /// </summary>
        /// <param name="transforms">The list of visible markers' transforms</param>
        /// <param name="confidences">The list of visible markers' confidence values</param>
        /// <param name="result">The computed transform</param>
        public delegate void ComputeMultiMarkeTransform(List<Matrix> transforms, List<float> confidences, out Matrix result);

        private NyARParam param;

        private Dictionary<int, MarkerInfo> markerInfoMap;
        private List<MarkerInfo> markerInfoList;
        private List<IdCode> codes;
        private List<double> pattSizes;
        private Object lastMarkerID;
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

        private NyARMarkerSystem markerSystem;
        private NyARIntSize nyARIntSize;
        private DsBGRX32Raster nyARRaster;
        private NyARSensor nyARSensor;

        private int previousFoundId;

        public NyARToolkitIdTracker()
        {
            configFilename = "";
            camProjMat = Matrix.Identity;

            initialized = false;
            markerInfoList = new List<MarkerInfo>();
            markerInfoMap = new Dictionary<int, MarkerInfo>();
            codes = new List<IdCode>();
            pattSizes = new List<double>();

            zNearPlane = 10;
            zFarPlane = 2000;

            EnableTracking = true;

            tmpMat1 = Matrix.Identity;
            tmpMat2 = Matrix.Identity;
        }

        public bool Initialized
        {
            get { return initialized; }
        }

        public Matrix CameraProjection
        {
            get { return camProjMat; }
        }

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

        public void InitTracker(params object[] configs)
        {
            if (!(configs.Length == 3 || configs.Length == 5))
            {
                throw new MarkerException("Problem in InitTracker in NewNyAR");
            }

            int imgWidth = 0;
            int imgHeight = 0;
            try
            {
                imgWidth = (int)configs[0];
                imgHeight = (int)configs[1];
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
                throw new MarkerException("Problem in InitTracker in NewNyAR");
            }

            nyARIntSize = new NyARIntSize(imgWidth, imgHeight);

            param = new NyARParam();
            param.loadARParam(TitleContainer.OpenStream(configFilename));
            param.changeScreenSize(nyARIntSize.w, nyARIntSize.h);

            camProjMat = GetProjectionMatrix(param, zNearPlane, zFarPlane);

            INyARMarkerSystemConfig arMarkerSystemConfig = new NyARMarkerSystemConfig(param);
            markerSystem = new NyARMarkerSystem(arMarkerSystemConfig);

            initialized = true;
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

        public object AssociateMarker(params object[] markerConfigs)
        {
            // make sure we are initialized
            if (!initialized)
                throw new MarkerException("NewNyAR is not initialized. Call InitTracker(...)");

            if (!(markerConfigs.Length == 2 || markerConfigs.Length == 5))
                throw new MarkerException("Problem in AssociateMarker in NewNyAR");

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
                catch (Exception exception)
                {
                
                    throw new MarkerException("Problem in AssociateMarker in NewNyAR");
                }

                ParseArray(arrayName, ref markerInfo);

                markerInfo.Method = method;
            }
            else
            {
                float pattSize = 0, conf = 0;
                String pattId = "";

                try
                {
                    pattId = (String)(markerConfigs[0]);
                    pattSize = (float)markerConfigs[3];
                    conf = (float)markerConfigs[4];
                }
                catch (Exception)
                {
                    throw new MarkerException("Problem in AssociateMarker in NewNyAR");
                }

                IdCode code = new IdCode();
                code.Id = int.Parse(pattId);
                code.Size = pattSize;
                code.MID = markerSystem.addNyIdMarker(code.Id, code.Size);
                
                codes.Add(code);
                pattSizes.Add(pattSize);

                PatternInfo info = new PatternInfo();
                info.ConfidenceThreshold = conf;

                markerInfo.PatternInfos.Add(code.MID, info);
                markerInfo.RelativeTransforms.Add(code.MID, Matrix.Identity);
                markerInfo.Method = ComputationMethod.Average;

                markerInfoMap.Add(code.MID, markerInfo);
            }
            
            markerInfoList.Add(markerInfo);
            
            /*
            // reinitialize the multi marker detector if the programmer adds new marker node
            // after the initialization phase
            if (started)
            {
                
            }
             * */
            return markerInfo;
        }

        private void ParseArray(string arrayName, ref MarkerInfo markerInfo)
        {
            XElement markerArray = XElement.Load(@"" + arrayName);

            float pattSize = 0;
            int pattId;
            Vector3 center = Vector3.Zero;
            string[] tmp = null;

            foreach (XElement markerElement in markerArray.Elements("marker"))
            {
                try
                {
                    pattId = int.Parse(markerElement.Attribute("patternId").Value);
                    pattSize = float.Parse(markerElement.Attribute("patternSize").Value);
                    tmp = markerElement.Attribute("center").Value.Split(',');
                    center.X = -float.Parse(tmp[0]);
                    center.Y = float.Parse(tmp[1]);
                }
                catch (Exception exp)
                {
                    throw new MarkerException("Wrong marker array format: " + exp.Message);
                }

                IdCode code = new IdCode();
                code.Id = pattId;
                code.Size = pattSize;
                code.MID = markerSystem.addNyIdMarker(code.Id, code.Size);
                
                codes.Add(code);
                pattSizes.Add(pattSize);

                PatternInfo info = new PatternInfo();

                markerInfo.PatternInfos.Add(code.MID, info);
                markerInfo.RelativeTransforms.Add(code.MID, Matrix.CreateTranslation(center));
                markerInfo.Method = ComputationMethod.Average;

                markerInfoMap.Add(code.MID, markerInfo);
            }
        }

        
#if WINDOWS
        public void ProcessImage(IVideoCapture captureDevice, IntPtr imagePtr)
        {
            if (captureDevice.Format != ImageFormat.B8G8R8A8_32)
                throw new MarkerException("Only ImageFormat.B8G8R8A8_32 format is acceptable for NyARToolkitTracker");

            // initialize the detector right before the image processing
            if (!started)
            {
                nyARSensor = new NyARSensor(nyARIntSize);

                nyARRaster = new DsBGRX32Raster(nyARIntSize.w, nyARIntSize.h);
                nyARSensor.update(nyARRaster);
                started = true;
            }
            
            nyARRaster.SetBuffer(imagePtr);

            nyARSensor.update(nyARRaster);
            nyARSensor.updateTimeStamp();

            markerSystem.update(nyARSensor);

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
                nyARSensor = new NyARSensor(nyARIntSize);

                nyARRaster = new DsBGRX32Raster(nyARIntSize.w, nyARIntSize.h);
                nyARSensor.update(nyARRaster);
                started = true;
            }

            nyARRaster.SetBuffer(imagePtr);
            nyARSensor.update(nyARRaster);
            nyARSensor.updateTimeStamp();
            
            markerSystem.update(nyARSensor);
            
            UpdateMarkerTransforms();
        }
#endif
        private void UpdateMarkerTransforms()
        {
            // Reset all marker patterns to not found
            foreach (MarkerInfo markerInfo in markerInfoList)
            {
                markerInfo.IsFound = false;
                foreach (PatternInfo patternInfo in markerInfo.PatternInfos.Values)
                    patternInfo.IsFound = false;
            }

            int numMarkerFound = 0;

            int newCandidate = -1;
            bool oldFound = false;

            for (int i = 0; i < codes.Count; i++)
            {
                IdCode idCode = codes[i];

                if (markerSystem.isExistMarker(idCode.MID))
                {
                    int mid = idCode.MID;
                    numMarkerFound++;
                    MarkerInfo markerInfo = markerInfoMap[mid];

                    try
                    {
                        markerInfo.IsFound = true;
                        markerInfo.PatternInfos[mid].IsFound = true;
                        //markerInfo.PatternInfos[mid].Confidence = (float)markerSystem.getConfidence(mid);
                        NyARDoubleMatrix44 resultMatrix = markerSystem.getMarkerMatrix(mid);
                        markerInfo.PatternInfos[mid].Transform = (new Matrix(
                            (float)resultMatrix.m00, (float)resultMatrix.m10, (float)resultMatrix.m20, (float)resultMatrix.m30,
                            (float)resultMatrix.m01, (float)resultMatrix.m11, (float)resultMatrix.m21, (float)resultMatrix.m31,
                            (float)resultMatrix.m02, (float)resultMatrix.m12, (float)resultMatrix.m22, (float)resultMatrix.m32,
                            (float)resultMatrix.m03, (float)resultMatrix.m13, (float)resultMatrix.m23, (float)resultMatrix.m33))
                            * Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.Pi);
                    }
                    catch (NyARException exp)
                    {
                        Log.Write(exp.Message);

                        markerInfo.PatternInfos[mid].IsFound = false;
                        markerInfo.PatternInfos[mid].Confidence = 0;
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
                    }

                    markerInfo.Transform = resultMat;
                }
            }
        }

        public bool FindMarker(object markerID)
        {
            lastMarkerID = markerID;

            return ((MarkerInfo)lastMarkerID).IsFound;
        }

        public Matrix GetMarkerTransform()
        {
            return ((MarkerInfo)lastMarkerID).Transform;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
            return;
        }

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

        private class IdCode
        {
            public int Id;
            public double Size;
            public int MID;
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
    }
}
