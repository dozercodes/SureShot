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
    /// A SpinnerModel for sequence of integer numbers. If both Minimum and Maximum properties are defined, the return value
    /// is always limited between these two bounds. Otherwise, the value is unlimited.
    /// </summary>
    public class SpinnerIntegerModel : SpinnerModel
    {
        #region Member Fields

        private int value;
        private int max;
        private int min;
        private int stepSize;
        private bool bounded;

        #endregion

        #region Events

        public event StateChanged StateChangedEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a number model with no upper nor lower limit, step size of 1, and an initial value of 0.
        /// </summary>
        public SpinnerIntegerModel() : this(0, 1) { }

        public SpinnerIntegerModel(int value, int stepSize)
        {
            this.value = value;
            max = 0;
            min = 0;
            this.stepSize = stepSize;

            bounded = false;
        }

        /// <summary>
        /// Creates a number model with upper limit 'maximum', lower limit 'minimum', incremental/decremental size
        /// 'stepSize', and an initial 'value'.
        /// </summary>
        /// <param name="value">The initial value of the sequence.</param>
        /// <param name="minimum">The minimum value in the sequence.</param>
        /// <param name="maximum">The maximum value in the sequence.</param>
        /// <param name="stepSize">The incremental or decremental size.</param>
        public SpinnerIntegerModel(int value, int minimum, int maximum, int stepSize)
        {
            this.value = value;
            this.max = maximum;
            this.min = minimum;
            this.stepSize = stepSize;

            bounded = true;
        }

        #endregion

        #region Properties

        public object Value
        {
            get { return value; }
            set
            {
                this.value = (int)value;

                InvokeStateChangedEvent(this);
            }
        }

        public object NextValue
        {
            get
            {
                int retVal = value + stepSize;

                if (bounded && (retVal > max))
                    retVal = max;

                return retVal;
            }
        }

        public object PreviousValue
        {
            get
            {
                int retVal = value - stepSize;

                if (bounded && (retVal < min))
                    retVal = min;

                return retVal;
            }
        }

        /// <summary>
        /// Gets or sets the upper limit of this number sequence.
        /// </summary>
        public int Maximum
        {
            get { return max; }
            set
            {
                max = value;
                bounded = true;
            }
        }

        /// <summary>
        /// Gets or sets the lower limit of this number sequence.
        /// </summary>
        public int Minimum
        {
            get { return min; }
            set
            {
                min = value;
                bounded = true;
            }
        }

        /// <summary>
        /// Gets or sets the incremental/decremental size.
        /// </summary>
        public int StepSize
        {
            get { return stepSize; }
            set { stepSize = value; }
        }

        #endregion

        #region Public Methods

        public void InvokeStateChangedEvent(object source)
        {
            if (StateChangedEvent != null)
                StateChangedEvent(source);
        }

        #endregion
    }
}
