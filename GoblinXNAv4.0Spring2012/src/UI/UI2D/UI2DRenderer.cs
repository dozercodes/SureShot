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
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// A helper class for performing 2D drawing, such as drawing lines, filling or drawing 
    /// rectangular, circular, or polygonal shapes.
    /// </summary>
    public class UI2DRenderer
    {
        #region Static Member Fields
        private static Dictionary<Circle, Texture2D> circleTextures = new Dictionary<Circle, Texture2D>();
        private static Dictionary<Polygon, Texture2D> polyTextures = new Dictionary<Polygon, Texture2D>();
        private static List<Drawable2DObject> queuedObjects = new List<Drawable2DObject>();
        private static List<Drawable2DObject> opaqueObjects = new List<Drawable2DObject>();

        private static List<EdgeState> GETPtr;
        private static List<EdgeState> AETPtr;
        #endregion

        #region Enums

        public enum PolygonShape { Convex, Nonconvex, Complex };

        #endregion

        #region Public Static Drawing Methods
        /// <summary>
        /// Fills a rectangle portion of the screen with given texture and color. If just want to fill
        /// it with a color, then set 'texture' to null.
        /// </summary>
        /// <param name="rect">A rectangle region to fill</param>
        /// <param name="texture">A texture to use for filling</param>
        /// <param name="color">A color to use for filling</param>
        public static void FillRectangle(Rectangle rect, Texture2D texture, Color color)
        {
            if (texture == null)
                texture = State.BlankTexture;

            Drawable2DObject obj2d = new Drawable2DObject();
            obj2d.isText = false;
            ShapeInfo shapeInfo = new ShapeInfo();
            shapeInfo.texture = texture;
            shapeInfo.rect = rect;
            shapeInfo.color = color;
            shapeInfo.angle = 0;
            obj2d.shapeInfo = shapeInfo;

            queuedObjects.Add(obj2d);
        }

        /// <summary>
        /// Draws a rectangle with the given color and pixel width.
        /// </summary>
        /// <param name="rect">A rectangle to draw</param>
        /// <param name="color">The color of the line</param>
        /// <param name="pixelWidth">The width of the line</param>
        public static void DrawRectangle(Rectangle rect, Color color, int pixelWidth)
        {
            Rectangle[] rects = {
                new Rectangle(rect.X, rect.Y, rect.Width, pixelWidth),
                new Rectangle(rect.X, rect.Y, pixelWidth, rect.Height),
                new Rectangle(rect.X, rect.Y + rect.Height - pixelWidth, rect.Width, pixelWidth),
                new Rectangle(rect.X + rect.Width - pixelWidth, rect.Y, pixelWidth, rect.Height)};

            foreach (Rectangle rectangle in rects)
            {
                Drawable2DObject obj2d = new Drawable2DObject();
                obj2d.isText = false;
                ShapeInfo shapeInfo = new ShapeInfo();
                shapeInfo.texture = State.BlankTexture;
                shapeInfo.rect = rectangle;
                shapeInfo.color = color;
                shapeInfo.angle = 0;
                obj2d.shapeInfo = shapeInfo;

                queuedObjects.Add(obj2d);
            }
        }

        /// <summary>
        /// Draws a line between two given points with the given color and pixel width
        /// </summary>
        /// <param name="p1">The starting point of the line</param>
        /// <param name="p2">The endint point of the line</param>
        /// <param name="color">The color of the line</param>
        /// <param name="pixelWidth">The width of the line</param>
        public static void DrawLine(Point p1, Point p2, Color color, int pixelWidth)
        {
            DrawLine(p1.X, p1.Y, p2.X, p2.Y, color, pixelWidth);
        }

        /// <summary>
        /// Draws a line between two given points with the given color and pixel width
        /// </summary>
        /// <param name="x1">The x-coordinate of the starting point of the line</param>
        /// <param name="y1">The y-coordinate of the starting point of the line</param>
        /// <param name="x2">The x-coordinate of the ending point of the line</param>
        /// <param name="y2">The y-coordinate of the ending point of the line</param>
        /// <param name="color">The color of the line</param>
        /// <param name="pixelWidth">The width of the line</param>
        public static void DrawLine(int x1, int y1, int x2, int y2, Color color, int pixelWidth)
        {
            int xDiff = x2 - x1;
            int yDiff = y2 - y1;
            Rectangle rect = new Rectangle(x1, y1, (int)Math.Round(Math.Sqrt(xDiff * xDiff +
                yDiff * yDiff)), pixelWidth);
            float angle = 0;
            if (xDiff != 0)
            {
                angle = (float)(Math.Atan(yDiff / (float)xDiff));
                if (xDiff < 0)
                    angle += (float)Math.PI;
            }
            else
            {
                if (y2 > y1)
                    angle = MathHelper.PiOver2;
                else
                    angle = MathHelper.PiOver2 * 3;
            }

            Drawable2DObject obj2d = new Drawable2DObject();
            obj2d.isText = false;
            ShapeInfo shapeInfo = new ShapeInfo();
            shapeInfo.texture = State.BlankTexture;
            shapeInfo.rect = rect;
            shapeInfo.color = color;
            shapeInfo.angle = angle;
            obj2d.shapeInfo = shapeInfo;

            queuedObjects.Add(obj2d);
        }

        /// <summary>
        /// Draws a circle centered at (x, y) with specified radius and a color.
        /// </summary>
        /// <param name="x">The x-coordinate of the center</param>
        /// <param name="y">The y-coordinate of the center</param>
        /// <param name="radius">The radius of this circle</param>
        /// <param name="color">The color of the circle</param>
        public static void DrawCircle(int x, int y, int radius, Color color)
        {
            Circle circle = new Circle(x, y, radius, false);
            Texture2D texture = null;
            Rectangle rect = new Rectangle(x - radius, y - radius, 2 * radius + 1, 2 * radius + 1);

            if (circleTextures.ContainsKey(circle))
                texture = circleTextures[circle];
            else
            {
                texture = new Texture2D(State.Device, rect.Width, rect.Height, false,
                    SurfaceFormat.Bgra5551);

                ushort[] data = new ushort[rect.Width * rect.Height];

                int i = 0;
                int j = radius;
                int d = 3 - 2 * radius;
                while (i < j)
                {
                    CirclePoints(data, i, j, rect.Width, radius);
                    if (d < 0)
                        d = d + 4 * i + 6;
                    else
                    {
                        d = d + 4 * (i - j) + 10;
                        j--;
                    }
                    i++;
                }
                if (i == j)
                    CirclePoints(data, i, j, rect.Width, radius);

                texture.SetData(data);
                circleTextures.Add(circle, texture);
            }

            Drawable2DObject obj2d = new Drawable2DObject();
            obj2d.isText = false;
            ShapeInfo shapeInfo = new ShapeInfo();
            shapeInfo.texture = texture;
            shapeInfo.rect = rect;
            shapeInfo.color = color;
            shapeInfo.angle = 0;
            obj2d.shapeInfo = shapeInfo;

            queuedObjects.Add(obj2d);
        }

        /// <summary>
        /// Draws a circle centered at the 'center' point with the specified 'radius' and a 'color'.
        /// </summary>
        /// <param name="center">The center point of the circle</param>
        /// <param name="radius">The radius of this circle</param>
        /// <param name="color">The color of the circle</param>
        public static void DrawCircle(Point center, int radius, Color color)
        {
            DrawCircle(center.X, center.Y, radius, color);
        }

        /// <summary>
        /// Fills a circlular region centered at (x, y) with the specified radius and a color.
        /// </summary>
        /// <param name="x">The x-coordinate of the center</param>
        /// <param name="y">The y-coordinate of the center</param>
        /// <param name="radius">The radius of this circle</param>
        /// <param name="color">The color of the circle</param>
        public static void FillCircle(int x, int y, int radius, Color color)
        {
            Circle circle = new Circle(x, y, radius, true);
            Texture2D texture = null;
            Rectangle rect = new Rectangle(x - radius, y - radius, 2 * radius + 1, 2 * radius + 1);

            if (circleTextures.ContainsKey(circle))
                texture = circleTextures[circle];
            else
            {
                texture = new Texture2D(State.Device, rect.Width, rect.Height, false, SurfaceFormat.Bgra5551);

                ushort[] data = new ushort[rect.Width * rect.Height];

                int i = 0;
                int j = radius;
                int d = 3 - 2 * radius;
                while (i < j)
                {
                    FillPoints(data, i, j, rect.Width, radius);
                    if (d < 0)
                        d = d + 4 * i + 6;
                    else
                    {
                        d = d + 4 * (i - j) + 10;
                        j--;
                    }
                    i++;
                }
                if (i == j)
                    FillPoints(data, i, j, rect.Width, radius);

                texture.SetData(data);
                circleTextures.Add(circle, texture);
            }

            Drawable2DObject obj2d = new Drawable2DObject();
            obj2d.isText = false;
            ShapeInfo shapeInfo = new ShapeInfo();
            shapeInfo.texture = texture;
            shapeInfo.rect = rect;
            shapeInfo.color = color;
            shapeInfo.angle = 0;
            obj2d.shapeInfo = shapeInfo;

            queuedObjects.Add(obj2d);
        }

        /// <summary>
        /// Fills a circlular region centered at the 'center' point with the specified 'radius' and a 'color'.
        /// </summary>
        /// <param name="center">The center point of the circle</param>
        /// <param name="radius">The radius of this circle</param>
        /// <param name="color">The color of the circle</param>
        public static void FillCircle(Point center, int radius, Color color)
        {
            FillCircle(center.X, center.Y, radius, color);
        }

        /// <summary>
        /// Fills a polygonal region defined by the given 'points' with the specified 'color'.
        /// </summary>
        /// <param name="points">A list of points that define the polygonal region.</param>
        /// <param name="color">The color used to fill the polygon.</param>
        public static void FillPolygon(List<Point> points, Color color)
        {
            FillPolygon(points, color, Point.Zero, PolygonShape.Nonconvex);
        }

        /// <summary>
        /// Fills a polygonal region defined by the given 'points' shifted by 'offset' with the 
        /// specified 'color'.
        /// </summary>
        /// <param name="points">A list of points that define the polygonal region.</param>
        /// <param name="color">The color used to fill the polygon.</param>
        /// <param name="offset">An offset to each point defined in 'points'.</param>
        public static void FillPolygon(List<Point> points, Color color, Point offset)
        {
            FillPolygon(points, color, offset, PolygonShape.Nonconvex);
        }

        /// <summary>
        /// Fills a polygon with the given point set, color, and offset. The 'shape' parameter is
        /// used for choosing an appropriate polygon filling algorithm.
        /// </summary>
        /// <remarks>
        /// This function still does not work for concave shapes.
        /// </remarks>
        /// <param name="points">A list of points that define the polygonal region.</param>
        /// <param name="color">The color used to fill the polygon.</param>
        /// <param name="offset">An offset to each point defined in 'points'.</param>
        /// <param name="shape">A parameter used for choosing an appropriate polygon filling algorithm.</param>
        public static void FillPolygon(List<Point> points, Color color, Point offset, PolygonShape shape)
        {
            if (points.Count < 3)
                throw new ArgumentException("It needs at least 3 points to fill a polygon");

            Texture2D texture = null;
            Point min = new Point(1000000, 1000000);
            Point max = new Point(-1000000, -1000000);

            foreach(Point p in points)
                UpdateMinMax(p, ref min, ref max);

            Polygon poly = new Polygon(points);
            Rectangle rect = new Rectangle(min.X + offset.X, min.Y + offset.Y, max.X - min.X, max.Y - min.Y);
            Point newOffset = new Point(-min.X, -min.Y);

            ushort[] data = new ushort[rect.Width * rect.Height];

            if (polyTextures.ContainsKey(poly))
                texture = polyTextures[poly];
            else if (shape == PolygonShape.Convex)
            {
                try
                {
                    FillConvexPolygon(points, newOffset, ref data, rect.Width);
                }
                catch (Exception)
                {
                    throw new GoblinException("The polygon is not a convex shape");
                }

                texture = new Texture2D(State.Device, rect.Width, rect.Height, false, SurfaceFormat.Bgra5551);
                texture.SetData(data);

                polyTextures.Add(poly, texture);
            }
            else
            {
                texture = new Texture2D(State.Device, rect.Width, rect.Height, false, SurfaceFormat.Bgra5551);

                int currentY = 0;

                if (GETPtr == null)
                {
                    GETPtr = new List<EdgeState>();
                    AETPtr = new List<EdgeState>();
                }

                // Build the global edge table
                BuildGET(points, newOffset);

                if (GETPtr.Count != 0)
                    currentY = GETPtr[0].StartY;

                EdgeXComparer edgeComparer = new EdgeXComparer();
                while ((GETPtr.Count != 0) || (AETPtr.Count != 0))
                {
                    MoveXSortedToAET(currentY);
                    ScanOutAET(currentY, ref data, rect.Width);
                    AdvanceAET();
                    AETPtr.Sort(edgeComparer);
                    currentY++;
                }

                texture.SetData(data);

                polyTextures.Add(poly, texture);

                GETPtr.Clear();
                AETPtr.Clear();
            }

            Drawable2DObject obj2d = new Drawable2DObject();
            obj2d.isText = false;
            ShapeInfo shapeInfo = new ShapeInfo();
            shapeInfo.texture = texture;
            shapeInfo.rect = rect;
            shapeInfo.color = color;
            shapeInfo.angle = 0;
            obj2d.shapeInfo = shapeInfo;

            queuedObjects.Add(obj2d);
        }

        #endregion

        #region Non-Drawing Methods

        public static void GetPolygonTexture(List<Point> points, PolygonShape shape, ref Texture2D texture)
        {
            if (points.Count < 3)
                throw new ArgumentException("It needs at least 3 points to fill a polygon");

            if (texture == null)
                throw new ArgumentException("texture must be non-null value");

            ushort[] data = new ushort[texture.Width * texture.Height];

            if (shape == PolygonShape.Convex)
            {
                try
                {
                    FillConvexPolygon(points, Point.Zero, ref data, texture.Width);
                }
                catch (Exception)
                {
                    throw new GoblinException("The polygon is not a convex shape");
                }

                texture.SetData(data);
            }
            else
            {
                int currentY = 0;

                if (GETPtr == null)
                {
                    GETPtr = new List<EdgeState>();
                    AETPtr = new List<EdgeState>();
                }

                // Build the global edge table
                BuildGET(points, Point.Zero);

                if (GETPtr.Count != 0)
                    currentY = GETPtr[0].StartY;

                EdgeXComparer edgeComparer = new EdgeXComparer();
                while ((GETPtr.Count != 0) || (AETPtr.Count != 0))
                {
                    MoveXSortedToAET(currentY);
                    ScanOutAET(currentY, ref data, texture.Width);
                    AdvanceAET();
                    AETPtr.Sort(edgeComparer);
                    currentY++;
                }

                texture.SetData(data);

                GETPtr.Clear();
                AETPtr.Clear();
            }
        }

        #endregion

        #region Public Static Text Writing Methods
        /// <summary>
        /// Draws a 2D text string on the screen
        /// </summary>
        /// <param name="pos">Position, in screen coordinates, of the upper left corner of 
        /// the first character</param>
        /// <param name="text">Text to be rendered on the screen</param>
        /// <param name="color">Color of the text</param>
        /// <param name="font">SpriteFont that defines the font family, style, size, etc</param>
        public static void WriteText(Vector2 pos, String text, Color color, SpriteFont font)
        {
            WriteText(pos, text, color, font, Vector2.One, SpriteEffects.None, Vector2.Zero,
                0, 0, GoblinEnums.HorizontalAlignment.None, GoblinEnums.VerticalAlignment.None);
        }

        /// <summary>
        /// Draws a 2D text string on the screen
        /// </summary>
        /// <param name="pos">Position, in screen coordinates, of the upper left corner of 
        /// the first character</param>
        /// <param name="text">Text to be rendered on the screen</param>
        /// <param name="color">Color of the text</param>
        /// <param name="font">SpriteFont that defines the font family, style, size, etc</param>
        /// <param name="scale">Scale to apply to the font</param>
        public static void WriteText(Vector2 pos, String text, Color color, SpriteFont font, Vector2 scale)
        {
            WriteText(pos, text, color, font, scale, SpriteEffects.None, Vector2.Zero,
                0, 0, GoblinEnums.HorizontalAlignment.None, GoblinEnums.VerticalAlignment.None);
        }

        /// <summary>
        /// Draws a 2D text string on the screen
        /// </summary>
        /// <param name="pos">Position, in screen coordinates, of the upper-left corner of 
        /// the first character</param>
        /// <param name="text">Text to be rendered on the screen</param>
        /// <param name="color">Color of the text</param>
        /// <param name="font">SpriteFont that defines the font family, style, size, etc</param>
        /// <param name="xAlign">Horizontal axis alignment of the text string (If set to other than None,
        /// then the X coordinate information of the 'pos' parameter will be ignored)</param>
        /// <param name="yAlign">Vertical axis alignment of the text string (If set to other than None,
        /// then the Y coordinate information of the 'pos' parameter will be ignored)</param>
        public static void WriteText(Vector2 pos, String text, Color color, SpriteFont font,
            GoblinEnums.HorizontalAlignment xAlign, GoblinEnums.VerticalAlignment yAlign)
        {
            WriteText(pos, text, color, font, Vector2.One, SpriteEffects.None, Vector2.Zero, 
                0, 0, xAlign, yAlign);
        }

        /// <summary>
        /// Draws a 2D text string on the screen
        /// </summary>
        /// <param name="pos">Position, in screen coordinates, of the upper-left corner of 
        /// the first character</param>
        /// <param name="text">Text to be rendered on the screen</param>
        /// <param name="color">Color of the text</param>
        /// <param name="font">SpriteFont that defines the font family, style, size, etc</param>
        /// <param name="scale">Scale to apply to the font</param>
        /// <param name="xAlign">Horizontal axis alignment of the text string (If set to other than None,
        /// then the X coordinate information of the 'pos' parameter will be ignored)</param>
        /// <param name="yAlign">Vertical axis alignment of the text string (If set to other than None,
        /// then the Y coordinate information of the 'pos' parameter will be ignored)</param>
        public static void WriteText(Vector2 pos, String text, Color color, SpriteFont font, Vector2 scale,
            GoblinEnums.HorizontalAlignment xAlign, GoblinEnums.VerticalAlignment yAlign)
        {
            WriteText(pos, text, color, font, scale, SpriteEffects.None, Vector2.Zero, 0, 0, xAlign, yAlign);
        }

        /// <summary>
        /// Draws a 2D text string on the screen
        /// </summary>
        /// <param name="pos">Position, in screen coordinates, of the left upper corner of 
        /// the first character</param>
        /// <param name="text">Text to be rendered on the screen</param>
        /// <param name="color">Color of the text</param>
        /// <param name="font">SpriteFont that defines the font family, style, size, etc</param>
        /// <param name="scale">Scale to apply to the font</param>
        /// <param name="effect">Rotations to apply prior to rendering</param>
        /// <param name="origin">Origin of the text. Specify (0, 0) for the upper-left corner</param>
        /// <param name="rotation">Angle, in radians, to rotate the text around origin</param>
        /// <param name="depth">Sorting depth of the sprite text, between 0(front) and 1(back)</param>
        /// <param name="xAlign">Horizontal axis alignment of the text string (If set to other than None,
        /// then the X coordinate information of the 'pos' parameter will be ignored)</param>
        /// <param name="yAlign">Vertical axis alignment of the text string (If set to other than None,
        /// then the Y coordinate information of the 'pos' parameter will be ignored)</param>
        public static void WriteText(Vector2 pos, String text, Color color, SpriteFont font,
            Vector2 scale, SpriteEffects effect, Vector2 origin, float rotation, float depth,
            GoblinEnums.HorizontalAlignment xAlign, GoblinEnums.VerticalAlignment yAlign)
        {
            Vector2 finalPos = new Vector2(pos.X, pos.Y);
            switch (xAlign)
            {
                case GoblinEnums.HorizontalAlignment.Left:
                    finalPos.X = origin.X;
                    break;
                case GoblinEnums.HorizontalAlignment.Center:
                    finalPos.X = (State.Width - font.MeasureString(text).X * scale.X) / 2 + origin.X;
                    break;
                case GoblinEnums.HorizontalAlignment.Right:
                    finalPos.X = State.Width - font.MeasureString(text).X * scale.X + origin.X;
                    break;
            }
            switch (yAlign)
            {
                case GoblinEnums.VerticalAlignment.Top:
                    finalPos.Y = origin.Y;
                    break;
                case GoblinEnums.VerticalAlignment.Center:
                    finalPos.Y = (State.Height - font.MeasureString(text).Y * scale.Y) / 2 + origin.Y;
                    break;
                case GoblinEnums.VerticalAlignment.Bottom:
                    finalPos.Y = State.Height - font.MeasureString(text).Y * scale.Y + origin.Y;
                    break;
            }

            Drawable2DObject obj2d = new Drawable2DObject();
            obj2d.isText = true;
            TextInfo textInfo = new TextInfo();
            textInfo.font = font;
            textInfo.text = text;
            textInfo.pos = finalPos;
            textInfo.color = color;
            textInfo.rotation = rotation;
            textInfo.origin = origin;
            textInfo.scale = scale;
            textInfo.effect = effect;
            textInfo.depth = depth;
            obj2d.textInfo = textInfo;

            queuedObjects.Add(obj2d);
        }
        #endregion

        #region Actual Drawing Function

        /// <summary>
        /// Flushes all of the accumulated 2D drawings including texts and shapes. The texts and shapes are not
        /// drawn until this function is called.
        /// </summary>
        public static void Flush(bool clear, int shiftAmount)
        {
            // Start rendering with alpha blending mode, and render back to front
            State.SharedSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Vector2 actualPos = Vector2.Zero;
            Rectangle actualRect = Rectangle.Empty;

            // First draw all of the transparent ones
            foreach (Drawable2DObject obj2d in queuedObjects)
            {
                if (obj2d.isText)
                {
                    if (obj2d.textInfo.color.A == 255)
                        opaqueObjects.Add(obj2d);
                    else
                    {
                        actualPos = obj2d.textInfo.pos;
                        if (shiftAmount != 0)
                            actualPos.X += shiftAmount;

                        State.SharedSpriteBatch.DrawString(obj2d.textInfo.font, obj2d.textInfo.text, actualPos,
                            obj2d.textInfo.color, obj2d.textInfo.rotation, obj2d.textInfo.origin,
                            obj2d.textInfo.scale, obj2d.textInfo.effect, obj2d.textInfo.depth);
                    }
                }
                else
                {
                    if (obj2d.shapeInfo.color.A == 255)
                        opaqueObjects.Add(obj2d);
                    else
                    {
                        actualRect = obj2d.shapeInfo.rect;
                        if (shiftAmount != 0)
                            actualRect.X += shiftAmount;

                        if (obj2d.shapeInfo.angle == 0)
                            State.SharedSpriteBatch.Draw(obj2d.shapeInfo.texture, actualRect, obj2d.shapeInfo.color);
                        else
                            State.SharedSpriteBatch.Draw(obj2d.shapeInfo.texture, actualRect, null, obj2d.shapeInfo.color,
                                obj2d.shapeInfo.angle, Vector2.Zero, SpriteEffects.None, 0);
                    }
                }
            }

            // Then draw the opaque ones
            foreach (Drawable2DObject obj2d in opaqueObjects)
            {
                if (obj2d.isText)
                {
                    actualPos = obj2d.textInfo.pos;
                    if (shiftAmount != 0)
                        actualPos.X += shiftAmount;

                    State.SharedSpriteBatch.DrawString(obj2d.textInfo.font, obj2d.textInfo.text, actualPos,
                        obj2d.textInfo.color, obj2d.textInfo.rotation, obj2d.textInfo.origin,
                        obj2d.textInfo.scale, obj2d.textInfo.effect, obj2d.textInfo.depth);
                }
                else
                {
                    actualRect = obj2d.shapeInfo.rect;
                    if (shiftAmount != 0)
                        actualRect.X += shiftAmount;

                    if (obj2d.shapeInfo.angle == 0)
                        State.SharedSpriteBatch.Draw(obj2d.shapeInfo.texture, actualRect, obj2d.shapeInfo.color);
                    else
                        State.SharedSpriteBatch.Draw(obj2d.shapeInfo.texture, actualRect, null, obj2d.shapeInfo.color,
                            obj2d.shapeInfo.angle, Vector2.Zero, SpriteEffects.None, 0);
                }
            }

            State.SharedSpriteBatch.End();

            if (clear)
            {
                opaqueObjects.Clear();
                queuedObjects.Clear();
            }
        }

        #endregion

        #region Helper Methods
        private static void CirclePoints(ushort[] data, int x, int y, int width, int radius)
        {
            int negX = radius - x;
            int posX = x + radius;
            int negY = radius - y;
            int posY = y + radius;
            ushort val = ushort.MaxValue;
            data[posY * width + posX] = val;
            data[posX * width + posY] = val;
            data[negX * width + posY] = val;
            data[negY * width + posX] = val;
            data[negY * width + negX] = val;
            data[negX * width + negY] = val;
            data[posX * width + negY] = val;
            data[posY * width + negX] = val;
        }

        private static void FillPoints(ushort[] data, int x, int y, int width, int radius)
        {
            int negX = radius - x;
            int posX = x + radius;
            int negY = radius - y;
            int posY = y + radius;
            ushort val = ushort.MaxValue;
            data[posY * width + posX] = val;
            data[posX * width + posY] = val;
            data[negX * width + posY] = val;
            data[negY * width + posX] = val;
            data[negY * width + negX] = val;
            data[negX * width + negY] = val;
            data[posX * width + negY] = val;
            data[posY * width + negX] = val;

            int start = (negY + 1) * width;
            int end = (posY - 1) * width;
            for (int i = start; i <= end; i += width)
            {
                data[i + posX] = val;
                data[i + negX] = val;
            }

            start = (negX + 1) * width;
            end = (posX - 1) * width;
            for (int i = start; i <= end; i += width)
            {
                data[i + posY] = ushort.MaxValue;
                data[i + negY] = ushort.MaxValue;
            }
        }

        private static void UpdateMinMax(Point p, ref Point min, ref Point max)
        {
            if (p.X < min.X)
                min.X = p.X;
            if (p.Y < min.Y)
                min.Y = p.Y;

            if (p.X > max.X)
                max.X = p.X;
            if (p.Y > max.Y)
                max.Y = p.Y;
        }

        private static void FillConvexPolygon(List<Point> points, Point offset, ref ushort[] data, int width)
        {
            int i, MinIndexL, MaxIndex, MinIndexR, SkipFirst, Temp;
            int MinPoint_Y, MaxPoint_Y, TopIsFlat, LeftEdgeDir;
            int NextIndex, CurrentIndex, PreviousIndex;
            int DeltaXN, DeltaYN, DeltaXP, DeltaYP;
            HorLineList WorkingHLineList;

            int length = points.Count;

            MaxPoint_Y = MinPoint_Y = points[MinIndexL = MaxIndex = 0].Y;
            for (i = 1; i < length; i++)
            {
                if (points[i].Y < MinPoint_Y)
                    MinPoint_Y = points[MinIndexL = i].Y; // new top
                else if (points[i].Y > MaxPoint_Y)
                    MaxPoint_Y = points[MaxIndex = i].Y; // new bottom
            }
            if (MinPoint_Y == MaxPoint_Y)
                return;  // polygon is 0-height; avoid infinite loop below

            // Scan in ascending order to find the last top-edge point
            MinIndexR = MinIndexL;
            while (points[MinIndexR].Y == MinPoint_Y)
                IndexForward(ref MinIndexR, length);
            IndexBackward(ref MinIndexR, length); // back up to last top-edge point

            // Now scan in descending order to find the first top-edge point
            while (points[MinIndexL].Y == MinPoint_Y)
                IndexBackward(ref MinIndexL, length);
            IndexForward(ref MinIndexL, length); // back up to first top-edge point

            // Figure out which direction through the vertex list from the top 
            // vertex is the left edge and which is the right
            LeftEdgeDir = -1; // assume left edge runs down thru vertex list
            if ((TopIsFlat = (points[MinIndexL].X !=
                 points[MinIndexR].X) ? 1 : 0) == 1)
            {
                // If the top is flat, just see which of the ends is leftmost
                if (points[MinIndexL].X > points[MinIndexR].X)
                {
                    LeftEdgeDir = 1;  // left edge runs up through vertex list
                    Temp = MinIndexL;       // swap the indices so MinIndexL 
                    MinIndexL = MinIndexR;  // points to the start of the left 
                    MinIndexR = Temp;       // edge, similarly for MinIndexR   
                }
            }
            else
            {
                // Point to the downward end of the first line of each of the 
                // two edges down from the top
                NextIndex = MinIndexR;
                IndexForward(ref NextIndex, length);
                PreviousIndex = MinIndexL;
                IndexBackward(ref PreviousIndex, length);
                // Calculate X and Y lengths from the top vertex to the end of 
                // the first line down each edge; use those to compare slopes 
                // and see which line is leftmost
                DeltaXN = points[NextIndex].X - points[MinIndexL].X;
                DeltaYN = points[NextIndex].Y - points[MinIndexL].Y;
                DeltaXP = points[PreviousIndex].X - points[MinIndexL].X;
                DeltaYP = points[PreviousIndex].Y - points[MinIndexL].Y;
                if (((long)DeltaXN * DeltaYP - (long)DeltaYN * DeltaXP) < 0L)
                {
                    LeftEdgeDir = 1;  // left edge runs up through vertex list
                    Temp = MinIndexL;       // swap the indices so MinIndexL
                    MinIndexL = MinIndexR;  // points to the start of the left
                    MinIndexR = Temp;       // edge, similarly for MinIndexR
                }
            }

            // Set the # of scan lines in the polygon, skipping the bottom edge 
            // and also skipping the top vertex if the top isn't flat because 
            // in that case the top vertex has a right edge component, and set 
            // the top scan line to draw, which is likewise the second line of 
            // the polygon unless the top is flat
            if ((WorkingHLineList.Length = MaxPoint_Y - MinPoint_Y - 1 + TopIsFlat) <= 0)
                return;  // there's nothing to draw, so we're done
            WorkingHLineList.YStart = offset.Y + MinPoint_Y + 1 - TopIsFlat;

            WorkingHLineList.HLinePtr = new List<HorLine>();
            for (i = 0; i < WorkingHLineList.Length; i++)
                WorkingHLineList.HLinePtr.Add(new HorLine());

            // Start from the top of the left edge
            int ptrIndex = 0;
            PreviousIndex = CurrentIndex = MinIndexL;
            // Skip the first point of the first line unless the top is flat; 
            // if the top isn't flat, the top vertex is exactly on a right 
            // edge and isn't drawn
            SkipFirst = (TopIsFlat > 0) ? 0 : 1;
            // Scan convert each line in the left edge from top to bottom
            do
            {
                IndexMove(ref CurrentIndex, length, LeftEdgeDir);
                ScanEdge(points[PreviousIndex].X + offset.X,
                      points[PreviousIndex].Y,
                      points[CurrentIndex].X + offset.X,
                      points[CurrentIndex].Y, 1, SkipFirst, ref WorkingHLineList.HLinePtr, ref ptrIndex);
                PreviousIndex = CurrentIndex;
                SkipFirst = 0; // scan convert the first point from now on
            } while (CurrentIndex != MaxIndex);

            // Scan the right edge and store the boundary points in the list
            ptrIndex = 0;
            PreviousIndex = CurrentIndex = MinIndexR;
            SkipFirst = (TopIsFlat > 0) ? 0 : 1;
            // Scan convert the right edge, top to bottom. X coordinates are 
            // adjusted 1 to the left, effectively causing scan conversion of 
            // the nearest points to the left of but not exactly on the edge
            do
            {
                IndexMove(ref CurrentIndex, length, -LeftEdgeDir);
                ScanEdge(points[PreviousIndex].X + offset.X - 1,
                      points[PreviousIndex].Y,
                      points[CurrentIndex].X + offset.X - 1,
                      points[CurrentIndex].Y, 0, SkipFirst, ref WorkingHLineList.HLinePtr, ref ptrIndex);
                PreviousIndex = CurrentIndex;
                SkipFirst = 0; // scan convert the first point from now on
            } while (CurrentIndex != MaxIndex);

            // Draw the line list representing the scan converted polygon
            FillHLineList(ref WorkingHLineList, ref data, width);
        }

        private static void BuildGET(List<Point> points, Point offset)
        {
            Point start, end, delta;
            int width;

            for (int i = 0; i < points.Count; i++)
            {
                // Calculate the edge height and width
                start.X = points[i].X + offset.X;
                start.Y = points[i].Y + offset.Y;
                // The edge runs from the current point to the previous one
                if (i == 0)
                {
                    // Wrap back around to the end of the list
                    end.X = points[points.Count - 1].X + offset.X;
                    end.Y = points[points.Count - 1].Y + offset.Y;
                }
                else
                {
                    end.X = points[i - 1].X + offset.X;
                    end.Y = points[i - 1].Y + offset.Y;
                }
                // Make sure the edge runs top to bottom
                if (start.Y > end.Y)
                {
                    Swap(ref start.X, ref end.X);
                    Swap(ref start.Y, ref end.Y);
                }
                // Skip if this can't ever be an active edge (has 0 height)
                if ((delta.Y = end.Y - start.Y) != 0)
                {
                    // Allocate space for this edge's info, and fill in the structure 
                    EdgeState newEdgePtr = new EdgeState();
                    newEdgePtr.XDirection =   // direction in which X moves
                          ((delta.X = end.X - start.X) > 0) ? 1 : -1;
                    width = Math.Abs(delta.X);
                    newEdgePtr.X = start.X;
                    newEdgePtr.StartY = start.Y;
                    newEdgePtr.Count = delta.Y;
                    newEdgePtr.ErrorTermAdjDown = delta.Y;
                    if (delta.X >= 0)  // initial error term going L->R 
                        newEdgePtr.ErrorTerm = 0;
                    else              // initial error term going R->L 
                        newEdgePtr.ErrorTerm = -delta.Y + 1;
                    if (delta.Y >= width)
                    {   // Y-major edge
                        newEdgePtr.WholePixelXMove = 0;
                        newEdgePtr.ErrorTermAdjUp = width;
                    }
                    else
                    {   // X-major edge
                        newEdgePtr.WholePixelXMove =
                              (width / delta.Y) * newEdgePtr.XDirection;
                        newEdgePtr.ErrorTermAdjUp = width % delta.Y;
                    }
                    
                    if (GETPtr.Count == 0)
                        GETPtr.Add(newEdgePtr);
                    else
                    {
                        bool foundPlaceToInsert = false;
                        for (int j = 0; j < GETPtr.Count; j++)
                        {
                            if ((GETPtr[j].StartY > start.Y) ||
                                  ((GETPtr[j].StartY == start.Y) &&
                                  (GETPtr[j].X >= start.X)))
                            {
                                GETPtr.Insert(j, newEdgePtr);
                                foundPlaceToInsert = true;
                                break;
                            }
                        }

                        if (!foundPlaceToInsert)
                            GETPtr.Add(newEdgePtr);
                    }
                } 
            }
        }

        private static void AdvanceAET()
        {
            List<EdgeState> removeList = new List<EdgeState>();
            // Count down and remove or advance each edge in the AET
            for(int i = 0; i < AETPtr.Count; i++)
            {
                // Count off one scan line for this edge
                AETPtr[i].Count = AETPtr[i].Count - 1;
                if (AETPtr[i].Count == 0)
                {
                    // This edge is finished, so remove it from the AET
                    removeList.Add(AETPtr[i]);
                }
                else
                {
                    // Advance the edge's X coordinate by minimum move
                    AETPtr[i].X += AETPtr[i].WholePixelXMove;
                    // Determine whether it's time for X to advance one extra
                    if ((AETPtr[i].ErrorTerm +=
                          AETPtr[i].ErrorTermAdjUp) > 0)
                    {
                        AETPtr[i].X += AETPtr[i].XDirection;
                        AETPtr[i].ErrorTerm -= AETPtr[i].ErrorTermAdjDown;
                    }
                } 
            }

            foreach (EdgeState edge in removeList)
                AETPtr.Remove(edge);

            removeList.Clear();
        }

        private static void MoveXSortedToAET(int YToMove)
        {
            int currentX = 0;
            // The GET is Y sorted. Any edges that start at the desired Y 
            // coordinate will be first in the GET, so we'll move edges from 
            // the GET to AET until the first edge left in the GET is no longer 
            // at the desired Y coordinate. Also, the GET is X sorted within 
            // each Y coordinate, so each successive edge we add to the AET is 
            // guaranteed to belong later in the AET than the one just added 
            
            if(GETPtr.Count == 0)
                return;

            List<EdgeState> removeList = new List<EdgeState>();
            for (int i = 0; i < GETPtr.Count; i++)
            {
                if (GETPtr[i].StartY != YToMove)
                    break;

                currentX = GETPtr[i].X;
                if (AETPtr.Count == 0)
                    AETPtr.Add(GETPtr[i]);
                else
                {
                    bool foundPlaceToInsert = false;
                    for (int j = 0; j < AETPtr.Count; j++)
                    {
                        if (AETPtr[j].X >= currentX)
                        {
                            AETPtr.Add(GETPtr[i]);
                            foundPlaceToInsert = true;
                            break;
                        }
                    }

                    if (!foundPlaceToInsert)
                        AETPtr.Add(GETPtr[i]);
                }

                removeList.Add(GETPtr[i]);
            }

            foreach (EdgeState edge in removeList)
                GETPtr.Remove(edge);

            removeList.Clear();
        }

        private static void ScanOutAET(int YToScan, ref ushort[] data, int width)
        {
            int leftX = 0;

            // Scan through the AET, drawing line segments as each pair of edge 
            // crossings is encountered. The nearest pixel on or to the right 
            // of left edges is drawn, and the nearest pixel to the left of but 
            // not on right edges is drawn
            for(int i = 0; i < AETPtr.Count - 1; i++)
            {
                leftX = AETPtr[i].X;
                FillHLineSegment(YToScan, leftX, AETPtr[i+1].X - 1, ref data, width);
            } 
        }

        private static void FillHLineSegment(int y, int leftX, int rightX, ref ushort[] data, int width)
        {
            int start = width * y + leftX;
            int end = start + rightX - leftX;
            ushort val = ushort.MaxValue;
            for (int i = start; i < end; i++)
                data[i] = val;
        }

        private static void FillHLineList(ref HorLineList lineList, ref ushort[] data, int width)
        {
            for (int i = 0; i < lineList.HLinePtr.Count; i++)
                FillHLineSegment(lineList.YStart + i, lineList.HLinePtr[i].XStart,
                    lineList.HLinePtr[i].XEnd, ref data, width);
        }

        private static void ScanEdge(int x1, int y1, int x2, int y2, int setXStart, int skipFirst,
            ref List<HorLine> hLines, ref int ptrIndex)
        {
            int DeltaX, Height, Width, AdvanceAmt, ErrorTerm, i;
            int ErrorTermAdvance, XMajorAdvanceAmt;

            // direction in which X moves (Y2 is always > Y1, so Y always counts up)
            AdvanceAmt = ((DeltaX = x2 - x1) > 0) ? 1 : -1;
            

            if ((Height = y2 - y1) <= 0)  // Y length of the edge
                return;     // guard against 0-length and horizontal edges

            // Figure out whether the edge is vertical, diagonal, X-major 
            // (mostly horizontal), or Y-major (mostly vertical) and handle 
            // appropriately
            if ((Width = Math.Abs(DeltaX)) == 0)
            {
                // The edge is vertical; special-case by just storing the same 
                // X coordinate for every scan line
                // Scan the edge for each scan line in turn 
                for (i = Height - skipFirst; i-- > 0; ptrIndex++)
                {
                    // Store the X coordinate in the appropriate edge list
                    if (setXStart == 1)
                        hLines[ptrIndex].XStart = x1;
                    else
                        hLines[ptrIndex].XEnd = x1;
                }
            }
            else if (Width == Height)
            {
                // The edge is diagonal; special-case by advancing the X 
                // coordinate 1 pixel for each scan line 
                if (skipFirst > 0) // skip the first point if so indicated
                    x1 += AdvanceAmt; // move 1 pixel to the left or right
                // Scan the edge for each scan line in turn
                for (i = Height - skipFirst; i-- > 0; ptrIndex++)
                {
                    // Store the X coordinate in the appropriate edge list
                    if (setXStart == 1)
                        hLines[ptrIndex].XStart = x1;
                    else
                        hLines[ptrIndex].XEnd = x1;
                    x1 += AdvanceAmt; // move 1 pixel to the left or right
                }
            }
            else if (Height > Width)
            {
                // Edge is closer to vertical than horizontal (Y-major)
                if (DeltaX >= 0)
                    ErrorTerm = 0; // initial error term going left->right
                else
                    ErrorTerm = -Height + 1;   // going right->left
                if (skipFirst > 0)
                {   // skip the first point if so indicated
                    // Determine whether it's time for the X coord to advance
                    if ((ErrorTerm += Width) > 0)
                    {
                        x1 += AdvanceAmt; // move 1 pixel to the left or right
                        ErrorTerm -= Height; // advance ErrorTerm to next point
                    }
                }
                // Scan the edge for each scan line in turn
                for (i = Height - skipFirst; i-- > 0; ptrIndex++)
                {
                    // Store the X coordinate in the appropriate edge list
                    if (setXStart == 1)
                        hLines[ptrIndex].XStart = x1;
                    else
                        hLines[ptrIndex].XEnd = x1;
                    // Determine whether it's time for the X coord to advance
                    if ((ErrorTerm += Width) > 0)
                    {
                        x1 += AdvanceAmt; // move 1 pixel to the left or right
                        ErrorTerm -= Height; // advance ErrorTerm to correspond
                    }
                }
            }
            else
            {
                // Edge is closer to horizontal than vertical (X-major)
                // Minimum distance to advance X each time */
                XMajorAdvanceAmt = (Width / Height) * AdvanceAmt;
                // Error term advance for deciding when to advance X 1 extra
                ErrorTermAdvance = Width % Height;
                if (DeltaX >= 0)
                    ErrorTerm = 0; // initial error term going left->right
                else
                    ErrorTerm = -Height + 1;   // going right->left
                if (skipFirst > 0)
                {   // skip the first point if so indicated
                    x1 += XMajorAdvanceAmt;    // move X minimum distance
                    // Determine whether it's time for X to advance one extra
                    if ((ErrorTerm += ErrorTermAdvance) > 0)
                    {
                        x1 += AdvanceAmt;       // move X one more
                        ErrorTerm -= Height; // advance ErrorTerm to correspond
                    }
                }
                // Scan the edge for each scan line in turn
                for (i = Height - skipFirst; i-- > 0; ptrIndex++)
                {
                    // Store the X coordinate in the appropriate edge list
                    if (setXStart == 1)
                        hLines[ptrIndex].XStart = x1;
                    else
                        hLines[ptrIndex].XEnd = x1;
                    x1 += XMajorAdvanceAmt;    // move X minimum distance
                    // Determine whether it's time for X to advance one extra
                    if ((ErrorTerm += ErrorTermAdvance) > 0)
                    {
                        x1 += AdvanceAmt;       // move X one more
                        ErrorTerm -= Height; // advance ErrorTerm to correspond
                    }
                }
            }
        }

        private static void Swap(ref int a, ref int b)
        {
            int tmp = a;
            a = b;
            b = tmp;
        }

        private static void IndexForward(ref int index, int length)
        {
            index = (index + 1) % length;
        }

        private static void IndexBackward(ref int index, int length)
        {
            index = (index - 1 + length) % length;
        }

        private static void IndexMove(ref int index, int length, int direction)
        {
            if(direction > 0)
                index = (index + 1) % length;
            else
                index = (index - 1 + length) % length;
        }
        #endregion

        #region Dispose
        internal static void Dispose()
        {
            foreach (Texture2D texture in circleTextures.Values)
                texture.Dispose();

            foreach (Texture2D texture in polyTextures.Values)
                texture.Dispose();

            circleTextures.Clear();
            polyTextures.Clear();
            queuedObjects.Clear();
        }
        #endregion

        #region Private Classes and Structs
        private class Circle
        {
            public Point Center;
            public float Radius;
            public bool Fill;

            public Circle(Point center, float radius, bool fill)
            {
                Center = center;
                Radius = radius;
                Fill = fill;
            }

            public Circle(int x, int y, float radius, bool fill)
            {
                Center = new Point(x, y);
                Radius = radius;
                Fill = fill;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Circle))
                    return false;
                else
                {
                    Circle c = (Circle)obj;
                    return (Radius == c.Radius) && (Fill == c.Fill);
                }
            }

            public override int GetHashCode()
            {
                return ((Fill) ? -1 : 1) * (int)(Radius * 1024);
            }
        }

        private class Polygon
        {
            private int hash;

            public Polygon(List<Point> points)
            {
                bool flip = true;
                float tmp = 0;
                foreach(Point p in points)
                {
                    if(flip)
                        tmp += p.X + p.Y;
                    else
                        tmp += p.X - p.Y;

                    flip = !flip;
                }

                hash = (int)tmp;
            }

            public override bool Equals(object obj)
            {
                if (obj is Polygon)
                    return obj.GetHashCode() == hash;
                else
                    return false;
            }

            public override int GetHashCode()
            {
 	             return hash;
            }
        }

        #region For Polygon Filling
        private class HorLine
        {
            public int XStart;
            public int XEnd;
        }

        private struct HorLineList
        {
            public int Length;
            public int YStart;
            public List<HorLine> HLinePtr;
        }

        private class EdgeState
        {
            public int X;
            public int StartY;
            public int WholePixelXMove;
            public int XDirection;
            public int ErrorTerm;
            public int ErrorTermAdjUp;
            public int ErrorTermAdjDown;
            public int Count;
        }

        private class EdgeXComparer : IComparer<EdgeState>
        {
            public int Compare(EdgeState x, EdgeState y)
            {
                if (x.X > y.X)
                    return 1;
                else if (x.X < y.X)
                    return -1;
                else
                    return 0;
            }
        }
        #endregion

        private struct Drawable2DObject
        {
            public bool isText;
            public TextInfo textInfo;
            public ShapeInfo shapeInfo;
        }

        private struct TextInfo
        {
            public Vector2 pos;
            public String text;
            public Color color;
            public SpriteFont font;
            public Vector2 scale;
            public SpriteEffects effect;
            public Vector2 origin;
            public float rotation;
            public float depth;
            public GoblinEnums.HorizontalAlignment xAlign;
            public GoblinEnums.VerticalAlignment yAlign;
        }

        private struct ShapeInfo
        {
            public Texture2D texture;
            public Rectangle rect;
            public Color color;
            public float angle;
        }
        #endregion
    }
}
