/************************************************************************************ 
 *
 * Microsoft XNA Community Game Platform
 * Copyright (C) Microsoft Corporation. All rights reserved.
 * 
 * ===================================================================================
 * Modified by: Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/

#region Using Statements

using System;
using System.Xml;
using System.ComponentModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using GoblinXNA.Shaders;
using GoblinXNA.Helpers;

#endregion

namespace GoblinXNA.Graphics.ParticleEffects
{
    /// <summary>
    /// Particle effects are custom vertex-based effects like exhaust steams, fire and smoke. 
    /// The implementation is based on the Particle 3D tutorial from the XNA Creators Club 
    /// website (http://creators.xna.com).
    /// </summary>
    public class ParticleEffect : IDisposable, IComparable<ParticleEffect>
    {
        #region Member Fields

        protected String textureName;
        protected int maxParticles;
        protected TimeSpan duration;
        protected float durationRandomness;
        protected float emitterVelocitySensitivity;
        protected float minHorizontalVelocity;
        protected float maxHorizontalVelocity;
        protected float minVerticalVelocity;
        protected float maxVerticalVelocity;
        protected Vector3 gravity;
        protected float endVelocity;
        protected Color minColor;
        protected Color maxColor;
        protected float minRotateSpeed;
        protected float maxRotateSpeed;
        protected float minStartSize;
        protected float maxStartSize;
        protected float minEndSize;
        protected float maxEndSize;
        protected BlendState blendState;

        protected Vector3 up;
        protected Vector3 right;
        protected Vector3 forward;

        protected IShader shader;
        protected String shaderTechnique;
        protected bool enabled;
        protected int drawOrder;

        /// <summary>
        /// An array of particles, treated as a circular queue.
        /// </summary>
        protected ParticleVertex[] particles;

        /// <summary>
        /// A vertex buffer holding our particles. This contains the same data as
        /// the particles array, but copied across to where the GPU can access it.
        /// </summary>
        DynamicVertexBuffer vertexBuffer;

        /// <summary>
        /// Index buffer turns sets of four vertices into particle quads (pairs of triangles).
        /// </summary>
        IndexBuffer indexBuffer;

        // The particles array and vertex buffer are treated as a circular queue.
        // Initially, the entire contents of the array are free, because no particles
        // are in use. When a new particle is created, this is allocated from the
        // beginning of the array. If more than one particle is created, these will
        // always be stored in a consecutive block of array elements. Because all
        // particles last for the same amount of time, old particles will always be
        // removed in order from the start of this active particle region, so the
        // active and free regions will never be intermingled. Because the queue is
        // circular, there can be times when the active particle region wraps from the
        // end of the array back to the start. The queue uses modulo arithmetic to
        // handle these cases. For instance with a four entry queue we could have:
        //
        //      0
        //      1 - first active particle
        //      2 
        //      3 - first free particle
        //
        // In this case, particles 1 and 2 are active, while 3 and 4 are free.
        // Using modulo arithmetic we could also have:
        //
        //      0
        //      1 - first free particle
        //      2 
        //      3 - first active particle
        //
        // Here, 3 and 0 are active, while 1 and 2 are free.
        //
        // But wait! The full story is even more complex.
        //
        // When we create a new particle, we add them to our managed particles array.
        // We also need to copy this new data into the GPU vertex buffer, but we don't
        // want to do that straight away, because setting new data into a vertex buffer
        // can be an expensive operation. If we are going to be adding several particles
        // in a single frame, it is faster to initially just store them in our managed
        // array, and then later upload them all to the GPU in one single call. So our
        // queue also needs a region for storing new particles that have been added to
        // the managed array but not yet uploaded to the vertex buffer.
        //
        // Another issue occurs when old particles are retired. The CPU and GPU run
        // asynchronously, so the GPU will often still be busy drawing the previous
        // frame while the CPU is working on the next frame. This can cause a
        // synchronization problem if an old particle is retired, and then immediately
        // overwritten by a new one, because the CPU might try to change the contents
        // of the vertex buffer while the GPU is still busy drawing the old data from
        // it. Normally the graphics driver will take care of this by waiting until
        // the GPU has finished drawing inside the VertexBuffer.SetData call, but we
        // don't want to waste time waiting around every time we try to add a new
        // particle! To avoid this delay, we can specify the SetDataOptions.NoOverwrite
        // flag when we write to the vertex buffer. This basically means "I promise I
        // will never try to overwrite any data that the GPU might still be using, so
        // you can just go ahead and update the buffer straight away". To keep this
        // promise, we must avoid reusing vertices immediately after they are drawn.
        //
        // So in total, our queue contains four different regions:
        //
        // Vertices between firstActiveParticle and firstNewParticle are actively
        // being drawn, and exist in both the managed particles array and the GPU
        // vertex buffer.
        //
        // Vertices between firstNewParticle and firstFreeParticle are newly created,
        // and exist only in the managed particles array. These need to be uploaded
        // to the GPU at the start of the next draw call.
        //
        // Vertices between firstFreeParticle and firstRetiredParticle are free and
        // waiting to be allocated.
        //
        // Vertices between firstRetiredParticle and firstActiveParticle are no longer
        // being drawn, but were drawn recently enough that the GPU could still be
        // using them. These need to be kept around for a few more frames before they
        // can be reallocated.

        protected int firstActiveParticle;
        protected int firstNewParticle;
        protected int firstFreeParticle;
        protected int firstRetiredParticle;

        /// <summary>
        /// Store the current time, in seconds.
        /// </summary>
        protected float currentTime;

        /// <summary>
        /// Count how many times Draw has been called. This is used to know
        /// when it is safe to retire old particles back into the free list.
        /// </summary>
        protected int drawCounter;


        /// <summary>
        /// Shared random number generator.
        /// </summary>
        protected static Random random = new Random();

        #endregion

        #region Constructors

        /// <summary>
        ///Creates a default particle effect with 100 maximum particles.
        /// </summary>
        public ParticleEffect() : this(100) { }

        /// <summary>
        ///Creates a default particle effect.
        /// </summary>
        /// <param name="maxParticles">
        /// Maximum number of particles that can be displayed at one time.
        /// </param>
        public ParticleEffect(int maxParticles)
        {
            this.maxParticles = maxParticles;
            Initialize();
            LoadContent();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a shader to render this particle effect
        /// </summary>
        public virtual IShader Shader
        {
            get { return shader; }
            set { shader = value; }
        }

        /// <summary>
        /// Gets or sets the shader technique to use .
        /// </summary>
        public virtual String ShaderTechnique
        {
            get { return shaderTechnique; }
            set { shaderTechnique = value; }
        }

        /// <summary>
        /// Gets or sets whether this particle effect should be rendered. The default is true.
        /// </summary>
        public virtual bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets or sets the name of the texture used by this particle system.
        /// </summary>
        public virtual String TextureName
        {
            get { return textureName; }
            set { textureName = value; }
        }

        /// <summary>
        /// Gets the maximum number of particles that can be displayed at one time.
        /// </summary>
        public int MaxParticles
        {
            get { return maxParticles; }
        }

        /// <summary>
        /// Gets or sets how long these particles will last. 
        /// </summary>
        public virtual TimeSpan Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        /// <summary>
        /// Gets or sets the plus/minus value randomly added to Duration.
        /// 
        /// If greater than zero, some particles will last a shorter time than others.
        /// </summary>
        public virtual float DurationRandomness
        {
            get { return durationRandomness; }
            set { durationRandomness = value; }
        }

        /// <summary>
        /// Controls how much particles are influenced by the velocity of the object
        /// that created them. You can see this in action with the explosion effect,
        /// where the flames continue to move in the same direction as the source
        /// projectile. The projectile trail particles, on the other hand, set this
        /// value very low so they are less affected by the velocity of the projectile.
        /// </summary>
        /// <see cref="GoblinXNA.Graphics.ParticleEffects.ExplosionParticleEffect"/>
        public virtual float EmitterVelocitySensitivity
        {
            get { return emitterVelocitySensitivity; }
            set { emitterVelocitySensitivity = value; }
        }

        /// <summary>
        /// Mininum of the range of values controlling how much X and Z axis velocity to give 
        /// each particle. Values for individual particles are randomly chosen from somewhere 
        /// between this value and MaxHorizontalVelocity.
        /// </summary>
        /// <see cref="MaxHorizontalVelocity"/>
        public virtual float MinHorizontalVelocity
        {
            get { return minHorizontalVelocity; }
            set { minHorizontalVelocity = value; }
        }

        /// <summary>
        /// Maximum of the range of values controlling how much X and Z axis velocity to give 
        /// each particle. Values for individual particles are randomly chosen from somewhere 
        /// between MinHorizontalVelocity and this value.
        /// </summary>
        /// <see cref="MinHorizontalVelocity"/>
        public virtual float MaxHorizontalVelocity
        {
            get { return maxHorizontalVelocity; }
            set { maxHorizontalVelocity = value; }
        }

        /// <summary>
        /// Minimum of the range of values controlling how much Y axis velocity to give each 
        /// particle. Values for individual particles are randomly chosen from somewhere 
        /// between this value and MaxVerticalVelocity.
        /// </summary>
        /// <see cref="MinVerticalVelocity"/>
        public virtual float MinVerticalVelocity
        {
            get { return minVerticalVelocity; }
            set { minVerticalVelocity = value; }
        }

        /// <summary>
        /// Maximum of the range of values controlling how much Y axis velocity to give each 
        /// particle. Values for individual particles are randomly chosen from somewhere 
        /// between MinVerticalVelocity and this value.
        /// </summary>
        /// <see cref="MaxVerticalVelocity"/>
        public virtual float MaxVerticalVelocity
        {
            get { return maxVerticalVelocity; }
            set { maxVerticalVelocity = value; }
        }

        /// <summary>
        /// Direction and strength of the gravity effect. Note that this can point in any
        /// direction, not just down! The fire effect points it upward to make the flames
        /// rise, and the smoke plume points it sideways to simulate wind.
        /// </summary>
        public virtual Vector3 Gravity
        {
            get { return gravity; }
            set { gravity = value; }
        }

        /// <summary>
        /// Controls how the particle velocity will change over their lifetime. If set
        /// to 1, particles will keep going at the same speed as when they were created.
        /// If set to 0, particles will come to a complete stop right before they die.
        /// Values greater than 1 make the particles speed up over time.
        /// The default value is 1.
        /// </summary>
        public virtual float EndVelocity
        {
            get { return endVelocity; }
            set { endVelocity = value; }
        }

        /// <summary>
        /// Minimum of range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between this color
        /// and MaxColor. The default value is Color.Black.
        /// </summary>
        /// <see cref="MaxColor"/>
        public virtual Color MinColor
        {
            get { return minColor; }
            set { minColor = value; }
        }

        /// <summary>
        /// Maximum of range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between MinColor
        /// and this value.
        /// </summary>
        /// <see cref="MinColor"/>
        public virtual Color MaxColor
        {
            get { return maxColor; }
            set { maxColor = value; }
        }

        /// <summary>
        /// Minimum of range of values controlling how fast the particles rotate. Values for
        /// individual particles are randomly chosen from somewhere between these
        /// limits. If both these values are set to 0, the particle system will
        /// automatically switch to an alternative shader technique that does not
        /// support rotation, and thus requires significantly less GPU power. This
        /// means if you don't need the rotation effect, you may get a performance
        /// boost from leaving these values at 0.
        /// </summary>
        /// <see cref="MaxRotateSpeed"/>
        public virtual float MinRotateSpeed
        {
            get { return minRotateSpeed; }
            set { minRotateSpeed = value; }
        }

        /// <summary>
        /// Maximum of range of values controlling how fast the particles rotate. Values for
        /// individual particles are randomly chosen from somewhere between these
        /// limits. If both these values are set to 0, the particle system will
        /// automatically switch to an alternative shader technique that does not
        /// support rotation, and thus requires significantly less GPU power. This
        /// means if you don't need the rotation effect, you may get a performance
        /// boost from leaving these values at 0. 
        /// </summary>
        /// <see cref="MinRotateSpeed"/>
        public virtual float MaxRotateSpeed
        {
            get { return maxRotateSpeed; }
            set { maxRotateSpeed = value; }
        }

        /// <summary>
        /// Minimum of range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between
        /// MinStartSize and MaxStartSize. 
        /// </summary>
        /// <see cref="MinStartSize"/>
        public virtual float MinStartSize
        {
            get { return minStartSize; }
            set { minStartSize = value; }
        }

        /// <summary>
        /// Maximum of range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between
        /// MinStartSize and MaxStartSize. 
        /// </summary>
        /// <see cref="MaxStartSize"/>
        public virtual float MaxStartSize
        {
            get { return maxStartSize; }
            set { maxStartSize = value; }
        }

        /// <summary>
        /// Minimum of range of values controlling how big particles become at the end of their
        /// life. Values for individual particles are randomly chosen from somewhere
        /// between MinEndSize and MaxEndSize. 
        /// </summary>
        /// <see cref="MaxEndSize"/>
        public virtual float MinEndSize
        {
            get { return minEndSize; }
            set { minEndSize = value; }
        }

        /// <summary>
        /// Maximum of range of values controlling how big particles become at the end of their
        /// life. Values for individual particles are randomly chosen from somewhere
        /// between MinEndSize and MaxEndSize.
        /// </summary>
        /// <see cref="MaxEndSize"/>
        public virtual float MaxEndSize
        {
            get { return maxEndSize; }
            set { maxEndSize = value; }
        }

        /// <summary>
        /// Alpha blending settings. 
        /// </summary>
        public virtual BlendState BlendState
        {
            get { return blendState; }
            set { blendState = value; }
        }

        /// <summary>
        /// A direction in which vertical velocity is applied. Default is Vector3.UnitY.
        /// </summary>
        public virtual Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        /// <summary>
        /// A direction in which horizontal velocity is applied. Default is Vector3.UnitX.
        /// </summary>
        public virtual Vector3 Right
        {
            get { return right; }
            set { right = value; }
        }

        /// <summary>
        /// A direction in which horizontal velocity is applied. Default is Vector3.UnitZ.
        /// </summary>
        public virtual Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }

        /// <summary>
        /// Gets the time elapsed since the particle simulation started. 
        /// </summary>
        public virtual float CurrentTime
        {
            get { return currentTime; }
        }

        /// <summary>
        /// Gets or sets the order of drawing each particle effect. Smaller value is drawn earlier. 
        /// </summary>
        public virtual int DrawOrder
        {
            get { return drawOrder; }
            set { drawOrder = value; }
        }

        #endregion

        #region Initialization

        protected virtual void Initialize()
        {
            textureName = null;
            duration = TimeSpan.FromSeconds(1);
            durationRandomness = 0;
            emitterVelocitySensitivity = 1;
            minHorizontalVelocity = 0;
            maxHorizontalVelocity = 0;
            minVerticalVelocity = 0;
            maxVerticalVelocity = 0;
            gravity = new Vector3();
            endVelocity = 1;
            minColor = Color.White;
            maxColor = Color.White;
            minRotateSpeed = 0;
            maxRotateSpeed = 0;
            minStartSize = 100;
            maxStartSize = 100;
            minEndSize = 100;
            maxEndSize = 100;
            blendState = BlendState.NonPremultiplied;
            currentTime = 0;
            drawOrder = 0;

            up = Vector3.UnitY;
            right = Vector3.UnitX;
            forward = Vector3.UnitZ;

            // Allocate the particle array, and fill in the corner fields (which never change).
            particles = new ParticleVertex[maxParticles * 4];

            for (int i = 0; i < maxParticles; i++)
            {
                particles[i * 4 + 0].Corner = new Short2(-1, -1);
                particles[i * 4 + 1].Corner = new Short2(1, -1);
                particles[i * 4 + 2].Corner = new Short2(1, 1);
                particles[i * 4 + 3].Corner = new Short2(-1, 1);
            }

            shader = new ParticleShader();
            shaderTechnique = "Particles";
            enabled = true;
        }

        /// <summary>
        /// Loads graphics for the particle system.
        /// </summary>
        protected virtual void LoadContent()
        {
            vertexBuffer = new DynamicVertexBuffer(State.Device, ParticleVertex.VertexDeclaration,
                                                   maxParticles * 4, BufferUsage.WriteOnly);

            // Create and populate the index buffer.
            ushort[] indices = new ushort[maxParticles * 6];

            for (int i = 0; i < maxParticles; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                indices[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            indexBuffer = new IndexBuffer(State.Device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);

            indexBuffer.SetData(indices);
        }

        #endregion

        #region Update and Render

        /// <summary>
        /// Updates the particle system.
        /// </summary>
        public virtual void Update(TimeSpan elapsedTime)
        {
            currentTime += (float)elapsedTime.TotalSeconds;

            RetireActiveParticles();
            FreeRetiredParticles();

            // If we let our timer go on increasing for ever, it would eventually
            // run out of floating point precision, at that point the particles
            // would render incorrectly. An easy way to prevent this is to notice
            // that the time value doesn't matter when no particles are being drawn,
            // so we can reset it back to zero any time the active queue is empty.

            if (firstActiveParticle == firstFreeParticle)
                currentTime = 0;

            if (firstRetiredParticle == firstActiveParticle)
                drawCounter = 0;
        }


        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        protected virtual void RetireActiveParticles()
        {
            float particleDuration = (float)duration.TotalSeconds;

            while (firstActiveParticle != firstNewParticle)
            {
                // Is this particle old enough to retire?
                float particleAge = currentTime - particles[firstActiveParticle * 4].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at that we retired this particle.
                particles[firstActiveParticle * 4].Time = drawCounter;

                // Move the particle from the active to the retired queue.
                firstActiveParticle++;

                if (firstActiveParticle >= maxParticles)
                    firstActiveParticle = 0;
            }
        }


        /// <summary>
        /// Helper for checking when retired particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old particles from the retired area of the queue to the free section.
        /// </summary>
        protected virtual void FreeRetiredParticles()
        {
            while (firstRetiredParticle != firstActiveParticle)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                int age = drawCounter - (int)particles[firstRetiredParticle * 4].Time;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                firstRetiredParticle++;

                if (firstRetiredParticle >= maxParticles)
                    firstRetiredParticle = 0;
            }
        }

        public virtual void Render()
        {
            Render(Matrix.Identity);
        }

        /// <summary>
        /// Renders this particle effect using the given shader.
        /// </summary>
        /// <param name="renderMatrix">The transformation to apply to this particle system</param>
        public virtual void Render(Matrix renderMatrix)
        {
            // Restore the vertex buffer contents if the graphics device was lost.
            if (vertexBuffer.IsContentLost)
            {
                vertexBuffer.SetData(particles);
            }

            // If there are any particles waiting in the newly added queue,
            // we'd better upload them to the GPU ready for drawing.
            if (firstNewParticle != firstFreeParticle)
            {
                AddNewParticlesToVertexBuffer();
            }

            // If there are any active particles, draw them now!
            if (firstActiveParticle != firstFreeParticle)
            {
                BlendState origBlendState = State.Device.BlendState;
                State.Device.BlendState = blendState;
                State.Device.DepthStencilState = DepthStencilState.DepthRead;

                // Set the particle vertex buffer and vertex declaration.
                State.Device.SetVertexBuffer(vertexBuffer);

                State.Device.Indices = indexBuffer;

                shader.Render(
                    ref renderMatrix, 
                    shaderTechnique,
                    delegate
                    {
                        if (firstActiveParticle < firstFreeParticle)
                        {
                            // If the active particles are all in one consecutive range,
                            // we can draw them all in a single call.
                            State.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                         firstActiveParticle * 4, (firstFreeParticle - firstActiveParticle) * 4,
                                                         firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 2);
                        }
                        else
                        {
                            // If the active particle range wraps past the end of the queue
                            // back to the start, we must split them over two draw calls.
                            State.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                         firstActiveParticle * 4, (maxParticles - firstActiveParticle) * 4,
                                                         firstActiveParticle * 6, (maxParticles - firstActiveParticle) * 2);

                            if (firstFreeParticle > 0)
                            {
                                State.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                             0, firstFreeParticle * 4,
                                                             0, firstFreeParticle * 2);
                            }
                        }
                    });

                // Reset some of the renderstates that we changed,
                // so as not to mess up any other subsequent drawing.
                State.Device.DepthStencilState = DepthStencilState.Default;
                State.Device.BlendState = origBlendState;
            }

            drawCounter++;
        }


        /// <summary>
        /// Helper for uploading new particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>
        protected virtual void AddNewParticlesToVertexBuffer()
        {
            int stride = ParticleVertex.SizeInBytes;

            if (firstNewParticle < firstFreeParticle)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                vertexBuffer.SetData(firstNewParticle * stride * 4, particles,
                                     firstNewParticle * 4,
                                     (firstFreeParticle - firstNewParticle) * 4,
                                     stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                vertexBuffer.SetData(firstNewParticle * stride * 4, particles,
                                     firstNewParticle * 4,
                                     (maxParticles - firstNewParticle) * 4,
                                     stride, SetDataOptions.NoOverwrite);

                if (firstFreeParticle > 0)
                {
                    vertexBuffer.SetData(0, particles,
                                         0, firstFreeParticle * 4,
                                         stride, SetDataOptions.NoOverwrite);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            firstNewParticle = firstFreeParticle;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a new particle to the system.
        /// </summary>
        public virtual void AddParticle(Vector3 position, Vector3 velocity)
        {
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= maxParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == firstRetiredParticle)
                return;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.
            velocity *= emitterVelocitySensitivity;

            // Add in some random amount of horizontal velocity.
            float horizontalVelocity = MathHelper.Lerp(minHorizontalVelocity,
                                                       maxHorizontalVelocity,
                                                       (float)random.NextDouble());

            double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            velocity += right * horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity += forward * horizontalVelocity * (float)Math.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            velocity += up * MathHelper.Lerp(minVerticalVelocity,
                                          maxVerticalVelocity,
                                          (float)random.NextDouble());

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            Color randomValues = new Color((byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255));

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                particles[firstFreeParticle * 4 + i].Position = position;
                particles[firstFreeParticle * 4 + i].Velocity = velocity;
                particles[firstFreeParticle * 4 + i].Random = randomValues;
                particles[firstFreeParticle * 4 + i].Time = currentTime;
            }

            firstFreeParticle = nextFreeParticle;
        }

        /// <summary>
        /// Resets the particle effect if it supports reset.
        /// </summary>
        public virtual void Reset()
        {
            LoadContent();
        }

        public virtual XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = xmlDoc.CreateElement(TypeDescriptor.GetClassName(this));

            xmlNode.SetAttribute("Enabled", enabled.ToString());
            xmlNode.SetAttribute("TextureName", textureName);
            xmlNode.SetAttribute("MaxParticles", maxParticles.ToString());
            xmlNode.SetAttribute("Duration", duration.TotalMilliseconds.ToString());
            xmlNode.SetAttribute("DurationRandomness", durationRandomness.ToString());
            xmlNode.SetAttribute("EmitterVelocitySensitivity", emitterVelocitySensitivity.ToString());
            xmlNode.SetAttribute("MinHorizontalVelocity", minHorizontalVelocity.ToString());
            xmlNode.SetAttribute("MaxHorizontalVelocity", maxHorizontalVelocity.ToString());
            xmlNode.SetAttribute("MinVerticalVelocity", minVerticalVelocity.ToString());
            xmlNode.SetAttribute("MaxVerticalVelocity", maxVerticalVelocity.ToString());
            xmlNode.SetAttribute("Gravity", gravity.ToString());
            xmlNode.SetAttribute("EndVelocity", endVelocity.ToString());
            xmlNode.SetAttribute("MinColor", minColor.ToString());
            xmlNode.SetAttribute("MaxColor", maxColor.ToString());
            xmlNode.SetAttribute("MinRotateSpeed", minRotateSpeed.ToString());
            xmlNode.SetAttribute("MaxRotateSpeed", maxRotateSpeed.ToString());
            xmlNode.SetAttribute("MinStartSize", minStartSize.ToString());
            xmlNode.SetAttribute("MaxStartSize", maxStartSize.ToString());
            xmlNode.SetAttribute("MinEndSize", minEndSize.ToString());
            xmlNode.SetAttribute("MaxEndSize", maxEndSize.ToString());
            xmlNode.SetAttribute("BlendState", blendState.ToString());
            xmlNode.SetAttribute("DrawOrder", drawOrder.ToString());
            xmlNode.SetAttribute("Shader", TypeDescriptor.GetClassName(shader));
            if (shaderTechnique.Length > 0)
                xmlNode.SetAttribute("ShaderTechnique", shaderTechnique);

            return xmlNode;
        }

        public virtual void Load(XmlElement xmlNode)
        {
            if (xmlNode.HasAttribute("Enabled"))
                enabled = bool.Parse(xmlNode.GetAttribute("Enabled"));
            if (xmlNode.HasAttribute("TextureName"))
                textureName = xmlNode.GetAttribute("TextureName");
            if (xmlNode.HasAttribute("MaxParticles"))
                maxParticles = int.Parse(xmlNode.GetAttribute("MaxParticles"));
            if (xmlNode.HasAttribute("Duration"))
                duration = TimeSpan.FromMilliseconds(double.Parse(xmlNode.GetAttribute("Duration")));
            if (xmlNode.HasAttribute("DurationRandomness"))
                durationRandomness = float.Parse(xmlNode.GetAttribute("DurationRandomness"));
            if (xmlNode.HasAttribute("EmitterVelocitySensitivity"))
                emitterVelocitySensitivity = float.Parse(xmlNode.GetAttribute("EmitterVelocitySensitivity"));
            if (xmlNode.HasAttribute("MinHorizontalVelocity"))
                minHorizontalVelocity = float.Parse(xmlNode.GetAttribute("MinHorizontalVelocity"));
            if (xmlNode.HasAttribute("MaxHorizontalVelocity"))
                maxHorizontalVelocity = float.Parse(xmlNode.GetAttribute("MaxHorizontalVelocity"));
            if (xmlNode.HasAttribute("MinVerticalVelocity"))
                minVerticalVelocity = float.Parse(xmlNode.GetAttribute("MinVerticalVelocity"));
            if (xmlNode.HasAttribute("MaxVerticalVelocity"))
                maxVerticalVelocity = float.Parse(xmlNode.GetAttribute("MaxVerticalVelocity"));
            if (xmlNode.HasAttribute("Gravity"))
                gravity = Vector3Helper.FromString(xmlNode.GetAttribute("Gravity"));
            if (xmlNode.HasAttribute("EndVelocity"))
                endVelocity = float.Parse(xmlNode.GetAttribute("EndVelocity"));
            if (xmlNode.HasAttribute("MinColor"))
                minColor = ColorHelper.FromString(xmlNode.GetAttribute("MinColor"));
            if (xmlNode.HasAttribute("MaxColor"))
                maxColor = ColorHelper.FromString(xmlNode.GetAttribute("MaxColor"));
            if (xmlNode.HasAttribute("MinRotateSpeed"))
                minRotateSpeed = float.Parse(xmlNode.GetAttribute("MinRotateSpeed"));
            if (xmlNode.HasAttribute("MaxRotateSpeed"))
                maxRotateSpeed = float.Parse(xmlNode.GetAttribute("MaxRotateSpeed"));
            if (xmlNode.HasAttribute("MinStartSize"))
                minStartSize = float.Parse(xmlNode.GetAttribute("MinStartSize"));
            if (xmlNode.HasAttribute("MaxStartSize"))
                maxStartSize = float.Parse(xmlNode.GetAttribute("MaxStartSize"));
            if (xmlNode.HasAttribute("MinEndSize"))
                minEndSize = float.Parse(xmlNode.GetAttribute("MinEndSize"));
            if (xmlNode.HasAttribute("MaxEndSize"))
                maxEndSize = float.Parse(xmlNode.GetAttribute("MaxEndSize"));
            if (xmlNode.HasAttribute("BlendState"))
                blendState = (BlendState)Enum.Parse(typeof(BlendState), xmlNode.GetAttribute("BlendState"));
            if (xmlNode.HasAttribute("DrawOrder"))
                drawOrder = int.Parse(xmlNode.GetAttribute("DrawOrder"));
            if (xmlNode.HasAttribute("Shader"))
                shader = (IShader)Activator.CreateInstance(Type.GetType(xmlNode.GetAttribute("Shader")));
            if (xmlNode.HasAttribute("ShaderTechnique"))
                shaderTechnique = xmlNode.GetAttribute("ShaderTechnique");
        }

        #endregion

        #region IComparable<int> Members

        public int CompareTo(ParticleEffect other)
        {
            if (this.DrawOrder > other.DrawOrder)
                return 1;
            else if (this.DrawOrder == other.DrawOrder)
                return 0;
            else
                return -1;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            vertexBuffer.Dispose();
            particles = null;
        }

        #endregion
    }
}

