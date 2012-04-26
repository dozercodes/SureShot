/************************************************************************************ 
 *
 * Microsoft XNA Community Game Platform
 * Copyright (C) Microsoft Corporation. All rights reserved.
 * 
 *************************************************************************************/ 
 

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace GoblinXNA.Sounds
{
    /// <summary>
    /// Interface used by the AudioManager to look up the position
    /// and velocity of entities that can emit 3D sounds.
    /// </summary>
    public interface IAudioEmitter
    {
        /// <summary>
        /// Gets the position of the sound
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Gets the forward direction of the sound (if sound source is moving)
        /// </summary>
        Vector3 Forward { get; }

        /// <summary>
        /// Gets the up vector of the sound
        /// </summary>
        Vector3 Up { get; }

        /// <summary>
        /// Gets the velocity of the sound (if sound source is moving)
        /// </summary>
        Vector3 Velocity { get; }
    }
}
