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
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;

namespace GoblinXNA.Physics
{
    #region Enums
    /// <summary>
    /// An enum that describes the shape of the physics object. 
    /// </summary>
    public enum ShapeType 
    { 
        Box, 
        Cylinder, 
        Sphere, 
        Cone, 
        Capsule, 
        ChamferCylinder,
        /// <summary>
        /// Compound of spherical shapes.
        /// </summary>
        Compound, 
        ConvexHull,
        TriangleMesh,
        /// <summary>
        /// A shape that is none of the above
        /// </summary>
        Extra
    }
    #endregion

    /// <summary>
    /// An interface that defines the physical properties of an object that can be added to a physics engine.
    /// </summary>
    public interface IPhysicsObject
    {
        #region Properties

        /// <summary>
        /// Gets or sets the container that holds this IPhysicsObject.
        /// </summary>
        Object Container { get; set; }

        /// <summary>
        /// Gets or sets the collision group ID.
        /// </summary>
        int CollisionGroupID { get; set; }

        /// <summary>
        /// Gets or sets the material name that defines this physical object such as
        /// Wood, Metal, Plastic, etc.
        /// </summary>
        String MaterialName { get; set; }

        /// <summary>
        /// Gets or sets the mass of this object.
        /// </summary>
        float Mass { get; set; }

        /// <summary>
        /// Gers or sets the relative position of the default center of mass, which is at (0, 0, 0). 
        /// </summary>
        /// <remarks>
        /// This value can be used to set the relative offset of the center of mass of a rigid body. 
        /// When a rigid body is created the center of mass is set the the point (0, 0, 0), and normally 
        /// this is the best setting for a rigid body. However the are situations in which and object does 
        /// not have symmetry or simple some kind of special effect is desired, and this origin need to be 
        /// changed.
        /// </remarks>
        Vector3 CenterOfMass { get; set; }

        /// <summary>
        /// The shape of this object. This information is required for proper physical interaction.
        /// Note that not all physics engine will support all shape types listed, and each
        /// of them will probably have different restrictions.
        /// </summary>
        ShapeType Shape { get; set; }

        /// <summary>
        /// Gets or sets the moment of inertia of this object. If not set, we will automatically 
        /// calculate it.
        /// </summary>
        Vector3 MomentOfInertia { get; set; }

        /// <summary>
        /// Gets or sets the data associated with each shape type.
        /// Box: dimensionX, dimensionY, dimensionZ;
        /// Sphere: radiusX, radiusY, radiusZ, or simply one radius;
        /// Cone, Capsule, Cylinder, and ChamferCylinder: radius, height.
        /// If this shape data is not specified, then the minimum bounding box is used to
        /// estimate the shape information.
        /// </summary>
        /// <remarks>
        /// This shape data is not used for ConvexHull or Vehicle type.
        /// </remarks>
        List<float> ShapeData { get; set; }

        /// <summary>
        /// Gets or sets the actual geometry model associated with this physics object.
        /// </summary>
        IModel Model { get; set; }

        /// <summary>
        /// Gets or sets the mesh provider for physics simulation.
        /// </summary>
        IPhysicsMeshProvider MeshProvider { get; set; }

        /// <summary>
        /// Gets or sets whether this object can be picked.
        /// </summary>
        bool Pickable { get; set; }

        /// <summary>
        /// Gets or sets whether this object can collide with other collidable scene nodes.
        /// </summary>
        bool Collidable { get; set; }

        /// <summary>
        /// Gets or sets whether this object reacts in response to physical simulation.
        /// </summary>
        bool Interactable { get; set; }

        /// <summary>
        /// Gets or sets whether gravity should be applied to this object.
        /// </summary>
        bool ApplyGravity { get; set; }

        /// <summary>
        /// Gets or sets whether this physics object's physics world transformation can be manipulated 
        /// directly instead of only using the physics world transformation calculated by the physics
        /// engine after simulation.
        /// </summary>
        bool Manipulatable { get; set; }

        /// <summary>
        /// Some physics engines deactivate an object whose bounding volume does not intersect
        /// with any active objects, such that forces cannot be applied to the deactivated object.
        /// This property indicates that, we want to make the object never be deactivated.
        /// </summary>
        bool NeverDeactivate { get; set; }

        /// <summary>
        /// Indicates whether any of the physics properties have been modified.
        /// </summary>
        bool Modified { get; set; }

        /// <summary>
        /// Indicates whether the model shape has been changed.
        /// </summary>
        bool ShapeModified { get; set; }

        /// <summary>
        /// Gets or sets the world transform retrieved from the physics engine after simulation.
        /// </summary>
        Matrix PhysicsWorldTransform { get; set; }

        /// <summary>
        /// Gets the compound initial world transform with the InitialWorldTransform
        /// and its parent world transform
        /// </summary>
        Matrix CompoundInitialWorldTransform { get; set; }

        /// <summary>
        /// Gets the initial world transform of this physics object when added to the physics engine.
        /// </summary>
        //Matrix InitialWorldTransform { get; set; }

        /// <summary>
        /// Gets or sets the initial linear velocity of this physics object.
        /// </summary>
        Vector3 InitialLinearVelocity { get; set; }

        /// <summary>
        /// Gets or sets the initial angular velocity of this physics object.
        /// </summary>
        Vector3 InitialAngularVelocity { get; set; }

        /// <summary>
        /// Gets or sets the linear damping coefficient.
        /// </summary>
        float LinearDamping { get; set; }

        /// <summary>
        /// Gets or sets the angular damping coefficient.
        /// </summary>
        Vector3 AngularDamping { get; set; }

        #endregion

        #region Methods

#if !WINDOWS_PHONE
        /// <summary>
        /// Saves the information of this physics object to an XML element.
        /// </summary>
        /// <param name="xmlDoc">The XML document to be saved.</param>
        /// <returns></returns>
        XmlElement Save(XmlDocument xmlDoc);

        /// <summary>
        /// Loads the information of this physics object from an XML element.
        /// </summary>
        /// <param name="xmlNode"></param>
        void Load(XmlElement xmlNode);
#endif

        #endregion
    }
}
