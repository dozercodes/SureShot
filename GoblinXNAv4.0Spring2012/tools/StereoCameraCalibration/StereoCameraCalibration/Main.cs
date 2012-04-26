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
using System.Threading;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using GoblinXNA;
using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.iWear;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.UI;
using GoblinXNA.Device.Generic;
using GoblinXNA.Device;
using GoblinXNA.Helpers;
using GoblinXNA.Device.Util;
using GoblinXNA.Network;

namespace StereoCameraCalibration
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        int CALIB_COUNT_MAX;
        string calibrationFilename;
        string adjustmentsFilename;
        string LEFT_CALIB;
        string RIGHT_CALIB;
        float EXPECTED_GAP_MIN;
        float EXPECTED_GAP_MAX;
        int leftDeviceID;
        int rightDeviceID;

        GraphicsDeviceManager graphics;

        Scene scene;

        RenderTarget2D stereoScreenLeft;
        RenderTarget2D stereoScreenRight;
        Rectangle leftRect;
        Rectangle rightRect;
        Rectangle leftSource;
        Rectangle rightSource;
        SpriteBatch spriteBatch;

        ALVARMarkerTracker markerTracker;
        Object markerID;

        IVideoCapture leftCaptureDevice;
        IVideoCapture rightCaptureDevice;
        Texture2D leftTexture;
        Texture2D rightTexture;
        int[] leftVideoData;
        int[] rightVideoData;
        IntPtr leftImagePtr;
        IntPtr rightImagePtr;
        bool calibrating = false;

        iWearTracker iTracker;

        List<Matrix> relativeTransforms;

        Thread calibrationThread;

        int captureCount = 0;
        bool finalized = false;

        TransformNode groundMarkerNode;

        int camWidthAdjustmentLeft = 0;
        int camHShiftAdjustmentLeft = 0;
        int camVShiftAdjustmentLeft = 0;
        int camWidthAdjustmentRight = 0;
        int camHShiftAdjustmentRight = 0;
        int camVShiftAdjustmentRight = 0;
        bool adjustingLeft = true;

        int stereoWidth;
        int stereoHeight;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Initialize the GoblinXNA framework
            State.InitGoblin(graphics, Content, "Setting.xml");

            if (bool.Parse(State.GetSettingVariable("IsFullScreen")))
            {
                graphics.IsFullScreen = true;
            }
            else
            {
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 480;
            }
            graphics.ApplyChanges();

            LEFT_CALIB = State.GetSettingVariable("LeftCameraCalibration");
            RIGHT_CALIB = State.GetSettingVariable("RightCameraCalibration");
            calibrationFilename = State.GetSettingVariable("StereoCameraCalibration");
            adjustmentsFilename = State.GetSettingVariable("StereoCameraAdjustments");

            leftDeviceID = int.Parse(State.GetSettingVariable("LeftCameraID"));
            rightDeviceID = int.Parse(State.GetSettingVariable("RightCameraID"));

            CALIB_COUNT_MAX = int.Parse(State.GetSettingVariable("CalibrationCount"));
            EXPECTED_GAP_MIN = float.Parse(State.GetSettingVariable("ExpectedMinDistance"));
            EXPECTED_GAP_MAX = float.Parse(State.GetSettingVariable("ExpectedMaxDistance"));

            // Initialize the scene graph
            scene = new Scene();

            // Set up the VUZIX's iWear Wrap920AR for stereo
            SetupIWear();

            // Set up the stereo camera
            SetupStereoCamera();

            SetupViewports();

            // Set up the stereo camera calibration
            SetupCalibration();

            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(HandleKeyPressEvent);

            State.ShowNotifications = true;
            Notifier.FadeOutTime = 2000;
        }

        private void HandleKeyPressEvent(Keys key, KeyModifier modifier)
        {
            // This is somewhat necessary to exit from full screen mode
            if (key == Keys.Escape)
                this.Exit();

            if(!finalized)
            {
                if (key == Keys.Space)
                {
                    if (!calibrating)
                    {
                        calibrationThread = new Thread(CalibrateStereo);
                        calibrationThread.Start();
                    }
                }
            }
            else
            {
                switch (key)
                {
                    case Keys.Space:
                        SaveAdjustments();
                        break;
                    case Keys.Down:
                        if (adjustingLeft)
                        {
                            camWidthAdjustmentLeft++;
                            int camHeightAdjustment = camWidthAdjustmentLeft * stereoHeight / stereoWidth;

                            leftSource = new Rectangle(camWidthAdjustmentLeft + camHShiftAdjustmentLeft,
                                camHeightAdjustment + camVShiftAdjustmentLeft,
                                stereoWidth - camWidthAdjustmentLeft * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        else
                        {
                            camWidthAdjustmentRight++;
                            int camHeightAdjustment = camWidthAdjustmentRight * stereoHeight / stereoWidth;

                            rightSource = new Rectangle(camWidthAdjustmentRight + camHShiftAdjustmentRight,
                                camHeightAdjustment + camVShiftAdjustmentRight,
                                stereoWidth - camWidthAdjustmentRight * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        break;
                    case Keys.Up:
                        if (adjustingLeft)
                        {
                            camWidthAdjustmentLeft--;
                            int camHeightAdjustment = camWidthAdjustmentLeft * stereoHeight / stereoWidth;

                            leftSource = new Rectangle(camWidthAdjustmentLeft + camHShiftAdjustmentLeft,
                                camHeightAdjustment + camVShiftAdjustmentLeft,
                                stereoWidth - camWidthAdjustmentLeft * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        else
                        {
                            camWidthAdjustmentRight--;
                            int camHeightAdjustment = camWidthAdjustmentRight * stereoHeight / stereoWidth;

                            rightSource = new Rectangle(camWidthAdjustmentRight + camHShiftAdjustmentRight,
                                camHeightAdjustment + camVShiftAdjustmentRight,
                                stereoWidth - camWidthAdjustmentRight * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        break;
                    case Keys.Right:
                        if (adjustingLeft)
                        {
                            camHShiftAdjustmentLeft--;
                            int camHeightAdjustment = camWidthAdjustmentLeft * stereoHeight / stereoWidth;

                            leftSource = new Rectangle(camWidthAdjustmentLeft + camHShiftAdjustmentLeft,
                                camHeightAdjustment + camVShiftAdjustmentLeft,
                                stereoWidth - camWidthAdjustmentLeft * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        else
                        {
                            camHShiftAdjustmentRight--;
                            int camHeightAdjustment = camWidthAdjustmentRight * stereoHeight / stereoWidth;

                            rightSource = new Rectangle(camWidthAdjustmentRight + camHShiftAdjustmentRight,
                                camHeightAdjustment + camVShiftAdjustmentRight,
                                stereoWidth - camWidthAdjustmentRight * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        break;
                    case Keys.Left:
                        if (adjustingLeft)
                        {
                            camHShiftAdjustmentLeft++;
                            int camHeightAdjustment = camWidthAdjustmentLeft * stereoHeight / stereoWidth;

                            leftSource = new Rectangle(camWidthAdjustmentLeft + camHShiftAdjustmentLeft,
                                camHeightAdjustment + camVShiftAdjustmentLeft,
                                stereoWidth - camWidthAdjustmentLeft * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        else
                        {
                            camHShiftAdjustmentRight++;
                            int camHeightAdjustment = camWidthAdjustmentRight * stereoHeight / stereoWidth;

                            rightSource = new Rectangle(camWidthAdjustmentRight + camHShiftAdjustmentRight,
                                camHeightAdjustment + camVShiftAdjustmentRight,
                                stereoWidth - camWidthAdjustmentRight * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        break;
                    case Keys.PageUp:
                        if (adjustingLeft)
                        {
                            camVShiftAdjustmentLeft--;
                            int camHeightAdjustment = camWidthAdjustmentLeft * stereoHeight / stereoWidth;

                            leftSource = new Rectangle(camWidthAdjustmentLeft + camHShiftAdjustmentLeft,
                                camHeightAdjustment + camVShiftAdjustmentLeft,
                                stereoWidth - camWidthAdjustmentLeft * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        else
                        {
                            camVShiftAdjustmentRight--;
                            int camHeightAdjustment = camWidthAdjustmentRight * stereoHeight / stereoWidth;

                            rightSource = new Rectangle(camWidthAdjustmentRight + camHShiftAdjustmentRight,
                                camHeightAdjustment + camVShiftAdjustmentRight,
                                stereoWidth - camWidthAdjustmentRight * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        break;
                    case Keys.PageDown:
                        if (adjustingLeft)
                        {
                            camVShiftAdjustmentLeft++;
                            int camHeightAdjustment = camWidthAdjustmentLeft * stereoHeight / stereoWidth;

                            leftSource = new Rectangle(camWidthAdjustmentLeft + camHShiftAdjustmentLeft,
                                camHeightAdjustment + camVShiftAdjustmentLeft,
                                stereoWidth - camWidthAdjustmentLeft * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        else
                        {
                            camVShiftAdjustmentRight++;
                            int camHeightAdjustment = camWidthAdjustmentRight * stereoHeight / stereoWidth;

                            rightSource = new Rectangle(camWidthAdjustmentRight + camHShiftAdjustmentRight,
                                camHeightAdjustment + camVShiftAdjustmentRight,
                                stereoWidth - camWidthAdjustmentRight * 2, stereoHeight - camHeightAdjustment * 2);
                        }
                        break;
                    case Keys.Enter:
                        adjustingLeft = !adjustingLeft;
                        break;
                }
            }
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(1, -0.75f, -0.5f);
            lightSource.Diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1);
            lightSource.Specular = new Vector4(0.4f, 0.4f, 0.4f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            // Add an ambient component
            lightNode.AmbientLightColor = new Vector4(0.15f, 0.15f, 0.15f, 1);
            lightNode.LightSource = lightSource;

            // Add this light node to the root node
            groundMarkerNode.AddChild(lightNode);
        }

        private void SetupStereoCamera()
        {
            StereoCamera camera = new StereoCamera();
            camera.Translation = new Vector3(0, 0, 0);

            CameraNode cameraNode = new CameraNode(camera);

            scene.RootNode.AddChild(cameraNode);
            scene.CameraNode = cameraNode;
        }

        private void SetupViewports()
        {
            stereoWidth = State.Width / 2;
            stereoHeight = State.Height;

            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            stereoScreenLeft = new RenderTarget2D(GraphicsDevice, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);
            stereoScreenRight = new RenderTarget2D(GraphicsDevice, stereoWidth, stereoHeight, false,
                SurfaceFormat.Color, pp.DepthStencilFormat);

            leftRect = new Rectangle(0, 0, stereoWidth, stereoHeight);
            rightRect = new Rectangle(stereoWidth, 0, stereoWidth, stereoHeight);

            int camHeightAdjustment = camWidthAdjustmentLeft * stereoHeight / stereoWidth;

            leftSource = new Rectangle(camWidthAdjustmentLeft + camHShiftAdjustmentLeft,
                camHeightAdjustment + camVShiftAdjustmentLeft,
                stereoWidth - camWidthAdjustmentLeft * 2, stereoHeight - camHeightAdjustment * 2);

            camHeightAdjustment = camWidthAdjustmentRight * stereoHeight / stereoWidth;

            rightSource = new Rectangle(camWidthAdjustmentRight + camHShiftAdjustmentRight,
                camHeightAdjustment + camVShiftAdjustmentRight,
                stereoWidth - camWidthAdjustmentRight * 2, stereoHeight - camHeightAdjustment * 2);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            scene.BackgroundBound = leftRect;
        }

        private void SetupCalibration()
        {
            leftCaptureDevice = new DirectShowCapture2();
            leftCaptureDevice.InitVideoCapture(leftDeviceID, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.R8G8B8_24, false);

            // Add left video capture device to the scene for rendering left eye image
            scene.AddVideoCaptureDevice(leftCaptureDevice);

            rightCaptureDevice = new DirectShowCapture2();
            rightCaptureDevice.InitVideoCapture(rightDeviceID, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.R8G8B8_24, false);

            // Add right video capture device to the scene for rendering right eye image
            scene.AddVideoCaptureDevice(rightCaptureDevice);

            // Create holders for retrieving the captured video images
            leftImagePtr = Marshal.AllocHGlobal(leftCaptureDevice.Width * leftCaptureDevice.Height * 3);
            rightImagePtr = Marshal.AllocHGlobal(rightCaptureDevice.Width * rightCaptureDevice.Height * 3);

            // Associate each video devices to each eye
            scene.LeftEyeVideoID = 0;
            scene.RightEyeVideoID = 1;

            scene.ShowCameraImage = true;

            float markerSize = 32.4f;

            // Initialize a marker tracker for tracking an marker array used for calibration
            markerTracker = new ALVARMarkerTracker();
            markerTracker.MaxMarkerError = 0.02f;
            markerTracker.ZNearPlane = 0.1f;
            markerTracker.ZFarPlane = 1000;
            markerTracker.InitTracker(leftCaptureDevice.Width, leftCaptureDevice.Height, LEFT_CALIB, markerSize);
            ((StereoCamera)scene.CameraNode.Camera).LeftProjection = markerTracker.CameraProjection;

            // Add another marker detector for tracking right video capture device
            ALVARDllBridge.alvar_add_marker_detector(markerSize, 5, 2);

            ALVARDllBridge.alvar_add_camera(RIGHT_CALIB, rightCaptureDevice.Width, rightCaptureDevice.Height);
            double[] projMat = new double[16];
            double cameraFovX = 0, cameraFovY = 0;
            ALVARDllBridge.alvar_get_camera_params(1, projMat, ref cameraFovX, ref cameraFovY, 1000, 0.1f);
            ((StereoCamera)scene.CameraNode.Camera).RightProjection = new Matrix(
                (float)projMat[0], (float)projMat[1], (float)projMat[2], (float)projMat[3],
                (float)projMat[4], (float)projMat[5], (float)projMat[6], (float)projMat[7],
                (float)projMat[8], (float)projMat[9], (float)projMat[10], (float)projMat[11],
                (float)projMat[12], (float)projMat[13], (float)projMat[14], (float)projMat[15]);

            // Add a marker array to be tracked
            markerID = markerTracker.AssociateMarker("ALVARGroundArray.xml");

            relativeTransforms = new List<Matrix>();
        }

        private void SetupIWear()
        {
            // Get an instance of iWearTracker
            iTracker = iWearTracker.Instance;
            // We need to initialize it before adding it to the InputMapper class
            iTracker.Initialize();

            iTracker.EnableStereo = true;
            // Add this iWearTracker to the InputMapper class for automatic update and disposal
            InputMapper.Instance.Add6DOFInputDevice(iTracker);
            // Re-enumerate all of the input devices so that the newly added device can be found
            InputMapper.Instance.Reenumerate();
        }

        private void CalibrateStereo()
        {
            calibrating = true;

            // get the left and right camera iamges
            leftCaptureDevice.GetImageTexture(null, ref leftImagePtr);
            rightCaptureDevice.GetImageTexture(null, ref rightImagePtr);

            markerTracker.DetectorID = 0;
            markerTracker.CameraID = 0;
            markerTracker.ProcessImage(leftCaptureDevice, leftImagePtr);

            bool markerFoundOnLeftVideo = markerTracker.FindMarker(markerID);

            if (markerFoundOnLeftVideo)
            {
                Matrix leftEyeTransform = markerTracker.GetMarkerTransform();

                markerTracker.DetectorID = 1;
                markerTracker.CameraID = 1;
                markerTracker.ProcessImage(rightCaptureDevice, rightImagePtr);

                bool markerFoundOnRightVideo = markerTracker.FindMarker(markerID);

                if (markerFoundOnRightVideo)
                {
                    Matrix rightEyeTransform = markerTracker.GetMarkerTransform();

                    leftEyeTransform = Matrix.Invert(leftEyeTransform);
                    rightEyeTransform = Matrix.Invert(rightEyeTransform);

                    Matrix relativeTransform = rightEyeTransform * Matrix.Invert(leftEyeTransform);
                    Vector3 rawScale, rawPos;
                    Quaternion rawRot;
                    relativeTransform.Decompose(out rawScale, out rawRot, out rawPos);

                    float xGap = Math.Abs(rawPos.X);
                    float yGap = Math.Abs(rawPos.Y);
                    float zGap = Math.Abs(rawPos.Z);

                    float xyRatio = yGap / xGap;
                    float xzRatio = zGap / xGap;

                    if (xyRatio < 0.2 && xzRatio < 0.2 && rawPos.Length() > EXPECTED_GAP_MIN && rawPos.Length() < EXPECTED_GAP_MAX)
                    {
                        relativeTransforms.Add(relativeTransform);

                        Console.WriteLine("Completed calculation " + (captureCount + 1));
                        Notifier.AddMessage("Completed calculation: " + (captureCount + 1) + "/" + CALIB_COUNT_MAX);

                        rawScale = Vector3Helper.QuaternionToEulerAngleVector3(rawRot);
                        rawScale = Vector3Helper.RadiansToDegrees(rawScale);
                        Console.WriteLine("Pos: " + rawPos.ToString() + ", Length: " + rawPos.Length() + ", Yaw: " 
                            + rawScale.X + ", Pitch: " + rawScale.Y + ": Roll, " + rawScale.Z);
                        Console.WriteLine();
                        captureCount++;
                    }
                    else
                    {
                        Console.WriteLine("Failed: Pos: " + rawPos.ToString() + ", Length: " + rawPos.Length());
                        Console.WriteLine();
                        Notifier.AddMessage("Failed. Try again");
                    }
                }
            }

            if (captureCount >= CALIB_COUNT_MAX)
            {
                SaveCalibration();

                Console.WriteLine("Finished calibration. Saved " + calibrationFilename);
                Notifier.AddMessage("Finished calibration!!");

                finalized = true;
            }

            calibrating = false;
        }

        private void SaveCameraImages()
        {
            if (leftVideoData == null)
            {
                leftVideoData = new int[leftCaptureDevice.Width * leftCaptureDevice.Height];
                rightVideoData = new int[rightCaptureDevice.Width * rightCaptureDevice.Height];

                leftTexture = new Texture2D(State.Device, leftCaptureDevice.Width, leftCaptureDevice.Height, false,
                    SurfaceFormat.Color);
                rightTexture = new Texture2D(State.Device, rightCaptureDevice.Width, rightCaptureDevice.Height, false,
                    SurfaceFormat.Color);

                if (!Directory.Exists("Images"))
                    Directory.CreateDirectory("Images");
            }

            IntPtr zeroPtr = IntPtr.Zero;
            leftCaptureDevice.GetImageTexture(leftVideoData, ref zeroPtr);
            rightCaptureDevice.GetImageTexture(rightVideoData, ref zeroPtr);

            int alpha = (int)(255 << 24);
            for (int i = 0; i < leftVideoData.Length; ++i)
            {
                leftVideoData[i] |= alpha;
                rightVideoData[i] |= alpha;
            }

            leftTexture.SetData<int>(leftVideoData);
            rightTexture.SetData<int>(rightVideoData);

            captureCount++;
            leftTexture.SaveAsPng(new FileStream("Images/left" + captureCount.ToString("00") + ".png", FileMode.Create, FileAccess.Write),
                leftTexture.Width, leftTexture.Height);
            rightTexture.SaveAsPng(new FileStream("Images/right" + captureCount.ToString("00") + ".png", FileMode.Create, FileAccess.Write),
                rightTexture.Width, rightTexture.Height);

            Console.WriteLine("Completed calculation " + (captureCount));

            if (captureCount > CALIB_COUNT_MAX)
            {
                Console.WriteLine("Finished calibration. Saved " + calibrationFilename);

                finalized = true;
            }
        }

        private void SaveCalibration()
        {
            Vector3 rawScale, rawPos;
            Quaternion rawRot;

            Vector3 posSum = Vector3.Zero;
            Quaternion rotSum = Quaternion.Identity;

            int count = relativeTransforms.Count;
            for (int i = 0; i < count; i++)
            {
                relativeTransforms[i].Decompose(out rawScale, out rawRot, out rawPos);

                posSum += rawPos;
                rotSum += rawRot;
            }

            Vector3 avgPos = posSum / count;
            rotSum.Normalize();
            rawScale = Vector3Helper.RadiansToDegrees(Vector3Helper.QuaternionToEulerAngleVector3(rotSum));

            Console.WriteLine("Relative camera position & orientation:");
            Console.WriteLine("Pos: " + avgPos.ToString() + ", Length: " + avgPos.Length() + ", Yaw: " 
                + rawScale.X + ", Pitch: " + rawScale.Y + ", Roll: " + rawScale.Z);

            Matrix avgTransform = Matrix.CreateFromQuaternion(rotSum);
            avgTransform.Translation = avgPos;

            MatrixHelper.SaveMatrixToXML(calibrationFilename, avgTransform);

            ((StereoCamera)scene.CameraNode.Camera).RightView = Matrix.Invert(avgTransform);

            CreateTestObjects();
        }

        private void CreateTestObjects()
        {
            groundMarkerNode = new TransformNode();
            scene.RootNode.AddChild(groundMarkerNode);

            CreateLights();

            Box box = new Box(24);

            // Create a box geometry
            {
                GeometryNode boxNode = new GeometryNode("Box1");
                boxNode.Model = box;
                Material boxMat = new Material();

                boxMat.Diffuse = Color.Red.ToVector4();
                boxMat.Specular = Color.White.ToVector4();
                boxMat.SpecularPower = 20;

                boxNode.Material = boxMat;

                TransformNode boxTrans = new TransformNode();
                boxTrans.Translation = new Vector3(0, 0, 12);

                groundMarkerNode.AddChild(boxTrans);
                boxTrans.AddChild(boxNode);
            }

            {
                GeometryNode boxNode = new GeometryNode("Box2");
                boxNode.Model = box;
                Material boxMat = new Material();

                boxMat.Diffuse = Color.Blue.ToVector4();
                boxMat.Specular = Color.White.ToVector4();
                boxMat.SpecularPower = 20;

                boxNode.Material = boxMat;

                TransformNode boxTrans = new TransformNode();
                boxTrans.Translation = new Vector3(-140, -72, 12);

                groundMarkerNode.AddChild(boxTrans);
                boxTrans.AddChild(boxNode);
            }

            {
                GeometryNode boxNode = new GeometryNode("Box3");
                boxNode.Model = box;
                Material boxMat = new Material();

                boxMat.Diffuse = Color.Green.ToVector4();
                boxMat.Specular = Color.White.ToVector4();
                boxMat.SpecularPower = 20;

                boxNode.Material = boxMat;

                TransformNode boxTrans = new TransformNode();
                boxTrans.Translation = new Vector3(140, -72, 12);

                groundMarkerNode.AddChild(boxTrans);
                boxTrans.AddChild(boxNode);
            }

            {
                GeometryNode boxNode = new GeometryNode("Box4");
                boxNode.Model = box;
                Material boxMat = new Material();

                boxMat.Diffuse = Color.Purple.ToVector4();
                boxMat.Specular = Color.White.ToVector4();
                boxMat.SpecularPower = 20;

                boxNode.Material = boxMat;

                TransformNode boxTrans = new TransformNode();
                boxTrans.Translation = new Vector3(-140, 72, 12);

                groundMarkerNode.AddChild(boxTrans);
                boxTrans.AddChild(boxNode);
            }

            {
                GeometryNode boxNode = new GeometryNode("Box5");
                boxNode.Model = box;
                Material boxMat = new Material();

                boxMat.Diffuse = Color.Yellow.ToVector4();
                boxMat.Specular = Color.White.ToVector4();
                boxMat.SpecularPower = 20;

                boxNode.Material = boxMat;

                TransformNode boxTrans = new TransformNode();
                boxTrans.Translation = new Vector3(140, 72, 12);

                groundMarkerNode.AddChild(boxTrans);
                boxTrans.AddChild(boxNode);
            }
        }

        private void SaveAdjustments()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);

            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

            XmlElement xmlRootNode = xmlDoc.CreateElement("CameraAdjustments");
            xmlDoc.AppendChild(xmlRootNode);

            {
                XmlElement xmlDataNode = xmlDoc.CreateElement("LeftWidthAdjustment");
                xmlDataNode.InnerText = camWidthAdjustmentLeft.ToString();
                xmlRootNode.AppendChild(xmlDataNode);
            }

            {
                XmlElement xmlDataNode = xmlDoc.CreateElement("LeftHorizontalShiftAdjustment");
                xmlDataNode.InnerText = camHShiftAdjustmentLeft.ToString();
                xmlRootNode.AppendChild(xmlDataNode);
            }

            {
                XmlElement xmlDataNode = xmlDoc.CreateElement("LeftVerticalShiftAdjustment");
                xmlDataNode.InnerText = camVShiftAdjustmentLeft.ToString();
                xmlRootNode.AppendChild(xmlDataNode);
            }

            {
                XmlElement xmlDataNode = xmlDoc.CreateElement("RightWidthAdjustment");
                xmlDataNode.InnerText = camWidthAdjustmentRight.ToString();
                xmlRootNode.AppendChild(xmlDataNode);
            }

            {
                XmlElement xmlDataNode = xmlDoc.CreateElement("RightHorizontalShiftAdjustment");
                xmlDataNode.InnerText = camHShiftAdjustmentRight.ToString();
                xmlRootNode.AppendChild(xmlDataNode);
            }

            {
                XmlElement xmlDataNode = xmlDoc.CreateElement("RightVerticalShiftAdjustment");
                xmlDataNode.InnerText = camVShiftAdjustmentRight.ToString();
                xmlRootNode.AppendChild(xmlDataNode);
            }

            try
            {
                xmlDoc.Save(adjustmentsFilename);
                Notifier.AddMessage("Saved adjustments");
            }
            catch (Exception)
            {
                throw new GoblinException("Failed to save the adjustments: " + adjustmentsFilename);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (calibrationThread != null && calibrationThread.IsAlive)
                calibrationThread.Abort();

            scene.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (finalized)
            {
                leftCaptureDevice.GetImageTexture(null, ref leftImagePtr);

                markerTracker.DetectorID = 0;
                markerTracker.CameraID = 0;
                markerTracker.ProcessImage(leftCaptureDevice, leftImagePtr);

                bool markerFoundOnLeftVideo = markerTracker.FindMarker(markerID);

                if (markerFoundOnLeftVideo)
                {
                    groundMarkerNode.WorldTransformation = markerTracker.GetMarkerTransform();
                }
            }

            scene.SceneRenderTarget = stereoScreenLeft;
            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);

            scene.SceneRenderTarget = stereoScreenRight;
            scene.RenderScene();

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(stereoScreenLeft, leftRect, leftSource, Color.White);
            spriteBatch.Draw(stereoScreenRight, rightRect, rightSource, Color.White);
            spriteBatch.End();
        }
    }
}