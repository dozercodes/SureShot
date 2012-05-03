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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GoblinXNA;
using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics;
using GoblinXNA.Device.Generic;
using GoblinXNA.Graphics.Geometry;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Physics;
#if WINDOWS
using GoblinXNA.Physics.Newton1;
#else
using GoblinXNA.Physics.Matali;
using Komires.MataliPhysics;
using MataliPhysicsObject = Komires.MataliPhysics.PhysicsObject;
#endif
using GoblinXNA.Sounds;
using GoblinXNA.UI;
using GoblinXNA.UI.UI3D;

// 3D Text rendering library from http://nuclexframework.codeplex.com/
// Nuclex.Fonts used here is modified a little bit to better suite Goblin
// framework, and compiled using XNA Game Studio 3.1 instead of 3.0 
using Nuclex.Fonts;

namespace Tutorial9___Advanced_Features
{
    /// <summary>
    /// This tutorial demonstrates some of the advanced features provided by Goblin XNA framework,
    /// including sound, physics material settings using the wrapped Newton physics library,
    /// 3D text rendering, and debugging. 
    /// </summary>
    public class Tutorial9 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Scene scene;
        int shooterID = 0;
        Material shooterMat;
        PrimitiveModel boxModel;
        int collisionCount = 0;
        // A font to render a 3D text
        VectorFont vectorFont;
        // A list of 3D texts to display
        List<Text3DInfo> text3ds;
        SoundEffect bounceSound;

        public Tutorial9()
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
#if WINDOWS
            // Display the mouse cursor
            this.IsMouseVisible = true;
#endif

            // Initialize the GoblinXNA framework
            State.InitGoblin(graphics, Content, "");

#if WINDOWS_PHONE
            this.Activated += Sound.Instance.GameActivated;
#endif

            // Initialize the scene graph
            scene = new Scene();

            // Set the background color to CornflowerBlue color. 
            // GraphicsDevice.Clear(...) is called by Scene object with this color. 
            scene.BackgroundColor = Color.CornflowerBlue;

#if WINDOWS
            // We will use the Newton physics engine (http://www.newtondynamics.com)
            // for processing the physical simulation
            scene.PhysicsEngine = new NewtonPhysics();
#else
            scene.PhysicsEngine = new MataliPhysics();
#endif
            scene.PhysicsEngine.Gravity = 30;

#if WINDOWS_PHONE
            ((MataliPhysics)scene.PhysicsEngine).SimulationTimeStep = 1 / 30f;
#endif

            text3ds = new List<Text3DInfo>();

            // Set up the lights used in the scene
            CreateLights();

            // Set up the camera which defines the eye location and viewing frustum
            CreateCamera();

            // Create 3D objects
            CreateObjects();

#if WINDOWS
            // Set up physics material interaction specifications between the shooting box and the ground
            NewtonMaterial physMat = new NewtonMaterial();
            physMat.MaterialName1 = "ShootingBox";
            physMat.MaterialName2 = "Ground";
            physMat.Elasticity = 0.7f;
            physMat.StaticFriction = 0.8f;
            physMat.KineticFriction = 0.2f;
            // Define a callback function that will be called when the two materials contact/collide
            physMat.ContactProcessCallback = delegate(Vector3 contactPosition, Vector3 contactNormal,
                float contactSpeed, float colObj1ContactTangentSpeed, float colObj2ContactTangentSpeed,
                Vector3 colObj1ContactTangentDirection, Vector3 colObj2ContactTangentDirection)
            {
                if (contactSpeed > 2)
                    collisionCount++;

                // When a cube box collides with the ground, it can have more than 1 contact points
                // depending on the collision surface, so we only play sound and add 3D texts once
                // every four contacts to avoid multiple sound play or text addition for one surface
                // contact
                if (collisionCount >= 4)
                {
                    // Set the collision sound volume based on the contact speed
                    SoundEffectInstance instance = Sound.Instance.PlaySoundEffect(bounceSound);
                    //instance.Volume = contactSpeed / 50f;
                    // Print a text message on the screen
                    Notifier.AddMessage("Contact with speed of " + contactSpeed);

                    // Create a 3D text to be rendered
                    Text3DInfo text3d = new Text3DInfo();
                    text3d.Text = "BOOM!!";
                    // The larger the contact speed, the longer the 3D text will stay displayed
                    text3d.Duration = contactSpeed * 500;
                    text3d.ElapsedTime = 0;
                    // Scale down the vector font since it's quite large, and display the text
                    // above the contact position
                    text3d.Transform = Matrix.CreateScale(0.03f) *
                        Matrix.CreateTranslation(contactPosition + Vector3.UnitY * 4);

                    // Add this 3D text to the display list
                    text3ds.Add(text3d);

                    // Reset the count
                    collisionCount = 0;
                }
            };

            // Add this physics material interaction specifications to the physics engine
            ((NewtonPhysics)scene.PhysicsEngine).AddPhysicsMaterial(physMat);
#endif

            // Add a mouse click handler for shooting a box model from the mouse location 
            MouseInput.Instance.MouseClickEvent += new HandleMouseClick(MouseClickHandler);

            // Show some debug information
            State.ShowFPS = true;
            
            // Show debugging messages on the screen
            State.ShowNotifications = true;

            // Make the debugging message fade out after 3000 ms (3 seconds)
            Notifier.FadeOutTime = 3000;
        }

        private void MouseClickHandler(int button, Point mouseLocation)
        {
            // Shoot a box if left mouse button is clicked
            if (button == MouseInput.LeftButton)
            {
                Vector3 nearSource = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
                Vector3 farSource = new Vector3(mouseLocation.X, mouseLocation.Y, 1);

                Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
                Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

                ShootBox(nearPoint, farPoint);
            }
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

            // Add this light node to the root node
            scene.RootNode.AddChild(lightNode);
        }

        private void CreateCamera()
        {
            // Set up the camera of the scene graph
            Camera camera = new Camera();
            // Put the camera at (0, 0, 0)
            camera.Translation = new Vector3(0, 0, 0);
            // Rotate the camera -20 degrees along the X axis
            camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                MathHelper.ToRadians(-20));
            // Set the vertical field of view to be 45 degrees
            camera.FieldOfViewY = MathHelper.ToRadians(45);
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

        private void CreateObjects()
        {
            // Create a model of box and sphere
            boxModel = new Box(Vector3.One);
            PrimitiveModel sphereModel = new Sphere(1f, 20, 20);

            // Create our ground plane
            GeometryNode groundNode = new GeometryNode("Ground");
            groundNode.Model = boxModel;
            // Define the material name of this ground model
            groundNode.Physics.MaterialName = "Ground";
            // Make this ground plane collidable, so other collidable objects can collide
            // with this ground
            groundNode.Physics.Collidable = true;
            groundNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            groundNode.AddToPhysicsEngine = true;

            // Create a material for the ground
            Material groundMaterial = new Material();
            groundMaterial.Diffuse = Color.LightGreen.ToVector4();
            groundMaterial.Specular = Color.White.ToVector4();
            groundMaterial.SpecularPower = 20;

            groundNode.Material = groundMaterial;

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

            // Create a material that will be applied to all of the sphere models
            Material sphereMaterial = new Material();
            sphereMaterial.Diffuse = Color.Cyan.ToVector4();
            sphereMaterial.Specular = Color.White.ToVector4();
            sphereMaterial.SpecularPower = 10;

            Random rand = new Random();

            // Create bunch of sphere models and pile them up
            for (int i = 0; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    TransformNode pileTrans = new TransformNode();
                    pileTrans.Translation = new Vector3(2 * j + (float)rand.NextDouble()/5, 2*i + 5f + (i + 1) * 0.05f,
                        0 + 0.01f * i + (float)rand.NextDouble()/5);

                    GeometryNode gNode = new GeometryNode("Sphere" + (10 * i + j));
                    gNode.Model = sphereModel;
                    gNode.Material = sphereMaterial;
                    // Make the sphere models interactable, which means that they
                    // participate in the physical simulation
                    gNode.Physics.Interactable = true;
                    gNode.Physics.Collidable = true;
                    gNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Sphere;
                    gNode.Physics.Mass = 30f;
                    gNode.AddToPhysicsEngine = true;

                    parentTransNode.AddChild(pileTrans);
                    pileTrans.AddChild(gNode);
                }
            }

            // Create a material for shooting box models
            shooterMat = new Material();
            shooterMat.Diffuse = Color.Pink.ToVector4();
            shooterMat.Specular = Color.Yellow.ToVector4();
            shooterMat.SpecularPower = 10;
        }

        /// <summary>
        /// Shoot a box from the clicked mouse location
        /// </summary>
        /// <param name="near"></param>
        /// <param name="far"></param>
        private void ShootBox(Vector3 near, Vector3 far)
        {
            GeometryNode shootBox = new GeometryNode("ShooterBox" + shooterID++);
            shootBox.Model = boxModel;
            shootBox.Material = shooterMat;
#if !WINDOWS
            shootBox.Physics = new MataliObject(shootBox);
            ((MataliObject)shootBox.Physics).CollisionStartCallback = BoxCollideWithGround;
#endif
            // Define the material name of this shooting box model
            shootBox.Physics.MaterialName = "ShootingBox";
            shootBox.Physics.Interactable = true;
            shootBox.Physics.Collidable = true;
            shootBox.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            shootBox.Physics.Mass = 60f;
            shootBox.AddToPhysicsEngine = true;

            // Calculate the direction to shoot the box based on the near and far point
            Vector3 linVel = far - near;
            linVel.Normalize();
            // Multiply the direction with the velocity of 20
            linVel *= 20f;

            // Assign the initial velocity to this shooting box
            shootBox.Physics.InitialLinearVelocity = linVel;

            TransformNode shooterTrans = new TransformNode();
            shooterTrans.Translation = near;

            scene.RootNode.AddChild(shooterTrans);
            shooterTrans.AddChild(shootBox);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            vectorFont = Content.Load<VectorFont>("Arial-24-Vector");
            bounceSound = Content.Load<SoundEffect>("rubber_ball_01");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

#if !WINDOWS

        private void BoxCollideWithGround(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject)
        {
            String materialName = ((IPhysicsObject)collidingObject.UserTagObj).MaterialName;
            if (materialName.Equals("Ground"))
            {
                // Set the collision sound volume based on the contact speed
                SoundEffectInstance instance = Sound.Instance.PlaySoundEffect(bounceSound);
                // Print a text message on the screen
                Notifier.AddMessage("Contact with ground");

                // Create a 3D text to be rendered
                Text3DInfo text3d = new Text3DInfo();
                text3d.Text = "BOOM!!";
                // The larger the contact speed, the longer the 3D text will stay displayed
                text3d.Duration = 1 * 500;
                text3d.ElapsedTime = 0;
                Vector3 contactPosition = Vector3.Zero;
                baseObject.MainWorldTransform.GetPosition(ref contactPosition);
                // Scale down the vector font since it's quite large, and display the text
                // above the contact position
                text3d.Transform = Matrix.CreateScale(0.03f) *
                    Matrix.CreateTranslation(contactPosition + Vector3.UnitY * 4);

                // Add this 3D text to the display list
                text3ds.Add(text3d);
            }
        }

#endif

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

            float elapsedMsecs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Create a list of 3D text to remove
            List<Text3DInfo> removeList = new List<Text3DInfo>();
            for (int i = 0; i < text3ds.Count; i++)
            {
                // Increment the elapsed time
                text3ds[i].ElapsedTime += elapsedMsecs;
                // If the elapsed time becomes larger than the duration, then remove the
                // 3D text from the display list
                if (text3ds[i].ElapsedTime > text3ds[i].Duration)
                    removeList.Add(text3ds[i]);
            }

            for (int i = 0; i < removeList.Count; i++)
                text3ds.Remove(removeList[i]);

            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (text3ds.Count > 0)
            {
                // Catch exception in case text3ds becomes empty after deletion in Update
                try
                {
                    Text3DInfo[] text3DArray = new Text3DInfo[text3ds.Count];
                    // Copy the display list to an array since the display list can be modified
                    // at any time
                    text3ds.CopyTo(text3DArray);

                    // Render the 3D texts in the display list in outline style with red color
                    foreach (Text3DInfo text3d in text3DArray)
                        UI3DRenderer.Write3DText(text3d.Text, vectorFont, UI3DRenderer.Text3DStyle.Outline,
                            Color.Red, text3d.Transform);
                }
                catch (Exception)
                {
                }
            }

            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }

        /// <summary>
        /// A class to store 3D text information.
        /// </summary>
        private class Text3DInfo
        {
            public String Text;
            public float Duration;
            public float ElapsedTime;
            public Matrix Transform;
        }
    }
}
