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
using System.Diagnostics;
using System.Windows.Navigation;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Color = Microsoft.Xna.Framework.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Microsoft.Phone.Controls;
//using Microsoft.Devices;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using Camera = GoblinXNA.SceneGraph.Camera;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Matali;
using Komires.MataliPhysics;
using MataliPhysicsObject = Komires.MataliPhysics.PhysicsObject;

using Microsoft.Devices;
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
        SpriteFont textFont;
        IGraphicsDeviceService graphics;

        GeometryNode groundNode;
        GeometryNode markerBoardGeom;
        GeometryNode pickedNode;
        GeometryNode vrCameraRepNode;
        TransformNode vrCameraRepTransNode;

        CameraNode arCameraNode;
        CameraNode vrCameraNode;

        RenderTarget2D arViewRenderTarget;
        RenderTarget2D vrViewRenderTarget;
        Rectangle arViewRect;
        Rectangle vrViewRect;

        SpriteBatch spriteBatch;
        Texture2D logo, helpbutton, playbutton, backbutton, helpscreen, winscreen;
        G2DButton helpb, playb, backb;
        Vector2 title, play, help;
        String label = "...";
        SpriteFont uiFont;
        bool playGame = false;
        bool viewHelp = false;
        bool goBack = false;
        bool gameOver = false;

        Texture2D videoTexture;
        int maxTargets = 3;
        int maxBarriers = 7;

        float markerSize = 32.4f;

        public SureShot()
        {
            TouchPanel.EnabledGestures = GestureType.Flick | GestureType.Pinch;
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

            textFont = content.Load<SpriteFont>("Sample");
            spriteBatch = new SpriteBatch(service.GraphicsDevice);
            logo = content.Load<Texture2D>("sureshot2");
            helpbutton = content.Load<Texture2D>("help");
            playbutton = content.Load<Texture2D>("play");
            helpscreen = content.Load<Texture2D>("helppage");
            backbutton = content.Load<Texture2D>("back");
            winscreen = content.Load<Texture2D>("win");

            // Initialize the GoblinXNA framework
            State.InitGoblin(service, content, "");

            graphics = service;

            State.ThreadOption = (ushort)ThreadOptions.MarkerTracking;

            // Initialize the scene graph
            scene = new Scene();
            scene.BackgroundColor = Color.Black;

            scene.PhysicsEngine = new MataliPhysics();
            scene.PhysicsEngine.Gravity = 400;
            scene.PhysicsEngine.GravityDirection = -Vector3.UnitY;
            ((MataliPhysics)scene.PhysicsEngine).SimulationTimeStep = 1 / 20f;

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

            // Create the ground that represents the physical ground marker array
            CreateGround();

            // Create 3D objects
            CreateObjects();

            State.ShowFPS = true;

            //MouseInput.Instance.MousePressEvent += new HandleMousePress(MouseClickHandler);
            CameraButtons.ShutterKeyReleased += ButtonPressHandler;
        }

        private void ButtonPressHandler(object sender, EventArgs e)
        {
            //first deselect all barriers
            //deselectBarriers();

            // shoot a beam from the center of the screen
            // 0 means on the near clipping plane, and 1 means on the far clipping plane
            Vector3 nearSource = new Vector3((viewport.Width / 2) + 80, (viewport.Height / 2), 0);//new Vector3(viewport.Width / 2 + 80, viewport.Height / 2, 0);
            Vector3 farSource = new Vector3((viewport.Width / 2) + 80, (viewport.Height / 2), 1);//(viewport.Width / 2 + 80, viewport.Height / 2, 1);

            //tranform center of the screen to the coordinates of the ground array
            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, groundMarkerNode.WorldTransformation, Matrix.Identity);
            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, groundMarkerNode.WorldTransformation, Matrix.Identity);
            
            List<PickedObject> pickedObjects = ((MataliPhysics)scene.PhysicsEngine).PickRayCast(
                nearPoint, farPoint);
            if (pickedObjects.Count > 0)
            {
                // get closest object
                pickedObjects.Sort();
                pickedNode = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container);
                if (pickedNode.Name.Contains("Target"))
                {
                    ((TransformNode)(pickedNode.Parent)).RemoveChild(pickedNode);
                }
            }
        }

        private void CreateCameras()
        {
            // Create a camera for VR scene 
            Camera vrCamera = new Camera();
            vrCamera.Translation = new Vector3(0, 350, 125);
            vrCamera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(-90));
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
            vrViewRenderTarget = new RenderTarget2D(State.Device, viewport.Width * 3 / 10, viewport.Height * 3 / 10, false,
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

        private void CreateGround()
        {
            groundNode = new GeometryNode("Ground");

            // We will use TexturedBox instead of regular Box class since we will need the
            // texture coordinates elements for passing the vertices to the SimpleShadowShader
            // we will be using
            groundNode.Model = new TexturedBox(330, 190, 0.1f);

            // Set this ground model to act as an occluder so that it appears transparent
            groundNode.IsOccluder = true;

            // Make the ground model to receive shadow casted by other objects with
            // ShadowAttribute.ReceiveCast
            groundNode.Model.ShadowAttribute = ShadowAttribute.ReceiveOnly;
            // Assign a shadow shader for this model that uses the IShadowMap we assigned to the scene
            //groundNode.Model.Shader = new SimpleShadowShader(scene.ShadowMap);

            Material groundMaterial = new Material();
            groundMaterial.Diffuse = Color.Gray.ToVector4();
            groundMaterial.Specular = Color.White.ToVector4();
            groundMaterial.SpecularPower = 20;

            groundNode.Material = groundMaterial;

            groundMarkerNode.AddChild(groundNode);
        }

        private void CreateObjects()
        {
            ModelLoader loader = new ModelLoader();

            //set up targets
            string[] ballColors = new string[]{"White", "Red", "Blue"};
            for (int i = 0; i < 1; i++)
            {
                GeometryNode target = new GeometryNode("Target" + i);
                target.Model = (Model)loader.Load("", "Ball" + ballColors[i % ballColors.Length]);
                ((Model)target.Model).UseInternalMaterials = true;
                target.Physics = new MataliObject(target);
                target.Physics.Shape = GoblinXNA.Physics.ShapeType.Sphere;
                target.Physics.Pickable = true;
                target.AddToPhysicsEngine = true;

                // Get the dimension of the model
                Vector3 dimension = Vector3Helper.GetDimensions(target.Model.MinimumBoundingBox);
                // Scale the model to fit to the size of 5 markers
                float scale = markerSize / Math.Max(dimension.X, dimension.Z) * 2;

                Random rand = new Random();
                Vector3 min = groundNode.Model.MinimumBoundingBox.Min;
                Vector3 max = groundNode.Model.MinimumBoundingBox.Max;

                TransformNode tarTransNode = new TransformNode("Trans" + i)
                {
                    //Translation = new Vector3((((float)rand.NextDouble() * (max.X - min.X)) + min.X), (((float)rand.NextDouble() * (max.Y - min.Y)) + min.Y), dimension.Y * scale / 2),
                    Translation = new Vector3(0, 0, dimension.Y * scale / 2),//new Vector3(viewport.Width/2 + 80, viewport.Height/2, dimension.Y * scale / 2),
                    Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(90)) *
                        Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)),
                    Scale = new Vector3(scale, scale, scale)
                };

                scene.RootNode.AddChild(tarTransNode);
                tarTransNode.AddChild(target);
            }
            
            //set up barriers
            for (int i = 0; i < 1; i++)
            {
                GeometryNode barrier = new GeometryNode("Barrier" + i);
                barrier.Model = (Model)loader.Load("", "barrier");
                ((Model)barrier.Model).UseInternalMaterials = true;
                barrier.Physics = new MataliObject(barrier);
                barrier.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
                barrier.Physics.Pickable = true;
                barrier.AddToPhysicsEngine = true;

                // Get the dimension of the model
                Vector3 dimension = Vector3Helper.GetDimensions(barrier.Model.MinimumBoundingBox);
                // Scale the model to fit to the size of 5 markers
                float scale = markerSize / Math.Max(dimension.X, dimension.Z) * 1.5f;

                Random rand = new Random();
                Vector3 min = groundNode.Model.MinimumBoundingBox.Min;
                Vector3 max = groundNode.Model.MinimumBoundingBox.Max;
                float maxZ = 200;

                TransformNode barrTransNode = new TransformNode("Trans" + (i+maxTargets))
                {
                    Translation = new Vector3((((float)rand.NextDouble() * (max.X - min.X)) + min.X), min.Y, (((float)rand.NextDouble() * (maxZ - (dimension.Y * scale / 2))) + (dimension.Y * scale / 2))),
                    Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(0)) *
                        Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)),
                    Scale = new Vector3(scale, (((float)rand.NextDouble() * 5)) * scale, scale)
                };

                scene.RootNode.AddChild(barrTransNode);
                barrTransNode.AddChild(barrier);
            }

            //buttons
            playb = new G2DButton();
            playb.Name = "play";
            playb.Bounds = new Rectangle(75, 400, 100, 40);
            playb.ActionPerformedEvent += new ActionPerformed(HandleActionPerformed);
            playb.Texture = playbutton;


            helpb = new G2DButton();
            helpb.Name = "help";
            helpb.Bounds = new Rectangle(75, 450, 100, 40);
            helpb.ActionPerformedEvent += new ActionPerformed(HandleActionPerformed);
            helpb.Texture = helpbutton;

            backb = new G2DButton();
            backb.Name = "back";
            backb.Bounds = new Rectangle(75, 400, 100, 40);
            backb.ActionPerformedEvent += new ActionPerformed(HandleActionPerformed);
            backb.Texture = backbutton;
            backb.Visible = false;
            backb.Enabled = false;


            scene.UIRenderer.Add2DComponent(playb);
            scene.UIRenderer.Add2DComponent(helpb);
            scene.UIRenderer.Add2DComponent(backb);
        }

        public void Dispose()
        {
            scene.Dispose();
        }

        public void Update(TimeSpan elapsedTime, bool isActive)
        {
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gs = TouchPanel.ReadGesture();
                if(pickedNode != null && pickedNode.Name.Contains("Barrier")){
                    switch (gs.GestureType)
                    {
                        case GestureType.Pinch:
                            ((TransformNode)pickedNode.Parent).Translation += new Vector3(0,0,gs.Delta.X - gs.Delta2.X);
                            break;

                        case GestureType.Flick:
                            ((TransformNode)pickedNode.Parent).Translation += new Vector3(gs.Delta.X/15, -gs.Delta.Y/15, 0);
                            break;
                    }
                }
            }
            scene.Update(elapsedTime, false, isActive);
        }

        public void Draw(TimeSpan elapsedTime)
        {
            if (playGame)
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
                try
                {
                    foreach (Node node in scene.RootNode.Children)
                    {
                        if (node.Name.Contains("Trans"))
                        {
                            foreach (GeometryNode child in ((BranchNode)node).Children)
                            {
                                if (child.Name.Contains("Target") || child.Name.Contains("Barrier")
                                    || child.Name.Contains("Plane") || child.Name.Contains("Background"))
                                {
                                    scene.RootNode.RemoveChild(child.Parent);
                                    groundMarkerNode.AddChild(child.Parent);

                                }
                            }
                        }
                    }
                }
                catch (NullReferenceException nre)
                {
                    //nothing to see here
                }
                // Don't render the marker board and camera representation
                markerBoardGeom.Enabled = false;
                vrCameraRepNode.Enabled = false;
                // Show the video background
                scene.BackgroundTexture = videoTexture;

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
                try
                {
                    foreach (Node node in groundMarkerNode.Children)
                    {
                        if (node.Name.Contains("Trans"))
                        {
                            foreach (Node child in ((BranchNode)node).Children)
                            {
                                if (child.Name.Contains("Target") || child.Name.Contains("Barrier")
                                    || child.Name.Contains("Plane") || child.Name.Contains("Background"))
                                {
                                    groundMarkerNode.RemoveChild(child.Parent);
                                    scene.RootNode.AddChild(child.Parent);
                                }
                            }
                        }
                    }
                }
                catch (NullReferenceException nre)
                {
                    //nothing to see here
                }
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
                State.SharedSpriteBatch.Draw(vrViewRenderTarget, vrViewRect, Color.White);
                State.SharedSpriteBatch.End();

                Texture2D ch = State.Content.Load<Texture2D>("crosshairs");
                Vector2 position = new Vector2((viewport.Width / 2) + 80 - (ch.Width / 2), (viewport.Height / 2) - (ch.Height / 2));
                State.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                State.SharedSpriteBatch.Draw(ch, position, Color.White);
                State.SharedSpriteBatch.End();

                if (pickedNode != null && pickedNode.Name.Contains("Barrier"))
                {
                    UI2DRenderer.WriteText(new Vector2(0, viewport.Height - 20), "Barrier has been selected", Color.White, textFont);
                }

                bool noTargets = true;
                foreach (Node node in scene.RootNode.Children)
                {
                    if (node.Name.Contains("Trans"))
                    {
                        foreach (GeometryNode child in ((BranchNode)node).Children)
                        {
                            if (child.Name.Contains("Target"))
                            {
                                noTargets = false;
                            }
                        }
                    }
                }

                foreach (Node node in groundMarkerNode.Children)
                {
                    if (node.Name.Contains("Trans"))
                    {
                        foreach (GeometryNode child in ((BranchNode)node).Children)
                        {
                            if (child.Name.Contains("Target"))
                            {
                                noTargets = false;
                            }
                        }
                    }
                }
                if (noTargets)
                {
                    Vector2 pos = new Vector2((viewport.Width / 2) + 80 - (winscreen.Width / 2), (viewport.Height / 2) - (winscreen.Height / 2));
                    State.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                    State.SharedSpriteBatch.Draw(winscreen, pos, Color.White);
                    State.SharedSpriteBatch.End();
                }

                // Reset the adjustments
                arViewRect.X -= viewport.X;
                vrViewRect.X -= viewport.X;
            }
            else
            {
                scene.BackgroundTexture = logo;
                if (viewHelp)
                {
                    scene.BackgroundTexture = helpscreen;
                }
                if (goBack)
                {
                    scene.BackgroundTexture = logo;
                }
                scene.Draw(elapsedTime, false);
            }
        }

        private void HandleActionPerformed(object source)
        {
            G2DComponent comp = (G2DComponent)source;
            if (comp is G2DButton && comp.Name == "play")
            {
                label = "PLAY MODE";
                playb.Enabled = false;
                playb.Visible = false;
                helpb.Enabled = false;
                helpb.Visible = false;
                backb.Enabled = false;
                backb.Visible = false;
                playGame = true;
                viewHelp = false;
                goBack = false;
            }

            if (comp is G2DButton && comp.Name == "help")
            {
                label = "HELP MODE";
                playb.Enabled = false;
                playb.Visible = false;
                helpb.Enabled = false;
                helpb.Visible = false;
                backb.Enabled = true;
                backb.Visible = true;
                viewHelp = true;
                goBack = false;
                playGame = false;
            }

            if (comp is G2DButton && comp.Name == "back")
            {
                label = "...";
                playb.Enabled = true;
                playb.Visible = true;
                helpb.Enabled = true;
                helpb.Visible = true;
                backb.Enabled = false;
                backb.Visible = false;
                viewHelp = false;
                goBack = true;
                playGame = false;
            }
        }
    }
}
