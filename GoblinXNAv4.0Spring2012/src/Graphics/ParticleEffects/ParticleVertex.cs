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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
#endregion

namespace GoblinXNA.Graphics.ParticleEffects
{
    /// <summary>
    /// Custom vertex structure for drawing point sprite particles.
    /// </summary>
    public struct ParticleVertex
    {
        /// <summary>
        /// Stores which corner of the particle quad this vertex represents.
        /// </summary>
        public Short2 Corner;

        /// <summary>
        /// Stores the starting position of the particle.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Stores the starting velocity of the particle.
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// Four random values, used to make each particle look slightly different.
        /// </summary>
        public Color Random;

        /// <summary>
        /// The time (in seconds) at that this particle was created.
        /// </summary>
        public float Time;

        // Describe the layout of this vertex structure.
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Short2,
                                 VertexElementUsage.Position, 0),

            new VertexElement(4, VertexElementFormat.Vector3,
                                 VertexElementUsage.Position, 1),

            new VertexElement(16, VertexElementFormat.Vector3,
                                  VertexElementUsage.Normal, 0),

            new VertexElement(28, VertexElementFormat.Color,
                                  VertexElementUsage.Color, 0),

            new VertexElement(32, VertexElementFormat.Single,
                                  VertexElementUsage.TextureCoordinate, 0)
        );


        /// <summary>
        /// Describe the size of this vertex structure.
        /// </summary>
        public const int SizeInBytes = 36;
    }
}

