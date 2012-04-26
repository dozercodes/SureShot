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
    /// An implementation of a radio button, which is a button that can be selected or deselected.
    /// 
    /// If you want to have at most one radio button selected at any time within a group of radio buttons,
    /// create a ButtonGroup instance, and add those radio buttons to the ButtonGroup instance. For
    /// multiple groups, you should use multiple ButtonGroup instances.
    /// 
    /// In order to display the text next to the radio button, G2DButton.TextFont must be set. 
    /// Otherwise, the text will not show up. Note that the size of the radio button icon depends on the
    /// font size. The larger the font size, the larger the radio button icon.
    /// </summary>
    public class G2DRadioButton : ToggleButton
    {
        #region Member Fields
        /// <summary>
        /// Radius of the radio button
        /// </summary>
        protected int radius;
        /// <summary>
        /// Center position of the radio button
        /// </summary>
        protected Point center;
        /// <summary>
        /// Color used to draw the inside of the radio button
        /// </summary>
        protected Color buttonColor;
        /// <summary>
        /// Color used to highlight the inner border of the radio button when selected
        /// </summary>
        protected Color selectedColor;
        /// <summary>
        /// Color used to draw the border of the radio button
        /// </summary>
        protected Color color;

        protected Vector2 textPos;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an initially unselected radio button with the specified label.
        /// </summary>
        /// <param name="label">The text displayed on the right of the radio button icon</param>
        public G2DRadioButton(String label)
            : base(label)
        {
            radius = 6;
            drawBorder = false;
            highlightColor = Color.Yellow;
            selectedColor = Color.Green;

            color = Color.Navy;
            buttonColor = Color.White;

            center = new Point();

            name = "G2DCheckBox";

            textPos = new Vector2();
        }
        /// <summary>
        /// Creates an initially unselected radio button with an empty label.
        /// </summary>
        public G2DRadioButton() : this("") { }
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

                UpdateTextPositionAndCircle();
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

                UpdateTextPositionAndCircle();
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
                color.A = selectedColor.A = buttonColor.A = alpha;
            }
        }
        #endregion

        #region Protected Methods

        protected virtual void UpdateTextPositionAndCircle()
        {
            radius = (bounds.Height - 10) / 2;
            center.X = (int)(paintBounds.X + radius + 1);
            center.Y = (int)(paintBounds.Y + bounds.Height / 2);

            textPos.X = (int)(center.X + radius + 5);
            textPos.Y = (int)(paintBounds.Y + (bounds.Height - textHeight) / 2);
        }

        #endregion

        #region Override Methods

        protected override void HandleMouseClick(int button, Point mouseLocation)
        {
            base.HandleMouseClick(button, mouseLocation);

            if (within && enabled && visible)
                DoClick();
        }

        protected override void PaintComponent()
        {
            base.PaintComponent();

            // Draw the radio button part
            if (mouseDown && enabled)
            {
                // Use disabled color to display pressed down action
                UI2DRenderer.FillCircle(center, radius - 1, disabledColor);
            }
            else if (selected)
            {
                Color c = (enabled) ? buttonColor : disabledColor;
                UI2DRenderer.DrawCircle(center, radius - 1, c);
                UI2DRenderer.DrawCircle(center, radius - 2, c);

                UI2DRenderer.FillCircle(center, radius - 3, selectedColor);
            }
            else
            {
                Color c = (enabled) ? buttonColor : disabledColor;
                UI2DRenderer.FillCircle(center, radius - 1, c);
            }

            // Draw the border of the check box part
            UI2DRenderer.DrawCircle(center, radius, color);

            // Show the highlight when mouse is over
            if (enabled && within && !mouseDown)
            {
                UI2DRenderer.DrawCircle(center, radius - 1, highlightColor);
                UI2DRenderer.DrawCircle(center, radius - 2, highlightColor);
            }

            if (label.Length > 0 && textFont != null)
                UI2DRenderer.WriteText(textPos, label, textColor, textFont);
        }
        #endregion
    }
}
