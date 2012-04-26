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

using Microsoft.Xna.Framework;

namespace GoblinXNA.Physics.Newton1
{
    /// <summary>
    /// A default implementation of the IPhysicsMaterial interface. A physics material defines
    /// how two materials behave when they physically interact, including support for friction 
    /// and elasticity. 
    /// </summary>
    public class NewtonMaterial
    {
        #region Delegates
        /// <summary>
        /// A delegate/callback function that defines what to do when two physics objects 
        /// begin to contact.
        /// </summary>
        /// <param name="physObj1">One of the collided pair</param>
        /// <param name="physObj2">The other of the collided pair</param>
        public delegate void ContactBegin(IPhysicsObject physObj1, IPhysicsObject physObj2);

        /// <summary>
        /// A delegate/callback function that defines what to do when the contact proceeds 
        /// between the two physics objects.
        /// </summary>
        /// <param name="contactPosition">The position of the contact between the two objects</param>
        /// <param name="contactNormal">The normal of the contact</param>
        /// <param name="contactSpeed">The speed of the contact</param>
        /// <param name="colObj1ContactTangentSpeed">One of the collided pair's (physObj1 returned
        /// from ContactBegin callback function) contact tangent speed.</param>
        /// <param name="colObj2ContactTangentSpeed">The other of the collided pair's (physObj2 returned
        /// from ContactBegin callback function) contact tangent speed.</param>
        /// <param name="colObj1ContactTangentDirection">One of the collided pair's (physObj1 returned
        /// from ContactBegin callback function) contact tangent direction.</param>
        /// <param name="colObj2ContactTangentDirection">The other of the collided pair's (physObj1 returned
        /// from ContactBegin callback function) contact tangent direction.</param>
        public delegate void ContactProcess(Vector3 contactPosition, Vector3 contactNormal, float contactSpeed,
            float colObj1ContactTangentSpeed, float colObj2ContactTangentSpeed,
            Vector3 colObj1ContactTangentDirection, Vector3 colObj2ContactTangentDirection);

        /// <summary>
        /// A delegate/callback function that defines what to do when the contact process ends.
        /// </summary>
        public delegate void ContactEnd();
        #endregion

        #region Member Fields
        protected String materialName1;
        protected String materialName2;
        protected bool collidable;
        protected float staticFriction;
        protected float kineticFriction;
        protected float softness;
        protected float elasticity;

        protected ContactBegin contactBeginCallback;
        protected ContactProcess contactProcessCallback;
        protected ContactEnd contactEndCallback;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a physics material that defines two materials behave when they physically
        /// interact.
        /// </summary>
        /// <remarks>
        /// 'materialName1' and 'materialName2' can be the same if you want to define how
        /// the same material behave when they physically interact.
        /// </remarks>
        /// <param name="materialName1">The first material name</param>
        /// <param name="materialName2">The second material name</param>
        /// <param name="collidable">Whether these two materials are collidable</param>
        /// <param name="staticFriction">The static friction coefficient</param>
        /// <param name="kineticFriction">The kinetic friction coefficient</param>
        /// <param name="softness">The softness of the material. Softness defines how fiercely
        /// the two materials will repel from each other when the two materials penetrate
        /// against each other</param>
        /// <param name="elasticity">The elasticity coefficient</param>
        /// <param name="contactBeginCallback">The callback function when two materials
        /// begin to contact. Set to 'null' if you don't need this callback function.</param>
        /// <param name="contactProcessCallback">The callback function when two materials
        /// proceed contact. Set to 'null' if you don't need this callback function.</param>
        /// <param name="contactEndCallback">The callback function when two materials
        /// end contact. Set to 'null' if you don't need this callback function.</param>
        public NewtonMaterial(String materialName1, String materialName2, bool collidable, 
            float staticFriction, float kineticFriction, float softness, float elasticity,
            ContactBegin contactBeginCallback, ContactProcess contactProcessCallback,
            ContactEnd contactEndCallback)
        {
            this.materialName1 = materialName1;
            this.materialName2 = materialName2;
            this.collidable = collidable;
            this.staticFriction = staticFriction;
            this.kineticFriction = kineticFriction;
            this.elasticity = elasticity;
            this.softness = softness;
            this.contactBeginCallback = contactBeginCallback;
            this.contactProcessCallback = contactProcessCallback;
            this.contactEndCallback = contactEndCallback;
        }

        /// <summary>
        /// Creates a physics material with empty material names and no callback functions.
        /// </summary>
        public NewtonMaterial()
            : this("", "", true, -1, -1, -1, -1, null, null, null)
        {
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the first material name
        /// </summary>
        public String MaterialName1
        {
            get { return materialName1; }
            set { materialName1 = value; }
        }

        /// <summary>
        /// Gets or sets the second material name
        /// </summary>
        public String MaterialName2
        {
            get { return materialName2; }
            set { materialName2 = value; }
        }

        /// <summary>
        /// Gets or sets whether these two materials can collide.
        /// </summary>
        public bool Collidable
        {
            get { return collidable; }
            set { collidable = value; }
        }

        /// <summary>
        /// Gets or sets the static friction between the two materials.
        /// </summary>
        public float StaticFriction
        {
            get { return staticFriction; }
            set { staticFriction = value; }
        }

        /// <summary>
        /// Gets or sets the kinetic/dynamic friction between the two materials.
        /// </summary>
        public float KineticFriction
        {
            get { return kineticFriction; }
            set { kineticFriction = value; }
        }

        /// <summary>
        /// Gets or sets the softness between the two materials. This property is used 
        /// only when the two objects interpenetrate. The larger the value, the more restoring 
        /// force is applied to the interpenetrating objects. Restoring force is a force 
        /// applied to make both interpenetrating objects push away from each other so that 
        /// they no longer interpenetrate.
        /// </summary>
        public float Softness
        {
            get { return softness; }
            set { softness = value; }
        }

        /// <summary>
        /// Gets or sets the elasticity between the two materials. 
        /// </summary>
        public float Elasticity
        {
            get { return elasticity; }
            set { elasticity = value; }
        }

        /// <summary>
        /// Gets or sets the delegate/callback function called when contact begins 
        /// between two materials.
        /// </summary>
        public ContactBegin ContactBeginCallback
        {
            get { return contactBeginCallback; }
            set { contactBeginCallback = value; }
        }

        /// <summary>
        /// Gets or sets the delegate/callback function called when contact proceeds 
        /// between two materials.
        /// </summary>
        public ContactProcess ContactProcessCallback
        {
            get { return contactProcessCallback; }
            set { contactProcessCallback = value; }
        }

        /// <summary>
        /// Gets or sets the delegate/callback function called when contact ends 
        /// between two materials.
        /// </summary>
        public ContactEnd ContactEndCallback
        {
            get { return contactEndCallback; }
            set { contactEndCallback = value; }
        }
        #endregion
    }
}
