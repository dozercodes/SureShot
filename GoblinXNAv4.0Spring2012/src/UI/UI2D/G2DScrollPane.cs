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

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// IMPLEMENTATION NOT FINISHED YET.
    /// </summary>
    internal class G2DScrollPane : G2DComponent
    {
        #region Enums

        public enum ScrollBarPolicy { Never, AsNeeded, Always }

        #endregion

        #region Member Fields

        protected G2DScrollBar hScrollBar;
        protected G2DScrollBar vScrollBar;
        protected G2DComponent scrollableContent;

        protected ScrollBarPolicy hBarPolicy;
        protected ScrollBarPolicy vBarPolicy;

        protected bool enableWheelScroll;

        protected bool showVScroll;
        protected bool showHScroll;

        #endregion 

        #region Constructors

        public G2DScrollPane(G2DComponent scrollableContent, ScrollBarPolicy horizontalPolicy, ScrollBarPolicy verticalPolicy)
        {
            if (!(scrollableContent is Scrollable))
                throw new ArgumentException("The scrollableContent has to implement Scrollable interface");

            this.scrollableContent = scrollableContent;

            enableWheelScroll = true;

            drawBackground = false;

            hBarPolicy = horizontalPolicy;
            vBarPolicy = verticalPolicy;

            hScrollBar = new G2DScrollBar(GoblinEnums.Orientation.Horizontal, 0, 2, 2);
            vScrollBar = new G2DScrollBar(GoblinEnums.Orientation.Vertical, 2, 2, 2);

            if (vBarPolicy == ScrollBarPolicy.Always)
                showVScroll = true;
            else
                showVScroll = false;

            if (hBarPolicy == ScrollBarPolicy.Always)
                showHScroll = true;
            else
                showHScroll = false;
        }

        public G2DScrollPane(G2DComponent scrollableContent) 
            : this(scrollableContent, ScrollBarPolicy.Never, ScrollBarPolicy.AsNeeded) { }

        #endregion

        #region Properties

        public virtual ScrollBarPolicy HorizontalScrollBarPolicy
        {
            get { return hBarPolicy; }
            set
            {
                hBarPolicy = value;

                if (hBarPolicy == ScrollBarPolicy.Always)
                    showHScroll = true;
                else
                    showHScroll = false;

                AdjustBounds();
            }
        }

        public virtual ScrollBarPolicy VerticalScrollBarPolicy
        {
            get { return vBarPolicy; }
            set
            {
                vBarPolicy = value;

                if (vBarPolicy == ScrollBarPolicy.Always)
                    showVScroll = true;
                else
                    showVScroll = false;

                AdjustBounds();
            }
        }

        public G2DScrollBar HorizontalScrollBar
        {
            get { return hScrollBar; }
        }

        public G2DScrollBar VerticalScrollBar
        {
            get { return vScrollBar; }
        }

        public virtual bool EnableWheelScroll
        {
            get { return enableWheelScroll; }
            set { enableWheelScroll = value; }
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
                vScrollBar.Transparency = value;
                hScrollBar.Transparency = value;
                scrollableContent.Transparency = value;
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
                vScrollBar.Enabled = value;
                hScrollBar.Enabled = value;
                scrollableContent.Enabled = value;
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
                vScrollBar.Visible = value;
                hScrollBar.Visible = value;
                scrollableContent.Visible = value;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void AdjustBounds()
        {
            AdjustScrollBarBounds();

            int width = (showVScroll) ? bounds.Width - vScrollBar.Bounds.Width : bounds.Width;
            width = (int)Math.Min(width, ((Scrollable)scrollableContent).PreferredScrollableViewportSize.X);

            int height = (showHScroll) ? bounds.Height - hScrollBar.Bounds.Height : bounds.Height;
            height = (int)Math.Min(height, ((Scrollable)scrollableContent).PreferredScrollableViewportSize.Y);

            scrollableContent.Bounds = new Rectangle(paintBounds.X, paintBounds.Y, width, height);
        }

        protected virtual void AdjustScrollBarBounds()
        {
            int barSize = 24;

            if (vBarPolicy != ScrollBarPolicy.Never)
            {
                int height = (showHScroll) ? bounds.Height - barSize : bounds.Height;
                vScrollBar.Bounds = new Rectangle(paintBounds.X + bounds.Width - barSize, paintBounds.Y, barSize, height);
            }

            if (hBarPolicy != ScrollBarPolicy.Never)
            {
                int width = (showVScroll) ? bounds.Width - barSize : bounds.Width;
                hScrollBar.Bounds = new Rectangle(paintBounds.X, paintBounds.Y + bounds.Width - barSize, width, barSize);
            }
        }

        #endregion

        #region Override Methods

        internal override void RegisterMouseInput()
        {
            base.RegisterMouseInput();

            vScrollBar.RegisterMouseInput();
            hScrollBar.RegisterMouseInput();
            scrollableContent.RegisterMouseInput();
        }

        internal override void RemoveMouseInput()
        {
            base.RemoveMouseInput();

            vScrollBar.RemoveMouseInput();
            hScrollBar.RemoveMouseInput();
            scrollableContent.RemoveMouseInput();
        }

        protected override void HandleMouseWheel(int delta, int value)
        {
            base.HandleMouseWheel(delta, value);

            if (!enableWheelScroll || !showVScroll || !visible || !enabled || !within)
                return;

            if (delta > 0)
                vScrollBar.IncrementUnit(null);
            else
                vScrollBar.DecrementUnit(null);
        }

        public override void RenderWidget()
        {
            base.RenderWidget();

            scrollableContent.RenderWidget();

            if (showVScroll)
                vScrollBar.RenderWidget();

            if (showHScroll)
                hScrollBar.RenderWidget();
        }

        #endregion
    }
}
