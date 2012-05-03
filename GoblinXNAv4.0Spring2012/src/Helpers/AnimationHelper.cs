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
 * Author: Nicolas Dedual (dedual@cs.columbia.edu)
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers.XNATweener;

namespace GoblinXNA.Helpers
{
    /// <summary>
    /// An enum that is used to determine what type of variables we will be interpolating.
    /// </summary>
    public enum VariableToAnimate
    {
        floatValue,Vector2Value,Vector3Value
    }

    /// <summary>
    /// Another enum type that will determine what kind of transition to implement
    /// </summary>
    public enum TypeOfTransition
    {
        Linear,
        Quadratic,
        Cubic,
        Quartic,
        Quintic,
        Sinusoidal,
        Exponential,
        Circular,
        Elastic,
        Back,
        Bounce
    }

    /// <summary>
    /// Yet another enum that declares how we will ease in and out in our interpolation
    /// </summary>
    public enum Easing { EaseIn, EaseOut, EaseInOut };
       
    /// <summary>
    /// This is a class that implements different interpolation functions for:
    ///  
    ///  - Single float values
    ///  - Vector2 values
    ///  - Vector3 values 
    ///  
    ///  
    /// </summary>
    public class AnimationHelper
    {
        #region Fields

        private Tweener singleValueTweener;
        private Vector2Tweener vectorTwoTweener;
        private Vector3Tweener vector3Tweener;

        private VariableToAnimate kindOfAnimation;

        private TypeOfTransition currentTransition;
        private Vector3 returnValue; // Value we will be returning.

        private Easing easing;

        #endregion

        #region Constructor

        public AnimationHelper()
        {
            kindOfAnimation = VariableToAnimate.Vector3Value;
            easing = Easing.EaseIn;
            returnValue = new Vector3(0, 0, 0);
            InitializeTweener();
        }

        public AnimationHelper(VariableToAnimate _aKindOfAnimation)
        {
            kindOfAnimation = _aKindOfAnimation;
            easing = Easing.EaseIn;
            returnValue = new Vector3(0, 0, 0);
            InitializeTweener();
        }
        public AnimationHelper(Easing _easing)
        {
            kindOfAnimation = VariableToAnimate.Vector3Value;
            easing = _easing;
            returnValue = new Vector3(0, 0, 0);
            InitializeTweener();
        }

        public AnimationHelper(VariableToAnimate _var, Easing _easing)
        {
            kindOfAnimation = _var;
            easing = _easing;
            returnValue = new Vector3(0, 0, 0);
            InitializeTweener();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Value to return. It'll always be a Vector3 but
        /// - For floats, only the first value is the output
        /// - For Vector2, the first two values are the output
        /// - For Vector3, all values are the output
        /// </summary>
        public Vector3 ReturnValue
        {
            get { return returnValue; }
        }

        /// <summary>
        /// Sets or returns our current Easing
        /// </summary>
        public Easing CurrentEasing
        {
            get { return easing; }
            set { easing = value; }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Initializes the Tweener. Useful for whenever we create an animator, but don't set it to animate anything.
        /// </summary>
        private void InitializeTweener()
        {
            Animate(TypeOfTransition.Linear, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.0);
        }

        /// <summary>
        /// How we determine which Tweening (another name for Interpolation) function we return. 
        /// </summary>
        /// <returns>A TweeningFunction, which is the type of interpolation we will carry out</returns>
        protected TweeningFunction GetTweeningFunction()
        {
            switch (currentTransition)
            {
                case TypeOfTransition.Back:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Back), easing.ToString());

                case TypeOfTransition.Bounce:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Bounce), easing.ToString());

                case TypeOfTransition.Circular:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Circular), easing.ToString());

                case TypeOfTransition.Cubic:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Cubic), easing.ToString());

                case TypeOfTransition.Elastic:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Elastic), easing.ToString());

                case TypeOfTransition.Exponential:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Exponential), easing.ToString());

                case TypeOfTransition.Linear:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Linear), easing.ToString());

                case TypeOfTransition.Quadratic:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Quadratic), easing.ToString());

                case TypeOfTransition.Quartic:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Quartic), easing.ToString());

                case TypeOfTransition.Quintic:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Quintic), easing.ToString());

                case TypeOfTransition.Sinusoidal:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Sinusoidal), easing.ToString());

                default:
                return (TweeningFunction)Delegate.CreateDelegate(typeof(TweeningFunction), typeof(Linear), easing.ToString());
            }
        }

        /// <summary>
        /// Function used to set our animation parameters
        /// </summary>
        /// <param name="transition">Type of interpolation function to utilize</param>
        /// <param name="startPosition"> Our start position</param>
        /// <param name="endPosition">Our end position</param>
        /// <param name="durationInSeconds">How many seconds the animation should take</param>
        public void Animate(TypeOfTransition transition, Vector3 startPosition, Vector3 endPosition, double durationInSeconds)
        {
            currentTransition = transition;

            switch (kindOfAnimation)
            {
                case VariableToAnimate.floatValue:
                    singleValueTweener = new Tweener(startPosition.X, endPosition.X, TimeSpan.FromSeconds(durationInSeconds), GetTweeningFunction());
                    singleValueTweener.PositionChanged += delegate(float newPosition) { returnValue = new Vector3(newPosition, 0, 0); };
                    break;
                case VariableToAnimate.Vector2Value:
                    vectorTwoTweener = new Vector2Tweener(new Vector2(startPosition.X, startPosition.Y), new Vector2(endPosition.X, endPosition.Y), TimeSpan.FromSeconds(durationInSeconds), GetTweeningFunction());
                    vectorTwoTweener.PositionChanged += delegate(Vector2 newPosition) { returnValue = new Vector3(newPosition.X, newPosition.Y, 0); };
                    break;

                case VariableToAnimate.Vector3Value:
                    vector3Tweener = new Vector3Tweener(startPosition, endPosition, TimeSpan.FromSeconds(durationInSeconds), GetTweeningFunction());
                    vector3Tweener.PositionChanged += delegate(Vector3 newPosition) { returnValue = newPosition;};
                    break;

                default:
                    vector3Tweener = new Vector3Tweener(startPosition, endPosition, TimeSpan.FromSeconds(durationInSeconds), GetTweeningFunction());
                    vector3Tweener.PositionChanged += delegate(Vector3 newPosition) { returnValue = newPosition;};
                    break;
            }
        }

        /// <summary>
        /// Function used to set an action to take place once the animation has concluded.
        /// </summary>
        /// <param name="ToCallAtEndOfAnimation"> Any function or delegate.</param>
        public void SetEndAction(EndHandler ToCallAtEndOfAnimation)
        {
            switch (kindOfAnimation)
            {
                case VariableToAnimate.floatValue:
                    singleValueTweener.Ended += ToCallAtEndOfAnimation;
                    break;
                case VariableToAnimate.Vector2Value:
                    vectorTwoTweener.Ended += ToCallAtEndOfAnimation;
                    break;
                case VariableToAnimate.Vector3Value:
                    vector3Tweener.Ended += ToCallAtEndOfAnimation;
                    break;
                default:
                    vector3Tweener.Ended += ToCallAtEndOfAnimation;
                    break;
            }
        }

        /// <summary>
        /// Set whether or not to loop over the animation, and how many times
        /// </summary>
        /// <param name="forward"> Determines whether we will loop from start to end positions, or from end to start positions. True means from start to end positions.</param>
        /// <param name="howManyTimes">Instances to loop over. If left black (or less than zero) it will loop indefinitely. </param>
        public void SetLooping(bool forward, int howManyTimes)
        {
                switch (kindOfAnimation)
                {
                    case VariableToAnimate.floatValue:
                        if(forward)
                        {
                            if (howManyTimes < 1)
                                singleValueTweener.Loop.FrontToBack();
                            else
                                singleValueTweener.Loop.FrontToBack(howManyTimes);
                        }
                        else
                        {
                            if (howManyTimes < 1)
                                singleValueTweener.Loop.BackAndForth();
                            else
                                singleValueTweener.Loop.BackAndForth(howManyTimes);
                        }
                        break;
                    case VariableToAnimate.Vector2Value:
                        if (forward)
                        {
                            if (howManyTimes < 1)
                                vectorTwoTweener.Loop.FrontToBack();
                            else
                                vectorTwoTweener.Loop.FrontToBack(howManyTimes);
                        }
                        else
                        {
                            if (howManyTimes < 1)
                                singleValueTweener.Loop.BackAndForth();
                            else
                                singleValueTweener.Loop.BackAndForth(howManyTimes);
                        }
                        break;
                    case VariableToAnimate.Vector3Value:
                        if (forward)
                        {
                            if (howManyTimes < 1)
                                vector3Tweener.Loop.FrontToBack();
                            else
                                vector3Tweener.Loop.FrontToBack(howManyTimes);
                        }
                        else
                        {
                            if (howManyTimes < 1)
                                vector3Tweener.Loop.BackAndForth();
                            else
                                vector3Tweener.Loop.BackAndForth(howManyTimes);
                        }
                        break;
                        
                    default:
                        if (forward)
                        {
                            if (howManyTimes < 1)
                                vector3Tweener.Loop.FrontToBack();
                            else
                                vector3Tweener.Loop.FrontToBack(howManyTimes);
                        }
                        else
                        {
                            if (howManyTimes < 1)
                                vector3Tweener.Loop.BackAndForth();
                            else
                                vector3Tweener.Loop.BackAndForth(howManyTimes);
                        }
                        break;
            }
        }

        /// <summary>
        /// Plays the animation
        /// </summary>
        public void Play()
        {
            switch (kindOfAnimation)
            {
                case VariableToAnimate.floatValue:
                    singleValueTweener.Play();
                    break;
                case VariableToAnimate.Vector2Value:
                    vectorTwoTweener.Play();
                    break;
                case VariableToAnimate.Vector3Value:
                    vector3Tweener.Play();
                    break;
                default:
                    vector3Tweener.Play();
                    break;
            }
        }

        /// <summary>
        /// Pauses the animation
        /// </summary>
        public void Pause()
        {
            switch (kindOfAnimation)
            {
                case VariableToAnimate.floatValue:
                    singleValueTweener.Pause();
                    break;
                case VariableToAnimate.Vector2Value:
                    vectorTwoTweener.Pause();
                    break;
                case VariableToAnimate.Vector3Value:
                    vector3Tweener.Pause();
                    break;
                default:
                    vector3Tweener.Pause();
                    break;
            }
        }

        /// <summary>
        /// Updates the animation
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (currentTransition != null)
            {
                switch (kindOfAnimation)
                {
                    case VariableToAnimate.floatValue:
                        singleValueTweener.Update(gameTime);
                        break;
                    case VariableToAnimate.Vector2Value:
                        vectorTwoTweener.Update(gameTime);
                        break;
                    case VariableToAnimate.Vector3Value:
                        vector3Tweener.Update(gameTime);
                        break;
                    default:
                        vector3Tweener.Update(gameTime);
                        break;
                }
            }
        }
        #endregion
    }
}
