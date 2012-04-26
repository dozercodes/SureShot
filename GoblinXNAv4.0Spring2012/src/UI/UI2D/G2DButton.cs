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
using Microsoft.Xna.Framework.Input;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.Device.Generic;
using GoblinXNA.Helpers;

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// An implementation of a "push" button. 
    /// 
    /// In order to display the text on the button, G2DButton.TextFont must be set. 
    /// Otherwise, the text will not show up.
    /// </summary>
    public class G2DButton : AbstractButton
    {
        #region Member Fields
        /// <summary>
        /// Color used for highlighting the inner border when focused
        /// </summary>
        protected Color focusedColor;
        /// <summary>
        /// Indicator of whether the 'click' key is held down
        /// </summary>
        protected bool clickKeyPressed;
        /// <summary>
        /// Key used to perform mouse click action when focused
        /// </summary>
        protected Keys clickKey;

        protected Vector2 textPos;

        protected Rectangle highlightBound;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a "push" button with the specified text.
        /// </summary>
        /// <param name="label">The text displayed on the button</param>
        public G2DButton(String label)
            : base(label)
        {
            this.label = label;
            borderColor = Color.Navy;
            focusedColor = Color.Blue;

            horizontalAlignment = GoblinEnums.HorizontalAlignment.Center;
            verticalAlignment = GoblinEnums.VerticalAlignment.Center;

            clickKeyPressed = false;
            clickKey = Keys.Enter;

            name = "G2DButton";

            textPos = new Vector2();
            highlightBound = new Rectangle();
        }
        /// <summary>
        /// Creates a "push" button with no text.
        /// </summary>
        public G2DButton() : this("") { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the key used to perform mouse click action when focused
        /// </summary>
        public virtual Keys ClickKey
        {
            get { return clickKey; }
            set { clickKey = value; }
        }
        /// <summary>
        /// Gets whether the 'click' key is held down
        /// </summary>
        public virtual bool ClickKeyPressed
        {
            get { return clickKeyPressed; }
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

                UpdateTextPosition();
                UpdateHighlightBorderBounds();
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
                UpdateHighlightBorderBounds();
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

        public override float Transparency
        {
            get
            {
                return base.Transparency;
            }
            set
            {
                base.Transparency = value;
                focusedColor.A = alpha;
            }
        }

        public override Texture2D Texture
        {
            get
            {
                return base.Texture;
            }
            set
            {
                base.Texture = value;
                drawBorder = true;
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
                        y += (int)(paintBounds.Height - textHeight) - 1;
                        break;
                }

                textPos.X = x;
                textPos.Y = y;
            }
        }

        private void UpdateHighlightBorderBounds()
        {
            highlightBound = new Rectangle(paintBounds.X + 1, paintBounds.Y + 1,
                paintBounds.Width - 2, paintBounds.Height - 2);
        }

        #endregion

        #region Override Methods

        protected override void HandleMouseClick(int button, Point mouseLocation)
        {
            base.HandleMouseClick(button, mouseLocation);

            if(within && enabled && visible)
                DoClick();
        }
        protected override void HandleKeyPress(Keys key, KeyModifier modifier)
        {
            base.HandleKeyPress(key, modifier);

            if (focused && enabled && visible && (key == clickKey))
                clickKeyPressed = true;
        }
        protected override void HandleKeyRelease(Keys key, KeyModifier modifier)
        {
            base.HandleKeyRelease(key, modifier);

            if (clickKeyPressed)
            {
                DoClick();
                clickKeyPressed = false;
            }
        }

        protected override void PaintBorder()
        {
            base.PaintBorder();
        }

        protected override void PaintComponent()
        {
            if ((mouseDown && enabled) || (focused && clickKeyPressed))
            {
                // Use disabled color to display pressed down action
                UI2DRenderer.FillRectangle(paintBounds, backTexture, disabledColor);
            }
            else
                base.PaintComponent();

            if (label.Length > 0)
                if(textFont != null)
                    UI2DRenderer.WriteText(textPos, label, textColor, textFont);

            Color color = ColorHelper.Empty;
            // Add highlight on the border if mouse is hovering
            if (enabled && within && !mouseDown)
                color = highlightColor;
            else if (focused)
                color = focusedColor;

            if (!color.Equals(ColorHelper.Empty))
                UI2DRenderer.DrawRectangle(highlightBound, color, 2);
        }
        #endregion
    }
}
