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
// $Id: PGRFlyCaptureTestCSharpModule.cs,v 1.2 2007/06/15 00:00:51 demos Exp $
//=============================================================================
//
// Modified by: Ohan Oda (ohan@cs.columbia.edu)
//
//==========================================================================

using System.Runtime.InteropServices;

namespace GoblinXNA.Device.Capture.PointGrey
{
    /// <summary>
    /// A collection of Enums used for the PGRFlyCapture class.
    /// </summary>
    public class PGRFlyModule
    {
        /// <summary>
        /// Enum describing different framerates. 
        /// (adapted from PGRFlyCapture.h)
        /// </summary>
        public enum FlyCaptureFrameRate
        {
            /// <summary>
            /// 1.875 fps. (Frames per second)
            /// </summary>
            FLYCAPTURE_FRAMERATE_1_875,
            /// <summary>
            /// 3.75 fps.
            /// </summary>
            FLYCAPTURE_FRAMERATE_3_75,
            /// <summary>
            /// 7.5 fps.
            /// </summary>
            FLYCAPTURE_FRAMERATE_7_5,
            /// <summary>
            /// 15 fps.
            /// </summary>
            FLYCAPTURE_FRAMERATE_15,
            /// <summary>
            /// 30 fps.
            /// </summary>
            FLYCAPTURE_FRAMERATE_30,
            /// <summary>
            /// Deprecated.  Please use Custom image.
            /// </summary>
            FLYCAPTURE_FRAMERATE_50,
            /// <summary>
            /// 60 fps.
            /// </summary>
            FLYCAPTURE_FRAMERATE_60,
            /// <summary>
            /// 120 fps.
            /// </summary>
            FLYCAPTURE_FRAMERATE_120,
            /// <summary>
            /// 240 fps.
            /// </summary>
            FLYCAPTURE_FRAMERATE_240,
            /// <summary>
            /// Number of possible camera frame rates.
            /// </summary>
            FLYCAPTURE_NUM_FRAMERATES,
            /// <summary>
            /// Custom frame rate.  Used with custom image size functionality.
            /// </summary>
            FLYCAPTURE_FRAMERATE_CUSTOM,
            /// <summary>
            /// Hook for "any usable frame rate."
            /// </summary>
            FLYCAPTURE_FRAMERATE_ANY,
        }

        /// <summary>
        /// Enum describing different video modes.
        /// </summary>
        /// <remarks>
        /// The explicit numbering is to provide downward compatibility for this enum.
        /// (adapted from PGRFlyCapture.h)
        /// </remarks>
        public enum FlyCaptureVideoMode
        {
            /// <summary>
            /// 160x120 YUV444.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_160x120YUV444 = 0,
            /// <summary>
            /// 320x240 YUV422.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_320x240YUV422 = 1,
            /// <summary>
            /// 640x480 YUV411.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_640x480YUV411 = 2,
            /// <summary>
            /// 640x480 YUV422.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_640x480YUV422 = 3,
            /// <summary>
            /// 640x480 24-bit RGB.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_640x480RGB = 4,
            /// <summary>
            /// 640x480 8-bit greyscale or bayer tiled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_640x480Y8 = 5,
            /// <summary>
            /// 640x480 16-bit greyscale or bayer tiled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_640x480Y16 = 6,
            /// <summary>
            /// 800x600 YUV422.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_800x600YUV422 = 17,
            /// <summary>
            /// 800x600 RGB.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_800x600RGB = 18,
            /// <summary>
            /// 800x600 8-bit greyscale or bayer tiled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_800x600Y8 = 7,
            /// <summary>
            /// 800x600 16-bit greyscale or bayer tiled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_800x600Y16 = 19,
            /// <summary>
            /// 1024x768 YUV422.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1024x768YUV422 = 20,
            /// <summary>
            /// 1024x768 RGB.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1024x768RGB = 21,
            /// <summary>
            /// 1024x768 8-bit greyscale or bayer tiled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1024x768Y8 = 8,
            /// <summary>
            /// 1024x768 16-bit greyscale or bayer tiled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1024x768Y16 = 9,
            /// <summary>
            /// 1280x960 YUV422.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1280x960YUV422 = 22,
            /// <summary>
            /// 1280x960 RGB.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1280x960RGB = 23,
            /// <summary>
            /// 1280x960 8-bit greyscale or bayer titled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1280x960Y8 = 10,
            /// <summary>
            /// 1280x960 16-bit greyscale or bayer titled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1280x960Y16 = 24,
            /// <summary>
            /// 1600x1200 YUV422.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1600x1200YUV422 = 50,
            /// <summary>
            /// 1600x1200 RGB.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1600x1200RGB = 51,
            /// <summary>
            /// 1600x1200 8-bit greyscale or bayer titled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1600x1200Y8 = 11,
            /// <summary>
            /// 1600x1200 16-bit greyscale or bayer titled color image.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_1600x1200Y16 = 52,

            /// <summary>
            /// Custom video mode.  Used with custom image size functionality.
            /// </summary>
            FLYCAPTURE_VIDEOMODE_CUSTOM = 15,
            /// <summary>
            /// Hook for "any usable video mode."
            /// </summary>
            FLYCAPTURE_VIDEOMODE_ANY = 16,

            /// <summary>
            /// Number of possible video modes.
            /// </summary>
            FLYCAPTURE_NUM_VIDEOMODES = 23,
        }

        /// <summary>
        /// An enumeration used to describe the different camera models that can be 
        /// accessed through this SDK.
        /// (adapted from PGRFlyCapture.h)
        /// </summary>
        public enum FlyCaptureCameraModel
        {
            FLYCAPTURE_FIREFLY,
            FLYCAPTURE_DRAGONFLY,
            FLYCAPTURE_AIM,
            FLYCAPTURE_SCORPION,
            FLYCAPTURE_TYPHOON,
            FLYCAPTURE_FLEA,
            FLYCAPTURE_DRAGONFLY_EXPRESS,
            FLYCAPTURE_FLEA2,
            FLYCAPTURE_FIREFLY_MV,
            FLYCAPTURE_DRAGONFLY2,
            FLYCAPTURE_BUMBLEBEE,
            FLYCAPTURE_BUMBLEBEE2,
            FLYCAPTURE_BUMBLEBEEXB3,
            FLYCAPTURE_GRASSHOPPER,
            //FLYCAPTURE_CHAMELEON,
            FLYCAPTURE_UNKNOWN = -1,
        }

        /// <summary>
        /// An enumeration used to describe the different camera color configurations.
        /// (adapted from PGRFlyCapture.h)
        /// </summary>
        public enum FlyCaptureCameraType
        {
            /// <summary>
            /// black and white system.
            /// </summary>
            FLYCAPTURE_BLACK_AND_WHITE,
            /// <summary>
            /// color system.
            /// </summary>
            FLYCAPTURE_COLOR
        }

        /// <summary>
        /// Enumerates the image file formats that flycaptureSaveImage() can write to.
        ///  (adapted from PGRFlyCapture.h)
        /// </summary>
        public enum FlyCaptureImageFileFormat
        {
            /// <summary>
            /// Single channel (8 or 16 bit) greyscale portable grey map.
            /// </summary>
            FLYCAPTURE_FILEFORMAT_PGM,
            /// <summary>
            /// 3 channel RGB portable pixel map.
            /// </summary>
            FLYCAPTURE_FILEFORMAT_PPM,
            /// <summary>
            /// 3 or 4 channel RGB windows bitmap.
            /// </summary>
            FLYCAPTURE_FILEFORMAT_BMP,
            /// <summary>
            /// JPEG format.  Not implemented.
            /// </summary>
            FLYCAPTURE_FILEFORMAT_JPG,
            /// <summary>
            /// Portable Network Graphics format.  Not implemented.
            /// </summary>
            FLYCAPTURE_FILEFORMAT_PNG,
            /// <summary>
            /// Raw data output.
            /// </summary>
            FLYCAPTURE_FILEFORMAT_RAW
        }

        /// <summary>
        /// Camera information structure.  This structure will eventually be replaced
        /// by FlyCaptureInfoEx.
        /// (adapted from PGRFlyCapture.h)
        /// </summary>
        public struct FlyCaptureInfo
        {
            /// <summary>
            /// camera serial number.
            /// </summary>
            public int SerialNumber;
            /// <summary>
            /// type of CCD (color or b&w).
            /// </summary>
            public FlyCaptureCameraType CameraType;
            /// <summary>
            /// Camera model.
            /// </summary>
            public FlyCaptureCameraModel CameraModel;
            /// <summary>
            /// Null-terminated camera model string for attached camera.
            /// public string pszModelString;
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string pszModelString;
        }

        /// <summary>
        /// An enumeration used to indicate the type of the returned pixels.  This
        /// enumeration is used as a member of FlyCaptureImage and as a parameter
        /// to FlyCaptureStartCustomImage.
        /// (adapted from PGRFlyCapture.h)
        /// </summary>
        public enum FlyCapturePixelFormat
        {
            /// <summary>
            /// 8 bits of mono information.
            /// </summary>
            FLYCAPTURE_MONO8 = 0x00000001,
            /// <summary>
            /// YUV 4:1:1.
            /// </summary>
            FLYCAPTURE_411YUV8 = 0x00000002,
            /// <summary>
            /// YUV 4:2:2.
            /// </summary>
            FLYCAPTURE_422YUV8 = 0x00000004,
            /// <summary>
            /// YUV 4:4:4.
            /// </summary>
            FLYCAPTURE_444YUV8 = 0x00000008,
            /// <summary>
            /// R = G = B = 8 bits.
            /// </summary>
            FLYCAPTURE_RGB8 = 0x00000010,
            /// <summary>
            /// 16 bits of mono information.
            /// </summary>
            FLYCAPTURE_MONO16 = 0x00000020,
            /// <summary>
            /// R = G = B = 16 bits.
            /// </summary>
            FLYCAPTURE_RGB16 = 0x00000040,
            /// <summary>
            /// 16 bits of signed mono information.
            /// </summary>
            FLYCAPTURE_S_MONO16 = 0x00000080,
            /// <summary>
            /// R = G = B = 16 bits signed.
            /// </summary>
            FLYCAPTURE_S_RGB16 = 0x00000100,
            /// <summary>
            /// 8 bit raw data output of sensor.
            /// </summary>
            FLYCAPTURE_RAW8 = 0x00000200,
            /// <summary>
            /// 16 bit raw data output of sensor.
            /// </summary>
            FLYCAPTURE_RAW16 = 0x00000400,
            /// <summary>
            /// 24 bit BGR
            /// </summary>
            FLYCAPTURE_BGR = 0x10000001,
            /// <summary>
            /// 32 bit BGRU
            /// </summary>
            FLYCAPTURE_BGRU = 0x10000002,
            /// <summary>
            /// Unused member to force this enum to compile to 32 bits.
            /// </summary>
            FCPF_FORCE_QUADLET = 0x7FFFFFFF,
        }

        /// <summary>
        /// This structure defines the format in which time is represented in the 
        /// PGRFlycapture SDK.  The ulSeconds and ulMicroSeconds values represent the
        /// absolute system time when the image was captured.  The ulCycleSeconds
        /// and ulCycleCount are higher-precision values that have either been 
        /// propagated up from the 1394 bus or extracted from the image itself.  The 
        /// data will be extracted from the image if image timestamping is enabled and
        /// directly (and less accurately) from the 1394 bus otherwise.
        ///
        /// The ulCycleSeconds value will wrap around after 128 seconds.  The ulCycleCount 
        /// represents the 1/8000 second component. Use these two values when synchronizing 
        /// grabs between two computers sharing a common 1394 bus that may not have 
        /// precisely synchronized system timers.
        /// (adapted from PGRFlyCapture.h)
        /// </summary>
        unsafe public struct FlyCaptureTimestamp
        {
            /// <summary>
            /// The number of seconds since the epoch. 
            /// </summary>
            public uint ulSeconds;
            /// <summary>
            /// The microseconds component.
            /// </summary>
            public uint ulMicroSeconds;
            /// <summary>
            /// The cycle time seconds.  0-127.
            /// </summary>
            public uint ulCycleSeconds;
            /// <summary>
            /// The cycle time count.  0-7999. (1/8000ths of a second.)
            /// </summary>
            public uint ulCycleCount;
            /// <summary>
            /// The cycle offset.  0-3071 (1/3072ths of a cycle count.)
            /// </summary>
            public uint ulCycleOffset;
        }

        /// <summary>
        /// This structure is used to pass image information into and out of the API.
        /// </summary>
        /// <remarks>
        /// The size of the image buffer is iRowInc * iRows, and depends on the
        /// pixel format.
        /// (adapted from PGRFlyCapture.h)
        /// </remarks>
        unsafe public struct FlyCaptureImage
        {
            /// <summary>
            /// Rows, in pixels, of the image.
            /// </summary>
            public int iRows;
            /// <summary>
            /// Columns, in pixels, of the image.
            /// </summary>
            public int iCols;
            /// <summary>
            /// Row increment.  The number of bytes per row.
            /// </summary>
            public int iRowInc;
            /// <summary>
            /// Video mode that this image was captured with.  Only populated when the
            /// image is returned from a grab call.
            /// </summary>
            public int videoMode;
            /// <summary>
            /// Timestamp of this image.
            /// </summary>
            public FlyCaptureTimestamp timeStamp;
            /// <summary>
            /// Pointer to the actual image data.
            /// </summary>
            public byte* pData;

            /// <summary>
            /// If the returned image is Y8 or Y16, this flag indicates whether it is
            /// a greyscale or stippled (bayer tiled) image.  In modes other than Y8
            /// or Y16, this flag has no meaning.
            /// </summary>
            public bool bStippled;
            /// <summary>
            /// The pixel format of this image.
            /// </summary>
            public FlyCapturePixelFormat pixelFormat;
            /// <summary>
            /// The number of images that make up the data.  
            /// Used for stereo cameras where images are interleaved.
            /// </summary>
            public uint uiNumImages;
            /// <summary>
            /// Reserved for future use.
            /// [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray,
            ///		 SizeConst=6)]
            /// </summary>
            public long ulReserved;
        }
    }
}