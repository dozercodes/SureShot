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

using GoblinXNA;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// This class can be used to control the viewing area of a scrollable object.
    /// </summary>
    public class G2DScrollBar : G2DComponent
    {
        #region Member Fields

        protected G2DButton upLeftButton;
        protected G2DButton downRightButton;
        protected G2DSlider scrollBar;

        protected Texture2D upLeftTexture;
        protected Texture2D downRightTexture;
        protected Color buttonColor;

        protected GoblinEnums.Orientation orientation;

        protected int unitIncrement;
        protected int blockIncrement;
        protected int extent;

        #endregion

        #region Events

        /// <summary>
        /// An event triggered whenever the state of the scroll bar changes.
        /// </summary>
        public event StateChanged StateChangedEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a scroll bar with the specified orientation, value, extent, and maximum.
        /// </summary>
        /// <param name="orientation"></param>
        /// <param name="value"></param>
        /// <param name="extent">The size of the viewable area (a.k.a visible amount)</param>
        /// <param name="max"></param>
        public G2DScrollBar(GoblinEnums.Orientation orientation, int value, int extent, int max)
            : base()
        {
            if (value < 0)
                throw new ArgumentException("value has to be greater than or equal to 0");

            if (extent > max)
                throw new ArgumentException("extent must be smaller than max");

            this.orientation = orientation;
            this.extent = extent;

            upLeftButton = new G2DButton();
            downRightButton = new G2DButton();

            if (orientation == GoblinEnums.Orientation.Horizontal)
            {
                upLeftButton.ActionPerformedEvent += new ActionPerformed(DecrementUnit);
                downRightButton.ActionPerformedEvent += new ActionPerformed(IncrementUnit);
            }
            else
            {
                upLeftButton.ActionPerformedEvent += new ActionPerformed(IncrementUnit);
                downRightButton.ActionPerformedEvent += new ActionPerformed(DecrementUnit);
            }

            scrollBar = new G2DSlider(0, max - extent, value);
            scrollBar.Orientation = orientation;
            scrollBar.PaintTrack = false;
            scrollBar.SnapToTicks = true;
            scrollBar.BackgroundColor = Color.White;
            scrollBar.StateChangedEvent += new StateChanged(ValueChanged);

            buttonColor = Color.DarkBlue;

            unitIncrement = 1;
            blockIncrement = 1;

            scrollBar.MinorTickSpacing = blockIncrement;
        }

        public G2DScrollBar(GoblinEnums.Orientation orientation) : this(orientation, 0, 8, 10) { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the orientation of this scroll bar.
        /// </summary>
        public GoblinEnums.Orientation Orientation
        {
            get { return orientation; }
        }

        /// <summary>
        /// Gets or sets the size of the viewable area (Note that this is different from the Extent property
        /// of G2DSlider).
        /// </summary>
        public virtual int Extent
        {
            get { return extent; }
            set 
            {
                if (value > (extent + scrollBar.Maximum))
                    throw new GoblinException("Extent must be smaller than Maximum");

                scrollBar.Maximum = (extent + scrollBar.Maximum) - value;
                extent = value;
                scrollBar.KnobLength = (int)((extent / (float)(extent + scrollBar.Maximum)) *
                    ((orientation == GoblinEnums.Orientation.Vertical) ? scrollBar.Bounds.Height : scrollBar.Bounds.Width));
            }
        }

        public virtual int Maximum
        {
            get { return extent + scrollBar.Maximum; }
            set
            {
                if (value < extent)
                    throw new GoblinException("Maximum must be greater than Extent");

                scrollBar.Maximum = value - extent;
                scrollBar.KnobLength = (int)((extent / (float)(extent + scrollBar.Maximum)) *
                    ((orientation == GoblinEnums.Orientation.Vertical) ? scrollBar.Bounds.Height : scrollBar.Bounds.Width));
            }
        }

        public virtual int Value
        {
            get { return scrollBar.Value; }
            set { scrollBar.Value = value; }
        }

        /// <summary>
        /// Gets or sets the amount of increment/decrement when the up/left or down/right button is pressed.
        /// </summary>
        public virtual int UnitIncrement
        {
            get { return unitIncrement; }
            set { unitIncrement = value; }
        }

        /// <summary>
        /// Gets or sets the amount of increment/decrement when the scrollbar is clicked (not on the scrollbar knob)
        /// </summary>
        public virtual int BlockIncrement
        {
            get { return blockIncrement; }
            set 
            { 
                blockIncrement = value;
                scrollBar.MinorTickSpacing = blockIncrement;
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
                AdjustBounds();

                CreateDefaultButtonArrows();

                upLeftButton.Texture = upLeftTexture;
                downRightButton.Texture = downRightTexture;
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
                AdjustBounds();
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

                upLeftButton.Transparency = value;
                scrollBar.Transparency = value;
                downRightButton.Transparency = value;
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

                upLeftButton.Enabled = value;
                scrollBar.Enabled = value;
                downRightButton.Enabled = value;
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;

                upLeftButton.Visible = value;
                scrollBar.Visible = value;
                downRightButton.Visible = value;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void AdjustBounds()
        {
            if (orientation == GoblinEnums.Orientation.Vertical)
            {
                upLeftButton.Bounds = new Rectangle(paintBounds.X, paintBounds.Y, bounds.Width, bounds.Width);
                scrollBar.Bounds = new Rectangle(paintBounds.X, paintBounds.Y + upLeftButton.Bounds.Height,
                    bounds.Width, bounds.Height - bounds.Width * 2);
                downRightButton.Bounds = new Rectangle(paintBounds.X, paintBounds.Y + upLeftButton.Bounds.Height +
                    scrollBar.Bounds.Height, bounds.Width, bounds.Width);
            }
            else
            {
                upLeftButton.Bounds = new Rectangle(paintBounds.X, paintBounds.Y, bounds.Height, bounds.Height);
                scrollBar.Bounds = new Rectangle(paintBounds.X + upLeftButton.Bounds.Width, paintBounds.Y,
                    bounds.Width - bounds.Height * 2, bounds.Height);
                downRightButton.Bounds = new Rectangle(paintBounds.X + upLeftButton.Bounds.Width + scrollBar.Bounds.Width, 
                    paintBounds.Y, bounds.Height, bounds.Height);
            }

            scrollBar.KnobLength = (int)((extent / (float)(extent + scrollBar.Maximum)) *
                ((orientation == GoblinEnums.Orientation.Vertical) ? scrollBar.Bounds.Height : scrollBar.Bounds.Width));
        }

        protected virtual void CreateDefaultButtonArrows()
        {
            upLeftTexture = new Texture2D(State.Device, upLeftButton.Bounds.Width, upLeftButton.Bounds.Height, false, SurfaceFormat.Bgra5551);
            downRightTexture = new Texture2D(State.Device, downRightButton.Bounds.Width, downRightButton.Bounds.Height, false, SurfaceFormat.Bgra5551);

            int sixth = downRightButton.Bounds.Width / 6;
            List<Point> points = new List<Point>();

            upLeftButton.TextureColor = buttonColor;
            downRightButton.TextureColor = buttonColor;

            if (orientation == GoblinEnums.Orientation.Vertical)
            {
                points.Add(new Point(upLeftButton.Bounds.Width / 2, sixth));
                points.Add(new Point(upLeftButton.Bounds.Width - sixth, upLeftButton.Bounds.Height - sixth));
                points.Add(new Point(sixth, upLeftButton.Bounds.Height - sixth));

                UI2DRenderer.GetPolygonTexture(points, UI2DRenderer.PolygonShape.Convex, ref upLeftTexture);

                points.Clear();
                points.Add(new Point(sixth, sixth));
                points.Add(new Point(downRightButton.Bounds.Width - sixth, sixth));
                points.Add(new Point(downRightButton.Bounds.Width / 2, downRightButton.Bounds.Height - sixth));

                UI2DRenderer.GetPolygonTexture(points, UI2DRenderer.PolygonShape.Convex, ref downRightTexture);
            }
            else
            {
                points.Add(new Point(sixth, upLeftButton.Bounds.Height / 2));
                points.Add(new Point(upLeftButton.Bounds.Width - sixth, sixth));
                points.Add(new Point(upLeftButton.Bounds.Width - sixth, upLeftButton.Bounds.Height - sixth));

                UI2DRenderer.GetPolygonTexture(points, UI2DRenderer.PolygonShape.Convex, ref upLeftTexture);

                points.Clear();
                points.Add(new Point(sixth, sixth));
                points.Add(new Point(downRightButton.Bounds.Width - sixth, downRightButton.Bounds.Height / 2));
                points.Add(new Point(sixth, downRightButton.Bounds.Height - sixth));

                UI2DRenderer.GetPolygonTexture(points, UI2DRenderer.PolygonShape.Convex, ref downRightTexture);
            }
        }

        internal virtual void IncrementUnit(object source)
        {
            scrollBar.Value += unitIncrement;
        }

        internal virtual void DecrementUnit(object source)
        {
            scrollBar.Value -= unitIncrement;
        }

        protected virtual void ValueChanged(object source)
        {
            if (StateChangedEvent != null)
                StateChangedEvent(this);
        }

        #endregion

        #region Override Methods

        internal override void RegisterMouseInput()
        {
            base.RegisterMouseInput();

            upLeftButton.RegisterMouseInput();
            scrollBar.RegisterMouseInput();
            downRightButton.RegisterMouseInput();
        }

        internal override void RemoveMouseInput()
        {
            base.RemoveMouseInput();

            upLeftButton.RemoveMouseInput();
            scrollBar.RemoveMouseInput();
            downRightButton.RemoveMouseInput();
        }

        public override void RenderWidget()
        {
            base.RenderWidget();

            upLeftButton.RenderWidget();
            scrollBar.RenderWidget();
            downRightButton.RenderWidget();
        }

        #endregion
    }
}
