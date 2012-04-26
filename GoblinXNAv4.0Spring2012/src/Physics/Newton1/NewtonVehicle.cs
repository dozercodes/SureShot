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
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;

using NewtonDynamics;

using GoblinXNA.Graphics;
using GoblinXNA.Helpers;

namespace GoblinXNA.Physics.Newton1
{
    #region Enums
    public enum TireID
    {
        FrontLeft,
        FrontRight,
        RearLeft,
        RearRight
    }
    #endregion

    public abstract class NewtonVehicle : IPhysicsObject
    {
        #region Member Fields
        protected Object container;
        protected int collisionGroupID;
        protected String materialName;
        protected float mass;
        protected Vector3 centerOfMass;
        protected ShapeType shape;
        protected List<float> shapeData;
        protected Vector3 momentOfInertia;
        protected bool pickable;
        protected bool collidable;
        protected bool interactable;
        protected bool applyGravity;
        protected bool manipulatable;
        protected bool neverDeactivate;
        protected bool modified;
        protected bool shapeModified;
        protected Matrix physicsWorldTransform;
        protected Matrix initialWorldTransform;
        protected Matrix compoundInitialWorldTransform;
        protected Vector3 initialLinearVelocity;
        protected Vector3 initialAngularVelocity;
        protected float linearDamping;
        protected Vector3 angularDamping;

        protected NewtonTire[] tires;
        protected Dictionary<IntPtr, TireID> tireTable;

        protected IModel model;
        protected IPhysicsMeshProvider meshProvider;

        protected IntPtr joint;
        protected NewtonDynamics.Newton.NewtonSetTransform transformCallback;
        protected NewtonDynamics.Newton.NewtonApplyForceAndTorque forceCallback;
        protected NewtonDynamics.Newton.NewtonVehicleTireUpdate tireUpdate;
        #endregion

        #region Constructors

        public NewtonVehicle(Object container)
        {
            this.container = container;
            collisionGroupID = 0;
            materialName = "";
            mass = 1.0f;
            centerOfMass = new Vector3();

            shape = ShapeType.ConvexHull;
            shapeData = new List<float>();
            momentOfInertia = new Vector3();
            pickable = false;
            collidable = false;
            interactable = false;
            manipulatable = false;
            applyGravity = true;
            neverDeactivate = false;
            modified = false;
            shapeModified = false;

            physicsWorldTransform = Matrix.Identity;
            initialWorldTransform = Matrix.Identity;
            compoundInitialWorldTransform = Matrix.Identity;

            initialLinearVelocity = new Vector3();
            initialAngularVelocity = new Vector3();

            linearDamping = 0.0f;
            angularDamping = -Vector3.One;

            tires = new NewtonTire[4];
            for(int i = 0; i < tires.Length; i++)
                tires[i] = null;
            tireTable = new Dictionary<IntPtr, TireID>();

            joint = IntPtr.Zero;
            transformCallback = null;
            forceCallback = null;
            tireUpdate = null;
        }

        public NewtonVehicle() : this(null) { }

        #endregion

        #region Properties

        public IModel Model
        {
            get { return model; }
            set
            {
                model = value;
                modified = true;
                shapeModified = true;
            }
        }

        public IPhysicsMeshProvider MeshProvider
        {
            get { return meshProvider; }
            set 
            {
                meshProvider = value;
                modified = true;
                shapeModified = true;
            }
        }

        public Object Container
        {
            get { return container; }
            set { container = value; }
        }

        public int CollisionGroupID
        {
            get { return collisionGroupID; }
            set { collisionGroupID = value; }
        }

        public String MaterialName
        {
            get { return materialName; }
            set { materialName = value; }
        }

        public float Mass
        {
            get { return mass; }
            set
            {
                mass = value;
                modified = true;
            }
        }

        public Vector3 CenterOfMass
        {
            get { return centerOfMass; }
            set
            {
                centerOfMass = value;
                modified = true;
            }
        }

        public ShapeType Shape
        {
            get { return shape; }
            set { shape = value; }
        }

        public List<float> ShapeData
        {
            get { return shapeData; }
            set { shapeData = value; }
        }

        public Vector3 MomentOfInertia
        {
            get { return momentOfInertia; }
            set
            {
                momentOfInertia = value;
                modified = true;
            }
        }

        public bool Pickable
        {
            get { return pickable; }
            set
            {
                pickable = value;
                modified = true;
            }
        }

        public bool Collidable
        {
            get { return collidable; }
            set
            {
                collidable = value;
                modified = true;
            }
        }

        public bool Interactable
        {
            get { return interactable; }
            set
            {
                interactable = value;
                modified = true;
            }
        }

        public bool Manipulatable
        {
            get { return manipulatable; }
            set { manipulatable = value; }
        }

        public bool ApplyGravity
        {
            get { return applyGravity; }
            set { applyGravity = value; }
        }

        public bool NeverDeactivate
        {
            get { return neverDeactivate; }
            set
            {
                neverDeactivate = value;
                modified = true;
            }
        }

        public bool Modified
        {
            get { return modified; }
            set { modified = value; }
        }

        public bool ShapeModified
        {
            get { return shapeModified; }
            set { shapeModified = value; }
        }

        public Matrix PhysicsWorldTransform
        {
            get { return physicsWorldTransform; }
            set { physicsWorldTransform = value; }
        }

        public Matrix CompoundInitialWorldTransform
        {
            get { return compoundInitialWorldTransform; }
            set { compoundInitialWorldTransform = value; }
        }

        public Matrix InitialWorldTransform
        {
            set
            {
                initialWorldTransform = MatrixHelper.CopyMatrix(value);
                modified = true;
            }
            get { return initialWorldTransform; }
        }

        public Vector3 InitialLinearVelocity
        {
            get { return initialLinearVelocity; }
            set
            {
                initialLinearVelocity = value;
                modified = true;
            }
        }

        public Vector3 InitialAngularVelocity
        {
            get { return initialAngularVelocity; }
            set
            {
                initialAngularVelocity = value;
                modified = true;
            }
        }

        public float LinearDamping
        {
            get { return linearDamping; }
            set
            {
                linearDamping = value;
                modified = true;
            }
        }

        public Vector3 AngularDamping
        {
            get { return angularDamping; }
            set
            {
                angularDamping = value;
                modified = true;
            }
        }

        public NewtonTire [] Tires
        {
            get { return tires; }
            set { tires = value; }
        }

        public IntPtr Joint
        {
            get { return joint; }
            internal set { joint = value; }
        }

        /// <summary>
        /// Gets or sets the transform callback for this vehicle. 
        /// </summary>
        /// <remarks>
        /// This callback must be set before a vehicle can be added for physical simulation.
        /// </remarks>
        public NewtonDynamics.Newton.NewtonSetTransform TransformCallback
        {
            get { return transformCallback; }
            set { transformCallback = value; }
        }

        /// <summary>
        /// Gets or sets the apply force and torque callback for this vehicle. 
        /// </summary>
        /// <remarks>
        /// This callback must be set before a vehicle can be added for physical simulation.
        /// </remarks>
        public NewtonDynamics.Newton.NewtonApplyForceAndTorque ForceCallback
        {
            get { return forceCallback; }
            set { forceCallback = value; }
        }

        /// <summary>
        /// Gets or sets the tire update callback for this vehicle. 
        /// </summary>
        /// <remarks>
        /// This callback must be set before this vehicle can be added for physical simulation.
        /// </remarks>
        public NewtonDynamics.Newton.NewtonVehicleTireUpdate TireUpdateCallback
        {
            get { return tireUpdate; }
            set { tireUpdate = value; }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Sets the steering parameter for the steerable tires. 
        /// </summary>
        /// <param name="value"></param>
        abstract public void SetSteering(float value);

        /// <summary>
        /// Sets the torque of the vehicle tires.
        /// </summary>
        /// <param name="value"></param>
        abstract public void SetTireTorque(float value);

        /// <summary>
        /// Applies hand brakes on the vehicle.
        /// </summary>
        /// <param name="value"></param>
        abstract public void ApplyHandBrakes(float value);

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the tire ID from the newton's registered tire ID (which is different from TireID).
        /// </summary>
        /// <param name="newtonTireID"></param>
        /// <returns></returns>
        public TireID GetTireID(IntPtr newtonTireID)
        {
            if (tireTable.ContainsKey(newtonTireID))
                return tireTable[newtonTireID];
            else
                return TireID.FrontLeft;
        }

        /// <summary>
        /// Associate newton's tire ID with our TireID.
        /// </summary>
        /// <param name="newtonTireID"></param>
        /// <param name="index"></param>
        internal void AddToTireTable(IntPtr newtonTireID, int index)
        {
            tireTable.Add(newtonTireID, (TireID)Enum.ToObject(typeof(TireID), index));
        }

        public virtual XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = xmlDoc.CreateElement(TypeDescriptor.GetClassName(this));

            xmlNode.SetAttribute("Pickable", pickable.ToString());
            xmlNode.SetAttribute("Collidable", collidable.ToString());
            xmlNode.SetAttribute("Interactable", interactable.ToString());
            xmlNode.SetAttribute("ApplyGravity", applyGravity.ToString());
            xmlNode.SetAttribute("Manipulatable", manipulatable.ToString());
            xmlNode.SetAttribute("NeverDeactivate", neverDeactivate.ToString());

            return xmlNode;
        }

        public virtual void Load(XmlElement xmlNode)
        {
        }

        #endregion
    }
}
