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
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.UI.UI2D;

namespace Tutorial4___3D_Object_Selection
{
    /// <summary>
    /// This tutorial demonstrates how to select 3D objects using the wrapped Newton physics library.
    /// </summary>
    public class Tutorial4 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Scene scene;
        SpriteFont textFont;

        String label = "Nothing is selected";

        public Tutorial4()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            // We will use the Newton physics engine (http://www.newtondynamics.com)
            // for processing the intersection of cast rays with
            // 3D objects to detect object selection. 
            scene.PhysicsEngine = new NewtonPhysics();

            // Set up the lights used in the scene
            CreateLights();

            // Set up the camera which defines the eye location and viewing frustum
            CreateCamera();

            // Create 3D objects
            CreateObjects();

            // Add a mouse click callback function to perform ray picking when mouse is clicked
            MouseInput.Instance.MouseClickEvent += new HandleMouseClick(MouseClickHandler);
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
            // Create a camera
            Camera camera = new Camera();
            // Put the camera at (0, 0, 10)
            camera.Translation = new Vector3(0, 4, 10);
            // Rotate the camera -20 degrees about the X axis
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
            // Create a geometry node with a model of box
            GeometryNode boxNode = new GeometryNode("Box");
            boxNode.Model = new Box(Vector3.One * 3);

            Material boxMat = new Material();
            boxMat.Diffuse = Color.Red.ToVector4();
            boxMat.Specular = Color.White.ToVector4();
            boxMat.SpecularPower = 5;

            boxNode.Material = boxMat;

            TransformNode boxTransNode = new TransformNode();
            boxTransNode.Translation = new Vector3(-5, 0, -6);

            // Define the most suitable shape type for this model
            // which is Box in this case so that the physics engine
            // will understand how to take care of the collision
            boxNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            // Set this box model to be pickable
            boxNode.Physics.Pickable = true;
            // Add this box model to the physics engine
            boxNode.AddToPhysicsEngine = true;

            scene.RootNode.AddChild(boxTransNode);
            boxTransNode.AddChild(boxNode);

            // Create more geometry nodes with different model shapes
            // Note that all geometry nodes are pickable by default
            // (although the pickability of an individual node can be disabled by the user)

            GeometryNode sphereNode = new GeometryNode("Sphere");
            sphereNode.Model = new Sphere(2, 20, 20);

            Material sphereMat = new Material();
            sphereMat.Diffuse = Color.Blue.ToVector4();
            sphereMat.Specular = Color.White.ToVector4();
            sphereMat.SpecularPower = 10;

            sphereNode.Material = sphereMat;

            TransformNode sphereTransNode = new TransformNode();
            sphereTransNode.Translation = new Vector3(0, 0, -6);

            sphereNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Sphere;
            sphereNode.Physics.Pickable = true;
            sphereNode.AddToPhysicsEngine = true;

            scene.RootNode.AddChild(sphereTransNode);
            sphereTransNode.AddChild(sphereNode);

            GeometryNode cylinderNode = new GeometryNode("Cylinder");
            cylinderNode.Model = new Cylinder(1.5f, 1.5f, 4f, 20);
            
            Material cylinderMat = new Material();
            cylinderMat.Diffuse = Color.Green.ToVector4();
            cylinderMat.Specular = Color.White.ToVector4();
            cylinderMat.SpecularPower = 5;

            cylinderNode.Material = cylinderMat;

            TransformNode cylinderTransNode = new TransformNode();
            cylinderTransNode.Translation = new Vector3(5, 0, -6);

            cylinderNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Cylinder;
            cylinderNode.Physics.Pickable = true;
            cylinderNode.AddToPhysicsEngine = true;

            scene.RootNode.AddChild(cylinderTransNode);
            cylinderTransNode.AddChild(cylinderNode);

            GeometryNode torusNode = new GeometryNode("Torus");
            torusNode.Model = new Torus(0.7f, 1.5f, 30, 30);

            Material torusMat = new Material();
            torusMat.Diffuse = Color.Yellow.ToVector4();
            torusMat.Specular = Color.White.ToVector4();
            torusMat.SpecularPower = 10;

            torusNode.Material = torusMat;

            TransformNode torusTransNode = new TransformNode();
            torusTransNode.Translation = new Vector3(-2, 0, 3);

            // Since GoblinXNA does not have a predefined shape type
            // of torus, we define the shape to be ConvexHull,
            // which is applicable to any shape, but more computationally
            // expensive compared to the other primitive shape types we have
            // used thus far
            torusNode.Physics.Shape = GoblinXNA.Physics.ShapeType.ConvexHull;
            torusNode.Physics.Pickable = true;
            torusNode.AddToPhysicsEngine = true;

            scene.RootNode.AddChild(torusTransNode);
            torusTransNode.AddChild(torusNode);

            GeometryNode coneNode = new GeometryNode("Cone");
            // Note that cone is a special case of a cylinder with top
            // radius of 0
            coneNode.Model = new Cylinder(1.5f, 0, 4f, 20);

            Material coneMat = new Material();
            coneMat.Diffuse = Color.Cyan.ToVector4();
            coneMat.Specular = Color.White.ToVector4();
            coneMat.SpecularPower = 5;

            coneNode.Material = coneMat;

            TransformNode coneTransNode = new TransformNode();
            coneTransNode.Translation = new Vector3(2, 0, 1);

            coneNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Cone;
            coneNode.Physics.Pickable = true;
            coneNode.AddToPhysicsEngine = true;

            scene.RootNode.AddChild(coneTransNode);
            coneTransNode.AddChild(coneNode);
        }

        private void MouseClickHandler(int button, Point mouseLocation)
        {
            // Only perform the ray picking if the clicked button is LeftButton
            if (button == MouseInput.LeftButton)
            {
                // In order to perform ray  picking, first we need to define a ray by projecting
                // the 2D mouse location to two 3D points: one on the near clipping plane and one on
                // the far clipping plane.  The vector between these two points defines the finite-length
                // 3D ray that we wish to intersect with objects in the scene.

                // 0 means on the near clipping plane, and 1 means on the far clipping plane
                Vector3 nearSource = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
                Vector3 farSource = new Vector3(mouseLocation.X, mouseLocation.Y, 1);

                // Now convert the near and far source to actual near and far 3D points based on our eye location
                // and view frustum
                Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource, 
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);
                Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource,
                    State.ProjectionMatrix, State.ViewMatrix, Matrix.Identity);

                // Have the physics engine intersect the pick ray defined by the nearPoint and farPoint with
                // the physics objects in the scene (which we have set up to approximate the model geometry).
                List<PickedObject> pickedObjects = ((NewtonPhysics)scene.PhysicsEngine).PickRayCast(
                    nearPoint, farPoint);

                // If one or more objects intersect with our ray vector
                if (pickedObjects.Count > 0)
                {
                    // Since PickedObject can be compared (which means it implements IComparable), we can sort it in 
                    // the order of closest intersected object to farthest intersected object
                    pickedObjects.Sort();

                    // We only care about the closest picked object for now, so we'll simply display the name 
                    // of the closest picked object whose container is a geometry node
                    label = ((GeometryNode)pickedObjects[0].PickedPhysicsObject.Container).Name + " is picked";

                    // NOTE: for a shape defined as ConvexHull (e.g.,the torus shape), even if you click the
                    // hole in the torus, it will think that it is picked. This is because a ConvexHull shape
                    // does not have holes, and the physics engine we use does not support shape with holes.
                    // However, it is possible to refine this behavior by performing your own ray intersection algorithm
                    // on this picked object. It's a good idea to perform your ray intersection after the
                    // physics engine returns you a picked object, since the physics engine's algorithm is well
                    // optimized. Then, you can work your way from the front of the pickedObjects list to the
                    // back, performing your own ray intersection with each object in sequence,
                    // until you find an object that it intersects.
                    // If you want to implement your own picking algorithm, we suggest that you see the 
                    // "Picking with Triangle-Accuracy Sample" at http://creators.xna.com/Education/Samples.aspx
                }
                else
                    label = "Nothing is selected";
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

        protected override void Dispose(bool disposing)
        {
            scene.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
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
            // Draw a 2D text string at the center of the screen
            UI2DRenderer.WriteText(Vector2.Zero, label, Color.Black,
                textFont, GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top);

            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);
        }
    }
}
