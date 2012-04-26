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
using System.Runtime.InteropServices;
using System.Text;

namespace GoblinXNA.Device.iWear
{
    /// <summary>
    /// A DLL bridge to access the VR920s/iWear stereo and head tracking driver.
    /// </summary>
    public class iWearDllBridge
    {
        #region Structs

        /// <summary>
        /// A struct that contains the magnetic sensor information from Wrap 920 tracker.
        /// 
        /// magx_msb,magx_lsb – can be combined into a single 16-bit 2’ compliment number with a range 
        /// of -2048 – 2047 for the magnetic sensor in the x-direction.
        /// magy_msb,magy_lsb – can be combined into a single 16-bit 2’ compliment number with a range 
        /// of -2048 – 2047 for the magnetic sensor in the y-direction.
        /// magz_msb,magz_lsb – can be combined into a single 16-bit 2’ compliment number with a range 
        /// of -2048 – 2047 for the magnetic sensor in the z-direction.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct IWRMagSensor
        {
            [MarshalAs(UnmanagedType.U1)]
            public byte magx_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte magx_msb;
            [MarshalAs(UnmanagedType.U1)]
            public byte magy_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte magy_msb;
            [MarshalAs(UnmanagedType.U1)]
            public byte magz_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte magz_msb;
        }

        /// <summary>
        /// A struct that contains the accelerometer sensor information from Wrap 920 tracker.
        /// 
        /// accx_msb,accx_lsb – can be combined into a single 16-bit 2’ compliment number with a 
        /// range of -2048 – 2047 for the accelerometer sensor in the x-direction.
        /// accy_msb,accy_lsb – can be combined into a single 16-bit 2’ compliment number with a 
        /// range of -2048 – 2047 for the accelerometer sensor in the y-direction.
        /// accz_msb,accz_lsb – can be combined into a single 16-bit 2’ compliment number with a 
        /// range of -2048 – 2047 for the accelerometer sensor in the z-direction.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct IWRAccelSensor
        {
            [MarshalAs(UnmanagedType.U1)]
            public byte accx_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte accx_msb;
            [MarshalAs(UnmanagedType.U1)]
            public byte accy_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte accy_msb;
            [MarshalAs(UnmanagedType.U1)]
            public byte accz_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte accz_msb;
        }

        /// <summary>
        /// A struct that contains the gyro sensor information from Wrap 920 tracker.
        /// This struct contains high bandwidth gyros with 2000 degress per second.
        /// 
        /// gyx_msb,gyx_lsb – can be combined into a single 16-bit 2’ compliment number with a 
        /// range of -2048 – 2047 for the gyroscope sensor in the x-direction. 
        /// gyy_msb,gyy_lsb – can be combined into a single 16-bit 2’ compliment number with a 
        /// range of -2048 – 2047 for the gyroscope sensor in the y-direction.
        /// gyz_msb,gyz_lsb – can be combined into a single 16-bit 2’ compliment number with a 
        /// range of -2048 – 2047 for the gyroscope sensor in the z-direction.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct IWRGyroSensor
        {
            [MarshalAs(UnmanagedType.U1)]
            public byte gyx_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyx_msb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyy_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyy_msb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyz_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyz_msb;
        }

        /// <summary>
        /// A struct that contains the gyro sensor information from Wrap 920 tracker.
        /// This struct contains low bandwidth gyros with 500 degress per second.
        /// 
        /// gyx_msb,gyx_lsb – can be combined into a single 16-bit 2’ compliment number with a 
        /// range of -2048 – 2047 for the gyroscope sensor in the x-direction. 
        /// gyy_msb,gyy_lsb – can be combined into a single 16-bit 2’ compliment number with a 
        /// range of -2048 – 2047 for the gyroscope sensor in the y-direction.
        /// gyz_msb,gyz_lsb – can be combined into a single 16-bit 2’ compliment number with a 
        /// range of -2048 – 2047 for the gyroscope sensor in the z-direction.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct IWRLBGyroSensor
        {
            [MarshalAs(UnmanagedType.U1)]
            public byte gyx_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyx_msb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyy_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyy_msb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyz_lsb;
            [MarshalAs(UnmanagedType.U1)]
            public byte gyz_msb;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IWRSensorData
        {
            public IWRMagSensor mag_sensor;
            public IWRAccelSensor acc_sensor;
            public IWRGyroSensor gyro_sensor;
            public IWRLBGyroSensor lbgyro_sensor;
        }

        #endregion

        #region Enums

        public enum Eyes : int { LEFT_EYE = 0, RIGHT_EYE = 1, MONO_EYES = 2 }
        public enum IWRError : int { IWR_OK = 0 }
        /// <summary>
        /// iWear tracker product IDs
        /// </summary>
        public enum IWRProductID : int 
        { 
            /// <summary>
            /// Indicates an unsupported product
            /// </summary>
            IWR_PROD_NONE = 0,
            /// <summary>
            /// iWear VR920
            /// </summary>
            IWR_PROD_VR920 = 227, 
            /// <summary>
            /// Wrap920AR
            /// </summary>
            IWR_PROD_WRAP920 = 329 
        }

        #endregion

        #region DLL Imports

        // iWear Tracking.
        [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWROpenTracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern long IWROpenTracker();

        [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRCloseTracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void IWRCloseTracker();

        [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRGetTracking", CallingConvention = CallingConvention.Cdecl)]
        public static extern long IWRGetTracking(ref int yaw, ref int pitch, ref int roll);

        [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRGetSensorData", CallingConvention = CallingConvention.Cdecl)]
        public static extern long IWRGetSensorData(ref IWRSensorData sensorData);

        [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRSetFilterState", CallingConvention = CallingConvention.Cdecl)]
        public static extern void IWRSetFilterState(Boolean on);

        [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRGetProductID", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort IWRGetProductID();

        // iWear Stereoscopy.
        [DllImport("iWrstDrv.dll", SetLastError = false, EntryPoint = "IWRSTEREO_Open", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IWROpenStereo();

        [DllImport("iWrstDrv.dll", SetLastError = false, EntryPoint = "IWRSTEREO_Close", CallingConvention = CallingConvention.Cdecl)]
        public static extern void IWRCloseStereo(IntPtr handle);

        [DllImport("iWrstDrv.dll", SetLastError = false, EntryPoint = "IWRSTEREO_SetLR", CallingConvention = CallingConvention.Cdecl)]
        public static extern Boolean IWRSetStereoLR(IntPtr handle, int eye);

        [DllImport("iWrstDrv.dll", SetLastError = false, EntryPoint = "IWRSTEREO_SetStereo", CallingConvention = CallingConvention.Cdecl)]
        public static extern Boolean IWRSetStereoEnabled(IntPtr handle, Boolean enabled);

        [DllImport("iWrstDrv.dll", SetLastError = false, EntryPoint = "IWRSTEREO_WaitForAck", CallingConvention = CallingConvention.Cdecl)]
        public static extern Byte IWRWaitForOpenFrame(IntPtr handle, Boolean eye);

        #endregion
    }
}
