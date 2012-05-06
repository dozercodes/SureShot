/************************************************************************************ 
 * Copyright (c) 2008-2012, Columbia University
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

namespace GoblinXNA.Helpers
{
    /// <summary>
    /// An enum that defines the interpolation method.
    /// </summary>
    public enum InterpolationMethod
    {
        /// <summary>
        /// Linear interpolation.
        /// </summary>
        Linear,
        //Logarithmic,
        //InverseLogarithmic,
    }

    public delegate void InterpolationFinished();

    /// <summary>
    /// A helper class for interpolating between two double precision numbers
    /// in a specified duration of time. 
    /// </summary>
    /// <remarks>
    /// Currently, only linear interpolation is implemented, but more interpolation methods
    /// will be added in the upcoming releases.
    /// </remarks>
    public class Interpolator
    {
        #region Member Fields
        protected double startValue;
        protected double endValue;
        protected long duration;
        protected InterpolationMethod method;
        protected bool started;
        protected bool done;

        protected double startTime;
        protected double logBase;
        #endregion

        #region Events
        /// <summary>
        /// An event to be called when the interpolation is done.
        /// </summary>
        /// <remarks>
        /// You need to keep accessing the Value property in order to have this event triggered
        /// when the interpolation finishes. If Value property is not accessed, then this event will 
        /// never be triggered. This is not a timer class, so do not use this as a timer.
        /// </remarks>
        public event InterpolationFinished DoneEvent;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an interpolator that interpolates from the specified 'startValue' to
        /// the 'endValue' in the specified duration of time using the specified interpolation 
        /// method.
        /// </summary>
        /// <param name="startValue">The start value</param>
        /// <param name="endValue">The end value</param>
        /// <param name="duration">The duration of time in milliseconds</param>
        /// <param name="method">The interpolation method</param>
        public Interpolator(double startValue, double endValue, long duration,
            InterpolationMethod method)
        {
            this.startValue = startValue;
            this.endValue = endValue;
            this.duration = duration;
            this.method = method;
            done = false;
            started = false;
            logBase = Math.E;
        }

        /// <summary>
        /// Creates an interpolator that interpolates from the specified 'startValue' to
        /// the 'endValue' in the specified duration of time using linear interpolation.
        /// </summary>
        /// <param name="startValue">The start value</param>
        /// <param name="endValue">The end value</param>
        /// <param name="duration">The duration of time in milliseconds</param>
        public Interpolator(double startValue, double endValue, long duration)
            :
            this(startValue, endValue, duration, InterpolationMethod.Linear)
        {
        }

        /// <summary>
        /// Creates an interpolator that interpolates from 0.0 to 1.0 in 1000 milliseconds
        /// using linear interpolation.
        /// </summary>
        public Interpolator() : this(0, 1, 1000, InterpolationMethod.Linear) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the start value.
        /// </summary>
        /// <exception cref="GoblinException">If you try to set this value after the 
        /// interpolation starts and before the interpolation finishes</exception>
        public double StartValue
        {
            get { return startValue; }
            set 
            {
                if (started)
                    throw new GoblinException("You can't change the start value after the interpolation " +
                        "starts and before the interpolation finishes");

                startValue = value; 
            }
        }

        /// <summary>
        /// Gets or sets the end value.
        /// </summary>
        /// <exception cref="GoblinException">If you try to set this value after the 
        /// interpolation starts and before the interpolation finishes</exception>
        public double EndValue
        {
            get { return endValue; }
            set 
            {
                if (started)
                    throw new GoblinException("You can't change the end value after the interpolation " +
                        "starts and before the interpolation finishes");

                endValue = value; 
            }
        }

        /// <summary>
        /// Gets or sets the log base to use for logarithmic interpolation.
        /// The default base is Math.E.
        /// </summary>
        /// <remarks>
        /// Logarithmic interpolation methods are not supported yet, so setting this value
        /// will not affect anything.
        /// </remarks>
        public double LogBase
        {
            get { return logBase; }
            set 
            {
                if (started)
                    throw new GoblinException("You can't change the end value after the interpolation " +
                        "starts and before the interpolation finishes");

                logBase = value; 
            }
        }

        /// <summary>
        /// Gets the current interpolated value.
        /// </summary>
        public double Value
        {
            get
            {
                if (done)
                    return endValue;
                else if (started)
                {
                    double stopTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                    double elapsed = stopTime - startTime;
                    if (elapsed >= duration)
                    {
                        done = true;
                        started = false;
                        if (DoneEvent != null)
                            DoneEvent();
                        return endValue;
                    }
                    else
                    {
                        if (startValue <= endValue)
                            return (endValue - startValue) * (elapsed / duration) + startValue;
                        else
                            return (startValue - endValue) * (1 - elapsed / duration) + endValue;
                    }
                }
                else
                    return startValue;
            }
        }

        /// <summary>
        /// Gets or sets the duration of the time takes to complete the interpolation in milliseconds.
        /// </summary>
        /// <exception cref="GoblinException">If you try to set this value after the 
        /// interpolation starts and before the interpolation finishes</exception>
        public long Duration
        {
            get { return duration; }
            set 
            {
                if (started)
                    throw new GoblinException("You can't change the duration after the interpolation " +
                        "starts and before the interpolation finishes");

                duration = value; 
            }
        }

        /// <summary>
        /// Gets or sets the interpolation method.
        /// </summary>
        /// <exception cref="GoblinException">If you try to set this value after the 
        /// interpolation starts and before the interpolation finishes</exception>
        public InterpolationMethod Method
        {
            get { return method; }
            set 
            {
                if (started)
                    throw new GoblinException("You can't change the interpolation method after " +
                        "the interpolation starts and before the interpolation finishes");

                method = value; 
            }
        }

        /// <summary>
        /// Gets whether the interpolation has started. Once the interpolation is done,
        /// this value is reset to false.
        /// </summary>
        public bool Started
        {
            get { return started; }
        }

        /// <summary>
        /// Gets whether the interpolation is done after it's started. If you start the interpolation
        /// again after an interpolation is finished, this value is reset to false until the interpolation
        /// finishes again.
        /// </summary>
        public bool Done
        {
            get { return done; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the interpolation.
        /// </summary>
        /// <exception cref="GoblinException">If you try to start again before your previous
        /// interpolation finishes.</exception>
        public void Start()
        {
            if (started)
                throw new GoblinException("Interpolation already started. You can't start a new " +
                    "interpolation until it's done");

            startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            started = true;
            done = false;
        }
        #endregion
    }
}
