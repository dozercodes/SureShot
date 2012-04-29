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
using System.Windows.Media;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Helpers;
using GoblinXNA.UI;
using GoblinXNA.UI.UI2D;

using Tutorial16___Multiple_Viewport;

namespace Tutorial16___Multiple_Viewport___PhoneLib
{
    /// <summary>
    /// This tutorial demonstrates how to create and render multiple viewports using Goblin XNA, one
    /// in AR mode, and another in VR mode.
    /// </summary>
    public class SureShot
    {
        Scene scene;
        MarkerNode groundMarkerNode;
        bool betterFPS = false; // has trade-off of worse tracking if set to true

        Viewport viewport;

        GeometryNode markerBoardGeom;
        GeometryNode target1;
        GeometryNode target2;
        GeometryNode target3;
        GeometryNode vrCameraRepNode;
        TransformNode vrCameraRepTransNode;

        CameraNode arCameraNode;
        CameraNode vrCameraNode;

        RenderTarget2D arViewRenderTarget;
        RenderTarget2D vrViewRenderTarget;
        Rectangle arViewRect;
        Rectangle vrViewRect;

        Texture2D videoTexture;

        float markerSize = 32.4f;

        public SureShot()
        {
            // no contents
        }

        public Texture2D VideoBackground
        {
            get { return videoTexture; }
            set { videoTexture = value; }
        }

        public void Initialize(IGraphicsDeviceService service, ContentManager content, VideoBrush videoBrush)
        {
            // Center the XNA view and set the XNA viewport size to be the size of the video resolution
            viewport = new Viewport(80, 0, 640, 480);
            viewport.MaxDepth = service.GraphicsDevice.Viewport.MaxDepth;
            viewport.MinDepth = service.GraphicsDevice.Viewport.MinDepth;
            service.GraphicsDevice.Viewport = viewport;

            //System.IO.FileStream fileStream = new System.IO.FileStream("Content/crosshair3.png", System.IO.FileMode.Open);
            //Texture2D crosshair = Texture2D.FromStream(service.GraphicsDevice, fileStream);

            // Initialize the GoblinXNA framework
            State.InitGoblin(service, content, "");

            // Initialize the scene graph
            scene = new Scene();

            // Set up the lights used in the scene
            CreateLights();

            // Set up cameras for both the AR and VR scenes
            CreateCameras();

            // Setup two viewports, one displasy the AR scene, the other displays the VR scene
            SetupViewport();

            // Set up optical marker tracking
            SetupMarkerTracking(videoBrush);

            // Create a geometry representing a camera in the VR scene
            CreateVirtualCameraRepresentation();

            // Create the ground that represents the physical ground marker array
            CreateMarkerBoard();

            // Create 3D objects
            CreateObjects();

            State.ShowFPS = true;
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
            PresentationParameters pp = State.Device.PresentationParameters;

            // Create a render target to render the AR scene to
            arViewRenderTarget = new RenderTarget2D(State.Device, viewport.Width, viewport.Height, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);

            // Create a render target to render the VR scene to.
            vrViewRenderTarget = new RenderTarget2D(State.Device, viewport.Width * 2 / 5, viewport.Height * 2 / 5, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);

            // Set the AR scene to take the full window size
            arViewRect = new Rectangle(0, 0, viewport.Width, viewport.Height);

            // Set the VR scene to take the 2 / 5 of the window size and positioned at the top right corner
            vrViewRect = new Rectangle(viewport.Width - vrViewRenderTarget.Width, 0,
                vrViewRenderTarget.Width, vrViewRenderTarget.Height);
        }

        private void SetupMarkerTracking(VideoBrush videoBrush)
        {
            PhoneCameraCapture captureDevice = new PhoneCameraCapture(videoBrush);
            captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.B8G8R8A8_32, false);
            ((PhoneCameraCapture)captureDevice).UseLuminance = true;

            if (betterFPS)
                captureDevice.MarkerTrackingImageResizer = new HalfResizer();

            scene.AddVideoCaptureDevice(captureDevice);

            // Use NyARToolkit marker tracker
            NyARToolkitTracker tracker = new NyARToolkitTracker();

            if (captureDevice.MarkerTrackingImageResizer != null)
                tracker.InitTracker((int)(captureDevice.Width * captureDevice.MarkerTrackingImageResizer.ScalingFactor),
                    (int)(captureDevice.Height * captureDevice.MarkerTrackingImageResizer.ScalingFactor),
                    "camera_para.dat");
            else
                tracker.InitTracker(captureDevice.Width, captureDevice.Height, "camera_para.dat");

            // Set the marker tracker to use for our scene
            scene.MarkerTracker = tracker;

            // Create a marker node to track a ground marker array.
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "NyARToolkitGroundArray.xml",
                NyARToolkitTracker.ComputationMethod.Average);
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
                    Texture = State.Content.Load<Texture2D>("ALVARArray")
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

            target1 = new GeometryNode("Ball1");
            target1.Model = (Model)loader.Load("", "BallWhite");
            ((Model)target1.Model).UseInternalMaterials = true;

            target2 = new GeometryNode("Ball2");
            target2.Model = (Model)loader.Load("", "BallRed");
            ((Model)target2.Model).UseInternalMaterials = true;

            target3 = new GeometryNode("Ball3");
            target3.Model = (Model)loader.Load("", "BallBlue");
            ((Model)target3.Model).UseInternalMaterials = true;

            // Get the dimension of the model
            Vector3 dimension = Vector3Helper.GetDimensions(target1.Model.MinimumBoundingBox);
            // Scale the model to fit to the size of 5 markers
            float scale = markerSize / Math.Max(dimension.X, dimension.Z);
            
            Random rand = new Random();
            float range = 4 * markerSize;
            //USE Bounding Box
            TransformNode tar1TransNode = new TransformNode()
            {
                Translation = new Vector3((float)rand.NextDouble(), (float)rand.NextDouble() * range, dimension.Y * scale / 2),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)) *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)),
                Scale = new Vector3(scale, scale, scale)
            };

            scene.RootNode.AddChild(tar1TransNode);
            tar1TransNode.AddChild(target1);

            TransformNode tar2TransNode = new TransformNode()
            {
                Translation = new Vector3((float)rand.NextDouble() * range, (float)rand.NextDouble() * range, dimension.Y * scale / 2),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)) *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)),
                Scale = new Vector3(scale, scale, scale)
            };

            scene.RootNode.AddChild(tar2TransNode);
            tar2TransNode.AddChild(target2);

            TransformNode tar3TransNode = new TransformNode()
            {
                Translation = new Vector3((float)rand.NextDouble() * range, (float)rand.NextDouble() * range, dimension.Y * scale / 2),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)) *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)),
                Scale = new Vector3(scale, scale, scale)
            };

            scene.RootNode.AddChild(tar3TransNode);
            tar3TransNode.AddChild(target3);
        }

        public void Dispose()
        {
            scene.Dispose();
        }

        public void Update(TimeSpan elapsedTime, bool isActive)
        {
            scene.Update(elapsedTime, false, isActive);
        }

        public void Draw(TimeSpan elapsedTime)
        {
            // Reset the XNA viewport to our centered and resized viewport
            State.Device.Viewport = viewport;

            // Set the render target for rendering the AR scene
            scene.SceneRenderTarget = arViewRenderTarget;
            scene.BackgroundColor = Color.Black;
            // Set the scene background size to be the size of the AR scene viewport
            scene.BackgroundBound = arViewRect;
            // Set the camera to be the AR camera
            scene.CameraNode = arCameraNode;
            // Associate the overlaid model with the ground marker for rendering it in AR scene
            scene.RootNode.RemoveChild(target1.Parent);
            groundMarkerNode.AddChild(target1.Parent);
            scene.RootNode.RemoveChild(target2.Parent);
            groundMarkerNode.AddChild(target2.Parent);
            scene.RootNode.RemoveChild(target3.Parent);
            groundMarkerNode.AddChild(target3.Parent);
            // Don't render the marker board and camera representation
            markerBoardGeom.Enabled = false;
            vrCameraRepNode.Enabled = false;
            // Show the video background
            scene.BackgroundTexture = videoTexture;
            
            UI2DRenderer.DrawCircle(new Point(0, 0), 1, Color.Aqua);
            // Render the AR scene
            scene.Draw(elapsedTime, false);

            
            // Set the render target for rendering the VR scene
            scene.SceneRenderTarget = vrViewRenderTarget;
            scene.BackgroundColor = Color.CornflowerBlue;
            // Set the scene background size to be the size of the VR scene viewport
            scene.BackgroundBound = vrViewRect;
            // Set the camera to be the VR camera
            scene.CameraNode = vrCameraNode;
            // Remove the overlaid model from the ground marker for rendering it in VR scene
            groundMarkerNode.RemoveChild(target1.Parent);
            scene.RootNode.AddChild(target1.Parent);
            groundMarkerNode.RemoveChild(target2.Parent);
            scene.RootNode.AddChild(target2.Parent);
            groundMarkerNode.RemoveChild(target3.Parent);
            scene.RootNode.AddChild(target3.Parent);
            // Render the marker board and camera representation in VR scene
            markerBoardGeom.Enabled = true;
            vrCameraRepNode.Enabled = true;
            // Update the transformation of the camera representation in VR scene based on the
            // marker array transformation
            if (groundMarkerNode.MarkerFound)
                vrCameraRepTransNode.WorldTransformation = Matrix.Invert(groundMarkerNode.WorldTransformation);
            // Do not show the video background
            scene.BackgroundTexture = null;
            // Re-traverse the scene graph since we have modified it, and render the VR scene 
            scene.RenderScene(false, true);

            // Adjust the viewport to be centered
            arViewRect.X += viewport.X;
            vrViewRect.X += viewport.X;

            // Set the render target back to the frame buffer
            State.Device.SetRenderTarget(null);
            State.Device.Clear(Color.Black);
            // Render the two textures rendered on the render targets
            State.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            State.SharedSpriteBatch.Draw(arViewRenderTarget, arViewRect, Color.White);
            //State.SharedSpriteBatch.Draw(vrViewRenderTarget, vrViewRect, Color.White);
            State.SharedSpriteBatch.End();

            // Reset the adjustments
            arViewRect.X -= viewport.X;
            vrViewRect.X -= viewport.X;
            
        }
    }
}
