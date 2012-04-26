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

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// A display area for a text string. A label does not react to input events such as 
    /// keyboard or mouse events. You can specify the location of the label with respect to the display
    /// area by setting the horizontal and vertical alignment.
    /// 
    /// In order to display the text, G2DLabel.TextFont must be set. Otherwise, the text will not show up.
    /// </summary>
    public class G2DLabel : G2DComponent
    {
        #region Member Fields

        protected Vector2 textPos;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a display area for a short text string with the specified text. 
        /// </summary>
        /// <param name="label">A text to be displayed</param>
        public G2DLabel(String label) 
            : base()
        {
            drawBorder = false;
            name = "G2DLabel";
            Text = label;

            drawBackground = false;
            textPos = new Vector2();
        }
        /// <summary>
        /// Creates a display area for a short text string with an empty text.
        /// </summary>
        public G2DLabel() : this("") { }
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

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;

                UpdateTextPosition();
            }
        }

        public override GoblinEnums.HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return base.HorizontalAlignment;
            }
            set
            {
                base.HorizontalAlignment = value;

                UpdateTextPosition();
            }
        }

        public override GoblinEnums.VerticalAlignment VerticalAlignment
        {
            get
            {
                return base.VerticalAlignment;
            }
            set
            {
                base.VerticalAlignment = value;

                UpdateTextPosition();
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void UpdateTextPosition()
        {
            if (label.Length > 0)
            {
                int x = paintBounds.X, y = paintBounds.Y;
                switch (horizontalAlignment)
                {
                    case GoblinEnums.HorizontalAlignment.Left:
                        x += 4;
                        break;
                    case GoblinEnums.HorizontalAlignment.Center:
                        x += (int)(paintBounds.Width - textWidth) / 2;
                        break;
                    case GoblinEnums.HorizontalAlignment.Right:
                        x += (int)(paintBounds.Width - textWidth);
                        break;
                }

                switch (verticalAlignment)
                {
                    case GoblinEnums.VerticalAlignment.Top:
                        y += 1;
                        break;
                    case GoblinEnums.VerticalAlignment.Center:
                        y += (int)(paintBounds.Height - textHeight) / 2;
                        break;
                    case GoblinEnums.VerticalAlignment.Bottom:
                        y += (int)(paintBounds.Height - textHeight - 1);
                        break;
                }

                textPos.X = x;
                textPos.Y = y;
            }
        }

        #endregion

        #region Override Methods
        protected override void PaintComponent()
        {
            base.PaintComponent();

            if (label.Length > 0)
            {
                if(textFont != null)
                    UI2DRenderer.WriteText(textPos, label, textColor, textFont);
            }
        }
        #endregion
    }
}
