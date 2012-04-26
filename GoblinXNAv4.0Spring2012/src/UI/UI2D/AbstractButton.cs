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
    /// An abstract class that defines a button. 
    /// </summary>
    /// <remarks>
    /// Any GoblinXNA 2D GUI button components should extend this class.
    /// </remarks>
    public class AbstractButton : G2DComponent
    {
        #region Member Fields
        /// Color used to highlight the inner border when the mouse is over it
        /// </summary>
        protected Color highlightColor;
        #endregion

        #region Events

        /// <summary>
        /// An event triggered whenever the button is activated.
        /// </summary>
        public event ActionPerformed ActionPerformedEvent;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates an abstract button with the specified label.
        /// </summary>
        /// <param name="label"></param>
        protected AbstractButton(String label) 
            : base()
        {
            Text = label;
            highlightColor = Color.Yellow;
        }
        /// <summary>
        /// Creates an abstract button with no text.
        /// </summary>
        protected AbstractButton() : this("") { }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the color used for highlighting the inner border when the mouse is over it.
        /// </summary>
        public virtual Color HighlightColor
        {
            get { return highlightColor; }
            set
            {
                highlightColor = new Color(value.R, value.G, value.B, (byte)(alpha * 255));
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

                highlightColor.A = alpha;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Programmatically click the button
        /// </summary>
        public virtual void DoClick() {
            InvokeActionPerformedEvent(this);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Invokes the action performed event.
        /// </summary>
        /// <param name="source">The class that invoked this method.</param>
        protected void InvokeActionPerformedEvent(object source)
        {
            if (ActionPerformedEvent != null)
                ActionPerformedEvent(source);
        }

        #endregion
    }
}
