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
using Microsoft.Xna.Framework.Media;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Helpers;
using GoblinXNA.Shaders;
using GoblinXNA.Device.Generic;

namespace Tutorial16___Multiple_Viewport
{
    /// <summary>
    /// This tutorial demonstrates how to create and render multiple viewports using Goblin XNA, one
    /// in AR mode, and another in VR mode.
    /// </summary>
    public class Tutorial16 : Game
    {
        GraphicsDeviceManager graphics;

        Scene scene;
        MarkerNode groundMarkerNode;
        GeometryNode markerBoardGeom;
        GeometryNode overlayNode;
        GeometryNode vrCameraRepNode;
        TransformNode vrCameraRepTransNode;

        CameraNode arCameraNode;
        CameraNode vrCameraNode;

        RenderTarget2D arViewRenderTarget;
        RenderTarget2D vrViewRenderTarget;
        Rectangle arViewRect;
        Rectangle vrViewRect;

        float markerSize = 32.4f;

        public Tutorial16()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
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

            // For some reason, it sometimes causes memory conflict when it attempts to update the
            // marker transformation in the multi-threaded code, so if you see weird exceptions 
            // thrown in Shaders, then you should not enable the marker tracking thread
            State.ThreadOption = (ushort)ThreadOptions.MarkerTracking;

            // Set up the lights used in the scene
            CreateLights();

            // Set up cameras for both the AR and VR scenes
            CreateCameras();

            // Setup two viewports, one displasy the AR scene, the other displays the VR scene
            SetupViewport();

            // Set up optical marker tracking
            SetupMarkerTracking();

            // Create a geometry representing a camera in the VR scene
            CreateVirtualCameraRepresentation();

            // Create the ground that represents the physical ground marker array
            CreateMarkerBoard();

            // Create 3D objects
            CreateObjects();

            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(Instance_KeyPressEvent);

            State.ShowFPS = true;
        }

        private void Instance_KeyPressEvent(Keys key, KeyModifier modifier)
        {
            if (key == Keys.Escape)
                this.Exit();
        }

        private void CreateCameras()
        {
            // Create a camera for VR scene
            Camera vrCamera = new Camera();
            vrCamera.Translation = new Vector3(0, -280, 480);
            vrCamera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(45));
            vrCamera.FieldOfViewY = MathHelper.ToRadians(60);
            vrCamera.ZNearPlane = 1;
            vrCamera.ZFarPlane = 2000;

            vrCameraNode = new CameraNode(vrCamera);
            scene.RootNode.AddChild(vrCameraNode);

            // Create a camera for AR scene
            Camera arCamera = new Camera();
            arCamera.ZNearPlane = 1;
            arCamera.ZFarPlane = 2000;

            arCameraNode = new CameraNode(arCamera);
            scene.RootNode.AddChild(arCameraNode);

            // Set the AR camera to be the main camera so that at the time of setting up the marker tracker,
            // the marker tracker will assign the right projection matrix to this camera
            scene.CameraNode = arCameraNode;
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(1, -1, -1);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.AmbientLightColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            lightNode.LightSource = lightSource;

            scene.RootNode.AddChild(lightNode);
        }

        private void SetupViewport()
        {
            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            // Create a render target to render the AR scene to
            arViewRenderTarget = new RenderTarget2D(State.Device, State.Width, State.Height, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);

            // Create a render target to render the VR scene to. 
            vrViewRenderTarget = new RenderTarget2D(State.Device, State.Width * 2 / 5, State.Height * 2 / 5, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);

            // Set the AR scene to take the full window size
            arViewRect = new Rectangle(0, 0, State.Width, State.Height);

            // Set the VR scene to take the 2 / 5 of the window size and positioned at the top right corner
            vrViewRect = new Rectangle(State.Width - vrViewRenderTarget.Width, 0,
                vrViewRenderTarget.Width, vrViewRenderTarget.Height);
        }

        private void SetupMarkerTracking()
        {
            DirectShowCapture2 captureDevice = new DirectShowCapture2();
            captureDevice.InitVideoCapture(0, FrameRate._60Hz, Resolution._640x480,
                ImageFormat.R8G8B8_24, false);

            scene.AddVideoCaptureDevice(captureDevice);

            // Use ALVAR marker tracker
            ALVARMarkerTracker tracker = new ALVARMarkerTracker();
            tracker.MaxMarkerError = 0.02f;
            tracker.ZNearPlane = arCameraNode.Camera.ZNearPlane;
            tracker.ZFarPlane = arCameraNode.Camera.ZFarPlane;

            tracker.InitTracker(captureDevice.Width, captureDevice.Height, "calib.xml", 32.4f);

            // Set the marker tracker to use for our scene
            scene.MarkerTracker = tracker;

            // Display the camera image in the background. Note that this parameter should
            // be set after adding at least one video capture device to the Scene class.
            scene.ShowCameraImage = true;

            // Create a marker node to track a ground marker array.
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ALVARGroundArray.xml");
            scene.RootNode.AddChild(groundMarkerNode);
        }

        private void CreateMarkerBoard()
        {
            markerBoardGeom = new GeometryNode("MarkerBoard")
            {
                Model = new TexturedPlane(340, 200),
                Material =
                {
                    Diffuse = Color.White.ToVector4(),
                    Specular = Color.White.ToVector4(),
                    SpecularPower = 20,
                    Texture = Content.Load<Texture2D>("ALVARArray")
                }
            };

            // Rotate the marker board in the VR scene so that it appears Z-up
            TransformNode markerBoardTrans = new TransformNode()
            {
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2)
            };

            scene.RootNode.AddChild(markerBoardTrans);
            markerBoardTrans.AddChild(markerBoardGeom);
        }

        private void CreateVirtualCameraRepresentation()
        {
            vrCameraRepNode = new GeometryNode("VR Camera")
            {
                Model = new Pyramid(markerSize * 4 / 3, markerSize, markerSize),
                Material =
                {
                    Diffuse = Color.Orange.ToVector4(),
                    Specular = Color.White.ToVector4(),
                    SpecularPower = 20
                }
            };

            vrCameraRepTransNode = new TransformNode();

            TransformNode camOffset = new TransformNode()
            {
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2)
            };

            scene.RootNode.AddChild(vrCameraRepTransNode);
            vrCameraRepTransNode.AddChild(camOffset);
            camOffset.AddChild(vrCameraRepNode);
        }

        private void CreateObjects()
        {
            ModelLoader loader = new ModelLoader();

            overlayNode = new GeometryNode("Overlay");
            overlayNode.Model = (Model)loader.Load("", "p1_wedge");
            ((Model)overlayNode.Model).UseInternalMaterials = true;

            // Get the dimension of the model
            Vector3 dimension = Vector3Helper.GetDimensions(overlayNode.Model.MinimumBoundingBox);
            // Scale the model to fit to the size of 5 markers
            float scale = markerSize * 5 / Math.Max(dimension.X, dimension.Z);

            TransformNode overlayTransNode = new TransformNode()
            {
                Translation = new Vector3(0, 0, dimension.Y * scale / 2),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)) *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)),
                Scale = new Vector3(scale, scale, scale)
            };

            scene.RootNode.AddChild(overlayTransNode);
            overlayTransNode.AddChild(overlayNode);
        }

        protected override void Dispose(bool disposing)
        {
            scene.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Set the render target for rendering the AR scene
            scene.SceneRenderTarget = arViewRenderTarget;
            // Set the scene background size to be the size of the AR scene viewport
            scene.BackgroundBound = arViewRect;
            // Set the camera to be the AR camera
            scene.CameraNode = arCameraNode;
            // Associate the overlaid model with the ground marker for rendering it in AR scene
            scene.RootNode.RemoveChild(overlayNode.Parent);
            groundMarkerNode.AddChild(overlayNode.Parent);
            // Don't render the marker board and camera representation
            markerBoardGeom.Enabled = false;
            vrCameraRepNode.Enabled = false;
            // Show the video background
            scene.ShowCameraImage = true;
            // Render the AR scene
            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);

            // Set the render target for rendering the VR scene
            scene.SceneRenderTarget = vrViewRenderTarget;
            // Set the scene background size to be the size of the VR scene viewport
            scene.BackgroundBound = vrViewRect;
            // Set the camera to be the VR camera
            scene.CameraNode = vrCameraNode;
            // Remove the overlaid model from the ground marker for rendering it in VR scene
            groundMarkerNode.RemoveChild(overlayNode.Parent);
            scene.RootNode.AddChild(overlayNode.Parent);
            // Render the marker board and camera representation in VR scene
            markerBoardGeom.Enabled = true;
            vrCameraRepNode.Enabled = true;
            // Update the transformation of the camera representation in VR scene based on the
            // marker array transformation
            if (groundMarkerNode.MarkerFound)
                vrCameraRepTransNode.WorldTransformation = Matrix.Invert(groundMarkerNode.WorldTransformation);
            // Do not show the video background
            scene.ShowCameraImage = false;
            // Re-traverse the scene graph since we have modified it, and render the VR scene 
            scene.RenderScene(false, true);

            // Set the render target back to the frame buffer
            State.Device.SetRenderTarget(null);
            // Render the two textures rendered on the render targets
            State.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            State.SharedSpriteBatch.Draw(arViewRenderTarget, arViewRect, Color.White);
            State.SharedSpriteBatch.Draw(vrViewRenderTarget, vrViewRect, Color.White);
            State.SharedSpriteBatch.End();
        }
    }
}
