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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GoblinXNA;
using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics;
using GoblinXNA.Graphics.ParticleEffects;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Generic;
using GoblinXNA.Device;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.Helpers;
using GoblinXNA.UI;

using NewtonDynamics;

namespace Tutorial12___Advanced_Physics
{
    /// <summary>
    /// Creates a race car based on the NewtonVehicle class. This race car is same as
    /// the race car implementation of the tutorial 9 of original Newton tutorials written 
    /// in C++.
    /// </summary>
    public class RaceCar : NewtonVehicle
    {
        public static float MAX_TORQUE = 2400.0f;
        public static float VEHICLE_MASS = 1000.0f;
        public static float SUSPENSION_FREQUENCE = 2.0f;
        public static float SUSPENSION_LENGTH = 0.3f;

        public static float MAX_STEER_ANGLE = 30.0f * MathHelper.Pi / 180.0f;

        private NewtonPhysics engine;

        // A callback triggered when this car leaves the newton world (outside of the
        // simulation bound)
        private Newton.NewtonBodyLeaveWorld bodyLeaveCallback;

        private TransformNode [] tireTransNode;

        public RaceCar(Object container, NewtonPhysics engine) : base(container)
        {
            this.engine = engine;

            tireTransNode = new TransformNode[4];
            for (int i = 0; i < 4; i++)
                tireTransNode[i] = new TransformNode();

            // set the mass of the race car
            mass = VEHICLE_MASS;

            // lower the center of the mass for a race car
            centerOfMass = -Vector3.UnitY * 1.5f;

            // sets up the callback function when body moves in the simulation
            transformCallback = delegate(IntPtr body, float[] matrix)
            {
                Matrix mat = MatrixHelper.FloatsToMatrix(matrix);

                // set the transformation of the vehicle body
                PhysicsWorldTransform = mat;

                Matrix invMat = Matrix.Invert(mat);
                float[] tireMatrix = new float[16];
                NewtonTire tire = null;
                float sign = 0;
                float angle = 0;
                float brakePosition = 0;

                // set the global matrix for each tire
                for (IntPtr tyreId = Newton.NewtonVehicleGetFirstTireID(joint);
                    tyreId != IntPtr.Zero; tyreId = Newton.NewtonVehicleGetNextTireID(joint, tyreId))
                {
                    int tireID = (int)GetTireID(tyreId);
                    tire = tires[tireID];
                    Newton.NewtonVehicleGetTireMatrix(joint, tyreId, tireMatrix);

                    // calculate the local matrix
                    Matrix tireMat = MatrixHelper.GetRotationMatrix(
                        MatrixHelper.FloatsToMatrix(tireMatrix) * invMat) * tire.TireOffsetMatrix;
                    tire.TireMatrix = tireMat;
                    tireTransNode[tireID].WorldTransformation = Matrix.CreateRotationX(MathHelper.PiOver2)
                        * tireMat;

                    // calcualte the parametric brake position
                    brakePosition = tireMat.Translation.Y - tire.TireRefHeight;
                    tire.BrakeMatrix = Matrix.CreateTranslation(0, tire.BrakeRefPosition.Y + brakePosition, 0);

                    // set suspensionMatrix
                    sign = (tire.BrakeRefPosition.Z > 0) ? 1 : -1;
                    angle = (float)Math.Atan2(sign * brakePosition, Math.Abs(tire.BrakeRefPosition.Z));
                    Matrix rotationMatrix = new Matrix(
                        1, 0, 0, 0,
                        0, (float)Math.Cos(angle), (float)Math.Sin(angle), 0,
                        0, -(float)Math.Sin(angle), (float)Math.Cos(angle), 0,
                        0, 0, 0, 1);
                    tire.AxelMatrix = rotationMatrix * tire.AxelMatrix;
                    tire.SuspensionTopMatrix = rotationMatrix * tire.SuspensionTopMatrix;
                    tire.SuspensionBottomMatrix = rotationMatrix * tire.SuspensionBottomMatrix;
                }
            };

            forceCallback = delegate(IntPtr body)
            {
                float Ixx = 0, Iyy = 0, Izz = 0, tmpMass = 0;

                Newton.NewtonBodyGetMassMatrix(body, ref tmpMass, ref Ixx, ref Iyy, ref Izz);
                tmpMass *= (1.0f + (float)Math.Abs(GetSpeed()) / 20.0f);
                float[] force = Vector3Helper.ToFloats(engine.GravityDirection * engine.Gravity * tmpMass);

                Newton.NewtonBodySetForce(body, force);
            };

            tireUpdate = delegate(IntPtr vehicleJoint)
            {
                NewtonTire tire = null;

                for (IntPtr tyreId = Newton.NewtonVehicleGetFirstTireID(vehicleJoint);
                    tyreId != IntPtr.Zero; tyreId = Newton.NewtonVehicleGetNextTireID(vehicleJoint, tyreId))
                {
                    tire = tires[(int)GetTireID(tyreId)];

                    // If the tire is a front tire
                    if ((tire == tires[(int)TireID.FrontLeft]) || (tire == tires[(int)TireID.FrontRight]))
                    {
                        float currSteerAngle = Newton.NewtonVehicleGetTireSteerAngle(vehicleJoint, tyreId);
                        Newton.NewtonVehicleSetTireSteerAngle(vehicleJoint, tyreId,
                            currSteerAngle + (tire.Steer - currSteerAngle) * 0.035f);
                    }
                    else // if the tire is a rear tire
                    {
                        Newton.NewtonVehicleSetTireTorque(vehicleJoint, tyreId, tire.Torque);

                        if (tire.Brakes > 0)
                        {
                            // ask Newton for the precise acceleration needed to stop the tire
                            float brakeAcceleration =
                                Newton.NewtonVehicleTireCalculateMaxBrakeAcceleration(vehicleJoint, tyreId);

                            // tell Newton you want this tire stoped but only if the torque need it is less than 
                            // the brakes pad can withstand (assume max brake pad torque is 500 newton * meter)
                            Newton.NewtonVehicleTireSetBrakeAcceleration(vehicleJoint, tyreId,
                                brakeAcceleration, 10000.0f);

                            // set some side slip as function of the linear speed
                            float speed = Newton.NewtonVehicleGetTireLongitudinalSpeed(vehicleJoint, tyreId);
                            Newton.NewtonVehicleSetTireMaxSideSleepSpeed(vehicleJoint, tyreId, speed * 0.1f);
                        }
                    }
                }
            };

            bodyLeaveCallback = delegate(IntPtr body)
            {
                Respawn(body);
            };
        }

        public TransformNode[] TireTransformNode
        {
            get { return tireTransNode; }
        }

        public Newton.NewtonBodyLeaveWorld LeaveWorldCallback
        {
            get { return bodyLeaveCallback; }
        }

        public float GetSpeed()
        {
            return Vector3.Dot(this.PhysicsWorldTransform.Forward, engine.GetVelocity(this));
        }

        /// <summary>
        /// Sets the front tires' steering angle. The angle depends on the speed of the car 
        /// (the more the speed, the less the angle is).
        /// </summary>
        /// <param name="value"></param>
        public override void SetSteering(float value)
        {
            float speed = (float)Math.Abs(GetSpeed());
            float scale = ((1.0f - speed / 60.0f) > 0.1f) ? (1.0f - speed / 60.0f) : 0.1f;
            if (value > 0)
                value = MAX_STEER_ANGLE * scale;
            else if (value < 0)
                value = -MAX_STEER_ANGLE * scale;
            else
                value = 0;

            tires[(int)TireID.FrontRight].Steer = value;
            tires[(int)TireID.FrontLeft].Steer = value;
        }

        public override void SetTireTorque(float value)
        {
            float speed = (float)Math.Abs((float)Math.Abs(GetSpeed()));

            if(value > 0)
                value = MAX_TORQUE * (1.0f - speed / 200.0f);
            else if(value < 0)
                value = -MAX_TORQUE * 0.5f * (1.0f - speed / 200.0f);
            else
                value = 0;

            tires[(int)TireID.RearRight].Torque = value;
            tires[(int)TireID.RearLeft].Torque = value;
        }

        public override void ApplyHandBrakes(float value)
        {
            tires[(int)TireID.RearRight].Brakes = value;
            tires[(int)TireID.RearLeft].Brakes = value;
        }

        public void Respawn(IntPtr body)
        {
            Notifier.AddMessage("Respawned");
            Matrix mat = Matrix.CreateTranslation(0, 0, -10);
            Newton.NewtonBodySetMatrixRecursive(body, MatrixHelper.ToFloats(mat));
        }
    }
}
