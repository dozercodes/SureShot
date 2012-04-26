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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Device.Generic;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.UI.UI2D;

namespace Tutorial2___Simple_Animation
{
    /// <summary>
    /// This tutorial demonstrates simple animation, keypress event handling,
    /// and loading a textured model from an .fbx file.
    /// Pressing the "a" key will toggle between one animation in which a ship
    /// rotates through two tori, and another animation in which the two tori
    /// rotate around the ship.
    ///
    /// Exercise for the reader:  Change the program so that the "s" key toggles
    /// the rotation of the ship on and off, and the "t" key toggles the rotation
    /// of the tori on and off.
    /// </summary>
    public class Tutorial2 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        // A scene graph used to render virtual reality
        Scene scene;

        TransformNode torusTransParentNode;
        TransformNode shipTransParentNode;

        SpriteFont textFont;

        // Boolean value to indicate whether 1st or 2nd animation sequence
        bool firstAnimation = true;
        double shipAngle = 0;
        double toriAngle = 0;

        public Tutorial2()
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

            // Initialize the scene graph
            scene = new Scene();

            // Set the background color to CornflowerBlue color. 
            // GraphicsDevice.Clear(...) is called by Scene object with this color. 
            scene.BackgroundColor = Color.CornflowerBlue;

            // Set up the lights used in the scene
            CreateLights();

            // Set up the camera which defines the eye location and viewing frustum
            CreateCamera();

            // Create 3D objects
            CreateObjects();

#if WINDOWS
            // Add a keyboard press handler for user input
            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(KeyPressHandler);
#elif WINDOWS_PHONE
            MouseInput.Instance.MousePressEvent += new HandleMousePress(MousePressHandler);
#endif
        }

        private void CreateLights()
        {
            // Create two directional light sources
            LightSource lightSource1 = new LightSource();
            lightSource1.Direction = new Vector3(-1, -1, -1);
            lightSource1.Diffuse = Color.White.ToVector4();
            lightSource1.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            LightSource lightSource2 = new LightSource();
            lightSource2.Direction = new Vector3(0, 1, 0);
            lightSource2.Diffuse = Color.White.ToVector4()*.2f;
            lightSource2.Specular = new Vector4(0.2f, 0.2f, 0.2f, 1);

            // Create a light node to hold the light sources
            LightNode lightNode1 = new LightNode();
            lightNode1.LightSource = lightSource1;

            LightNode lightNode2 = new LightNode();
            lightNode2.LightSource = lightSource2;

            // Add this light node to the root node
            scene.RootNode.AddChild(lightNode1);
            scene.RootNode.AddChild(lightNode2);
        }

        private void CreateCamera()
        {
            // Create a camera
            Camera camera = new Camera();
            // Put the camera at (-6, 0, 4)
            camera.Translation = new Vector3(-6, 0, 4);
            // Rotate the camera -20 degrees about the Y axis
            camera.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0),
                MathHelper.ToRadians(-20));
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

        private void CreateObjects()
        {
            // Loads a textured model of a ship
            ModelLoader loader = new ModelLoader();
            Model shipModel = (Model)loader.Load("", "p1_wedge");

            // Create a geometry node of a loaded ship model
            GeometryNode shipNode = new GeometryNode("Ship");
            shipNode.Model = shipModel;
            // This ship model has material definitions in the model file, so instead
            // of creating a material node for this ship model, we simply use its internal materials
            ((Model)shipNode.Model).UseInternalMaterials = true;

            // Create a transform node to define the transformation for the ship
            TransformNode shipTransNode = new TransformNode();
            shipTransNode.Translation = new Vector3(0, 5, -12); 
            shipTransNode.Scale = new Vector3(0.002f, 0.002f, 0.002f); // It's huge!
            shipTransNode.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0),
                MathHelper.ToRadians(90));

            shipTransParentNode = new TransformNode();
            shipTransParentNode.Translation = Vector3.Zero;

            // Create a torus model and assign it to two geometry nodes
            Model torusModel = (Model)loader.Load("", "torus");// new Torus(2.2f, 3.5f, 30, 30);
 
            GeometryNode torusNode1 = new GeometryNode("Torus1");
            torusNode1.Model = torusModel;
  
            GeometryNode torusNode2 = new GeometryNode("Torus2");
            torusNode2.Model = torusModel;

            // Create transform node for these two torus models
            TransformNode torusTransNode1 = new TransformNode();
            torusTransNode1.Translation = new Vector3(5, 0, -12);

            TransformNode torusTransNode2 = new TransformNode();
            torusTransNode2.Translation = new Vector3(-5, 0, -12);

            torusTransParentNode = new TransformNode();
            torusTransParentNode.Translation = Vector3.Zero;

            // Create a material node for these two torus models
            Material torusMaterial = new Material();
            torusMaterial.Diffuse = Color.DarkGoldenrod.ToVector4();
            torusMaterial.Specular = Color.White.ToVector4();
            torusMaterial.SpecularPower = 5;

            // Now apply this material to these two torus models
            torusNode1.Material = torusMaterial;
            torusNode2.Material = torusMaterial;

            // Now add the above nodes to the scene graph in appropriate order
            scene.RootNode.AddChild(shipTransParentNode);
            shipTransParentNode.AddChild(shipTransNode);
            shipTransNode.AddChild(shipNode);

            scene.RootNode.AddChild(torusTransParentNode);
            torusTransParentNode.AddChild(torusTransNode1);
            torusTransParentNode.AddChild(torusTransNode2);
            torusTransNode1.AddChild(torusNode1);
            torusTransNode2.AddChild(torusNode2);
        }

        private void KeyPressHandler(Keys keys, KeyModifier modifier)
        {
            // Detect key press "a" (with or without a modifier) and toggle the animation
            if (keys == Keys.A)
                firstAnimation = !firstAnimation;
        }

        private void MousePressHandler(int button, Point mouseLocation)
        {
            firstAnimation = !firstAnimation;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            textFont = Content.Load<SpriteFont>("Sample");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
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

            if (firstAnimation)
            {
                shipAngle += gameTime.ElapsedGameTime.TotalSeconds;
                // Rotate the ship model about the origin along Z axis
                shipTransParentNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 
                    (float)shipAngle);
            }
            else
            {
                toriAngle += gameTime.ElapsedGameTime.TotalSeconds;
                // Rotate the two torus models about the origin along Z axis
                torusTransParentNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                    (float)toriAngle);
            }

            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
#if WINDOWS_PHONE
            UI2DRenderer.WriteText(Vector2.Zero, "Tap the screen to toggle the animation!!", Color.GreenYellow,
                textFont);
#else
            // Draw a 2D text string at the center of the screen
            UI2DRenderer.WriteText(Vector2.Zero, "Press 'A' to toggle the animation!!", Color.GreenYellow,
                textFont);
#endif

            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
    }
}
