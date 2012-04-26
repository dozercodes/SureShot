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

using GoblinXNA.Helpers;

// 3D Text rendering library from http://nuclexframework.codeplex.com/
// Nuclex.Fonts used here is modified a little bit to better suite Goblin
// framework, and compiled using XNA Game Studio 3.1 instead of 3.0 
using Nuclex.Fonts;

namespace GoblinXNA.UI.UI3D
{
    /// <summary>
    /// A helper class for performing 3D drawing, such as drawing 3D lines and 3D texts.
    /// </summary>
    public class UI3DRenderer
    {
        /// <summary>
        /// An enum that specifies the display style of the 3D text.
        /// </summary>
        /// <remarks>
        /// This is different from the font style (e.g., Plain, Italic) which is defined in the .spritefont file.
        /// </remarks>
        public enum Text3DStyle 
        { 
            /// <summary>
            /// A style that outlines the text string.
            /// </summary>
            Outline, 
            /// <summary>
            /// A style that fills the front face of the text string.
            /// </summary>
            Fill, 
            /// <summary>
            /// A style that extrudes the text string.
            /// </summary>
            Extrude
        };

        private static int MAX_NUM_INDICES = 20000;

        #region Static Member Fields
        private static TextBatch textBatch;
        private static List<Text3DInfo> queued3DTexts;
        private static List<Line3DInfo> queued3DLines;
        #endregion

        #region 3D Text Drawing Functions

        /// <summary>
        /// Draws a 3D text with the given font, style, color, and transformation.
        /// </summary>
        /// <remarks>
        /// This 3D text won't be actually drawn until Flush() method is called. If this method is called before
        /// base.Draw(...) in your main Game class's Draw(...) function, then it's automatically flushed when
        /// base.Draw(...) is called. If this method is called after base.Draw(...) function, then you need to
        /// call Flush() function after calling one or more of this function. Otherwise, the 3D texts drawing will
        /// be deferred until the next base.Draw(...) or Flush() call.
        /// </remarks>
        /// <param name="text">Text string to be displayed in 3D.</param>
        /// <param name="font">Font to use for the 3D text.</param>
        /// <param name="style">3D text style (Outline, Fill, or Extrude).</param>
        /// <param name="color">Color of the 3D text.</param>
        /// <param name="transform">Transformation of the left-bottom corner of the 3D text.</param>
        public static void Write3DText(String text, VectorFont font, Text3DStyle style, Color color,
            Matrix transform)
        {
            Write3DText(text, font, style, color, transform, GoblinEnums.HorizontalAlignment.None,
                GoblinEnums.VerticalAlignment.None);
        }

        /// <summary>
        /// Draws a 3D text with the given font, style, color, transformation, horizontal align, and vertical
        /// align.
        /// </summary>
        /// <remarks>
        /// This 3D text won't be actually drawn until Flush() method is called. If this method is called before
        /// base.Draw(...) in your main Game class's Draw(...) function, then it's automatically flushed when
        /// base.Draw(...) is called. If this method is called after base.Draw(...) function, then you need to
        /// call Flush() function after calling one or more of this function. Otherwise, the 3D texts drawing will
        /// be deferred until the next base.Draw(...) or Flush() call.
        /// </remarks>
        /// <param name="text">Text string to be displayed in 3D.</param>
        /// <param name="font">Font to use for the 3D text.</param>
        /// <param name="style">3D text style (Outline, Fill, or Extrude).</param>
        /// <param name="color">Color of the 3D text.</param>
        /// <param name="transform">Transformation of the 3D text.</param>
        /// <param name="hAlign">The horizontal (x-axis) shifting</param>
        /// <param name="vAlign"></param>
        public static void Write3DText(String text, VectorFont font, Text3DStyle style, Color color, 
            Matrix transform, GoblinEnums.HorizontalAlignment hAlign, GoblinEnums.VerticalAlignment vAlign)
        {
            if (queued3DTexts == null)
                queued3DTexts = new List<Text3DInfo>();

            Text3DInfo textInfo = new Text3DInfo();
            Text text3d = null;

            if (font == null)
                throw new ArgumentException("'font' must be non-null value");

            switch (style)
            {
                case Text3DStyle.Outline:
                    text3d = font.Outline(text);
                    break;
                case Text3DStyle.Fill:
                    text3d = font.Fill(text);
                    break;
                case Text3DStyle.Extrude:
                    text3d = font.Extrude(text);
                    break;
            }

            textInfo.text3d = text3d;
            textInfo.color = color;

            Matrix shiftTransform = Matrix.Identity;

            switch (hAlign)
            {
                case GoblinEnums.HorizontalAlignment.None:
                case GoblinEnums.HorizontalAlignment.Left:
                    // The default is aligned to left, so nothing to do
                    break;
                case GoblinEnums.HorizontalAlignment.Center:
                    shiftTransform.Translation -= Vector3.UnitX * text3d.Width / 2;
                    break;
                case GoblinEnums.HorizontalAlignment.Right:
                    shiftTransform.Translation -= Vector3.UnitX * text3d.Width;
                    break;
            }

            switch (vAlign)
            {
                case GoblinEnums.VerticalAlignment.None:
                case GoblinEnums.VerticalAlignment.Bottom:
                    // The default is aligned to bottom, so nothing to do
                    break;
                case GoblinEnums.VerticalAlignment.Center:
                    shiftTransform.Translation -= Vector3.UnitY * text3d.Height / 2;
                    break;
                case GoblinEnums.VerticalAlignment.Top:
                    shiftTransform.Translation -= Vector3.UnitY * text3d.Height;
                    break;
            }

            shiftTransform *= MatrixHelper.GetRotationMatrix(transform);
            transform.Translation += shiftTransform.Translation;
            textInfo.transform = transform;

            queued3DTexts.Add(textInfo);
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes
        /// </summary>
        public static void Dispose()
        {
            if (queued3DTexts != null)
                queued3DTexts.Clear();

            if (queued3DLines != null)
                queued3DLines.Clear();

            if (textBatch != null)
                textBatch.Dispose();
        }

        #endregion

        #region Actual Drawing Function

        /// <summary>
        /// Performs batch drawing of the queued 3D texts and lines.
        /// </summary>
        public static void Flush(bool clear)
        {
            if (queued3DTexts == null)
                return;

            if (textBatch == null)
                textBatch = new TextBatch(State.Device);

            if (queued3DTexts.Count > 0)
            {
                textBatch.ViewProjection = State.ViewMatrix * State.ProjectionMatrix;
                int numIndices = 0;
                Text3DInfo info;
                for (int i = 0; i < queued3DTexts.Count; i++)
                {
                    info = queued3DTexts[i];

                    if(numIndices == 0)
                        textBatch.Begin();

                    textBatch.DrawText(info.text3d, info.transform, info.color);

                    numIndices += info.text3d.Indices.Length;

                    if (numIndices > MAX_NUM_INDICES)
                    {
                        textBatch.End();
                        numIndices = 0;
                    }
                }

                if (numIndices != 0)
                    textBatch.End();

                if(clear)
                    queued3DTexts.Clear();
            }
        }

        #endregion

        #region Private Structs

        private struct Text3DInfo
        {
            public Text text3d;
            public Color color;
            public Matrix transform;
        }

        private struct Line3DInfo
        {
        }

        #endregion
    }
}
