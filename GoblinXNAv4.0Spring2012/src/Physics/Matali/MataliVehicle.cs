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
using System.Linq;
using System.Text;

using GoblinXNA.Graphics;
using Komires.MataliPhysics;

namespace GoblinXNA.Physics.Matali
{
    public class MataliVehicle : MataliObject
    {
        #region Member Fields

        private List<MataliObject> bodyObjects;
        private List<MataliObject>[] wheelObjects;
        private List<MataliObject>[] doorObjects;
        private List<MataliObject> steeringObjects;

        private List<ConstraintPair> constraints; 

        private bool hasBody;
        private bool hasWheels;
        private bool hasDoors;
        private bool hasSteering;

        #endregion

        #region Constructors

        public MataliVehicle(object container)
            : base(container)
        {
            hasBody = false;
            hasWheels = false;

            bodyObjects = new List<MataliObject>(1);
            wheelObjects = new List<MataliObject>[4];
            doorObjects = new List<MataliObject>[4];
            steeringObjects = new List<MataliObject>(1);

            for (int i = 0; i < 4; ++i)
            {
                wheelObjects[i] = new List<MataliObject>(1);
                doorObjects[i] = new List<MataliObject>(1);
            }

            constraints = new List<ConstraintPair>();
        }

        #endregion

        #region Properties

        public bool HasEnoughParts
        {
            get { return true; }// hasBody && hasWheels; }
        }

        public List<MataliObject> Body
        {
            get { return bodyObjects; }
        }

        public List<MataliObject>[] Wheels
        {
            get { return wheelObjects; }
        }

        public List<MataliObject>[] Doors
        {
            get { return doorObjects; }
        }

        public List<MataliObject> SteeringWheel
        {
            get { return steeringObjects; }
        }

        public List<ConstraintPair> Constraints
        {
            get { return constraints; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a body as a single object.
        /// </summary>
        /// <param name="bodyObject"></param>
        public void AddBody(MataliObject bodyObject)
        {
            if (hasBody)
                throw new GoblinException("You already have a body for this vehicle");

            bodyObjects.Add(bodyObject);

            hasBody = true;
        }

        /// <summary>
        /// Add a body as a composite of multiple objects.
        /// </summary>
        /// <param name="bodyComposite"></param>
        public void AddBody(List<MataliObject> bodyComposite)
        {
            if (hasBody)
                throw new GoblinException("You already have a body for this vehicle");

            bodyObjects.AddRange(bodyComposite);

            hasBody = true;
        }

        /// <summary>
        /// Add four wheels that have the same graphical geometry and physical properties.
        /// </summary>
        /// <param name="wheelObject"></param>
        public void AddWheels(MataliObject wheelObject)
        {
            AddWheels(wheelObject, wheelObject, wheelObject, wheelObject);
        }

        /// <summary>
        /// Add four wheels each with different graphical geometry and physical properties.
        /// </summary>
        /// <param name="frontLeftWheel"></param>
        /// <param name="frontRightWheel"></param>
        /// <param name="rearLeftWheel"></param>
        /// <param name="rearRightWheel"></param>
        public void AddWheels(MataliObject frontLeftWheel, MataliObject frontRightWheel,
            MataliObject rearLeftWheel, MataliObject rearRightWheel)
        {
            if (hasWheels)
                throw new GoblinException("You already have wheels for this vehicle");

            wheelObjects[0].Add(frontLeftWheel);
            wheelObjects[1].Add(frontRightWheel);
            wheelObjects[2].Add(rearLeftWheel);
            wheelObjects[3].Add(rearRightWheel);

            hasWheels = true;
        }

        /// <summary>
        /// Add four wheels that are made of composited objects and have the same graphical 
        /// geometry and physical properties.
        /// </summary>
        /// <param name="wheelComposite"></param>
        public void AddWheels(List<MataliObject> wheelComposite)
        {
            AddWheels(wheelComposite, wheelComposite, wheelComposite, wheelComposite);
        }

        public void AddWheels(List<MataliObject> frontLeftWheelComposite, List<MataliObject> frontRightWheelComposite,
            List<MataliObject> rearLeftWheelComposite, List<MataliObject> rearRightWheelComposite)
        {
            if (hasWheels)
                throw new GoblinException("You already have wheels for this vehicle");

            wheelObjects[0].AddRange(frontLeftWheelComposite);
            wheelObjects[1].AddRange(frontRightWheelComposite);
            wheelObjects[2].AddRange(rearLeftWheelComposite);
            wheelObjects[3].AddRange(rearRightWheelComposite);

            hasWheels = true;
        }

        /// <summary>
        /// Add doors that can fling. If you don't need certain doors, just pass null (e.g., if your car
        /// only has front doors, then pass rearLeftDoor and rearRightDoor as null).
        /// </summary>
        /// <remarks>
        /// Doors are optional.
        /// </remarks>
        /// <param name="frontLeftDoor"></param>
        /// <param name="frontRightDoor"></param>
        /// <param name="rearLeftDoor"></param>
        /// <param name="rearRightDoor"></param>
        public void AddDoors(MataliObject frontLeftDoor, MataliObject frontRightDoor,
            MataliObject rearLeftDoor, MataliObject rearRightDoor)
        {
            if (hasDoors)
                throw new GoblinException("You already have doors for this vehicle");

            doorObjects[0].Add(frontLeftDoor);
            doorObjects[1].Add(frontRightDoor);
            doorObjects[2].Add(rearLeftDoor);
            doorObjects[3].Add(rearRightDoor);

            hasDoors = true;
        }

        /// <summary>
        /// Add doors that are made of a composite object and can fling. If you don't need certain doors, 
        /// just pass null (e.g., if your car only has front doors, then pass rearLeftDoorComposite and 
        /// rearRightDoorComposite as null).
        /// </summary>
        /// <remarks>
        /// Doors are optional.
        /// </remarks>
        /// <param name="frontLeftDoor"></param>
        /// <param name="frontRightDoor"></param>
        /// <param name="rearLeftDoor"></param>
        /// <param name="rearRightDoor"></param>
        public void AddDoors(List<MataliObject> frontLeftDoorComposite, List<MataliObject> frontRightDoorComposite,
            List<MataliObject> rearLeftDoorComposite, List<MataliObject> rearRightDoorComposite)
        {
            if (hasDoors)
                throw new GoblinException("You already have doors for this vehicle");

            doorObjects[0].AddRange(frontLeftDoorComposite);
            doorObjects[1].AddRange(frontRightDoorComposite);
            doorObjects[2].AddRange(rearLeftDoorComposite);
            doorObjects[3].AddRange(rearRightDoorComposite);

            hasDoors = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Steering wheel is optional. You can still steer the car without this object.
        /// </remarks>
        /// <param name="steeringObject"></param>
        public void AddSteering(MataliObject steeringObject)
        {
            if (hasSteering)
                throw new GoblinException("You already have steering for this vehicle");

            steeringObjects.Add(steeringObject);

            hasSteering = true;
        }

        public void AddSteering(List<MataliObject> steeringComposite)
        {
            if (hasSteering)
                throw new GoblinException("You already have steering for this vehicle");

            steeringObjects.AddRange(steeringComposite);

            hasSteering = true;
        }

        public void AddConstraint(string name, IPhysicsObject physObj1, IPhysicsObject physObj2, 
            CreateConstraintCallback callback)
        {
            ConstraintPair pair = new ConstraintPair();

            pair.Name = name;
            pair.Callback = callback;
            pair.PhysicsObject1 = physObj1;
            pair.PhysicsObject2 = physObj2;

            constraints.Add(pair);
        }

        #endregion
    }
}
