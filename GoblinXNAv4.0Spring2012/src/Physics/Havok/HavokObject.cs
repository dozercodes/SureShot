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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using GoblinXNA.Physics;
using GoblinXNA.Helpers;
using System.Xml;

namespace GoblinXNA.Physics.Havok
{
    /// <summary>
    /// An extension of the default PhysicsObject that contains additional properties
    /// specific to Havok physics library. Please see Havok documentation for details of each
    /// additional property.
    /// </summary>
    public class HavokObject : PhysicsObject
    {
        #region Member Fields

        private HavokPhysics.MotionType motionType;
        private HavokPhysics.CollidableQualityType qualityType;
        private float friction;
        private float restitution;
        private float allowedPenetrationDepth;
        private float maxLinearVelocity;
        private float maxAngularVelocity;
        private float convexRadius;
        private float gravityFactor;
        private bool isPhantom;

        private HavokDllBridge.ContactCallback contactCallback;
        private HavokDllBridge.CollisionStarted collisionStartCallback;
        private HavokDllBridge.CollisionEnded collisionEndCallback;

        private HavokDllBridge.PhantomEnterCallback phantomEnterCallback;
        private HavokDllBridge.PhantomLeaveCallback phantomLeaveCallback;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a physics object with Havok specific properties.
        /// </summary>
        /// <param name="container">The container of this physics object.</param>
        public HavokObject(object container) : base(container)
        {
            motionType = HavokPhysics.MotionType.MOTION_BOX_INERTIA;
            qualityType = HavokPhysics.CollidableQualityType.COLLIDABLE_QUALITY_INVALID;
            mass = 0;
            restitution = -1;
            friction = -1;
            allowedPenetrationDepth = -1;
            maxLinearVelocity = -1;
            maxAngularVelocity = -1;
            convexRadius = 0.05f;
            gravityFactor = 1;

            isPhantom = false;
        }

        public HavokObject() : this(null) { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the motion type of this physics object. Default value is
        /// HavokPhysics.MotionType.MOTION_BOX_INERTIA.
        /// </summary>
        public HavokPhysics.MotionType MotionType
        {
            get { return motionType; }
            set 
            { 
                motionType = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the collision quality type of this physics object. Default value is
        /// HavokPhysics.CollidableQualityType.COLLIDABLE_QUALITY_INVALID.
        /// </summary>
        public HavokPhysics.CollidableQualityType QualityType
        {
            get { return qualityType; }
            set 
            { 
                qualityType = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the friction of this physics object.
        /// </summary>
        public float Friction
        {
            get { return friction; }
            set 
            { 
                friction = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the restitution of this physics object.
        /// </summary>
        public float Restitution
        {
            get { return restitution; }
            set 
            { 
                restitution = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed penetration depth for this physics object.
        /// </summary>
        public float AllowedPenetrationDepth
        {
            get { return allowedPenetrationDepth; }
            set 
            { 
                allowedPenetrationDepth = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the maximum linear velocity this physics object can have.
        /// </summary>
        public float MaxLinearVelocity
        {
            get { return maxLinearVelocity; }
            set 
            {
                maxLinearVelocity = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the maximum angular velocity this physics object can have.
        /// </summary>
        public float MaxAngularVelocity
        {
            get { return maxAngularVelocity; }
            set 
            { 
                maxAngularVelocity = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the convex radius of this physics object. Default value is 0.05.
        /// </summary>
        public float ConvexRadius
        {
            get { return convexRadius; }
            set 
            { 
                convexRadius = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the factor multiplied to the gravity applied to this physics object.
        /// Default value is 1.
        /// </summary>
        public float GravityFactor
        {
            get { return gravityFactor; }
            set 
            { 
                gravityFactor = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets whether this physics object will be treated as a phantom object.
        /// Default value is false.
        /// </summary>
        public bool IsPhantom
        {
            get { return isPhantom; }
            set { isPhantom = value; }
        }

        /// <summary>
        /// Gets or sets the callback function when there is a contact with other physics objects.
        /// </summary>
        public HavokDllBridge.ContactCallback ContactCallback
        {
            get { return contactCallback; }
            set { contactCallback = value; }
        }

        /// <summary>
        /// Gets or sets the callback function when a collision between other physics objects starts.
        /// </summary>
        public HavokDllBridge.CollisionStarted CollisionStartCallback
        {
            get { return collisionStartCallback; }
            set { collisionStartCallback = value; }
        }

        /// <summary>
        /// Gets or sets the callback function when a collision between other physics objects ends.
        /// </summary>
        public HavokDllBridge.CollisionEnded CollisionEndCallback
        {
            get { return collisionEndCallback; }
            set { collisionEndCallback = value; }
        }

        /// <summary>
        /// Gets or sets the callback function when a physics object enters this phantom object.
        /// Effective only IsPhantom is set to true.
        /// </summary>
        /// <see cref="IsPhantom"/>
        public HavokDllBridge.PhantomEnterCallback PhantomEnterCallback
        {
            get { return phantomEnterCallback; }
            set { phantomEnterCallback = value; }
        }

        /// <summary>
        /// Gets or sets the callback function when a physics object leaves this phantom object.
        /// Effective only IsPhantom is set to true.
        /// </summary>
        /// <see cref="IsPhantom"/>
        public HavokDllBridge.PhantomLeaveCallback PhantomLeaveCallback
        {
            get { return phantomLeaveCallback; }
            set { phantomLeaveCallback = value; }
        }

        #endregion

        #region Overriden Properties

        public override bool ApplyGravity
        {
            get
            {
                return base.ApplyGravity;
            }
            set
            {
                base.ApplyGravity = value;
                if (!value)
                    gravityFactor = 0;
            }
        }

        public override bool Manipulatable
        {
            get
            {
                return base.Manipulatable;
            }
            set
            {
                base.Manipulatable = value;
                if (value)
                    motionType = HavokPhysics.MotionType.MOTION_KEYFRAMED;
            }
        }

        #endregion

        #region Overriden Methods

        public override XmlElement Save(XmlDocument xmlDoc)
        {
            XmlElement xmlNode = base.Save(xmlDoc);

            xmlNode.SetAttribute("MotionType", motionType.ToString());
            xmlNode.SetAttribute("QualityType", qualityType.ToString());
            xmlNode.SetAttribute("Friction", friction.ToString());
            xmlNode.SetAttribute("Restitution", restitution.ToString());
            xmlNode.SetAttribute("AllowedPenetrationDepth", allowedPenetrationDepth.ToString());
            xmlNode.SetAttribute("MaxLinearVelocity", maxLinearVelocity.ToString());
            xmlNode.SetAttribute("MaxAngularVelocity", maxAngularVelocity.ToString());
            xmlNode.SetAttribute("ConvexRadius", convexRadius.ToString());
            xmlNode.SetAttribute("GravityFactor", gravityFactor.ToString());
            xmlNode.SetAttribute("IsPhantom", isPhantom.ToString());

            if (contactCallback != null)
                xmlNode.SetAttribute("ContactCallback", contactCallback.Method.Name);
            if (collisionStartCallback != null)
                xmlNode.SetAttribute("CollisionStartCallback", collisionStartCallback.Method.Name);
            if (collisionEndCallback != null)
                xmlNode.SetAttribute("CollisionEndCallback", collisionEndCallback.Method.Name);
            if (phantomEnterCallback != null)
                xmlNode.SetAttribute("PhantomEnterCallback", phantomEnterCallback.Method.Name);
            if (phantomLeaveCallback != null)
                xmlNode.SetAttribute("PhantomEndCallback", phantomEnterCallback.Method.Name);

            return xmlNode;
        }

        public override void Load(XmlElement xmlNode)
        {
            base.Load(xmlNode);

            if (xmlNode.HasAttribute("MotionType"))
                motionType = (HavokPhysics.MotionType)Enum.Parse(typeof(HavokPhysics.MotionType),
                    xmlNode.GetAttribute("MotionType"));
            if (xmlNode.HasAttribute("QualityType"))
                qualityType = (HavokPhysics.CollidableQualityType)Enum.Parse(
                    typeof(HavokPhysics.CollidableQualityType), xmlNode.GetAttribute("QualityType"));
            if (xmlNode.HasAttribute("Friction"))
                friction = float.Parse(xmlNode.GetAttribute("Friction"));
            if (xmlNode.HasAttribute("Restitution"))
                restitution = float.Parse(xmlNode.GetAttribute("Restitution"));
            if (xmlNode.HasAttribute("AllowedPenetrationDepth"))
                allowedPenetrationDepth = float.Parse(xmlNode.GetAttribute("AllowedPenetrationDepth"));
            if (xmlNode.HasAttribute("MaxLinearVelocity"))
                maxLinearVelocity = float.Parse(xmlNode.GetAttribute("MaxLinearVelocity"));
            if (xmlNode.HasAttribute("MaxAngularVelocity"))
                maxAngularVelocity = float.Parse(xmlNode.GetAttribute("MaxAngularVelocity"));
            if (xmlNode.HasAttribute("ConvexRadius"))
                convexRadius = float.Parse(xmlNode.GetAttribute("ConvexRadius"));
            if (xmlNode.HasAttribute("GravityFactor"))
                gravityFactor = float.Parse(xmlNode.GetAttribute("GravityFactor"));
            if (xmlNode.HasAttribute("IsPhantom"))
                isPhantom = bool.Parse(xmlNode.GetAttribute("IsPhantom"));
        }

        #endregion
    }
}
