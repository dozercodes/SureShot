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

using GoblinXNA.Graphics;
using GoblinXNA.Device.Generic;

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// A text field displays a text string based on the user's keyboard input. This component
    /// needs to be focused in order to receive the keyboard input.
    /// 
    /// In order to display the text and the caret, G2DTextField.TextFont must be set. 
    /// Otherwise, the text and caret will not show up.
    /// </summary>
    public class G2DTextField : TextComponent
    {
        #region Member Fields

        protected Vector2 textPos;
        protected Rectangle focusRect;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a text field with specified initial text string and the width of the text field.
        /// </summary>
        /// <param name="text">An initial text string</param>
        /// <param name="columns">The width of the text field that is used to determine
        /// how many characters this text field can display</param>
        public G2DTextField(String text, int columns)
            : base(text, columns)
        {
            name = "G2DTextField";
            backgroundColor = Color.White;

            textPos = new Vector2();
        }

        /// <summary>
        /// Creates a text field with specified initial text and column width of 30.
        /// </summary>
        /// <param name="text">An initial text string</param>
        public G2DTextField(String text) : this(text, 0) { }

        /// <summary>
        /// Creates a text field with specified column width.
        /// </summary>
        /// <param name="columns"></param>
        public G2DTextField(int columns) : this("", columns) { }

        /// <summary>
        /// Creates a text field with column width of 30.
        /// </summary>
        public G2DTextField() : this("", 0) { }
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
                UpdateFocusBound();
                UpdateCaretPosition();
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
                UpdateFocusBound();
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
                UpdateCaretPosition();
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
                UpdateCaretPosition();
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
                        x += (int)(paintBounds.Width - textWidth - 4);
                        break;
                }

                switch (verticalAlignment)
                {
                    case GoblinEnums.VerticalAlignment.Top:
                        y += (int)(bounds.Height - textHeight) / 2;
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

        protected virtual void UpdateCaretPosition()
        {
            switch (horizontalAlignment)
            {
                case GoblinEnums.HorizontalAlignment.None:
                case GoblinEnums.HorizontalAlignment.Left:
                    caretPosition.X = (int)(textWidth + 4);
                    break;
                case GoblinEnums.HorizontalAlignment.Center:
                    caretPosition.X = (int)((bounds.Width + textWidth) / 2);
                    break;
                case GoblinEnums.HorizontalAlignment.Right:
                    caretPosition.X = bounds.Width - 4;
                    break;
            }

            InvokeCaretUpdate(this);
        }

        protected virtual void UpdateFocusBound()
        {
            focusRect = new Rectangle(paintBounds.X + 1, paintBounds.Y + 1, paintBounds.Width - 3, 
                paintBounds.Height - 3);
        }

        #endregion

        #region Override Methods

        protected override void UpdateCaret(Keys key, KeyModifier modifier)
        {
            UpdateCaretPosition();
        }

        public override void Clear()
        {
            base.Clear();

            UpdateCaretPosition();
        }

        protected override void UpdateText(Keys key, KeyModifier modifier)
        {
            if (textFont == null)
                return;

            switch (key)
            {
                case Keys.Back:
                case Keys.Delete:
                    if (label.Length > 0)
                        Text = label.Substring(0, label.Length - 1);
                    break;
                case Keys.Enter:
                    break;
                default:
                    bool handle = true;
                    if (columns > 0)
                    {
                        if (label.Length >= columns)
                            handle = false;
                    }
                    else
                        if (textWidth >= (bounds.Width - 8))
                            handle = false;
                    
                    if(handle)
                        Text += "" + KeyboardInput.KeyToChar(key, modifier.ShiftKeyPressed);
                    break;
            }

            if(Text.Length != 0)
                textHeight = (int)textFont.MeasureString(Text).Y;
        }

        protected override void PaintComponent()
        {
            base.PaintComponent();

            // Show the highlight when mouse is over
            if (focused)
                UI2DRenderer.DrawRectangle(focusRect, focusedColor, 2);

            if (label.Length > 0 && textFont != null)
                UI2DRenderer.WriteText(textPos, label, textColor, textFont);
        }
        #endregion
    }
}
