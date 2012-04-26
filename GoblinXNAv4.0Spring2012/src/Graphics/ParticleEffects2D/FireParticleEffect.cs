using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;

namespace GoblinXNA.Graphics.ParticleEffects2D
{
    public class FireParticleEffect : ParticleEffect
    {
        public FireParticleEffect(int howManyEffects, SpriteBatch spriteBatch)
            : this(howManyEffects, spriteBatch, false)
        {
        }

        public FireParticleEffect(int howManyEffects, SpriteBatch spriteBatch, bool computeIn3D)
            : base(howManyEffects, spriteBatch, computeIn3D)
        {
        }

        /// <summary>
        /// Set up the constants that will give this particle system its behavior and
        /// properties.
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "fire";

            minInitialSpeed = 10;
            maxInitialSpeed = 50;

            if (ComputeIn3D)
            {
                minInitialSpeed /= 10;
                maxInitialSpeed /= 10;
            }

            // doesn't matter what these values are set to, acceleration is tweaked in
            // the override of InitializeParticle.
            minAcceleration = 0;
            maxAcceleration = 0;

            minLifetime = 2.0f;
            maxLifetime = 3.5f;

            minScale = .1f;
            maxScale = .25f;

            // we need to reduce the number of particles on Windows Phone in order to keep
            // a good framerate
#if WINDOWS_PHONE
            minNumParticles = 4;
            maxNumParticles = 8;
#else
            minNumParticles = 20;
            maxNumParticles = 25;
#endif

            minRotationSpeed = -MathHelper.PiOver4;
            maxRotationSpeed = MathHelper.PiOver4;

            // additive blending is very good at creating fiery effects.
			blendState = BlendState.Additive;

            drawOrder = AdditiveDrawOrder;
        }

        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            base.InitializeParticle(p, where);

            p.Acceleration.Y -= RandomHelper.GetRandomFloat(5, 20);
        }

        protected override void InitializeParticle3D(Particle p, Vector3 where)
        {
            base.InitializeParticle3D(p, where);

            p.Acceleration3D.Y -= RandomHelper.GetRandomFloat(0.5f, 2.0f);
        }
    }
}
