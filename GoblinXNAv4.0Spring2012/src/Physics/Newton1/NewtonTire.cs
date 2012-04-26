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

using GoblinXNA.Graphics;

namespace GoblinXNA.Physics.Newton1
{
    /// <summary>
    /// This class represents a vehicle tire used for vehicle simulation by Newton physics library.
    /// </summary>
    public class NewtonTire
    {
        #region Member Fields

        protected float radius;
        protected float width;
        protected float mass;
        protected int collisionID;

        protected float steer;
        protected float torque;
        protected float brake;

        protected float tireRefHeight;
        protected Vector3 pin;
        protected float suspShock;
        protected float suspSpring;
        protected float suspLength;

        protected Vector3 brakeRefPosition;
        protected Matrix tireOffsetMatrix;

        protected Matrix tireMatrix;
        protected Matrix brakeMatrix;
        protected Matrix axelMatrix;
        protected Matrix suspTopMatrix;
        protected Matrix suspBottomMatrix;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a vehicle tire object used by the Newton physics engine.
        /// </summary>
        public NewtonTire() 
        {
            radius = 0;
            width = 0;
            mass = 0;
            collisionID = 0;

            steer = 0;
            torque = 0;
            brake = 0;

            tireRefHeight = 0;
            pin = new Vector3();
            suspShock = 0;
            suspSpring = 0;
            suspLength = 0;

            brakeRefPosition = new Vector3();
            tireOffsetMatrix = Matrix.Identity;

            tireMatrix = Matrix.Identity;
            brakeMatrix = Matrix.Identity;
            axelMatrix = Matrix.Identity;
            suspTopMatrix = Matrix.Identity;
            suspBottomMatrix = Matrix.Identity;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The collision ID use by the application to identify the tire contacts in a contact 
        /// callback function.
        /// </summary>
        public int CollisionID
        {
            get { return collisionID; }
            set { collisionID = value; }
        }

        /// <summary>
        /// The radius of the tire.
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        /// <summary>
        /// The width of the tire, must be smaller than the tire radius.
        /// </summary>
        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        /// <summary>
        /// The mass of the tire, must be much smaller than the vehicle body. 
        /// A ratio of 50:1 to 100:1 is the recommended value.
        /// </summary>
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        /// <summary>
        /// Gets or sets the steering parameter applied to this tire.
        /// </summary>
        public float Steer
        {
            get { return steer; }
            set { steer = value; }
        }

        /// <summary>
        /// Gets or sets the torque parameter applied to this tire.
        /// </summary>
        public float Torque
        {
            get { return torque; }
            set { torque = value; }
        }

        /// <summary>
        /// Gets or sets the brake parameter applied to this tire.
        /// </summary>
        public float Brakes
        {
            get { return brake; }
            set { brake = value; }
        }

        public float TireRefHeight
        {
            get { return tireRefHeight; }
            set { tireRefHeight = value; }
        }

        /// <summary>
        /// The parametrized damping constant for a spring, mass, damper system. A value of one 
        /// corresponds to a critically damped system.
        /// </summary>
        public float SuspensionShock
        {
            get { return suspShock; }
            set { suspShock = value; }
        }

        /// <summary>
        /// The parametrized spring constant for a spring, mass, damper system. A value of one 
        /// corresponds to a critically damped system.
        /// </summary>
        public float SuspensionSpring
        {
            get { return suspSpring; }
            set { suspSpring = value; }
        }

        /// <summary>
        /// The distance from the tire set position to the upper stop on the vehicle body frame. 
        /// The total suspension length is twice of that. 
        /// </summary>
        public float SuspensionLength
        {
            get { return suspLength; }
            set { suspLength = value; }
        }

        /// <summary>
        /// The rotation axis of the tire, in the space of the tire.
        /// </summary>
        public Vector3 Pin
        {
            get { return pin; }
            set { pin = value; }
        }

        public Vector3 BrakeRefPosition
        {
            get { return brakeRefPosition; }
            set { brakeRefPosition = value; }
        }

        /// <summary>
        /// Gets or sets the offset matrix of the tire relative to the vehicle body.
        /// </summary>
        public Matrix TireOffsetMatrix
        {
            get { return tireOffsetMatrix; }
            set { tireOffsetMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the global matrix of the tire.
        /// </summary>
        public Matrix TireMatrix
        {
            get { return tireMatrix; }
            set { tireMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the global matrix of the axel.
        /// </summary>
        public Matrix AxelMatrix
        {
            get { return axelMatrix; }
            set { axelMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the global matrix of the brake.
        /// </summary>
        public Matrix BrakeMatrix
        {
            get { return brakeMatrix; }
            set { brakeMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the global matrix of the top suspension.
        /// </summary>
        public Matrix SuspensionTopMatrix
        {
            get { return suspTopMatrix; }
            set { suspTopMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the global matrix of the bottom suspension.
        /// </summary>
        public Matrix SuspensionBottomMatrix
        {
            get { return suspBottomMatrix; }
            set { suspBottomMatrix = value; }
        }

        #endregion
    }
}
