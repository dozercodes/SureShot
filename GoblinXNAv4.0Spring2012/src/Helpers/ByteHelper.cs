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
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.Helpers
{
    /// <summary>
    /// A helper class that implements various useful functions to convert between array of bytes
    /// and other types.
    /// </summary>
    public class ByteHelper
    {
        /// <summary>
        /// Converts a string to an array of bytes.
        /// </summary>
        /// <param name="s">A string to be converted</param>
        /// <returns></returns>
        public static byte[] ConvertToByte(String s)
        {
#if WINDOWS_PHONE
            return System.Text.Encoding.UTF8.GetBytes(s);
#else
            return System.Text.Encoding.ASCII.GetBytes(s);
#endif
        }

        /// <summary>
        /// Converts a string to an array of bytes.
        /// </summary>
        /// <param name="s">A string to be converted</param>
        /// <param name="bytes">The resulting byte array</param>
        public static void ConvertToByte(String s, byte[] bytes)
        {
#if WINDOWS_PHONE
            System.Text.Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);
#else
            System.Text.Encoding.ASCII.GetBytes(s, 0, s.Length, bytes, 0);
#endif
        }

        /// <summary>
        /// Converts an array of bytes to a string.
        /// </summary>
        /// <param name="b">An array of bytes</param>
        /// <returns></returns>
        public static String ConvertToString(byte[] b)
        {
            return ConvertToString(b, 0, b.Length);
        }

        /// <summary>
        /// Converts an array of bytes to a string.
        /// </summary>
        /// <param name="b">An array of bytes</param>
        /// <returns></returns>
        public static String ConvertToString(byte[] b, int startIndex, int length)
        {
#if WINDOWS_PHONE
            return System.Text.Encoding.UTF8.GetString(b, startIndex, length);
#else
            return System.Text.Encoding.ASCII.GetString(b, startIndex, length);
#endif
        }

        /// <summary>
        /// Converts 4 bytes to a single-precision floating number.
        /// </summary>
        /// <param name="b">An array of bytes with length of at least 4</param>
        /// <param name="startIndex">4 bytes are taken from this start index in the byte array</param>
        /// <returns></returns>
        public static float ConvertToFloat(byte[] b, int startIndex)
        {
            return BitConverter.ToSingle(b, startIndex);
        }

        /// <summary>
        /// Converts 4 bytes to a 32-bit integer value.
        /// </summary>
        /// <param name="b">An array of bytes with length of at least 4</param>
        /// <param name="startIndex">4 bytes are taken from this start index in the byte array</param>
        /// <returns></returns>
        public static int ConvertToInt(byte[] b, int startIndex)
        {
            return BitConverter.ToInt32(b, startIndex);
        }

        /// <summary>
        /// Converts 2 bytes to a 16-bit integer value.
        /// </summary>
        /// <param name="b">An array of bytes with length of at least 2</param>
        /// <param name="startIndex">2 bytes are taken from this start index in the byte array</param>
        /// <returns></returns>
        public static short ConvertToShort(byte[] b, int startIndex)
        {
            return BitConverter.ToInt16(b, startIndex);
        }

        /// <summary>
        /// Converts a list of single-precision floating numbers to an array of bytes.
        /// </summary>
        /// <param name="floats">A list of single-precision floating numbers</param>
        /// <returns>An array of bytes with size of 4 * (number of floats)</returns>
        public static byte[] ConvertFloatArray(List<float> floats)
        {
            byte[] b = new byte[floats.Count * 4];
            float[] temp = floats.ToArray();
            Buffer.BlockCopy(temp, 0, b, 0, b.Length);

            return b;
        }

        /// <summary>
        /// Converts a list of single-precision floating numbers to an array of bytes.
        /// </summary>
        /// <param name="floats">A list of single-precision floating numbers</param>
        /// <param name="bytes">The resulting byte array</param>
        public static void ConvertFloatArray(List<float> floats, byte[] bytes)
        {
            float[] temp = floats.ToArray();
            Buffer.BlockCopy(temp, 0, bytes, 0, temp.Length * 4);
        }

        /// <summary>
        /// Converts a list of 32-bit integer numbers to an array of bytes.
        /// </summary>
        /// <param name="ints">A list of 32-bit integer numbers</param>
        /// <returns>An array of bytes with size of 4 * (number of ints)</returns>
        public static byte[] ConvertIntArray(List<int> ints)
        {
            int[] temp = ints.ToArray();
            byte[] b = new byte[ints.Count * sizeof(int)];
            Buffer.BlockCopy(temp, 0, b, 0, b.Length);

            return b;
        }

        /// <summary>
        /// Converts a list of 32-bit integer numbers to an array of bytes.
        /// </summary>
        /// <param name="ints">A list of 32-bit integer numbers</param>
        /// <param name="bytes">The resulting byte array</param>
        public static void ConvertIntArray(List<int> ints, byte[] bytes)
        {
            int[] temp = ints.ToArray();
            Buffer.BlockCopy(temp, 0, bytes, 0, temp.Length * 4);
        }

        /// <summary>
        /// Converts a list of 16-bit integer numbers to an array of bytes.
        /// </summary>
        /// <param name="shorts">A list of 16-bit integer numbers</param>
        /// <returns>An array of bytes with size of 2 * (number of shorts)</returns>
        public static byte[] ConvertShortArray(List<short> shorts)
        {
            short[] temp = shorts.ToArray();
            byte[] b = new byte[shorts.Count * sizeof(short)];
            Buffer.BlockCopy(temp, 0, b, 0, b.Length);

            return b;
        }

        /// <summary>
        /// Converts a list of 16-bit integer numbers to an array of bytes.
        /// </summary>
        /// <param name="shorts">A list of 16-bit integer numbers</param>
        /// <param name="bytes">The resulting byte array</param>
        public static void ConvertShortArray(List<short> shorts, byte[] bytes)
        {
            short[] temp = shorts.ToArray();
            Buffer.BlockCopy(temp, 0, bytes, 0, temp.Length * 2);
        }

        /// <summary>
        /// Fills the given dest byte array from the destStartIndex with the entire src byte array
        /// </summary>
        /// <remarks>
        /// If the source contains more than (dest.Length - destStartIndex) bytes, then the overflowed 
        /// bytes are not copied into the destination array..
        /// </remarks>
        /// <param name="dest">The destination where the source array will be copied</param>
        /// <param name="destStartIndex">The index of the destination array where the copy starts</param>
        /// <param name="src">The source array where to copy from</param>
        public static void FillByteArray(ref byte[] dest, int destStartIndex, byte[] src)
        {
            int length = (src.Length > (dest.Length - destStartIndex)) ?
                (dest.Length - destStartIndex) : src.Length;
            Buffer.BlockCopy(src, 0, dest, destStartIndex, length);
        }

        /// <summary>
        /// Fills the given dest byte array from the destStartIndex with the src byte array starting at
        /// specific index with specific length
        /// </summary>
        /// <remarks>
        /// If the source contains more than (dest.Length - destStartIndex) bytes, then the overflowed 
        /// bytes are not copied into the destination array..
        /// </remarks>
        /// <param name="dest">The destination where the source array will be copied</param>
        /// <param name="destStartIndex">The index of the destination array where the copy starts</param>
        /// <param name="src">The source array where to copy from</param>
        /// <param name="srcStartIndex">The starting index to copy from</param>
        /// <param name="srcLength">The length to copy from the starting index</param>
        public static void FillByteArray(ref byte[] dest, int destStartIndex, byte[] src, int srcStartIndex, int srcLength)
        {
            Buffer.BlockCopy(src, srcStartIndex, dest, destStartIndex, srcLength);
        }

        /// <summary>
        /// Concatenates two byte arrays.
        /// </summary>
        /// <param name="b1">The first byte array to be concatenated</param>
        /// <param name="b2">The second byte array to be concatenated</param>
        /// <returns>The concatenated byte array with size of b1 + b2</returns>
        public static byte[] ConcatenateBytes(byte[] b1, byte[] b2)
        {
            byte[] b = new byte[b1.Length + b2.Length];

            Buffer.BlockCopy(b1, 0, b, 0, b1.Length);
            Buffer.BlockCopy(b2, 0, b, b1.Length, b2.Length);

            return b;
        }

        /// <summary>
        /// Concatenates two byte arrays from specific starting index and length.
        /// </summary>
        /// <param name="b1">The first byte array to be concatenated</param>
        /// <param name="b1StartIndex"></param>
        /// <param name="b1Length"></param>
        /// <param name="b2">The second byte array to be concatenated</param>
        /// <param name="b2StartIndex"></param>
        /// <param name="b2Length"></param>
        /// <returns>The concatenated byte array with size of b1 + b2</returns>
        public static byte[] ConcatenateBytes(byte[] b1, int b1StartIndex, int b1Length, byte[] b2, int b2StartIndex, int b2Length)
        {
            byte[] b = new byte[b1Length + b2Length];

            Buffer.BlockCopy(b1, b1StartIndex, b, 0, b1Length);
            Buffer.BlockCopy(b2, b2StartIndex, b, b1Length, b2Length);

            return b;
        }

        /// <summary>
        /// Concatenates two byte arrays from specific starting index and length.
        /// </summary>
        /// <param name="b1">The first byte array to be concatenated</param>
        /// <param name="b1StartIndex"></param>
        /// <param name="b1Length"></param>
        /// <param name="b2">The second byte array to be concatenated</param>
        /// <param name="b2StartIndex"></param>
        /// <param name="b2Length"></param>
        /// <param name="bytes">The resulting byte array</param>
        public static void ConcatenateBytes(byte[] b1, int b1StartIndex, int b1Length, byte[] b2, 
            int b2StartIndex, int b2Length, byte[] bytes)
        {
            Buffer.BlockCopy(b1, b1StartIndex, bytes, 0, b1Length);
            Buffer.BlockCopy(b2, b2StartIndex, bytes, b1Length, b2Length);
        }

        /// <summary>
        /// Concatenates two byte arrays from specific starting index and length and copy to a resulting byte array 
        /// at specific starting index.
        /// </summary>
        /// <param name="b1">The first byte array to be concatenated</param>
        /// <param name="b1StartIndex"></param>
        /// <param name="b1Length"></param>
        /// <param name="b2">The second byte array to be concatenated</param>
        /// <param name="b2StartIndex"></param>
        /// <param name="b2Length"></param>
        /// <param name="bytes">The resulting byte array</param>
        /// <param name="bytesStartIndex"></param>
        public static void ConcatenateBytes(byte[] b1, int b1StartIndex, int b1Length, byte[] b2,
            int b2StartIndex, int b2Length, byte[] bytes, int bytesStartIndex)
        {
            Buffer.BlockCopy(b1, b1StartIndex, bytes, bytesStartIndex, b1Length);
            Buffer.BlockCopy(b2, b2StartIndex, bytes, bytesStartIndex + b1Length, b2Length);
        }

        /// <summary>
        /// Truncates an array of bytes from the startIndex for the specified length.
        /// </summary>
        /// <param name="value">A byte array to be truncated</param>
        /// <param name="startIndex">The index in the 'value' array where truncation starts</param>
        /// <param name="length">The length of the new truncated array</param>
        /// <returns>The truncated byte array with size of 'length'</returns>
        public static byte[] Truncate(byte[] value, int startIndex, int length)
        {
            byte[] b = new byte[length];
            Buffer.BlockCopy(value, startIndex, b, 0, length);

            return b;
        }

        /// <summary>
        /// Truncates an array of bytes from the startIndex for the specified length.
        /// </summary>
        /// <param name="value">A byte array to be truncated</param>
        /// <param name="startIndex">The index in the 'value' array where truncation starts</param>
        /// <param name="length">The length of the new truncated array</param>
        /// <param name="bytes">The resulting byte array</param>
        public static void Truncate(byte[] value, int startIndex, int length, byte[] bytes)
        {
            Buffer.BlockCopy(value, startIndex, bytes, 0, length);
        }

        /// <summary>
        /// Rounds a given number to the nearst power of two number. The resulting number is
        /// always larger or equal to the given number.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int RoundToPowerOfTwo(int number)
        {
            double log2 = Math.Log(number, 2);
            return (int)Math.Pow(2, Math.Ceiling(log2));
        }

        /// <summary>
        /// Expands an existing array to new size without losing the data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="newLength"></param>
        public static void ExpandArray(ref byte[] data, int newLength)
        {
            byte[] copy = new byte[data.Length];
            Buffer.BlockCopy(data, 0, copy, 0, copy.Length);
            data = new byte[newLength];
            Buffer.BlockCopy(copy, 0, data, 0, copy.Length);
            copy = null;
        }

        /// <summary>
        /// Encode a texture2D into a Jpeg byte array.
        /// </summary>
        /// <param name="image">A texture2D to be encoded</param>
        /// <param name="width">The width of the returned encoded jpeg</param>
        /// <param name="height">The height of the returned encoded jpeg</param>
        public static byte[] Encode(Texture2D image, int width, int height)
        {
            byte[] jpegData = new byte[width * height / 4];
            MemoryStream jpegStream = new MemoryStream(jpegData, 0, jpegData.Length);
            image.SaveAsJpeg(jpegStream, width, height);

            //set the memory stream pointer to the begining 
            jpegStream.Seek(0, SeekOrigin.Begin);

            return jpegData;
        }

        /// <summary>
        /// Decode a Jpeg byte array in order to get a texture2D.
        /// </summary>
        /// <param name="jpegData">A Jpeg byte array to be decoded</param>
        /// <param name="graphicsDevice">GraphicsDevice from graphicsDeviceManager</param>
        public static Texture2D Decode(byte[] jpegData, GraphicsDevice graphicsDevice)
        {
            MemoryStream jpegStream = new MemoryStream(jpegData);
            Texture2D texture = Texture2D.FromStream(graphicsDevice, jpegStream);

            return texture;
        }
    }
}
