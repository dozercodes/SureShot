/************************************************************************************ 
 * Copyright (c) 2008-2011, Columbia University
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
using System.Text;
using System.Collections;

using Microsoft.Xna.Framework;

namespace GoblinXNA.Physics
{
    /// <summary>
    /// An interface class that defines the properties and methods required for a physics engine.
    /// </summary>
    public interface IPhysics
    {
        /// <summary>
        /// Gets or sets the gravity value used for this physics simulation.
        /// The default value is 9.8f
        /// </summary>
        float Gravity { get; set; }

        /// <summary>
        /// Gets or sets the direction of gravity used for this physics simulation.
        /// The default value is (0, -1, 0).
        /// </summary>
        Vector3 GravityDirection { get; set; }

        /// <summary>
        /// Initializes the physics engine for physical simulation.
        /// </summary>
        void InitializePhysics();

        /// <summary>
        /// Restarts the simulation from its initial state.
        /// </summary>
        void RestartsSimulation();

        /// <summary>
        /// Adds a physics object to this physics engine for simulation.
        /// </summary>
        /// <param name="physObj">A physics object to be added</param>
        void AddPhysicsObject(IPhysicsObject physObj);

        /// <summary>
        /// Modifies an existing physics object's physical properties and transformation.
        /// </summary>
        /// <param name="physObj">A physics object to be modified</param>
        /// <param name="newTransform">A new transformation of this physics object
        /// (e.g. physObj.InitialWorldTransform)</param>
        void ModifyPhysicsObject(IPhysicsObject physObj, Matrix newTransform);

        /// <summary>
        /// Removes an existing physics object.
        /// </summary>
        /// <param name="physObj">A physics object to be removed</param>
        void RemovePhysicsObject(IPhysicsObject physObj);

        /// <summary>
        /// Gets the axis-aligned bounding box of this physics object used in the engine.
        /// </summary>
        /// <param name="physObj">A physics object to get the axis aligned bounding box</param>
        /// <returns></returns>
        BoundingBox GetAxisAlignedBoundingBox(IPhysicsObject physObj);

        /// <summary>
        /// Gets the actual mesh used for the collision detection in a list of polygon vertices.
        /// </summary>
        /// <param name="physObj">A physics object to get the collision mesh</param>
        /// <returns>A list of polygon vertices (the internal list consists polygon vertices)</returns>
        List<List<Vector3>> GetCollisionMesh(IPhysicsObject physObj);

        /// <summary>
        /// Updates the physical simulation.
        /// </summary>
        /// <param name="elapsedTime">The amount of time to proceed the simulation</param>
        void Update(float elapsedTime);

        /// <summary>
        /// Disposes of the physics engine.
        /// </summary>
        void Dispose();
    }
}
