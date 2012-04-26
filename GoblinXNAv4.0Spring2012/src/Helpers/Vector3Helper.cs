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

using GoblinXNA.Graphics;

namespace GoblinXNA.Helpers
{
    /// <summary>
    /// A helper class that implements various useful static functions that the Vector3 class
    /// does not support. 
    /// </summary>
    public class Vector3Helper
    {
        /// <summary>
        /// Gets the x, y, and z dimensions of a bounding box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns>The x, y, and z dimension of a bounding box stored in Vector3 class</returns>
        public static Vector3 GetDimensions(BoundingBox box)
        {
            return (box.Max - box.Min);
        }

        /// <summary>
        /// Converts from Vector4 type to Vector3 type by dropping the w component.
        /// </summary>
        /// <param name="v4">A Vector4 object</param>
        /// <returns>A Vector3 object without the w component</returns>
        public static Vector3 GetVector3(Vector4 v4)
        {
            Vector3 vector3 = new Vector3();
            vector3.X = v4.X;
            vector3.Y = v4.Y;
            vector3.Z = v4.Z;
            return vector3;
        }

        /// <summary>
        /// Calculate the normal perpendicular to two vectors v0->v1 and v0->v2 using right hand rule.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 GetNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 v0_1 = v1 - v0;
            Vector3 v0_2 = v2 - v0;

            Vector3 normal = Vector3.Cross(v0_2, v0_1);
            normal.Normalize();

            return normal;
        }

        public static Vector3 Get(float x, float y, float z)
        {
            Vector3 result = new Vector3();
            result.X = x;
            result.Y = y;
            result.Z = z;
            return result;
        }

        /// <summary>
        /// Converts a Vector3 object to an array of three floats in the order of x, y, and z.
        /// </summary>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static float[] ToFloats(ref Vector3 v3)
        {
            float[] floats = { v3.X, v3.Y, v3.Z };
            return floats;
        }

        public static float[] ToFloats(Vector3 v3)
        {
            float[] floats = { v3.X, v3.Y, v3.Z };
            return floats;
        }

        public static Vector3 FromFloats(float[] values)
        {
            return new Vector3(values[0], values[1], values[2]);
        }

        public static void FromFloats(float[] values, out Vector3 vec3)
        {
#if WINDOWS_PHONE
            vec3 = Vector3.Zero;
#endif
            vec3.X = values[0];
            vec3.Y = values[1];
            vec3.Z = values[2];
        }

        public static Vector3 FromString(String strVal)
        {
            Vector3 vec3 = Vector3.Zero;

            String[] vals = strVal.Split(':', ' ', '}');
            vec3.X = float.Parse(vals[1]);
            vec3.Y = float.Parse(vals[3]);
            vec3.Z = float.Parse(vals[5]);

            return vec3;
        }

        public static Vector3 FromCommaSeparatedString(String strVal)
        {
            Vector3 vec3 = Vector3.Zero;

            String[] vals = strVal.Split(',');
            vec3.X = float.Parse(vals[0]);
            vec3.Y = float.Parse(vals[1]);
            vec3.Z = float.Parse(vals[2]);

            return vec3;
        }

        /// <summary>
        /// http://www.codeguru.com/forum/archive/index.php/t-329530.html
        /// 
        /// For a homogeneous geometrical transformation matrix, you can get the roll, pitch and yaw angles, 
        /// following the TRPY convention, using the following formulas:
        ///
        ///  roll (rotation around z) : atan2(xy, xx)
        ///  pitch (rotation around y) : -arcsin(xz)
        ///  yaw (rotation around x) : atan2(yz,zz)
        ///
        ///  where the matrix is defined in the form:
        ///
        ///  [
        ///   xx, yx, zx, px;
        ///   xy, yy, zy, py;
        ///   xz, yz, zz, pz;
        ///   0, 0, 0, 1
        ///  ]
        /// </summary>
        /// <param name="mat"></param>
        /// <returns>x=pitch, y=yaw, z=roll</returns>
        public static Vector3 ExtractAngles(Matrix mat)
        {
            float roll = (float)Math.Atan2(mat.M12, mat.M11);
            float pitch = (float)Math.Atan2(mat.M23, mat.M33);
            float yaw = -(float)Math.Asin(mat.M13);

            return new Vector3(pitch, yaw, roll);
        }

        /// <summary>
        /// Returns Euler angles that point from one point to another.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="location"></param>
        /// <remarks>
        /// http://www.innovativegames.net/blog/blog/2009/03/18/matrices-quaternions-and-euler-angle-vectors/
        /// </remarks>
        /// <returns></returns>
        public static Vector3 AngleTo(Vector3 from, Vector3 location)
        {
            Vector3 angle = new Vector3();
            Vector3 v3 = Vector3.Normalize(location - from);

            angle.X = (float)Math.Asin(v3.Y);
            angle.Y = (float)Math.Atan2((double)-v3.X, (double)-v3.Z);

            return angle;
        }

        /// <summary>
        /// Converts a Quaternion to Euler angles (X = Yaw, Y = Pitch, Z = Roll)
        /// </summary>
        /// <param name="rotation"></param>
        /// <remarks>
        /// http://www.innovativegames.net/blog/blog/2009/03/18/matrices-quaternions-and-euler-angle-vectors/
        /// </remarks>
        /// <returns></returns>
        public static Vector3 QuaternionToEulerAngleVector3(Quaternion rotation)
        {
            Vector3 rotationaxes = new Vector3();
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);

            rotationaxes = AngleTo(new Vector3(), forward);

            if (rotationaxes.X == MathHelper.PiOver2)
            {
                rotationaxes.Y = (float)Math.Atan2((double)up.X, (double)up.Z);
                rotationaxes.Z = 0;
            }
            else if (rotationaxes.X == -MathHelper.PiOver2)
            {
                rotationaxes.Y = (float)Math.Atan2((double)-up.X, (double)-up.Z);
                rotationaxes.Z = 0;
            }
            else
            {
                up = Vector3.Transform(up, Matrix.CreateRotationY(-rotationaxes.Y));
                up = Vector3.Transform(up, Matrix.CreateRotationX(-rotationaxes.X));

                rotationaxes.Z = (float)Math.Atan2((double)-up.Z, (double)up.Y);
            }

            return rotationaxes;
        }

        /// <summary>
        /// Converts a Rotation Matrix to a quaternion, then into a Vector3 containing
        /// Euler angles (X: Pitch, Y: Yaw, Z: Roll)
        /// </summary>
        /// <param name="Rotation"></param>
        /// <remarks>
        /// http://www.innovativegames.net/blog/blog/2009/03/18/matrices-quaternions-and-euler-angle-vectors/
        /// </remarks>
        /// <returns></returns>
        public static Vector3 MatrixToEulerAngleVector3(Matrix Rotation)
        {
            Vector3 translation, scale;
            Quaternion rotation;

            Rotation.Decompose(out scale, out rotation, out translation);

            Vector3 eulerVec = QuaternionToEulerAngleVector3(rotation);

            return eulerVec;
        }

        /// <summary>
        /// Converts Euler angles from radian format to degree format.
        /// </summary>
        /// <param name="Vector"></param>
        /// <remarks>
        /// http://www.innovativegames.net/blog/blog/2009/03/18/matrices-quaternions-and-euler-angle-vectors/
        /// </remarks>
        /// <returns></returns>
        public static Vector3 RadiansToDegrees(Vector3 Vector)
        {
            return new Vector3(
                MathHelper.ToDegrees(Vector.X),
                MathHelper.ToDegrees(Vector.Y),
                MathHelper.ToDegrees(Vector.Z));
        }

        /// <summary>
        /// Converts Euler angles from degree format to radian format.
        /// </summary>
        /// <param name="Vector"></param>
        /// <remarks>
        /// http://www.innovativegames.net/blog/blog/2009/03/18/matrices-quaternions-and-euler-angle-vectors/
        /// </remarks>
        /// <returns></returns>
        public static Vector3 DegreesToRadians(Vector3 Vector)
        {
            return new Vector3(
                MathHelper.ToRadians(Vector.X),
                MathHelper.ToRadians(Vector.Y),
                MathHelper.ToRadians(Vector.Z));
        }
    }
}
