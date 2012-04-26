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
using Microsoft.Xna.Framework.Input;

using GoblinXNA.Helpers;

namespace GoblinXNA.Device.Generic
{
    /// <summary>
    /// An implementation of 6DOF input device using mouse based manipulation (e.g., dragging).
    /// </summary>
    public class GenericInput : InputDevice_6DOF
    {
        #region Member Fields

        private bool isAvailable;
        private Vector3 translation;
        private Quaternion rotation;

        private Vector3 initialTranslation;
        private Quaternion initialRotation;
        private Matrix baseTransform;

        private float panSpeed;
        private float zoomSpeed;
        private float rotateSpeed;

        private Point curMouseLocation;
        private Point prevMouseDragLocation;
        private static GenericInput input;

        #endregion

        #region Private Constructors

        /// <summary>
        /// A private constructor.
        /// </summary>
        private GenericInput()
        {
            panSpeed = 1;
            zoomSpeed = 1;
            rotateSpeed = 1;

            translation = new Vector3();
            rotation = Quaternion.Identity;

            initialTranslation = new Vector3();
            initialRotation = Quaternion.Identity;

            curMouseLocation = new Point();
            prevMouseDragLocation = new Point(-1, -1);

            MouseInput.Instance.MouseWheelMoveEvent +=
                delegate(int delta, int value)
                {
                    Vector3 nearSource = Vector3Helper.Get(curMouseLocation.X, curMouseLocation.Y, 0);
                    Vector3 farSource = Vector3Helper.Get(curMouseLocation.X, curMouseLocation.Y, 1);

                    // Now convert the near and far source to actual near and far 3D points based on our eye location
                    // and view frustum
                    Vector3 nearPoint = State.Device.Viewport.Unproject(nearSource,
                        State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
                    Vector3 farPoint = State.Device.Viewport.Unproject(farSource,
                        State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

                    Vector3 zoomRay = farPoint - nearPoint;
                    zoomRay.Normalize();
                    zoomRay *= zoomSpeed * delta / 100;

                    translation += zoomRay;
                };

            MouseInput.Instance.MouseDragEvent += 
                delegate(int button, Point startLocation, Point currentLocation)
                {
                    if (prevMouseDragLocation.X < 0)
                        prevMouseDragLocation = startLocation;

                    if (button == MouseInput.RightButton)
                    {
                        int deltaX = currentLocation.X - prevMouseDragLocation.X;
                        int deltaY = currentLocation.Y - prevMouseDragLocation.Y;

                        if (!(deltaX == 0 && deltaY == 0))
                        {
                            Quaternion change = Quaternion.CreateFromYawPitchRoll
                                ((float)(deltaX * rotateSpeed * Math.PI / 45),
                                (float)(deltaY * rotateSpeed * Math.PI / 45), 0);
                            rotation = Quaternion.Multiply(rotation, change);
                        }
                    }
                    else if (button == MouseInput.MiddleButton)
                    {
                        translation += (currentLocation.Y - prevMouseDragLocation.Y) *
                            panSpeed * baseTransform.Up;
                    }
                    else if (button == MouseInput.LeftButton)
                    {
                        Vector3 leftAxis = (currentLocation.X - prevMouseDragLocation.X) / 2.0f * 
                            panSpeed * baseTransform.Left;
                        Vector3 forwardAxis = (currentLocation.Y - prevMouseDragLocation.Y) / 2.0f * 
                            panSpeed * baseTransform.Forward;

                        Vector3 change = leftAxis + forwardAxis;

                        translation += change;
                    }

                    prevMouseDragLocation = currentLocation;
                };

            MouseInput.Instance.MouseMoveEvent +=
                delegate(Point mouseLocation)
                {
                    curMouseLocation = mouseLocation;
                };

            MouseInput.Instance.MouseReleaseEvent +=
                delegate(int button, Point mouseLocation)
                {
                    prevMouseDragLocation.X = -1;
                };

            isAvailable = true;
        }

        #endregion

        #region Public Properties

        public String Identifier
        {
            get { return "GenericInput"; }
        }

        public bool IsAvailable
        {
            get { return isAvailable; }
        }

        public Vector3 Translation
        {
            get 
            {
                return translation; 
            }
        }

        public Quaternion Rotation
        {
            get 
            {
                return rotation; 
            }
        }

        /// <summary>
        /// Sets the initial translation. Note that this will also set Translation property.
        /// </summary>
        public Vector3 InitialTranslation
        {
            set 
            { 
                initialTranslation = value;
                translation = value;
            }
        }

        /// <summary>
        /// Sets the initial rotation. Note that this will also set Rotation property.
        /// </summary>
        public Quaternion InitialRotation
        {
            set 
            { 
                initialRotation = value;
                rotation = value;
            }
        }

        public Matrix WorldTransformation
        {
            get
            {
                return Matrix.Transform(Matrix.CreateTranslation(translation), rotation);
            }
        }

        /// <summary>
        /// Sets the base transform to use for panning.
        /// </summary>
        public Matrix BaseTransformation
        {
            set { baseTransform = value; }
        }

        /// <summary>
        /// Gets or sets the pan speed. The default value is 1.
        /// </summary>
        public float PanSpeed
        {
            get { return panSpeed; }
            set { panSpeed = value; }
        }

        /// <summary>
        /// Gets or sets the zoom speed. The default value is 1.
        /// </summary>
        public float ZoomSpeed
        {
            get { return zoomSpeed; }
            set { zoomSpeed = value; }
        }

        /// <summary>
        /// Gets or sets the rotation speed. The default value is 1.
        /// </summary>
        public float RotateSpeed
        {
            get { return rotateSpeed; }
            set { rotateSpeed = value; }
        }

        /// <summary>
        /// Gets the instantiation of GenericInput class.
        /// </summary>
        public static GenericInput Instance
        {
            get
            {
                if (input == null)
                {
                    input = new GenericInput();
                }

                return input;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the translation and rotation to initial values.
        /// </summary>
        public void Reset()
        {
            translation = initialTranslation;
            rotation = initialRotation;
        }

        public void Update(TimeSpan elapsedTime, bool deviceActive)
        {
            // nothing to update
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        #endregion
    }
}
