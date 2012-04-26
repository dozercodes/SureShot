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
 * Author: Nicolas J. Dedual (dedual@cs.columbia.edu)
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

using GoblinXNA.Helpers;

namespace Tutorial17___Interpolation
{
    /// <summary>
    /// This tutorial highlights how GoblinXNA can help you in creating more complex animations 
    /// easily through interpolation.  
    /// </summary>
    /// 
    public class Tutorial17 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        // A scene graph used to render virtual reality
        Scene scene;
        TransformNode shipTransParentNode;

        SpriteFont textFont;
        AnimationHelper animationTranslation;
        AnimationHelper animationRotation;

        int currentInterpolation;
        int currentEasing;

        String textToPrint;

        TypeOfTransition[] arrayOfTransitions = { TypeOfTransition.Linear, TypeOfTransition.Quadratic, TypeOfTransition.Cubic, TypeOfTransition.Quartic, TypeOfTransition.Quintic, TypeOfTransition.Sinusoidal, TypeOfTransition.Sinusoidal, TypeOfTransition.Circular, TypeOfTransition.Elastic, TypeOfTransition.Back, TypeOfTransition.Bounce };
        Easing[] arrayOfEasing = { Easing.EaseIn, Easing.EaseOut, Easing.EaseInOut };
        Vector3 startPosition;
        Vector3 endPosition;

        Vector3 startRotationVector;
        Vector3 endRotationVector;

        public Tutorial17()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
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
            //How we will be keeping track of our interpolation settings
            currentInterpolation = 0;
            currentEasing = 0;

            //Global variables that contain our start and end positions
            startPosition = new Vector3();
            endPosition = new Vector3();

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
            // Put the camera at (0, 0, 10)
            camera.Translation = new Vector3(0, 0, 10);
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
            ((Model)shipNode.Model).ContainsTransparency = true;

            // Create a transform node to define the transformation for the ship
            TransformNode shipTransNode = new TransformNode();
           // shipTransNode.Translation = new Vector3(0, 5, -12);
            shipTransNode.Scale = new Vector3(0.002f, 0.002f, 0.002f); // It's huge!
            shipTransNode.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0),
                MathHelper.ToRadians(-90));

            shipTransParentNode = new TransformNode();
            
            //Set our rotation animation to ease in initially
            animationRotation = new AnimationHelper(Easing.EaseIn);

            startPosition = new Vector3(-10, 0, -10);
            endPosition = new Vector3(5, 2, -5);

            startRotationVector = new Vector3(0, 0, 0);
            endRotationVector = new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(180), MathHelper.ToRadians(180));

            //Set our translation animation to ease in initially
            animationTranslation = new AnimationHelper(Easing.EaseIn);

            //Define what kind of interpolation to use.
            animationTranslation.Animate(arrayOfTransitions[currentInterpolation], startPosition, endPosition, 2);
           // animationTranslation.SetLooping(true, 5); // Use if you want to loop through an animation.

            // Set an action to take place once the animation concludes.
            animationTranslation.SetEndAction(delegate()
            {
                animationRotation.Animate(arrayOfTransitions[currentInterpolation], startRotationVector, endRotationVector, 2.0);
            }); // Note: if you loop and set this animation, it'll occur after the end of the first loop, not after the end of all loops!

            textToPrint = "Linear interpolation";

            // Now add the above nodes to the scene graph in appropriate order
            scene.RootNode.AddChild(shipTransParentNode);
            shipTransParentNode.AddChild(shipTransNode);
            shipTransNode.AddChild(shipNode);
        }

        private void KeyPressHandler(Keys keys, KeyModifier modifier)
        {
            if (keys == Keys.OemMinus)
            {
                currentInterpolation = 0;
                textToPrint = "Linear interpolation";
            }

            if (keys == Keys.D1)
            {
                currentInterpolation = 1;
                textToPrint = "Quadratic interpolation";
            }

            if (keys == Keys.D2)
            {
                currentInterpolation = 2;
                textToPrint = "Cubic interpolation";
            }

            if (keys == Keys.D3)
            {
                currentInterpolation = 3;
                textToPrint = "Quartic interpolation";

            }

            if (keys == Keys.D4)
            {
                currentInterpolation = 4;
                textToPrint = "Quintic interpolation";
            }

            if (keys == Keys.D5)
            {
                currentInterpolation = 5;
                textToPrint = "Sinusoidal interpolation";
            }

            if (keys == Keys.D6)
            {
                currentInterpolation = 6;
                textToPrint = "Exponential interpolation";
            }

            if (keys == Keys.D7)
            {
                currentInterpolation = 7;
                textToPrint = "Circular interpolation";
            }

            if (keys == Keys.D8)
            {
                currentInterpolation = 8;
                textToPrint = "Elastic interpolation";
            }

            if (keys == Keys.D9)
            {
                currentInterpolation = 9;
                textToPrint = "Back interpolation";
            }

            if (keys == Keys.D0)
            {
                currentInterpolation = 10;
                textToPrint = "Bounce interpolation";
            }

            if (keys == Keys.Q)
            {
                currentEasing = 0;
                textToPrint = "Ease in";
            }

            if (keys == Keys.W)
            {
                currentEasing = 1;
                textToPrint = "Ease out";
            }

            if (keys == Keys.E)
            {
                currentEasing = 2;
                textToPrint = "Ease in and out";
            }

            //Play the animation
            if (keys == Keys.Space)
            {

                shipTransParentNode.Rotation = Quaternion.CreateFromYawPitchRoll(0,0,0);

                animationTranslation.CurrentEasing = arrayOfEasing[currentEasing];
                animationTranslation.Animate(arrayOfTransitions[currentInterpolation], endPosition, startPosition, 2);

                animationTranslation.SetEndAction(delegate()
                {
                    animationRotation.CurrentEasing = arrayOfEasing[currentEasing];
                    animationRotation.Animate(arrayOfTransitions[currentInterpolation], startRotationVector, endRotationVector, 2.0);
                });

                //Switch all the values.
                Vector3 temp; Vector3 tempRotation;
                temp = endPosition;
                tempRotation = endRotationVector;

                endPosition = startPosition;
                endRotationVector = startRotationVector;

                startPosition = temp;
                startRotationVector = tempRotation;
            }

            if (keys == Keys.Escape)
            {
                Exit();
            }
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

            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);

            //Update the animations
            animationTranslation.Update(gameTime);
            animationRotation.Update(gameTime);
            
            //Use the values returned by the animation.
            shipTransParentNode.Translation = animationTranslation.ReturnValue;
            shipTransParentNode.Rotation = Quaternion.CreateFromYawPitchRoll(animationRotation.ReturnValue.X, animationRotation.ReturnValue.Y, animationRotation.ReturnValue.Z);

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
            UI2DRenderer.WriteText(new Vector2(0, 35), textToPrint, Color.GreenYellow,
                textFont);
#else
            // Draw a 2D text string at the center of the screen
            UI2DRenderer.WriteText(Vector2.Zero, "Press the 'Space' bar to play the animation!!", Color.Orange,
                textFont);
            UI2DRenderer.WriteText(new Vector2(0, 40), textToPrint, Color.Orange,
                textFont);
#endif

            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
    }
}
