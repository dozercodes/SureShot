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

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// An implementation of a two-state button that can be toggled.
    /// </summary>
    /// <remarks>
    /// Any GoblinXNA 2D GUI button component with toggling should extend this class
    /// </remarks>
    public class ToggleButton : AbstractButton
    {
        #region Member Fields
        /// <summary>
        /// Indicator of whether this toggle button is selected
        /// </summary>
        protected bool selected;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a two-state button that can be toggled with a specified label.
        /// </summary>
        /// <param name="label"></param>
        public ToggleButton(String label)
            : base(label)
        {
            selected = false;
            drawBackground = false;
        }
        /// <summary>
        /// Creates a two-state button that can be toggled.
        /// </summary>
        public ToggleButton() : this("") { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets whether this toggle button is selected. Call the DoClick() method in order to 
        /// set Selected to true.
        /// </summary>
        public virtual bool Selected
        {
            get { return selected; }
            internal set { selected = value; }
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Programmatically click the toggle button
        /// </summary>
        public override void DoClick()
        {
            selected = !selected;

            base.DoClick();
        }
        #endregion
    }
}
