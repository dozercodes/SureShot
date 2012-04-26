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
 * Authors: Ohan Oda (ohan@cs.columbia.edu) 
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Xna.Framework;

namespace GoblinXNA.Device.Vision
{
    /// <summary>
    /// A DLL bridge class that accesses the APIs defined in ALVARWrapper.dll, which contains
    /// wrapped methods from the original ALVAR marker & feature tracking library.
    /// </summary>
    public class ALVARDllBridge
    {
        #region Dll Imports

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_init", CallingConvention = CallingConvention.Cdecl)]
        public static extern void alvar_init();

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_add_camera", CallingConvention = CallingConvention.Cdecl)]
        public static extern int alvar_add_camera(
            string calibFile, 
            int width, 
            int height);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_get_camera_projection", CallingConvention = CallingConvention.Cdecl)]
        private static extern void alvar_get_camera_projection(
            string calibFile, 
            int width, 
            int height,
            float farClip, 
            float nearClip,
            [Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] double[] projMatrix);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_get_camera_params", CallingConvention = CallingConvention.Cdecl)]
        public static extern int alvar_get_camera_params(
            int camID,
            [Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] double[] projMatrix,
            ref double fovX, 
            ref double fovY, 
            float farClip, 
            float nearClip);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_add_marker_detector", CallingConvention = CallingConvention.Cdecl)]
        public static extern int alvar_add_marker_detector(
            double markerSize, 
            int markerRes,
            double margin);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_set_marker_size", CallingConvention = CallingConvention.Cdecl)]
        public static extern int alvar_set_marker_size(
            int detectorID, 
            int markerID, 
            double markerSize);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_add_multi_marker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void alvar_add_multi_marker(String filename);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_detect_marker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void alvar_detect_marker(
            int detectorID, 
            int camID,
            int numChannels, 
            string colorModel,
            string channelSeq, 
            IntPtr imageData, 
            [In, Out] IntPtr interestedMarkerIDs,
            ref int numFoundMarkers, 
            ref int numInterestedMarkers, 
            double maxMarkerError, 
            double maxTrackError);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_get_poses", CallingConvention = CallingConvention.Cdecl)]
        public static extern void alvar_get_poses(
            int detectorID,
            [Out] IntPtr ids,
            [Out] IntPtr projMatrix);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_get_multi_marker_poses", CallingConvention = CallingConvention.Cdecl)]
        public static extern void alvar_get_multi_marker_poses(
            int detectorID,
            int camID,
            bool detectAdditional,
            [Out] IntPtr ids,
            [Out] IntPtr projMatrix,
            [Out] IntPtr errors);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_calibrate_camera", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool alvar_calibrate_camera(
            int camID, 
            int numChannels, 
            string colorModel,
            string channelSeq, 
            IntPtr imageData, 
            double etalon_square_size, 
            int etalon_rows,
            int etalon_columns);

        [DllImport("ALVARWrapper.dll", EntryPoint = "alvar_finalize_calibration", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool alvar_finalize_calibration(
            int camID, 
            string calibrationFilename);

        #endregion

        #region Static Helpers

        public static Matrix GetCameraProjection(string calibFilename, int width, int height, float nearClipPlane,
            float farClipPlane)
        {
            double[] projMat = new double[16];

            alvar_get_camera_projection(calibFilename, width, height, farClipPlane, nearClipPlane, projMat);

            return new Matrix(
                (float)projMat[0], (float)projMat[1], (float)projMat[2], (float)projMat[3],
                (float)projMat[4], (float)projMat[5], (float)projMat[6], (float)projMat[7],
                (float)projMat[8], (float)projMat[9], (float)projMat[10], (float)projMat[11],
                (float)projMat[12], (float)projMat[13], (float)projMat[14], (float)projMat[15]);
        }

        #endregion
    }
}
