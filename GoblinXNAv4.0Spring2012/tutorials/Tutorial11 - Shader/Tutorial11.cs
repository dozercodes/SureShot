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
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;

namespace Tutorial11___Shader
{
    /// <summary>
    /// This tutorial demonstrates how to create your own shader and incorporate it into
    /// Goblin framework. GeneralShader is exactly same as the OpenGLShader provided in
    /// the GoblinXNA.Shaders package.
    /// </summary>
    public class Tutorial11 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        // A GoblinXNA scene graph
        Scene scene;

        TransformNode sphereTrans;
        TransformNode floorTrans;

        TransformNode pointLightTrans;
        TransformNode spotLingtTransR;
        TransformNode spotLingtTransG;
        TransformNode spotLingtTransB;

        GeneralShader generalShader;

        public Tutorial11()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Initialize the GoblinXNA framework
            State.InitGoblin(graphics, Content, "");

            // Initialize the scene graph
            scene = new Scene();

            // Set the background color to Black
            scene.BackgroundColor = Color.Black;

            // Set up the lights used in the scene
            CreateLights();

            // Set up the camera, which defines the eye location and viewing frustum
            CreateCamera();

            // Create 3D objects
            CreateObject();
        }

        private void CreateLights()
        {
            generalShader = new GeneralShader();

            PrimitiveModel lightModel = new Box(0.2f);
            lightModel.Shader = generalShader;

            //Point Light
            {
                LightSource lightSource = new LightSource();
                lightSource.Type = LightType.Point;
                
                lightSource.Falloff = 2f;
                lightSource.Attenuation0 = 1.0f;
                lightSource.Attenuation1 = 0.01f;
                lightSource.Attenuation2 = 0.001f;
                
                lightSource.Range = 40;

                lightSource.Diffuse = Color.White.ToVector4();
                lightSource.Specular = Color.White.ToVector4();
                
                // Create a light node to hold the light source
                LightNode lightNode = new LightNode();
                lightNode.LightSource = lightSource;
                // Set the ambient light color applied to the entire scene
                lightNode.AmbientLightColor = new Vector4(0.1f, 0.1f, 0.1f, 1);
                
                // Use a transform node to control the light
                pointLightTrans = new TransformNode();
                pointLightTrans.Translation = new Vector3(-10, 10, 0);
                pointLightTrans.AddChild(lightNode);
                
                // Create a geometry to represent the light source position
                GeometryNode bulb = new GeometryNode();
                bulb.Model = lightModel;

                // Create a material which we want to apply to the light model
                Material modelMaterial = new Material();
                modelMaterial.Emissive = Color.White.ToVector4();
                bulb.Material = modelMaterial;

                pointLightTrans.AddChild(bulb);
                // Add this light node to the root node
                scene.RootNode.AddChild(pointLightTrans);
            }

            //Spot Light
            {
                LightSource lightSource = new LightSource();
                lightSource.Type = LightType.SpotLight;

                lightSource.Falloff = 1;
                lightSource.Attenuation0 = 1.0f;
                lightSource.Attenuation1 = 0.0f;
                lightSource.Attenuation2 = 0.0f;

                lightSource.Range = 100;

                lightSource.Diffuse = Color.Red.ToVector4();
                lightSource.Specular = Color.Red.ToVector4();

                lightSource.Direction = new Vector3(0, -1, -1);
                lightSource.InnerConeAngle = MathHelper.ToRadians(30);
                lightSource.OuterConeAngle = MathHelper.ToRadians(35);

                // Create a light node to hold the light source
                LightNode lightNode = new LightNode();
                lightNode.LightSource = lightSource;


                // Use a transform node to control the light
                spotLingtTransR = new TransformNode();
                spotLingtTransR.Translation = new Vector3(0, 10, 10);
                spotLingtTransR.AddChild(lightNode);

                // Create a geometry to represent the light source position
                GeometryNode bulb = new GeometryNode();
                bulb.Model = lightModel;

                // Create a material which we want to apply to the light model
                Material modelMaterial = new Material();
                modelMaterial.Emissive = Color.Red.ToVector4();
                bulb.Material = modelMaterial;

                spotLingtTransR.AddChild(bulb);
                // Add this light node to the root node
                scene.RootNode.AddChild(spotLingtTransR);
            }

            //Spot Light
            {
                LightSource lightSource = new LightSource();
                lightSource.Type = LightType.SpotLight;

                lightSource.Falloff = 1;
                lightSource.Attenuation0 = 1.0f;
                lightSource.Attenuation1 = 0.0f;
                lightSource.Attenuation2 = 0.0f;

                lightSource.Range = 100;

                lightSource.Diffuse = Color.Green.ToVector4();
                lightSource.Specular = Color.Green.ToVector4();

                lightSource.Direction = new Vector3(0, -1, 1);
                lightSource.InnerConeAngle = MathHelper.ToRadians(30);
                lightSource.OuterConeAngle = MathHelper.ToRadians(35);

                // Create a light node to hold the light source
                LightNode lightNode = new LightNode();
                lightNode.LightSource = lightSource;


                // Use a transform node to control the light;
                spotLingtTransG = new TransformNode();
                spotLingtTransG.Translation = new Vector3(0, 10, -10);
                spotLingtTransG.AddChild(lightNode);

                // Create a geometry to represent the light source position
                GeometryNode bulb = new GeometryNode();
                bulb.Model = lightModel;

                // Create a material which we want to apply to the light model
                Material modelMaterial = new Material();
                modelMaterial.Emissive = Color.Green.ToVector4();
                bulb.Material = modelMaterial;

                spotLingtTransG.AddChild(bulb);
                // Add this light node to the root node
                scene.RootNode.AddChild(spotLingtTransG);
            }

            //Spot Light
            {
                LightSource lightSource = new LightSource();
                lightSource.Type = LightType.SpotLight;

                lightSource.Falloff = 1;
                lightSource.Attenuation0 = 1.0f;
                lightSource.Attenuation1 = 0.0f;
                lightSource.Attenuation2 = 0.0f;

                lightSource.Range = 100;

                lightSource.Diffuse = Color.Blue.ToVector4();
                lightSource.Specular = Color.Blue.ToVector4();

                lightSource.Direction = new Vector3(-1, -1, 0);
                lightSource.InnerConeAngle = MathHelper.ToRadians(30);
                lightSource.OuterConeAngle = MathHelper.ToRadians(35);

                // Create a light node to hold the light source
                LightNode lightNode = new LightNode();
                lightNode.LightSource = lightSource;


                // Use a transform node to control the light;
                spotLingtTransB = new TransformNode();
                spotLingtTransB.Translation = new Vector3(10, 10, 0);
                spotLingtTransB.AddChild(lightNode);

                // Create a geometry to represent the light source position
                GeometryNode bulb = new GeometryNode();
                bulb.Model = lightModel;

                // Create a material which we want to apply to the light model
                Material modelMaterial = new Material();
                modelMaterial.Emissive = Color.Blue.ToVector4();
                bulb.Material = modelMaterial;

                spotLingtTransB.AddChild(bulb);
                // Add this light node to the root node
                scene.RootNode.AddChild(spotLingtTransB);
            }
        }

        private void CreateCamera()
        {
            // Create a camera 
            Camera camera = new Camera();

            camera.View = Matrix.CreateLookAt(new Vector3(15, 15, 15), new Vector3(0, 0, 0), Vector3.Up);

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

        private void CreateObject()
        {
            {
                // Create a sphere
                GeometryNode shape = new GeometryNode();
                shape.Model = new Sphere(2, 30, 30);
                shape.Model.Shader = generalShader;

                // Create a material which we want to apply to a sphere model
                Material modelMaterial = new Material();
                modelMaterial.Diffuse = new Vector4(1, 1, 1, 1);
                modelMaterial.Specular = Color.White.ToVector4();
                modelMaterial.SpecularPower = 128f;
                //modelMaterial.Emissive = Color.Yellow.ToVector4();
                shape.Material = modelMaterial;

                sphereTrans = new TransformNode();
                sphereTrans.Translation = new Vector3(0, 2, 0);
                sphereTrans.AddChild(shape);
                scene.RootNode.AddChild(sphereTrans);
            }

            {
                // Create a ground
                GeometryNode shape = new GeometryNode();
                shape.Model = new Box(200, 0.01f, 200);
                shape.Model.Shader = generalShader;

                // Create a material which we want to apply to a ground model
                Material modelMaterial = new Material();
                modelMaterial.Diffuse = new Vector4(1, 1, 1, 1);
                modelMaterial.SpecularPower = 64f;
                

                shape.Material = modelMaterial;

                TransformNode trans = new TransformNode();
                trans.Translation = new Vector3(0, 0, 0);
                trans.AddChild(shape);
                scene.RootNode.AddChild(trans);
            }

            {
                // Create a box
                GeometryNode shape = new GeometryNode();
                shape.Model = new Box(2);
                shape.Model.Shader = generalShader;

                // Create a material which we want to apply to a box model
                Material modelMaterial = new Material();
                modelMaterial.Diffuse = new Vector4(1, 1, 1, 1);
                
                shape.Material = modelMaterial;

                floorTrans = new TransformNode();
                floorTrans.Translation = new Vector3(10, 1, 0);
                floorTrans.AddChild(shape);
                scene.RootNode.AddChild(floorTrans);
            }
        }

        protected override void Dispose(bool disposing)
        {
            scene.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
#if WINDOWS_PHONE
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
#endif
            UpdateControl();
            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        protected void UpdateControl()
        {
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Left))
            {
                pointLightTrans.Translation += Vector3.Left / 10;
            }
            if (keys.IsKeyDown(Keys.Right))
            {
                pointLightTrans.Translation += Vector3.Right / 10;
            }
            if (keys.IsKeyDown(Keys.Up))
            {
                pointLightTrans.Translation += Vector3.Forward / 10;
            }
            if (keys.IsKeyDown(Keys.Down))
            {
                pointLightTrans.Translation += Vector3.Backward / 10;
            }
            if (keys.IsKeyDown(Keys.PageUp))
            {
                pointLightTrans.Translation += Vector3.Up / 10;
            }
            if (keys.IsKeyDown(Keys.PageDown))
            {
                pointLightTrans.Translation += Vector3.Down / 10;
            }
            if (keys.IsKeyDown(Keys.R))
            {
                spotLingtTransR.Rotation = spotLingtTransR.Rotation * 
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.PiOver4 / 10);
            }
            if (keys.IsKeyDown(Keys.B))
            {
                spotLingtTransB.Rotation = spotLingtTransB.Rotation *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.PiOver4 / 10);
            }
            if (keys.IsKeyDown(Keys.G))
            {
                spotLingtTransG.Rotation = spotLingtTransG.Rotation *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.PiOver4 / 10);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
    }
}
