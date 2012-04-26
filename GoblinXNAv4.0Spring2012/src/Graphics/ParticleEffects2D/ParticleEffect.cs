#region File Description
//-----------------------------------------------------------------------------
// SmokePlumeParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;
#endregion

namespace GoblinXNA.Graphics.ParticleEffects2D
{
    /// <summary>
    /// ParticleSystem is an abstract class that provides the basic functionality to
    /// create a particle effect. Different subclasses will have different effects,
    /// such as fire, explosions, and plumes of smoke. To use these subclasses, 
    /// simply call AddParticles, and pass in where the particles should exist
    /// </summary>
    public abstract class ParticleEffect : IDisposable, IComparable<ParticleEffect>
    {
        // these two values control the order that particle systems are drawn in.
        // typically, particles that use additive blending should be drawn on top of
        // particles that use regular alpha blending. ParticleSystems should therefore
        // set their DrawOrder to the appropriate value in InitializeConstants, though
        // it is possible to use other values for more advanced effects.
        public const int AlphaBlendDrawOrder = 100;
        public const int AdditiveDrawOrder = 200;

        // the texture this particle system will use.
        private Texture2D texture;

        // the origin when we're drawing textures. this will be the middle of the
        // texture.
        private Vector2 origin;

        private Vector2 drawPosition;
        private Vector3 tempVec3;
        private Vector3 tempVec3_2;

        // this number represents the maximum number of effects this particle system
        // will be expected to draw at one time. this is set in the constructor and is
        // used to calculate how many particles we will need.
        private int howManyEffects;
        
        // the array of particles used by this system. these are reused, so that calling
        // AddParticles will not cause any allocations.
        Particle[] particles;

        // the queue of free particles keeps track of particles that are not curently
        // being used by an effect. when a new effect is requested, particles are taken
        // from this queue. when particles are finished they are put onto this queue.
        Queue<Particle> freeParticles;
        /// <summary>
        /// returns the number of particles that are available for a new effect.
        /// </summary>
        public int FreeParticleCount
        {
            get { return freeParticles.Count; }
        }

        // This region of values control the "look" of the particle system, and should 
        // be set by deriving particle systems in the InitializeConstants method. The
        // values are then used by the virtual function InitializeParticle. Subclasses
        // can override InitializeParticle for further
        // customization.
        #region constants to be set by subclasses

        protected int drawOrder;
        public int DrawOrder
        {
            get { return drawOrder; }
            set { drawOrder = value; }
        }

        protected bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// minNumParticles and maxNumParticles control the number of particles that are
        /// added when AddParticles is called. The number of particles will be a random
        /// number between minNumParticles and maxNumParticles.
        /// </summary>
        protected int minNumParticles;
        protected int maxNumParticles;

        /// <summary>
        /// Gets or sets the minimum number of particles that are added when AddParticles
        /// is called. The number of particles will be a random number between MinNumParticles 
        /// and MaxNumParticles.
        /// </summary>
        /// <see cref="MaxNumParticles"/>
        public int MinNumParticles
        {
            get { return minNumParticles; }
            set { minNumParticles = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of particles that are added when AddParticles
        /// is called. The number of particles will be a random number between MinNumParticles 
        /// and MaxNumParticles.
        /// </summary>
        /// <see cref="MinNumParticles"/>
        public int MaxNumParticles
        {
            get { return maxNumParticles; }
            set { maxNumParticles = value; }
        }
       
        /// <summary>
        /// this controls the texture that the particle system uses. It will be used as
        /// an argument to ContentManager.Load.
        /// </summary>
        protected string textureFilename;

        /// <summary>
        /// Gets or sets the texture that the particle system uses. 
        /// </summary>
        public string TextureFilename
        {
            get { return textureFilename; }
            set { textureFilename = value; }
        }

        /// <summary>
        /// minInitialSpeed and maxInitialSpeed are used to control the initial velocity
        /// of the particles. The particle's initial speed will be a random number 
        /// between these two. The direction is determined by the function 
        /// PickRandomDirection, which can be overriden.
        /// </summary>
        protected float minInitialSpeed;
        protected float maxInitialSpeed;

        /// <summary>
        /// Gets or sets the minimum initial speed used to control the initial velocity
        /// of the particles. he particle's initial speed will be a random number 
        /// between MinInitialSpeed and MaxInitialSpeed. The direction is determined by the 
        /// function PickRandomDirection, which can be overriden.
        /// </summary>
        /// <see cref="MaxInitialSpeed"/>
        public float MinInitialSpeed
        {
            get { return minInitialSpeed; }
            set { minInitialSpeed = value; }
        }

        /// <summary>
        /// Gets or sets the maximum initial speed used to control the initial velocity
        /// of the particles. he particle's initial speed will be a random number 
        /// between MinInitialSpeed and MaxInitialSpeed. The direction is determined by the 
        /// function PickRandomDirection, which can be overriden.
        /// </summary>
        /// <see cref="MinInitialSpeed"/>
        public float MaxInitialSpeed
        {
            get { return maxInitialSpeed; }
            set { maxInitialSpeed = value; }
        }

        /// <summary>
        /// minAcceleration and maxAcceleration are used to control the acceleration of
        /// the particles. The particle's acceleration will be a random number between
        /// these two. By default, the direction of acceleration is the same as the
        /// direction of the initial velocity.
        /// </summary>
        protected float minAcceleration;
        protected float maxAcceleration;

        /// <summary>
        /// Gets or sets the minimum acceleration used to control the acceleration of
        /// the particles. The particle's acceleration will be a random number between
        /// MinAcceleration and MaxAcceleration. By default, the direction of acceleration 
        /// is the same as the direction of the initial velocity.
        /// </summary>
        /// <see cref="MaxAcceleration"/>
        public float MinAcceleration
        {
            get { return minAcceleration; }
            set { minAcceleration = value; }
        }

        /// <summary>
        /// Gets or sets the maximum acceleration used to control the acceleration of
        /// the particles. The particle's acceleration will be a random number between
        /// MinAcceleration and MaxAcceleration. By default, the direction of acceleration 
        /// is the same as the direction of the initial velocity.
        /// </summary>
        /// <see cref="MinAcceleration"/>
        public float MaxAcceleration
        {
            get { return maxAcceleration; }
            set { maxAcceleration = value; }
        }

        /// <summary>
        /// minRotationSpeed and maxRotationSpeed control the particles' angular
        /// velocity: the speed at which particles will rotate. Each particle's rotation
        /// speed will be a random number between minRotationSpeed and maxRotationSpeed.
        /// Use smaller numbers to make particle systems look calm and wispy, and large 
        /// numbers for more violent effects.
        /// </summary>
        protected float minRotationSpeed;
        protected float maxRotationSpeed;

        /// <summary>
        /// Gets or sets the minimum rotation speed that controls the particles' angular
        /// velocity: the speed at which particles will rotate. Each particle's rotation
        /// speed will be a random number between MinRotationSpeed and MaxRotationSpeed.
        /// Use smaller numbers to make particle systems look calm and wispy, and large 
        /// numbers for more violent effects.
        /// </summary>
        /// <see cref="MaxRotationSpeed"/>
        public float MinRotationSpeed
        {
            get { return minRotationSpeed; }
            set { minRotationSpeed = value; }
        }

        /// <summary>
        /// Gets or sets the maximum rotation speed that controls the particles' angular
        /// velocity: the speed at which particles will rotate. Each particle's rotation
        /// speed will be a random number between MinRotationSpeed and MaxRotationSpeed.
        /// Use smaller numbers to make particle systems look calm and wispy, and large 
        /// numbers for more violent effects.
        /// </summary>
        /// <see cref="MinRotationSpeed"/>
        public float MaxRotationSpeed
        {
            get { return maxRotationSpeed; }
            set { maxRotationSpeed = value; }
        }

        /// <summary>
        /// minLifetime and maxLifetime are used to control the lifetime. Each
        /// particle's lifetime will be a random number between these two. Lifetime
        /// is used to determine how long a particle "lasts." Also, in the base
        /// implementation of Draw, lifetime is also used to calculate alpha and scale
        /// values to avoid particles suddenly "popping" into view
        /// </summary>
        protected float minLifetime;
        protected float maxLifetime;

        /// <summary>
        /// Gets or sets the minimum lifetime used to control the lifetime. Each
        /// particle's lifetime will be a random number between MinLifetime and MaxLifetime. 
        /// Lifetime is used to determine how long a particle "lasts." Also, in the base
        /// implementation of Draw, lifetime is also used to calculate alpha and scale
        /// values to avoid particles suddenly "popping" into view
        /// </summary>
        /// <see cref="MaxLifetime"/>
        public float MinLifetime
        {
            get { return minLifetime; }
            set { minLifetime = value; }
        }

        /// <summary>
        /// Gets or sets the maximum lifetime used to control the lifetime. Each
        /// particle's lifetime will be a random number between MinLifetime and MaxLifetime. 
        /// Lifetime is used to determine how long a particle "lasts." Also, in the base
        /// implementation of Draw, lifetime is also used to calculate alpha and scale
        /// values to avoid particles suddenly "popping" into view
        /// </summary>
        /// <see cref="MinLifetime"/>
        public float MaxLifetime
        {
            get { return maxLifetime; }
            set { maxLifetime = value; }
        }

        /// <summary>
        /// to get some additional variance in the appearance of the particles, we give
        /// them all random scales. the scale is a value between minScale and maxScale,
        /// and is additionally affected by the particle's lifetime to avoid particles
        /// "popping" into view.
        /// </summary>
        protected float minScale;
        protected float maxScale;

        /// <summary>
        /// Gets or sets the minimum scale. To get some additional variance in the appearance 
        /// of the particles, we give them all random scales. the scale is a value between 
        /// MinScale and MaxScale, and is additionally affected by the particle's lifetime to 
        /// avoid particles "popping" into view.
        /// </summary>
        /// <see cref="MaxScale"/>
        public float MinScale
        {
            get { return minScale; }
            set { minScale = value; }
        }

        /// <summary>
        /// Gets or sets the maximum scale. To get some additional variance in the appearance 
        /// of the particles, we give them all random scales. the scale is a value between 
        /// MinScale and MaxScale, and is additionally affected by the particle's lifetime to 
        /// avoid particles "popping" into view.
        /// </summary>
        /// <see cref="MinScale"/>
        public float MaxScale
        {
            get { return maxScale; }
            set { maxScale = value; }
        }

        /// <summary>
        /// Gets whether to compute the particle positions in 3D space, and project
        /// to the screen space at the time of drawing. Setting this to true can be expensive
        /// if you add many particles, but if you want to make the particles appear at appropriate
        /// locations in AR space, it is necessary to set this to true. Default value is false.
        /// </summary>
        public bool ComputeIn3D
        {
            get;
            protected set;
        }

        /// <summary>
        /// different effects can use different blend states. fire and explosions work
        /// well with additive blending, for example.
        /// </summary>
		protected BlendState blendState;

        private SpriteBatch spriteBatch;

        #endregion

        protected ParticleEffect(int howManyEffects, SpriteBatch spriteBatch)
            : this(howManyEffects, spriteBatch, false)
        {
        }
        
        /// <summary>
        /// Constructs a new ParticleSystem.
        /// </summary>
        /// <param name="howManyEffects">the maximum number of particle effects that
        /// are expected on screen at once.</param>
        /// <param name="spriteBatch">a sprite batch instance used to draw the particles</param>
        /// <param name="computeIn3D">whether to compute the particle positions in 3D space, and project
        /// to the screen space at the time of drawing. Setting this to true can be expensive
        /// if you add many particles, but if you want to make the particles appear at appropriate
        /// locations in AR space, it is necessary to set this to true</param>
        /// <remarks>it is tempting to set the value of howManyEffects very high.
        /// However, this value should be set to the minimum possible, because
        /// it has a large impact on the amount of memory required, and slows down the
        /// Update and Draw functions.</remarks>
        protected ParticleEffect(int howManyEffects, SpriteBatch spriteBatch, bool computeIn3D)
        {            
            this.howManyEffects = howManyEffects;
            this.spriteBatch = spriteBatch;
            ComputeIn3D = computeIn3D;
            drawOrder = 0;
            enabled = true;
            Initialize();
            LoadContent();
        }

        /// <summary>
        /// override the base class's Initialize to do some additional work; we want to
        /// call InitializeConstants to let subclasses set the constants that we'll use.
        /// 
        /// also, the particle array and freeParticles queue are set up here.
        /// </summary>
        protected virtual void Initialize()
        {
            InitializeConstants();
            
            // calculate the total number of particles we will ever need, using the
            // max number of effects and the max number of particles per effect.
            // once these particles are allocated, they will be reused, so that
            // we don't put any pressure on the garbage collector.
            particles = new Particle[howManyEffects * maxNumParticles];
            freeParticles = new Queue<Particle>(howManyEffects * maxNumParticles);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = new Particle();
                freeParticles.Enqueue(particles[i]);
            }
        }

        /// <summary>
        /// this abstract function must be overriden by subclasses of ParticleSystem.
        /// It's here that they should set all the constants marked in the region
        /// "constants to be set by subclasses", which give each ParticleSystem its
        /// specific flavor.
        /// </summary>
        protected abstract void InitializeConstants();

        /// <summary>
        /// Override the base class LoadContent to load the texture. once it's
        /// loaded, calculate the origin.
        /// </summary>
        protected virtual void LoadContent()
        {
            // make sure sub classes properly set textureFilename.
            if (string.IsNullOrEmpty(textureFilename))
            {
                string message = "textureFilename wasn't set properly, so the " +
                    "particle system doesn't know what texture to load. Make " +
                    "sure your particle system's InitializeConstants function " +
                    "properly sets textureFilename.";
                throw new InvalidOperationException(message);
            }
            // load the texture....
            texture = State.Content.Load<Texture2D>(textureFilename);

            // ... and calculate the center. this'll be used in the draw call, we
            // always want to rotate and scale around this point.
            origin.X = texture.Width / 2;
            origin.Y = texture.Height / 2;
        }

        /// <summary>
        /// AddParticles's job is to add an effect somewhere on the screen. If there 
        /// aren't enough particles in the freeParticles queue, it will use as many as 
        /// it can. This means that if there not enough particles available, calling
        /// AddParticles will have no effect.
        /// </summary>
        /// <param name="where">where the particle effect should be created</param>
        public void AddParticles(Vector2 where)
        {
            if (ComputeIn3D)
                throw new GoblinException("Use AddParticles3D method when ComputeIn3D is set to true");

            // the number of particles we want for this effect is a random number
            // somewhere between the two constants specified by the subclasses.
            int numParticles = RandomHelper.GetRandomInt(minNumParticles, maxNumParticles);

            // create that many particles, if you can.
            for (int i = 0; i < numParticles && freeParticles.Count > 0; i++)
            {
                // grab a particle from the freeParticles queue, and Initialize it.
                Particle p = freeParticles.Dequeue();
                InitializeParticle(p, where);               
            }
        }

        /// <summary>
        /// AddParticles3D's job is to add an effect somewhere on the screen. If there 
        /// aren't enough particles in the freeParticles queue, it will use as many as 
        /// it can. This means that if there not enough particles available, calling
        /// AddParticles3D will have no effect.
        /// </summary>
        /// <param name="where"></param>
        public void AddParticles3D(Vector3 where)
        {
            if (!ComputeIn3D)
                throw new GoblinException("Use AddParticles method when ComputeIn3D is set to false");

            // the number of particles we want for this effect is a random number
            // somewhere between the two constants specified by the subclasses.
            int numParticles = RandomHelper.GetRandomInt(minNumParticles, maxNumParticles);

            // create that many particles, if you can.
            for (int i = 0; i < numParticles && freeParticles.Count > 0; i++)
            {
                // grab a particle from the freeParticles queue, and Initialize it.
                Particle p = freeParticles.Dequeue();
                InitializeParticle3D(p, where);
            }
        }

        /// <summary>
        /// InitializeParticle randomizes some properties for a particle, then
        /// calls initialize on it. It can be overriden by subclasses if they 
        /// want to modify the way particles are created. For example, 
        /// SmokePlumeParticleSystem overrides this function make all particles
        /// accelerate to the right, simulating wind.
        /// </summary>
        /// <param name="p">the particle to initialize</param>
        /// <param name="where">the position on the screen that the particle should be
        /// </param>
        protected virtual void InitializeParticle(Particle p, Vector2 where)
        {
            // first, call PickRandomDirection to figure out which way the particle
            // will be moving. velocity and acceleration's values will come from this.
            Vector2 direction = PickRandomDirection();

            // pick some random values for our particle
            float velocity = RandomHelper.GetRandomFloat(minInitialSpeed, maxInitialSpeed);
            float acceleration = RandomHelper.GetRandomFloat(minAcceleration, maxAcceleration);
            float lifetime = RandomHelper.GetRandomFloat(minLifetime, maxLifetime);
            float scale = RandomHelper.GetRandomFloat(minScale, maxScale);
            float rotationSpeed = RandomHelper.GetRandomFloat(minRotationSpeed, maxRotationSpeed);

            // then initialize it with those random values. initialize will save those,
            // and make sure it is marked as active.
            p.Initialize(
                where, velocity * direction, acceleration * direction,
                lifetime, scale, rotationSpeed);
        }

        /// <summary>
        /// InitializeParticle3D randomizes some properties for a particle, then
        /// calls initialize on it. It can be overriden by subclasses if they 
        /// want to modify the way particles are created. For example, 
        /// SmokePlumeParticleSystem overrides this function make all particles
        /// accelerate to the right, simulating wind.
        /// </summary>
        /// <param name="p">the particle to initialize</param>
        /// <param name="where">the position on the screen that the particle should be</param>
        protected virtual void InitializeParticle3D(Particle p, Vector3 where)
        {
            // first, call PickRandomDirection to figure out which way the particle
            // will be moving. velocity and acceleration's values will come from this.
            Vector3 direction = PickRandomDirection3D();
            direction.Normalize();

            // pick some random values for our particle
            float velocity = RandomHelper.GetRandomFloat(minInitialSpeed, maxInitialSpeed);
            float acceleration = RandomHelper.GetRandomFloat(minAcceleration, maxAcceleration);
            float lifetime = RandomHelper.GetRandomFloat(minLifetime, maxLifetime);
            float scale = RandomHelper.GetRandomFloat(minScale, maxScale);
            float rotationSpeed = RandomHelper.GetRandomFloat(minRotationSpeed, maxRotationSpeed);

            // then initialize it with those random values. initialize will save those,
            // and make sure it is marked as active.
            p.Initialize3D(
                where, velocity * direction, acceleration * direction,
                lifetime, scale, rotationSpeed);
        }

        /// <summary>
        /// PickRandomDirection is used by InitializeParticles to decide which direction
        /// particles will move. The default implementation is a random vector in a
        /// circular pattern.
        /// </summary>
        protected virtual Vector2 PickRandomDirection()
        {
            float angle = RandomHelper.GetRandomFloat(0, MathHelper.TwoPi);
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        /// <summary>
        /// PickRandomDirection3D is used by InitializeParticles3D to decide which direction
        /// particles will move. The default implementation is a random vector in a
        /// circular pattern.
        /// </summary>
        /// <returns></returns>
        protected virtual Vector3 PickRandomDirection3D()
        {
            float angle = RandomHelper.GetRandomFloat(0, MathHelper.TwoPi);
            float theta = RandomHelper.GetRandomFloat(-MathHelper.PiOver2, MathHelper.PiOver2);
            return new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), (float)Math.Sin(theta));
        }

        /// <summary>
        /// overriden from DrawableGameComponent, Update will update all of the active
        /// particles.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            // calculate dt, the change in the since the last frame. the particle
            // updates will use this value.
            float dt = (float)elapsedTime.TotalSeconds;

            // go through all of the particles...
            foreach (Particle p in particles)
            {
                
                if (p.Active)
                {
                    // ... and if they're active, update them.
                    if (ComputeIn3D)
                        p.Update3D(dt);
                    else
                        p.Update(dt);
                    // if that update finishes them, put them onto the free particles
                    // queue.
                    if (!p.Active)
                    {
                        freeParticles.Enqueue(p);
                    }
                }   
            }
        }

        /// <summary>
        /// Draw will use ParticleSampleGame's 
        /// sprite batch to render all of the active particles.
        /// </summary>
        public void Render(Matrix renderMatrix)
        {
            // tell sprite batch to begin, using the spriteBlendMode specified in
            // initializeConstants
			spriteBatch.Begin(SpriteSortMode.Deferred, blendState);

            if (ComputeIn3D)
                MatrixHelper.PrepareProject(renderMatrix, State.ViewMatrix, State.ProjectionMatrix);
            
            foreach (Particle p in particles)
            {
                // skip inactive particles
                if (!p.Active)
                    continue;

                // normalized lifetime is a value from 0 to 1 and represents how far
                // a particle is through its life. 0 means it just started, .5 is half
                // way through, and 1.0 means it's just about to be finished.
                // this value will be used to calculate alpha and scale, to avoid 
                // having particles suddenly appear or disappear.
                float normalizedLifetime = p.TimeSinceStart / p.Lifetime;

                // we want particles to fade in and fade out, so we'll calculate alpha
                // to be (normalizedLifetime) * (1-normalizedLifetime). this way, when
                // normalizedLifetime is 0 or 1, alpha is 0. the maximum value is at
                // normalizedLifetime = .5, and is
                // (normalizedLifetime) * (1-normalizedLifetime)
                // (.5)                 * (1-.5)
                // .25
                // since we want the maximum alpha to be 1, not .25, we'll scale the 
                // entire equation by 4.
                float alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);
				Color color = Color.White * alpha;

                // make particles grow as they age. they'll start at 75% of their size,
                // and increase to 100% once they're finished.
                float scale = p.Scale * (.75f + .25f * normalizedLifetime);

                if (ComputeIn3D)
                {
                    tempVec3_2 = p.Position3D;
                    MatrixHelper.Project(ref tempVec3_2, ref tempVec3);
                    drawPosition.X = tempVec3.X * State.Width;
                    drawPosition.Y = tempVec3.Y * State.Height;
                }
                else
                    drawPosition = p.Position;

                spriteBatch.Draw(texture, drawPosition, null, color,
                    p.Rotation, origin, scale, SpriteEffects.None, 0.0f);
            }

            spriteBatch.End();
        }

        public int CompareTo(ParticleEffect other)
        {
            if (this.DrawOrder > other.DrawOrder)
                return 1;
            else if (this.DrawOrder == other.DrawOrder)
                return 0;
            else
                return -1;
        }

        public void Dispose()
        {
            texture.Dispose();
            freeParticles.Clear();
        }
    }
}
