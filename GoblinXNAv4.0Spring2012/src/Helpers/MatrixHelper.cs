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
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.Helpers
{
    /// <summary>
    /// A helper class that implements various useful static functions that the Matrix class
    /// does not support.
    /// </summary>
    public class MatrixHelper
    {
        private static Matrix emptyMatrix = new Matrix();

        // for optimized unproject method
        private static Matrix invProjView;  
        private static Vector4 screenPosition;  
        private static Vector4 worldSpace;  
        private static Vector3 a;  
        private static Vector3 b; 
        private static float w;

        // for optimized project method
        private static Matrix viewProj;
        private static Matrix worldViewProj;

        /// <summary>
        /// Copies the contents from a 'src' matrix.
        /// </summary>
        /// <param name="src">The matrix to copy from</param>
        /// <returns>The matrix with copied contents</returns>
        public static Matrix CopyMatrix(Matrix src)
        {
            return new Matrix(src.M11, src.M12, src.M13, src.M14, src.M21, src.M22, src.M23, src.M24,
                src.M31, src.M32, src.M33, src.M34, src.M41, src.M42, src.M43, src.M44);
        }

        /// <summary>
        /// Converts an array of sixteen floats to Matrix.
        /// </summary>
        /// <param name="mat">An array of 16 floats</param>
        /// <returns></returns>
        public static Matrix FloatsToMatrix(float[] mat)
        {
            if (mat == null || mat.Length != 16)
                throw new ArgumentException("mat has to contain 16 floating numbers");

            return new Matrix(
                mat[0], mat[1], mat[2], mat[3],
                mat[4], mat[5], mat[6], mat[7],
                mat[8], mat[9], mat[10], mat[11],
                mat[12], mat[13], mat[14], mat[15]);
        }

        public static void FloatsToMatrix(float[] mat, out Matrix m)
        {
#if WINDOWS_PHONE
            m = Matrix.Identity;
#endif
            m.M11 = mat[0]; m.M12 = mat[1]; m.M13 = mat[2]; m.M14 = mat[3];
            m.M21 = mat[4]; m.M22 = mat[5]; m.M23 = mat[6]; m.M24 = mat[7];
            m.M31 = mat[8]; m.M32 = mat[9]; m.M33 = mat[10]; m.M34 = mat[11];
            m.M41 = mat[12]; m.M42 = mat[13]; m.M43 = mat[14]; m.M44 = mat[15];
        }

        /// <summary>
        /// Copies only the rotation part of the matrix (the upper-left 3x3 matrix, so it
        /// may actually contain the scaling factor as well).
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Matrix GetRotationMatrix(Matrix src)
        {
            Matrix rotMat = CopyMatrix(src);
            rotMat.M41 = rotMat.M42 = rotMat.M43 = 0;
            return rotMat;
        }

        public static void GetRotationMatrix(ref Matrix src, out Matrix dest)
        {
#if WINDOWS_PHONE
            dest = Matrix.Identity;
#endif
            dest.M11 = src.M11; dest.M12 = src.M12; dest.M13 = src.M13; dest.M14 = src.M14;
            dest.M21 = src.M21; dest.M22 = src.M22; dest.M23 = src.M23; dest.M24 = src.M24;
            dest.M31 = src.M31; dest.M32 = src.M32; dest.M33 = src.M33; dest.M34 = src.M34;
            dest.M41 = 0; dest.M42 = 0; dest.M43 = 0; dest.M44 = 1;
        }

        /// <summary>
        /// Multiplies a matrix with a vector. The calculation is Matrix.CreateTranslation('v') *
        /// 'mat'.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Matrix Multiply(Vector3 v, Matrix mat)
        {
            Matrix vMat = Matrix.CreateTranslation(v);
            return vMat * mat;
        }

        /// <summary>
        /// Orthonormalizes a transformation matrix.
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Matrix OrthonormalizeMatrix(Matrix mat)
        {
            Matrix m = mat;

            Vector3 axisX = Vector3Helper.Get(m.M11, m.M12, m.M13);
            Vector3 axisY = Vector3Helper.Get(m.M21, m.M22, m.M23);
            Vector3 axisZ = Vector3Helper.Get(m.M31, m.M32, m.M33);

            axisX.Normalize();
            axisY.Normalize();
            axisZ.Normalize();

            axisZ = Vector3.Normalize(Vector3.Cross(axisX, axisY));
            axisY = Vector3.Normalize(Vector3.Cross(axisZ, axisX));
            axisX = Vector3.Normalize(Vector3.Cross(axisY, axisZ));

            m.M11 = axisX.X; m.M12 = axisX.Y; m.M13 = axisX.Z;
            m.M21 = axisY.X; m.M22 = axisY.Y; m.M23 = axisY.Z;
            m.M31 = axisZ.X; m.M32 = axisZ.Y; m.M33 = axisZ.Z;

            return m;
        }

        /// <summary>
        /// Checks whether a transformation has changed/moved significantly compared to the previous
        /// transformation with 0.01f translational threshold and 0.1f * Math.PI / 180 rotational
        /// threshold. This means that if either the transformation's translation component changed
        /// more than 0.1f in distance or rotation component changed more than 0.1f * Math.PI / 180
        /// radians in any of the three (x, y, z) directions, then it's judged as having moved significantly.
        /// </summary>
        /// <param name="matPrev">The previous transformation matrix</param>
        /// <param name="matCurr">The current transformation matrix</param>
        /// <returns></returns>
        public static bool HasMovedSignificantly(Matrix matPrev, Matrix matCurr)
        {
            return HasMovedSignificantly(matPrev, matCurr, 0.01f, 0.1f * (float)Math.PI / 180);
        }
        
        /// <summary>
        /// Checks whether a transformation has changed/moved significantly compared to the previous
        /// transformation with the specified translational threshold and rotational threshold. 
        /// </summary>
        /// <param name="matPrev">The previous transformation matrix</param>
        /// <param name="matCurr">The current transformation matrix</param>
        /// <param name="transThreshold">The translational threshold</param>
        /// <param name="rotThreshold">The rotational threshold</param>
        /// <returns></returns>
        public static bool HasMovedSignificantly(Matrix matPrev, Matrix matCurr,
            float transThreshold, float rotThreshold)
        {
            // 1st time through
            if (matPrev.Equals(Matrix.Identity))
                return true;

            // Test translation
            if (Vector3.Distance(matPrev.Translation, matCurr.Translation) > transThreshold)
                return true;

            // Test rotations
            float dRollPrev, dPitchPrev, dYawPrev, dRollCurr, dPitchCurr, dYawCurr;
            Vector3 dPrev = Vector3Helper.ExtractAngles(matPrev);
            Vector3 dCurr = Vector3Helper.ExtractAngles(matCurr);

            dPitchPrev = dPrev.X;
            dYawPrev = dPrev.Y;
            dRollPrev = dPrev.Z;
            dPitchCurr = dCurr.X;
            dYawCurr = dCurr.Y;
            dRollCurr = dCurr.Z;

            if (Math.Abs(dRollPrev - dRollCurr) > rotThreshold)
                return true;

            if (Math.Abs(dPitchPrev - dPitchCurr) > rotThreshold)
                return true;

            if (Math.Abs(dYawPrev - dYawCurr) > rotThreshold)
                return true;

            // Not enough movement
            return false;
        }

        /// <summary>
        /// Convert every elements of the matrix to bytes.
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static byte[] ConvertToUnoptimizedBytes(Matrix mat)
        {
            List<float> data = new List<float>();
            data.Add(mat.M11); data.Add(mat.M12); data.Add(mat.M13); data.Add(mat.M14);
            data.Add(mat.M21); data.Add(mat.M22); data.Add(mat.M23); data.Add(mat.M24);
            data.Add(mat.M31); data.Add(mat.M32); data.Add(mat.M33); data.Add(mat.M34);
            data.Add(mat.M41); data.Add(mat.M42); data.Add(mat.M43); data.Add(mat.M44);

            return ByteHelper.ConvertFloatArray(data);
        }

        /// <summary>
        /// Decompose the matrix into rotation (Quaternion: 4 floats), scale (3 floats) if
        /// the scale is not Vector.One, and translation (3 floats), and pack these information
        /// into an array of bytes for efficiently transfering over the network.
        /// </summary>
        /// <param name="mat">A matrix to be converted</param>
        /// <returns>The resulting byte array</returns>
        public static byte[] ConvertToOptimizedBytes(Matrix mat)
        {
            return ByteHelper.ConvertFloatArray(ConvertToOptimizedFloats(mat));
        }

        /// <summary>
        /// Decompose the matrix into rotation (Quaternion: 4 floats), scale (3 floats) if
        /// the scale is not Vector.One, and translation (3 floats), and pack these information
        /// into an array of bytes for efficiently transfering over the network.
        /// </summary>
        /// <param name="mat">A matrix to be converted</param>
        /// <param name="bytes">The resulting byte array</param>
        /// <returns>Number of resulting bytes</returns>
        public static int ConvertToOptimizedBytes(Matrix mat, byte[] bytes)
        {
            List<float> data = ConvertToOptimizedFloats(mat);
            ByteHelper.ConvertFloatArray(data, bytes);
            return data.Count * sizeof(float);
        }

        private static List<float> ConvertToOptimizedFloats(Matrix mat)
        {
            Quaternion rot;
            Vector3 scale;
            Vector3 trans;
            mat.Decompose(out scale, out rot, out trans);

            List<float> data = new List<float>();
            data.Add(rot.X);
            data.Add(rot.Y);
            data.Add(rot.Z);
            data.Add(rot.W);
            data.Add(trans.X);
            data.Add(trans.Y);
            data.Add(trans.Z);

            // Send scale information if and only if its not Vector.One
            if (Vector3.Distance(scale, Vector3.One) > 0.00001f)
            {
                data.Add(scale.X);
                data.Add(scale.Y);
                data.Add(scale.Z);
            }

            return data;
        }

        /// <summary>
        /// Converts an array of bytes containing transformation (rotation, scale, and
        /// translation) into a matrix. Use this method to convert back the information
        /// packed by ConvertToOptimizedBytes method.
        /// </summary>
        /// <param name="bytes"></param>
        /// <see cref="ConvertToOptimizedBytes"/>
        /// <returns></returns>
        public static Matrix ConvertFromOptimizedBytes(byte[] bytes)
        {
            Matrix mat = Matrix.Identity;
            ConvertFromOptimizedBytes(bytes, ref mat);
            return mat;
        }

        /// <summary>
        /// Converts an array of bytes containing transformation (rotation, scale, and
        /// translation) into a matrix. Use this method to convert back the information
        /// packed by ConvertToOptimizedBytes method.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <see cref="ConvertToOptimizedBytes"/>
        /// <returns></returns>
        public static Matrix ConvertFromOptimizedBytes(byte[] bytes, int startIndex, int length)
        {
            Matrix mat = Matrix.Identity;
            ConvertFromOptimizedBytes(bytes, startIndex, length, ref mat);
            return mat;
        }

        /// <summary>
        /// Converts an array of bytes containing transformation (rotation, scale, and
        /// translation) into a matrix. Use this method to convert back the information
        /// packed by ConvertToOptimizedBytes method.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="mat"></param>
        /// <see cref="ConvertToOptimizedBytes"/>
        public static void ConvertFromOptimizedBytes(byte[] bytes, ref Matrix mat)
        {
            ConvertFromOptimizedBytes(bytes, 0, bytes.Length, ref mat);
        }

        /// <summary>
        /// Converts an array of bytes containing transformation (rotation, scale, and
        /// translation) into a matrix. Use this method to convert back the information
        /// packed by ConvertToOptimizedBytes method.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="mat"></param>
        public static void ConvertFromOptimizedBytes(byte[] bytes, int startIndex, int length, ref Matrix mat)
        {
            float[] vals = new float[length / 4];
            Buffer.BlockCopy(bytes, startIndex, vals, 0, length);

            Quaternion rot = new Quaternion(vals[0], vals[1], vals[2], vals[3]);
            Vector3 trans = new Vector3(vals[4], vals[5], vals[6]);
            Vector3 scale = Vector3.One;

            Matrix temp = Matrix.Identity;
            Matrix temp2 = Matrix.Identity;

            if (length > 28)
            {
                scale = new Vector3(vals[7], vals[8], vals[9]);

                Matrix.CreateScale(ref scale, out mat);
                Matrix.CreateFromQuaternion(ref rot, out temp);
                Matrix.Multiply(ref mat, ref temp, out temp2);
                Matrix.CreateTranslation(ref trans, out temp);
                Matrix.Multiply(ref temp2, ref temp, out mat);
            }
            else
            {
                Matrix.CreateFromQuaternion(ref rot, out temp);
                Matrix.CreateTranslation(ref trans, out temp2);
                Matrix.Multiply(ref temp, ref temp2, out mat);
            }
        }

        /// <summary>
        /// Converts an array of bytes containing transformation (rotation, scale, and
        /// translation) into a matrix. Use this method to convert back the information
        /// packed by ConvertToUnptimizedBytes method.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Matrix ConvertFromUnoptimizedBytes(byte[] bytes)
        {
            Matrix mat = Matrix.Identity;
            ConvertFromUnoptimizedBytes(bytes, ref mat);
            return mat;
        }

        /// <summary>
        /// Converts an array of bytes containing transformation (rotation, scale, and
        /// translation) into a matrix. Use this method to convert back the information
        /// packed by ConvertToUnptimizedBytes method.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="mat"></param>
        public static void ConvertFromUnoptimizedBytes(byte[] bytes, ref Matrix mat)
        {
            ConvertFromUnoptimizedBytes(bytes, 0, ref mat);
        }

        /// <summary>
        /// Converts an array of bytes containing transformation (rotation, scale, and
        /// translation) into a matrix. Use this method to convert back the information
        /// packed by ConvertToUnptimizedBytes method.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startIndex"></param>
        /// <param name="mat"></param>
        public static void ConvertFromUnoptimizedBytes(byte[] bytes, int startIndex, ref Matrix mat)
        {
            mat.M11 = BitConverter.ToSingle(bytes, startIndex);
            mat.M12 = BitConverter.ToSingle(bytes, startIndex + 4);
            mat.M13 = BitConverter.ToSingle(bytes, startIndex + 8);
            mat.M14 = BitConverter.ToSingle(bytes, startIndex + 12);

            mat.M21 = BitConverter.ToSingle(bytes, startIndex + 16);
            mat.M22 = BitConverter.ToSingle(bytes, startIndex + 20);
            mat.M23 = BitConverter.ToSingle(bytes, startIndex + 24);
            mat.M24 = BitConverter.ToSingle(bytes, startIndex + 28);

            mat.M31 = BitConverter.ToSingle(bytes, startIndex + 32);
            mat.M32 = BitConverter.ToSingle(bytes, startIndex + 36);
            mat.M33 = BitConverter.ToSingle(bytes, startIndex + 40);
            mat.M34 = BitConverter.ToSingle(bytes, startIndex + 44);

            mat.M41 = BitConverter.ToSingle(bytes, startIndex + 48);
            mat.M42 = BitConverter.ToSingle(bytes, startIndex + 52);
            mat.M43 = BitConverter.ToSingle(bytes, startIndex + 56);
            mat.M44 = BitConverter.ToSingle(bytes, startIndex + 60);
        }

        public static Matrix FromString(String matVals)
        {
            Matrix mat = Matrix.Identity;

            String[] strs = matVals.Split(':', ' ', '{', '}');
            mat.M11 = float.Parse(strs[4]);
            mat.M12 = float.Parse(strs[6]);
            mat.M13 = float.Parse(strs[8]);
            mat.M14 = float.Parse(strs[10]);

            mat.M21 = float.Parse(strs[14]);
            mat.M22 = float.Parse(strs[16]);
            mat.M23 = float.Parse(strs[18]);
            mat.M24 = float.Parse(strs[20]);

            mat.M31 = float.Parse(strs[24]);
            mat.M32 = float.Parse(strs[26]);
            mat.M33 = float.Parse(strs[28]);
            mat.M34 = float.Parse(strs[30]);

            mat.M41 = float.Parse(strs[34]);
            mat.M42 = float.Parse(strs[36]);
            mat.M43 = float.Parse(strs[38]);
            mat.M44 = float.Parse(strs[40]);

            return mat;
        }

        /// <summary>
        /// An empty (all zero) matrix.
        /// </summary>
        public static Matrix Empty
        {
            get { return emptyMatrix; }
        }

        /// <summary>
        /// Converts a matrix to an array of 16 floats.
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static float[] ToFloats(Matrix mat)
        {
            float[] floats = {mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23, mat.M24, 
                mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44};

            return floats;
        }

        public static void ToFloats(ref Matrix mat, float[] floats)
        {
            floats[0] = mat.M11; floats[1] = mat.M12; floats[2] = mat.M13; floats[3] = mat.M14;
            floats[4] = mat.M21; floats[5] = mat.M22; floats[6] = mat.M23; floats[7] = mat.M24;
            floats[8] = mat.M31; floats[9] = mat.M32; floats[10] = mat.M33; floats[11] = mat.M34;
            floats[12] = mat.M41; floats[13] = mat.M42; floats[14] = mat.M43; floats[15] = mat.M44;
        }

        /// <summary>
        /// Prints out a matrix to the console.
        /// </summary>
        /// <param name="mat"></param>
        public static void PrintMatrix(Matrix mat)
        {
            Console.WriteLine(mat.M11 + " " + mat.M21 + " " + mat.M31 + " " + mat.M41);
            Console.WriteLine(mat.M12 + " " + mat.M22 + " " + mat.M32 + " " + mat.M42);
            Console.WriteLine(mat.M13 + " " + mat.M23 + " " + mat.M33 + " " + mat.M43);
            Console.WriteLine(mat.M14 + " " + mat.M24 + " " + mat.M34 + " " + mat.M44);
            Console.WriteLine("");
        }

        /// <summary>
        /// Converts a rotation vector into a rotation matrix.
        /// </summary>
        /// <param name="Rotation"></param>
        /// <remarks>
        /// http://www.innovativegames.net/blog/blog/2009/03/18/matrices-quaternions-and-euler-angle-vectors/
        /// </remarks>
        /// <returns></returns>
        public static Matrix Vector3ToMatrix(Vector3 Rotation)
        {
            return Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
        }

        /// <summary>
        /// Prepares for the Unproject function (which is 30 times faster than Viewport.Unproject)
        /// </summary>
        /// <param name="viewportX"></param>
        /// <param name="viewportY"></param>
        /// <param name="viewportWidth"></param>
        /// <param name="viewportHeight"></param>
        /// <param name="maxDepth"></param>
        /// <param name="minDepth"></param>
        /// <param name="viewMat"></param>
        /// <param name="projMat"></param>
        public static void PrepareUnproject(float viewportX, float viewportY, float viewportWidth,
            float viewportHeight, float maxDepth, float minDepth, Matrix viewMat, Matrix projMat)
        {
            Matrix invProj = Matrix.Invert(projMat);
            Matrix invView = Matrix.Invert(viewMat);
            invProjView = invProj * invView;
            screenPosition = new Vector4(1f);
            screenPosition.W = 1f;
            a.X = 2f * (1f / viewportWidth);
            b.X = a.X * viewportX + 1f;
            a.Y = 2f * (1f / viewportHeight);
            b.Y = a.Y * viewportY + 1f;
            a.Z = minDepth;
            b.Z = 1f / (maxDepth - minDepth);
        }

        /// <summary>
        /// Unproject method that is 30 times faster than the Viewport.Unproject function due to
        /// the usage of pre-computed values. Make sure to call PrepareUnproject method for the
        /// pre-computation to happen, and then call this Unproject method unless any of the variables
        /// passed in to PrepareUnproject changes (if they do change, make sure to call it again with
        /// update values to guarantee correct unprojection).
        /// </summary>
        /// <remarks>
        /// This code is from MSDN forum (http://forums.create.msdn.com/forums/p/57082/348602.aspx).
        /// Credit goes to nathanjervis. 
        /// </remarks>
        /// <see cref="PrepareUnproject"/>
        /// <param name="screenSpace"></param>
        public static void Unproject(ref Vector3 screenSpace, ref Vector3 result)
        {
            screenPosition.X = (a.X * screenSpace.X - b.X);
            screenPosition.Y = -(a.Y * screenSpace.Y - b.Y);
            screenPosition.Z = (screenSpace.Z - a.Z) * b.Z;
            Vector4.Transform(ref screenPosition, ref invProjView, out worldSpace);
            w = 1f / worldSpace.W;
            result.X = worldSpace.X * w;
            result.Y = worldSpace.Y * w;
            result.Z = worldSpace.Z * w;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="world"></param>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        public static void PrepareProject(Matrix world, Matrix view, Matrix projection)
        {
            Matrix.Multiply(ref view, ref projection, out viewProj);
            Matrix.Multiply(ref world, ref viewProj, out worldViewProj);
        }

        /// <summary>
        /// Project method
        /// </summary>
        /// <remarks>
        /// This code is based on the HLSL code provided on 
        /// http://social.msdn.microsoft.com/Forums/en-US/xnaframework/thread/fa479f61-c31f-4b73-b7a4-29d101b79048/ .
        /// Credit goes to riemerg. 
        /// </remarks>
        /// <see cref="PrepareProject"/>
        /// <param name="worldPos"></param>
        /// <param name="screenSpace"></param>
        public static void Project(ref Vector3 worldPos, ref Vector3 screenSpace)
        {
            Vector4.Transform(ref worldPos, ref worldViewProj, out screenPosition);
            float w = 1 / screenPosition.W / 2.0f;
            screenSpace.X = screenPosition.X * w + 0.5f;
            screenSpace.Y = 1 - (screenPosition.Y * w + 0.5f);
        }

#if WINDOWS

        /// <summary>
        /// Saves Matrix values to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="mat"></param>
        public static void SaveMatrixToXML(string filename, Matrix mat)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);

            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

            XmlElement xmlRootNode = xmlDoc.CreateElement("Matrix");
            xmlDoc.AppendChild(xmlRootNode);

            SaveMatrixToXML(xmlRootNode, mat, xmlDoc);

            try
            {
                xmlDoc.Save(filename);
            }
            catch (Exception exp)
            {
                throw new GoblinException("Failed to save the matrix: " + filename);
            }
        }

        /// <summary>
        /// Saves Matrix values to an XML node
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="mat"></param>
        /// <param name="xmlDoc"></param>
        public static void SaveMatrixToXML(XmlElement rootNode, Matrix mat, XmlDocument xmlDoc)
        {
            XmlElement m11Node = xmlDoc.CreateElement("M11");
            m11Node.InnerText = mat.M11.ToString();
            rootNode.AppendChild(m11Node);

            XmlElement m12Node = xmlDoc.CreateElement("M12");
            m12Node.InnerText = mat.M12.ToString();
            rootNode.AppendChild(m12Node);

            XmlElement m13Node = xmlDoc.CreateElement("M13");
            m13Node.InnerText = mat.M13.ToString();
            rootNode.AppendChild(m13Node);

            XmlElement m14Node = xmlDoc.CreateElement("M14");
            m14Node.InnerText = mat.M14.ToString();
            rootNode.AppendChild(m14Node);

            XmlElement m21Node = xmlDoc.CreateElement("M21");
            m21Node.InnerText = mat.M21.ToString();
            rootNode.AppendChild(m21Node);

            XmlElement m22Node = xmlDoc.CreateElement("M22");
            m22Node.InnerText = mat.M22.ToString();
            rootNode.AppendChild(m22Node);

            XmlElement m23Node = xmlDoc.CreateElement("M23");
            m23Node.InnerText = mat.M23.ToString();
            rootNode.AppendChild(m23Node);

            XmlElement m24Node = xmlDoc.CreateElement("M24");
            m24Node.InnerText = mat.M24.ToString();
            rootNode.AppendChild(m24Node);

            XmlElement m31Node = xmlDoc.CreateElement("M31");
            m31Node.InnerText = mat.M31.ToString();
            rootNode.AppendChild(m31Node);

            XmlElement m32Node = xmlDoc.CreateElement("M32");
            m32Node.InnerText = mat.M32.ToString();
            rootNode.AppendChild(m32Node);

            XmlElement m33Node = xmlDoc.CreateElement("M33");
            m33Node.InnerText = mat.M33.ToString();
            rootNode.AppendChild(m33Node);

            XmlElement m34Node = xmlDoc.CreateElement("M34");
            m34Node.InnerText = mat.M34.ToString();
            rootNode.AppendChild(m34Node);

            XmlElement m41Node = xmlDoc.CreateElement("M41");
            m41Node.InnerText = mat.M41.ToString();
            rootNode.AppendChild(m41Node);

            XmlElement m42Node = xmlDoc.CreateElement("M42");
            m42Node.InnerText = mat.M42.ToString();
            rootNode.AppendChild(m42Node);

            XmlElement m43Node = xmlDoc.CreateElement("M43");
            m43Node.InnerText = mat.M43.ToString();
            rootNode.AppendChild(m43Node);

            XmlElement m44Node = xmlDoc.CreateElement("M44");
            m44Node.InnerText = mat.M44.ToString();
            rootNode.AppendChild(m44Node);
        }

        /// <summary>
        /// Loads Matrix values from a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="mat"></param>
        public static void LoadMatrixFromXML(string filename, ref Matrix mat)
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(filename);
            }
            catch (Exception exp)
            {
                throw new GoblinException(exp.Message);
            }

            foreach (XmlNode xmlNode in xmlDoc.ChildNodes)
            {
                if (xmlNode is XmlElement)
                {
                    if (xmlNode.Name.Equals("Matrix"))
                    {
                        MatrixHelper.LoadMatrixFromXML(xmlNode, ref mat);
                    }
                }
            }
        }

        /// <summary>
        /// Loads Matrix values from an XML node
        /// </summary>
        /// <param name="matrixNode"></param>
        /// <param name="mat"></param>
        public static void LoadMatrixFromXML(XmlNode matrixNode, ref Matrix mat)
        {
            mat.M11 = float.Parse(matrixNode.ChildNodes[0].InnerText);
            mat.M12 = float.Parse(matrixNode.ChildNodes[1].InnerText);
            mat.M13 = float.Parse(matrixNode.ChildNodes[2].InnerText);
            mat.M14 = float.Parse(matrixNode.ChildNodes[3].InnerText);

            mat.M21 = float.Parse(matrixNode.ChildNodes[4].InnerText);
            mat.M22 = float.Parse(matrixNode.ChildNodes[5].InnerText);
            mat.M23 = float.Parse(matrixNode.ChildNodes[6].InnerText);
            mat.M24 = float.Parse(matrixNode.ChildNodes[7].InnerText);

            mat.M31 = float.Parse(matrixNode.ChildNodes[8].InnerText);
            mat.M32 = float.Parse(matrixNode.ChildNodes[9].InnerText);
            mat.M33 = float.Parse(matrixNode.ChildNodes[10].InnerText);
            mat.M34 = float.Parse(matrixNode.ChildNodes[11].InnerText);

            mat.M41 = float.Parse(matrixNode.ChildNodes[12].InnerText);
            mat.M42 = float.Parse(matrixNode.ChildNodes[13].InnerText);
            mat.M43 = float.Parse(matrixNode.ChildNodes[14].InnerText);
            mat.M44 = float.Parse(matrixNode.ChildNodes[15].InnerText);
        }
#endif
    }
}
