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
using GoblinXNA.Graphics.Geometry;
#if WINDOWS_PHONE
using GoblinXNA.Graphics.ParticleEffects2D;
#else
using GoblinXNA.Graphics.ParticleEffects;
#endif
using Model = GoblinXNA.Graphics.Model;

namespace Tutorial6___Simple_Particle_Systems
{
    /// <summary>
    /// This tutorial demonstrates how to use our easily-extendable particle systems.
    /// </summary>
    public class Tutorial6 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

#if WINDOWS_PHONE
        SpriteBatch spriteBatch;
#endif

        Scene scene;
        TransformNode shipTransParentNode;

        Random random = new Random();

        double shipAngle = 0;

        public Tutorial6()
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

#if WINDOWS_PHONE
            spriteBatch = new SpriteBatch(GraphicsDevice);
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

            // Show Frames-Per-Second on the screen for debugging
            State.ShowFPS = true;
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
            // Set up the camera of the scene graph
            Camera camera = new Camera();
            // Put the camera at (0, 0, 0)
            camera.Translation = new Vector3(0, 50, 120);
            // Rotate the camera -45 degrees along the X axis
            camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                MathHelper.ToRadians(-30));
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

        private void CreateObject()
        {
            // Loads a textured model of a ship
            ModelLoader loader = new ModelLoader();
            Model shipModel = (Model)loader.Load("", "p1_wedge");

            // Create a geometry node of a loaded ship model
            GeometryNode shipNode = new GeometryNode("Ship");
            shipNode.Model = shipModel;
            ((Model)shipNode.Model).UseInternalMaterials = true;

            // Create a transform node to define the transformation for the ship
            TransformNode shipTransNode = new TransformNode();
            shipTransNode.Translation = new Vector3(0, 0, -50);
            shipTransNode.Scale = new Vector3(0.01f, 0.01f, 0.01f); // It's huge!
            shipTransNode.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0),
                MathHelper.ToRadians(90));

            shipTransParentNode = new TransformNode();
            shipTransParentNode.Translation = Vector3.Zero;

            // Create a geometry node with model of a torus
            GeometryNode torusNode = new GeometryNode("Torus");
            torusNode.Model = new Torus(12f, 15.5f, 20, 20);

            TransformNode torusTransNode = new TransformNode();
            torusTransNode.Translation = new Vector3(-50, 0, 0);
            torusTransNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                MathHelper.ToRadians(90));

            // Create a material node for this torus model
            Material torusMaterial = new Material();
            torusMaterial.Diffuse = Color.DarkGoldenrod.ToVector4();
            torusMaterial.Specular = Color.White.ToVector4();
            torusMaterial.SpecularPower = 5;

            torusNode.Material = torusMaterial;

            // Now add the above nodes to the scene graph in appropriate order
            scene.RootNode.AddChild(shipTransParentNode);
            shipTransParentNode.AddChild(shipTransNode);
            shipTransNode.AddChild(shipNode);

            scene.RootNode.AddChild(torusTransNode);
            torusTransNode.AddChild(torusNode);

            // Now create couple of particle effects to attach to the models

            // Create a smoke particle effect and fire particle effect to simulate a
            // ring of fire around the torus model
#if WINDOWS_PHONE
            SmokePlumeParticleEffect smokeParticles = new SmokePlumeParticleEffect(20, spriteBatch);
            FireParticleEffect fireParticles = new FireParticleEffect(40, spriteBatch);
#else
            SmokePlumeParticleEffect smokeParticles = new SmokePlumeParticleEffect();
            FireParticleEffect fireParticles = new FireParticleEffect();
            // The order defines which particle effect to render first. Since we want
            // to show the fire particles in front of the smoke particles, we make
            // the smoke particles to be rendered first, and then fire particles
            smokeParticles.DrawOrder = 200;
            fireParticles.DrawOrder = 300;
#endif

            // Create a particle node to hold these two particle effects
            ParticleNode fireRingEffectNode = new ParticleNode();
            fireRingEffectNode.ParticleEffects.Add(smokeParticles);
            fireRingEffectNode.ParticleEffects.Add(fireParticles);

            // Implement an update handler for each of the particle effects which will be called
            // every "Update" call 
            fireRingEffectNode.UpdateHandler += new ParticleUpdateHandler(UpdateRingOfFire);

            torusNode.AddChild(fireRingEffectNode);

            // Create another set of fire and smoke particle effects to simulate the fire
            // the ship catches when the ship passes the ring of fire
#if WINDOWS_PHONE
            FireParticleEffect shipFireEffect = new FireParticleEffect(150, spriteBatch);
            SmokePlumeParticleEffect shipSmokeEffect = new SmokePlumeParticleEffect(80, spriteBatch);
            shipFireEffect.MinScale *= 1.5f;
            shipFireEffect.MaxScale *= 1.5f;
#else
            FireParticleEffect shipFireEffect = new FireParticleEffect();
            SmokePlumeParticleEffect shipSmokeEffect = new SmokePlumeParticleEffect();
            shipSmokeEffect.DrawOrder = 400;
            shipFireEffect.DrawOrder = 500;
#endif

            ParticleNode shipFireNode = new ParticleNode();
            shipFireNode.ParticleEffects.Add(shipFireEffect);
            shipFireNode.ParticleEffects.Add(shipSmokeEffect);

            shipFireNode.UpdateHandler += new ParticleUpdateHandler(UpdateShipFire);

            shipNode.AddChild(shipFireNode);
        }

        /// <summary>
        /// Update the fire effect on the ship model when the ship goes through the torus model
        /// </summary>
        /// <param name="worldTransform"></param>
        /// <param name="particleEffects"></param>
        private void UpdateShipFire(Matrix worldTransform, List<ParticleEffect> particleEffects)
        {
            double diff = (shipAngle % MathHelper.ToRadians(360)) - MathHelper.ToRadians(70);
            // Add fire particles only at certain rotation angle
            if (diff > 0 && diff < MathHelper.ToRadians(180))
            {
                foreach (ParticleEffect particle in particleEffects)
                {
                    // Add different number of fire particles based on the position of the ship
                    // model. We want to add more particles when it goes through the torus model
                    // and decrease the number after that
                    int numParticles = 0;
#if WINDOWS_PHONE
                    int maxFireParticles = 2;
#else
                    int maxFireParticles = 8;
#endif
                    if (particle is FireParticleEffect)
                        numParticles = maxFireParticles - (int)(diff * 2);
                    else
                        numParticles = 1;

#if WINDOWS_PHONE
                    for (int i = 0; i < (numParticles + 2) / 3; i++)
                        particle.AddParticles(Project(worldTransform.Translation + 
                            worldTransform.Forward * 1000));
#else
                    for(int i = 0; i < numParticles; i++)
                        particle.AddParticle(worldTransform.Translation + worldTransform.Forward * 1000, 
                            Vector3.Zero);
#endif
                }
            }
        }

        /// <summary>
        /// Update the fire effect on the torus model
        /// </summary>
        /// <param name="worldTransform"></param>
        /// <param name="particleEffects"></param>
        private void UpdateRingOfFire(Matrix worldTransform, List<ParticleEffect> particleEffects)
        {
            foreach (ParticleEffect particle in particleEffects)
            {
                if (particle is FireParticleEffect)
                {
#if WINDOWS_PHONE
                    for(int k = 0; k < 1; k++)
                        particle.AddParticles(Project(RandomPointOnCircle(worldTransform.Translation)));
#else
                    // Add 10 fire particles every frame
                    for (int k = 0; k < 10; k++)
                        particle.AddParticle(RandomPointOnCircle(worldTransform.Translation), Vector3.Zero);
#endif
                }
                else
                {
#if WINDOWS_PHONE
                    particle.AddParticles(Project(RandomPointOnCircle(worldTransform.Translation)));
#else
                    // Add 1 smoke particle every frame
                    particle.AddParticle(RandomPointOnCircle(worldTransform.Translation), Vector3.Zero);
#endif
                }
            }
        }

        /// <summary>
        /// Get a random point on a circle
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private Vector3 RandomPointOnCircle(Vector3 pos)
        {
            const float radius = 12.5f;

            double angle = random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius + pos.X, y * radius + pos.Y, pos.Z);
        }

#if WINDOWS_PHONE
        /// <summary>
        /// Projects object space coordinate to screen coordinate
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector2 Project(Vector3 position)
        {
            Vector3 pos2d = State.Device.Viewport.Project(position, State.ProjectionMatrix,
                State.ViewMatrix, Matrix.Identity);
            return new Vector2(pos2d.X, pos2d.Y);
        }
#endif

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

            shipAngle += gameTime.ElapsedGameTime.TotalSeconds;
            // Rotate the ship model about the origin along Z axis
            shipTransParentNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY,
                (float)shipAngle);

            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
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
