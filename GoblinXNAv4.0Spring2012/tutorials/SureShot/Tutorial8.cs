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

//#define USE_NYARTOOLKIT

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
using GoblinXNA.Physics.Newton1;
using GoblinXNA.Helpers;
using GoblinXNA.Shaders;

namespace Tutorial8___Optical_Marker_Tracking
{
    /// <summary>
    /// This tutorial demonstrates how to use both the ALVAR and ARTag optical marker tracker 
    /// library with our Goblin XNA framework. Please read the README.pdf included with this 
    /// project before running this tutorial. If you're using ARTag, please remove calib.xml
    /// from the solution explorer so that you can successfully build the project. 
    /// </summary>
    public class Tutorial8 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        Scene scene;
        MarkerNode groundMarkerNode, toolbarMarkerNode;
        GeometryNode boxNode;
        bool useStaticImage = true;

        public Tutorial8()
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

            // Use the newton physics engine to perform collision detection
            scene.PhysicsEngine = new NewtonPhysics();

            // For some reason, it sometimes causes memory conflict when it attempts to update the
            // marker transformation in the multi-threaded code, so if you see weird exceptions 
            // thrown in Shaders, then you should not enable the marker tracking thread
            State.ThreadOption = (ushort)ThreadOptions.MarkerTracking;

            // Set up optical marker tracking
            // Note that we don't create our own camera when we use optical marker
            // tracking. It'll be created automatically
            SetupMarkerTracking();

            // Set up the lights used in the scene
            CreateLights();

            // Enable shadow mapping
            // NOTE: In order to use shadow mapping, you will need to add 'MultiLightShadowMap.fx'
            // and 'SimpleShadowShader.fx' shader files to your 'Content' directory. Also in here,
            // we're creating the ShadowMap before the creation of 3D objects since we need to assign
            // this ShadowMap to the IShadowShader used for the 3D objects
            scene.ShadowMap = new MultiLightShadowMap();

            // Create 3D objects
            CreateObjects();

            // Create the ground that represents the physical ground marker array
            CreateGround();

            // Show Frames-Per-Second on the screen for debugging
            State.ShowFPS = true;
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
            lightNode.LightSource = lightSource;

            // Set this light node to cast shadows (by just setting this to true will not cast any shadows,
            // scene.ShadowMap needs to be set to a valid IShadowMap and Model.Shader needs to be set to
            // a proper IShadowShader implementation
            lightNode.CastShadows = true;

            // You should also set the light projection when casting shadow from this light
            lightNode.LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                1, 1f, 500);

            scene.RootNode.AddChild(lightNode);
        }

        private void SetupMarkerTracking()
        {
            IVideoCapture captureDevice = null;

            if (useStaticImage)
            {
                captureDevice = new NullCapture();
#if USE_NYARTOOLKIT
                captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._800x600,
                    ImageFormat.B8G8R8A8_32, false);
#else
                captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._800x600,
                    ImageFormat.R8G8B8_24, false);
#endif
                ((NullCapture)captureDevice).StaticImageFile = "MarkerImage";
            }
            else
            {
                // Create our video capture device that uses DirectShow library. Note that 
                // the combinations of resolution and frame rate that are allowed depend on 
                // the particular video capture device. Thus, setting incorrect resolution 
                // and frame rate values may cause exceptions or simply be ignored, depending 
                // on the device driver.  The values set here will work for a Microsoft VX 6000, 
                // and many other webcams.
                captureDevice = new DirectShowCapture2();
#if USE_NYARTOOLKIT
                captureDevice.InitVideoCapture(0, FrameRate._60Hz, Resolution._640x480,
                    ImageFormat.B8G8R8A8_32, false);
#else
                captureDevice.InitVideoCapture(0, FrameRate._60Hz, Resolution._640x480,
                    ImageFormat.R8G8B8_24, false);
#endif
            }

            // Add this video capture device to the scene so that it can be used for
            // the marker tracker
            scene.AddVideoCaptureDevice(captureDevice);

#if USE_NYARTOOLKIT
            NyARToolkitTracker tracker = new NyARToolkitTracker();
            tracker.InitTracker(captureDevice.Width, captureDevice.Height, "camera_para.dat");
#else
            ALVARMarkerTracker tracker = new ALVARMarkerTracker();
            tracker.MaxMarkerError = 0.02f;
            tracker.InitTracker(captureDevice.Width, captureDevice.Height, "calib.xml", 32.4f);
#endif

            // Set the marker tracker to use for our scene
            scene.MarkerTracker = tracker;

            // Display the camera image in the background. Note that this parameter should
            // be set after adding at least one video capture device to the Scene class.
            scene.ShowCameraImage = true;
        }

        private void CreateGround()
        {
            GeometryNode groundNode = new GeometryNode("Ground");

            // We will use TexturedBox instead of regular Box class since we will need the
            // texture coordinates elements for passing the vertices to the SimpleShadowShader
            // we will be using
            groundNode.Model = new TexturedBox(324, 180, 0.1f);

            // Set this ground model to act as an occluder so that it appears transparent
            groundNode.IsOccluder = true;

            // Make the ground model to receive shadow casted by other objects with
            // ShadowAttribute.ReceiveCast
            groundNode.Model.ShadowAttribute = ShadowAttribute.ReceiveOnly;
            // Assign a shadow shader for this model that uses the IShadowMap we assigned to the scene
            groundNode.Model.Shader = new SimpleShadowShader(scene.ShadowMap);

            Material groundMaterial = new Material();
            groundMaterial.Diffuse = Color.Gray.ToVector4();
            groundMaterial.Specular = Color.White.ToVector4();
            groundMaterial.SpecularPower = 20;

            groundNode.Material = groundMaterial;

            groundMarkerNode.AddChild(groundNode);
        }

        private void CreateObjects()
        {
            // Create a geometry node with a model of a sphere that will be overlaid on
            // top of the ground marker array
            GeometryNode sphereNode = new GeometryNode("Sphere");
            // We will use TexturedSphere instead of regular Box class since we will need the
            // texture coordinates elements for passing the vertices to the SimpleShadowShader
            // we will be using
            sphereNode.Model = new TexturedSphere(16, 20, 20);

            // Add this sphere model to the physics engine for collision detection
            sphereNode.AddToPhysicsEngine = true;
            sphereNode.Physics.Shape = ShapeType.Sphere;
            // Make this sphere model cast and receive shadows
            sphereNode.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;
            // Assign a shadow shader for this model that uses the IShadowMap we assigned to the scene
            sphereNode.Model.Shader = new SimpleShadowShader(scene.ShadowMap);

            // Create a marker node to track a ground marker array.
#if USE_NYARTOOLKIT
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "NyARToolkitGroundArray.xml",
                NyARToolkitTracker.ComputationMethod.Average);
#else
            groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ALVARGroundArray.xml");
#endif

            TransformNode sphereTransNode = new TransformNode();
            sphereTransNode.Translation = new Vector3(0, 0, 50);

            // Create a material to apply to the sphere model
            Material sphereMaterial = new Material();
            sphereMaterial.Diffuse = new Vector4(0, 0.5f, 0, 1);
            sphereMaterial.Specular = Color.White.ToVector4();
            sphereMaterial.SpecularPower = 10;

            sphereNode.Material = sphereMaterial;

            // Now add the above nodes to the scene graph in the appropriate order.
            // Note that only the nodes added below the marker node are affected by 
            // the marker transformation.
            scene.RootNode.AddChild(groundMarkerNode);
            groundMarkerNode.AddChild(sphereTransNode);
            sphereTransNode.AddChild(sphereNode);

            // Create a geometry node with a model of a box that will be overlaid on
            // top of the ground marker array initially. (When the toolbar marker array is
            // detected, it will be overlaid on top of the toolbar marker array.)
            boxNode = new GeometryNode("Box");
            // We will use TexturedBox instead of regular Box class since we will need the
            // texture coordinates elements for passing the vertices to the SimpleShadowShader
            // we will be using
            boxNode.Model = new TexturedBox(32.4f);

            // Add this box model to the physics engine for collision detection
            boxNode.AddToPhysicsEngine = true;
            boxNode.Physics.Shape = ShapeType.Box;
            // Make this box model cast and receive shadows
            boxNode.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;
            // Assign a shadow shader for this model that uses the IShadowMap we assigned to the scene
            boxNode.Model.Shader = new SimpleShadowShader(scene.ShadowMap);

            // Create a marker node to track a toolbar marker array.
#if USE_NYARTOOLKIT
            toolbarMarkerNode = new MarkerNode(scene.MarkerTracker, "ToolbarNyARToolkit.xml",
                NyARToolkitTracker.ComputationMethod.Average);
#else
            toolbarMarkerNode = new MarkerNode(scene.MarkerTracker, "ALVARToolbar.xml");
#endif

            scene.RootNode.AddChild(toolbarMarkerNode);

            // Create a material to apply to the box model
            Material boxMaterial = new Material();
            boxMaterial.Diffuse = new Vector4(0.5f, 0, 0, 1);
            boxMaterial.Specular = Color.White.ToVector4();
            boxMaterial.SpecularPower = 10;

            boxNode.Material = boxMaterial;

            // Add this box model node to the ground marker node
            groundMarkerNode.AddChild(boxNode);

            // Create a collision pair and add a collision callback function that will be
            // called when the pair collides
            NewtonPhysics.CollisionPair pair = new NewtonPhysics.CollisionPair(boxNode.Physics, sphereNode.Physics);
            ((NewtonPhysics)scene.PhysicsEngine).AddCollisionCallback(pair, BoxSphereCollision);
        }

        /// <summary>
        /// A callback function that will be called when the box and sphere model collides
        /// </summary>
        /// <param name="pair"></param>
        private void BoxSphereCollision(NewtonPhysics.CollisionPair pair)
        {
            Console.WriteLine("Box and Sphere has collided");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
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
            // If ground marker array is detected
            if (groundMarkerNode.MarkerFound)
            {
                // If the toolbar marker array is detected, then overlay the box model on top
                // of the toolbar marker array; otherwise, overlay the box model on top of
                // the ground marker array
                if (toolbarMarkerNode.MarkerFound)
                {
                    // The box model is overlaid on the ground marker array, so in order to
                    // make the box model appear overlaid on the toolbar marker array, we need
                    // to offset the ground marker array's transformation. Thus, we multiply
                    // the toolbar marker array's transformation with the inverse of the ground marker
                    // array's transformation, which becomes T*G(inv)*G = T*I = T as a result, 
                    // where T is the transformation of the toolbar marker array, G is the 
                    // transformation of the ground marker array, and I is the identity matrix. 
                    // The Vector3(0, 0, 16.1) is a shift translation to make the box overlaid right 
                    // on top of the toolbar marker. The top-left corner of the left marker of the 
                    // toolbar marker array is defined as (0, 0, 0), so in order to make the box model
                    // appear right on top of the left marker of the toolbar marker array, we shift by
                    // half of each dimension of the 8x8x8 box model.  The approach used here requires that
                    // the ground marker array remains visible at all times.
                    Vector3 shiftVector = new Vector3(0, 0, 16.1f);
                    Matrix mat = Matrix.CreateTranslation(shiftVector) * 
                        toolbarMarkerNode.WorldTransformation * 
                        Matrix.Invert(groundMarkerNode.WorldTransformation);

                    // Modify the transformation in the physics engine
                    ((NewtonPhysics)scene.PhysicsEngine).SetTransform(boxNode.Physics, mat);
                }
                else
                    ((NewtonPhysics)scene.PhysicsEngine).SetTransform(boxNode.Physics, 
                        Matrix.CreateTranslation(0, 0, 16.1f));
            }

            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
    }
}
