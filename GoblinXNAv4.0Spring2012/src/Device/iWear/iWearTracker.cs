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
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;

namespace GoblinXNA.Device.iWear
{
    /// <summary>
    /// A 6DOF input device class that supports VUZIX's iWear VS920 device which is a stereoscopic display
    /// with embedded orientation tracker. 
    /// </summary>
    public class iWearTracker : InputDevice_6DOF
    {
        #region Member Fields

        private String identifier;
        private bool isAvailable;
        private bool stereoAvailable;
        private bool trackerAvailable;
        private bool sensorAvailable;
        private Quaternion rotation;
        private float yaw;
        private float pitch;
        private float roll;
        private Vector3 magneticData;
        private Vector3 accelerationData;
        private Vector3 gyroData;
        private Vector3 lbGyroData;

        private OcclusionQuery g_QueryGPU;
        private IntPtr stereoHandle;
        private int windowBottomLine;

        private int iwr_status;
        private int y = 0, p = 0, r = 0;
        private iWearDllBridge.IWRSensorData sensorData;
        private iWearDllBridge.IWRProductID productID;

        private static iWearTracker tracker;

        private bool isHiDef;

        #endregion

        #region Constructor

        /// <summary>
        /// A private constructor.
        /// </summary>
        private iWearTracker()
        {
            identifier = "iWearTracker";
            isAvailable = false;
            stereoAvailable = false;
            trackerAvailable = false;
            sensorAvailable = false;
            productID = iWearDllBridge.IWRProductID.IWR_PROD_NONE;
            rotation = Quaternion.Identity;
            yaw = 0;
            pitch = 0;
            roll = 0;
            magneticData = Vector3.Zero;
            accelerationData = Vector3.Zero;
            gyroData = Vector3.Zero;
            lbGyroData = Vector3.Zero;
            sensorData = new iWearDllBridge.IWRSensorData();
            isHiDef = (State.Device.GraphicsProfile == GraphicsProfile.HiDef);

            stereoHandle = ((IntPtr)(-1));
            // Setup a query, to provide GPU syncing method.
            if(isHiDef)
                g_QueryGPU = new OcclusionQuery(State.Device);
            windowBottomLine = 0;
        }

        #endregion

        #region Public Properties

        public String Identifier
        {
            get { return identifier; }
            set { identifier = value; }
        }

        /// <summary>
        /// Gets the product ID of the connected iWear device. 
        /// </summary>
        public iWearDllBridge.IWRProductID ProductID
        {
            get { return productID; }
        }

        public bool IsAvailable
        {
            get { return isAvailable; }
        }

        /// <summary>
        /// Gets whether the stereoscopic view is available.
        /// </summary>
        public bool IsStereoAvailable
        {
            get { return stereoAvailable; }
        }

        /// <summary>
        /// Gets whether the orientation tracker is available.
        /// </summary>
        public bool IsTrackerAvailable
        {
            get { return trackerAvailable; }
        }

        /// <summary>
        /// Gets whether the sensors (e.g., magnetic, accelerometer, and gyros) are available.
        /// </summary>
        public bool IsSensorAvailable
        {
            get { return sensorAvailable; }
        }

        /// <summary>
        /// Gets the yaw (in radians) updated by the device's orientation tracker. 
        /// </summary>
        public float Yaw
        {
            get { return yaw; }
        }

        /// <summary>
        /// Gets the pitch (in radians) updated by the device's orientation tracker. 
        /// </summary>
        public float Pitch
        {
            get { return pitch; }
        }

        /// <summary>
        /// Gets the roll (in radians) updated by the device's orientation tracker. 
        /// </summary>
        public float Roll
        {
            get { return roll; }
        }

        /// <summary>
        /// Gets the raw sensor data (e.g., magnetic, acceleration, and gyro).
        /// </summary>
        /// <remarks>
        /// The data is valid only if IsSensorAvailable is true.
        /// </remarks>
        /// <see cref="IsSensorAvailable"/>
        public iWearDllBridge.IWRSensorData SensorData
        {
            get { return sensorData; }
        }

        /// <summary>
        /// Gets the magnetic sensor data in the x, y, and z directions. The values range from
        /// -2048 to 2048.
        /// </summary>
        /// <remarks>
        /// The data is valid only if IsSensorAvailable is true.
        /// </remarks>
        /// <see cref="IsSensorAvailable"/>
        public Vector3 MagneticData
        {
            get { return magneticData; }
        }

        /// <summary>
        /// Gets the accelometer sensor data in the x, y, and z directions. The values range from
        /// -2048 to 2048.
        /// </summary>
        /// <remarks>
        /// The data is valid only if IsSensorAvailable is true.
        /// </remarks>
        /// <see cref="IsSensorAvailable"/>
        public Vector3 AccelerationData
        {
            get { return accelerationData; }
        }

        /// <summary>
        /// Gets the gyro sensor data in the x, y, and z directions. The values range from
        /// -2048 to 2048.
        /// </summary>
        /// <remarks>
        /// The data is valid only if IsSensorAvailable is true.
        /// </remarks>
        /// <see cref="IsSensorAvailable"/>
        public Vector3 GyroData
        {
            get { return gyroData; }
        }

        /// <summary>
        /// Gets the low bandwidth gyro sensor data in the x, y, and z directions. The values range from
        /// -2048 to 2048.
        /// </summary>
        /// <remarks>
        /// The data is valid only if IsSensorAvailable is true.
        /// </remarks>
        /// <see cref="IsSensorAvailable"/>
        public Vector3 LowBandGyroData
        {
            get 
            {
                if (sensorAvailable)
                {
                    lbGyroData.X = (short)(((ushort)sensorData.lbgyro_sensor.gyx_msb << 8) |
                        ((ushort)sensorData.lbgyro_sensor.gyx_lsb));
                    lbGyroData.Y = (short)(((ushort)sensorData.lbgyro_sensor.gyy_msb << 8) |
                        ((ushort)sensorData.lbgyro_sensor.gyy_lsb));
                    lbGyroData.Z = (short)(((ushort)sensorData.lbgyro_sensor.gyz_msb << 8) |
                        ((ushort)sensorData.lbgyro_sensor.gyz_lsb));
                }
                return lbGyroData; 
            }
        }

        /// <summary>
        /// Sets whether to enable internal filtering by the tracker. The internal filtering is
        /// disabled by default.
        /// </summary>
        public bool EnableFiltering
        {
            set
            {
                try
                {
                    iWearDllBridge.IWRSetFilterState(value);
                }
                catch
                {
                    Log.Write("Filtering is not available. Could be pre 2.4 driver install.", 
                        Log.LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Sets whether to enable stereoscopic view.
        /// </summary>
        public bool EnableStereo
        {
            set
            {
                if (stereoAvailable)
                {
                    iWearDllBridge.IWRSetStereoEnabled(stereoHandle, value);
                }
                else if(value)
                {
                    // Acquire stereoscopic handle
                    stereoHandle = iWearDllBridge.IWROpenStereo();
                    if (stereoHandle == ((IntPtr)(-1)))
                    {
                        Log.Write("Unable to obtain stereo handle. Please ensure your iWear is connected, and " +
                            "that your firmware supports stereoscopy.", Log.LogLevel.Error);
                    }
                    else
                    {
                        stereoAvailable = true;
                        iWearDllBridge.IWRSetStereoEnabled(stereoHandle, true);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the rotation updated by the device's orientation tracker. 
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            }
        }

        public Matrix WorldTransformation
        {
            get
            {
                return Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            }
        }

        /// <summary>
        /// Gets the instance of iWearTracker.
        /// </summary>
        public static iWearTracker Instance
        {
            get
            {
                if (tracker == null)
                    tracker = new iWearTracker();

                return tracker;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the iWear tracker.
        /// </summary>
        public void Initialize()
        {
            try
            {
                // Acquire tracking interface
                iWearDllBridge.IWROpenTracker();
                isAvailable = true;

                productID = (iWearDllBridge.IWRProductID)iWearDllBridge.IWRGetProductID();
            }
            catch (Exception)
            {
                Log.Write("Unable to open iWear Drivers...Check VR920 Driver installation.", Log.LogLevel.Error);
            }
        }

        /// <summary>
        /// Begin GPU query. This method must be called before rendering any 3D object (any information that
        /// will be passed to the GPU for rendering) and before calling EndGPUQuery() method.
        /// </summary>
        public void BeginGPUQuery()
        {
            if (productID != iWearDllBridge.IWRProductID.IWR_PROD_VR920)
                return;

            if (stereoAvailable && isHiDef)
                g_QueryGPU.Begin();
        }

        /// <summary>
        /// End GPU query. This method must be called after rendering any 3D object (any information that
        /// will be passed to the GPU for rendering) and after calling BeginGPUQuery() method.
        /// </summary>
        public void EndGPUQuery()
        {
            if (productID != iWearDllBridge.IWRProductID.IWR_PROD_VR920)
                return;

            if (stereoAvailable && isHiDef)
                g_QueryGPU.End();
        }

        /// <summary>
        /// Synchronize.
        /// </summary>
        /// <param name="eye"></param>
        /// <returns></returns>
        public bool SynchronizeEye(iWearDllBridge.Eyes eye)
        {
            if (!stereoAvailable || (productID != iWearDllBridge.IWRProductID.IWR_PROD_VR920))
                return false;

            iWearDllBridge.IWRWaitForOpenFrame(stereoHandle, false);
            // In windowed mode, we must poll for vSync.
            //if (!State.Graphics.IsFullScreen)
            //    while (State.Device.RasterStatus.ScanLine < windowBottomLine) ;

            if (isHiDef)
            {
                while (!g_QueryGPU.IsComplete)
                {
                    // Waiting on gpu to complete rendering.
                    // MUST be certain the frame will scan out on the next vSync interval.
                }
            }

            if (eye == iWearDllBridge.Eyes.LEFT_EYE)
                State.Device.Present();

            return iWearDllBridge.IWRSetStereoLR(stereoHandle, (int)eye);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        public void UpdateBottomLine(Game game)
        {
            if (productID != iWearDllBridge.IWRProductID.IWR_PROD_VR920)
                return;

            if (!((GraphicsDeviceManager)State.Graphics).IsFullScreen)
            {
                // In windowed mode we need the bottom line of our window.
                // MUST Never be greater than the displays last scanline.
                windowBottomLine = game.Window.ClientBounds.Bottom;
                if (windowBottomLine >= State.Device.DisplayMode.Height)
                    windowBottomLine = State.Device.DisplayMode.Height - 1;
            }
        }

        public void Update(TimeSpan elapsedTime, bool deviceActive)
        {
            // Get iWear tracking yaw, pitch, roll
            iwr_status = (int)iWearDllBridge.IWRGetTracking(ref y, ref p, ref r);
            if (iwr_status == (int)iWearDllBridge.IWRError.IWR_OK)
            {
                yaw = ConvertToRadians(y);
                pitch = ConvertToRadians(p);
                roll = ConvertToRadians(r);
                trackerAvailable = true;
            }
            else
                trackerAvailable = false;

            if (productID == iWearDllBridge.IWRProductID.IWR_PROD_WRAP920)
            {
                iwr_status = (int)iWearDllBridge.IWRGetSensorData(ref sensorData);

                if (iwr_status == (int)iWearDllBridge.IWRError.IWR_OK)
                {
                    magneticData.X = (short)(((ushort)sensorData.mag_sensor.magx_msb << 8) |
                        ((ushort)sensorData.mag_sensor.magx_lsb));
                    magneticData.Y = (short)(((ushort)sensorData.mag_sensor.magy_msb << 8) |
                        ((ushort)sensorData.mag_sensor.magy_lsb));
                    magneticData.Z = (short)(((ushort)sensorData.mag_sensor.magz_msb << 8) |
                        ((ushort)sensorData.mag_sensor.magz_lsb));

                    accelerationData.X = (short)(((ushort)sensorData.acc_sensor.accx_msb << 8) |
                        ((ushort)sensorData.acc_sensor.accx_lsb));
                    accelerationData.Y = (short)(((ushort)sensorData.acc_sensor.accy_msb << 8) |
                        ((ushort)sensorData.acc_sensor.accy_lsb));
                    accelerationData.Z = (short)(((ushort)sensorData.acc_sensor.accz_msb << 8) |
                        ((ushort)sensorData.acc_sensor.accz_lsb));

                    gyroData.X = (short)(((ushort)sensorData.gyro_sensor.gyx_msb << 8) |
                        ((ushort)sensorData.gyro_sensor.gyx_lsb));
                    gyroData.Y = (short)(((ushort)sensorData.gyro_sensor.gyy_msb << 8) |
                        ((ushort)sensorData.gyro_sensor.gyy_lsb));
                    gyroData.Z = (short)(((ushort)sensorData.gyro_sensor.gyz_msb << 8) |
                        ((ushort)sensorData.gyro_sensor.gyz_lsb));

                    sensorAvailable = true;
                }
                else
                    sensorAvailable = false;
            }
        }

        public void Dispose()
        {
            if (stereoHandle != ((IntPtr)(-1)))
                iWearDllBridge.IWRCloseStereo(stereoHandle);
            iWearDllBridge.IWRCloseTracker();
            if(isHiDef)
                g_QueryGPU.Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Convert Raw values from the iWear Tracker to radians.
        /// </summary>
        private float ConvertToRadians(long value)
        {
            return (float)value * MathHelper.Pi / 32768.0f;
        }

        #endregion
    }
}
