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

using GoblinXNA.Physics;
using System.Xml;
using Komires.MataliPhysics;
using MataliPhysicsObject = Komires.MataliPhysics.PhysicsObject;

namespace GoblinXNA.Physics.Matali
{
    #region Delegates

    public delegate void CollisionCallback(MataliPhysicsObject baseObject, MataliPhysicsObject collidingObject);

    #endregion

    #region Enum

    public enum CompoundShapeType
    {
        ConvexHull,
        MinkowskiSum
    }

    public enum ExtraShapeType
    {
        Undefined,
        Point,
        Edge,
        Plane,
        Triangle,
        Tetrahedron,
        Hemisphere,
        Heightmap,
        Fluid
    }

    #endregion

    public class MataliObject : PhysicsObject
    {
        #region Member Fields

        protected float density;
        protected CompoundShapeType compoundShapeType;
        protected ExtraShapeType extraShapeType;
        protected SimulateMethod postTransformCallback;
        protected int maxTrianglesForPartition;
        protected bool flipTriangleOrder;

        protected float dynamicFriction;
        protected float staticFriction;
        protected float restitution;

        protected float triangleMeshFriction;
        protected float triangleMeshRestitution;

        protected float maxPreUpdateAngularVelocity;
        protected float maxPostUpdateAngularVelocity;

        protected float minResponseLinearVelocity;
        protected float minResponseAngularVelocity;

        protected Matrix relativeTransform;

        protected CollisionCallback collisionStartCallback;
        protected CollisionCallback collisionContinueCallback;
        protected CollisionCallback collisionEndCallback;

        protected Matrix shapeOriginalMatrix;
        protected bool shapeOriginalMatrixSet;
        protected float shapeCollisionMargin;

        protected bool isDynamic;

        #endregion

        #region Constructor

        public MataliObject(object container)
            : base(container)
        {
            density = 1.0f;
            mass = 0;
            compoundShapeType = CompoundShapeType.ConvexHull;
            extraShapeType = ExtraShapeType.Undefined;
            maxTrianglesForPartition = 2;

            relativeTransform = Matrix.Identity;

            triangleMeshFriction = 1.0f;
            triangleMeshRestitution = 0.0f;

            dynamicFriction = 1.0f;
            staticFriction = 1.0f;
            restitution = 0.0f;

            maxPreUpdateAngularVelocity = 1000;
            maxPostUpdateAngularVelocity = 1000;

            minResponseAngularVelocity = 0;
            minResponseLinearVelocity = 0;

            shapeOriginalMatrix = Matrix.Identity;
            shapeOriginalMatrixSet = false;
            shapeCollisionMargin = 0.0f;

            isDynamic = false;
            flipTriangleOrder = false;
        }

        #endregion

        #region Properties

        public float Density
        {
            get { return density; }
            set 
            { 
                density = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the dynamic friction of this physics object. The value should be between 0 and 1.
        /// The default value is 1.0f.
        /// </summary>
        public float DynamicFriction
        {
            get { return dynamicFriction; }
            set
            {
                if (value < 0 || value > 1)
                    throw new GoblinException("DynamicFriction must be between 0 and 1");

                dynamicFriction = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the static friction of this physics object. The value should be between 0 and 1.
        /// The default value is 1.0f.
        /// </summary>
        public float StaticFriction
        {
            get { return staticFriction; }
            set
            {
                if (value < 0 || value > 1)
                    throw new GoblinException("StaticFriction must be between 0 and 1");

                staticFriction = value;
                modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the restitution of this physics object. The value should be between 0 and 10.
        /// The default value is 0.0f.
        /// </summary>
        public float Restitution
        {
            get { return restitution; }
            set
            {
                if (value < 0 || value > 10)
                    throw new GoblinException("Restitution must be between 0 and 10");

                restitution = value;
                modified = true;
            }
        }

        public SimulateMethod PostTransformCallback
        {
            get { return postTransformCallback; }
            set { postTransformCallback = value; }
        }

        /// <summary>
        /// Gets or sets the callback function when there is a collision that just started.
        /// </summary>
        public CollisionCallback CollisionStartCallback
        {
            get { return collisionStartCallback; }
            set { collisionStartCallback = value; }
        }

        /// <summary>
        /// Gets or sets the callback function when there is a continuing collision.
        /// </summary>
        public CollisionCallback CollisionContinueCallback
        {
            get { return collisionContinueCallback; }
            set { collisionContinueCallback = value; }
        }

        /// <summary>
        /// Gets or sets the callback function when a collision between two objects end.
        /// </summary>
        public CollisionCallback CollisionEndCallback
        {
            get { return collisionEndCallback; }
            set { collisionEndCallback = value; }
        }

        public CompoundShapeType CompoundShape
        {
            get { return compoundShapeType; }
            set { compoundShapeType = value; }
        }

        public ExtraShapeType ExtraShape
        {
            get { return extraShapeType; }
            set { extraShapeType = value; }
        }

        public Matrix ShapeOriginalMatrix
        {
            get { return shapeOriginalMatrix; }
            set 
            { 
                shapeOriginalMatrix = value;
                shapeOriginalMatrixSet = true;
            }
        }

        internal bool ShapeOriginalMatrixSet
        {
            get { return shapeOriginalMatrixSet; }
        }

        public float ShapeCollisionMargin
        {
            get { return shapeCollisionMargin; }
            set { shapeCollisionMargin = value; }
        }

        /// <summary>
        /// Gets or sets whether this mesh may be updated dynamically. Only applies to HeightMap type.
        /// </summary>
        public bool IsDynamic
        {
            get { return isDynamic; }
            set { isDynamic = value; }
        }

        /// <summary>
        /// This only applies for Shape.TriangleMesh. Default value is 2.
        /// </summary>
        public int MaxTrianglesForPartition
        {
            get { return maxTrianglesForPartition; }
            set { maxTrianglesForPartition = value; }
        }

        public bool FlipTriangleOrder
        {
            get { return flipTriangleOrder; }
            set { flipTriangleOrder = value; }
        }

        public float TriangleMeshFriction
        {
            get { return triangleMeshFriction; }
            set { triangleMeshFriction = value; }
        }

        public float TriangleMeshRestitution
        {
            get { return triangleMeshRestitution; }
            set { triangleMeshRestitution = value; }
        }

        public Matrix RelativeTransform
        {
            get { return relativeTransform; }
            set { relativeTransform = value; }
        }

        public float MaxPreUpdateAngularVelocity
        {
            get { return maxPreUpdateAngularVelocity; }
            set { maxPreUpdateAngularVelocity = value; }
        }

        public float MaxPostUpdateAngularVelocity
        {
            get { return maxPostUpdateAngularVelocity; }
            set { maxPostUpdateAngularVelocity = value; }
        }

        public float MinResponseLinearVelocity
        {
            get { return minResponseLinearVelocity; }
            set { minResponseLinearVelocity = value; }
        }

        public float MinResponseAngularVelocity
        {
            get { return minResponseAngularVelocity; }
            set { minResponseAngularVelocity = value; }
        }

        #endregion

        #region

        public void Copy(MataliObject mataliObj)
        {
            // needs more to copy, but will implement later
            Model = mataliObj.Model;
            Shape = mataliObj.Shape;
            if (mataliObj.ShapeData.Count > 0)
                ShapeData.AddRange(mataliObj.ShapeData);
            ExtraShape = mataliObj.ExtraShape;

            Mass = mataliObj.Mass;
            Density = mataliObj.Density;
            RelativeTransform = mataliObj.RelativeTransform;
            ShapeCollisionMargin = mataliObj.ShapeCollisionMargin;
            ShapeOriginalMatrix = mataliObj.ShapeOriginalMatrix;
            StaticFriction = mataliObj.StaticFriction;
            DynamicFriction = mataliObj.DynamicFriction;
            Restitution = mataliObj.Restitution;
        }

        #endregion
    }
}
