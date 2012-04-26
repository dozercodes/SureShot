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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GoblinXNA;
using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics;
using GoblinXNA.Device.Generic;
using GoblinXNA.Graphics.Geometry;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.UI;
using GoblinXNA.UI.UI2D;

namespace Tutorial12___Advanced_Physics
{
    /// <summary>
    /// This tutorial demonstrates some of the advanced features of the Newton physics including
    /// joint physics (hinge joint, ball and socket joint, corkscrew joint, and universal joint)
    /// and vehicle physics. This tutorial also shows how to directly call the methods in the
    /// original Newton physics.
    /// </summary>
    public class Tutorial12 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Scene scene;
        RaceCar car;
        SpriteFont textFont;

        public Tutorial12()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Initialize the GoblinXNA framework
            State.InitGoblin(graphics, Content, "");

            // Initialize the scene graph
            scene = new Scene();

            // Set the background color to CornflowerBlue color. 
            // GraphicsDevice.Clear(...) is called by Scene object with this color. 
            scene.BackgroundColor = Color.CornflowerBlue;

            // We will use the Newton physics engine (http://www.newtondynamics.com)
            // for processing the physical simulation
            scene.PhysicsEngine = new NewtonPhysics();

            // Set up the lights used in the scene
            CreateLights();

            // Create 3D objects
            CreateObjects();

            // Create a static camera near the center of the scene
            CreateStaticCamera(true);

            // Create a static camera far from the center of the scene
            CreateStaticCamera(false);

            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(KeyPressHandler);

            State.ShowFPS = true;

            // Display the notification messages
            State.ShowNotifications = true;

            // Set the notification messages to disappear after 1 second
            Notifier.FadeOutTime = 1000;
        }

        void KeyPressHandler(Keys key, KeyModifier modifier)
        {
            String cameraName = "";
            if (key == Keys.N)
                cameraName = "NearCamera";
            else if (key == Keys.F)
                cameraName = "FarCamera";
            else if (key == Keys.C)
                cameraName = "ChasingCamera";

            if (!cameraName.Equals(""))
            {
                CameraNode cam = (CameraNode)scene.GetNode(cameraName);

                // Set the selected camera to be our active camera
                scene.CameraNode = cam;
            }
        }

        private void CreateStaticCamera(bool near)
        {
            // Set up the camera of the scene graph
            Camera camera = new Camera();

            if (near)
                camera.Translation = new Vector3(0, 0, 0);
            else
                camera.Translation = new Vector3(0, 10, 55);

            // Rotate the camera -20 degrees along the X axis
            camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                MathHelper.ToRadians(-20));
            // Set the vertical field of view to be 45 degrees
            camera.FieldOfViewY = MathHelper.ToRadians(45);
            // Set the near clipping plane to be 0.1f unit away from the camera
            camera.ZNearPlane = 0.1f;
            // Set the far clipping plane to be 1000 units away from the camera
            camera.ZFarPlane = 1000;

            String prefix = (near) ? "Near" : "Far";
            // Add this camera node to our scene graph
            CameraNode cameraNode = new CameraNode(prefix + "Camera", camera);
            scene.RootNode.AddChild(cameraNode);
        }

        /// <summary>
        /// Creates a camera that chases a given object.
        /// </summary>
        /// <param name="chasedObject"></param>
        private void CreateChasingCamera(GeometryNode chasedObject)
        {
            // Set up the camera of the scene graph
            Camera camera = new Camera();

            camera.Translation = new Vector3(5, 3, 0);
            // Rotate the camera -20 degrees along the X axis
            camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 
                MathHelper.ToRadians(90)) * Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                MathHelper.ToRadians(-20));
            // Set the vertical field of view to be 60 degrees
            camera.FieldOfViewY = MathHelper.ToRadians(45);
            // Set the near clipping plane to be 0.1f unit away from the camera
            camera.ZNearPlane = 0.1f;
            // Set the far clipping plane to be 1000 units away from the camera
            camera.ZFarPlane = 1000;

            // Add this camera node to a geometry node we want to chase for
            CameraNode cameraNode = new CameraNode("ChasingCamera", camera);
            chasedObject.AddChild(cameraNode);

            // Initially assign the chasing camera to be our scene graph's active camera node
            scene.CameraNode = cameraNode;
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(0, -1, -1);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.LightSource = lightSource;

            // Add this light node to the root node
            scene.RootNode.AddChild(lightNode);
        }

        private void CreateObjects()
        {
            // Create our ground plane
            GeometryNode groundNode = new GeometryNode("Ground");
            groundNode.Model = new Box(Vector3.One);
            // Make this ground plane collidable, so other collidable objects can collide
            // with this ground
            groundNode.Physics.Collidable = true;
            groundNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            groundNode.AddToPhysicsEngine = true;

            // Create a material for the ground
            Material groundMat = new Material();
            groundMat.Diffuse = Color.LightGreen.ToVector4();
            groundMat.Specular = Color.White.ToVector4();
            groundMat.SpecularPower = 20;

            groundNode.Material = groundMat;

            // Create a parent transformation for both the ground and the sphere models
            TransformNode parentTransNode = new TransformNode();
            parentTransNode.Translation = new Vector3(0, -10, -20);

            // Create a scale transformation for the ground to make it bigger
            TransformNode groundScaleNode = new TransformNode();
            groundScaleNode.Scale = new Vector3(100, 1, 100);

            // Add this ground model to the scene
            scene.RootNode.AddChild(parentTransNode);
            parentTransNode.AddChild(groundScaleNode);
            groundScaleNode.AddChild(groundNode);

            // Add a rope-like object using ball and socket joint
            JointCreator.AddRope(scene, parentTransNode);

            // Add a door-like object using hinge joint
            JointCreator.AddDoubleSwingDoors(scene, parentTransNode);

            JointCreator.AddRollingBeats(scene, parentTransNode);

            // Add a race car
            car = VehicleCreator.AddRaceCar(scene, parentTransNode);

            // Create a camera that chases the car
            CreateChasingCamera((GeometryNode)car.Container);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            textFont = Content.Load<SpriteFont>("Sample");
        }

        protected override void Dispose(bool disposing)
        {
            scene.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // If car is not null and already added to the physics engine
            if (car != null && ((NewtonPhysics)scene.PhysicsEngine).GetBody(car) != IntPtr.Zero)
            {
                // Gets the keyboard state
                KeyboardState keyboard = Keyboard.GetState();

                // Control the car steering with right and left arrow keys
                if (keyboard.IsKeyDown(Keys.Right))
                    car.SetSteering(-1);
                else if (keyboard.IsKeyDown(Keys.Left))
                    car.SetSteering(1);
                else
                    car.SetSteering(0);

                // Control the car's forward torque with up and down arrow keys
                if (keyboard.IsKeyDown(Keys.Up))
                    car.SetTireTorque(1);
                else if (keyboard.IsKeyDown(Keys.Down))
                    car.SetTireTorque(-1);
                else
                    car.SetTireTorque(0); 

                // Control the hand brake with space key
                if (keyboard.IsKeyDown(Keys.Space))
                    car.ApplyHandBrakes(1);
                else
                    car.ApplyHandBrakes(0); 
            }

            UI2DRenderer.WriteText(new Vector2(5, 30), "Press the following keys to change " +
                "the active camera:", Color.Red, textFont, Vector2.One * 0.5f);
            UI2DRenderer.WriteText(new Vector2(5, 50), "'C' -- Car chasing camera", 
                Color.Red, textFont, Vector2.One * 0.5f);
            UI2DRenderer.WriteText(new Vector2(5, 70), "'F' -- Far camera",
                Color.Red, textFont, Vector2.One * 0.5f);
            UI2DRenderer.WriteText(new Vector2(5, 90), "'N' -- Near camera",
                Color.Red, textFont, Vector2.One * 0.5f);

            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
    }
}
