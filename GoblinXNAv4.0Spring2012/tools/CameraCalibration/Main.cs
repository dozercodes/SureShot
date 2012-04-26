/************************************************************************************ 
 * Copyright (c) 2008-2011, Columbia University
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
 * Authors: Ohan Oda (ohan@cs.columbia.edu) 
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Keys = Microsoft.Xna.Framework.Input.Keys;

using GoblinXNA;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Generic;
using GoblinXNA.SceneGraph;
using GoblinXNA.UI;
using GoblinXNA.UI.UI2D;

//using Microsoft.Research.Kinect.Nui;
using Camera = GoblinXNA.SceneGraph.Camera;

namespace CameraCalibration
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteFont font;
        Scene scene;

        IVideoCapture captureDevice;
        IntPtr imagePtr;
        bool useImageSequence = false;
        int sequenceID = 0;
        string[] imageNames;
        int successCount = 0;
        bool calibrateNextSequence = false;
        string imageDirectory = "../../../Images/";

        const int ETALON_ROWS = 6;
        const int ETALON_COLUMNS = 8;
        const int CALIB_COUNT_MAX = 50;
        const float CAPTURE_INTERVAL = 1500; // in milliseconds = 1.5s

        int captureCount = 0;
        float timer = 0;
        bool finalized = false;

        int cameraID = 0;

        string calibrationFilename = "calib.xml";

        public Main()
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

            // Set up the camera, which defines the eye location and viewing frustum
            CreateCamera();

            // Set up the camera calibration
            SetupCalibration();

            State.ShowNotifications = true;
            Notifier.FadeOutTime = 5000;

            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(KeyPressHandler);
        }

        private void KeyPressHandler(Keys key, KeyModifier modifier)
        {
            if ((key == Keys.Escape) && finalized)
                this.Exit();

            if ((key == Keys.Enter || key == Keys.Space) && useImageSequence && !finalized)
            {
                calibrateNextSequence = true;
            }
        }

        private void CreateCamera()
        {
            // Create a camera 
            Camera camera = new Camera();

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

        private void SetupCalibration()
        {
            if (useImageSequence)
                captureDevice = new NullCapture();
            else
                captureDevice = new DirectShowCapture2();
            captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.R8G8B8_24, false);
            if (useImageSequence)
            {
                imageNames = Directory.GetFiles(imageDirectory);

                if (imageNames != null && imageNames.Length > 0)
                {
                    ((NullCapture)captureDevice).StaticImageFile = imageNames[0];
                }
                else
                {
                    MessageBox.Show("No images are found in " + imageDirectory + " for static image calibration");
                    this.Exit();
                }
            }

            // Add this video capture device to the scene so that it can be used for
            // the marker tracker
            scene.AddVideoCaptureDevice(captureDevice);

            imagePtr = Marshal.AllocHGlobal(captureDevice.Width * captureDevice.Height * 3);

            scene.ShowCameraImage = true;

            // Initializes ALVAR camera
            ALVARDllBridge.alvar_init();
            ALVARDllBridge.alvar_add_camera(null, captureDevice.Width, captureDevice.Height);
        }

        private void Calibrate()
        {
            string channelSeq = "RGB";
            int nChannles = 3;

            captureDevice.GetImageTexture(null, ref imagePtr);

            double square_size = 22.8; // in millimeters
            if (ALVARDllBridge.alvar_calibrate_camera(cameraID, nChannles, channelSeq, channelSeq, imagePtr,
                square_size, ETALON_ROWS, ETALON_COLUMNS))
            {
                if (useImageSequence)
                {
                    Notifier.AddMessage(((NullCapture)captureDevice).StaticImageFile + " succeeded");
                    successCount++;
                }
                else
                    Notifier.AddMessage("Captured Image " + (captureCount + 1));
                captureCount++;
            }
            else if (useImageSequence)
            {
                Notifier.AddMessage(((NullCapture)captureDevice).StaticImageFile + " failed");
            }
        }

        private void FinalizeCalibration()
        {
            if (useImageSequence)
                Notifier.AddMessage("Calibrating " + successCount + " images...");
            else
                Notifier.AddMessage("Calibrating...");
            ALVARDllBridge.alvar_finalize_calibration(cameraID, calibrationFilename);

            Notifier.FadeOutTime = -1;
            Notifier.AddMessage("Finished calibration. Saved " + calibrationFilename);

            finalized = true;
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Sample");
        }

        protected override void Dispose(bool disposing)
        {
            scene.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (useImageSequence)
            {
                if (calibrateNextSequence)
                {
                    Calibrate();

                    if (sequenceID < imageNames.Length - 1)
                    {
                        sequenceID++;
                        ((NullCapture)captureDevice).StaticImageFile = imageNames[sequenceID];
                    }
                    else
                    {
                        FinalizeCalibration();
                    }

                    calibrateNextSequence = false;
                }

                UI2DRenderer.WriteText(Vector2.Zero, "Press ENTER key to proceed to the next image", Color.Yellow,
                    font, Vector2.One * 0.6f, GoblinEnums.HorizontalAlignment.Center,
                    GoblinEnums.VerticalAlignment.Bottom);
            }
            else
            {
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                // If we are still collecting calibration data
                // - For every 1.5s add calibration data from detected 7*9 chessboard
                if (captureCount < CALIB_COUNT_MAX)
                {
                    if (timer >= CAPTURE_INTERVAL)
                    {
                        Calibrate();
                        timer = 0;
                    }
                }
                else
                {
                    if (!finalized)
                    {
                        FinalizeCalibration();
                    }
                }
            }

            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
    }
}
