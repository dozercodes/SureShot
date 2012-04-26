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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GoblinXNA.Device.Capture.PointGrey
{
    #region Structs
    public struct RGBQUAD
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
    }

    public struct BITMAPINFOHEADER
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }

    public struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        public RGBQUAD bmiColors;
    }
    #endregion

    internal unsafe class PGRFlyDllBridge
    {
        #region DLL Imports
        //
        // DLL Functions to import
        // 
        // Follow this format to import any DLL with a specific function.
        //

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int flycaptureCreateContext(int* flycapcontext);

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int flycaptureStart(int flycapcontext,
            PGRFlyModule.FlyCaptureVideoMode videoMode,
            PGRFlyModule.FlyCaptureFrameRate frameRate);

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string flycaptureErrorToString(int error);

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int flycaptureInitialize(int flycapContext,
            int cameraIndex);

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int flycaptureGetCameraInformation(int flycapContext,
            ref PGRFlyModule.FlyCaptureInfo arInfo);

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern int flycaptureGrabImage2(int flycapContext,
            ref PGRFlyModule.FlyCaptureImage image);

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern int flycaptureSaveImage(int flycapContext,
            ref PGRFlyModule.FlyCaptureImage image, string filename,
            PGRFlyModule.FlyCaptureImageFileFormat fileFormat);

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int flycaptureStop(int flycapContext);

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int flycaptureDestroyContext(int flycapContext);

        [DllImport("pgrflycapture.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int flycaptureConvertImage(int flycapContext,
            ref PGRFlyModule.FlyCaptureImage image, ref PGRFlyModule.FlyCaptureImage imageConvert);
        #endregion
    }
}
