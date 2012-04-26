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
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
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
using GoblinXNA.Physics;
using GoblinXNA.Helpers;
using GoblinXNA.Device.iWear;
using GoblinXNA.Device;
using GoblinXNA.Device.Generic;
using GoblinXNA.Shaders;

using GoblinXNA.UI.UI2D;

namespace Tutorial13___iWear_VR920
{
    /// <summary>
    /// This tutorial demonstrates the stereoscropic rendering using Vuzix's iWear VR920.
    /// If you're using ARTag, then uncomment the define command at the beginning of this file.
    /// NOTE: Some resources included in this project are shared between Tutorial 8, so 
    /// please make sure that Tutorial 8 runs before running this tutorial.
    /// </summary>
    public class Tutorial13 : Microsoft.Xna.Framework.Game
    {
        bool FULL_SCREEN = false;

        GraphicsDeviceManager graphics;
        SpriteFont sampleFont;

        Scene scene;
        MarkerNode groundMarkerNode;

        bool stereoMode = true;

        iWearTracker iTracker;

        RenderTarget2D stereoScreenLeft;
        RenderTarget2D stereoScreenRight;
        Rectangle leftRect;
        Rectangle rightRect;
        Rectangle leftSource;
        Rectangle rightSource;
        SpriteBatch spriteBatch;

        float markerSize = 32.4f;

        public Tutorial13()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            if (FULL_SCREEN)
                graphics.IsFullScreen = true;
            else
            {
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 480;
            }
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

            State.ThreadOption = (ushort)(ThreadOptions.MarkerTracking);

            // Initialize the scene graph
            scene = new Scene();

            // Set up the VUZIX's iWear VR920 for both stereo and orientation tracking
            SetupIWear();

            // If stereo mode is true, then setup stereo camera. If not stereo, we don't need to
            // setup a camera since it's automatically setup by Scene when marker tracker is
            // used. This stereo camera needs to be setup before setting up the marker tracker so
            // the stereo camera will have correct projection matrix computed by the marker tracker.
            if (stereoMode)
                SetupStereoCamera();

            // Set up optical marker tracking
            SetupMarkerTracking();

            // Set up the lights used in the scene
            CreateLights();

            // Use the multi light shadow map shader for our shadow mapping
            // NOTE: In order to use shadow mapping, you will need to add 'MultiLightShadowMap.fx'
            // and 'SimpleShadowShader.fx' shader files to your 'Content' directory
            scene.ShadowMap = new MultiLightShadowMap();

            // Create 3D objects
            CreateObjects();

            // Create the ground that represents the physical ground marker array
            CreateGround();

            // Show Frames-Per-Second on the screen for debugging
            State.ShowFPS = true;

            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(HandleKeyPressEvent);

            if (stereoMode && (iTracker.ProductID == iWearDllBridge.IWRProductID.IWR_PROD_WRAP920))
            {
                SetupStereoViewport();
            }
        }

        private void HandleKeyPressEvent(Keys key, KeyModifier modifier)
        {
            // This is somewhat necessary to exit from full screen mode
            if (key == Keys.Escape)
                this.Exit();
        }

        private void SetupStereoCamera()
        {
            StereoCamera camera = new StereoCamera();

            // Load the right eye view matrix from a calibration file created in StereoCameraCalibration tool
            Matrix cameraRightView = Matrix.Identity;
            MatrixHelper.LoadMatrixFromXML("Wrap920_1_Stereo_Millimeter.xml", ref cameraRightView);

            camera.LeftView = Matrix.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);
            camera.RightView = Matrix.Invert(cameraRightView);

            CameraNode cameraNode = new CameraNode(camera);

            scene.RootNode.AddChild(cameraNode);
            scene.CameraNode = cameraNode;
        }

        private void SetupStereoViewport()
        {
            int stereoWidth = State.Width / 2;
            int stereoHeight = State.Height;

            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            stereoScreenLeft = new RenderTarget2D(GraphicsDevice, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);
            stereoScreenRight = new RenderTarget2D(GraphicsDevice, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);

            leftRect = new Rectangle(0, 0, stereoWidth, stereoHeight);
            rightRect = new Rectangle(stereoWidth, 0, stereoWidth, stereoHeight);

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load("Wrap920_1_Adjustments.xml");
            }
            catch (Exception exp)
            {
                throw new GoblinException(exp.Message);
            }

            int camWidthAdjustmentLeft = 0, camHShiftAdjustmentLeft = 0, camVShiftAdjustmentLeft = 0;
            int camWidthAdjustmentRight = 0, camHShiftAdjustmentRight = 0, camVShiftAdjustmentRight = 0;
            foreach (XmlNode xmlNode in xmlDoc.ChildNodes)
            {
                if (xmlNode is XmlElement)
                {
                    if (xmlNode.Name.Equals("CameraAdjustments"))
                    {
                        foreach (XmlNode child in xmlNode.ChildNodes)
                        {
                            if (child.Name.Equals("LeftWidthAdjustment"))
                                camWidthAdjustmentLeft = int.Parse(child.InnerText);
                            else if (child.Name.Equals("LeftHorizontalShiftAdjustment"))
                                camHShiftAdjustmentLeft = int.Parse(child.InnerText);
                            else if (child.Name.Equals("LeftVerticalShiftAdjustment"))
                                camVShiftAdjustmentLeft = int.Parse(child.InnerText);
                            else if (child.Name.Equals("RightWidthAdjustment"))
                                camWidthAdjustmentRight = int.Parse(child.InnerText);
                            else if (child.Name.Equals("RightHorizontalShiftAdjustment"))
                                camHShiftAdjustmentRight = int.Parse(child.InnerText);
                            else if (child.Name.Equals("RightVerticalShiftAdjustment"))
                                camVShiftAdjustmentRight = int.Parse(child.InnerText);
                        }
                    }
                }
            }

            int camHeightAdjustment = camWidthAdjustmentLeft * stereoHeight / stereoWidth;

            leftSource = new Rectangle(camWidthAdjustmentLeft + camHShiftAdjustmentLeft,
                camHeightAdjustment + camVShiftAdjustmentLeft,
                stereoWidth - camWidthAdjustmentLeft * 2, stereoHeight - camHeightAdjustment * 2);

            camHeightAdjustment = camWidthAdjustmentRight * stereoHeight / stereoWidth;

            rightSource = new Rectangle(camWidthAdjustmentRight + camHShiftAdjustmentRight,
                camHeightAdjustment + camVShiftAdjustmentRight,
                stereoWidth - camWidthAdjustmentRight * 2, stereoHeight - camHeightAdjustment * 2);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            scene.BackgroundBound = leftRect;
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(1, -1, -1);
            lightSource.Diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1);
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.CastShadows = true;
            lightNode.LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                1, 1f, 500);
            // Add an ambient component
            lightNode.AmbientLightColor = new Vector4(0.3f, 0.3f, 0.3f, 1);
            lightNode.LightSource = lightSource;

            // Add this light node to the root node
            groundMarkerNode.AddChild(lightNode);
        }

        private void SetupMarkerTracking()
        {
            DirectShowCapture2 captureDevice = new DirectShowCapture2();
            captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.R8G8B8_24, false);

            // Add this video capture device to the scene so that it can be used for
            // the marker tracker
            scene.AddVideoCaptureDevice(captureDevice);

            // if we're using Wrap920AR, then we need to add another capture device for
            // processing stereo camera
            DirectShowCapture2 captureDevice2 = null;
            if (iTracker.ProductID == iWearDllBridge.IWRProductID.IWR_PROD_WRAP920)
            {
                captureDevice2 = new DirectShowCapture2();
                captureDevice2.InitVideoCapture(1, FrameRate._30Hz, Resolution._640x480,
                    ImageFormat.R8G8B8_24, false);

                scene.AddVideoCaptureDevice(captureDevice2);

                // Calculate the right projection matrix using the camera intrinsic parameters for the 
                // right camera
                ((StereoCamera)scene.CameraNode.Camera).RightProjection =
                    ALVARDllBridge.GetCameraProjection("Wrap920_1_Right.xml", captureDevice2.Width, 
                        captureDevice2.Height, 0.1f, 1000);
            }

            // Create an optical marker tracker that uses ALVAR library
            ALVARMarkerTracker tracker = new ALVARMarkerTracker();
            tracker.MaxMarkerError = 0.02f;
            tracker.ZNearPlane = 0.1f;
            tracker.ZFarPlane = 1000;
            tracker.InitTracker(captureDevice.Width, captureDevice.Height, "Wrap920_1_Left.xml", markerSize);

            ((StereoCamera)scene.CameraNode.Camera).LeftProjection = tracker.CameraProjection;

            scene.MarkerTracker = tracker;

            if (iTracker.ProductID == iWearDllBridge.IWRProductID.IWR_PROD_WRAP920)
            {
                scene.LeftEyeVideoID = 0;
                scene.RightEyeVideoID = 1;
                scene.TrackerVideoID = 0;
            }
            else
            {
                scene.LeftEyeVideoID = 0;
                scene.RightEyeVideoID = 0;
                scene.TrackerVideoID = 0;
            }

            // Create a marker node to track a ground marker array. 
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ALVARGroundArray.xml");

            // Add a transform node to tranlate the objects to be centered around the
            // marker board.
            TransformNode transNode = new TransformNode();

            scene.RootNode.AddChild(groundMarkerNode);

            scene.ShowCameraImage = true;
        }

        private void SetupIWear()
        {
            // Get an instance of iWearTracker
            iTracker = iWearTracker.Instance;
            // We need to initialize it before adding it to the InputMapper class
            iTracker.Initialize();
            // If not stereo, then we need to set the iWear VR920 to mono mode (by default, it's
            // stereo mode if stereo is available)
            if (stereoMode)
                iTracker.EnableStereo = true;
            // Add this iWearTracker to the InputMapper class for automatic update and disposal
            InputMapper.Instance.Add6DOFInputDevice(iTracker);
            // Re-enumerate all of the input devices so that the newly added device can be found
            InputMapper.Instance.Reenumerate();
        }

        private void CreateGround()
        {
            GeometryNode groundNode = new GeometryNode("Ground");

            groundNode.Model = new TexturedBox(340, 200, 0.1f);

            // Set this ground model to act as an occluder so that it appears transparent
            groundNode.IsOccluder = true;

            // Make the ground model to receive shadow casted by other objects with
            // CastShadows set to true
            groundNode.Model.ShadowAttribute = ShadowAttribute.ReceiveOnly;
            groundNode.Model.Shader = new SimpleShadowShader(scene.ShadowMap);

            Material groundMaterial = new Material();
            groundMaterial.Diffuse = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
            groundMaterial.Specular = Color.White.ToVector4();
            groundMaterial.SpecularPower = 20;

            groundNode.Material = groundMaterial;

            groundMarkerNode.AddChild(groundNode);
        }

        private void CreateObjects()
        {
            // Create a sphere geometry
            {
                GeometryNode sphereNode = new GeometryNode("Sphere");
                sphereNode.Model = new TexturedSphere(14, 20, 20);
                sphereNode.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;
                sphereNode.Model.Shader = new SimpleShadowShader(scene.ShadowMap);

                Material sphereMat = new Material();
                sphereMat.Diffuse = Color.Red.ToVector4();
                sphereMat.Specular = Color.White.ToVector4();
                sphereMat.SpecularPower = 20;

                sphereNode.Material = sphereMat;

                TransformNode sphereTrans = new TransformNode();
                sphereTrans.Translation = new Vector3(0, 0, 20);

                groundMarkerNode.AddChild(sphereTrans);
                sphereTrans.AddChild(sphereNode);
            }

            // Create a box geometry
            {
                GeometryNode boxNode = new GeometryNode("Box");
                boxNode.Model = new TexturedBox(24);
                boxNode.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;
                boxNode.Model.Shader = new SimpleShadowShader(scene.ShadowMap);

                Material boxMat = new Material();
                boxMat.Diffuse = Color.Blue.ToVector4();
                boxMat.Specular = Color.White.ToVector4();
                boxMat.SpecularPower = 20;

                boxNode.Material = boxMat;

                TransformNode boxTrans = new TransformNode();
                boxTrans.Translation = new Vector3(-140, -72, 32);

                groundMarkerNode.AddChild(boxTrans);
                boxTrans.AddChild(boxNode);
            }

            // Create a cylinder geometry
            {
                GeometryNode cylinderNode = new GeometryNode("Cylinder");
                cylinderNode.Model = new Cylinder(14, 14, 10, 20);
                cylinderNode.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

                Material cylinderMat = new Material();
                cylinderMat.Diffuse = Color.Green.ToVector4();
                cylinderMat.Specular = Color.White.ToVector4();
                cylinderMat.SpecularPower = 20;

                cylinderNode.Material = cylinderMat;

                TransformNode cylinderTrans = new TransformNode();
                cylinderTrans.Translation = new Vector3(140, -72, 32);

                groundMarkerNode.AddChild(cylinderTrans);
                cylinderTrans.AddChild(cylinderNode);
            }

            // Create a torus geometry
            {
                GeometryNode torusNode = new GeometryNode("Torus");
                torusNode.Model = new Torus(10, 24, 20, 20);
                torusNode.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

                Material torusMat = new Material();
                torusMat.Diffuse = Color.Yellow.ToVector4();
                torusMat.Specular = Color.White.ToVector4();
                torusMat.SpecularPower = 20;

                torusNode.Material = torusMat;

                TransformNode torusTrans = new TransformNode();
                torusTrans.Translation = new Vector3(-140, 72, 32);

                groundMarkerNode.AddChild(torusTrans);
                torusTrans.AddChild(torusNode);
            }

            // Create a capsule geometry
            {
                GeometryNode capsuleNode = new GeometryNode("Capsule");
                capsuleNode.Model = new Capsule(12, 48, 20);
                capsuleNode.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;

                Material capsuleMat = new Material();
                capsuleMat.Diffuse = Color.Cyan.ToVector4();
                capsuleMat.Specular = Color.White.ToVector4();
                capsuleMat.SpecularPower = 20;

                capsuleNode.Material = capsuleMat;

                TransformNode capsuleTrans = new TransformNode();
                capsuleTrans.Translation = new Vector3(140, 72, 32);

                groundMarkerNode.AddChild(capsuleTrans);
                capsuleTrans.AddChild(capsuleNode);
            }
        }

        protected override void LoadContent()
        {
            sampleFont = Content.Load<SpriteFont>("Sample");

            base.LoadContent();
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
            // If it's in stereo mode, and iWear VR920 supports stereo
            if (stereoMode)
            {
                if (iTracker.ProductID == iWearDllBridge.IWRProductID.IWR_PROD_WRAP920)
                {
                    scene.SceneRenderTarget = stereoScreenLeft;
                    scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);

                    scene.SceneRenderTarget = stereoScreenRight;
                    scene.RenderScene();

                    GraphicsDevice.SetRenderTarget(null);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                    spriteBatch.Draw(stereoScreenLeft, leftRect, leftSource, Color.White);
                    spriteBatch.Draw(stereoScreenRight, rightRect, rightSource, Color.White);
                    spriteBatch.End();
                }
                else if (iTracker.IsStereoAvailable)
                {
                    ///////////// First, render the scene for the left eye view /////////////////

                    iTracker.UpdateBottomLine(this);
                    // Begin GPU query for rendering the scene
                    iTracker.BeginGPUQuery();
                    // Render the scene for left eye. Note that since base.Draw(..) will update the
                    // physics simulation and scene graph as well. 
                    scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
                    // Renders our own 2D UI for left eye view
                    RenderUI();
                    // Wait for the GPU to finish rendering
                    iTracker.EndGPUQuery();

                    // Signal the iWear VR920 that the image for the left eye is ready
                    iTracker.SynchronizeEye(iWearDllBridge.Eyes.LEFT_EYE);

                    ///////////// Then, render the scene for the right eye view /////////////////

                    // Begin GPU query for rendering the scene
                    iTracker.BeginGPUQuery();
                    // Render the scene for right eye. Note that we called scene.RenderScene(...) instead of
                    // base.Draw(...). This is because we do not want to update the scene graph or physics
                    // simulation since we want to keep both the left and right eye view in the same time
                    // frame. Also, RenderScene(...) is much more light-weighted compared to base.Draw(...).
                    // The parameter forces the scene to also render the UI in the right eye view. If this is
                    // set to false, then you would only see the UI displayed on the left eye view.
                    scene.RenderScene();
                    // Renders our own 2D UI for right eye view
                    RenderUI();
                    // Wait for the GPU to finish rendering
                    iTracker.EndGPUQuery();

                    // Signal the iWear VR920 that the image for the right eye is ready
                    iTracker.SynchronizeEye(iWearDllBridge.Eyes.RIGHT_EYE);
                }
            }
            else
            {
                scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
                RenderUI();
            }
        }

        /// <summary>
        /// Renders your 2D UI.
        /// </summary>
        /// <param name="trackerAvailable"></param>
        private void RenderUI()
        {
            UI2DRenderer.WriteText(Vector2.Zero, "Stereoscopic UI", Color.Red,
                sampleFont, Vector2.One, GoblinEnums.HorizontalAlignment.Center,
                GoblinEnums.VerticalAlignment.Top);
        }
    }
}
