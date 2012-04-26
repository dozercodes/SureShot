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

namespace GoblinXNA
{
    /// <summary>
    /// A collection of enums used in the GoblinXNA framework
    /// </summary>
    public class GoblinEnums
    {
        /// <summary>
        /// An enum that describes the orientation of a component.
        /// </summary>
        public enum Orientation
        {
            /// <summary>
            /// Horizontal orientation
            /// </summary>
            Horizontal,
            /// <summary>
            /// Vertical orientation
            /// </summary>
            Vertical
        }

        /// <summary>
        /// An enum that describes the horizontal alignment of, usually, a textual display.
        /// </summary>
        public enum HorizontalAlignment
        {
            /// <summary>
            /// Aligned on the left edge of a certain bound
            /// </summary>
            Left,
            /// <summary>
            /// Aligned on the center of a certain bound
            /// </summary>
            Center,
            /// <summary>
            /// Aligned on the right edge of a certain bound
            /// </summary>
            Right,
            /// <summary>
            /// No alignment
            /// </summary>
            None
        }

        /// <summary>
        /// An enum that describes the vertical alignment of, usually, a textual display.
        /// </summary>
        public enum VerticalAlignment
        {
            /// <summary>
            /// Aligned on the top edge of a certain bound
            /// </summary>
            Top,
            /// <summary>
            /// Aligned on the center of a certain bound
            /// </summary>
            Center,
            /// <summary>
            /// Aligned on the bottom edge of a certain bound
            /// </summary>
            Bottom,
            /// <summary>
            /// No alignnment
            /// </summary>
            None
        }

        /// <summary>
        /// An enum that describes the border type.
        /// </summary>
        /// <remarks>
        /// This enum is only used for G2DPanel class
        /// </remarks>
        public enum BorderFactory
        {
            /// <summary>
            /// An empty boarder
            /// </summary>
            EmptyBorder,
            /// <summary>
            /// A border with etched line decoration
            /// </summary>
            EtchedBorder,
            /// <summary>
            /// A border with normal line decoration
            /// </summary>
            LineBorder
            //RaisedBevelBorder,
            //LoweredBevelBorder,
            //TitledBorder
        }

        /// <summary>
        /// Defines how 3D User Interface objects are placed in the world.
        /// </summary>
        public enum DisplayConfig
        {
            /// <summary>
            /// Fixed to the display
            /// </summary>
            DisplayFixed,
            /// <summary>
            /// Fixed to an object
            /// </summary>
            ObjectFixed,
            /// <summary>
            /// Fixed to the viewer's surrounding
            /// </summary>
            SurroundFixed,
            /// <summary>
            /// Fixed to the world
            /// </summary>
            WorldFixed
        }
    }
}
