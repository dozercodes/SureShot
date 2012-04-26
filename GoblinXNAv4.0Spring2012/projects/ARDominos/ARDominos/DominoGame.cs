/************************************************************************************ 
 * Copyright (c) 2008-2009, Columbia University
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

//#define USE_ARTAG

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.Sounds;
using GoblinXNA.Helpers;
using GoblinXNA.Shaders;

using GoblinXNA.UI.UI2D;

using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;

namespace ARDominos
{
    /// <summary>
    /// The main domino game. 
    /// </summary>
    public class DominoGame : Microsoft.Xna.Framework.Game
    {
        #region Member Fields
        GraphicsDeviceManager graphics;

        // A GoblinXNA scene graph
        static Scene scene;

        // A marker node for tracking the ground plane (game board)
        MarkerNode markerNode;

        // The geometric model that represents the domino
        PrimitiveModel dominoModel;

        // A list of currently selected dominos
        static List<GeometryNode> selectedDominos;

        // A list of dominos in the scene (either on or off the game board)
        static List<GeometryNode> dominos;

        // A list of shootable balls
        static List<GeometryNode> balls;

        // A list of larger and heavier shootable balls
        static List<GeometryNode> heavyBalls;

        // A list of dominos that fall off the game board
        static List<GeometryNode> fallenDominos;

        // Current game state including the modes, play time, etc
        static GameState gameState;

        // Whether to display the GUI for mode selection
        bool showGUI = false;

        // The UI manager which draws the 2D GUIs
        static UIManager uiManager;

        // Used for keeping track of which keys are currently held down
        bool leftKeyPressed = false;
        bool rightKeyPressed = false;

        // A list of points that are used to connect the draw line in the
        // Add.LineDrawing mode
        List<VertexPositionColor> pointList;

        // A list of indices that connect the draw points
        // (only used in AdditionMode.LineDrawing mode)
        List<short> lineListIndices;

        // A basic effect used to draw the line
        // (only used in AdditionMode.LineDrawing mode)
        BasicEffect basicEffect;

        // The gap between each dominos when placing the dominos on the drawn line
        // (only used in AdditionMode.LineDrawing mode)
        float dominoGap = 7.0f;

        // Whether currently in the multiple selection mode
        // (only used in GameMode.Edit mode)
        bool multiSelection = true;

        // Used for multiple domino selection
        Point anchorPoint = new Point();
        Point dragPoint = new Point();

        // The current color to use for a ball to be shot (gets modified every shot)
        Vector4 ballColor = new Vector4(1, 0, 0, 1);

        // Three color states used for changing ball colors
        int rState = 0, gState = 1, bState = 0; // 0 = stay, 1 = up, 2 = down

        // The size of the domino
        Vector3 dominoSize = new Vector3(2.5f, 16, 8); // (3/8)x2x1 ratio

        // The distance of the player/viewer to the game board when a ball is shot
        // Used for determining the initial speed of the ball
        float ballPressDistance;

        // The start time of currently played sounds. Used to limit the number of 
        // sounds played at a time in order to keep reasonable FPS on less-powerful machines
        List<double> soundsPlaying;

        // The maximum number of sounds allowed to be played at the same time
        // Used to keep reasonable FPS on less-powerful machines
        const int SOUND_LIMIT = 15;

        // The maximum number of dominos allowed to be added to the game board
        // Used to keep reasonable FPS on less-powerful machines
        const int DOMINO_LIMIT = 40;
        int curBall = 0;
        int curHeavyBall = 0;

        // A texture that has Columbia Computer Graphics and User Interface Lab's logo
        Texture2D cguiLogoTexture;

        // Whether the victory sound effect is played
        static bool victorySoundPlayed = false;

        // Number of shooting balls to create
        const int BALL_NUM = 40;
        static int ballCount = 0;
        float volume = 1;
        const float MAX_VOLUME = 5;

        // Whether to always shoot from the center of the screen
        bool shootCenterMode = false;

        // Sound effects
        SoundEffect hammerWood1Sound;
        SoundEffect rubberBall01Sound;
        SoundEffect victorySound;
        SoundEffect woodHitConcrete1Sound;
        SoundEffect woodHitWood3Sound;
        #endregion

        #region Constructor
        public DominoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
        }
        #endregion

        #region Override Methods
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

            //State.ThreadOption = (ushort)ThreadOptions.MarkerTracking;

            // Initialize the scene graph
            scene = new Scene();

            gameState = new GameState();
            uiManager = new UIManager(gameState);

            dominos = new List<GeometryNode>();
            selectedDominos = new List<GeometryNode>();
            balls = new List<GeometryNode>();
            heavyBalls = new List<GeometryNode>();
            fallenDominos = new List<GeometryNode>();

            pointList = new List<VertexPositionColor>();
            lineListIndices = new List<short>();

            soundsPlaying = new List<double>();

            // Setup for physics simulation
            SetupPhysics();

            // Setup marker tracking using ALVAR tracking library
            SetupMarkerTracking();

            // Enable the shadow mapping
            scene.ShadowMap = new MultiLightShadowMap();

            // Set up the lights used in the scene
            CreateLights();

            // Create 3D objects
            CreateObject();

            // Initialize the UI manager
            uiManager.Initialize(scene, GameModeSwitched, ExtraModeSwitched);
            uiManager.LoadContent();

            // Show Frames-Per-Second on the screen for debugging
            State.ShowFPS = true;

            MouseInput.Instance.MousePressEvent += new HandleMousePress(MousePressHandler);
            MouseInput.Instance.MouseDragEvent += new HandleMouseDrag(MouseDragHandler);
            MouseInput.Instance.MouseReleaseEvent += new HandleMouseRelease(MouseReleaseHandler);
            MouseInput.Instance.MouseMoveEvent += new HandleMouseMove(MouseMoveHandler);

            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(KeyPressHandler);
            KeyboardInput.Instance.KeyReleaseEvent += new HandleKeyRelease(KeyReleaseHandler);

            // Create a basic effect to draw the line for AdditionMode.LineDrawing
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.DiffuseColor = new Vector3(1.0f, 0.0f, 0.0f);
        }

        protected override void LoadContent()
        {
            cguiLogoTexture = Content.Load<Texture2D>("cguiLogo");

            hammerWood1Sound = Content.Load<SoundEffect>("hammer_wood_1");
            rubberBall01Sound = Content.Load<SoundEffect>("rubber_ball_01");
            victorySound = Content.Load<SoundEffect>("win");
            woodHitConcrete1Sound = Content.Load<SoundEffect>("wood_hit_concrete_1");
            woodHitWood3Sound = Content.Load<SoundEffect>("wood_hit_wood_3");
        }

        protected override void UnloadContent()
        {
            Content.Unload();

            base.UnloadContent();
        }

        protected override void Dispose(bool disposing)
        {
            scene.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            Vector3 trans, scale;
            Quaternion rot;
            Matrix view = markerNode.WorldTransformation * State.ViewMatrix;
            view.Decompose(out scale, out rot, out trans);
            Vector3 cameraPos = Vector3.Zero;
            Vector3 upV = Vector3.Zero;
            Vector3 forwardV = Vector3.Zero;

            // If not in the play mode
            if (gameState.CurrentGameMode != GameState.GameMode.Play)
            {
                // Rotate the selected dominos if 'leftKey' or 'rightKey' is pressed
                if (selectedDominos.Count != 0)
                {
                    float angle = 0;
                    if (leftKeyPressed)
                        angle = MathHelper.ToRadians(5);
                    else if (rightKeyPressed)
                        angle = MathHelper.ToRadians(-5);

                    if (angle != 0)
                    {
                        foreach (GeometryNode domino in selectedDominos)
                        {
                            TransformNode tNode = (TransformNode)domino.Parent;

                            tNode.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);
                        }
                    }
                }
            }

            List<double> removes = new List<double>();
            // Remove the sound count if it's been more than 1200 milliseconds since the
            // sound is played
            foreach (double playTime in soundsPlaying)
            {
                double now = DateTime.Now.TimeOfDay.TotalMilliseconds;
                if ((now - playTime) >= 1200)
                    removes.Add(playTime);
            }

            foreach (double playTime in removes)
                soundsPlaying.Remove(playTime);

            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Remove balls fall off from the ground if they are certain distance away from the ground
            foreach (GeometryNode ballNode in balls)
                if (ballNode.Physics.PhysicsWorldTransform.Translation.Z < -100 ||
                    ballNode.Physics.PhysicsWorldTransform.Translation.X < -180 ||
                    ballNode.Physics.PhysicsWorldTransform.Translation.X > 180 ||
                    ballNode.Physics.PhysicsWorldTransform.Translation.Y < -180 ||
                    ballNode.Physics.PhysicsWorldTransform.Translation.Y > 180)
                {
                    ballNode.Material.Diffuse -= Vector4.UnitW;
                }
            foreach (GeometryNode ballNode in heavyBalls)
                if (ballNode.Physics.PhysicsWorldTransform.Translation.Z < -100 ||
                    ballNode.Physics.PhysicsWorldTransform.Translation.X < -180 ||
                    ballNode.Physics.PhysicsWorldTransform.Translation.X > 180 ||
                    ballNode.Physics.PhysicsWorldTransform.Translation.Y < -180 ||
                    ballNode.Physics.PhysicsWorldTransform.Translation.Y > 180)
                {
                    ballNode.Material.Diffuse -= Vector4.UnitW;
                }

            // Add dominos that fall off the edge of the ground to the fallen domino list
            foreach (GeometryNode dominoNode in dominos)
                if (!fallenDominos.Contains(dominoNode) &&
                    (dominoNode.Physics.PhysicsWorldTransform.Translation.Z < -100 ||
                     dominoNode.Physics.PhysicsWorldTransform.Translation.Z > 240 ||
                     dominoNode.Physics.PhysicsWorldTransform.Translation.X < -180 ||
                     dominoNode.Physics.PhysicsWorldTransform.Translation.X > 180 ||
                     dominoNode.Physics.PhysicsWorldTransform.Translation.Y < -180 ||
                     dominoNode.Physics.PhysicsWorldTransform.Translation.Y > 180))
                {
                    dominoNode.Material.Diffuse -= Vector4.UnitW;
                    fallenDominos.Add(dominoNode);
                }

            // If all of the dominos fall off the edge of the ground, then the game is over/won
            if (fallenDominos.Count == dominos.Count && dominos.Count > 0)
                gameState.GameOver = true;

            // Shows the victory texts as well as play the victory sound if won
            if (gameState.GameOver)
            {
                if (!victorySoundPlayed)
                {
                    //Sound.SetVolume("Default", 5 * volume);
                    Sound.Instance.PlaySoundEffect(victorySound);
                    victorySoundPlayed = true;
                }
            }

            // Draw the line in the AdditionMode.LineDrawing mode if there are more than 2 points
            // added on the ground
            if (pointList.Count >= 2)
            {
                Matrix viewMatrix = markerNode.WorldTransformation * State.ViewMatrix;
                basicEffect.View = viewMatrix;
                basicEffect.Projection = State.ProjectionMatrix;
                basicEffect.World = Matrix.Identity;

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        pointList.ToArray(),
                        0,
                        pointList.Count,
                        lineListIndices.ToArray(),
                        0,
                        pointList.Count - 1);
                }
            }

            // Renders GUI
            uiManager.Draw(gameTime);

            // Processes and renders the scene graph
            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
        #endregion

        #region Input Handlers
        private void MouseMoveHandler(Point mouseLocation)
        {
            // If either in Addition or Edit mode, make the cross hair cursor follow the mouse
            if (gameState.CurrentGameMode != GameState.GameMode.Play)
                uiManager.CrossHairPoint = mouseLocation;
            else
            {
                // If shootCenterMode is not true, then make the cross hair follow the mouse
                // Otherwise, the cross hair cursor always stays at the center of the screen
                if(!shootCenterMode)
                    uiManager.CrossHairPoint = mouseLocation;
            }
        }

        private void MouseReleaseHandler(int button, Point mouseLocation)
        {
            // If the GUI is shown, and the mouse is within the bound of the GUI, then
            // don't perform the mouse press event for the scene
            if (uiManager.Frame.Visible && UI2DHelper.IsWithin(mouseLocation, uiManager.Frame.Bounds))
                return;

            if (gameState.CurrentGameMode == GameState.GameMode.Add)
            {
                if (gameState.CurrentAdditionMode == GameState.AdditionMode.Single)
                {
                    // Nothing to do for single mode
                }
                else
                {
                    if (button == MouseInput.LeftButton)
                    {
                        // Places the dominos on the drawn line with certain fixed gaps as long as
                        // it doesn't go beyond the domino count limit
                        for (int i = 0; i < pointList.Count && (dominos.Count <= DOMINO_LIMIT); i++)
                        {
                            GeometryNode dominoNode = new GeometryNode("Domino " + dominos.Count);
                            dominoNode.Model = dominoModel;

                            dominoNode.Physics.Mass = 20;
                            dominoNode.Physics.Shape = ShapeType.Box;
                            dominoNode.Physics.Pickable = true;
                            dominoNode.Physics.MaterialName = "Domino";
                            dominoNode.Physics.Pickable = true;
                            dominoNode.AddToPhysicsEngine = true;

                            Material dominoMaterial = new Material();
                            dominoMaterial.Diffuse = new Vector4(1, 1, 1, 0.5f);
                            dominoMaterial.Specular = Color.White.ToVector4();
                            dominoMaterial.SpecularPower = 10;
                            dominoMaterial.Texture = cguiLogoTexture;

                            dominoNode.Material = dominoMaterial;

                            TransformNode dominoTransNode = new TransformNode();
                            dominoTransNode.Translation = pointList[i].Position + 
                                (Vector3.UnitZ * dominoSize.Y / 2);

                            int start = 0, end = 0;
                            // Get the point that is 2 points behind the current point (if any)
                            if ((i - 2) <= 0)
                                start = 0;
                            else
                                start = i - 2;

                            // Get the point that is 2 points ahead of the current point (if any)
                            if ((i + 2) >= pointList.Count)
                                end = pointList.Count - 1;
                            else
                                end = i + 2;

                            // Calculate the orientation of the domino based on the tangent line of 
                            // the current point (which is based on end pos - start pos)
                            Vector3 u = -Vector3.UnitY;
                            Vector3 v = pointList[end].Position - pointList[start].Position;
                            u.Normalize();
                            v.Normalize();
                            float angle = (float)Math.Acos(Vector3.Dot(u, v));

                            if (v.X < 0)
                                angle = -angle;

                            dominoTransNode.Rotation = Quaternion.CreateFromAxisAngle(
                                Vector3.UnitZ, angle - MathHelper.PiOver2);

                            markerNode.AddChild(dominoTransNode);

                            dominoTransNode.AddChild(dominoNode);

                            dominos.Add(dominoNode);
                            selectedDominos.Add(dominoNode);

                            // Moves the point position to the next one that has at least the gap
                            // of 'dominoGap' from the current position
                            Vector3 curPoint = pointList[i].Position;
                            while (i < pointList.Count)
                            {
                                if (Vector3.Distance(curPoint, pointList[i++].Position) >= dominoGap)
                                    break;
                            }
                        }

                        // Clears the line points
                        pointList.Clear();
                        lineListIndices.Clear();
                    }
                }
            }
            else if (gameState.CurrentGameMode == GameState.GameMode.Edit)
            {
                if (gameState.CurrentEditMode == GameState.EditMode.Single)
                {
                    // Nothing to do for single mode
                }
                else
                {
                    /*if (multiSelection)
                    {
                        Point upLeft = new Point(Math.Min(anchorPoint.X, dragPoint.X),
                            Math.Min(anchorPoint.Y, dragPoint.Y));
                        Point downRight = new Point(Math.Max(anchorPoint.X, dragPoint.X),
                            Math.Max(anchorPoint.Y, dragPoint.Y));

                        float dist = scene.CameraNode.Camera.ZNearPlane;
                        float height = 2 * (dist / (float)Math.Tan(scene.CameraNode.Camera.FieldOfViewY / 2));
                        float width = height * scene.CameraNode.Camera.AspectRatio;

                        float left = upLeft.X / (float)State.Width * width - width / 2;
                        float right = downRight.X / (float)State.Width * width - width / 2;
                        float bottom = upLeft.Y / (float)State.Height * height - height / 2;
                        float top = downRight.Y / (float)State.Height * height - height / 2;

                        Matrix proj = Matrix.CreatePerspectiveOffCenter(left, right, bottom, top,
                            scene.CameraNode.Camera.ZNearPlane, scene.CameraNode.Camera.ZFarPlane);

                        BoundingFrustum frustum = new BoundingFrustum(State.ViewMatrix *
                            proj);

                        foreach (GeometryNode domino in dominos)
                        {
                            if (frustum.Intersects(domino.Model.MinimumBoundingBox))
                            {
                                selectedDominos.Add(domino);

                                Vector4 mod = ((MaterialNode)domino.Parent).Material.Diffuse;
                                ((MaterialNode)domino.Parent).Material.Diffuse =
                                    new Vector4(mod.X, mod.Y, mod.Z, 0.5f);

                                scene.PhysicsEngine.RemovePhysicsObject(domino.Physics);
                            }
                        }

                        anchorPoint = Point.Zero;
                        dragPoint = Point.Zero;
                        multiSelection = false;
                    }
                    else
                    {

                        selectedDominos.Clear();
                        multiSelection = true;
                    }*/
                }
            }
            else
            {
                // If game is not over, and the released button is either left button (for normal balls)
                // or right button (for heavier balls)
                if (button != MouseInput.MiddleButton && !gameState.GameOver)
                {
                    bool heavy = (button == MouseInput.RightButton);

                    float additionalSpeed = 0;

                    // Add extra speed to the ball being shot if the camera point is moved from 
                    // far point to closer point (to the ground)
                    float ballReleaseDistance = markerNode.WorldTransformation.Translation.Length();

                    if (ballReleaseDistance < ballPressDistance)
                        additionalSpeed = ballPressDistance - ballReleaseDistance;

                    Vector3 nearSource = new Vector3(uiManager.CrossHairPoint.X, uiManager.CrossHairPoint.Y, 0);
                    Vector3 farSource = new Vector3(uiManager.CrossHairPoint.X, uiManager.CrossHairPoint.Y, 1);

                    Matrix viewMatrix = markerNode.WorldTransformation * State.ViewMatrix;

                    Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                        State.ProjectionMatrix, viewMatrix, Matrix.Identity);
                    Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                        State.ProjectionMatrix, viewMatrix, Matrix.Identity);

                    ballCount++;
                    uiManager.BallCount++;
                    // Create a ball and shoot it
                    CreateBallCharactor(nearPoint, farPoint, additionalSpeed, heavy);
                }
            }
        }

        private void MouseDragHandler(int button, Point startLocation, Point currentLocation)
        {
            // If the GUI is shown, and the mouse is within the bound of the GUI, then
            // don't perform the mouse press event for the scene
            if (uiManager.Frame.Visible && UI2DHelper.IsWithin(currentLocation, uiManager.Frame.Bounds))
                return;

            if (gameState.CurrentGameMode == GameState.GameMode.Add)
            {
                if (gameState.CurrentAdditionMode == GameState.AdditionMode.Single)
                {
                    if (button == MouseInput.LeftButton)
                    {
                        if (dominos.Count < DOMINO_LIMIT)
                        {
                            Vector3 nearSource = new Vector3(currentLocation.X, currentLocation.Y, 0);
                            Vector3 farSource = new Vector3(currentLocation.X, currentLocation.Y, 1);

                            Matrix viewMatrix = markerNode.WorldTransformation * State.ViewMatrix;

                            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                                State.ProjectionMatrix, viewMatrix, Matrix.Identity);
                            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                                State.ProjectionMatrix, viewMatrix, Matrix.Identity);

                            // Casts a ray from the eye to the scene to pick a point on the ground in which to add
                            // the domino
                            List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

                            for (int i = 0; i < pickedObjects.Count; i++)
                            {
                                if (((GeometryNode)pickedObjects[i].PickedPhysicsObject.Container).
                                    Name.Equals("Ground"))
                                {
                                    // Calculate the intersection point of the ray and the ground
                                    Vector3 intersectPoint = nearPoint * (1 - pickedObjects[i].IntersectParam) +
                                        pickedObjects[i].IntersectParam * farPoint;

                                    ((TransformNode)dominos[dominos.Count - 1].Parent).Translation =
                                        intersectPoint + Vector3.UnitZ * dominoSize.Y / 2;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (button == MouseInput.LeftButton)
                    {
                        // Add a point for the line
                        AddPoint(currentLocation);
                    }
                }
            }
            else if (gameState.CurrentGameMode == GameState.GameMode.Edit)
            {
                if (gameState.CurrentEditMode == GameState.EditMode.Single)
                {
                    if (button == MouseInput.LeftButton)
                    {
                        // If there is no selected dominos, then don't do anything
                        if (selectedDominos.Count == 0)
                            return;

                        // 0 means on the near clipping plane, and 1 means on the far clipping plane
                        Vector3 nearSource = new Vector3(currentLocation.X, currentLocation.Y, 0);
                        Vector3 farSource = new Vector3(currentLocation.X, currentLocation.Y, 1);

                        Matrix viewMatrix = markerNode.WorldTransformation * State.ViewMatrix;

                        Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                            State.ProjectionMatrix, viewMatrix, Matrix.Identity);
                        Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                            State.ProjectionMatrix, viewMatrix, Matrix.Identity);

                        List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

                        for (int i = 0; i < pickedObjects.Count; i++)
                        {
                            if (((GeometryNode)pickedObjects[i].PickedPhysicsObject.Container).
                                Name.Equals("Ground"))
                            {
                                if (selectedDominos.Count == 1)
                                {
                                    GeometryNode domino = selectedDominos[0];
                                    Vector3 intersectPoint = nearPoint * (1 - pickedObjects[i].IntersectParam) +
                                        pickedObjects[i].IntersectParam * farPoint;

                                    ((TransformNode)domino.Parent).Translation
                                        = intersectPoint + Vector3.UnitZ * dominoSize.Y / 2;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (multiSelection)
                    {
                        dragPoint = currentLocation;
                    }
                    else
                    {
                    }
                }
            }
            else
            {
            }
        }

        private void MousePressHandler(int button, Point mouseLocation)
        {
            // If the GUI is shown, and the mouse is within the bound of the GUI, then
            // don't perform the mouse press event for the scene
            if(!(gameState.CurrentGameMode == GameState.GameMode.Edit && 
                gameState.CurrentEditMode == GameState.EditMode.Multiple) && uiManager.Frame.Visible &&
                UI2DHelper.IsWithin(mouseLocation, uiManager.Frame.Bounds))
                return;

            if (gameState.CurrentGameMode == GameState.GameMode.Add)
            {
                if (gameState.CurrentAdditionMode == GameState.AdditionMode.Single)
                {
                    // Add a domino on the ground
                    if ((button == MouseInput.LeftButton) && (dominos.Count <= DOMINO_LIMIT))
                    {
                        foreach (GeometryNode domino in selectedDominos)
                        {
                            Vector4 orig = domino.Material.Diffuse;
                            domino.Material.Diffuse = new Vector4(orig.X, orig.Y, orig.Z, 1.0f);
                        }

                        selectedDominos.Clear();

                        GeometryNode dominoNode = new GeometryNode("Domino " + dominos.Count);
                        dominoNode.Model = dominoModel;

                        dominoNode.Physics.Mass = 20;
                        dominoNode.Physics.Shape = ShapeType.Box;
                        dominoNode.Physics.MaterialName = "Domino";
                        dominoNode.Physics.Pickable = true;
                        dominoNode.AddToPhysicsEngine = true;

                        Material dominoMaterial = new Material();
                        dominoMaterial.Diffuse = new Vector4(1, 1, 1, 0.5f);
                        dominoMaterial.Specular = Color.White.ToVector4();
                        dominoMaterial.SpecularPower = 10;
                        dominoMaterial.Texture = cguiLogoTexture;

                        dominoNode.Material = dominoMaterial;

                        TransformNode dominoTransNode = new TransformNode();

                        Vector3 nearSource = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
                        Vector3 farSource = new Vector3(mouseLocation.X, mouseLocation.Y, 1);

                        Matrix viewMatrix = markerNode.WorldTransformation * State.ViewMatrix;


                        Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                            State.ProjectionMatrix, viewMatrix, Matrix.Identity);
                        Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                            State.ProjectionMatrix, viewMatrix, Matrix.Identity);

                        // Cast a ray to the scene to pick a point on the ground where the domino
                        // will be added
                        List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

                        for (int i = 0; i < pickedObjects.Count; i++)
                        {
                            if (((GeometryNode)pickedObjects[i].PickedPhysicsObject.Container).
                                Name.Equals("Ground"))
                            {
                                Vector3 intersectPoint = nearPoint * (1 - pickedObjects[i].IntersectParam) +
                                    pickedObjects[i].IntersectParam * farPoint;

                                dominoTransNode.Translation = intersectPoint + Vector3.UnitZ * dominoSize.Y / 2;
                                break;
                            }
                        }

                        markerNode.AddChild(dominoTransNode);

                        dominoTransNode.AddChild(dominoNode);

                        dominos.Add(dominoNode);

                        selectedDominos.Add(dominoNode);
                    }
                }
                else
                {
                    if (button == MouseInput.LeftButton)
                    {
                        foreach (GeometryNode domino in selectedDominos)
                        {
                            Vector4 orig = domino.Material.Diffuse;
                            domino.Material.Diffuse = new Vector4(orig.X, orig.Y, orig.Z, 1.0f);
                        }

                        selectedDominos.Clear();

                        AddPoint(mouseLocation);
                    }
                }
            }
            else if (gameState.CurrentGameMode == GameState.GameMode.Edit)
            {
                if (button == MouseInput.LeftButton)
                {
                    if (gameState.CurrentEditMode == GameState.EditMode.Single)
                    {
                        foreach (GeometryNode domino in selectedDominos)
                        {
                            Vector4 orig = domino.Material.Diffuse;
                            domino.Material.Diffuse = new Vector4(orig.X, orig.Y, orig.Z, 1.0f);
                        }

                        selectedDominos.Clear();

                        Vector3 nearSource = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
                        Vector3 farSource = new Vector3(mouseLocation.X, mouseLocation.Y, 1);

                        Matrix viewMatrix = markerNode.WorldTransformation * State.ViewMatrix;

                        Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                            State.ProjectionMatrix, viewMatrix, Matrix.Identity);
                        Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                            State.ProjectionMatrix, viewMatrix, Matrix.Identity);

                        // Cast a ray to the scene to select the closest object (to the eye location) that hits the ray
                        List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

                        // If one or more objects intersect with our ray vector
                        if (pickedObjects.Count > 0)
                        {
                            // Sort the picked objects in ascending order of intersection parameter
                            // (the closest to the eye is the first one in the sorted list)
                            pickedObjects.Sort();

                            // If the closest object is the ground, then don't do anything
                            if (((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).
                                Name.Equals("Ground"))
                                return;

                            selectedDominos.Add((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container);
                            
                            GeometryNode domino = selectedDominos[0];
                            Vector4 mod = domino.Material.Diffuse;
                            domino.Material.Diffuse = new Vector4(mod.X, mod.Y, mod.Z, 0.5f);

                            foreach (PickedObject obj in pickedObjects)
                            {
                                if (((GeometryNode)obj.PickedPhysicsObject.Container).Name.
                                    Equals("Ground"))
                                {
                                    Vector3 intersectPoint = nearPoint * (1 - obj.IntersectParam) +
                                        obj.IntersectParam * farPoint;

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (multiSelection)
                        {
                            anchorPoint = mouseLocation;
                            dragPoint = mouseLocation;
                        }
                        else
                        {
                        }
                    }
                }
            }
            else
            {
                ballPressDistance = markerNode.WorldTransformation.Translation.Length();
            }
        }

        /// <summary>
        /// Adds a point on the ground for the line drawing in AdditionMode.LineDrawing mode
        /// </summary>
        /// <param name="mouseLocation"></param>
        private void AddPoint(Point mouseLocation)
        {
            Vector3 nearSource = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
            Vector3 farSource = new Vector3(mouseLocation.X, mouseLocation.Y, 1);

            Matrix viewMatrix = markerNode.WorldTransformation * State.ViewMatrix;

            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, viewMatrix, Matrix.Identity);
            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, viewMatrix, Matrix.Identity);

            List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(nearPoint, farPoint);

            Vector3 intersectPoint = Vector3.Zero;
            for (int i = 0; i < pickedObjects.Count; i++)
            {
                if (((GeometryNode)pickedObjects[i].PickedPhysicsObject.Container).
                    Name.Equals("Ground"))
                {
                    intersectPoint = nearPoint * (1 - pickedObjects[i].IntersectParam) +
                        pickedObjects[i].IntersectParam * farPoint;

                    break;
                }
            }

            if (!intersectPoint.Equals(Vector3.Zero))
            {
                pointList.Add(new VertexPositionColor(intersectPoint + Vector3.UnitZ * 0.1f, Color.Red));
                lineListIndices.Add((short)lineListIndices.Count);
            }
        }

        private void KeyReleaseHandler(Keys key, KeyModifier modifier)
        {
            if (gameState.CurrentGameMode != GameState.GameMode.Play)
            {
                if (key == Keys.Right)
                    rightKeyPressed = false;
                if (key == Keys.Left)
                    leftKeyPressed = false;
            }
        }

        private void KeyPressHandler(Keys key, KeyModifier modifier)
        {
            if (key == Keys.Escape)
                this.Exit();

            if (key == Keys.S)
            {
                scene.EnableShadowMapping = !scene.EnableShadowMapping;
            }
            if (key == Keys.C)
            {
                if (gameState.CurrentGameMode == GameState.GameMode.Play)
                {
                    shootCenterMode = !shootCenterMode;
                    if (shootCenterMode)
                        uiManager.CrossHairPoint = new Point(State.Width / 2, State.Height / 2);
                }
            }

            if (key == Keys.G)
            {
                showGUI = !showGUI;
                if (showGUI)
                {
                    uiManager.Frame.Visible = true;
                    uiManager.Frame.Enabled = true;
                    uiManager.ModeChoiceEnabled = false;
                }
                else
                {
                    uiManager.Frame.Visible = false;
                    uiManager.Frame.Enabled = false;
                }
            }
            else if (key == Keys.P)
                uiManager.GamePlay.DoClick();
            else if (key == Keys.A)
                uiManager.GameAdd.DoClick();
            else if (key == Keys.E)
                uiManager.GameEdit.DoClick();
            else if (key == Keys.R)
                ResetGame();
            else if (key == Keys.H)
                uiManager.EnableHelpMenu = !uiManager.EnableHelpMenu;

            if (gameState.CurrentGameMode == GameState.GameMode.Add)
            {
                if (key == Keys.Right)
                    rightKeyPressed = true;
                if (key == Keys.Left)
                    leftKeyPressed = true;
            }
            else if (gameState.CurrentGameMode == GameState.GameMode.Edit)
            {
                if (key == Keys.Right)
                    rightKeyPressed = true;
                if (key == Keys.Left)
                    leftKeyPressed = true;

                if (key == Keys.D || key == Keys.Delete)
                {
                    foreach (GeometryNode domino in selectedDominos)
                    {
                        dominos.Remove(domino);
                        ((TransformNode)domino.Parent).RemoveChild(domino);
                    }

                    selectedDominos.Clear();
                }
            }
            else
            {
                if (key == Keys.Up || key == Keys.Right)
                    if (volume < MAX_VOLUME)
                        volume += 0.5f;
                if (key == Keys.Down || key == Keys.Left)
                    if (volume > 1)
                        volume -= 0.5f;
            }
        }
        #endregion
 
        #region Scene Creation
        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(-1, -1, -1);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.CastShadows = true;
            lightNode.LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 500);
            lightNode.AmbientLightColor = new Vector4(0.3f, 0.3f, 0.3f, 1);
            lightNode.LightSource = lightSource;

            // Add this light node to the root node
            scene.RootNode.AddChild(lightNode);
        }

        private void SetupMarkerTracking()
        {
            DirectShowCapture captureDevice = new DirectShowCapture();
            captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.R8G8B8_24, false);

            scene.AddVideoCaptureDevice(captureDevice);

            ALVARMarkerTracker tracker = new ALVARMarkerTracker();
            tracker.MaxMarkerError = 0.02f;
            tracker.InitTracker(captureDevice.Width, captureDevice.Height, "calib.xml", 9.0);

            scene.MarkerTracker = tracker;

            scene.ShowCameraImage = true;

            scene.PhysicsEngine.GravityDirection = -Vector3.UnitZ;

            // Create a marker node to track the ground plane
            markerNode = new MarkerNode(scene.MarkerTracker, "ARDominoALVAR.txt");

            scene.RootNode.AddChild(markerNode);
        }

        private void CreateObject()
        {
            CreateGround();

            CreateDominos();

            // Pre-loads the balls to the scene before shooting in order to keep the game play
            // smooth during the play mode for less-powerful machines
            CreateBalls();
        }

        private void SetupPhysics()
        {
            scene.PhysicsEngine = new NewtonPhysics();
            // Make the physics simulation space larger to 500x500 centered at the origin
            ((NewtonPhysics)scene.PhysicsEngine).WorldSize = new BoundingBox(Vector3.One * -250,
                Vector3.One * 250);
            // Increase the gravity
            scene.PhysicsEngine.Gravity = 60.0f;

            ((NewtonPhysics)scene.PhysicsEngine).MaxSimulationSubSteps = 5;

            // Creates several physics material to associate appropriate collision sounds for each
            // different materials
            NewtonMaterial physMat = new NewtonMaterial();
            // Domino to domino material interaction
            physMat.MaterialName1 = "Domino";
            physMat.MaterialName2 = "Domino";
            // Defines the callback function when the two materials contact/collide
            physMat.ContactProcessCallback = delegate(Vector3 contactPosition, Vector3 contactNormal,
                float contactSpeed, float colObj1ContactTangentSpeed, float colObj2ContactTangentSpeed,
                Vector3 colObj1ContactTangentDirection, Vector3 colObj2ContactTangentDirection)
            {
                // Only play sound if the collision/contact speed is above 4
                if (contactSpeed > 4f)
                {
                    // If we're already playing more than the limited number of sounds, then
                    // don't play
                    if (soundsPlaying.Count >= SOUND_LIMIT)
                        return;

                    /*try
                    {
                        // Set the volume proportional to the collision/contact speed
                        Sound.SetVolume("Default", contactSpeed / 4 * volume);
                    }
                    catch (Exception exp) { }*/
                    try
                    {
                        Sound.Instance.PlaySoundEffect(woodHitWood3Sound);
                        soundsPlaying.Add(DateTime.Now.TimeOfDay.TotalMilliseconds);
                    }
                    catch (Exception exp) { }
                }

            };

            NewtonMaterial physMat2 = new NewtonMaterial();
            // Gound to ball material interaction
            physMat2.MaterialName1 = "Ground";
            physMat2.MaterialName2 = "Ball";
            physMat2.Elasticity = 0.7f;
            physMat2.ContactProcessCallback = delegate(Vector3 contactPosition, Vector3 contactNormal,
                float contactSpeed, float colObj1ContactTangentSpeed, float colObj2ContactTangentSpeed,
                Vector3 colObj1ContactTangentDirection, Vector3 colObj2ContactTangentDirection)
            {
                if (contactSpeed > 3f)
                {
                    if (soundsPlaying.Count >= SOUND_LIMIT)
                        return;

                    /*try
                    {
                        Sound.SetVolume("Default", contactSpeed * volume);
                    }
                    catch (Exception exp) { }*/
                    try
                    {
                        Sound.Instance.PlaySoundEffect(rubberBall01Sound);
                        soundsPlaying.Add(DateTime.Now.TimeOfDay.TotalMilliseconds);
                    }
                    catch (Exception exp) { }
                }

            };

            NewtonMaterial physMat3 = new NewtonMaterial();
            physMat3.MaterialName1 = "Ground";
            physMat3.MaterialName2 = "Domino";
            physMat3.ContactProcessCallback = delegate(Vector3 contactPosition, Vector3 contactNormal,
                float contactSpeed, float colObj1ContactTangentSpeed, float colObj2ContactTangentSpeed,
                Vector3 colObj1ContactTangentDirection, Vector3 colObj2ContactTangentDirection)
            {
                if (contactSpeed > 4f)
                {
                    if (soundsPlaying.Count >= SOUND_LIMIT)
                        return;

                    /*try
                    {
                        Sound.SetVolume("Default", contactSpeed / 2 * volume);
                    }
                    catch (Exception exp) { }*/
                    try
                    {
                        Sound.Instance.PlaySoundEffect(woodHitConcrete1Sound);
                        soundsPlaying.Add(DateTime.Now.TimeOfDay.TotalMilliseconds);
                    }
                    catch (Exception exp) { }
                }

            };

            NewtonMaterial physMat4 = new NewtonMaterial();
            physMat4.MaterialName1 = "Ball";
            physMat4.MaterialName2 = "Domino";
            physMat4.Elasticity = 0.5f;
            physMat4.ContactProcessCallback = delegate(Vector3 contactPosition, Vector3 contactNormal,
                float contactSpeed, float colObj1ContactTangentSpeed, float colObj2ContactTangentSpeed,
                Vector3 colObj1ContactTangentDirection, Vector3 colObj2ContactTangentDirection)
            {
                if (contactSpeed > 4f)
                {
                    if (soundsPlaying.Count >= SOUND_LIMIT)
                        return;

                    /*try
                    {
                        Sound.SetVolume("Default", contactSpeed * volume);
                    }
                    catch (Exception exp) { }*/
                    try
                    {
                        Sound.Instance.PlaySoundEffect(hammerWood1Sound);
                        soundsPlaying.Add(DateTime.Now.TimeOfDay.TotalMilliseconds);
                    }
                    catch (Exception exp) { }
                }

            };

            NewtonMaterial physMat5 = new NewtonMaterial();
            physMat5.MaterialName1 = "Ball";
            physMat5.MaterialName2 = "Ball";
            physMat5.ContactProcessCallback = delegate(Vector3 contactPosition, Vector3 contactNormal,
                float contactSpeed, float colObj1ContactTangentSpeed, float colObj2ContactTangentSpeed,
                Vector3 colObj1ContactTangentDirection, Vector3 colObj2ContactTangentDirection)
            {
                if (contactSpeed > 4f)
                {
                    if (soundsPlaying.Count >= SOUND_LIMIT)
                        return;

                    /*try
                    {
                        Sound.SetVolume("Default", contactSpeed * volume);
                    }
                    catch (Exception exp) { }*/
                    try
                    {
                        Sound.Instance.PlaySoundEffect(rubberBall01Sound);
                        soundsPlaying.Add(DateTime.Now.TimeOfDay.TotalMilliseconds);
                    }
                    catch (Exception exp) { }
                }

            };

            NewtonMaterial physMat6 = new NewtonMaterial();
            physMat6.MaterialName1 = "Ball";
            physMat6.MaterialName2 = "Obstacle";
            physMat6.Elasticity = 0.7f;
            physMat6.ContactProcessCallback = delegate(Vector3 contactPosition, Vector3 contactNormal,
                float contactSpeed, float colObj1ContactTangentSpeed, float colObj2ContactTangentSpeed,
                Vector3 colObj1ContactTangentDirection, Vector3 colObj2ContactTangentDirection)
            {
                if (contactSpeed > 4f)
                {
                    if (soundsPlaying.Count >= SOUND_LIMIT)
                        return;

                    /*try
                    {
                        Sound.SetVolume("Default", contactSpeed * volume);
                    }
                    catch (Exception exp) { }*/
                    try
                    {
                        Sound.Instance.PlaySoundEffect(rubberBall01Sound);
                        soundsPlaying.Add(DateTime.Now.TimeOfDay.TotalMilliseconds);
                    }
                    catch (Exception exp) { }
                }

            };

            ((NewtonPhysics)scene.PhysicsEngine).AddPhysicsMaterial(physMat);
            ((NewtonPhysics)scene.PhysicsEngine).AddPhysicsMaterial(physMat2);
            ((NewtonPhysics)scene.PhysicsEngine).AddPhysicsMaterial(physMat3);
            ((NewtonPhysics)scene.PhysicsEngine).AddPhysicsMaterial(physMat4);
            ((NewtonPhysics)scene.PhysicsEngine).AddPhysicsMaterial(physMat5);
            ((NewtonPhysics)scene.PhysicsEngine).AddPhysicsMaterial(physMat6);
        }

        private void CreateGround()
        {
            GeometryNode groundNode = new GeometryNode("Ground");
            groundNode.Model = new TexturedBox(129.5f, 99, 0.2f);

            groundNode.Physics.Collidable = true;
            groundNode.Physics.Shape = ShapeType.Box;
            groundNode.Physics.MaterialName = "Ground";
            groundNode.Physics.Pickable = true;
            groundNode.AddToPhysicsEngine = true;
            groundNode.IsOccluder = true;

            groundNode.Model.ShadowAttribute = ShadowAttribute.ReceiveOnly;
            groundNode.Model.Shader = new SimpleShadowShader(scene.ShadowMap);

            Material groundMaterial = new Material();
            groundMaterial.Diffuse = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
            groundMaterial.Specular = Color.White.ToVector4();
            groundMaterial.SpecularPower = 20;

            groundNode.Material = groundMaterial;

            markerNode.AddChild(groundNode);
        }

        private void CreateDominos()
        {
            dominoModel = new DominoBox(new Vector3(dominoSize.X, dominoSize.Z, dominoSize.Y),
                new Vector2(0.663f, 0.707f));

            dominoModel.ShadowAttribute = ShadowAttribute.ReceiveCast;
            dominoModel.Shader = new SimpleShadowShader(scene.ShadowMap);

            float radius = 18;
            for (int x = 0; x < 360; x += 30)
            {
                GeometryNode dominoNode = new GeometryNode("Domino " + dominos.Count);
                dominoNode.Model = dominoModel;

                dominoNode.Physics.Mass = 20;
                dominoNode.Physics.Shape = ShapeType.Box;
                dominoNode.Physics.MaterialName = "Domino";
                dominoNode.Physics.Pickable = true;
                dominoNode.AddToPhysicsEngine = true;

                Material dominoMaterial = new Material();
                dominoMaterial.Diffuse = Color.White.ToVector4();
                dominoMaterial.Specular = Color.White.ToVector4();
                dominoMaterial.SpecularPower = 10;
                dominoMaterial.Texture = cguiLogoTexture;

                dominoNode.Material = dominoMaterial;

                TransformNode dominoTransNode = new TransformNode();
                dominoTransNode.Translation = new Vector3(
                    (float)(radius * Math.Cos(MathHelper.ToRadians(x))),
                    radius * (float)(Math.Sin(MathHelper.ToRadians(x))), dominoSize.Y / 2);
                dominoTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                    MathHelper.ToRadians(x + 90));

                markerNode.AddChild(dominoTransNode);

                dominoTransNode.AddChild(dominoNode);

                dominos.Add(dominoNode);
            }
        }

        /// <summary>
        /// Resets the game to the initial state with unmodified game board (all of the added, removed
        /// or editted dominos are reset to the initial domino layout)
        /// </summary>
        private void ResetGame()
        {
            foreach (GeometryNode domino in dominos)
                markerNode.RemoveChild(domino.Parent);
            dominos.Clear();
            CreateDominos();
            uiManager.GameAdd.DoClick();
        }

        /// <summary>
        /// A helper function to get the next color from a list of distinctive colors to 
        /// assign to the balls we shoot
        /// </summary>
        /// <returns></returns>
        private Vector4 GetNextColor()
        {
            Vector4 nextColor = ballColor;
            if (rState == 1)
            {
                nextColor.X += 0.25f;
                if (nextColor.X >= 1.0f)
                {
                    rState = 0;
                    bState = 2;
                }
            }
            else if (rState == 2)
            {
                nextColor.X -= 0.25f;
                if (nextColor.X <= 0)
                {
                    rState = 0;
                    bState = 1;
                }
            }
            else if (gState == 1)
            {
                nextColor.Y += 0.25f;
                if (nextColor.Y >= 1.0f)
                {
                    gState = 0;
                    rState = 2;
                }
            }
            else if (gState == 2)
            {
                nextColor.Y -= 0.25f;
                if (nextColor.Y <= 0)
                {
                    gState = 0;
                    rState = 1;
                }
            }
            else if (bState == 1)
            {
                nextColor.Z += 0.25f;
                if (nextColor.Z >= 1.0f)
                {
                    bState = 0;
                    gState = 2;
                }
            }
            else if (bState == 2)
            {
                nextColor.Z -= 0.25f;
                if (nextColor.Z <= 0)
                {
                    bState = 0;
                    gState = 1;
                }
            }

            ballColor = nextColor;
            return ballColor;
        }

        /// <summary>
        /// Create both normal and heavy balls for shooting
        /// </summary>
        private void CreateBalls()
        {
            PrimitiveModel smallSphere = new TexturedSphere(3.5f, 20, 20);
            PrimitiveModel bigSphere = new TexturedSphere(6.5f, 20, 20);

            smallSphere.Shader = new SimpleShadowShader(scene.ShadowMap);
            bigSphere.Shader = new SimpleShadowShader(scene.ShadowMap);

            for (int i = 0; i < BALL_NUM; i++)
            {
                if (i == BALL_NUM / 2)
                {
                    ballColor = new Vector4(1, 0, 0, 1);
                    rState = 0;
                    gState = 1;
                    bState = 0;
                }
                GeometryNode ballNode = new GeometryNode();
                ballNode.Name = "Ball " + ballNode.ID;
                if (i < BALL_NUM / 2)
                    ballNode.Model = smallSphere;
                else
                    ballNode.Model = bigSphere;

                ballNode.Model.ShadowAttribute = ShadowAttribute.None;
                ballNode.Physics.Collidable = true;
                ballNode.Physics.Interactable = true;
                ballNode.AddToPhysicsEngine = true;

                Material ballMaterial = new Material();
                ballMaterial.Diffuse = GetNextColor();
                ballMaterial.Specular = Color.White.ToVector4();
                ballMaterial.SpecularPower = 30;

                ballNode.Material = ballMaterial;

                if (i < BALL_NUM / 2)
                    ballNode.Physics.Mass = 150f;
                else
                    ballNode.Physics.Mass = 250f;
                ballNode.Physics.Shape = ShapeType.Sphere;
                ballNode.Physics.MaterialName = "Ball";

                TransformNode ballTransNode = new TransformNode();
                ballTransNode.Translation = new Vector3(1000, 1000, 1000);

                markerNode.AddChild(ballTransNode);

                ballTransNode.AddChild(ballNode);

                if (i < BALL_NUM / 2)
                    balls.Add(ballNode);
                else
                    heavyBalls.Add(ballNode);
            }
        }

        /// <summary>
        /// Pick a ball from the list of balls already loaded at the start of the program
        /// and shoots it
        /// </summary>
        /// <param name="near"></param>
        /// <param name="far"></param>
        /// <param name="additionalSpeed"></param>
        /// <param name="heavy"></param>
        private void CreateBallCharactor(Vector3 near, Vector3 far, float additionalSpeed,
            bool heavy)
        {
            if (!markerNode.MarkerFound)
                return;

            Vector3 linVel = far - near;
            linVel.Normalize();

            GeometryNode ballNode = (heavy) ? heavyBalls[curHeavyBall] : balls[curBall];
            ballNode.Model.ShadowAttribute = ShadowAttribute.ReceiveCast;
            Vector4 orig = ballNode.Material.Diffuse;
            ballNode.Material.Diffuse = new Vector4(orig.X, orig.Y, orig.Z, 1);
            Vector3 v = near + linVel * 10;

            // Forces the physics engine to 'transport' this ball to the location 'v' in the simulation world
            ((NewtonPhysics)scene.PhysicsEngine).SetTransform(ballNode.Physics, Matrix.CreateTranslation(v));
            // Apply a linear velocity to this ball
            ((NewtonPhysics)scene.PhysicsEngine).ApplyLinearVelocity(ballNode.Physics, linVel * (100f + additionalSpeed));
            if (heavy)
            {
                curHeavyBall++;
                // Circles through if we run out of heavy balls
                if (curHeavyBall >= BALL_NUM / 2)
                    curHeavyBall = 0;
            }
            else
            {
                curBall++;
                // Circles through if we run out of normal balls
                if (curBall >= BALL_NUM / 2)
                    curBall = 0;
            }
        }
        #endregion

        #region Action Listeners
        /// <summary>
        /// An action event handler for extra mode switch
        /// </summary>
        /// <param name="source"></param>
        private void ExtraModeSwitched(object source)
        {
            if (gameState.CurrentGameMode == GameState.GameMode.Add)
            {
                if (((G2DRadioButton)source).Text.Equals("Single"))
                    gameState.CurrentAdditionMode = GameState.AdditionMode.Single;
                else
                    gameState.CurrentAdditionMode = GameState.AdditionMode.LineDrawing;
            }
            else if (gameState.CurrentGameMode == GameState.GameMode.Edit)
            {
                if (((G2DRadioButton)source).Text.Equals("Single"))
                    gameState.CurrentEditMode = GameState.EditMode.Single;
                else
                    gameState.CurrentEditMode = GameState.EditMode.Multiple;
            }
        }

        /// <summary>
        /// An action event handler for game mode switch
        /// </summary>
        private void GameModeSwitched(object source)
        {
            // Make all of the selected dominos that were transparent to opaque
            foreach (GeometryNode domino in selectedDominos)
            {
                Vector4 orig = domino.Material.Diffuse;
                domino.Material.Diffuse = new Vector4(orig.X, orig.Y, orig.Z, 1.0f);
            }

            selectedDominos.Clear();

            if (((G2DRadioButton)source).Text.Equals("Add"))
            {
                // Reset the game status to the initial state with the modified game board
                fallenDominos.Clear();

                // If the previuos mode was the Play mode
                if (gameState.CurrentGameMode == GameState.GameMode.Play)
                {
                    ballCount = 0;
                    uiManager.BallCount = 0;

                    // Enable certain radio buttons that were disabled during the play mode
                    uiManager.ModeChoiceEnabled = true;

                    // Make all of the dominos on the game board non-collidable and non-interactable
                    // as well as opaque
                    foreach (GeometryNode domino in dominos)
                    {
                        domino.Physics.Collidable = false;
                        domino.Physics.Interactable = false;

                        Vector4 orig = domino.Material.Diffuse;
                        domino.Material.Diffuse = new Vector4(orig.X, orig.Y, orig.Z, 1.0f);
                    }

                    // Restart the physical simulation
                    scene.PhysicsEngine.RestartsSimulation();
                    // Force garbage collection for unused references
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Move the normal balls out of the sight from the player, and make them
                    // not to cast or receive shadows
                    foreach (GeometryNode ball in balls)
                    {
                        ((NewtonPhysics)scene.PhysicsEngine).SetTransform(ball.Physics,
                            Matrix.CreateTranslation(Vector3.One * 1000));
                        ball.Model.ShadowAttribute = ShadowAttribute.None;
                    }
                    // Move the heavy balls out of the sight from the player, and make them
                    // not to cast or receive shadows
                    foreach (GeometryNode ball in heavyBalls)
                    {
                        ((NewtonPhysics)scene.PhysicsEngine).SetTransform(ball.Physics,
                            Matrix.CreateTranslation(Vector3.One * 1000));
                        ball.Model.ShadowAttribute = ShadowAttribute.None;
                    }
                }
                gameState.CurrentGameMode = GameState.GameMode.Add;

                uiManager.SwitchMode();
            }
            else if (((G2DRadioButton)source).Text.Equals("Edit"))
            {
                // Reset the game status to the initial state with the modified game board
                fallenDominos.Clear();

                // If the previous mode was the Play mode
                if (gameState.CurrentGameMode == GameState.GameMode.Play)
                {
                    ballCount = 0;
                    uiManager.BallCount = 0;

                    // Enable certain radio buttons that were disabled during the play mode
                    uiManager.ModeChoiceEnabled = true;

                    // Make all of the dominos on the game board non-collidable and non-interactable
                    // as well as opaque
                    foreach (GeometryNode domino in dominos)
                    {
                        domino.Physics.Collidable = false;
                        domino.Physics.Interactable = false;

                        Vector4 orig = domino.Material.Diffuse;
                        domino.Material.Diffuse = new Vector4(orig.X, orig.Y, orig.Z, 1.0f);
                    }

                    // Restart the physical simulation
                    scene.PhysicsEngine.RestartsSimulation();
                    // Force garbage collection for unused references
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Move the normal balls out of the sight from the player, and make them
                    // not to cast or receive shadows
                    foreach (GeometryNode ball in balls)
                    {
                        ((NewtonPhysics)scene.PhysicsEngine).SetTransform(ball.Physics,
                            Matrix.CreateTranslation(Vector3.One * 1000));
                        ball.Model.ShadowAttribute = ShadowAttribute.None;
                    }
                    // Move the heavy balls out of the sight from the player, and make them
                    // not to cast or receive shadows
                    foreach (GeometryNode ball in heavyBalls)
                    {
                        ((NewtonPhysics)scene.PhysicsEngine).SetTransform(ball.Physics,
                            Matrix.CreateTranslation(Vector3.One * 1000));
                        ball.Model.ShadowAttribute = ShadowAttribute.None;
                    }
                }
                gameState.CurrentGameMode = GameState.GameMode.Edit;

                uiManager.SwitchMode();
            }
            else
            { // Play mode
                victorySoundPlayed = false;

                // If previous mode was not Play mode
                if (gameState.CurrentGameMode != GameState.GameMode.Play)
                {
                    // Make all of the dominos collidable and interactable
                    foreach (GeometryNode domino in dominos)
                    {
                        domino.Physics.Collidable = true;
                        domino.Physics.Interactable = true;
                    }
                }
                // Restart the game
                else
                {
                    gameState.ResetState();
                    ballCount = 0;
                    uiManager.BallCount = 0;

                    fallenDominos.Clear();

                    // Make all of the dominos opaque in case they were totally transparent after
                    // they fall off the edge of the ground in the Play mode
                    foreach (GeometryNode domino in dominos)
                    {
                        Vector4 orig = domino.Material.Diffuse;
                        domino.Material.Diffuse = new Vector4(orig.X, orig.Y, orig.Z, 1.0f);
                    }

                    // Restart the physical simulation
                    scene.PhysicsEngine.RestartsSimulation();
                    // Force garbage collection for unused references
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Move the normal balls out of the sight from the player
                    foreach (GeometryNode ball in balls)
                    {
                        ((NewtonPhysics)scene.PhysicsEngine).SetTransform(ball.Physics,
                            Matrix.CreateTranslation(Vector3.One * 1000));
                    }
                    // Move the heavy balls out of the sight from the player
                    foreach (GeometryNode ball in heavyBalls)
                    {
                        ((NewtonPhysics)scene.PhysicsEngine).SetTransform(ball.Physics,
                            Matrix.CreateTranslation(Vector3.One * 1000));
                    }
                }
                gameState.CurrentGameMode = GameState.GameMode.Play;

                uiManager.SwitchMode();
            }
        }
        #endregion
    }
}
