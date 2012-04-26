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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;

namespace Tutorial14___Skinned_Model_Animation
{
    /// <summary>
    /// This tutorial demonstrates how you can incorporate an animated skinned model to
    /// Goblin XNA. 
    /// </summary>
    public class Tutorial14 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        Scene scene;
        AnimatedModel animatedModel;
        TransformNode modelTransNode;
        float elapsedTime = 0;

        public Tutorial14()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);

            graphics.IsFullScreen = true;
#endif
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

            // Set up the camera, which defines the eye location and viewing frustum
            CreateCamera();

            // Set up the lights used in the scene
            CreateLights();

            // Create a ground for the skinned model to walk around
            CreateGround();

            // Loads the skinned model
            LoadModel();
        }

        private void CreateCamera()
        {
            // Create a camera 
            Camera camera = new Camera();
            camera.Translation = new Vector3(0, 160, 180);
            camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathHelper.ToRadians(45));
            // Set the vertical field of view to be 60 degrees
            camera.FieldOfViewY = MathHelper.ToRadians(60);
            // Set the near clipping plane to be 0.1f unit away from the camera
            camera.ZNearPlane = 0.1f;
            // Set the far clipping plane to be 1000 units away from the camera
            camera.ZFarPlane = 1000;

            // Now assign this camera to a camera node, and add this camera node to our scene graph
            CameraNode cameraNode = new CameraNode(camera);
            scene.RootNode.AddChild(cameraNode);

            // Assign the camera node to be our scene graph's current camera node
            scene.CameraNode = cameraNode;
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(-1, -1, -1);
            lightSource.Diffuse = Color.White.ToVector4();

            LightSource lightSource2 = new LightSource();
            lightSource2.Direction = new Vector3(1, 0, 0);
            lightSource2.Diffuse = Color.White.ToVector4();

            LightSource lightSource3 = new LightSource();
            lightSource3.Direction = new Vector3(-0.5f, 0, 1);
            lightSource3.Diffuse = new Vector4(0.5f, 0.5f, 0.5f, 1);

            // Create a light node to hold the light source
            LightNode lightNode1 = new LightNode();
            lightNode1.LightSource = lightSource;

            LightNode lightNode2 = new LightNode();
            lightNode2.LightSource = lightSource2;

            LightNode lightNode3 = new LightNode();
            lightNode3.LightSource = lightSource3;

            scene.RootNode.AddChild(lightNode1);
            scene.RootNode.AddChild(lightNode2);
            scene.RootNode.AddChild(lightNode3);
        }

        private void CreateGround()
        {
            GeometryNode groundNode = new GeometryNode("Ground");
            groundNode.Model = new TexturedLayer(new Vector2(300, 300));

            Material groundMaterial = new Material();
            groundMaterial.Diffuse = Color.White.ToVector4();
            groundMaterial.Texture = Content.Load<Texture2D>("checkerboard");

            groundNode.Material = groundMaterial;

            scene.RootNode.AddChild(groundNode);
        }

        private void LoadModel()
        {
            AnimatedModelLoader loader = new AnimatedModelLoader();
            animatedModel = (AnimatedModel)loader.Load("", "dude");
            animatedModel.UseInternalMaterials = true;

            GeometryNode modelNode = new GeometryNode("Dude");
            modelNode.Model = animatedModel;

            animatedModel.LoadAnimationClip("Take 001");

            modelTransNode = new TransformNode();
            modelTransNode.Translation = new Vector3(20, 0, 0);

            scene.RootNode.AddChild(modelTransNode);
            modelTransNode.AddChild(modelNode);
        }

#if !WINDOWS_PHONE
        protected override void Dispose(bool disposing)
        {
            scene.Dispose();
        }
#endif

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
#if WINDOWS_PHONE
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                scene.Dispose();

                this.Exit();
            }
#endif

            animatedModel.Update(gameTime);

            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds / 2;

            Vector3 curPos = Vector3.Zero;
            curPos.X = (float)(100 * Math.Cos(elapsedTime));
            curPos.Z = (float)(100 * Math.Sin(elapsedTime));

            modelTransNode.Translation = curPos;

            modelTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY,
                -elapsedTime);

            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
    }
}
