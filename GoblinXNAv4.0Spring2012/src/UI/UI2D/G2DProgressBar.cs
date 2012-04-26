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
    /// A G2D component that displays an integer value within a bounded interval. This component is 
    /// typically used to express the progress of some task by displaying the percentage completed
    /// and optionally, a textual display of the percentage.
    /// 
    /// In order to display the percentage completed text, G2DProgressBar.TextFont must be set. 
    /// Otherwise, the text will not show up.
    /// </summary>
    public class G2DProgressBar : G2DComponent
    {
        #region Member Fields

        protected bool indeterminate;
        protected GoblinEnums.Orientation orientation;
        protected bool paintString;
        protected int min;
        protected int max;
        protected int value;
        protected Color barColor;
        protected Color stringColor;

        // for drawing the indeterminate progress bar
        protected int indeterminateCounter;

        #endregion

        #region Events

        /// <summary>
        /// An event triggered whenever the state of the progress bar changes.
        /// </summary>
        public event StateChanged StateChangedEvent;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a progress bar with the specified orientation, and minimum and maximum values.
        /// </summary>
        /// <param name="orientation">The orientation of the progress bar</param>
        /// <param name="min">The minimum value of the progress bar</param>
        /// <param name="max">The maximum value of the progress bar</param>
        public G2DProgressBar(GoblinEnums.Orientation orientation, int min, int max)
            : base()
        {
            this.orientation = orientation;
            if(min >= max)
                throw new GoblinException("Minimum value has to be less than maximum value");

            this.min = min;
            this.max = max;

            name = "G2DProgressBar";
            backgroundColor = Color.White;

            indeterminate = false;
            paintString = false;
            value = min;

            barColor = new Color(198, 226, 255);
            stringColor = new Color(16, 78, 139);
        }

        /// <summary>
        /// Creates a horizontal progress bar with minimum value of 0 and maximum value of 100.
        /// </summary>
        /// <param name="min">The minimum value of the progress bar</param>
        /// <param name="max">The maximum value of the progress bar</param>
        public G2DProgressBar(int min, int max) : 
            this(GoblinEnums.Orientation.Horizontal, min, max) { }

        /// <summary>
        /// Creates a progress bar with the specified orientation, and minimum value of 0 and 
        /// maximum value of 100.
        /// </summary>
        /// <param name="orientation">The orientation of the progress bar</param>
        public G2DProgressBar(GoblinEnums.Orientation orientation) :
            this(orientation, 0, 100) { }

        /// <summary>
        /// Creates a horizontal progress bar with minimum value of 0 and maximum value of 100.
        /// </summary>
        public G2DProgressBar() :
            this(GoblinEnums.Orientation.Horizontal, 0, 100) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets whether to use indeterminate mode. Indeterminate mode is used to indicate
        /// that a task of unknown length is running. While the bar is in indeterminate mode, it 
        /// animates constantly to show that some tasks are being executed.
        /// </summary>
        public virtual bool Indeterminate
        {
            get { return indeterminate; }
            set { indeterminate = value; }
        }

        /// <summary>
        /// Gets or sets the desired orientation of the progress bar
        /// </summary>
        public virtual GoblinEnums.Orientation Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        /// <summary>
        /// Gets or sets the minimum value of the progress bar
        /// </summary>
        public virtual int Minimum
        {
            get { return min; }
            set 
            {
                if (value >= max)
                    throw new GoblinException("Minimum value has to be less than maximum value");

                min = value; 
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the progress bar
        /// </summary>
        public virtual int Maximum
        {
            get { return max; }
            set 
            {
                if (value <= min)
                    throw new GoblinException("Maximum value has to be larger than minimum value");

                max = value; 
            }
        }

        /// <summary>
        /// Gets or sets the progress bar's current value
        /// </summary>
        public virtual int Value
        {
            get { return value; }
            set 
            {
                if (value < min)
                    this.value = min;
                else if (value > max)
                    this.value = max;
                else
                    this.value = value;

                InvokeStateChangedEvent(this);
            }
        }

        /// <summary>
        /// Gets or sets whether to paint the percentage complete text message in the middle of 
        /// the progress bar if the progress bar is not indeterminate
        /// </summary>
        public virtual bool PaintString
        {
            get { return paintString; }
            set { paintString = value; }
        }

        /// <summary>
        /// Gets or sets the color of the progress bar
        /// </summary>
        public virtual Color BarColor
        {
            get { return barColor; }
            set { barColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of the text message that shows percentage completed
        /// </summary>
        public virtual Color StringColor
        {
            get { return stringColor; }
            set { stringColor = value; }
        }

        /// <summary>
        /// Gets the percentage completed. If indeterminate, this will return -1.
        /// </summary>
        public virtual double PercentComplete
        {
            get 
            {
                if (indeterminate)
                    return -1;
                else
                    return value / (double)(max - min) * 100; 
            }
        }
        #endregion

        #region Override Properties

        public override float Transparency
        {
            get
            {
                return base.Transparency;
            }
            set
            {
                base.Transparency = value;

                barColor.A = stringColor.A = alpha;
            }
        }
        #endregion

        #region Protected Methods

        /// <summary>
        /// Invokes the state changed event.
        /// </summary>
        /// <param name="source">The class that invoked this method.</param>
        protected void InvokeStateChangedEvent(object source)
        {
            if (StateChangedEvent != null)
                StateChangedEvent(source);
        }

        #endregion

        #region Override Methods

        protected override void PaintComponent()
        {
            base.PaintComponent();

            // paint the progress bar
            if (indeterminate)
            {
                int x = 0;
                int gap = paintBounds.Width / 12;
                int bigGap = gap * 2;

                if (indeterminateCounter > 80)
                    indeterminateCounter = 0;

                if (indeterminateCounter > 40)
                {
                    UI2DRenderer.FillRectangle(new Rectangle(paintBounds.X, paintBounds.Y + 1,
                        (int)((indeterminateCounter - 40) / 40f * gap), (paintBounds.Height - 2)),
                        null, barColor);

                    int w = (int)((80 - indeterminateCounter) / 40f * gap);
                    UI2DRenderer.FillRectangle(new Rectangle(paintBounds.X + paintBounds.Width - w,
                        paintBounds.Y + 1, w, (paintBounds.Height - 2)), null, barColor);
                }
                else
                {
                    x = (int)((indeterminateCounter / 80f + 5) * bigGap);
                    UI2DRenderer.FillRectangle(new Rectangle(paintBounds.X + x, paintBounds.Y + 1,
                        gap, (paintBounds.Height - 2)), null, barColor);
                }

                for (int i = 0; i < 5; i++)
                {
                    x = (int)((indeterminateCounter / 80f + i) * bigGap);
                    UI2DRenderer.FillRectangle(new Rectangle(paintBounds.X + x, paintBounds.Y + 1,
                        gap, (paintBounds.Height - 2)), null, barColor);
                }

                indeterminateCounter++;
            }
            else
            {
                UI2DRenderer.FillRectangle(new Rectangle(paintBounds.X + 1, paintBounds.Y + 1,
                    (int)(PercentComplete / 100 * (paintBounds.Width - 2)), paintBounds.Height - 2), 
                    null, barColor);
            }

            // paint the percent complete
            if (!indeterminate && paintString && textFont != null)
            {
                String msg = (int)PercentComplete + "%";
                Vector2 pos = new Vector2();
                Vector2 msgMeasure = textFont.MeasureString(msg);
                pos.X = paintBounds.X + (int)(paintBounds.Width - msgMeasure.X) / 2;
                pos.Y = paintBounds.Y + (int)(paintBounds.Height - msgMeasure.Y) / 2;

                UI2DRenderer.WriteText(pos, msg, stringColor, textFont);
            }
        }

        #endregion
    }
}
