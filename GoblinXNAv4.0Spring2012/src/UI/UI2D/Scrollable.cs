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
    /// An interface that provides information to a scrolling container like G2DScrollPane.
    /// </summary>
    public interface Scrollable
    {
        /// <summary>
        /// Gets the preferred size of the viewport.
        /// </summary>
        Vector2 PreferredScrollableViewportSize { get; }

        /// <summary>
        /// Indicates whether a viewport should always force the height of this Scrollable to
        /// match the height of the viewport.
        /// </summary>
        bool ScrollableTracksViewportHeight { get; }

        /// <summary>
        /// Indicates whether a viewport should always force the width of this Scrollable to 
        /// match the width of the viewport.
        /// </summary>
        bool ScrollableTracksViewportWidth { get; }

        /// <summary>
        /// Gets the scroll increment that will completely expose one block of rows or columns, depending on
        /// the orientation.
        /// </summary>
        /// <param name="visibleRect">The view area visible within the viewport</param>
        /// <param name="orientation">Horizontal indicates columns, and Vertical indicates rows</param>
        /// <param name="direction">Scroll up/left if less than 0; otherwise, scroll down/right</param>
        /// <returns></returns>
        int GetScrollableBlockIncrement(Rectangle visibleRect, GoblinEnums.Orientation orientation, int direction);

        /// <summary>
        /// Gets the scroll increment that will completely expose one new row or column depending on the
        /// orientation.
        /// </summary>
        /// <param name="visibleRect">The view area visible within the viewport</param>
        /// <param name="orientation">Horizontal indicates columns, and Vertical indicates rows</param>
        /// <param name="direction">Scroll up/left if less than 0; otherwise, scroll down/right</param>
        /// <returns></returns>
        int GetScrollableUnitIncrement(Rectangle visibleRect, GoblinEnums.Orientation orientation, int direction);
    }
}
