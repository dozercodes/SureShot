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

using NewtonDynamics;

namespace GoblinXNA.Physics.Newton1
{
    /// <summary>
    /// General joint information that applies to all of the joint types implemented in
    /// Newton dynamics library.
    /// </summary>
    public class JointInfo
    {
        #region Member Fields
        protected bool enableCollision;
        protected float stiffness;
        #endregion

        #region Constructors
        public JointInfo()
        {
            enableCollision = false;
            stiffness = 0.9f;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Enable or disable collision between the two bodies linked by this joint. Defalut
        /// value is false.
        /// </summary>
        /// <remarks>
        /// Usually when two bodies are linked by a joint, the application wants collision between 
        /// this two bodies to be disabled. This is the default behavior of joints when they are 
        /// created, however when this behavior is not desired the application can change it by 
        /// setting collision on. If the application decides to enable collision between jointed 
        /// bodies, the application should make sure the collision geometry do not collide in the 
        /// work space of the joint.
        /// </remarks>
        public bool EnableCollision
        {
            get { return enableCollision; }
            set { enableCollision = value; }
        }

        /// <summary>
        /// Gets or sets the strength coeficient to be applied to the joint reaction forces. 
        /// Default value is 0.9f, and must be between 0.0 and 1.0.
        /// </summary>
        /// <remarks>
        /// Constraint keep bodies together by calculating the exact force necessary to cancel 
        /// the relative acceleration between one or more common points fixed in the two bodies. 
        /// The problem is that when the bodies drift apart due to numerical integration inaccuracies, 
        /// the reaction force work to pull eliminated the error but at the expense of adding extra 
        /// energy to the system, does violating the rule that constraint forces must be work less. 
        /// This is a inevitable situation and the only think we can do is to minimize the effect of 
        /// the extra energy by dampening the force by some amount. In essence the stiffness 
        /// coefficient tell Newton calculate the precise reaction force by only apply a fraction of 
        /// it to the joint point. And value of 1.0 will apply the exact force, and a value of zero 
        /// will apply only 10 percent. 
        /// 
        /// The stiffness is set to a all around value that work well for most situation, however 
        /// the application can play with these parameter to make finals adjustment. A high value 
        /// will make the joint stronger but more prompt to vibration of instability; a low value 
        /// will make the joint more stable but weaker.
        /// </remarks>
        public float Stiffness
        {
            get { return stiffness; }
            set 
            {
                if (stiffness < 0 || stiffness > 1)
                    throw new GoblinException("Stiffness has to be between 0.0f and 1.0f");

                stiffness = value; 
            }
        }
        #endregion
    }

    /// <summary>
    /// A joint info class that represents Newton's ball and socket joint interface.
    /// </summary>
    public class BallAndSocketJoint : JointInfo
    {
        #region Member Fields
        private Vector3 pivotPoint;
        private Vector3 pin;
        private float maxConeAngle;
        private float maxTwistAngle;
        private Newton.NewtonBall callback;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a ball and socket joint with the given 'pivot'.
        /// </summary>
        /// <param name="pivot">The origin of ball and socket in global space.</param>
        public BallAndSocketJoint(Vector3 pivot)
            : base()
        {
            pivotPoint = pivot;
            pin = new Vector3();
            maxConeAngle = 0;
            maxTwistAngle = 0;
        }
        #endregion

        #region Properties
        internal Vector3 Pivot
        {
            get { return pivotPoint; }
        }

        /// <summary>
        /// Sets a pointer to a unit vector defining the cone axis in global space. If this value
        /// is set to other than new Vector3(), the cone and twist limits of this ball and socket
        /// joint will be set. In that case, you need to set MaxConeAngle and MaxTwistAngle as 
        /// well.
        /// </summary>
        /// <see cref="MaxConeAngle"/>
        /// <see cref="MaxTwistAngle"/>
        public Vector3 Pin
        {
            internal get { return pin; }
            set { pin = value; }
        }

        /// <summary>
        /// Sets the max angle in radians the attached body is allow to swing relative to the 
        /// pin axis.  
        /// </summary>
        /// <remarks>
        /// A value of zero disable the cone limit. All non-zero values are clamped between 
        /// 5 degree and 175 degrees.
        /// </remarks>
        public float MaxConeAngle
        {
            internal get { return maxConeAngle; }
            set { maxConeAngle = value; }
        }

        /// <summary>
        /// Sets the kax angle in radians the attached body is allow to twist relative to 
        /// the pin axis.
        /// </summary>
        /// <remarks>
        /// A value of zero disable the twist limit.
        /// </remarks>
        public float MaxTwistAngle
        {
            internal get { return maxTwistAngle; }
            set { maxTwistAngle = value; }
        }

        /// <summary>
        /// Sets an update call back to be called when either of the two bodies linked by the joint 
        /// is active.
        /// </summary>
        /// <remarks>
        /// If the application wants to have some feedback from the joint simulation, the application 
        /// can register a function update callback to be called every time any of the bodies linked 
        /// by this joint is active. This is useful to provide special effects like particles, sound 
        /// or even to simulate breakable moving parts.
        /// </remarks>
        /// <example>
        /// <code>
        /// BallAndSocketJoint.NewtonBallCallback = delegate(IntPtr pNewtonBall)
        /// {
        ///     float[] force = new float[3];
        ///        if (newtonJoint != IntPtr.Zero)
        ///            Newton.NewtonBallGetJointForce(pNewtonBall, force);
        ///     
        ///     if(force[0] + force[1] + force[2] > 100)
        ///         Newton.NewtonDestroyJoint(NewtonPhysics.NewtonWorld, pNewtonBall);
        /// };
        /// </code>
        /// </example>
        public Newton.NewtonBall NewtonBallCallback
        {
            internal get { return callback; }
            set { callback = value; }
        }
        #endregion
    }

    /// <summary>
    /// A joint info class that represents Newton's hinge joint interface.
    /// </summary>
    public class HingeJoint : JointInfo
    {
        #region Member Fields
        private Vector3 pivotPoint;
        private Vector3 pinDir;
        private Newton.NewtonHinge callback;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a hinge joint with the given 'pivot' and 'pin'.
        /// </summary>
        /// <param name="pivot">The origin of the hinge in global space.</param>
        /// <param name="pin">The line of action of the hinge in global space.</param>
        public HingeJoint(Vector3 pivot, Vector3 pin)
            : base()
        {
            pivotPoint = pivot;
            pinDir = pin;
        }
        #endregion

        #region Properties
        internal Vector3 Pivot
        {
            get { return pivotPoint; }
        }

        internal Vector3 Pin
        {
            get { return pinDir; }
        }

        /// <summary>
        /// Set an update call back to be called when either of the two body linked by the joint 
        /// is active. 
        /// </summary>
        public Newton.NewtonHinge NewtonHingeCallback
        {
            internal get { return callback; }
            set { callback = value; }
        }
        #endregion
    }

    /// <summary>
    /// A joint info class that represents Newton's slider joint interface.
    /// </summary>
    public class SliderJoint : JointInfo
    {
        #region Member Fields
        private Vector3 pivotPoint;
        private Vector3 pinDir;
        private Newton.NewtonSlider callback;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a slider joint with the given 'pivot' and 'pin'.
        /// </summary>
        /// <param name="pivot">The origin of the hinge in global space.</param>
        /// <param name="pinDir">The line of action of the hinge in global space.</param>
        public SliderJoint(Vector3 pivot, Vector3 pinDir)
            : base()
        {
            pivotPoint = pivot;
            this.pinDir = pinDir;
        }
        #endregion

        #region Properties
        internal Vector3 Pivot
        {
            get { return pivotPoint; }
        }

        internal Vector3 Pin
        {
            get { return pinDir; }
        }

        /// <summary>
        /// Set an update call back to be called when either of the two body linked by the joint 
        /// is active. 
        /// </summary>
        public Newton.NewtonSlider NewtonSliderCallback
        {
            internal get { return callback; }
            set { callback = value; }
        }
        #endregion
    }

    /// <summary>
    /// A joint info class that represents Newton's corkscrew joint interface.
    /// </summary>
    public class CorkscrewJoint : JointInfo
    {
        #region Member Fields
        private Vector3 pivotPoint;
        private Vector3 pinDir;
        private Newton.NewtonCorkscrew callback;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a corkscrew joint with the given 'pivot' and 'pin'.
        /// </summary>
        /// <param name="pivot">The origin of the hinge in global space.</param>
        /// <param name="pinDir">The line of action of the hinge in global space.</param>
        public CorkscrewJoint(Vector3 pivot, Vector3 pinDir)
            : base()
        {
            pivotPoint = pivot;
            this.pinDir = pinDir;
        }
        #endregion

        #region Properties
        internal Vector3 Pivot
        {
            get { return pivotPoint; }
        }

        internal Vector3 Pin
        {
            get { return pinDir; }
        }

        /// <summary>
        /// Set an update call back to be called when either of the two body linked by the joint 
        /// is active. 
        /// </summary>
        public Newton.NewtonCorkscrew NewtonCorkscrewCallback
        {
            internal get { return callback; }
            set { callback = value; }
        }
        #endregion
    }

    /// <summary>
    /// A joint info class that represents Newton's universal joint interface.
    /// </summary>
    public class UniversalJoint : JointInfo
    {
        #region Member Fields
        private Vector3 pivotPoint;
        private Vector3 pinDir0;
        private Vector3 pinDir1;
        private Newton.NewtonUniversal callback;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a universal joint with the given 'pivot', 'pinDir0', and 'pinDir1'.
        /// </summary>
        /// <param name="pivot">The origin of the hinge in global space.</param>
        /// <param name="pinDir0">First axis of rotation fixed on childBody body and perpendicular 
        /// to pinDir1.</param>
        /// <param name="pinDir1">Second axis of rotation fixed on parentBody body and perpendicular 
        /// to pinDir0.</param>
        public UniversalJoint(Vector3 pivot, Vector3 pinDir0, Vector3 pinDir1)
            : base()
        {
            pivotPoint = pivot;
            this.pinDir0 = pinDir0;
            this.pinDir1 = pinDir1;
        }
        #endregion

        #region Properties
        internal Vector3 Pivot
        {
            get { return pivotPoint; }
        }

        internal Vector3 Pin0
        {
            get { return pinDir0; }
        }

        internal Vector3 Pin1
        {
            get { return pinDir1; }
        }

        /// <summary>
        /// Set an update call back to be called when either of the two body linked by the joint 
        /// is active. 
        /// </summary>
        public Newton.NewtonUniversal NewtonUniversalCallback
        {
            internal get { return callback; }
            set { callback = value; }
        }
        #endregion
    }

    /// <summary>
    /// A joint info class that represents Newton's up vector joint interface.
    /// </summary>
    public class UpVectorJoint : JointInfo
    {
        #region Member Fields
        private Vector3 pinDir;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a up vector joint with the given 'pin'.
        /// </summary>
        /// <remarks>
        /// The child body is used, but the parent body is not used for this joint. 
        /// </remarks>
        /// <param name="pinDir">The aligning vector.</param>
        public UpVectorJoint(Vector3 pinDir)
            : base()
        {
            this.pinDir = pinDir;
        }
        #endregion

        #region Properties
        internal Vector3 Pin
        {
            get { return pinDir; }
        }
        #endregion
    }


}
