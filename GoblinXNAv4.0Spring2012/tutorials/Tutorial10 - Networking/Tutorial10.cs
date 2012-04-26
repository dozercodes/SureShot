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

//#define USE_SOCKET_NETWORK

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GoblinXNA;
using GoblinXNA.SceneGraph;
using GoblinXNA.Helpers;
using GoblinXNA.Graphics;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.Device;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Network;
using GoblinXNA.Physics;
using GoblinXNA.UI;
#if WINDOWS
using GoblinXNA.Physics.Newton1;
#else
using GoblinXNA.Physics.Matali;
#endif

namespace Tutorial10___Networking
{
    /// <summary>
    /// This tutorial demonstrates how to use Goblin XNA's networking capabilities with
    /// server-client model. In order to run both server and client on the same machine
    /// you need to copy the generated .exe file to other folder, and set one of them
    /// to be the server, and the other to be the client (isServer = false). If you're
    /// running the server and client on different machines, then you can simply run the
    /// project (of course, you will need to set one of them to be client).
    /// </summary>
    public class Tutorial10 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        // A Goblin XNA scene graph
        Scene scene;

        // A material for the shooting boxes
        Material shootMat;
        Box boxModel;
        int shooterID = 0;

        // A network object which transmits mouse press information
        MouseNetworkObject mouseNetworkObj;

        // Indicates whether this is a server
        bool isServer;

        public Tutorial10()
            : this(false)
        {
        }

        public Tutorial10(bool isServer)
        {
            this.isServer = isServer;
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
            this.IsMouseVisible = true;
#endif

            // Initialize the GoblinXNA framework
            State.InitGoblin(graphics, Content, "");

            // Initialize the scene graph
            scene = new Scene();

            State.EnableNetworking = true;
            State.IsServer = isServer;

            State.ShowNotifications = true;
            State.ShowFPS = true;
            Notifier.FadeOutTime = 2000;

            // Set up the lights used in the scene
            CreateLights();

            // Set up the camera, which defines the eye location and viewing frustum
            CreateCamera();

            // Create 3D objects
            CreateObject();

            // Create a network object that contains mouse press information to be
            // transmitted over network
            mouseNetworkObj = new MouseNetworkObject();

            // When a mouse press event is sent from the other side, then call "ShootBox"
            // function
            mouseNetworkObj.CallbackFunc = ShootBox;

            // Create a network handler for handling the network transfers
            INetworkHandler networkHandler = null;

#if USE_SOCKET_NETWORK || WINDOWS_PHONE
            networkHandler = new SocketNetworkHandler();
#else
            networkHandler = new NetworkHandler();
#endif

            if (State.IsServer)
            {
                IServer server = null;
#if WINDOWS
#if USE_SOCKET_NETWORK
                server = new SocketServer(14242);
#else
                server = new LidgrenServer("Tutorial10", 14242);
#endif
                State.NumberOfClientsToWait = 1;
                scene.PhysicsEngine = new NewtonPhysics();
#else

                scene.PhysicsEngine = new MataliPhysics();
                ((MataliPhysics)scene.PhysicsEngine).SimulationTimeStep = 1 / 30f;
#endif
                scene.PhysicsEngine.Gravity = 30;
                server.ClientConnected += new HandleClientConnection(ClientConnected);
                server.ClientDisconnected += new HandleClientDisconnection(ClientDisconnected);
                networkHandler.NetworkServer = server;
            }
            else
            {
                IClient client = null;
                // Create a client that connects to the local machine assuming that both
                // the server and client will be running on the same machine. In order to 
                // connect to a remote machine, you need to either pass the host name or
                // the IP address of the remote machine in the 3rd parameter. 
#if WINDOWS
                client = new LidgrenClient("Tutorial10", 14242, "Localhost");
#else
                client = new SocketClient("10.0.0.2", 14242);
#endif

                // If the server is not running when client is started, then wait for the
                // server to start up.
                client.WaitForServer = true;
                client.ConnectionTrialTimeOut = 60 * 1000; // 1 minute timeout

                client.ServerConnected += new HandleServerConnection(ServerConnected);
                client.ServerDisconnected += new HandleServerDisconnection(ServerDisconnected);

                networkHandler.NetworkClient = client;
            }

            // Assign the network handler used for this scene
            scene.NetworkHandler = networkHandler;

            // Add the mouse network object to the scene graph, so it'll be sent over network
            // whenever ReadyToSend is set to true.
            scene.NetworkHandler.AddNetworkObject(mouseNetworkObj);

            MouseInput.Instance.MousePressEvent += new HandleMousePress(MouseInput_MousePressEvent);
        }

        private void ServerDisconnected()
        {
            Notifier.AddMessage("Disconnected from the server");
        }

        private void ServerConnected()
        {
            Notifier.AddMessage("Connected to the server");
        }

        private void ClientDisconnected(string clientIP, int portNumber)
        {
            Notifier.AddMessage("Disconnected from " + clientIP + " at port " + portNumber);
        }

        private void ClientConnected(string clientIP, int portNumber)
        {
            Notifier.AddMessage("Accepted connection from " + clientIP + " at port " + portNumber);
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(-1, -1, 0);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = Color.White.ToVector4();

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.LightSource = lightSource;

            // Add this light node to the root node
            scene.RootNode.AddChild(lightNode);
        }

        private void CreateCamera()
        {
            // Create a camera 
            Camera camera = new Camera();

            if (State.IsServer)
                camera.Translation = new Vector3(0, 0, 10);
            else
            {
                camera.Translation = new Vector3(0, 0, -30);
                camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.Pi);
            }
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
            SynchronizedGeometryNode sphereNode = new SynchronizedGeometryNode("Sphere");
            sphereNode.Model = new Sphere(3, 20, 20);
            sphereNode.Model.ShowBoundingBox = true;

            Material sphereMaterial = new Material();
            sphereMaterial.Diffuse = Color.Red.ToVector4();
            sphereMaterial.Ambient = Color.Blue.ToVector4();
            sphereMaterial.Emissive = Color.Green.ToVector4();

            sphereNode.Material = sphereMaterial;

            TransformNode transNode = new TransformNode();

            SynchronizedGeometryNode cylinderNode = new SynchronizedGeometryNode("Cylinder");
            cylinderNode.Model = new Cylinder(3, 3, 8, 20);
            cylinderNode.Model.ShowBoundingBox = true;

            Material cylinderMat = new Material();
            cylinderMat.Diffuse = Color.Cyan.ToVector4();
            cylinderMat.Specular = Color.Yellow.ToVector4();
            cylinderMat.SpecularPower = 5;

            cylinderNode.Material = cylinderMat;

            TransformNode parentTrans = new TransformNode();
            parentTrans.Translation = new Vector3(0, -2, -10);

            cylinderNode.Physics.Collidable = true;
            cylinderNode.Physics.Interactable = true;
            cylinderNode.AddToPhysicsEngine = true;
            cylinderNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Cylinder;
            cylinderNode.Physics.Mass = 200;

            sphereNode.Physics.Collidable = true;
            sphereNode.Physics.Interactable = true;
            sphereNode.AddToPhysicsEngine = true;
            sphereNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Sphere;
            sphereNode.Physics.Mass = 0;

            transNode.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), -MathHelper.PiOver2);
            transNode.Translation = new Vector3(0, 12, 0);

            scene.RootNode.AddChild(parentTrans);
            parentTrans.AddChild(sphereNode);
            parentTrans.AddChild(transNode);
            transNode.AddChild(cylinderNode);

            shootMat = new Material();
            shootMat.Diffuse = Color.Pink.ToVector4();
            shootMat.Specular = Color.Yellow.ToVector4();
            shootMat.SpecularPower = 10;

            boxModel = new Box(1);
        }

        private void MouseInput_MousePressEvent(int button, Point mouseLocation)
        {
            if (button == MouseInput.LeftButton)
            {
                Vector3 nearSource = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
                Vector3 farSource = new Vector3(mouseLocation.X, mouseLocation.Y, 1);

                Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                        State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
                Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

                // Shoot the box model
                ShootBox(nearPoint, farPoint);

                // Set ReadyToSend to true so that the scene graph will handle the transfer
                // NOTE: Once it is sent, ReadyToSend will be set to false automatically
                mouseNetworkObj.ReadyToSend = true;

                // Pass the necessary information to be sent
                mouseNetworkObj.PressedButton = button;
                mouseNetworkObj.NearPoint = nearPoint;
                mouseNetworkObj.FarPoint = farPoint;
            }
        }

        /// <summary>
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// Unload your graphics content.  If unloadAllContent is true, you should
        /// unload content from both ResourceManagementMode pools.  Otherwise, just
        /// unload ResourceManagementMode.Manual content.  Manual content will get
        /// Disposed by the GraphicsDevice during a Reset.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
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
        /// checking for collisions, gathering input and playing audio.
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
        }

        /// <summary>
        /// Shoot a box from the clicked mouse location
        /// </summary>
        /// <param name="near"></param>
        /// <param name="far"></param>
        private void ShootBox(Vector3 near, Vector3 far)
        {
            Vector3 camPos = scene.CameraNode.Camera.Translation;

            SynchronizedGeometryNode shootBox = new SynchronizedGeometryNode("ShooterBox" + shooterID++);
            shootBox.Model = boxModel;
            shootBox.Material = shootMat;
            shootBox.Physics.Interactable = true;
            shootBox.Physics.Collidable = true;
            shootBox.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            shootBox.Physics.Mass = 20f;
            shootBox.AddToPhysicsEngine = true;

            // Calculate the direction to shoot the box based on the near and far point
            Vector3 linVel = far - near;
            linVel.Normalize();
            // Multiply the direction with the velocity of 20
            linVel *= 60f;

            // Assign the initial velocity to this shooting box
            shootBox.Physics.InitialLinearVelocity = linVel;

            TransformNode shooterTrans = new TransformNode();
            shooterTrans.Translation = near;

            scene.RootNode.AddChild(shooterTrans);
            shooterTrans.AddChild(shootBox);
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
