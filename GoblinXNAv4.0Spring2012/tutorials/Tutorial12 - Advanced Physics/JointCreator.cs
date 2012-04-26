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
using System.Runtime.InteropServices;
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
    /// A helper class for creating various newton joints. This class mimics the tutorial 5 of
    /// the original Newton tutorials written in C++.
    /// </summary>
    public class JointCreator
    {
        /// <summary>
        /// A callback function for a hinge joint used for the double door and rolling beats.
        /// </summary>
        private static Newton.NewtonHinge doubleDoor = delegate(IntPtr hingeJoint,
            ref Newton.NewtonHingeSliderUpdateDesc desc)
        {
            float angle;

            // set the angle limit to 90 degrees for the hinge
            float angleLimit = 90;

            angle = Newton.NewtonHingeGetJointAngle(hingeJoint);

            // if the hinge's angle is greater than the limit, then stop it
            if (angle > ((angleLimit / 180.0f) * MathHelper.Pi))
            {
                desc.m_Accel = Newton.NewtonHingeCalculateStopAlpha(hingeJoint, ref desc,
                    (angleLimit / 180.0f) * MathHelper.Pi);
                return 1;
            }
            // if the hinge's angle is greater than the limit on the reverse side, then also stop it
            else if (angle < ((-angleLimit / 180.0f) * MathHelper.Pi))
            {
                desc.m_Accel = Newton.NewtonHingeCalculateStopAlpha(hingeJoint, ref desc,
                    (-angleLimit / 180.0f) * MathHelper.Pi);
                return 1;
            }

            // no action need it if the joint angle is with the limits
            return 0;
        };

        private static Newton.NewtonSlider sliderUpdate;
        private static Newton.NewtonCorkscrew corkscrewUpdate;
        private static Newton.NewtonUniversal universalUpdate;

        private static Vector2 sliderLimit;
        private static Vector2 corkscrewLimit;
        private static Vector2 universalLimit;

        /// <summary>
        /// Creates a rope-like object by connecting several ball and socket joints.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="parentTrans"></param>
        public static void AddRope(Scene scene, TransformNode parentTrans)
        {
            Vector3 size = new Vector3(1.5f, 0.25f, 0.25f);
            Vector3 location = new Vector3(0, 9, 0);

            IPhysicsObject link0 = null;
            Color[] colors = {Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue,
                                  Color.Indigo, Color.Violet};

            PrimitiveModel capsule = new Capsule(size.Y, size.X, 12);
            for (int i = 0; i < 7; i++)
            {
                TransformNode pileTrans = new TransformNode();
                pileTrans.Translation = location;

                Material linkMat = new Material();
                linkMat.Diffuse = colors[i].ToVector4();
                linkMat.Specular = Color.White.ToVector4();
                linkMat.SpecularPower = 10;

                GeometryNode link1 = new GeometryNode("Link " + i);
                link1.Model = capsule;
                link1.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;
                link1.Material = linkMat;

                link1.AddToPhysicsEngine = true;
                link1.Physics.Interactable = true;
                link1.Physics.Collidable = true;
                link1.Physics.Shape = ShapeType.Capsule;
                link1.Physics.Mass = 2.0f;
                link1.Physics.LinearDamping = 1f;
                link1.Physics.AngularDamping = new Vector3(1f, 1f, 1f);

                parentTrans.AddChild(pileTrans);
                pileTrans.AddChild(link1);

                Vector3 pivot = location + parentTrans.Translation;
                pivot.Y += (size.X - size.Y) * 0.5f;

                BallAndSocketJoint joint = new BallAndSocketJoint(pivot);
                joint.Pin = -Vector3.UnitY;
                joint.MaxConeAngle = 0.0f;
                joint.MaxTwistAngle = 10.0f * MathHelper.Pi / 180;

                ((NewtonPhysics)scene.PhysicsEngine).CreateJoint(link1.Physics, link0, joint);

                link0 = link1.Physics;
                location = new Vector3(location.X, location.Y
                    - (size.X - size.Y), location.Z);
            }
        }

        public static void AddDoubleSwingDoors(Scene scene, TransformNode parentTrans)
        {
            Vector3 size = new Vector3(2.0f, 5.0f, 0.5f);
            Vector3 location = new Vector3(0, 3.5f, -3);

            Material linkMat = new Material();
            linkMat.Diffuse = Color.Cyan.ToVector4();
            linkMat.Specular = Color.White.ToVector4();
            linkMat.SpecularPower = 10;

            GeometryNode link1 = null;
            // Make first wing
            {
                TransformNode pileTrans = new TransformNode();
                pileTrans.Translation = location;

                link1 = new GeometryNode("Door 1");
                link1.Model = new Box(size.X, size.Y, size.Z);
                link1.Material = linkMat;
                link1.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

                link1.AddToPhysicsEngine = true;
                link1.Physics.Interactable = true;
                link1.Physics.Collidable = true;
                link1.Physics.Shape = ShapeType.Box;
                link1.Physics.Mass = 2.0f;

                parentTrans.AddChild(pileTrans);
                pileTrans.AddChild(link1);

                Vector3 pivot = location + parentTrans.Translation;
                Vector3 pin = Vector3.UnitY;
                pivot.X += size.X * 0.5f;
                pivot.Y -= size.Y * 0.5f;

                HingeJoint joint = new HingeJoint(pivot, pin);
                joint.NewtonHingeCallback = doubleDoor;

                ((NewtonPhysics)scene.PhysicsEngine).CreateJoint(link1.Physics, null, joint);
            }

            // Make second wing
            {
                location.X -= size.X;

                TransformNode pileTrans = new TransformNode();
                pileTrans.Translation = location;

                GeometryNode link2 = new GeometryNode("Door 2");
                link2.Model = new Box(size.X, size.Y, size.Z);
                link2.Material = linkMat;
                link2.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

                link2.AddToPhysicsEngine = true;
                link2.Physics.Interactable = true;
                link2.Physics.Collidable = true;
                link2.Physics.Shape = ShapeType.Box;
                link2.Physics.Mass = 2.0f;

                parentTrans.AddChild(pileTrans);
                pileTrans.AddChild(link2);

                Vector3 pivot = location + parentTrans.Translation;
                Vector3 pin = Vector3.UnitY;
                pivot.X += size.X * 0.5f;
                pivot.Y -= size.Y * 0.5f;

                HingeJoint joint = new HingeJoint(pivot, pin);
                joint.NewtonHingeCallback = doubleDoor;

                ((NewtonPhysics)scene.PhysicsEngine).CreateJoint(link2.Physics, link1.Physics, joint);
            }
        }

        public static void AddRollingBeats(Scene scene, TransformNode parentTrans)
        {
            Vector3 size = new Vector3(10.0f, 0.25f, 0.25f);
            Vector3 location = new Vector3(0, 3, 3);

            GeometryNode bar;
            // //////////////////////////////////////////////////////////////////            
            //
            // Create a bar and attach it to the world with a hinge with limits
            //
            // //////////////////////////////////////////////////////////////////
            {
                TransformNode pileTrans = new TransformNode();
                pileTrans.Translation = location;
                pileTrans.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);

                Material barMat = new Material();
                barMat.Diffuse = Color.Purple.ToVector4();
                barMat.Specular = Color.White.ToVector4();
                barMat.SpecularPower = 10;

                bar = new GeometryNode("Bar");
                bar.Model = new Cylinder(size.Y, size.Y, size.X, 20);
                bar.Material = barMat;
                bar.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

                bar.AddToPhysicsEngine = true;
                bar.Physics.Interactable = true;
                bar.Physics.Collidable = true;
                bar.Physics.Shape = ShapeType.Cylinder;
                bar.Physics.Mass = 2.0f;

                parentTrans.AddChild(pileTrans);
                pileTrans.AddChild(bar);

                Vector3 pivot = location + parentTrans.Translation;
                Vector3 pin = Vector3.UnitY;
                pivot.X -= size.X * 0.5f;

                HingeJoint joint = new HingeJoint(pivot, pin);
                joint.NewtonHingeCallback = doubleDoor;

                ((NewtonPhysics)scene.PhysicsEngine).CreateJoint(bar.Physics, null, joint);
            }

            // /////////////////////////////////////////////////////////////////
            //
            // Add a sliding visualObject with limits
            //
            ////////////////////////////////////////////////////////////////////
            {
                Vector3 beatLocation = location;
                Vector3 beatSize = new Vector3(0.5f, 2.0f, 2.0f);

                beatLocation.X += size.X * 0.25f;

                TransformNode pileTrans = new TransformNode();
                pileTrans.Translation = beatLocation;

                Material beatMat = new Material();
                beatMat.Diffuse = Color.Red.ToVector4();
                beatMat.Specular = Color.White.ToVector4();
                beatMat.SpecularPower = 10;

                GeometryNode beat = new GeometryNode("Beat Slider");
                beat.Model = new Box(beatSize);
                beat.Material = beatMat;
                beat.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

                beat.AddToPhysicsEngine = true;
                beat.Physics.Interactable = true;
                beat.Physics.Collidable = true;
                beat.Physics.Shape = ShapeType.Box;
                beat.Physics.Mass = 2.0f;

                parentTrans.AddChild(pileTrans);
                pileTrans.AddChild(beat);

                Vector3 pivot = beatLocation + parentTrans.Translation;
                Vector3 pin = Vector3.UnitX;

                sliderLimit.X = ((location.X - beatLocation.X) - size.X * 0.5f);
                sliderLimit.Y = ((location.X - beatLocation.X) + size.X * 0.5f);

                SliderJoint joint = new SliderJoint(pivot, pin);
                sliderUpdate = delegate(IntPtr slider, ref Newton.NewtonHingeSliderUpdateDesc desc)
                {
                    float distance = Newton.NewtonSliderGetJointPosit(slider);
                    if (distance < sliderLimit.X)
                    {
                        // if the distance is smaller than the predefine interval, stop the slider
                        desc.m_Accel = Newton.NewtonSliderCalculateStopAccel(slider, ref desc, sliderLimit.X);
                        return 1;
                    }
                    else if (distance > sliderLimit.Y)
                    {
                        // if the distance is larger than the predefine interval, stop the slider
                        desc.m_Accel = Newton.NewtonSliderCalculateStopAccel(slider, ref desc, sliderLimit.Y);
                        return 1;
                    }

                    // no action need it if the joint angle is with the limits
                    return 0;
                };
                joint.NewtonSliderCallback = sliderUpdate;

                ((NewtonPhysics)scene.PhysicsEngine).CreateJoint(beat.Physics, bar.Physics, joint);
            }

            // /////////////////////////////////////////////////////////////////
            //
            // Add a corkscrew visualObject with limits
            //
            // /////////////////////////////////////////////////////////////////
            {
                Vector3 beatLocation = location;
                Vector3 beatSize = new Vector3(0.5f, 2.0f, 2.0f);

                beatLocation.X -= size.X * 0.25f;

                TransformNode pileTrans = new TransformNode();
                pileTrans.Translation = beatLocation;

                Material beatMat = new Material();
                beatMat.Diffuse = Color.YellowGreen.ToVector4();
                beatMat.Specular = Color.White.ToVector4();
                beatMat.SpecularPower = 10;

                GeometryNode beat = new GeometryNode("Beat Corkscrew");
                beat.Model = new Box(beatSize);
                beat.Material = beatMat;
                beat.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

                beat.AddToPhysicsEngine = true;
                beat.Physics.Interactable = true;
                beat.Physics.Collidable = true;
                beat.Physics.Shape = ShapeType.Box;
                beat.Physics.Mass = 2.0f;

                parentTrans.AddChild(pileTrans);
                pileTrans.AddChild(beat);

                Vector3 pivot = beatLocation + parentTrans.Translation;
                Vector3 pin = Vector3.UnitX;

                corkscrewLimit.X = ((location.X - beatLocation.X) - size.X * 0.5f);
                corkscrewLimit.Y = ((location.X - beatLocation.X) + size.X * 0.5f);

                CorkscrewJoint joint = new CorkscrewJoint(pivot, pin);
                corkscrewUpdate = delegate(IntPtr corkscrew, Newton.NewtonHingeSliderUpdateDesc[] desc)
                {
                    // no action need it if the joint angle is with the limits
                    uint retCode = 0;

                    float distance = Newton.NewtonCorkscrewGetJointPosit(corkscrew);

                    // The first entry in NewtonHingeSliderUpdateDesc control the screw linear acceleration 
                    if (distance < corkscrewLimit.X)
                    {
                        // if the distance is smaller than the predefine interval, stop the slider
                        desc[0].m_Accel = Newton.NewtonCorkscrewCalculateStopAccel(corkscrew,
                            ref desc[0], corkscrewLimit.X);
                        retCode |= 1;
                    }
                    else if (distance > corkscrewLimit.Y)
                    {
                        // if the distance is larger than the predefine interval, stop the slider
                        desc[0].m_Accel = Newton.NewtonCorkscrewCalculateStopAccel(corkscrew, ref 
                            desc[0], corkscrewLimit.Y);
                        retCode |= 1;
                    }

                    // The second entry in NewtonHingeSliderUpdateDesc control the screw angular acceleration. 
                    // Make s small screw motor by setting the angular acceleration of the screw axis
                    // We are not going to limit the angular rotation of the screw, but is we did we should 
                    // or return code with 2
                    float omega = Newton.NewtonCorkscrewGetJointOmega(corkscrew);
                    desc[1].m_Accel = 1.5f - 0.2f * omega;

                    // or with 0x10 to tell newton this axis is active
                    retCode |= 2;

                    // return the code
                    return retCode;
                };
                joint.NewtonCorkscrewCallback = corkscrewUpdate;

                ((NewtonPhysics)scene.PhysicsEngine).CreateJoint(beat.Physics, bar.Physics, joint);
            }

            // /////////////////////////////////////////////////////////////////
            //
            // Add a universal joint visualObject with limits
            //
            // /////////////////////////////////////////////////////////////////
            {
                Vector3 beatLocation = location;
                Vector3 beatSize = new Vector3(0.5f, 2.0f, 2.0f);

                beatLocation.X -= size.X * 0.45f;

                TransformNode pileTrans = new TransformNode();
                pileTrans.Translation = beatLocation;

                Material beatMat = new Material();
                beatMat.Diffuse = Color.YellowGreen.ToVector4();
                beatMat.Specular = Color.White.ToVector4();
                beatMat.SpecularPower = 10;

                GeometryNode beat = new GeometryNode("Beat Universal");
                beat.Model = new Box(beatSize);
                beat.Material = beatMat;
                beat.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

                beat.AddToPhysicsEngine = true;
                beat.Physics.Interactable = true;
                beat.Physics.Collidable = true;
                beat.Physics.Shape = ShapeType.Box;
                beat.Physics.Mass = 2.0f;

                parentTrans.AddChild(pileTrans);
                pileTrans.AddChild(beat);

                Vector3 pivot = beatLocation + parentTrans.Translation;
                Vector3 pin0 = Vector3.UnitX;
                Vector3 pin1 = Vector3.UnitY;

                universalLimit.X = -30.0f * MathHelper.Pi / 180.0f;
                universalLimit.Y = 30.0f * MathHelper.Pi / 180.0f;

                UniversalJoint joint = new UniversalJoint(pivot, pin0, pin1);
                universalUpdate = delegate(IntPtr universal, Newton.NewtonHingeSliderUpdateDesc[] desc)
                {
                    // no action need it if the joint angle is with the limits
                    uint retCode = 0;

                    float omega = Newton.NewtonUniversalGetJointOmega0(universal);
                    desc[0].m_Accel = -1.5f - 0.2f * omega;
                    retCode |= 1;

                    float angle = Newton.NewtonUniversalGetJointAngle1(universal);
                    if (angle < universalLimit.X)
                    {
                        desc[1].m_Accel = Newton.NewtonUniversalCalculateStopAlpha1(universal, ref desc[1],
                            universalLimit.X);
                        retCode |= 2;
                    }
                    else if (angle > universalLimit.Y)
                    {
                        desc[1].m_Accel = Newton.NewtonUniversalCalculateStopAlpha1(universal, ref desc[1],
                            universalLimit.Y);
                        retCode |= 2;
                    }

                    // return the code
                    return retCode;
                };
                joint.NewtonUniversalCallback = universalUpdate;

                ((NewtonPhysics)scene.PhysicsEngine).CreateJoint(beat.Physics, bar.Physics, joint);
            }
        }

    }
}
