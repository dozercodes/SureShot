using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace GoblinXNA.Device.Vision
{
    public partial class OpenCVWrapper
    {
        //private const String CV_DLL_LIB = "libcv200.dll";
        //private const String CV_DLL_LIB = "opencv_imgproc220.dll";
        private const String CV_DLL_LIB = "cv210.dll";

        #region Color conversion code

        public static int CV_BGR2BGRA    = 0;
        public static int CV_RGB2RGBA    = CV_BGR2BGRA;

        public static int CV_BGRA2BGR    = 1;
        public static int CV_RGBA2RGB    = CV_BGRA2BGR;

        public static int CV_BGR2RGBA    = 2;
        public static int CV_RGB2BGRA    = CV_BGR2RGBA;

        public static int CV_RGBA2BGR    = 3;
        public static int CV_BGRA2RGB    = CV_RGBA2BGR;

        public static int CV_BGR2RGB     = 4;
        public static int CV_RGB2BGR     = CV_BGR2RGB;

        public static int CV_BGRA2RGBA   = 5;
        public static int CV_RGBA2BGRA   = CV_BGRA2RGBA;

        public static int CV_BGR2GRAY    = 6;
        public static int CV_RGB2GRAY    = 7;
        public static int CV_GRAY2BGR    = 8;
        public static int CV_GRAY2RGB    = CV_GRAY2BGR;
        public static int CV_GRAY2BGRA   = 9;
        public static int CV_GRAY2RGBA   = CV_GRAY2BGRA;
        public static int CV_BGRA2GRAY   = 10;
        public static int CV_RGBA2GRAY   = 11;

        public static int CV_BGR2BGR565  = 12;
        public static int CV_RGB2BGR565  = 13;
        public static int CV_BGR5652BGR  = 14;
        public static int CV_BGR5652RGB  = 15;
        public static int CV_BGRA2BGR565 = 16;
        public static int CV_RGBA2BGR565 = 17;
        public static int CV_BGR5652BGRA = 18;
        public static int CV_BGR5652RGBA = 19;

        public static int CV_GRAY2BGR565 = 20;
        public static int CV_BGR5652GRAY = 21;

        public static int CV_BGR2BGR555  = 22;
        public static int CV_RGB2BGR555  = 23;
        public static int CV_BGR5552BGR  = 24;
        public static int CV_BGR5552RGB  = 25;
        public static int CV_BGRA2BGR555 = 26;
        public static int CV_RGBA2BGR555 = 27;
        public static int CV_BGR5552BGRA = 28;
        public static int CV_BGR5552RGBA = 29;

        public static int CV_GRAY2BGR555 = 30;
        public static int CV_BGR5552GRAY = 31;

        public static int CV_BGR2XYZ     = 32;
        public static int CV_RGB2XYZ     = 33;
        public static int CV_XYZ2BGR     = 34;
        public static int CV_XYZ2RGB     = 35;

        public static int CV_BGR2YCrCb   = 36;
        public static int CV_RGB2YCrCb   = 37;
        public static int CV_YCrCb2BGR   = 38;
        public static int CV_YCrCb2RGB   = 39;

        public static int CV_BGR2HSV     = 40;
        public static int CV_RGB2HSV     = 41;

        public static int CV_BGR2Lab     = 44;
        public static int CV_RGB2Lab     = 45;

        public static int CV_BayerBG2BGR = 46;
        public static int CV_BayerGB2BGR = 47;
        public static int CV_BayerRG2BGR = 48;
        public static int CV_BayerGR2BGR = 49;

        public static int CV_BayerBG2RGB = CV_BayerRG2BGR;
        public static int CV_BayerGB2RGB = CV_BayerGR2BGR;
        public static int CV_BayerRG2RGB = CV_BayerBG2BGR;
        public static int CV_BayerGR2RGB = CV_BayerGB2BGR;

        public static int CV_BGR2Luv     = 50;
        public static int CV_RGB2Luv     = 51;
        public static int CV_BGR2HLS     = 52;
        public static int CV_RGB2HLS     = 53;

        public static int CV_HSV2BGR     = 54;
        public static int CV_HSV2RGB     = 55;

        public static int CV_Lab2BGR     = 56;
        public static int CV_Lab2RGB     = 57;
        public static int CV_Luv2BGR     = 58;
        public static int CV_Luv2RGB     = 59;
        public static int CV_HLS2BGR     = 60;
        public static int CV_HLS2RGB     = 61;

        #endregion

        #region Interpolation Contants

        public static int CV_INTER_NN     = 0;
        public static int CV_INTER_LINEAR = 1;
        public static int CV_INTER_CUBIC  = 2;
        public static int CV_INTER_AREA   = 3;

        #endregion

        #region Smoothing Types

        public static int CV_BLUR_NO_SCALE = 0;
        public static int CV_BLUR          = 1;
        public static int CV_GAUSSIAN      = 2;
        public static int CV_MEDIAN        = 3;
        public static int CV_BILATERAL     = 4;

        #endregion

        #region Depth Values

        public static int IPL_DEPTH_SIGN = 0x8000000;

        public static int IPL_DEPTH_1U   = 1;
        public static int IPL_DEPTH_8U   = 8;
        public static int IPL_DEPTH_16U  = 16;
        public static int IPL_DEPTH_32F  = 32;

        public static int IPL_DEPTH_8S   = (IPL_DEPTH_SIGN | 8);
        public static int IPL_DEPTH_16S  = (IPL_DEPTH_SIGN | 16);
        public static int IPL_DEPTH_32S  = (IPL_DEPTH_SIGN | 32);

        public static int IPL_DEPTH_64F = 64;

        #endregion

        #region Shapes
        public static int CV_SHAPE_RECT = 0;
        public static int CV_SHAPE_CROSS = 1;
        public static int CV_SHAPE_ELLIPSE = 2;
        public static int CV_SHAPE_CUSTOM = 100;
        #endregion

        #region CvTermCriteria

        public static int CV_TERMCRIT_ITER = 1;
        public static int CV_TERMCRIT_NUMBER = CV_TERMCRIT_ITER;
        public static int CV_TERMCRIT_EPS = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct CvTermCriteria
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Type;

            [MarshalAs(UnmanagedType.I4)]
            public int Max_iter;

            [MarshalAs(UnmanagedType.R8)]
            public double Epsilon;

            public CvTermCriteria(int type, int max_iter, double epsilon)
            {
                Type = type;
                Max_iter = max_iter;
                Epsilon = epsilon;
            }
        }

        #endregion

        [DllImport(CV_DLL_LIB, EntryPoint = "cvCvtColor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvCvtColor(IntPtr image, IntPtr outputImage, int code);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvResize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvResize(IntPtr src, IntPtr dst, int interpolation);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvSobel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvSobel(IntPtr src, IntPtr dst, int xorder, int yorder, int apertureSize);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvLaplace", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvLaplace(IntPtr src, IntPtr dst, int apertureSize);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvSmooth", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvSmooth(IntPtr src, IntPtr dst, int smoothType, int size1, int size2,
            double sigma1, double sigma2);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvPyrDown", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvPyrDown(IntPtr src, IntPtr dst, int filter);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvLaplace", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvPyrUp(IntPtr src, IntPtr dst, int filter);

        /// <summary>
        /// Implements the Canny algorithm for edge detection.
        /// </summary>
        /// <remarks>
        /// The function finds the edges on the input image image and marks them in the output image edges 
        /// using the Canny algorithm. The smallest value between threshold1 and threshold2 is used for edge 
        /// linking, the largest value is used to find the initial segments of strong edges.
        /// </remarks>
        /// <param name="image">Single-channel input image</param>
        /// <param name="edges">Single-channel image to store the edges found by the function</param>
        /// <param name="threshold1"></param>
        /// <param name="threshold2"></param>
        /// <param name="apetureSize">Aperture parameter for the Sobel operator</param>
        [DllImport(CV_DLL_LIB, EntryPoint = "cvCanny", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvCanny(IntPtr image, IntPtr edges, double threshold1, 
            double threshold2, int apetureSize);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvFindContours", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cvFindContours(IntPtr img, IntPtr storage, IntPtr firstContour, int cntHeaderSize,
            int mode, int method, IntPtr offset);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvErode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvErode(IntPtr src, IntPtr dst, IntPtr element, int iterations);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvDilate", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvDilate(IntPtr src, IntPtr dst, IntPtr element, int iterations);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvCreateStructuringElementEx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvCreateStructuringElementEx(int cols, int rows, int anchor_x, int anchor_y, int shape, IntPtr values);

        /// <summary>
        /// Finds a sparse set of points within the selected region that seem to be easy to track
        /// </summary>
        /// <param name="image"></param>
        /// <param name="eigImage"></param>
        /// <param name="tempImage"></param>
        /// <param name="corners"></param>
        /// <param name="cornerCount"></param>
        /// <param name="qualityLevel"></param>
        /// <param name="minDistance"></param>
        /// <param name="mask">Default is NULL</param>
        /// <param name="blockSize">Default is 3</param>
        /// <param name="useHarris">Default is 0</param>
        /// <param name="k">Default is 0.04</param>
        [DllImport(CV_DLL_LIB, EntryPoint = "cvGoodFeaturesToTrack", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvGoodFeaturesToTrack(IntPtr image, IntPtr eigImage, IntPtr tempImage,
            IntPtr corners, ref int cornerCount, double qualityLevel, double minDistance, IntPtr mask,
            int blockSize, int useHarris, double k);

        [DllImport(CV_DLL_LIB, EntryPoint = "cvCalcOpticalFlowPyrLK", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvCalcOpticalFlowPyrLK(IntPtr prev, IntPtr curr, IntPtr prevPyr,
            IntPtr currPyr, IntPtr prevFeatures, IntPtr currFeatures, int count, CvSize winSize, 
            int level, IntPtr status, IntPtr trackError, CvTermCriteria criteria, int flags);
    }
}
