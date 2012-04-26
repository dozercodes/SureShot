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

namespace GoblinXNA.UI.UI2D.Fancy
{
    /// <summary>
    /// An animated cycling bar that can be used to indicate busy wait. An optional 
    /// text message can be displayed with the wait animation. 
    /// </summary>
    public class G2DWaitBar : G2DComponent
    {
        #region Member Fields

        protected int animationCounter;
        protected Color animationBarColor;
        protected Line[] lines;
        protected int lineWidth;
        protected Vector2 textPos;
        protected List<Line> linesToDraw;
        protected int curTail;
        protected ushort updateInterval;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an animated cycling bar without any text messages.
        /// </summary>
        public G2DWaitBar() : this("") { }

        /// <summary>
        /// Creates an animated cycling bar to indicate busy wait with a text message.
        /// </summary>
        /// <param name="label">A text message to be displayed with the wait animation.</param>
        public G2DWaitBar(String label)
            : base()
        {
            this.label = label;
            animationCounter = 0;

            animationBarColor = Color.DarkGray;
            updateInterval = 6;

            lines = null;
            lineWidth = 1;
            textPos = new Vector2();
            linesToDraw = new List<Line>();

            name = "G2DWaitBar";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the color of the circling animation bars.
        /// </summary>
        public virtual Color AnimationBarColor
        {
            get { return animationBarColor; }
            set { animationBarColor = value; }
        }

        /// <summary>
        /// Gets or sets the update interval (speed) of the circling animation. The smaller the value is,
        /// the faster the update will be. The default update interval is 6.
        /// </summary>
        public virtual ushort UpdateInterval
        {
            get { return updateInterval; }
            set { updateInterval = value; }
        }

        #endregion

        #region Override Properties

        public override Rectangle Bounds
        {
            get
            {
                return base.Bounds;
            }
            set
            {
                base.Bounds = value;

                UpdateAnimationPoints();
                UpdateTextPosition();
            }
        }

        public override Component Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                base.Parent = value;

                UpdateAnimationPoints();
                UpdateTextPosition();
            }
        }

        public override SpriteFont TextFont
        {
            get
            {
                return base.TextFont;
            }
            set
            {
                base.TextFont = value;

                UpdateTextPosition();
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void UpdateAnimationPoints()
        {
            if(bounds.Height <= 40)
                lines = new Line[8];
            else
                lines = new Line[12];

            float innerRadius = bounds.Height / 8.0f;
            float outerRadius = bounds.Height / 2.0f - innerRadius;
            float angleGap = MathHelper.Pi * 2 / lines.Length;
            float angle = 0;
            double cos = 0, sin = 0;

            Vector2 center = new Vector2();
            center.X = paintBounds.X + bounds.Height / 2 + 2;
            center.Y = paintBounds.Y + bounds.Height / 2;

            for (int i = 0; i < lines.Length; i++)
            {
                angle = angleGap * i;
                cos = (float)Math.Cos(angle);
                sin = (float)Math.Sin(angle);
                lines[i] = new Line();
                lines[i].Start.X = (int)Math.Round(center.X + innerRadius * sin);
                lines[i].Start.Y = (int)Math.Round(center.Y - innerRadius * cos);

                lines[i].End.X = (int)Math.Round(center.X + outerRadius * sin);
                lines[i].End.Y = (int)Math.Round(center.Y - outerRadius * cos);
            }

            lineWidth = (int)(Math.Max((outerRadius - innerRadius) / 6, 2));

            linesToDraw.Clear();
            for (int i = 0; i < lines.Length / 2; i++)
                linesToDraw.Add(lines[i]);
            curTail = lines.Length / 2;
        }

        protected virtual void UpdateTextPosition()
        {
            textPos.X = paintBounds.X + bounds.Height + 6;
            textPos.Y = paintBounds.Y + (int)((paintBounds.Height - textHeight) / 2);
        }

        #endregion

        #region Override Methods

        protected override void PaintComponent()
        {
            base.PaintComponent();

            if (lines == null)
                return;

            Color c = animationBarColor;
            c.A = 255;
            for (int i = 0; i < linesToDraw.Count; i++)
            {
                if (alpha < 255 && linesToDraw.Count > 4)
                    c.A = (byte)((i + 1) / 6.0f * alpha);
                UI2DRenderer.DrawLine(linesToDraw[i].Start, linesToDraw[i].End, c, lineWidth);
            }

            if (textFont != null && label.Length > 0)
                UI2DRenderer.WriteText(textPos, label, textColor, textFont);

            animationCounter++;

            if (animationCounter > updateInterval)
            {
                linesToDraw.RemoveAt(0);
                linesToDraw.Add(lines[curTail]);
                curTail++;
                if (curTail >= lines.Length)
                    curTail = 0;

                animationCounter = 0;
            }
        }

        #endregion

        #region Protected Class

        protected class Line
        {
            public Point Start;
            public Point End;
        }

        #endregion
    }
}
