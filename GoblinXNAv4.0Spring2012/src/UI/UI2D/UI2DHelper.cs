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

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// A helper class that implements various helper functions for 2D UI component classes.
    /// </summary>
    public class UI2DHelper
    {
        /// <summary>
        /// Tests whether a point 'p' is within the bounds of a rectangle 'rect'.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="rect"></param>
        /// <returns>Whether 'p' is within 'rect'</returns>
        public static bool IsWithin(Point p, Rectangle rect)
        {
            return IsWithin(p.X, p.Y, rect);
        }

        /// <summary>
        /// Tests whether a point (x, y) is within the bounds of a rectangle 'rect'.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rect"></param>
        /// <returns>Whether a point (x, y) is within 'rect'</returns>
        public static bool IsWithin(int x, int y, Rectangle rect)
        {
            return ((x >= rect.X) && (x <= rect.X + rect.Width) &&
                (y >= rect.Y) && (y <= rect.Y + rect.Height));
        }

        /// <summary>
        /// Tests whether a point 'p' is within a non-self-crossing polygon. The result can be inaccurate if
        /// the point 'p' is really close to the edge of the polygon.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static bool IsWithin(Vector2 p, List<Vector2> points)
        {
            return IsWithin(p.X, p.Y, points);
        }

        /// <summary>
        /// Tests whether a point (x, y) is within a non-self-crossing polygon. The result can be inaccurate if
        /// the point (x, y) is really close to the edge of the polygon.
        /// </summary>
        /// <remarks>
        /// This code was referenced from http://alienryderflex.com/polygon/ . 
        /// </remarks>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static bool IsWithin(float x, float y, List<Vector2> points)
        {
            int j = points.Count - 1;
            bool inside = false;

            for (int i = 0; i < points.Count; i++)
            {
                if ((points[i].Y < y && points[j].Y >= y) || (points[j].Y < y && points[i].Y >= y))
                    if (points[i].X + (y - points[i].Y) / (points[j].Y - points[i].Y) * (points[j].X - points[i].X) < x)
                        inside = !inside;

                j = i;
            }

            return inside;
        }

        /// <summary>
        /// Tests whether a point 'p' is on an edge of the rectangle 'rect'.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static bool OnEdge(Point p, Rectangle rect)
        {
            return OnEdge(p.X, p.Y, rect);
        }

        /// <summary>
        /// Tests whether a point (x, y) is on an edge of the rectangle 'rect'.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static bool OnEdge(int x, int y, Rectangle rect)
        {
            if ((x == rect.X) || (x == rect.X + rect.Width))
            {
                if ((y >= rect.Y) && (y <= rect.Y + rect.Height))
                    return true;
            }
            else if ((y == rect.Y) || (y == rect.Y + rect.Height))
            {
                if ((x >= rect.X) && (x <= rect.X + rect.Width))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Tests whether a point 'p' is on a corner of the rectangle 'rect'.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static bool OnCorner(Point p, Rectangle rect)
        {
            return OnCorner(p.X, p.Y, rect);
        }

        /// <summary>
        /// Tests whether a point (x, y) is on a corner of the rectangle 'rect'.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static bool OnCorner(int x, int y, Rectangle rect)
        {
            return (((x == rect.X) || (x == rect.X + rect.Width)) &&
                ((y == rect.Y) || (y == rect.Y + rect.Height)));
        }

        

        /// <summary>
        /// Gets the distance between point 'p1' and 'p2'.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double GetDistance(Vector3 p1, Vector3 p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) +
                (p1.Y - p2.Y) * (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z));
        }

        /// <summary>
        /// Converts a 3D point to a projected 2D screen point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point Convert3DPointTo2D(Vector3 point)
        {
            Vector4 result4 = Vector4.Transform(point, State.ViewMatrix * State.ProjectionMatrix);
            if (result4.W == 0)
                result4.W = 0.000001f;
            Vector3 result = new Vector3(result4.X / result4.W, result4.Y / result4.W, result4.Z / result4.W);

            return new Point((int)Math.Round(+result.X * (State.Width / 2)) + (State.Width / 2), 
                (int)Math.Round(-result.Y * (State.Height / 2)) + (State.Height / 2));
        }
    }
}
