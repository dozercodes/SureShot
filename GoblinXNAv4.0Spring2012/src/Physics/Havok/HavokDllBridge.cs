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

namespace GoblinXNA.Physics.Havok
{
    /// <summary>
    /// A C# interface to the unmanaged HavokWrapper.dll file.
    /// </summary>
    public class HavokDllBridge
    {
        #region Delegate Functions

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ContactCallback(IntPtr body1, IntPtr body2, float contactSpeed);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CollisionStarted(IntPtr body1, IntPtr body2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CollisionEnded(IntPtr body1, IntPtr body2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BodyLeaveWorldCallback(IntPtr body);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PhantomEnterCallback(IntPtr body);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PhantomLeaveCallback(IntPtr body);

        #endregion

        private const String HAVOK_DLL = "HavokWrapper.dll";

        [DllImport(HAVOK_DLL, EntryPoint = "init_world", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool init_world(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] gravity, 
            float worldSize, 
            float collisionTolerance,
            HavokPhysics.SimulationType simulationType,
            HavokPhysics.SolverType solverType,
            bool fireCollisionCallbacks,
            bool enableDeactivation);

        [DllImport(HAVOK_DLL, EntryPoint = "set_gravity", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_gravity(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] gravity);

        [DllImport(HAVOK_DLL, EntryPoint = "add_world_leave_callback", CallingConvention = CallingConvention.Cdecl)]
        public static extern void add_world_leave_callback(
            BodyLeaveWorldCallback callback);

        [DllImport(HAVOK_DLL, EntryPoint = "create_box_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_box_shape(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] dim,
            float convexRadius);

        [DllImport(HAVOK_DLL, EntryPoint = "create_sphere_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_sphere_shape(float radius);

        [DllImport(HAVOK_DLL, EntryPoint = "create_triangle_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_triangle_shape(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] v0, 
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] v1, 
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] v2,
            float convexRadius);

        [DllImport(HAVOK_DLL, EntryPoint = "create_capsule_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_capsule_shape(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] top,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] bottom,
            float radius);

        [DllImport(HAVOK_DLL, EntryPoint = "create_cylinder_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_cylinder_shape(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] top,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] bottom,
            float radius,
            float convexRadius);

        [DllImport(HAVOK_DLL, EntryPoint = "create_convex_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_convex_shape(
            int numVertices,
            [MarshalAs(UnmanagedType.LPArray)] float[] vertices,
            int stride,
            float convexRadius);

        [DllImport(HAVOK_DLL, EntryPoint = "create_phantom_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_phantom_shape(
            IntPtr boundingShape,
            PhantomEnterCallback enterCallback,
            PhantomLeaveCallback leaveCallback);

        [DllImport(HAVOK_DLL, EntryPoint = "create_mesh_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_mesh_shape(
            int numVertices,
            [MarshalAs(UnmanagedType.LPArray)] float[] vertices,
            int vertexStride,
            int numTriangles,
            [MarshalAs(UnmanagedType.LPArray)] int[] indices,
            float convexRadius);

        [DllImport(HAVOK_DLL, EntryPoint = "add_rigid_body", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr add_rigid_body(
            IntPtr shape,
            float mass,
            HavokPhysics.MotionType motionType,
            HavokPhysics.CollidableQualityType qualityType,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] pos,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] float[] rot,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] linearVelocity,
            float linearDamping,
            float maxLinearVelocity,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] angularVelocity,
            float angularDamping,
            float maxAngularVelocity,
            float friction,
            float restitution,
            float allowedPenetrationDepth,
            bool neverDeactivate,
            float gravityFactor);

        [DllImport(HAVOK_DLL, EntryPoint = "remove_rigid_body", CallingConvention = CallingConvention.Cdecl)]
        public static extern void remove_rigid_body(
            IntPtr body);

        [DllImport(HAVOK_DLL, EntryPoint = "add_contact_listener", CallingConvention = CallingConvention.Cdecl)]
        public static extern void add_contact_listener(
            IntPtr body,
            ContactCallback cc,
            CollisionStarted cs,
            CollisionEnded ce);

        [DllImport(HAVOK_DLL, EntryPoint = "add_force", CallingConvention = CallingConvention.Cdecl)]
        public static extern void add_force(
            IntPtr body,
            float timeStep,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] force);

        [DllImport(HAVOK_DLL, EntryPoint = "add_torque", CallingConvention = CallingConvention.Cdecl)]
        public static extern void add_torque(
            IntPtr body,
            float timeStep,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] torque);

        [DllImport(HAVOK_DLL, EntryPoint = "set_linear_velocity", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_linear_velocity(
            IntPtr body,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] vel);

        [DllImport(HAVOK_DLL, EntryPoint = "set_angular_velocity", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_angular_velocity(
            IntPtr body,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] vel);

        [DllImport(HAVOK_DLL, EntryPoint = "get_linear_velocity", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_linear_velocity(
            IntPtr body,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] vel);

        [DllImport(HAVOK_DLL, EntryPoint = "get_angular_velocity", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_angular_velocity(
            IntPtr body,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] vel);

        [DllImport(HAVOK_DLL, EntryPoint = "apply_hard_keyframe", CallingConvention = CallingConvention.Cdecl)]
        public static extern void apply_hard_keyframe(
            IntPtr body,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] pos,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] float[] rot,
            float timeStep);

        [DllImport(HAVOK_DLL, EntryPoint = "apply_soft_keyframe", CallingConvention = CallingConvention.Cdecl)]
        public static extern void apply_soft_keyframe(
            IntPtr body,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] pos,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] float[] rot,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] angularPosFac,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] angularVelFac,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] linearPosFac,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] linearVelFac,
            float maxAngularAccel,
            float maxLinearAccel,
            float maxAllowedDistance,
            float timeStep);

        [DllImport(HAVOK_DLL, EntryPoint = "get_AABB", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_AABB(
            IntPtr body,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] min,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] max);

        [DllImport(HAVOK_DLL, EntryPoint = "update", CallingConvention = CallingConvention.Cdecl)]
        public static extern void update(float elapsedSeconds);

        [DllImport(HAVOK_DLL, EntryPoint = "get_body_transform", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_body_transform(
            IntPtr body,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] float[] transform);

        [DllImport(HAVOK_DLL, EntryPoint = "get_body_position", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_body_position(
            IntPtr body,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] position);

        [DllImport(HAVOK_DLL, EntryPoint = "get_body_rotation", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_body_rotation(
            IntPtr body,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] float[] rotation);

        [DllImport(HAVOK_DLL, EntryPoint = "get_updated_transforms", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_updated_transforms(
            [Out] IntPtr bodyPtr,
            [Out] IntPtr transformPtr, 
            ref int totalSize);

        [DllImport(HAVOK_DLL, EntryPoint = "dispose")]
        public static extern void dispose();
    }
}
