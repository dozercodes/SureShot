using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace GoblinXNA.Device.Vision
{
    public partial class OpenCVWrapper
    {
        //private const String CXCORE_DLL_LIB = "libcxcore200.dll";
        //private const String CXCORE_DLL_LIB = "opencv_core220.dll";
        private const String CXCORE_DLL_LIB = "cxcore210.dll";

        #region Fourier Transform Constants

        public static int CV_DXT_FORWARD = 0;
        public static int CV_DXT_INVERSE = 1;
        public static int CV_DXT_SCALE = 2;
        /// <summary>
        /// divide result by size of array
        /// </summary>
        public static int CV_DXT_INV_SCALE = CV_DXT_INVERSE + CV_DXT_SCALE;
        public static int CV_DXT_INVERSE_SCALE = CV_DXT_INV_SCALE;
        /// <summary>
        /// transform each row individually
        /// </summary>
        public static int CV_DXT_ROWS = 4;
        /// <summary>
        /// conjugate the second argument of cvMulSpectrums
        /// </summary>
        public static int CV_DXT_MUL_CONJ = 8;

        #endregion

        #region Matrix Constants

        public static int CV_CN_MAX    = 64;
        public static int CV_CN_SHIFT  = 3;
        public static int CV_DEPTH_MAX = (1 << CV_CN_SHIFT);

        public static int CV_8U  = 0;
        public static int CV_8S  = 1;
        public static int CV_16U = 2;
        public static int CV_16S = 3;
        public static int CV_32S = 4;
        public static int CV_32F = 5;
        public static int CV_64F = 6;

        public static int CV_MAT_DEPTH_MASK = (CV_DEPTH_MAX - 1);

        public static int CV_MAT_DEPTH(int flags)
        {
            return ((flags) & CV_MAT_DEPTH_MASK);
        }

        public static int CV_MAKE_TYPE(int depth, int cn)
        {
            return (CV_MAT_DEPTH(depth) + (((cn)-1) << CV_CN_SHIFT));
        }

        #endregion

        #region Structure

        [StructLayout(LayoutKind.Sequential)]
        public struct CvPoint
        {
            [MarshalAs(UnmanagedType.I4)]
            public int X;

            [MarshalAs(UnmanagedType.I4)]
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CvPoint2D32f
        {
            [MarshalAs(UnmanagedType.R4)]
            public float X;

            [MarshalAs(UnmanagedType.R4)]
            public float Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CvPoint3D32f
        {
            [MarshalAs(UnmanagedType.R4)]
            public float X;

            [MarshalAs(UnmanagedType.R4)]
            public float Y;

            [MarshalAs(UnmanagedType.R4)]
            public float Z;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CvPoint2D64f
        {
            [MarshalAs(UnmanagedType.R8)]
            public double X;

            [MarshalAs(UnmanagedType.R8)]
            public double Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CvPoint3D64f
        {
            [MarshalAs(UnmanagedType.R8)]
            public double X;

            [MarshalAs(UnmanagedType.R8)]
            public double Y;

            [MarshalAs(UnmanagedType.R8)]
            public double Z;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CvRect
        {
            [MarshalAs(UnmanagedType.I4)]
            public int X;

            [MarshalAs(UnmanagedType.I4)]
            public int Y;

            [MarshalAs(UnmanagedType.I4)]
            public int Width;

            [MarshalAs(UnmanagedType.I4)]
            public int Height;

            public CvRect(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CvSize
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Width;

            [MarshalAs(UnmanagedType.I4)]
            public int Height;

            public CvSize(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IplROI
        {
            public int coi;
            public int height;
            public int width;
            public int xOffset;
            public int yOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IplConvKernel
        {
            public int nCols;
            public int nRows;
            public int anchorX;
            public int anchorY;
            public IntPtr values;
            public int nShiftR;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IplImage
        {
            /// <summary>
            /// sizeof(IplImage)
            /// </summary>
            public int nSize;

            /// <summary>
            /// Version, always equals 0
            /// </summary>
            public int ID;

            /// <summary>
            /// Number of color channels (1, 2, 3, 4)
            /// </summary>
            public int nChannels;

            /// <summary>
            /// Ignored by OpenCV
            /// </summary>
            public int alphaChannel;

            /// <summary>
            /// Pixel depth in bits:
            /// IPL_DEPTH_8U, IPL_DEPTH_8S, 
            /// IPL_DEPTH_16U,IPL_DEPTH_16S, 
            /// IPL_DEPTH_32S,IPL_DEPTH_32F, 
            /// IPL_DEPTH_64F
            /// </summary>
            public int depth;

            /// <summary>
            /// Color model - ignored by OpenCV
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public char[] colorModel; // size = 4

            /// <summary>
            /// Ignored by OpenCV
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public char[] channelSeq; // size = 4

            /// <summary>
            /// 0 - interleaved color channels,
            /// 1 - separate color channels
            /// cvCreateImage can only create interleaved images
            /// </summary>
            public int dataOrder;

            /// <summary>
            /// 0 - top-left origin,
            /// 1 - bottom-left origin (Windows bitmaps style)
            /// </summary>
            public int origin;

            /// <summary>
            /// Alignment of image rows: 4 or 8 byte alignment
            /// OpenCV ignores this and uses widthStep instead
            /// </summary>
            public int align;

            /// <summary>
            /// Image width in pixels
            /// </summary>
            public int width;

            /// <summary>
            /// Image height in pixels
            /// </summary>
            public int height;

            /// <summary>
            /// Image Region of Interest. When not IntPtr.Zero specifies image region to be processed
            /// </summary>
            public IntPtr roi;

            /// <summary>
            /// Must be NULL in OpenCV
            /// </summary>
            public IntPtr maskROI;

            /// <summary>
            /// Must be NULL in OpenCV
            /// </summary>
            public IntPtr imageId;

            /// <summary>
            /// Must be NULL in OpenCV
            /// </summary>
            public IntPtr titleInfo;

            /// <summary>
            /// Image data size in bytes = height * widthStep
            /// </summary>
            public int imageSize;

            /// <summary>
            /// Pointer to aligned image data. Note that color images are stored in RGB order
            /// </summary>
            public IntPtr imageData;

            /// <summary>
            /// Size of aligned image row in bytes
            /// </summary>
            public int widthStep;

            /// <summary>
            /// Border completion mode, ignored by OpenCV
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] BorderMode; // size = 4

            /// <summary>
            /// Border completion mode, ignored by OpenCV
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] BorderConst; // size = 4

            /// <summary>
            /// A pointer to the origin of the image data (not necessarily aligned). 
            /// This is used for image deallocation.
            /// </summary>
            public IntPtr imageDataOrigin;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CvMat
        {
            /// <summary>
            /// CvMat signature (CV_MAT_MAGIC_VAL), element type and flags
            /// </summary>
            public int type;

            /// <summary>
            /// full row length in bytes
            /// </summary>
            public int step;

            /// <summary>
            /// underlying data reference counter
            /// </summary>
            public IntPtr refcount;

            /// <summary>
            /// Header reference count
            /// </summary>
            public int hdr_refcount;

            /// <summary>
            /// data pointers
            /// </summary>
            public IntPtr data;

            /// <summary>
            /// number of rows
            /// </summary>
            public int rows;

            /// <summary>
            /// number of columns
            /// </summary>
            public int cols;

            /// <summary>
            /// Width
            /// </summary>
            public int width { get { return cols; } }

            /// <summary>
            /// Height
            /// </summary>
            public int height { get { return rows; } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CvScalar
        {
            /// <summary>
            /// The scalar value
            /// </summary>
            public double v0;
            /// <summary>
            /// The scalar value
            /// </summary>
            public double v1;
            /// <summary>
            /// The scalar value
            /// </summary>
            public double v2;
            /// <summary>
            /// The scalar value
            /// </summary>
            public double v3;

            /// <summary>
            /// The scalar values as a vector (of size 4)
            /// </summary>
            public double[] ToArray()
            {
                return new double[4] { v0, v1, v2, v3 };
            }

            /// <summary>
            /// Create a new MCvScalar structure using the specific values
            /// </summary>
            /// <param name="v0">v0</param>
            public CvScalar(double v0)
            {
                this.v0 = v0;
                v1 = 0;
                v2 = 0;
                v3 = 0;
            }

            /// <summary>
            /// Create a new MCvScalar structure using the specific values
            /// </summary>
            /// <param name="v0">v0</param>
            /// <param name="v1">v1</param>
            public CvScalar(double v0, double v1)
            {
                this.v0 = v0;
                this.v1 = v1;
                v2 = 0;
                v3 = 0;
            }

            /// <summary>
            /// Create a new MCvScalar structure using the specific values
            /// </summary>
            /// <param name="v0">v0</param>
            /// <param name="v1">v1</param>
            /// <param name="v2">v2</param>
            public CvScalar(double v0, double v1, double v2)
            {
                this.v0 = v0;
                this.v1 = v1;
                this.v2 = v2;
                v3 = 0;
            }

            /// <summary>
            /// Create a new MCvScalar structure using the specific values
            /// </summary>
            /// <param name="v0">v0</param>
            /// <param name="v1">v1</param>
            /// <param name="v2">v2</param>
            /// <param name="v3">v3</param>
            public CvScalar(double v0, double v1, double v2, double v3)
            {
                this.v0 = v0;
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }
        #endregion

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvCreateMat", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvCreateMat(int rows, int cols, int type);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvReleaseMat", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvReleaseMat(ref IntPtr mat);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvCreateMemStorage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvCreateMemStorage(int blockSize);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvCreateChildMemStorage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvCreateChildMemStorage(IntPtr parent);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvReleaseMemStorage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvReleaseMemStorage(ref IntPtr storage);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvClearMemStorage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvClearMemStorage(IntPtr storage);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvCreateImageHeader", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvCreateImageHeader(CvSize size, int depth, int channels);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvInitImageHeader", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvInitImageHeader(IntPtr image, CvSize size, int depth,
            int channels, int origin, int align);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvCreateImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvCreateImage(CvSize size, int depth, int channels);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvReleaseImageHeader", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvReleaseImageHeader(ref IntPtr image);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvReleaseImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvReleaseImage(ref IntPtr image);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvCloneImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvCloneImage(IntPtr image);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvSetImageCOI", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvSetImageCOI(IntPtr image, int coi);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvGetImageCOI", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cvGetImageCOI(IntPtr image);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvSetImageROI", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvSetImageROI(IntPtr image, CvRect rect);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvResetImageROI", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvResetImageROI(IntPtr image);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvGetImageROI", CallingConvention = CallingConvention.Cdecl)]
        public static extern CvRect cvGetImageROI(IntPtr image);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvGetSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern CvSize cvGetSize(IntPtr arr);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvGetElemType", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cvGetElemType(IntPtr arr);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="dst"></param>
        /// <param name="flags"></param>
        /// <param name="nonzeroRows">Default is 0</param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvDFT", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvDFT(IntPtr arr, IntPtr dst, int flags, int nonzeroRows);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvDCT", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvDCT(IntPtr arr, IntPtr dst, int flags);

        /// <summary>
        /// Performs linear transformation on every source array element:
        /// dst(x,y,c) = scale*src(x,y,c)+shift.
        /// Arbitrary combination of input and output array depths are allowed
        /// (number of channels must be the same), thus the function can be used
        /// for type conversion
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="dst"></param>
        /// <param name="scale">Default is 1</param>
        /// <param name="shift">Default is 0</param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvConvertScale", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvConvertScale(IntPtr arr, IntPtr dst, double scale, double shift);

        /// <summary>
        /// Copies source array to destination array
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="mask">Default is IntPtr.Zero</param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvCopy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvCopy(IntPtr src, IntPtr dst, IntPtr mask);

        /// <summary>
        /// Clears all the array elements (sets them to 0)
        /// </summary>
        /// <param name="arr"></param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvSetZero", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvSetZero(IntPtr arr);

        /// <summary>
        /// Splits a multi-channel array into the set of single-channel arrays or
        /// extracts particular [color] plane
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst0"></param>
        /// <param name="dst1"></param>
        /// <param name="dst2"></param>
        /// <param name="dst3"></param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvSplit", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvSplit(IntPtr src, IntPtr dst0, IntPtr dst1, 
            IntPtr dst2, IntPtr dst3);

        /// <summary>
        /// Merges a set of single-channel arrays into the single multi-channel array
        /// or inserts one particular [color] plane to the array
        /// </summary>
        /// <param name="src0"></param>
        /// <param name="src1"></param>
        /// <param name="src2"></param>
        /// <param name="src3"></param>
        /// <param name="dst"></param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvMerge", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvMerge(IntPtr src0, IntPtr src1, IntPtr src2,
            IntPtr src3, IntPtr dst);

        /// <summary>
        /// Finds optimal DFT vector size >= size0
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvGetOptimalDFTSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cvGetOptimalDFTSize(int size0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="submat">CvMat*</param>
        /// <param name="rect"></param>
        /// <returns>CvMat*</returns>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvGetSubRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvGetSubRect(IntPtr arr, IntPtr submat, CvRect rect);

        /// <summary>
        /// dst(mask) = src1(mask) + src2(mask)
        /// </summary>
        /// <param name="src1"></param>
        /// <param name="src2"></param>
        /// <param name="dst"></param>
        /// <param name="mask">Default is IntPtr.Zero</param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvAdd", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvAdd(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask);

        /// <summary>
        /// dst(mask) = src(mask) + value
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <param name="dst"></param>
        /// <param name="mask">Default is IntPtr.Zero</param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvAddS", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvAddS(IntPtr src, CvScalar value, IntPtr dst, IntPtr mask);

        /// <summary>
        /// dst(mask) = src1(mask) - src2(mask)
        /// </summary>
        /// <param name="src1"></param>
        /// <param name="src2"></param>
        /// <param name="dst"></param>
        /// <param name="mask">Default is IntPtr.Zero</param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvSub", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvSub(IntPtr src1, IntPtr src2, IntPtr dst, IntPtr mask);

        /// <summary>
        /// dst(mask) = src(mask) - value = src(mask) + (-value)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="value"></param>
        /// <param name="dst"></param>
        /// <param name="mask">Default is IntPtr.Zero</param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvSubS", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvSubS(IntPtr src, CvScalar value, IntPtr dst, IntPtr mask);

        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvLog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvLog(IntPtr src, IntPtr dst);

        /// <summary>
        /// Does powering: dst(idx) = src(idx)^power
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="power"></param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvPow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvPow(IntPtr src, IntPtr dst, double power);

        /// <summary>
        /// Finds global minimum, maximum and their positions
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <param name="minLoc">CvPoint*</param>
        /// <param name="maxLoc">CvPoint*</param>
        /// <param name="mask"></param>
        [DllImport(CXCORE_DLL_LIB, EntryPoint = "cvMinMaxLoc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvMinMaxLoc(IntPtr arr, ref double minVal, ref double maxVal,
            IntPtr minLoc, IntPtr maxLoc, IntPtr mask);

    }
}
