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
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace MarkerLayout
{
    public class ImageReader
    {
        #region Enums

        public enum ImageFormat
        {
            PGM,
            PPM,
            JPEG,
            BMP,
            GIF,
            TIFF,
            PNG
        }

        public enum ResizeOption
        {
            NearestNeighbor,
            Linear,
            Bicubic
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Loads an image, and the format is automatically determined by the extension of the file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static byte[] Load(String filename, ref int width, ref int height)
        {
            String ext = Path.GetExtension(filename);
            ImageFormat format = ImageFormat.JPEG;

            if (ext.Equals(".pgm"))
                format = ImageFormat.PGM;
            else if (ext.Equals(".ppm"))
                format = ImageFormat.PPM;
            else if (ext.Equals(".jpg") || ext.Equals(".jpeg"))
                format = ImageFormat.JPEG;
            else if (ext.Equals(".bmp"))
                format = ImageFormat.BMP;
            else if (ext.Equals(".gif"))
                format = ImageFormat.GIF;
            else if (ext.Equals(".tif"))
                format = ImageFormat.TIFF;
            else if (ext.Equals(".png"))
                format = ImageFormat.PNG;
            else
                throw new ArgumentException("The image format: " + ext + " is not supported");

            return Load(filename, format, ref width, ref height);
        }

        /// <summary>
        /// Loads an image with the given format.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static byte[] Load(String filename, ImageFormat format, ref int width, ref int height)
        {
            byte[] image = null;

            switch (format)
            {
                case ImageFormat.PGM:
                    image = ReadPGM(filename, ref width, ref height);
                    break;
                case ImageFormat.PPM:
                    image = ReadPPM(filename, ref width, ref height);
                    break;
                case ImageFormat.JPEG:
                case ImageFormat.GIF:
                case ImageFormat.BMP:
                case ImageFormat.TIFF:
                    image = ReadBitmap(filename, ref width, ref height);
                    break;
                case ImageFormat.PNG:
                    image = ReadPNG(filename, ref width, ref height);
                    break;
            }

            return image;
        }

        /// <summary>
        /// Crops the given image to an area specified by 'cropArea'.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="origSize"></param>
        /// <param name="cropArea"></param>
        /// <returns></returns>
        public static byte[] Crop(byte[] image, Size origSize, Rectangle cropArea)
        {
            byte[] cropImage = new byte[cropArea.Width * cropArea.Height];

            for (int i = 0; i < cropArea.Height; i++)
                for (int j = 0; j < cropArea.Width; j++)
                    cropImage[i * cropArea.Width + j] =
                        image[(i + cropArea.Y) * origSize.Width + (j + cropArea.X)];

            return cropImage;
        }

        /// <summary>
        /// Resizes the given image to 'newSize' with the given resize option (interpolation method).
        /// </summary>
        /// <param name="image"></param>
        /// <param name="origSize"></param>
        /// <param name="newSize"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static byte[] Resize(byte[] image, Size origSize, Size newSize, ResizeOption option)
        {
            byte[] resizeImage = new byte[newSize.Width * newSize.Height];

            float percentW = (float)newSize.Width / origSize.Width;
            float percentH = (float)newSize.Height / origSize.Height;

            //if (percent >= 1)
            //    throw new ArgumentException("Enlarging operation not supported yet");

            float invPercentW = 1 / percentW;
            float invPercentH = 1 / percentH;

            switch (option)
            {
                case ResizeOption.NearestNeighbor:
                    int ri = 0, rj = 0;
                    for (int i = 0; i < newSize.Height; i++)
                    {
                        for (int j = 0; j < newSize.Width; j++)
                        {
                            ri = (int)(i * invPercentH);
                            rj = (int)(j * invPercentW);
                            if (ri >= origSize.Height)
                                ri = origSize.Height - 1;
                            if (rj >= origSize.Width)
                                rj = origSize.Width - 1;

                            resizeImage[i * newSize.Width + j] = image[ri * origSize.Width + rj];
                        }
                    }
                    break;
                case ResizeOption.Linear:
                case ResizeOption.Bicubic:
                    throw new ArgumentException("Linear and Bicubic options not supported yet");
                    break;
            }

            return resizeImage;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Reads in an image in PGM format.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static byte[] ReadPGM(String filename, ref int width, ref int height)
        {
            char c;
            char [] comment = new char[256];
            byte [] image = null;
            byte uc;

            try
            {
                FileStream fs = File.OpenRead(filename);
                if (!fs.CanRead)
                    throw new ArgumentException("File " + filename + " does not exist or can not be read");

                BinaryReader br = new BinaryReader(fs);

                c = br.ReadChar();
                if (c != 'P')
                    throw new ArgumentException(filename + " is not a PGM file.");
                c = br.ReadChar();
                if (c != '5')
                    throw new ArgumentException(filename + " is not a PGM file.");
                c = br.ReadChar();
                if (c != '\n')
                    throw new ArgumentException(filename + " is not a PGM file.");

                while (true)
                {
                    comment = fgets(br, 256);
                    if ((comment[0] != '#') && (comment[0] != 0x0a) && (comment[0] != 0x0d)) 
                        break;
                }

                char[] seps = { ' ', '\n' };
                String[] parts = (new String(comment)).Split(seps);
                width = int.Parse(parts[0]);
                height = int.Parse(parts[1]);

                comment = fgets(br, 256);
                parts = (new String(comment)).Split(seps);
                int max_grey = int.Parse(parts[0]);

                int size = width * height;
                image = new byte[size];

                for (int i = 0; i < size; i++)
                {
                    uc = br.ReadByte();
                    if (max_grey != 255)
                        uc = (byte)((int)uc * 256 / (max_grey + 1));
                    image[i] = uc;
                }

                br.Close();
                fs.Close();
            }
            catch (Exception exp)
            {
                System.Console.WriteLine(exp.Message);
            }

            return image;
        }

        /// <summary>
        /// Reads in an image in PPM format, and returns a grayscaled image.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static byte[] ReadPPM(String filename, ref int width, ref int height)
        {
            char c;
            char[] comment = new char[256];
            byte[] image = null;
            byte r, g, b;

            try
            {
                FileStream fs = File.OpenRead(filename);
                if (!fs.CanRead)
                    throw new ArgumentException("File " + filename + " does not exist or can not be read");

                BinaryReader br = new BinaryReader(fs);

                c = br.ReadChar();
                if (c != 'P')
                    throw new ArgumentException(filename + " is not a PPM file.");
                c = br.ReadChar();
                if (c != '6')
                    throw new ArgumentException(filename + " is not a PPM file.");
                c = br.ReadChar();
                if (c != '\n')
                    throw new ArgumentException(filename + " is not a PPM file.");

                while (true)
                {
                    comment = fgets(br, 256);
                    if ((comment[0] != '#') && (comment[0] != 0x0a) && (comment[0] != 0x0d))
                        break;
                }

                char[] seps = { ' ', '\n' };
                String[] parts = (new String(comment)).Split(seps);
                width = int.Parse(parts[0]);
                height = int.Parse(parts[1]);

                comment = fgets(br, 256);
                parts = (new String(comment)).Split(seps);
                int max_grey = int.Parse(parts[0]);

                int size = width * height;
                image = new byte[size];

                for (int i = 0; i < size; i++)
                {
                    r = br.ReadByte();
                    if (max_grey != 255)
                        r = (byte)((int)r * 256 / (max_grey + 1));
                    g = br.ReadByte();
                    if (max_grey != 255)
                        g = (byte)((int)r * 256 / (max_grey + 1));
                    b = br.ReadByte();
                    if (max_grey != 255)
                        b = (byte)((int)r * 256 / (max_grey + 1));

                    image[i] = (byte)(r * 0.3f + g * 0.59f + b * 0.11f);
                }

                br.Close();
                fs.Close();
            }
            catch (Exception exp)
            {
                System.Console.WriteLine(exp.Message);
            }

            return image;
        }

        /// <summary>
        /// Reads in an image with any format that Image.FromFile(...) function supports, and returns a
        /// grayscaled image.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static byte[] ReadBitmap(String filename, ref int width, ref int height)
        {
            byte[] image = null;

            Bitmap bitmap = (Bitmap)Image.FromFile(filename);
            width = bitmap.Width;
            height = bitmap.Height;
            image = new byte[width * height];

            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            int bpp = bmpData.Stride / width;
            if(bpp == 1)
                Marshal.Copy(bmpData.Scan0, image, 0, image.Length);
            else if(bpp >= 3)
            {
                unsafe
                {
                    byte* src = (byte*)bmpData.Scan0;
                    byte r, g, b;
                    for (int i = 0; i < height * bmpData.Stride; i += bpp)
                    {
                        r = *(src + i);
                        g = *(src + i + 1);
                        b = *(src + i + 2);

                        image[i / bpp] = (byte)(r * 0.3f + g * 0.59f + b * 0.11f);
                    }
                }
            }
            else
                throw new FormatException("Wrong image format for " + filename);

            bitmap.UnlockBits(bmpData);

            return image;
        }

        private static byte[] ReadPNG(String filename, ref int width, ref int height)
        {
            Stream imageStreamSource = new FileStream(filename, FileMode.Open, FileAccess.Read, 
                FileShare.Read);
            PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, 
                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource bitmapSource = decoder.Frames[0];

            width = bitmapSource.PixelWidth;
            height = bitmapSource.PixelHeight;

            byte[] image = new byte[width * height];

            int bpp = bitmapSource.Format.BitsPerPixel / 8;
            bitmapSource.CopyPixels(image, width * bpp, 0);

            return image;
        }

        /// <summary>
        /// Gets an array of chars upto 'count' or until it encounters '\n'. This function
        /// imitates fgets(...) function in C library.
        /// </summary>
        /// <param name="br"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static char[] fgets(BinaryReader br, int count)
        {
            int index = 0;
            char[] data = new char[256];
            while (index < count)
            {
                data[index] = br.ReadChar();
                if (data[index] == '\n')
                    break;

                index++;
            }

            return data;
        }

        #endregion
    }
}
