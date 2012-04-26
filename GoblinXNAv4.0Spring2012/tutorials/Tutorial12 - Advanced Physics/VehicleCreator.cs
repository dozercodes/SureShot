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

using NewtonDynamics;

namespace Tutorial12___Advanced_Physics
{
    /// <summary>
    /// A helper class for creating newton vehicles. This class mimics the tutorial 9 of
    /// the original Newton tutorials written in C++.
    /// </summary>
    public class VehicleCreator
    {
        public static RaceCar AddRaceCar(Scene scene, TransformNode parentTrans)
        {
            TransformNode transNode = new TransformNode();
            transNode.Translation = new Vector3(0, 10, 10);

            Material carMat = new Material();
            carMat.Diffuse = Color.Pink.ToVector4();
            carMat.Specular = Color.White.ToVector4();
            carMat.SpecularPower = 10;

            GeometryNode carNode = new GeometryNode("Race Car");
            carNode.Model = new Box(3, 1.0f, 2);
            carNode.Material = carMat;
            carNode.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

            NewtonPhysics physicsEngine = (NewtonPhysics)scene.PhysicsEngine;

            RaceCar car = new RaceCar(carNode, physicsEngine);
            for (int i = 0; i < 4; i++)
                car.Tires[i] = CreateTire((TireID)Enum.ToObject(typeof(TireID), i), 
                    car.TireTransformNode[i], carNode, scene.PhysicsEngine.Gravity);

            car.Collidable = true;
            car.Interactable = true;

            carNode.Physics = car;
            carNode.Physics.NeverDeactivate = true;
            carNode.AddToPhysicsEngine = true;

            parentTrans.AddChild(transNode);
            transNode.AddChild(carNode);

            Newton.NewtonSetBodyLeaveWorldEvent(physicsEngine.NewtonWorld,
                car.LeaveWorldCallback);

            return car;
        }

        private static NewtonTire CreateTire(TireID tireID, TransformNode tireTrans,
            GeometryNode carNode, float gravity)
        {
            NewtonTire tire = new NewtonTire();

            Material tireMat = new Material();
            tireMat.Diffuse = Color.Orange.ToVector4();
            tireMat.Specular = Color.White.ToVector4();
            tireMat.SpecularPower = 10;

            float tireRadius = 0.7f;
            GeometryNode tireNode = new GeometryNode("Race Car " + tireID + " Tire");
            tireNode.Model = new Cylinder(tireRadius, tireRadius, 0.3f, 20);
            tireNode.Material = tireMat;

            carNode.AddChild(tireTrans);
            tireTrans.AddChild(tireNode);

            tire.Mass = 5.0f;
            tire.Width = 0.3f * 1.25f;
            tire.Radius = tireRadius;

            switch (tireID)
            {
                case TireID.FrontLeft:
                    tire.TireOffsetMatrix = Matrix.CreateTranslation(new Vector3(-1.5f, 0, -1f));
                    break;
                case TireID.FrontRight:
                    tire.TireOffsetMatrix = Matrix.CreateTranslation(new Vector3(-1.5f, 0, 1f));
                    break;
                case TireID.RearLeft:
                    tire.TireOffsetMatrix = Matrix.CreateTranslation(new Vector3(1.5f, 0, -1f));
                    break;
                case TireID.RearRight:
                    tire.TireOffsetMatrix = Matrix.CreateTranslation(new Vector3(1.5f, 0, 1f));
                    break;
            }

            // the tire will spin around the lateral axis of the same tire space
            tire.Pin = Vector3.UnitZ;

            tire.SuspensionLength = RaceCar.SUSPENSION_LENGTH;

            // calculate the spring and damper contact for the subquestion  
            //
            // from the equation of a simple spring the force is given by
            // a = k * x
            // where k is a spring constant and x is the displacement and a is the spring acceleration.
            // we desire that a rest length the acceleration generated by the spring equal that of the gravity
            // several gravity forces
            // m * gravity = k * SUSPENSION_LENGTH * 0.5f, 
            float x = RaceCar.SUSPENSION_LENGTH;
            tire.SuspensionSpring = (200.0f * (float)Math.Abs(gravity)) / x;
            //tireSuspesionSpring = (100.0f * dAbs (GRAVITY)) / x;

            // from the equation of a simple spring / damper system
            // the system resonate at a frequency 
            // w^2 = ks
            //
            // and the damping attenuation is given by
            // d = ks / 2.0f
            // where:
            // if   d = w = 2 * pi * f -> the system is critically damped
            // if   d > w < 2 * pi * f -> the system is super critically damped
            // if   d < w < 2 * pi * f -> the system is under damped damped 
            // for a vehicle we usually want a suspension that is about 10% super critically damped 
            float w = (float)Math.Sqrt(tire.SuspensionSpring);

            // a critically damped suspension act too jumpy for a race car
            tire.SuspensionShock = 1.0f * w;

            // make it a little super critical y damped
            //tireSuspesionShock = 2.0f * w;

            tire.CollisionID = 0x100;

            return tire;
        }
    }
}
