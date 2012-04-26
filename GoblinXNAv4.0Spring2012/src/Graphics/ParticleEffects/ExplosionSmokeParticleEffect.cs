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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;

namespace GoblinXNA.Graphics.ParticleEffects
{
    /// <summary>
    /// A particle system that simulates an explosion effect with smoke.
    /// </summary>
    public class ExplosionSmokeParticleEffect : ParticleEffect
    {
        /// <summary>
        /// Creates a particle system that simulates an explosion effect with smoke with 200 maximum particles.
        /// </summary>
        public ExplosionSmokeParticleEffect() : this(200) { }

        /// <summary>
        /// Creates a particle system that simulates an explosion effect with smoke.
        /// </summary>
        public ExplosionSmokeParticleEffect(int maxParticles)
            : base(maxParticles)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            textureName = "smoke";

            duration = TimeSpan.FromSeconds(4);

            minHorizontalVelocity = 0;
            maxHorizontalVelocity = 50;

            minVerticalVelocity = -10;
            maxVerticalVelocity = 50;

            gravity = Vector3Helper.Get(0, -20, 0);

            endVelocity = 0;

            minColor = Color.LightGray;
            maxColor = Color.White;

            minRotateSpeed = -2;
            maxRotateSpeed = 2;

            minStartSize = 10;
            maxStartSize = 10;

            minEndSize = 100;
            maxEndSize = 200;
        }
    }
}
