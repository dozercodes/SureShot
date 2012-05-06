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

// Uncomment this if you want to render only the virtual contents
//#define VR

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
using GoblinXNA.Helpers;

#if !VR
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
#endif

namespace Tutorial13___Stereo_Rendering___PhoneLib
{
    /// <summary>
    /// This tutorial demonstrates how to render stereo on the phone both in VR and single-camera
    /// AR. For single-camera AR mode, the video image is cropped to about half-width of the original
    /// size since only half of the screen is seen to each eye. The cropped image is then shifted from
    /// its center with different amount for each eye, so that the user can see the real image in stereo
    /// as if there are two cameras. 
    /// </summary>
    public class Tutorial13_Phone
    {
        // The gap on the center between the left and right screen to prevent the left eye
        // seeing the right eye view and the right eye seeing the left eye view
        const int CENTER_GAP = 16; // in pixels

        // The shift amount in pixels from the center of the cropped video image presented to the left eye. 
        const int LEFT_IMAGE_SHIFT_FROM_CENTER = 20; // 20 pixels to the right from its center

        // The shift amount in pixels of the right eye image relative to the center of the cropped image
        // presented to the left eye.
        const int GAP_BETWEEN_LEFT_AND_RIGHT_IMAGE = -40; // 40 pixels to the left

        bool betterFPS = false; // has trade-off of worse tracking if set to true

        Scene scene;

        RenderTarget2D stereoScreenLeft;
        RenderTarget2D stereoScreenRight;
        Rectangle leftRect;
        Rectangle rightRect;

        TransformNode modelTransformNode;
#if VR
        float angle = 0;
#else
        MarkerNode groundMarkerNode;
        Viewport viewport;

        Rectangle leftSource;
        Rectangle rightSource;
#endif

        public Tutorial13_Phone()
        {
            // no contents
        }

#if !VR
        public Texture2D VideoBackground
        {
            get { return scene.BackgroundTexture; }
            set { scene.BackgroundTexture = value; }
        }
#endif

        public void Initialize(IGraphicsDeviceService service, ContentManager content, VideoBrush videoBrush)
        {
#if !VR
            // Set the viewport to have the right aspect ratio for rendering the video image
            viewport = new Viewport(80, 0, 640, 480);
            viewport.MaxDepth = service.GraphicsDevice.Viewport.MaxDepth;
            viewport.MinDepth = service.GraphicsDevice.Viewport.MinDepth;
            service.GraphicsDevice.Viewport = viewport;
#endif
            // Initialize the GoblinXNA framework
            State.InitGoblin(service, content, "");

            // Initialize the scene graph
            scene = new Scene();

            // Set the background color to CornflowerBlue color. 
            // GraphicsDevice.Clear(...) is called by Scene object with this color. 
#if VR
            scene.BackgroundColor = Color.CornflowerBlue;
#else
            scene.BackgroundColor = Color.Black;
#endif
            // Set up the lights used in the scene
            CreateLights();

            // Set up the stereo camera, which defines the location and viewing frustum of
            // left and right eyes
            SetupStereoCamera();
#if !VR
            // Set up optical marker tracking
            SetupMarkerTracking(videoBrush);
#endif
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
            lightNode.AmbientLightColor = new Vector4(0.2f, 0.2f, 0.2f, 1);
            lightNode.LightSource = lightSource;

            // Add this light node to the root node
            scene.RootNode.AddChild(lightNode);
        }

        private void SetupStereoCamera()
        {
            // Create a stereo camera
            StereoCamera camera = new StereoCamera();
            camera.Translation = new Vector3(0, 0, 0);

            // Set the interpupillary distance which defines the distance between the left
            // and right eyes
#if VR
            camera.InterpupillaryDistance = 5.5f; // 5.5 cm
#else
            camera.InterpupillaryDistance = 20; 
#endif
            // Set the focal distance to be at infinity
            camera.FocalLength = float.MaxValue;

            CameraNode cameraNode = new CameraNode(camera);

            scene.RootNode.AddChild(cameraNode);
            scene.CameraNode = cameraNode;
        }

        private void SetupStereoViewport()
        {
#if VR
            // Since we're doing split-screen stereo rendering, the width for each eye's rendered view
            // will be half of the entire screen
            int stereoWidth = (State.Width - CENTER_GAP) / 2;
            int stereoHeight = State.Height;

            PresentationParameters pp = State.Device.PresentationParameters;

            stereoScreenLeft = new RenderTarget2D(State.Device, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);
            stereoScreenRight = new RenderTarget2D(State.Device, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);

            leftRect = new Rectangle(0, 0, stereoWidth, stereoHeight);
            rightRect = new Rectangle(stereoWidth + CENTER_GAP, 0, stereoWidth, stereoHeight);

            scene.BackgroundBound = leftRect;
#else
            // The phone's width is 800, but since we're rendering the video image with aspect ratio of 4x3 
            // on the background, so we'll hard-code the width to be 640
            int stereoWidth = 640; 
            int stereoHeight = State.Height;

            PresentationParameters pp = State.Device.PresentationParameters;

            stereoScreenLeft = new RenderTarget2D(State.Device, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);
            stereoScreenRight = new RenderTarget2D(State.Device, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);

            int screenWidth = 800;

            leftRect = new Rectangle(0, 0, (screenWidth - CENTER_GAP) / 2, stereoHeight);
            rightRect = new Rectangle(leftRect.Width + CENTER_GAP, 0, leftRect.Width, stereoHeight);

            int sourceWidth = (screenWidth - CENTER_GAP) / 2;

            // We will render half (a little less than half to be exact due to CENTER_GAP) of the 
            // entire video image for both the left and right eyes, so we need to set the crop
            // area
            leftSource = new Rectangle((screenWidth - sourceWidth) / 2 + LEFT_IMAGE_SHIFT_FROM_CENTER, 0, sourceWidth, State.Height);
            rightSource = new Rectangle(leftSource.X + GAP_BETWEEN_LEFT_AND_RIGHT_IMAGE, 0, sourceWidth, State.Height);
#endif
        }

#if !VR
        private void SetupMarkerTracking(VideoBrush videoBrush)
        {
            PhoneCameraCapture captureDevice = new PhoneCameraCapture(videoBrush);
            captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.B8G8R8A8_32, false);
            ((PhoneCameraCapture)captureDevice).UseLuminance = true;

            if (betterFPS)
                captureDevice.MarkerTrackingImageResizer = new HalfResizer();

            scene.AddVideoCaptureDevice(captureDevice);

            // Use NyARToolkit ID marker tracker
            NyARToolkitIdTracker tracker = new NyARToolkitIdTracker();

            if (captureDevice.MarkerTrackingImageResizer != null)
                tracker.InitTracker((int)(captureDevice.Width * captureDevice.MarkerTrackingImageResizer.ScalingFactor),
                    (int)(captureDevice.Height * captureDevice.MarkerTrackingImageResizer.ScalingFactor),
                    "camera_para.dat");
            else
                tracker.InitTracker(captureDevice.Width, captureDevice.Height, "camera_para.dat");

            // Set the marker tracker to use for our scene
            scene.MarkerTracker = tracker;

            ((StereoCamera)scene.CameraNode.Camera).RightProjection = tracker.CameraProjection;

            // Create a marker node to track a ground marker array.
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "NyARIdGroundArray.xml", 
                NyARToolkitTracker.ComputationMethod.Average);
            scene.RootNode.AddChild(groundMarkerNode);
        }
#endif

        private void CreateObject()
        {
            ModelLoader loader = new ModelLoader();

            // Create a geometry node with a model of a space ship 
            GeometryNode shipNode = new GeometryNode("Ship");
            shipNode.Model = (Model)loader.Load("", "p1_wedge");
            ((Model)shipNode.Model).UseInternalMaterials = true;

            // Compute the right scale to apply to the model so that the max dimension
            // of the model will be 50.0 cm (cm is used here since we used cm measure
            // for setting our interpupillary distance)
            Vector3 dim = Vector3Helper.GetDimensions(shipNode.Model.MinimumBoundingBox);
            float scale = 50.0f / Math.Max(Math.Max(dim.X, dim.Y), dim.Z);

            // Create a transform node to define the transformation of this model
            // (Transformation includes translation, rotation, and scaling)
            modelTransformNode = new TransformNode();
#if VR
            modelTransformNode.Scale = new Vector3(scale, scale, scale);
            // Place the model 60 cm away from the viewer
            modelTransformNode.Translation = new Vector3(0, 0, -60);
            modelTransformNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY,
                MathHelper.ToRadians(90));

            scene.RootNode.AddChild(modelTransformNode);
#else
            float largeScale = scale * 4;
            modelTransformNode.Scale = new Vector3(largeScale, largeScale, largeScale);
            modelTransformNode.Translation = new Vector3(0, 0, dim.Y * largeScale / 2);
            modelTransformNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)) *
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90));
            groundMarkerNode.AddChild(modelTransformNode);
#endif
            modelTransformNode.AddChild(shipNode);
        }

        public void Dispose()
        {
            scene.Dispose();
        }

        public void Update(TimeSpan elapsedTime, bool isActive)
        {
#if VR
            // Keep rotating the model little by little
            angle += MathHelper.ToRadians(0.5f);
            modelTransformNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
#endif

            scene.Update(elapsedTime, false, isActive);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(TimeSpan elapsedTime)
        {
#if !VR
            State.Device.Viewport = viewport;
#endif
            // Set the render target to be the left screen render target
            scene.SceneRenderTarget = stereoScreenLeft;
            // Render the scene viewed from the left eye to the left screen render target
            scene.Draw(elapsedTime, false);

            // Set the render target to be the right screen render target
            scene.SceneRenderTarget = stereoScreenRight;
            // Render the scene viewed from the right eye to the right screen render target
            // NOTE: We use the light version of Draw function here for better performance
            scene.RenderScene(false, false);

            // Set the render target to be the default one (frame buffer)
            State.Device.SetRenderTarget(null);
            State.Device.Clear(scene.BackgroundColor);
            // Render the left and right render targets as textures
            State.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
#if VR
            State.SharedSpriteBatch.Draw(stereoScreenLeft, leftRect, Color.White);
            State.SharedSpriteBatch.Draw(stereoScreenRight, rightRect, Color.White);
#else
            State.SharedSpriteBatch.Draw(stereoScreenLeft, leftRect, leftSource, Color.White);
            State.SharedSpriteBatch.Draw(stereoScreenRight, rightRect, rightSource, Color.White);
#endif
            State.SharedSpriteBatch.End();
        }
    }
}
