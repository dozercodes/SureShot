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
    /// A separator is a divider line that, typically used to separate regions on a panel by drawing
    /// a line.
    /// </summary>
    public class G2DSeparator : G2DComponent
    {
        #region Member Fields
        /// <summary>
        /// Orietation of the separator
        /// </summary>
        protected GoblinEnums.Orientation orientation;
        /// <summary>
        /// Thickness of the separator (This variable replaces bounds.Height if drawn horizontally
        /// or bounds.Width if drawn vertically)
        /// </summary>
        protected int thickness;
        /// <summary>
        /// Indicator of whether to adjust the length of this separator to either its parent's width
        /// or height depending on the orientation if it has a parent
        /// </summary>
        protected bool adjustLength;
        /// <summary>
        /// Upper-left corner of the separator
        /// </summary>
        protected Point p1;
        /// <summary>
        /// Lower-right corner of the separator
        /// </summary>
        protected Point p2;
        protected Rectangle rect;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a divider line with the specified thickness and orientation.
        /// </summary>
        /// <param name="thickness">The thickness of the line</param>
        /// <param name="adjustLength">Indicates whether to adjust the length of this divider line to 
        /// either its parent's width or height depending on the orientation. If false, its own Bounds
        /// information is used to decide the length</param>
        /// <param name="orientation">The orientation of the line</param>
        public G2DSeparator(int thickness, bool adjustLength, GoblinEnums.Orientation orientation)
            : base()
        {
            backgroundColor = Color.DarkBlue;
            borderColor = Color.Black;

            this.orientation = orientation;
            this.thickness = thickness;
            this.adjustLength = adjustLength;

            name = "G2DSeparator";

            p1 = new Point();
            p2 = new Point();
            rect = new Rectangle();

            SetDrawingPoints();
        }

        /// <summary>
        /// Creates a divider line with thickness of 3 pixels and specified orientation.
        /// </summary>
        /// <param name="adjustLength">Indicates whether to adjust the length of this divider line to 
        /// either its parent's width or height depending on the orientation. If false, its own Bounds
        /// information is used to decide the length</param>
        /// <param name="orientation">The orientation of the line</param>
        public G2DSeparator(bool adjustLength, GoblinEnums.Orientation orientation)
            : this(3, adjustLength, orientation) { }

        /// <summary>
        /// Creates a horizontal divider line with thickness of 3 pixels. 
        /// </summary>
        /// <param name="adjustLength">Indicates whether to adjust the length of this divider line to 
        /// either its parent's width or height depending on the orientation. If false, its own Bounds
        /// information is used to decide the length</param>
        public G2DSeparator(bool adjustLength) : this(3, adjustLength, GoblinEnums.Orientation.Horizontal) { }

        /// <summary>
        /// Creaes a horizontal divider line with thickness of 3 pixels.
        /// The length of the line is not adjusted to its parent.
        /// </summary>
        public G2DSeparator() : this(3, false, GoblinEnums.Orientation.Horizontal) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the drawing orientation.
        /// </summary>
        public virtual GoblinEnums.Orientation Orientation
        {
            get { return orientation; }
            set
            {
                orientation = value;
                SetDrawingPoints();
            }
        }
        /// <summary>
        /// Gets or sets the thickness of the divider line (This variable replaces bounds.Height 
        /// if drawn horizontally or bounds.Width if drawn vertically)
        /// </summary>
        public virtual int SeparatorThickness
        {
            get { return thickness; }
            set
            {
                thickness = value;
                SetDrawingPoints();
            }
        }
        /// <summary>
        /// Gets or sets whether to adjust the length of this divider line to either its parent's width or height
        /// depending on the orientation if it has a parent.
        /// </summary>
        public virtual bool AdjustLength
        {
            get { return adjustLength; }
            set
            {
                adjustLength = value;
                SetDrawingPoints();
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
                SetDrawingPoints();
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
                SetDrawingPoints();
            }
        }
        #endregion

        #region Override Methods

        protected override void PaintBorder()
        {
            if (!visible)
                return;

            UI2DRenderer.DrawRectangle(rect, borderColor, 1);
        }
        protected override void PaintComponent()
        {
            if (!visible)
                return;

            UI2DRenderer.FillRectangle(rect, null, backgroundColor);
        }
        #endregion

        #region Inner Class Methods
        /// <summary>
        /// Set the drawing points of the separator
        /// </summary>
        protected virtual void SetDrawingPoints()
        {
            if (orientation == GoblinEnums.Orientation.Horizontal)
            {
                if (adjustLength && HasParent)
                {
                    p1.X = paintBounds.X - bounds.X + 5;
                    p2.X = p1.X + ((G2DComponent)parent).Bounds.Width - 10;
                }
                else
                {
                    p1.X = paintBounds.X;
                    p2.X = p1.X + bounds.Width;
                }
                p1.Y = paintBounds.Y + (bounds.Height - thickness) / 2;
                p2.Y = p1.Y + thickness;
            }
            else
            {
                if (adjustLength && HasParent)
                {
                    p1.Y = paintBounds.Y - bounds.Y + 5;
                    p2.Y = p1.Y + ((G2DComponent)parent).Bounds.Height - 10;
                }
                else
                {
                    p1.Y = paintBounds.Y;
                    p2.Y = p1.Y + bounds.Height;
                }
                p1.X = paintBounds.X + (bounds.Width - thickness) / 2;
                p2.X = p1.X + thickness;
            }

            rect.X = p1.X;
            rect.Y = p1.Y;
            rect.Width = p2.X - p1.X;
            rect.Height = p2.Y - p1.Y;
        }
        #endregion
    }
}
