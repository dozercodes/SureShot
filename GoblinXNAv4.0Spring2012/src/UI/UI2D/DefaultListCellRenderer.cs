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
    /// A default implementation of ListCellRenderer interface. This default implementation uses G2DLabel component
    /// to render the cell.
    /// </summary>
    public class DefaultListCellRenderer : G2DLabel, ListCellRenderer
    {
        #region Constructors

        /// <summary>
        /// Creates a default implementation of ListCellRenderer interface using G2DLabel component.
        /// </summary>
        public DefaultListCellRenderer()
            : base()
        {
        }

        #endregion

        #region Properties

        public int CellHeight
        {
            get
            {
                if (textFont != null)
                    return (int)Math.Ceiling(textFont.MeasureString("A").Y) + 2;
                else
                    return 0;
            }
        }

        #endregion

        #region Implemented Methods

        public G2DComponent GetListCellRendererComponent(G2DList list, object value, int index, bool isSeleced)
        {
            this.TextFont = list.TextFont;
            this.Transparency = list.Transparency;
            this.TextTransparency = list.TextTransparency;
            this.HorizontalAlignment = list.HorizontalAlignment;

            this.Text = value.ToString();

            this.Bounds = new Rectangle(list.PaintBounds.X + 2, list.PaintBounds.Y + 2 + CellHeight * index, 
                list.Bounds.Width - 4, CellHeight);

            if (isSeleced)
            {
                drawBackground = true;
                this.BackgroundColor = list.SelectionBackgroundColor;
                this.TextColor = list.SelectionForegroundColor;
            }
            else
            {
                drawBackground = false;
                this.TextColor = list.TextColor;
            }

            return this;
        }

        #endregion
    }
}
