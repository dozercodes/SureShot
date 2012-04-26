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
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;
using GoblinXNA.Device.Generic;
using Model = GoblinXNA.Graphics.Model;

using GoblinXNA;
using GoblinXNA.Physics;

namespace GoblinXNA.Physics.Havok
{
    /// <summary>
    /// An implementation of the IPhysics interface using the Havok physics library developed by
    /// Havok (http://www.havok.com/).
    /// </summary>
    public class HavokPhysics : IPhysics
    {
        #region Enums

        public enum SolverType
        {
            SOLVER_TYPE_INVALID,
            /// <summary>
            /// 'Softest' solver type.
            /// </summary>
            SOLVER_TYPE_2ITERS_SOFT,
            /// .
            SOLVER_TYPE_2ITERS_MEDIUM,
            /// .
            SOLVER_TYPE_2ITERS_HARD,
            /// .
            SOLVER_TYPE_4ITERS_SOFT,
            /// .
            SOLVER_TYPE_4ITERS_MEDIUM,
            /// .
            SOLVER_TYPE_4ITERS_HARD,
            /// .
            SOLVER_TYPE_8ITERS_SOFT,
            /// .
            SOLVER_TYPE_8ITERS_MEDIUM,
            /// <summary>
            /// 'Hardest' solver type.
            /// </summary>
            SOLVER_TYPE_8ITERS_HARD,
            ///
            SOLVER_TYPE_MAX_ID
        }

        public enum SimulationType
        {
            SIMULATION_TYPE_INVALID,

            /// <summary>
            /// No continuous simulation is performed
            /// </summary>
            SIMULATION_TYPE_DISCRETE,

            /// <summary>
            /// Use this simulation if you want any continuous simulation.
            /// Depending on the hkpEntity->getQualityType(), collisions
            /// are not only performed at 'normal' physical timesteps (called PSI), but
            /// at any time when two objects collide (TOI)
            /// </summary>
            SIMULATION_TYPE_CONTINUOUS,

            /// <summary>
            /// Multithreaded continuous simulation.
            /// You must have read the multi threading user guide.
            /// To use this you should call hkpWorld::stepMultithreaded(), see
            /// the hkDefaultPhysicsDemo::stepDemo for an example.
            /// Notes:
            ///   - The internal overhead for multi threaded is small and you can expect
            ///     good speedups, except:
            ///   - solving multiple TOI events can not be done on different threads,
            ///     so TOI are solved on a single thread. However the collision detection
            ///     for each TOI event can be solver multithreaded (see m_processToisMultithreaded)
            /// </summary>
            SIMULATION_TYPE_MULTITHREADED,
        };

        public enum MotionType
        {
            MOTION_INVALID,

            /// <summary>
            /// A fully-simulated, movable rigid body. At construction time the engine checks
            /// the input inertia and selects MOTION_SPHERE_INERTIA or MOTION_BOX_INERTIA as
            /// appropriate.
            /// </summary>
            MOTION_DYNAMIC,

            /// <summary>
            /// Simulation is performed using a sphere inertia tensor. (A multiple of the
            /// Identity matrix). The highest value of the diagonal of the rigid body's
            /// inertia tensor is used as the spherical inertia.
            /// </summary>
            MOTION_SPHERE_INERTIA,

            /// <summary>
            /// Simulation is performed using a box inertia tensor. The non-diagonal elements
            /// of the inertia tensor are set to zero. This is slower than the
            /// MOTION_SPHERE_INERTIA motions, however it can produce more accurate results,
            /// especially for long thin objects.
            /// </summary>
            MOTION_BOX_INERTIA,

            /// <summary>
            /// Simulation is not performed as a normal rigid body. During a simulation step,
            /// the velocity of the rigid body is used to calculate the new position of the
            /// rigid body, however the velocity is NOT updated. The user can keyframe a rigid
            /// body by setting the velocity of the rigid body to produce the desired keyframe
            /// positions. The hkpKeyFrameUtility class can be used to simply apply keyframes
            /// in this way. The velocity of a keyframed rigid body is NOT changed by the
            /// application of impulses or forces. The keyframed rigid body has an infinite
            /// mass when viewed by the rest of the system.
            /// </summary>
            MOTION_KEYFRAMED,

            /// <summary>
            /// This motion type is used for the static elements of a game scene, e.g. the
            /// landscape. Fixed rigid bodies are treated in a special way by the system. They
            /// have the same effect as a rigid body with a motion of type MOTION_KEYFRAMED
            /// and velocity 0, however they are much faster to use, incurring no simulation
            /// overhead, except in collision with moving bodies.
            /// </summary>
            MOTION_FIXED,

            /// <summary>
            /// A box inertia motion which is optimized for thin boxes and has less 
            /// stability problems
            /// </summary>
            MOTION_THIN_BOX_INERTIA,

            /// <summary>
            /// A specialized motion used for character controllers
            /// </summary>
            MOTION_CHARACTER,

            MOTION_MAX_ID
        };

        public enum CollidableQualityType
        {
            /// <summary>
            /// Invalid or unassinged type. If you add a hkpRigidBody to the hkpWorld,
            /// this type automatically gets converted to either
            /// COLLIDABLE_QUALITY_FIXED, COLLIDABLE_QUALITY_KEYFRAMED or 
            /// COLLIDABLE_QUALITY_DEBRIS
            /// </summary>
            COLLIDABLE_QUALITY_INVALID = -1,

            /// <summary>
            /// Use this for fixed bodies. 
            /// </summary>
            COLLIDABLE_QUALITY_FIXED = 0,

            /// <summary>
            /// Use this for moving objects with infinite mass. 
            /// </summary>
            COLLIDABLE_QUALITY_KEYFRAMED,

            /// <summary>
            /// Use this for all your debris objects
            /// </summary>
            COLLIDABLE_QUALITY_DEBRIS,

            /// <summary>
            /// Use this for debris objects that should have simplified Toi collisions 
            /// with fixed/landscape objects.
            /// </summary>
            COLLIDABLE_QUALITY_DEBRIS_SIMPLE_TOI,

            /// <summary>
            /// Use this for moving bodies, which should not leave the world, 
            /// but you rather prefer those objects to tunnel through the world than
            /// dropping frames because the engine 
            /// </summary>
            COLLIDABLE_QUALITY_MOVING,

            /// <summary>
            /// Use this for all objects, which you cannot afford to tunnel through
            /// the world at all
            /// </summary>
            COLLIDABLE_QUALITY_CRITICAL,

            /// <summary>
            /// Use this for very fast objects 
            /// </summary>
            COLLIDABLE_QUALITY_BULLET,

            /// <summary>
            /// For user. If you want to use this, you have to modify 
            /// hkpCollisionDispatcher::initCollisionQualityInfo()
            /// </summary>
            COLLIDABLE_QUALITY_USER,

            /// <summary>
            /// Use this for rigid body character controllers
            /// </summary>
            COLLIDABLE_QUALITY_CHARACTER,

            /// <summary>
            /// Use this for moving objects with infinite mass which should report contact 
            /// points and Toi-collisions against all other bodies, including other fixed and keyframed bodies.
            ///
            /// Note that only non-Toi contact points are reported in collisions against 
            /// debris-quality objects.
            /// </summary>
            COLLIDABLE_QUALITY_KEYFRAMED_REPORTING,

            /// <summary>
            /// End of this list
            /// </summary>
            COLLIDABLE_QUALITY_MAX
        }

        #endregion

        #region Structs

        public class WorldCinfo
        {
            public float Gravity;
            public Vector3 GravityDirection;
            public float WorldSize;
            public float CollisionTolerance;
            public SimulationType HavokSimulationType;
            public SolverType HavokSolverType;
            public bool EnableDeactivation;
            public bool FireCollisionCallbacks;

            public WorldCinfo()
            {
                Gravity = 9.8f;
                GravityDirection = -Vector3.UnitY;
                WorldSize = 150;
                CollisionTolerance = 0.1f;
                HavokSimulationType = SimulationType.SIMULATION_TYPE_DISCRETE;
                HavokSolverType = SolverType.SOLVER_TYPE_4ITERS_MEDIUM;
                FireCollisionCallbacks = false;
                EnableDeactivation = true;
            }
        }

        #endregion

        #region Member Fields

        protected WorldCinfo info;

        protected Dictionary<IPhysicsObject, IntPtr> objectIDs;
        protected Dictionary<IntPtr, IPhysicsObject> reverseIDs;
        protected Dictionary<IntPtr, Vector3> scaleTable;

        protected bool pauseSimulation;
        protected int numSubSteps;
        protected float simulationTimeStep;

        protected float simulationSpeed;

        #region Temporary Variables For Calculation

        protected Matrix tmpMat1 = Matrix.Identity;
        protected Matrix tmpMat2 = Matrix.Identity;
        protected Vector3 tmpVec1 = new Vector3();
        protected Vector3 tmpVec2 = new Vector3();

        #endregion

        #endregion

        #region Constructor

        public HavokPhysics(WorldCinfo info)
        {
            this.info = info;

            numSubSteps = 1;
            simulationTimeStep = 0.016f;
            pauseSimulation = false;
            simulationSpeed = 1;

            objectIDs = new Dictionary<IPhysicsObject, IntPtr>();
            reverseIDs = new Dictionary<IntPtr, IPhysicsObject>();
            scaleTable = new Dictionary<IntPtr, Vector3>();
        }

        #endregion

        #region Properties

        public float Gravity
        {
            get { return info.Gravity; }
            set 
            {
                if (info.Gravity != value)
                {
                    info.Gravity = value;
                    Vector3 g = info.Gravity * info.GravityDirection;
                    HavokDllBridge.set_gravity(Vector3Helper.ToFloats(ref g));
                }
            }
        }

        public Vector3 GravityDirection
        {
            get { return info.GravityDirection; }
            set
            {
                if (!info.GravityDirection.Equals(value))
                {
                    info.GravityDirection = value;
                    Vector3 g = info.Gravity * info.GravityDirection;
                    HavokDllBridge.set_gravity(Vector3Helper.ToFloats(ref g));
                }
            }
        }

        public bool PauseSimulation
        {
            get { return pauseSimulation; }
            set { pauseSimulation = value; }
        }

        public int MaxSimulationSubSteps
        {
            get { return numSubSteps; }
            set { numSubSteps = value; }
        }

        public float SimulationTimeStep
        {
            get { return simulationTimeStep; }
            set { simulationTimeStep = value; }
        }

        public float SimulationSpeed
        {
            get { return simulationSpeed; }
            set { simulationSpeed = value; }
        }

        public WorldCinfo WorldInfo
        {
            get { return info; }
        }

        #endregion

        #region Public Methods

        public void InitializePhysics()
        {
            Vector3 g = info.Gravity * info.GravityDirection;
            if (!HavokDllBridge.init_world(Vector3Helper.ToFloats(g), info.WorldSize, 
                info.CollisionTolerance, info.HavokSimulationType, info.HavokSolverType,
                info.FireCollisionCallbacks, info.EnableDeactivation))
                throw new GoblinException("Failed to initialize Havok physics");
        }

        public void RestartsSimulation()
        {
            HavokDllBridge.dispose();

            InitializePhysics();

            List<IPhysicsObject> physObjs = new List<IPhysicsObject>(objectIDs.Keys);

            objectIDs.Clear();
            reverseIDs.Clear();
            scaleTable.Clear();

            foreach (IPhysicsObject physObj in physObjs)
                AddPhysicsObject(physObj);
        }

        public void AddPhysicsObject(IPhysicsObject physObj)
        {
            if (objectIDs.ContainsKey(physObj))
                return;

            physObj.PhysicsWorldTransform = physObj.CompoundInitialWorldTransform;

            HavokPhysics.MotionType motionType = MotionType.MOTION_INVALID;
            HavokPhysics.CollidableQualityType qualityType = 
                CollidableQualityType.COLLIDABLE_QUALITY_INVALID;
            float friction = -1;
            float restitution = -1;
            float maxLinearVelocity = -1;
            float maxAngularVelocity = -1;
            float allowedPenetrationDepth = -1;
            float gravityFactor = 1;

            if ((physObj is HavokObject))
            {
                HavokObject havokObj = (HavokObject)physObj;
                motionType = havokObj.MotionType;
                qualityType = havokObj.QualityType;
                friction = havokObj.Friction;
                restitution = havokObj.Restitution;
                maxLinearVelocity = havokObj.MaxLinearVelocity;
                maxAngularVelocity = havokObj.MaxAngularVelocity;
                allowedPenetrationDepth = havokObj.AllowedPenetrationDepth;
                gravityFactor = havokObj.GravityFactor;
            }
            else
            {
                bool isDynamic = (physObj.Mass != 0.0f && physObj.Interactable);
                if (isDynamic)
                    motionType = MotionType.MOTION_DYNAMIC;
                else
                    motionType = MotionType.MOTION_FIXED;
            }

            Quaternion rotation;
            Vector3 trans;
            Vector3 scale;
            physObj.CompoundInitialWorldTransform.Decompose(out scale, out rotation, out trans);

            IntPtr shape = GetCollisionShape(physObj, scale);

            float[] pos = Vector3Helper.ToFloats(ref trans);
            float[] rot = { rotation.X, rotation.Y, rotation.Z, rotation.W };

            IntPtr body = HavokDllBridge.add_rigid_body(shape, physObj.Mass, motionType, qualityType,
                pos, rot, Vector3Helper.ToFloats(physObj.InitialLinearVelocity), physObj.LinearDamping,
                maxLinearVelocity, Vector3Helper.ToFloats(physObj.InitialAngularVelocity), 
                physObj.AngularDamping.X, maxAngularVelocity, friction, restitution, 
                allowedPenetrationDepth, physObj.NeverDeactivate, gravityFactor);

            objectIDs.Add(physObj, body);
            reverseIDs.Add(body, physObj);
            scaleTable.Add(body, scale);

            if ((physObj is HavokObject))
            {
                if (((HavokObject)physObj).ContactCallback != null ||
                    ((HavokObject)physObj).CollisionStartCallback != null ||
                    ((HavokObject)physObj).CollisionEndCallback != null)
                    HavokDllBridge.add_contact_listener(body, ((HavokObject)physObj).ContactCallback,
                        ((HavokObject)physObj).CollisionStartCallback,
                        ((HavokObject)physObj).CollisionEndCallback);
            }
        }

        public BoundingBox GetAxisAlignedBoundingBox(IPhysicsObject physObj)
        {
            if (!objectIDs.ContainsKey(physObj))
                return new BoundingBox();

            float[] min = new float[3];
            float[] max = new float[3];
            HavokDllBridge.get_AABB(objectIDs[physObj], min, max);
            Vector3Helper.FromFloats(min, out tmpVec1);
            Vector3Helper.FromFloats(max, out tmpVec2);

            return new BoundingBox(tmpVec1, tmpVec2);
        }

        public List<List<Vector3>> GetCollisionMesh(IPhysicsObject physObj)
        {
            throw new GoblinException("Havok physics does not return collision mesh information");
        }

        /// <summary>
        /// Not implemented yet.
        /// </summary>
        /// <param name="physObj"></param>
        /// <param name="newTransform"></param>
        public void ModifyPhysicsObject(IPhysicsObject physObj, Matrix newTransform)
        {
            
        }

        public void RemovePhysicsObject(IPhysicsObject physObj)
        {
            if (objectIDs.ContainsKey(physObj))
            {
                HavokDllBridge.remove_rigid_body(objectIDs[physObj]);

                reverseIDs.Remove(objectIDs[physObj]);
                scaleTable.Remove(objectIDs[physObj]);
                objectIDs.Remove(physObj);
            }
        }

        public virtual void Update(float elapsedTime)
        {
            if (pauseSimulation)
                return;

            elapsedTime *= simulationSpeed;

            if (numSubSteps > 1)
            {
                int updateTime = Math.Max((int)(Math.Round(elapsedTime / simulationTimeStep)), 1);
                updateTime = Math.Min(numSubSteps, updateTime);
                for (int i = 0; i < updateTime; i++)
                    HavokDllBridge.update(simulationTimeStep);
            }
            else
                HavokDllBridge.update(elapsedTime);

            IntPtr bodyPtr = Marshal.AllocHGlobal(objectIDs.Count * sizeof(int));
            IntPtr transformPtr = Marshal.AllocHGlobal(objectIDs.Count * sizeof(float) * 16);

            int totalSize = 0;
            HavokDllBridge.get_updated_transforms(bodyPtr, transformPtr, ref totalSize);

            unsafe
            {
                int* bodyAddr = (int*)bodyPtr;
                IntPtr tmpPtr = transformPtr;
                float[] mat = new float[16];
                IntPtr body = IntPtr.Zero;
                for (int i = 0; i < totalSize; i++)
                {
                    body = new IntPtr(*bodyAddr);
                    if (reverseIDs.ContainsKey(body))
                    {
                        tmpVec1 = scaleTable[body];

                        Matrix.CreateScale(ref tmpVec1, out tmpMat1);

                        Marshal.Copy(tmpPtr, mat, 0, mat.Length);
                        MatrixHelper.FloatsToMatrix(mat, out tmpMat2);

                        Matrix.Multiply(ref tmpMat1, ref tmpMat2, out tmpMat1);
                        reverseIDs[body].PhysicsWorldTransform = tmpMat1;
                    }

                    tmpPtr = new IntPtr(tmpPtr.ToInt32() + sizeof(float) * 16);
                    bodyAddr++;
                }
            }

            Marshal.FreeHGlobal(bodyPtr);
            Marshal.FreeHGlobal(transformPtr);
        }

        public void Dispose()
        {
            HavokDllBridge.dispose();
            objectIDs.Clear();
            reverseIDs.Clear();
            scaleTable.Clear();
        }

        #endregion

        #region Additional Supported Features

        public void ApplyKeyframe(IPhysicsObject physObj, Vector3 newPos, Quaternion newRot, float timeStep)
        {
            ApplyHardKeyFrame(physObj, newPos, newRot, timeStep);
        }

        public void ApplyHardKeyFrame(IPhysicsObject physObj, Vector3 newPos, Quaternion newRot, float timeStep)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;

            float[] pos = Vector3Helper.ToFloats(ref newPos);
            float[] rot = { newRot.X, newRot.Y, newRot.Z, newRot.W };

            HavokDllBridge.apply_hard_keyframe(objectIDs[physObj], pos, rot, timeStep);
        }

        public void ApplySoftKeyFrame(IPhysicsObject physObj, Vector3 newPos, Quaternion newRot, 
            Vector3 angularPositionFactor, Vector3 angularVelocityFactor, Vector3 linearPositionFactor,
		    Vector3 linearVelocityFactor, float maxAngularAcceleration, float maxLinearAcceleration, 
            float maxAllowedDistance, float timeStep)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;

            float[] pos = Vector3Helper.ToFloats(ref newPos);
            float[] rot = { newRot.X, newRot.Y, newRot.Z, newRot.W };
            float[] angularPosFac = Vector3Helper.ToFloats(ref angularPositionFactor);
            float[] angularVelFac = Vector3Helper.ToFloats(ref angularVelocityFactor);
            float[] linearPosFac = Vector3Helper.ToFloats(ref linearPositionFactor);
            float[] linearVelFac = Vector3Helper.ToFloats(ref linearVelocityFactor);

            HavokDllBridge.apply_soft_keyframe(objectIDs[physObj], pos, rot, angularPosFac,
                angularVelFac, linearPosFac, linearVelFac, maxAngularAcceleration,
                maxLinearAcceleration, maxAllowedDistance, timeStep);
        }

        public void StopKeyframe(IPhysicsObject physObj)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;

            Vector3 scale, trans;
            Quaternion quat;
            physObj.PhysicsWorldTransform.Decompose(out scale, out quat, out trans);

            float[] pos = Vector3Helper.ToFloats(ref trans);
            float[] rot = { quat.X, quat.Y, quat.Z, quat.W };

            HavokDllBridge.apply_hard_keyframe(objectIDs[physObj], pos, rot, 0.016f);
        }

        public void AddForce(IPhysicsObject physObj, float timeStep, Vector3 force)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;

            HavokDllBridge.add_force(objectIDs[physObj], timeStep, Vector3Helper.ToFloats(force));
        }

        public void AddTorque(IPhysicsObject physObj, float timeStep, Vector3 torque)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;

            HavokDllBridge.add_torque(objectIDs[physObj], timeStep, Vector3Helper.ToFloats(torque));
        }

        public void SetLinearVelocity(IPhysicsObject physObj, Vector3 velocity)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;

            HavokDllBridge.set_linear_velocity(objectIDs[physObj], Vector3Helper.ToFloats(ref velocity));
        }

        public Vector3 GetLinearVelocity(IPhysicsObject physObj)
        {
            if (!objectIDs.ContainsKey(physObj))
                return Vector3.Zero;

            float[] velocity = new float[3];
            HavokDllBridge.get_linear_velocity(objectIDs[physObj], velocity);
            return new Vector3(velocity[0], velocity[1], velocity[2]);
        }

        public void SetAngularVelocity(IPhysicsObject physObj, Vector3 velocity)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;

            HavokDllBridge.set_angular_velocity(objectIDs[physObj], Vector3Helper.ToFloats(velocity));
        }

        public Vector3 GetAngularVelocity(IPhysicsObject physObj)
        {
            if (!objectIDs.ContainsKey(physObj))
                return Vector3.Zero;

            float[] velocity = new float[3];
            HavokDllBridge.get_angular_velocity(objectIDs[physObj], velocity);
            return new Vector3(velocity[0], velocity[1], velocity[2]);
        }

        public Matrix GetTransform(IPhysicsObject physObj)
        {
            if (!objectIDs.ContainsKey(physObj))
                return Matrix.Identity;

            float[] transform = new float[16];
            HavokDllBridge.get_body_transform(objectIDs[physObj], transform);
            return MatrixHelper.FloatsToMatrix(transform);
        }

        public Vector3 GetPosition(IPhysicsObject physObj)
        {
            if (!objectIDs.ContainsKey(physObj))
                return Vector3.Zero;

            float[] position = new float[3];
            HavokDllBridge.get_body_position(objectIDs[physObj], position);
            return Vector3Helper.FromFloats(position);
        }

        public Quaternion GetRotation(IPhysicsObject physObj)
        {
            if (!objectIDs.ContainsKey(physObj))
                return Quaternion.Identity;

            float[] rotation = new float[4];
            HavokDllBridge.get_body_rotation(objectIDs[physObj], rotation);
            return new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
        }

        public IPhysicsObject GetPhysicsObject(IntPtr body)
        {
            if (reverseIDs.ContainsKey(body))
                return reverseIDs[body];
            else
                return null;
        }

        public void SetBodyWorldLeaveCallback(HavokDllBridge.BodyLeaveWorldCallback callback)
        {
            HavokDllBridge.add_world_leave_callback(callback);
        }

        #endregion

        #region Helper Functions

        private IntPtr GetCollisionShape(IPhysicsObject physObj, Vector3 scale)
        {
            IntPtr collisionShape = IntPtr.Zero;

            Vector3 boundingBox = Vector3Helper.GetDimensions(physObj.Model.MinimumBoundingBox);
            float[] dim = new float[3];
            float[] top = new float[3];
            float[] bottom = new float[3];
            float[] vertexCloud = null;
            float radius = 0;
            float height = 0;
            float convexRadius = 0.05f;

            if (physObj is HavokObject)
                convexRadius = ((HavokObject)physObj).ConvexRadius;
            
            switch (physObj.Shape)
            {
                case ShapeType.Box:
                    if (physObj.ShapeData.Count == 3)
                    {
                        dim[0] = physObj.ShapeData[0];
                        dim[1] = physObj.ShapeData[1];
                        dim[2] = physObj.ShapeData[2];
                    }
                    else
                    {
                        dim[0] = boundingBox.X * scale.X;
                        dim[1] = boundingBox.Y * scale.Y;
                        dim[2] = boundingBox.Z * scale.Z;
                    }

                    collisionShape = HavokDllBridge.create_box_shape(dim, convexRadius);
                    
                    break;
                case ShapeType.Sphere:
                    if (physObj.ShapeData.Count == 1)
                        radius = physObj.ShapeData[0];
                    else
                        radius = boundingBox.X * scale.X / 2;

                    collisionShape = HavokDllBridge.create_sphere_shape(radius);

                    break;
                case ShapeType.Capsule:
                case ShapeType.Cylinder:
                    if (physObj.ShapeData.Count == 2)
                    {
                        radius = physObj.ShapeData[0];
                        height = physObj.ShapeData[1];
                    }
                    else
                    {
                        radius = boundingBox.X * scale.X / 2;
                        height = boundingBox.Y * scale.Y;
                    }

                    if (physObj.Shape == ShapeType.Capsule)
                    {
                        top[1] = height / 2 - radius;
                        bottom[1] = -height / 2 + radius;
                        collisionShape = HavokDllBridge.create_capsule_shape(top, bottom, radius);
                    }
                    else
                    {
                        top[1] = height / 2;
                        bottom[1] = -height / 2;
                        collisionShape = HavokDllBridge.create_cylinder_shape(top, bottom, radius, convexRadius);
                    }

                    break;
                case ShapeType.Cone:
                    throw new GoblinException("Cone shape is not supported by Havok physics");
                case ShapeType.ChamferCylinder:
                    throw new GoblinException("ChamferCylinder is not supported by Havok physics");
                case ShapeType.ConvexHull:
                    List<Vector3> vertices = physObj.MeshProvider.Vertices;
                    vertexCloud = new float[vertices.Count * 3];
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        vertexCloud[i * 3] = vertices[i].X * scale.X;
                        vertexCloud[i * 3 + 1] = vertices[i].Y * scale.Y;
                        vertexCloud[i * 3 + 2] = vertices[i].Z * scale.Z;
                    }

                    collisionShape = HavokDllBridge.create_convex_shape(vertices.Count, vertexCloud,
                        sizeof(float) * 3, convexRadius);

                    break;
                case ShapeType.TriangleMesh:
                    if(physObj.MeshProvider == null)
                        throw new GoblinException("MeshProvider cannot be null to construct TriangleMesh shape");

                    List<int> indices = physObj.MeshProvider.Indices;
                    vertices = physObj.MeshProvider.Vertices;

                    vertexCloud = new float[vertices.Count * 3];
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        vertexCloud[i * 3] = vertices[i].X * scale.X;
                        vertexCloud[i * 3 + 1] = vertices[i].Y * scale.Y;
                        vertexCloud[i * 3 + 2] = vertices[i].Z * scale.Z;
                    }

                    collisionShape = HavokDllBridge.create_mesh_shape(vertices.Count, vertexCloud,
                        sizeof(float) * 3, indices.Count / 3, indices.ToArray(), convexRadius);
                        
                    break;
                case ShapeType.Compound:
                    break;
            }

            if (physObj is HavokObject)
            {
                if (((HavokObject)physObj).IsPhantom)
                {
                    collisionShape = HavokDllBridge.create_phantom_shape(collisionShape,
                        ((HavokObject)physObj).PhantomEnterCallback,
                        ((HavokObject)physObj).PhantomLeaveCallback);
                }
            }

            return collisionShape;
        }

        #endregion
    }
}
