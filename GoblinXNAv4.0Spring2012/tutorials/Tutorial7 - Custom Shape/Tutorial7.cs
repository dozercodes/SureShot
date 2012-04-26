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
using GoblinXNA.UI.UI2D;
using Model = GoblinXNA.Graphics.Model;

namespace Tutorial7___Custom_Shape
{
    /// <summary>
    /// This tutorial demonstrates how to create a custom shape using our PrimitiveShape class.
    /// </summary>
    public class Tutorial7 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        Scene scene;

        TransformNode genericInputNode;
        LightSource lightSource;

        SpriteFont textFont;

        public Tutorial7()
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
            CreateObject();

            KeyboardInput.Instance.KeyTypeEvent += new HandleKeyType(KeyTypeHandler);
        }

        /// <summary>
        /// Handle the key input to change the light direction
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modifier"></param>
        private void KeyTypeHandler(Keys key, KeyModifier modifier)
        {
            if (key == Keys.A && lightSource.Direction.X < 1)
                lightSource.Direction = new Vector3(lightSource.Direction.X + 0.1f, 
                    lightSource.Direction.Y, lightSource.Direction.Z);
            else if (key == Keys.Z && lightSource.Direction.X > -1)
                lightSource.Direction = new Vector3(lightSource.Direction.X - 0.1f,
                    lightSource.Direction.Y, lightSource.Direction.Z);
            else if (key == Keys.S && lightSource.Direction.Y < 1)
                lightSource.Direction = new Vector3(lightSource.Direction.X,
                    lightSource.Direction.Y + 0.1f, lightSource.Direction.Z);
            else if (key == Keys.X && lightSource.Direction.Y > -1)
                lightSource.Direction = new Vector3(lightSource.Direction.X,
                    lightSource.Direction.Y - 0.1f, lightSource.Direction.Z);
            else if (key == Keys.D && lightSource.Direction.Z < 1)
                lightSource.Direction = new Vector3(lightSource.Direction.X,
                    lightSource.Direction.Y, lightSource.Direction.Z + 0.1f);
            else if (key == Keys.C && lightSource.Direction.Z > -1)
                lightSource.Direction = new Vector3(lightSource.Direction.X,
                    lightSource.Direction.Y, lightSource.Direction.Z - 0.1f);
        }

        private void CreateLights()
        {
            // Create a directional light source
            lightSource = new LightSource();
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
            // Put the camera at (0, 3, 7)
            camera.Translation = new Vector3(0, 3, 7);
            // Rotate the camera -15 degrees along the X axis
            camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                MathHelper.ToRadians(-15));
            // Set the vertical field of view to be 45 degrees
            camera.FieldOfViewY = MathHelper.ToRadians(45); 
            // Set the near clipping plane to be 0.1f unit away from the camera
            camera.ZNearPlane = 0.1f;
            // Set the far clipping plane to be 1000 units away from the camera
            camera.ZFarPlane = 1000;

            // Set the initial translation and rotation of the generic input which controls 
            // the camera transform in this application
            GenericInput.Instance.InitialTranslation = camera.Translation;
            GenericInput.Instance.InitialRotation = camera.Rotation;

            // Adjust the speed of the camera control
            GenericInput.Instance.PanSpeed = 0.05f;
            GenericInput.Instance.ZoomSpeed = 0.5f;
            GenericInput.Instance.RotateSpeed = 0.02f;

            // Now assign this camera to a camera node
            CameraNode cameraNode = new CameraNode(camera);

            // Add a transform node which will be updated based on GenericInput class
            // which implements simple camera rotation and translation based on mouse
            // drag and key presses.
            // To rotate the camera, hold right mouse button and drag around
            genericInputNode = new TransformNode();

            scene.RootNode.AddChild(genericInputNode);
            genericInputNode.AddChild(cameraNode);

            // Assign the camera node to be our scene graph's current camera node
            scene.CameraNode = cameraNode;
        }

        private void CreateObject()
        {
            // Create a primitive mesh for creating our custom pyramid shape
            CustomMesh pyramid = new CustomMesh();

            // Even though we really need 5 vertices to create a pyramid shape, since
            // we want to assign different normals for points with same position to have
            // more accurate lighting effect, we will use 16 vertices.
            // Also, we want to have position, normal, and texture information in our
            // vertices, we'll use VertexPositionNormalTexture format. There are other
            // types of Vertex format as well depending on your needs. 
            VertexPositionNormalTexture[] verts = new VertexPositionNormalTexture[16];

            // Create a pyramid with 8x8 base and height of 6
            Vector3 vBase0 = new Vector3(-4, 0, 4);
            Vector3 vBase1 = new Vector3(4, 0, 4);
            Vector3 vBase2 = new Vector3(4, 0, -4);
            Vector3 vBase3 = new Vector3(-4, 0, -4);
            Vector3 vTop = new Vector3(0, 6, 0);

            verts[0].Position = vTop;
            verts[1].Position = vBase1;
            verts[2].Position = vBase0;

            verts[3].Position = vTop;
            verts[4].Position = vBase2;
            verts[5].Position = vBase1;

            verts[6].Position = vTop;
            verts[7].Position = vBase3;
            verts[8].Position = vBase2;

            verts[9].Position = vTop;
            verts[10].Position = vBase0;
            verts[11].Position = vBase3;

            verts[12].Position = vBase0;
            verts[13].Position = vBase1;
            verts[14].Position = vBase2;
            verts[15].Position = vBase3;

            verts[0].Normal = verts[1].Normal = verts[2].Normal = CalcNormal(vTop, vBase1, vBase0);
            verts[3].Normal = verts[4].Normal = verts[5].Normal = CalcNormal(vTop, vBase2, vBase1);
            verts[6].Normal = verts[7].Normal = verts[8].Normal = CalcNormal(vTop, vBase3, vBase2);
            verts[9].Normal = verts[10].Normal = verts[11].Normal = CalcNormal(vTop, vBase0, vBase3);
            verts[12].Normal = verts[13].Normal = verts[14].Normal = verts[15].Normal = CalcNormal(vBase0, 
                vBase1, vBase3);

            Vector2 texCoordTop = new Vector2(0.5f, 0);
            Vector2 texCoordLeftBase = new Vector2(0, 1);
            Vector2 texCoordRightBase = new Vector2(1, 1);

            verts[0].TextureCoordinate = texCoordTop;
            verts[1].TextureCoordinate = texCoordLeftBase;
            verts[2].TextureCoordinate = texCoordRightBase;

            verts[3].TextureCoordinate = texCoordTop;
            verts[4].TextureCoordinate = texCoordLeftBase;
            verts[5].TextureCoordinate = texCoordRightBase;

            verts[6].TextureCoordinate = texCoordTop;
            verts[7].TextureCoordinate = texCoordLeftBase;
            verts[8].TextureCoordinate = texCoordRightBase;

            verts[9].TextureCoordinate = texCoordTop;
            verts[10].TextureCoordinate = texCoordLeftBase;
            verts[11].TextureCoordinate = texCoordRightBase;

            verts[12].TextureCoordinate = new Vector2(1, 1);
            verts[13].TextureCoordinate = new Vector2(1, 0);
            verts[14].TextureCoordinate = new Vector2(0, 0);
            verts[15].TextureCoordinate = new Vector2(0, 1);

            pyramid.VertexBuffer = new VertexBuffer(graphics.GraphicsDevice, 
                typeof(VertexPositionNormalTexture), 16, BufferUsage.None);
            pyramid.VertexDeclaration = VertexPositionNormalTexture.VertexDeclaration;
            pyramid.VertexBuffer.SetData(verts);
            pyramid.NumberOfVertices = 16;

            short[] indices = new short[18];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 3;
            indices[4] = 4;
            indices[5] = 5;

            indices[6] = 6;
            indices[7] = 7;
            indices[8] = 8;

            indices[9] = 9;
            indices[10] = 10;
            indices[11] = 11;

            indices[12] = 12;
            indices[13] = 13;
            indices[14] = 15;

            indices[15] = 13;
            indices[16] = 14;
            indices[17] = 15;

            pyramid.IndexBuffer = new IndexBuffer(graphics.GraphicsDevice, typeof(short), 18, 
                BufferUsage.WriteOnly);
            pyramid.IndexBuffer.SetData(indices);

            pyramid.PrimitiveType = PrimitiveType.TriangleList;
            pyramid.NumberOfPrimitives = 6;

            PrimitiveModel pyramidModel = new PrimitiveModel(pyramid);

            GeometryNode pyramidNode = new GeometryNode();
            pyramidNode.Model = pyramidModel;

            Material pyramidMaterial = new Material();
            pyramidMaterial.Diffuse = Color.White.ToVector4();
            pyramidMaterial.Specular = Color.White.ToVector4();
            pyramidMaterial.SpecularPower = 10;
            pyramidMaterial.Texture = Content.Load<Texture2D>("brick_wall_14");

            pyramidNode.Material = pyramidMaterial;

            scene.RootNode.AddChild(pyramidNode);
        }

        /// <summary>
        /// Calculate the normal of two vectors v0->v1 and v0->v2 using right hand rule
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private Vector3 CalcNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 v0_1 = v1 - v0;
            Vector3 v0_2 = v2 - v0;

            Vector3 normal = Vector3.Cross(v0_2, v0_1);
            normal.Normalize();

            return normal;
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

            // Update the generic input's base transform to use for panning
            GenericInput.Instance.BaseTransformation = scene.CameraNode.WorldTransformation;

            // Move the camera based on the generic input's rotation information
            // You can uncomment the translation update so that you can move the
            // camera as well using key presses. I commented it out here because
            // the light direction manipulation uses key press as well
            genericInputNode.Translation = GenericInput.Instance.Translation;
            genericInputNode.Rotation = GenericInput.Instance.Rotation;

            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            UI2DRenderer.WriteText(Vector2.Zero, "Light Direction: " + lightSource.Direction.ToString(),
                Color.White, textFont);

            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
    }
}
