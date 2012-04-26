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

using GoblinXNA.Device.Generic;

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// A display area that renders a list of objects. This allows the user to select one or more objects from the
    /// list. A separate model, ListModel, represents the contens of the list. The rendering of each list element
    /// is handled by a ListCellRenderer implementation.
    /// </summary>
    public class G2DList : G2DComponent, Scrollable
    {
        #region Member Fields

        protected ListModel listModel;
        protected ListSelectionModel selectionModel;
        protected ListCellRenderer cellRenderer;

        protected Color selectionBackgroundColor;
        protected Color selectionForegroundColor;

        protected KeyboardState keyState;
        protected List<Keys> pressedKeys;

        protected Vector2 preferredViewportSize;
        protected bool scrollableTracksViewportHeight;
        protected bool scrollableTracksViewportWidth;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a list that displays the elements in the specified model.
        /// </summary>
        /// <param name="listModel"></param>
        public G2DList(ListModel listModel)
            : base()
        {
            if (listModel == null)
                throw new ArgumentException("listModel has to be non-null value");

            this.listModel = listModel;

            selectionModel = new DefaultListSelectionModel();
            cellRenderer = new DefaultListCellRenderer();

            backgroundColor = Color.White;

            selectionBackgroundColor = Color.SkyBlue;
            selectionForegroundColor = Color.White;

            pressedKeys = new List<Keys>();

            scrollableTracksViewportHeight = false;
            scrollableTracksViewportWidth = false;

            name = "G2DList";
        }

        /// <summary>
        /// Creates a list that displays the elements in the specified array.
        /// </summary>
        /// <param name="listData"></param>
        public G2DList(object[] listData)
            : this(new DefaultListModel(listData))
        { 
        }

        /// <summary>
        /// Creates a list with an empty model.
        /// </summary>
        public G2DList()
            : this(new DefaultListModel())
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the list data model. The default model is DefaultListModel.
        /// </summary>
        public virtual ListModel Model
        {
            get { return listModel; }
            set { listModel = value; }
        }

        /// <summary>
        /// Gets or sets the selection model for this list. The default model is DefaultListSelectionModel.
        /// </summary>
        public virtual ListSelectionModel SelectionModel
        {
            get { return selectionModel; }
            set { selectionModel = value; }
        }

        /// <summary>
        /// Gets or sets the renderer that handles the painting of each cell. The default renderer is
        /// DefaultListCellRenderer.
        /// </summary>
        public virtual ListCellRenderer CellRenderer
        {
            get { return cellRenderer; }
            set { cellRenderer = value; }
        }

        /// <summary>
        /// Gets or sets the background color of the cell that contains the selected element.
        /// </summary>
        public virtual Color SelectionBackgroundColor
        {
            get { return selectionBackgroundColor; }
            set
            {
                selectionBackgroundColor = new Color(value.R, value.G, value.B, alpha);
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the cell that contains the selected element.
        /// </summary>
        public virtual Color SelectionForegroundColor
        {
            get { return selectionForegroundColor; }
            set
            {
                selectionForegroundColor = new Color(value.R, value.G, value.B, alpha);
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

                selectionBackgroundColor.A = selectionForegroundColor.A = alpha;
            }
        }

        #endregion

        #region Scrollable Members

        public Vector2 PreferredScrollableViewportSize
        {
            get { return preferredViewportSize; }
            set { preferredViewportSize = value; }
        }

        public bool ScrollableTracksViewportHeight
        {
            get { return scrollableTracksViewportHeight; }
            set { scrollableTracksViewportHeight = value; }
        }

        public bool ScrollableTracksViewportWidth
        {
            get { return scrollableTracksViewportWidth; }
            set { scrollableTracksViewportWidth = value; }
        }

        public int GetScrollableBlockIncrement(Rectangle visibleRect, GoblinEnums.Orientation orientation, int direction)
        {
            throw new NotImplementedException();
        }

        public int GetScrollableUnitIncrement(Rectangle visibleRect, GoblinEnums.Orientation orientation, int direction)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Internal Methods

        protected virtual int LocationToIndex(Point mouseLocation)
        {
            if (cellRenderer.CellHeight == 0)
                return 0;

            Point offset = new Point(mouseLocation.X - paintBounds.X, mouseLocation.Y - paintBounds.Y);
            return offset.Y / cellRenderer.CellHeight;
        }

        #endregion

        #region Override Methods

        protected override void HandleMouseClick(int button, Point mouseLocation)
        {
            base.HandleMousePress(button, mouseLocation);

            if (!enabled || !visible || !within)
                return;

            int index = LocationToIndex(mouseLocation);

            if (selectionModel.SelectionMode == SelectionMode.Single)
            {
                selectionModel.ClearSelection();

                if (index < listModel.Elements.Count)
                    selectionModel.AddSelectionInterval(index, index);
            }
            else
            {
                keyState = Keyboard.GetState();
                pressedKeys.Clear();
                pressedKeys.AddRange(keyState.GetPressedKeys());
                KeyModifier modifier = KeyboardInput.GetKeyModifier(pressedKeys);

                if (modifier.CtrlKeyPressed)
                {
                    if(selectionModel.SelectionMode == SelectionMode.MultipleInterval)
                        if(index < listModel.Elements.Count)
                            selectionModel.SetSelectionInterval(index, index);
                }
                else if (modifier.ShiftKeyPressed)
                {
                    if (index < listModel.Elements.Count)
                    {
                        if (selectionModel.AnchorSelectionIndex < 0)
                            return;

                        int startIndex = selectionModel.AnchorSelectionIndex;
                        int endIndex = index;
                        if (startIndex > endIndex)
                        {
                            startIndex = index;
                            endIndex = selectionModel.AnchorSelectionIndex;
                        }

                        selectionModel.AddSelectionInterval(startIndex, endIndex);
                    }
                }
                else
                {
                    selectionModel.ClearSelection();

                    if (index < listModel.Elements.Count)
                        selectionModel.AddSelectionInterval(index, index);
                }
            }
        }

        protected override void PaintComponent()
        {
            base.PaintComponent();

            for (int i = 0; i < listModel.Elements.Count; i++)
            {
                cellRenderer.GetListCellRendererComponent(this, listModel.Elements[i], i,
                    selectionModel.SelectedIndices.Contains(i)).RenderWidget();
            }
        }

        #endregion
    }
}
