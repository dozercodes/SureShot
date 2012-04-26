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
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Helpers;

namespace Tutorial13___Stereo_Rendering
{
    /// <summary>
    /// This tutorial demonstrates how to render stereo on the phone.
    /// </summary>
    public class Tutorial13_Phone : Microsoft.Xna.Framework.Game
    {
        // The gap on the center between the left and right screen to prevent the left eye
        // seeing the right eye view and the right eye seeing the left eye view
        const int CENTER_GAP = 16; // in pixels

        GraphicsDeviceManager graphics;

        Scene scene;

        RenderTarget2D stereoScreenLeft;
        RenderTarget2D stereoScreenRight;
        Rectangle leftRect;
        Rectangle rightRect;

        TransformNode modelTransformNode;
        float angle = 0;

        public Tutorial13_Phone()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);

            graphics.IsFullScreen = true;
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

            // Set up the lights used in the scene
            CreateLights();

            // Set up the stereo camera, which defines the location and viewing frustum of
            // left and right eyes
            SetupStereoCamera();

            // Set up the viewport for rendering stereo view
            SetupStereoViewport();

            // Create a 3D object
            CreateObject();
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(-0.8f, -0.5f, -1f);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.LightSource = lightSource;

            // Add this light node to the root node
            scene.RootNode.AddChild(lightNode);
        }

        private void SetupStereoCamera()
        {
            // Create a stereo camera
            StereoCamera camera = new StereoCamera();
            camera.Translation = new Vector3(0, 0, 0);
            camera.FieldOfViewY = MathHelper.ToRadians(60);
            camera.AspectRatio = ((State.Width - CENTER_GAP) / 2) / (float)State.Height;
            camera.ZNearPlane = 0.1f;
            camera.ZFarPlane = 1000;

            // Set the interpupillary distance which defines the distance between the left
            // and right eyes
            camera.InterpupillaryDistance = 5.5f; // 5.5 cm
            // Set the focal distance to be at infinity
            camera.FocalLength = float.MaxValue;

            CameraNode cameraNode = new CameraNode(camera);

            scene.RootNode.AddChild(cameraNode);
            scene.CameraNode = cameraNode;
        }

        private void SetupStereoViewport()
        {
            // Since we're doing split-screen stereo rendering, the width for each eye's rendered view
            // will be half of the entire screen
            int stereoWidth = (State.Width - CENTER_GAP) / 2;
            int stereoHeight = State.Height;

            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            stereoScreenLeft = new RenderTarget2D(GraphicsDevice, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);
            stereoScreenRight = new RenderTarget2D(GraphicsDevice, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);

            leftRect = new Rectangle(0, 0, stereoWidth, stereoHeight);
            rightRect = new Rectangle(stereoWidth + CENTER_GAP, 0, stereoWidth, stereoHeight);

            scene.BackgroundBound = leftRect;
        }

        private void CreateObject()
        {
            ModelLoader loader = new ModelLoader();

            // Create a geometry node with a model of a space ship 
            GeometryNode shipNode = new GeometryNode("Ship");
            shipNode.Model = (Model)loader.Load("", "p1_wedge");
            ((Model)shipNode.Model).UseInternalMaterials = true;

            // Create a transform node to define the transformation of this model
            // (Transformation includes translation, rotation, and scaling)
            modelTransformNode = new TransformNode();

            // Compute the right scale to apply to the model so that the max dimension
            // of the model will be 50.0 cm (cm is used here since we used cm measure
            // for setting our interpupillary distance)
            Vector3 dim = Vector3Helper.GetDimensions(shipNode.Model.MinimumBoundingBox);
            float scale = 50.0f / Math.Max(Math.Max(dim.X, dim.Y), dim.Z);

            modelTransformNode.Scale = new Vector3(scale, scale, scale);
            // Place the model 60 cm away from the viewer
            modelTransformNode.Translation = new Vector3(0, 0, -60);
            modelTransformNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY,
                MathHelper.ToRadians(90));

            scene.RootNode.AddChild(modelTransformNode);
            modelTransformNode.AddChild(shipNode);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                scene.Dispose();

                this.Exit();
            }

            // Keep rotating the model little by little
            angle += MathHelper.ToRadians(0.5f);
            modelTransformNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);

            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Set the render target to be the left screen render target
            scene.SceneRenderTarget = stereoScreenLeft;
            // Render the scene viewed from the left eye to the left screen render target
            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);

            // Set the render target to be the right screen render target
            scene.SceneRenderTarget = stereoScreenRight;
            // Render the scene viewed from the right eye to the right screen render target
            // NOTE: We use the light version of Draw function here for better performance
            scene.RenderScene(false, false);

            // Set the render target to be the default one (frame buffer)
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(scene.BackgroundColor);
            // Render the left and right render targets as textures
            State.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            State.SharedSpriteBatch.Draw(stereoScreenLeft, leftRect, Color.White);
            State.SharedSpriteBatch.Draw(stereoScreenRight, rightRect, Color.White);
            State.SharedSpriteBatch.End();
        }
    }
}
