/************************************************************************************ 
 *
 * Microsoft XNA Community Game Platform
 * Copyright (C) Microsoft Corporation. All rights reserved.
 * 
 * ===================================================================================
 * Modified by: Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/


using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.Graphics.ParticleEffects
{
    /// <summary>
    /// A particle system that simulates an explosion effect.
    /// </summary>
    public class ExplosionParticleEffect : ParticleEffect
    {
        public ExplosionParticleEffect() : this(100) { }

        /// <summary>
        /// Creates an explosion particle system.
        /// </summary>
        public ExplosionParticleEffect(int maxParticles)
            : base(maxParticles)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            textureName = "explosion";

            duration = TimeSpan.FromSeconds(2);
            durationRandomness = 1;

            minHorizontalVelocity = 20;
            maxHorizontalVelocity = 30;

            minVerticalVelocity = -20;
            maxVerticalVelocity = 20;

            endVelocity = 0;

            minColor = Color.DarkGray;
            maxColor = Color.Gray;

            minRotateSpeed = -1;
            maxRotateSpeed = 1;

            minStartSize = 10;
            maxStartSize = 10;

            minEndSize = 100;
            maxEndSize = 200;

            // Use additive blending.
            blendState = BlendState.Additive;
        }
    }
}
