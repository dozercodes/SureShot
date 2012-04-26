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

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// A single line input field that lets the user select a number or an object from an ordered sequence. It provides
    /// a pair of tiny arrow buttons for stepping through the elements of the sequence. The keyboard up/down arrow keys
    /// also cycle through the elements. The value in the spinner is not directly editable.
    /// </summary>
    public class G2DSpinner : G2DComponent
    {
        #region Member Fields
        protected SpinnerModel model;

        #region Internal handling
        /// <summary>
        /// When the mouse is held down on either the up or down arrow, it should increment or
        /// decrement the value. But before taking the action, wait for some press intervals
        /// for avoiding unintended multiple increments or decrements
        /// </summary>
        protected int initialPressInterval;
        protected int pressInterval;
        protected int pressCount;
        protected bool initialPress;
        protected bool upPressed;
        protected bool downPressed;
        protected bool heldDown;
        #endregion

        #region Only for drawing purpose
        protected G2DTextField textField;
        protected Rectangle upArrowBound;
        protected Rectangle downArrowBound;
        protected Color highlightColor;
        protected Color arrowColor;
        protected Color buttonColor;
        protected List<Point> upArrowPoints;
        protected List<Point> downArrowPoints;
        #endregion
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a spinner with a SpinnerIntegerModel with initial value of 0 and no minimum or maximum limits.
        /// </summary>
        public G2DSpinner() : this(new SpinnerIntegerModel()) { }

        /// <summary>
        /// Creates a custom spinner with a given SpinnerModel.
        /// </summary>
        /// <param name="model"></param>
        public G2DSpinner(SpinnerModel model)
            : base()
        {
            this.model = model;
            initialPressInterval = 20;
            pressInterval = 5;
            pressCount = 0;
            initialPress = true;
            textField = new G2DTextField(""+model.Value);
            textField.HorizontalAlignment = GoblinEnums.HorizontalAlignment.Right;
            textField.Editable = false;

            upArrowBound = new Rectangle();
            downArrowBound = new Rectangle();

            upArrowPoints = new List<Point>();
            downArrowPoints = new List<Point>();

            upPressed = downPressed = false;
            heldDown = false;

            buttonColor = Color.Turquoise;
            highlightColor = new Color((byte)0x99, (byte)255, (byte)255, (byte)255);
            arrowColor = Color.Navy;

            name = "G2DSpinner";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the SpinnerModel for this spinner.
        /// </summary>
        public virtual SpinnerModel Model
        {
            get { return model; }
            set { model = value; }
        }

        /// <summary>
        /// Gets or sets the color of the buttons that hold up and down arrows.
        /// </summary>
        public Color ButtonColor
        {
            get { return buttonColor; }
            set
            {
                buttonColor = new Color(value.R, value.G, value.B, alpha);
            }
        }

        /// <summary>
        /// Gets or sets the color of the up and down arrows.
        /// </summary>
        public Color ArrowColor
        {
            get { return arrowColor; }
            set
            {
                arrowColor = new Color(value.R, value.G, value.B, alpha);
            }
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

                int halfHeight = paintBounds.Height / 2;
                textField.Bounds = new Rectangle(paintBounds.X, paintBounds.Y, 
                    paintBounds.Width - halfHeight * 2, paintBounds.Height);
                upArrowBound = new Rectangle(paintBounds.X + paintBounds.Width - halfHeight * 2, paintBounds.Y,
                    halfHeight * 2, halfHeight);
                downArrowBound = new Rectangle(paintBounds.X + paintBounds.Width - halfHeight * 2,
                    paintBounds.Y + halfHeight, halfHeight * 2, halfHeight);

                upArrowPoints.Clear();
                downArrowPoints.Clear();

                int quatY = halfHeight / 6;
                int halfY = halfHeight / 2;
                int fourThirdY = upArrowBound.Height - quatY;

                int quatX = halfHeight / 3;
                int halfX = halfHeight;
                int fourThirdX = upArrowBound.Width - quatX;

                upArrowPoints.Add(new Point(upArrowBound.X + halfX, upArrowBound.Y + quatY - 1));
                upArrowPoints.Add(new Point(upArrowBound.X + fourThirdX, upArrowBound.Y + fourThirdY - 1));
                upArrowPoints.Add(new Point(upArrowBound.X + quatX, upArrowBound.Y + fourThirdY - 1));

                downArrowPoints.Add(new Point(downArrowBound.X + quatX + 1, downArrowBound.Y + quatY + 1));
                downArrowPoints.Add(new Point(downArrowBound.X + fourThirdX - 1, downArrowBound.Y + quatY + 1));
                downArrowPoints.Add(new Point(downArrowBound.X + halfX, downArrowBound.Y + fourThirdY + 1));
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

                int halfHeight = paintBounds.Height / 2;
                textField.Bounds = new Rectangle(paintBounds.X, paintBounds.Y,
                    paintBounds.Width - halfHeight * 2, paintBounds.Height);
                upArrowBound = new Rectangle(paintBounds.X + paintBounds.Width - halfHeight * 2, paintBounds.Y,
                    halfHeight * 2, halfHeight);
                downArrowBound = new Rectangle(paintBounds.X + paintBounds.Width - halfHeight * 2,
                    paintBounds.Y + halfHeight, halfHeight * 2, halfHeight);

                upArrowPoints.Clear();
                downArrowPoints.Clear();

                int quatY = halfHeight / 6;
                int halfY = halfHeight / 2;
                int fourThirdY = upArrowBound.Height - quatY;

                int quatX = halfHeight / 3;
                int halfX = halfHeight;
                int fourThirdX = upArrowBound.Width - quatX;

                upArrowPoints.Add(new Point(upArrowBound.X + halfX, upArrowBound.Y + quatY - 1));
                upArrowPoints.Add(new Point(upArrowBound.X + fourThirdX, upArrowBound.Y + fourThirdY - 1));
                upArrowPoints.Add(new Point(upArrowBound.X + quatX, upArrowBound.Y + fourThirdY - 1));

                downArrowPoints.Add(new Point(downArrowBound.X + quatX + 1, downArrowBound.Y + quatY + 1));
                downArrowPoints.Add(new Point(downArrowBound.X + fourThirdX - 1, downArrowBound.Y + quatY + 1));
                downArrowPoints.Add(new Point(downArrowBound.X + halfX, downArrowBound.Y + fourThirdY + 1));
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return base.BackgroundColor;
            }
            set
            {
                base.BackgroundColor = value;
                textField.BackgroundColor = value;
            }
        }

        public override Color DisabledColor
        {
            get
            {
                return base.DisabledColor;
            }
            set
            {
                base.DisabledColor = value;
                textField.DisabledColor = value;
            }
        }

        public override Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
                textField.BorderColor = value;
            }
        }

        public override bool DrawBackground
        {
            get
            {
                return base.DrawBackground;
            }
            set
            {
                base.DrawBackground = value;
                textField.DrawBackground = value;
            }
        }

        public override bool DrawBorder
        {
            get
            {
                return base.DrawBorder;
            }
            set
            {
                base.DrawBorder = value;
                textField.DrawBorder = value;
            }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                textField.Enabled = value;
            }
        }

        public override bool Focused
        {
            get
            {
                return base.Focused;
            }
            internal set
            {
                base.Focused = value;
                textField.Focused = value;
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
                textField.HorizontalAlignment = value;
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
                textField.VerticalAlignment = value;
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
                textField.Transparency = value;
                highlightColor.A = arrowColor.A = buttonColor.A = alpha;
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
                textField.TextFont = value;
            }
        }

        public override Color TextColor
        {
            get
            {
                return base.TextColor;
            }
            set
            {
                base.TextColor = value;
                textField.TextColor = value;
            }
        }

        public override float TextTransparency
        {
            get
            {
                return base.TextTransparency;
            }
            set
            {
                base.TextTransparency = value;
                textField.TextTransparency = value;
            }
        }
        #endregion

        #region Action Methods
        /// <summary>
        /// Programmatically perform 'up arrow' click.
        /// </summary>
        public virtual void DoNextClick()
        {
            model.Value = model.NextValue;
            textField.Text = "" + model.Value;
        }
        /// <summary>
        /// Programmatically perform 'down arrow' click.
        /// </summary>
        public virtual void DoPreviousClick()
        {
            model.Value = model.PreviousValue;
            textField.Text = "" + model.Value;
        }
        #endregion

        #region Inner Class Methods
        protected virtual void CheckMouseHeldDown()
        {
            if (heldDown)
            {
                if (initialPress)
                {
                    if (pressCount > initialPressInterval)
                    {
                        pressCount = 0;
                        if (upPressed)
                            DoNextClick();
                        else if (downPressed)
                            DoPreviousClick();
                        initialPress = false;
                    }
                }
                else
                {
                    if (pressCount > pressInterval)
                    {
                        pressCount = 0;
                        if (upPressed)
                            DoNextClick();
                        else if (downPressed)
                            DoPreviousClick();
                    }
                }

                pressCount++;
            }
        }

        protected virtual void CheckArrowPress(Point location)
        {
            if (UI2DHelper.IsWithin(location, upArrowBound)){
                DoNextClick();
                upPressed = true;
                heldDown = true;
            }
            else if (UI2DHelper.IsWithin(location, downArrowBound)){
                DoPreviousClick();
                downPressed = true;
                heldDown = true;
            }
        }
        #endregion

        #region Override Methods
        protected override void HandleMousePress(int button, Point mouseLocation)
        {
            base.HandleMousePress(button, mouseLocation);

            if (within && enabled && visible)
                CheckArrowPress(mouseLocation);
        }

        protected override void HandleMouseRelease(int button, Point mouseLocatione)
        {
            base.HandleMouseRelease(button, mouseLocatione);

            pressCount = 0;
            initialPress = true;
            upPressed = downPressed = false;
            heldDown = false;
        }

        protected override void HandleMouseWheel(int delta, int value)
        {
            base.HandleMouseWheel(delta, value);

            if (!focused || !enabled || !visible)
                return;

            bool up = (delta > 0);
            if (up)
                for (int i = 0; i < delta; i++)
                    DoNextClick();
            else
                for (int i = 0; i < -(delta); i++)
                    DoPreviousClick();
        }

        protected override void PaintComponent()
        {
            CheckMouseHeldDown();

            textField.RenderWidget();

            // Render the up and down buttons
            Color c = (!enabled || upPressed) ? disabledColor : buttonColor;
            UI2DRenderer.FillRectangle(upArrowBound, null, c);
            c = (!enabled || downPressed) ? disabledColor : buttonColor;
            UI2DRenderer.FillRectangle(downArrowBound, null, c);

            // Render the border of the up and down button
            UI2DRenderer.DrawRectangle(upArrowBound, borderColor, 1);
            UI2DRenderer.DrawRectangle(downArrowBound, borderColor, 1);

            // Render the up and down arrows
            // First render the up arrow
            if (upArrowPoints.Count > 0)
            {
                UI2DRenderer.FillPolygon(upArrowPoints, arrowColor, Point.Zero, UI2DRenderer.PolygonShape.Convex);
                UI2DRenderer.FillPolygon(downArrowPoints, arrowColor, Point.Zero, UI2DRenderer.PolygonShape.Convex);
            }
        }
        #endregion
    }
}
