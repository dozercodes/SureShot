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
using System.Runtime.InteropServices;

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
using GoblinXNA.Device;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Util;
using GoblinXNA.Device.Generic;
using GoblinXNA.Helpers;

namespace Tutorial15___OpenCV
{
    /// <summary>
    /// This tutorial demonstrates how to use OpenCV 2.1 with Goblin XNA
    /// </summary>
    public class Tutorial15 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Scene scene;

        Texture2D overlayTexture;
        short[] overlayData;
        bool drawOverlay = true;

        public Tutorial15()
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

            State.InitGoblin(graphics, Content, "");

            scene = new Scene();

            SetupCaptureDevices();

            CreateLights();

            CreateCamera();

            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(KeyPress);

            State.ShowFPS = true;
        }

        private void KeyPress(Keys key, KeyModifier modifier)
        {
            if (key == Keys.Escape)
                this.Exit();

            if (key == Keys.Space)
                drawOverlay = !drawOverlay;
        }

        private void CreateLights()
        {
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(1, -1, -1);
            lightSource.Diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1);
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            LightNode lightNode = new LightNode();
            lightNode.AmbientLightColor = new Vector4(0.2f, 0.2f, 0.2f, 1);
            lightNode.LightSource = lightSource;

            scene.RootNode.AddChild(lightNode);
        }

        private void CreateCamera()
        {
            Camera camera = new Camera();
            camera.Translation = Vector3.Zero;
            camera.FieldOfViewY = MathHelper.ToRadians(60);
            camera.ZNearPlane = 1;
            camera.ZFarPlane = 1000;

            CameraNode cameraNode = new CameraNode(camera);

            scene.RootNode.AddChild(cameraNode);
            scene.CameraNode = cameraNode;
        }

        private void SetupCaptureDevices()
        {
            // Create our video capture device that uses OpenCV library.
            OpenCVCapture captureDevice = new OpenCVCapture();
            captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.R8G8B8_24, false);

            // Create a 16bit color texture that contains the image processed by OpenCV
            // We're using alpha gray texture because we want the black color to represent
            // transparent color so that we can overlay the texture properly on top of the live
            // video image
            overlayTexture = new Texture2D(GraphicsDevice, captureDevice.Width, captureDevice.Height,
                false, SurfaceFormat.Bgra4444);
            // Create an array that will contain the image data of overlayTexture
            overlayData = new short[overlayTexture.Width * overlayTexture.Height];

            // Assigns a callback function to be called whenever a new video frame is captured
            captureDevice.CaptureCallback = delegate(IntPtr image, int[] background)
            {
                // Creates a holder for an OpenCV image
                IntPtr grayImage = OpenCVWrapper.cvCreateImage(OpenCVWrapper.cvGetSize(image), 8, 1);

                // Converts the color image (live video image) to a gray image
                OpenCVWrapper.cvCvtColor(image, grayImage, OpenCVWrapper.CV_BGR2GRAY);

                // Performs canny edge detection on the gray image
                OpenCVWrapper.cvCanny(grayImage, grayImage, 10, 200, 3);

                // Converts the gray image pointer to IplImage structure so that we can access
                // the image data of the processed gray image
                OpenCVWrapper.IplImage videoImage = (OpenCVWrapper.IplImage)Marshal.PtrToStructure(grayImage,
                    typeof(OpenCVWrapper.IplImage));

                unsafe
                {
                    int index = 0;
                    // Gets a pointer to the first byte of the image data
                    byte* src = (byte*)videoImage.imageData;
                    // Iterates through the image pointer
                    for (int i = 0; i < videoImage.height; i++)
                    {
                        for (int j = 0; j < videoImage.width; j++)
                        {
                            // src data contains 8 bit gray scaled color, so we need to convert it
                            // to Rgba4444 format.
                            // We assign the black color to be totally transparent
                            overlayData[index++] = (short)((*(src) << 8) | (*(src)));
                            src++;
                        }
                    }
                }

                // Resets the texture assigned to the device (this is needed for XNA 3.1 since
                // they have a bug)
                GraphicsDevice.Textures[0] = null;
                // Assigns the image data to the overlay texture
                overlayTexture.SetData<short>(overlayData);

                // Deallocates the memory assigned to the OpenCV image
                OpenCVWrapper.cvReleaseImage(ref grayImage);

                // We don't want to modify the original video image, so we return false
                return false;
            };

            scene.AddVideoCaptureDevice(captureDevice);

            scene.ShowCameraImage = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Dispose(bool disposing)
        {
            scene.Dispose();
        }

        /// <summary>
        /// Allows the game component to update itself.
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
            // Draws the screen
            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);

            if (drawOverlay)
            {
                spriteBatch.Begin();

                // Draws the overlay image processed by OpenCV on top of the live video
                spriteBatch.Draw(overlayTexture, new Rectangle(0, 0, State.Width, State.Height), 
                    Color.White);

                spriteBatch.End();
            }
        }
    }
}