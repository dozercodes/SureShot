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
    /// IMPLEMENTATION NOT FINISHED YET.
    /// </summary>
    internal class G2DComboBox : G2DComponent
    {
        #region Member Fields

        protected int selectedIndex;
        protected int maxRowCount;
        protected G2DList contentList;

        #region For drawing pull down button
        protected List<Point> downArrowPoints;
        protected Rectangle buttonBounds;
        #endregion

        #endregion

        #region Events

        /// <summary>
        /// An event triggered whenever an item state changes.
        /// </summary>
        public event ItemStateChanged ItemStateChangedEvent;

        #endregion

        #region Constructors
        public G2DComboBox(object[] items) 
            : base()
        {
            selectedIndex = 0;
            maxRowCount = 10;
            contentList = new G2DList(items);

            downArrowPoints = new List<Point>();
            buttonBounds = Rectangle.Empty;

            name = "G2DComboBox";
        }

        public G2DComboBox() : this(null) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the maximum number of rows this combo box displays.
        /// </summary>
        public virtual int MaximumRowCount
        {
            get { return maxRowCount; }
            set { maxRowCount = value; }
        }

        /// <summary>
        /// Gets or sets the selected index in the combo box list.
        /// </summary>
        public virtual int SelectedIndex
        {
            get { return selectedIndex; }
            set 
            {
                if (value < 0 || value >= contentList.Model.Elements.Count)
                    return;

                if (value != selectedIndex)
                {
                    if (ItemStateChangedEvent != null)
                    {
                        ItemStateChangedEvent(this, contentList.Model.Elements[selectedIndex], false);
                        ItemStateChangedEvent(this, contentList.Model.Elements[value], true);
                    }

                    selectedIndex = value;
                }
            }
        }

        public virtual ListCellRenderer CellRenderer
        {
            get { return contentList.CellRenderer; }
            set { contentList.CellRenderer = value; }
        }

        public virtual ListModel DataModel
        {
            get { return contentList.Model; }
            set { contentList.Model = value; }
        }

        public virtual Object SelectedItem
        {
            get { return contentList.Model.Elements[selectedIndex]; }
            set
            {
                if (value != null && contentList.Model.Elements.Contains(value))
                {
                    int val = contentList.Model.Elements.IndexOf(value);
                    SelectedIndex = val;
                }
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
                contentList.BackgroundColor = value;
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
                contentList.DisabledColor = value;
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
                contentList.BorderColor = value;
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
                contentList.DrawBackground = value;
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
                contentList.DrawBorder = value;
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
                contentList.Enabled = value;
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
                contentList.Focused = value;
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
                contentList.HorizontalAlignment = value;
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
                contentList.VerticalAlignment = value;
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
                contentList.Transparency = value;
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
                contentList.TextFont = value;
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
                contentList.TextColor = value;
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
                contentList.TextTransparency = value;
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Causes the combo box to display its popup window.
        /// </summary>
        public void ShowPopup()
        {
            contentList.Visible = true;
        }

        #endregion

        #region Protected Methods

        protected void InvokeItemStateChangedEvent(object source, object item, bool selected)
        {
            if (ItemStateChangedEvent != null)
                ItemStateChangedEvent(source, item, selected);
        }

        #endregion

        #region Override Methods

        protected override void HandleMousePress(int button, Point mouseLocation)
        {
            base.HandleMousePress(button, mouseLocation);

            if (!enabled || !visible)
                return;
        }

        protected override void PaintComponent()
        {
            base.PaintComponent();
        }

        #endregion
    }
}
