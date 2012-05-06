/************************************************************************************ 
 * Copyright (c) 2008-2012, Columbia University
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
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;

using Model = GoblinXNA.Graphics.Model;

using Komires.MataliPhysics;
using MataliPhysicsObject = Komires.MataliPhysics.PhysicsObject;

namespace GoblinXNA.Physics.Matali
{
    public delegate void CreateConstraintCallback(Constraint constraint);

    #region Structs

    public struct ConstraintPair
    {
        public string Name;
        public CreateConstraintCallback Callback;
        public IPhysicsObject PhysicsObject1;
        public IPhysicsObject PhysicsObject2;
    }

    #endregion

    public class MataliPhysics : IPhysics
    {
        #region Member Fields

        protected List<PickedObject> pickedObjects;

        protected Vector3 gravityDir;

        private bool pauseSimulation;

        private PhysicsEngine engine;
        private PhysicsScene scene;
        private MataliPhysicsObject obj;

        private Dictionary<IPhysicsObject, MataliPhysicsObject> objectIDs;
        private Dictionary<MataliPhysicsObject, Dictionary<MataliPhysicsObject, bool>> collisionTable;
        private Dictionary<MataliPhysicsObject, List<Constraint>> constraintTable;

        private List<ConstraintPair> constraintsToBeAdded;

        private int nameCount;
        private bool buildCollisionMesh;

        private Matrix tempMat1;
        private Matrix tempMat2;
        private Matrix tempMat3;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a physics engine that uses Matali Physics from Komires.
        /// </summary>
        /// <remarks>
        /// For all of the shapes that has directions (e.g., Cylinder, Hemisphere), Y direction is
        /// used, so if you would like it to face other directions, use MataliObject.ShapeOriginalMatrix
        /// to orient them.
        /// 
        /// For a cylinder with different bottom and top radius, IPhysicsObject.ShapeData are used.
        /// ShapeData[0] - bottom radius, ShapeData[1] - height, ShapeData[2] = top radius
        /// 
        /// For Compound shape, an additional information can be set by using
        /// MataliObject.CompoundShape.
        /// 
        /// For additional shape types such as Heightmap, Point, and so on, set Shape to ShapeType.Extra
        /// and define MataliObject.ExtraShape.
        /// </remarks>
        public MataliPhysics()
        {
            gravityDir = Vector3Helper.Get(0, -1, 0);

            pauseSimulation = false;

            objectIDs = new Dictionary<IPhysicsObject, MataliPhysicsObject>();
            collisionTable = new Dictionary<MataliPhysicsObject, Dictionary<MataliPhysicsObject, bool>>();
            constraintsToBeAdded = new List<ConstraintPair>();
            constraintTable = new Dictionary<MataliPhysicsObject, List<Constraint>>();
            nameCount = 0;

            tempMat1 = Matrix.Identity;
            tempMat2 = Matrix.Identity;
            tempMat3 = Matrix.Identity;

            buildCollisionMesh = false;

            pickedObjects = new List<PickedObject>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Default is 9.81f.
        /// </summary>
        public float Gravity
        {
            get { return scene.GravityAcceleration; }
            set { scene.GravityAcceleration = value; }
        }

        public Vector3 GravityDirection
        {
            get 
            {
                scene.GetGravityDirection(ref gravityDir);
                return gravityDir; 
            }
            set
            {
                gravityDir = value;
                gravityDir.Normalize();
                scene.SetGravityDirection(ref gravityDir);
            }
        }

        /// <summary>
        /// Default is 10.
        /// </summary>
        public int MaxSimulationSubSteps
        {
            get { return scene.MaxIterationCount; }
            set { scene.MaxIterationCount = value; }
        }

        /// <summary>
        /// Default is 1/15 (15 Hz)
        /// </summary>
        public float SimulationTimeStep
        {
            get { return scene.TimeOfSimulation; }
            set { scene.TimeOfSimulation = value; }
        }

        public PhysicsEngine MataliEngine
        {
            get { return engine; }
        }

        public PhysicsScene MataliScene
        {
            get { return scene; }
        }

        /// <summary>
        /// MataliPhysics builds collision mesh for only Shapes that requires a call to CreateMesh() method including
        /// TriangleMesh, Heightmap, CompoundShape, etc. Set this to true if you want MataliPhysics to build collision
        /// mesh for any shape types when you want to visualize the collision mesh for debugging purposes (not recommended
        /// to be set to true unless you really need them)
        /// </summary>
        public bool BuildCollisionMesh
        {
            get { return buildCollisionMesh; }
            set { buildCollisionMesh = value; }
        }

        #endregion

        #region Public Methods

        public void InitializePhysics()
        {
            engine = new PhysicsEngine("MetaliPhysics");
            scene = engine.Factory.PhysicsSceneManager.Create("Scene");
            scene.TimeOfSimulation = 0.016f;
        }

        public void RestartsSimulation()
        {
            List<IPhysicsObject> physObjs = new List<IPhysicsObject>(objectIDs.Keys);
            foreach (IPhysicsObject physObj in physObjs)
                RemovePhysicsObject(physObj);

            objectIDs.Clear();
            collisionTable.Clear();
            constraintsToBeAdded.Clear();

            foreach (IPhysicsObject physObj in physObjs)
                AddPhysicsObject(physObj);
        }

        public void AddPhysicsObject(IPhysicsObject physObj)
        {
            if (objectIDs.ContainsKey(physObj))
                return;

            // rigidbody is dynamic if and only if mass is non zero, otherwise static
            bool isDynamic = (physObj.Mass != 0.0f && physObj.Interactable) ||
                (physObj.Shape == ShapeType.TriangleMesh);

            physObj.PhysicsWorldTransform = physObj.CompoundInitialWorldTransform;

            MataliPhysicsObject mataliPhysicsObj = scene.Factory.PhysicsObjectManager.Create(nameCount.ToString());
            
            if (physObj is MataliVehicle)
            {
                MataliVehicle vehicle = (MataliVehicle)physObj;
                if (!vehicle.HasEnoughParts)
                    throw new GoblinException("Your vehicle does not have enough parts. You need to at least" +
                        " have a body and four wheels");

                MataliPhysicsObject body = scene.Factory.PhysicsObjectManager.Create("Car Body " + nameCount);
                body.EnableFeedback = true;
                body.Material.RigidGroup = true;
                body.EnableBreakRigidGroup = false;
                mataliPhysicsObj.AddPhysicsObject(body);

                // Add vehicle body
                if (vehicle.Body.Count == 1)
                {
                    vehicle.Body[0].CompoundInitialWorldTransform = vehicle.Body[0].RelativeTransform;
                    vehicle.Body[0].PhysicsWorldTransform = physObj.CompoundInitialWorldTransform *
                        vehicle.Body[0].RelativeTransform;
                    body.Material.RigidGroup = true;
                    body.EnableBreakRigidGroup = false;
                    SetPhysicalProperties(body, vehicle.Body[0]);
                    objectIDs.Add(vehicle.Body[0], body);
                }
                else
                {
                    for (int i = 0; i < vehicle.Body.Count; ++i)
                    {
                        MataliPhysicsObject bodyPart =
                            scene.Factory.PhysicsObjectManager.Create("Car Body Parts " + nameCount + " " + i);
                        vehicle.Body[i].CompoundInitialWorldTransform = vehicle.Body[i].RelativeTransform;
                        vehicle.Body[i].PhysicsWorldTransform = MatrixHelper.Empty;
                        bodyPart.Material.RigidGroup = true;
                        bodyPart.EnableBreakRigidGroup = false;
                        SetPhysicalProperties(bodyPart, vehicle.Body[i]);
                        body.AddPhysicsObject(bodyPart);
                        objectIDs.Add(vehicle.Body[i], body);
                    }
                }

                // Add vehicle wheels
                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < vehicle.Wheels[i].Count; ++j)
                    {
                        MataliPhysicsObject wheelPart =
                            scene.Factory.PhysicsObjectManager.Create("Car Wheel Parts " + nameCount + " " + i + " " + j);
                        vehicle.Wheels[i][j].CompoundInitialWorldTransform = vehicle.Wheels[i][j].RelativeTransform;
                        vehicle.Wheels[i][j].PhysicsWorldTransform = MatrixHelper.Empty;
                        for (int k = 0; k < vehicle.Body.Count; ++k)
                        {
                            wheelPart.DisableCollision(objectIDs[vehicle.Body[k]], true);
                        }
                        SetPhysicalProperties(wheelPart, vehicle.Wheels[i][j]);
                        mataliPhysicsObj.AddPhysicsObject(wheelPart);
                        objectIDs.Add(vehicle.Wheels[i][j], wheelPart);
                    }
                }

                mataliPhysicsObj.UpdateFromInitLocalTransform();

                if(vehicle.Constraints.Count > 0)
                    constraintsToBeAdded.AddRange(vehicle.Constraints);

                AddConstraint();
            }
            else if (physObj is MataliCloth)
            {
                MataliCloth cloth = (MataliCloth)physObj;

                int i = 0;
                foreach (MataliObject particle in cloth.Particles)
                {
                    MataliPhysicsObject clothParticle =
                        scene.Factory.PhysicsObjectManager.Create("Point Cloth " + nameCount + " Particle " + i);
                    mataliPhysicsObj.AddPhysicsObject(clothParticle);
                    SetPhysicalProperties(clothParticle, particle);
                    objectIDs.Add(particle, clothParticle);
                    i++;
                }

                mataliPhysicsObj.UpdateFromInitLocalTransform();

                constraintsToBeAdded.AddRange(cloth.Constraints);
                AddConstraint();
            }

            SetPhysicalProperties(mataliPhysicsObj, physObj);

            scene.UpdateFromInitLocalTransform(mataliPhysicsObj);

            objectIDs.Add(physObj, mataliPhysicsObj);

            nameCount++;
        }

        public void ModifyPhysicsObject(IPhysicsObject physObj, Matrix newTransform)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;
        }

        public void RemovePhysicsObject(IPhysicsObject physObj)
        {
            if (objectIDs.ContainsKey(physObj))
            {
                MataliPhysicsObject mataliPhysicsObj = objectIDs[physObj];
                scene.RemovePhysicsObject(mataliPhysicsObj, true, true);
                collisionTable.Remove(mataliPhysicsObj);

                foreach (MataliPhysicsObject obj in collisionTable.Keys)
                {
                    if (collisionTable[obj].ContainsKey(mataliPhysicsObj))
                        collisionTable[obj].Remove(mataliPhysicsObj);
                }
                objectIDs.Remove(physObj);
            }
        }

        public BoundingBox GetAxisAlignedBoundingBox(IPhysicsObject physObj)
        {
            BoundingBox aabb = new BoundingBox();
            if (objectIDs.ContainsKey(physObj))
            {
                MataliPhysicsObject mataliPhysicsObj = objectIDs[physObj];
                mataliPhysicsObj.GetBoundingBox(ref aabb);
            }

            return aabb;
        }

        public List<List<Vector3>> GetCollisionMesh(IPhysicsObject physObj)
        {
            List<List<Vector3>> collisionMesh = new List<List<Vector3>>();

            Matrix mat;
            if (objectIDs.ContainsKey(physObj))
            {
                MataliPhysicsObject mataliPhysicsObj = objectIDs[physObj];
                if (mataliPhysicsObj.Shape == null)
                    return collisionMesh;

                mat = physObj.PhysicsWorldTransform;
                if ((physObj is MataliObject) && (((MataliObject)physObj).ShapeOriginalMatrixSet))
                    mat = ((MataliObject)physObj).ShapeOriginalMatrix * mat;

                if (mataliPhysicsObj.Shape.TriangleVertexCount > 0)
                {
                    VertexPositionNormalTexture[] verts = new VertexPositionNormalTexture[mataliPhysicsObj.Shape.VertexCount];
                    int[] indices = new int[mataliPhysicsObj.Shape.IndexCount];
                    mataliPhysicsObj.Shape.GetMeshVertices(1, 1, false, false, verts);
                    mataliPhysicsObj.Shape.GetMeshIndices(false, indices);

                    if (!mat.Equals(Matrix.Identity))
                    {
                        for (int i = 0; i < verts.Length; ++i)
                        {
                            verts[i].Position = Vector3.Transform(verts[i].Position, mat);
                        }
                    }

                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        List<Vector3> triangle = new List<Vector3>();
                        triangle.Add(verts[indices[i]].Position);
                        triangle.Add(verts[indices[i + 1]].Position);
                        triangle.Add(verts[indices[i + 2]].Position);
                        collisionMesh.Add(triangle);
                    }
                }
            }

            return collisionMesh;
        }

        public void SetTransform(IPhysicsObject physObj, Matrix transform)
        {
            if (objectIDs.ContainsKey(physObj))
            {
                MataliPhysicsObject mataliObj = objectIDs[physObj];
                mataliObj.MainWorldTransform.SetTransformMatrix(transform);
                mataliObj.RecalculateMainTransform();
            }
        }

        public void SetPosition(IPhysicsObject physObj, Vector3 position)
        {
            if (objectIDs.ContainsKey(physObj))
            {
                MataliPhysicsObject mataliObj = objectIDs[physObj];
                mataliObj.MainWorldTransform.SetPosition(position);
                mataliObj.RecalculateMainTransform();
            }
        }

        public void SetRotation(IPhysicsObject physObj, Quaternion rotation)
        {
            if (objectIDs.ContainsKey(physObj))
            {
                MataliPhysicsObject mataliObj = objectIDs[physObj];
                mataliObj.MainWorldTransform.SetRotation(Matrix.CreateFromQuaternion(rotation));
                mataliObj.RecalculateMainTransform();
            }
        }

        public void Update(float elapsedTime)
        {
            if (pauseSimulation)
                return;

            AddConstraint();

            scene.Simulate(elapsedTime);

            Dictionary<MataliPhysicsObject, bool> table = null;
            List<MataliPhysicsObject> removeList = new List<MataliPhysicsObject>();
            foreach (MataliPhysicsObject objBase in collisionTable.Keys)
            {
                table = collisionTable[objBase];
                MataliObject mataliObj = (MataliObject)objBase.UserTagObj;

                foreach (MataliPhysicsObject objCol in table.Keys)
                    if (!objBase.IsColliding(objCol))
                    {
                        removeList.Add(objCol);
                        if (mataliObj.CollisionEndCallback != null)
                            mataliObj.CollisionEndCallback(objBase, objCol);
                    }

                foreach (MataliPhysicsObject obj in removeList)
                    table.Remove(obj);

                removeList.Clear();
            }
        }

        public void Dispose()
        {
            engine.Factory.PhysicsSceneManager.RemoveAll();
            try
            {
                engine.Exit();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Gets the associated Matali's PhysicsObject instance from a physics object.
        /// </summary>
        /// <param name="physObj"></param>
        /// <returns></returns>
        public MataliPhysicsObject GetMataliPhysicsObject(IPhysicsObject physObj)
        {
            return objectIDs[physObj];
        }

        public IPhysicsObject GetIPhysicsObject(MataliPhysicsObject matPhysObj)
        {
            foreach (KeyValuePair<IPhysicsObject, MataliPhysicsObject> pair in objectIDs)
            {
                if (matPhysObj.Equals(pair.Value))
                {
                    return pair.Key;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a constraint between two existing physical objects.
        /// </summary>
        /// <remarks>
        /// The constraints are not actually added at this stage, but in the Update method.
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="physObj1"></param>
        /// <param name="physObj2"></param>
        /// <param name="callback"></param>
        public void CreateConstraint(string name, IPhysicsObject physObj1, IPhysicsObject physObj2, 
            CreateConstraintCallback callback)
        {
            ConstraintPair pair = new ConstraintPair();
            pair.Name = name;
            pair.Callback = callback;
            pair.PhysicsObject1 = physObj1;
            pair.PhysicsObject2 = physObj2;

            constraintsToBeAdded.Add(pair);
        }

        /// <summary>
        /// Modifies an existing height map field with float arrays.
        /// </summary>
        /// <param name="physObj"></param>
        /// <param name="heightmap"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ModifyHeightmap(IPhysicsObject physObj, float[] heightmap, int width, int height)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;

            MataliPhysicsObject mataliPhysicsObj = objectIDs[physObj];
            if (mataliPhysicsObj.InternalControllers.HeightmapController == null)
                return;

            int index = 0;
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j, index++)
                {
                    mataliPhysicsObj.InternalControllers.HeightmapController.SetHeight(i, j, heightmap[index]);
                }
            }

            mataliPhysicsObj.InternalControllers.HeightmapController.UpdateBounding();
        }

        /// <summary>
        /// Performs raycast picking with the given near and far points.
        /// </summary>
        /// <param name="nearPoint">The near point of the pick ray</param>
        /// <param name="farPoint">The far point of the pick ray</param>
        /// <returns>A list of picked objects</returns>
        public List<PickedObject> PickRayCast(Vector3 nearPoint, Vector3 farPoint)
        {
            pickedObjects.Clear();

            Vector3 rayDirection = (farPoint - nearPoint);
            rayDirection.Normalize();
            scene.UpdatePhysicsObjectsIntersectedByRay(ref nearPoint, ref rayDirection, 0, false);
            int objectsCollided = scene.IntersectedPhysicsObjectsCount;

            Vector3 hitPosition = Vector3.Zero;

            for (int i = 0; i < objectsCollided; i++)
            {

                MataliPhysicsObject obj = scene.GetIntersectedPhysicsObject(i, ref hitPosition);

                IPhysicsObject physObj = GetIPhysicsObject(obj);
                if (physObj != null && physObj.Pickable)
                {
                    PickedObject pickedObject = new PickedObject(physObj, i);
                    pickedObjects.Add(pickedObject);
                }
            }

            return pickedObjects;
        }

        /// <summary>
        /// Modifies an existing height map field with short arrays.
        /// </summary>
        /// <param name="physObj"></param>
        /// <param name="heightmap"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ModifyHeightmapShort(IPhysicsObject physObj, short[] heightmap, int width, int height)
        {
            if (!objectIDs.ContainsKey(physObj))
                return;

            MataliPhysicsObject mataliPhysicsObj = objectIDs[physObj];
            if (mataliPhysicsObj.InternalControllers.HeightmapController == null)
                return;

            int index = 0;
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j, index++)
                {
                    mataliPhysicsObj.InternalControllers.HeightmapController.SetHeight(i, j, heightmap[index]);
                }
            }

            mataliPhysicsObj.InternalControllers.HeightmapController.UpdateBounding();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Actually adds the constraints to the physics engine.
        /// </summary>
        private void AddConstraint()
        {
            if (constraintsToBeAdded.Count == 0)
                return;

            List<ConstraintPair> removeList = new List<ConstraintPair>();
            foreach (ConstraintPair pair in constraintsToBeAdded)
            {
                if (!objectIDs.ContainsKey(pair.PhysicsObject1) ||
                    !objectIDs.ContainsKey(pair.PhysicsObject2))
                    continue;

                Constraint constraint = scene.Factory.ConstraintManager.Create(pair.Name);
                constraint.PhysicsObject1 = objectIDs[pair.PhysicsObject1];
                constraint.PhysicsObject2 = objectIDs[pair.PhysicsObject2];
                pair.Callback(constraint);
                constraint.Update();

                removeList.Add(pair);
            }

            foreach (ConstraintPair pair in removeList)
                constraintsToBeAdded.Remove(pair);
        }

        private void SetPhysicalProperties(MataliPhysicsObject mataliPhysicsObj, IPhysicsObject physObj)
        {
            mataliPhysicsObj.UserTagObj = physObj;
            mataliPhysicsObj.InitLocalTransform.SetTransformMatrix(physObj.CompoundInitialWorldTransform);
            mataliPhysicsObj.InitLocalTransform.SetLinearVelocity(physObj.InitialLinearVelocity);
            mataliPhysicsObj.InitLocalTransform.SetAngularVelocity(physObj.InitialAngularVelocity);

            mataliPhysicsObj.UserControllers.EnableDraw = false;
            mataliPhysicsObj.UserControllers.EnablePostDraw = false;

            if (physObj is MataliObject && ((MataliObject)physObj).PostTransformCallback != null)
                mataliPhysicsObj.UserControllers.PostTransformMethods +=
                    new SimulateMethod(((MataliObject)physObj).PostTransformCallback);

            if ((physObj is MataliVehicle) || (physObj is MataliCloth))
                return;

            mataliPhysicsObj.UserControllers.PostTransformMethods += new SimulateMethod(UpdateTransforms);
            SetShape(mataliPhysicsObj, physObj);

            if (physObj is MataliObject)
            {
                if (physObj.Mass > 0)
                    mataliPhysicsObj.Integral.SetMass(physObj.Mass);
                else
                    mataliPhysicsObj.Integral.SetDensity(((MataliObject)physObj).Density);
                mataliPhysicsObj.Material.StaticFriction = ((MataliObject)physObj).StaticFriction;
                mataliPhysicsObj.Material.DynamicFriction = ((MataliObject)physObj).DynamicFriction;
                mataliPhysicsObj.Material.Restitution = ((MataliObject)physObj).Restitution;

                mataliPhysicsObj.MaxPreUpdateAngularVelocity = 
                    ((MataliObject)physObj).MaxPreUpdateAngularVelocity;
                mataliPhysicsObj.MaxPostUpdateAngularVelocity =
                    ((MataliObject)physObj).MaxPostUpdateAngularVelocity;
                mataliPhysicsObj.MinResponseAngularVelocity =
                    ((MataliObject)physObj).MinResponseAngularVelocity;
                mataliPhysicsObj.MinResponseLinearVelocity =
                    ((MataliObject)physObj).MinResponseLinearVelocity;

                if ((((MataliObject)physObj).CollisionStartCallback != null) ||
                    (((MataliObject)physObj).CollisionContinueCallback != null) ||
                    (((MataliObject)physObj).CollisionEndCallback != null))
                {
                    mataliPhysicsObj.UserControllers.CollisionMethods += new CollisionMethod(CheckCollision);
                    Dictionary<MataliPhysicsObject, bool> table = new Dictionary<MataliPhysicsObject, bool>();
                    collisionTable.Add(mataliPhysicsObj, table);
                }
            }
            else
                mataliPhysicsObj.Integral.SetMass(physObj.Mass);

            mataliPhysicsObj.EnableCollisions = physObj.Collidable;
            mataliPhysicsObj.EnableMoving = physObj.Interactable;
            if (!physObj.ApplyGravity)
            {
                mataliPhysicsObj.EnableLocalGravity = true;
                mataliPhysicsObj.LocalGravityAcceleration = 0;
            }
            mataliPhysicsObj.EnableSleeping = !physObj.NeverDeactivate;
            mataliPhysicsObj.EnableScreenToRayInteraction = physObj.Pickable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// For all of the shapes that has directions (e.g., Cylinder, Hemisphere), Y direction is
        /// used, so if you would like it to face other directions, use MataliObject.ShapeOriginalMatrix
        /// to orient them.
        /// 
        /// For a cylinder with different bottom and top radius, IPhysicsObject.ShapeData are used.
        /// ShapeData[0] - bottom radius, ShapeData[1] - height, ShapeData[2] = top radius
        /// 
        /// For Compound shape, an additional information can be set by using
        /// MataliObject.CompoundShape.
        /// 
        /// For additional shape types such as Heightmap, Point, and so on, set Shape to ShapeType.Extra
        /// and define MataliObject.ExtraShape.
        /// </remarks>
        /// <param name="mataliPhysicsObj"></param>
        /// <param name="physObj"></param>
        private void SetShape(MataliPhysicsObject mataliPhysicsObj, IPhysicsObject physObj)
        {
            Vector3 boundingBox = Vector3.Zero;
            if(physObj.Model != null)
                boundingBox = Vector3Helper.GetDimensions(physObj.Model.MinimumBoundingBox);
            Vector3 size = Vector3.Zero;

            MataliObject mataliObj = null;
            if(physObj is MataliObject)
                mataliObj = (MataliObject)physObj;
            
            switch (physObj.Shape)
            {
                case ShapeType.Box:
                    if (physObj.ShapeData.Count == 3)
                        size = new Vector3(physObj.ShapeData[0], physObj.ShapeData[1], physObj.ShapeData[2]);
                    else
                        size = boundingBox;

                    size /= 2;
                    break;
                case ShapeType.Sphere:
                    if (physObj.ShapeData.Count == 1)
                        size = new Vector3(physObj.ShapeData[0], 0, 0);
                    else
                        size = boundingBox / 2;
                    break;
                case ShapeType.Cone:
                case ShapeType.Cylinder:
                case ShapeType.Capsule:
                    if (physObj.ShapeData.Count == 2)
                        size = new Vector3(physObj.ShapeData[0], physObj.ShapeData[1], physObj.ShapeData[0]);
                    else if(physObj.ShapeData.Count == 3)
                        size = new Vector3(physObj.ShapeData[0], physObj.ShapeData[1], physObj.ShapeData[2]);
                    else
                        size = new Vector3(boundingBox.X / 2, boundingBox.Y, boundingBox.X / 2);
                    break;
                case ShapeType.Compound:
                    // size is used solely for naming, not used for the collision shape
                    size = new Vector3(physObj.ShapeData.Count, physObj.ShapeData[0], 
                        physObj.ShapeData[physObj.ShapeData.Count - 1]);
                    break;
                case ShapeType.ConvexHull:
                    // size is used solely for naming, not used for the collision shape
                    size = new Vector3(physObj.MeshProvider.Vertices.Count,
                        physObj.MeshProvider.Indices.Count, physObj.MeshProvider.Vertices[0].X);
                    break;
                case ShapeType.Extra:
                    if (mataliObj == null)
                        throw new GoblinException("For extra shape type, you need to define the 'physObj' " +
                            "as MataliObject instance");

                    if (mataliObj.ExtraShape == ExtraShapeType.Undefined)
                        throw new GoblinException("Undefined type is not allowed if Extra shape type is specified");

                    // size is used solely for naming, not used for the collision shape
                    switch(mataliObj.ExtraShape)
                    {
                        case ExtraShapeType.Point:
                            if (physObj.ShapeData.Count != 3)
                                throw new GoblinException("For Point shape type, you need to specify the position (x,y,z) in ShapeData");

                            size = new Vector3(physObj.ShapeData[0], physObj.ShapeData[1], physObj.ShapeData[2]);
                            break;
                        case ExtraShapeType.Heightmap:
                            if (physObj.ShapeData.Count < 2)
                                throw new GoblinException("There needs to be at least two floats specifying the " +
                                    "width and height");

                            size = new Vector3(physObj.ShapeData[0], physObj.ShapeData[1], physObj.ShapeData.Count);
                            break;
                        case ExtraShapeType.Edge:
                            if (physObj.ShapeData.Count != 6)
                                throw new GoblinException("For Edge shape type, you need to specify the start and " +
                                    "end positions (x,y,z) in ShapeData");

                            size = new Vector3(physObj.ShapeData[0] + physObj.ShapeData[3], 
                                physObj.ShapeData[1] + physObj.ShapeData[4], 
                                physObj.ShapeData[2] + physObj.ShapeData[5]);
                            break;
                        default:
                            throw new GoblinException(mataliObj.ExtraShape.ToString() + " not implemented yet");
                    }
                    break;
            }

            String shapeName = physObj.Shape.ToString() + size.ToString();
            String primitiveName = physObj.Shape.ToString() + size.ToString();
            if (mataliObj != null)
            {
                String suffix = "";
                if (physObj.Shape == ShapeType.Extra)
                    suffix += mataliObj.ExtraShape.ToString();
                else if(physObj.Shape == ShapeType.Compound)
                    suffix += mataliObj.CompoundShape.ToString();
                suffix += mataliObj.ShapeOriginalMatrix.ToString();
                suffix += mataliObj.ShapeCollisionMargin;

                shapeName += suffix;
                primitiveName += suffix;
            }
            Shape shape = null;
            ShapePrimitive primitive = null;

            if (scene.Factory.ShapeManager.Contains(shapeName))
                shape = scene.Factory.ShapeManager.Find(shapeName);
            else
            {
                if(physObj.Shape != ShapeType.Compound)
                    primitive = scene.Factory.ShapePrimitiveManager.Create(primitiveName);
                shape = scene.Factory.ShapeManager.Create(shapeName);
                bool shapeSet = false;
                switch (physObj.Shape)
                {
                    case ShapeType.Box:
                        primitive.CreateBox(size.X, size.Y, size.Z);
                        break;
                    case ShapeType.Sphere:
                        primitive.CreateSphere(size.X);
                        break;
                    case ShapeType.Cone:
                        primitive.CreateConeY(size.Y, size.X);
                        break;
                    case ShapeType.Cylinder:
                        if (size.X != size.Z)
                            primitive.CreateCylinder2RY(size.Y, size.X, size.Z);
                        else
                            primitive.CreateCylinderY(size.Y, size.X);
                        break;
                    case ShapeType.Capsule:
                        primitive.CreateCapsuleY(size.Y - size.X * 2, size.X);
                        break;
                    case ShapeType.Compound:
                        ShapeCompoundType type = ShapeCompoundType.ConvexHull;
                        if (mataliObj != null)
                            if (mataliObj.CompoundShape == CompoundShapeType.MinkowskiSum)
                                type = ShapeCompoundType.MinkowskiSum;

                        int dataIndex = 0;
                        Shape compoundShapePart = null;
                        ShapePrimitive compoundPrimitive = null;
                        float[] matrixVals = new float[16];
                        while (dataIndex < physObj.ShapeData.Count)
                        {
                            ShapeType shapeType = (ShapeType)Enum.ToObject(typeof(ShapeType), (int)physObj.ShapeData[dataIndex++]);
                            switch (shapeType)
                            {
                                case ShapeType.Cylinder:
                                    size = new Vector3(physObj.ShapeData[dataIndex], physObj.ShapeData[dataIndex + 1],
                                        physObj.ShapeData[dataIndex]);
                                    dataIndex += 2;
                                    break;
                                case ShapeType.Sphere:
                                    size = new Vector3(physObj.ShapeData[dataIndex], 0, 0);
                                    dataIndex++;
                                    break;
                            }

                            shapeName = shapeType.ToString() + size.ToString();
                            primitiveName = shapeType.ToString() + size.ToString();

                            if (scene.Factory.ShapeManager.Contains(shapeName))
                                compoundShapePart = scene.Factory.ShapeManager.Find(shapeName);
                            else
                            {
                                compoundPrimitive = scene.Factory.ShapePrimitiveManager.Create(primitiveName);
                                compoundShapePart = scene.Factory.ShapeManager.Create(shapeName);
                                switch (shapeType)
                                {
                                    case ShapeType.Cylinder:
                                        compoundPrimitive.CreateCylinderY(size.Y, size.X);
                                        break;
                                    case ShapeType.Sphere:
                                        compoundPrimitive.CreateSphere(size.X);
                                        break;
                                    default:
                                        throw new GoblinException(shape.ToString() + " is not supported yet as a compound part");
                                }

                                compoundShapePart.Set(compoundPrimitive, Matrix.Identity, 0.0f);
                            }

                            for (int i = 0; i < 16; ++i)
                                matrixVals[i] = physObj.ShapeData[dataIndex + i];
                            dataIndex += 16;
                            shape.Add(compoundShapePart, MatrixHelper.FloatsToMatrix(matrixVals), 0.0f, type);
                        }

                        float margin = 0.0f;
                        if(mataliObj != null)
                            margin = mataliObj.ShapeCollisionMargin;
                        shape.CreateMesh(margin);

                        shapeSet = true;
                        break;
                    case ShapeType.ConvexHull:
                    case ShapeType.TriangleMesh:
                        float[] frictions = null;
                        float[] restitutions = null;

                        if (physObj.Shape == ShapeType.ConvexHull)
                            primitive.CreateConvex(physObj.MeshProvider.Vertices);
                        else
                        {
                            int triangleCount = physObj.MeshProvider.Indices.Count / 3;
                            frictions = new float[triangleCount];
                            restitutions = new float[triangleCount];

                            for (int i = 0; i < frictions.Length; i++)
                            {
                                frictions[i] = 1.0f;
                                restitutions[i] = 0.0f;
                            }

                            Vector3[] triVerts = new Vector3[physObj.MeshProvider.Indices.Count];
                            for (int i = 0; i < triVerts.Length; ++i)
                                triVerts[i] = physObj.MeshProvider.Vertices[physObj.MeshProvider.Indices[i]];

                            bool flipTriangle = (physObj is MataliObject) ? ((MataliObject)physObj).FlipTriangleOrder : true;
                            primitive.CreateTriangleMesh(flipTriangle, 2, frictions, restitutions, 1.0f, 0.0f, triVerts);
                        }

                        break;
                    case ShapeType.Extra:
                        switch (mataliObj.ExtraShape)
                        {
                            case ExtraShapeType.Heightmap:
                                int width = (int)physObj.ShapeData[0];
                                int height = (int)physObj.ShapeData[1];
                                float[] heightData = new float[height * width];
                                float[] heightFrictions = new float[height * width];
                                float[] heightRestituions = new float[height * width];

                                if (physObj.ShapeData.Count < (2 + heightData.Length))
                                    throw new GoblinException("You also need to specify the hegith map data");
                                Buffer.BlockCopy(physObj.ShapeData.ToArray(), 2 * sizeof(float), heightData, 0,
                                    heightData.Length * sizeof(float));

                                if (physObj.ShapeData.Count > (2 + heightData.Length * 2))
                                    Buffer.BlockCopy(physObj.ShapeData.ToArray(), (2 + heightData.Length) * sizeof(float),
                                        heightFrictions, 0, heightFrictions.Length * sizeof(float));
                                else
                                    for (int i = 0; i < heightFrictions.Length; ++i)
                                        heightFrictions[i] = mataliObj.TriangleMeshFriction;

                                if (physObj.ShapeData.Count > (2 + heightData.Length * 3))
                                    Buffer.BlockCopy(physObj.ShapeData.ToArray(), (2 + heightData.Length * 2) * sizeof(float),
                                        heightRestituions, 0, heightRestituions.Length * sizeof(float));
                                else
                                    for (int i = 0; i < heightRestituions.Length; ++i)
                                        heightRestituions[i] = mataliObj.TriangleMeshRestitution;

                                primitive.CreateHeightmap(0, 0, width, height, width, height, heightData, heightFrictions, heightRestituions,
                                    mataliObj.TriangleMeshFriction, mataliObj.TriangleMeshRestitution, mataliObj.IsDynamic);
                                shape.Set(primitive, mataliObj.ShapeOriginalMatrix, mataliObj.ShapeCollisionMargin);
                                shape.CreateMesh(0.0f);
                                shapeSet = true;

                                mataliPhysicsObj.InternalControllers.CreateHeightmapController(true);
                                break;
                            case ExtraShapeType.Point:
                                primitive.CreatePoint(physObj.ShapeData[0], physObj.ShapeData[1], physObj.ShapeData[2]);
                                break;
                            case ExtraShapeType.Edge:
                                primitive.CreateEdge(new Vector3(physObj.ShapeData[0], physObj.ShapeData[1], physObj.ShapeData[2]),
                                    new Vector3(physObj.ShapeData[3], physObj.ShapeData[4], physObj.ShapeData[5]));
                                break;
                            case ExtraShapeType.Plane:
                                primitive.CreatePlaneY(physObj.ShapeData[0], (physObj.ShapeData[1] > 0));
                                break;
                            case ExtraShapeType.Triangle:
                                primitive.CreateTriangle(
                                    new Vector3(physObj.ShapeData[0], physObj.ShapeData[1], physObj.ShapeData[2]),
                                    new Vector3(physObj.ShapeData[3], physObj.ShapeData[4], physObj.ShapeData[5]),
                                    new Vector3(physObj.ShapeData[6], physObj.ShapeData[7], physObj.ShapeData[8]));
                                break;
                            case ExtraShapeType.Tetrahedron:
                                primitive.CreateTetrahedron(
                                    new Vector3(physObj.ShapeData[0], physObj.ShapeData[1], physObj.ShapeData[2]),
                                    new Vector3(physObj.ShapeData[3], physObj.ShapeData[4], physObj.ShapeData[5]),
                                    new Vector3(physObj.ShapeData[6], physObj.ShapeData[7], physObj.ShapeData[8]),
                                    new Vector3(physObj.ShapeData[9], physObj.ShapeData[10], physObj.ShapeData[11]));
                                break;
                            case ExtraShapeType.Fluid:
                                break;
                            case ExtraShapeType.Hemisphere:
                                primitive.CreateHemisphereY(physObj.ShapeData[0]);
                                break;
                        }
                        break;
                }

                if (!shapeSet)
                {
                    if (mataliObj != null)
                        shape.Set(primitive, mataliObj.ShapeOriginalMatrix, mataliObj.ShapeCollisionMargin);
                    else
                        shape.Set(primitive, Matrix.Identity, 0.0f);

                    if (buildCollisionMesh)
                    {
                        float margin = 0.0f;
                        if (mataliObj != null)
                            margin = mataliObj.ShapeCollisionMargin;
                        shape.CreateMesh(margin);
                    }
                }
            }

            mataliPhysicsObj.Shape = shape;
        }

        private void UpdateTransforms(SimulateMethodArgs args)
        {
            MataliPhysicsObject mataliPhysicsObj = scene.Factory.PhysicsObjectManager.Get(args.OwnerIndex);
            
            mataliPhysicsObj.MainWorldTransform.GetTransformMatrix(ref tempMat1);
            ((IPhysicsObject)mataliPhysicsObj.UserTagObj).PhysicsWorldTransform = tempMat1;
        }

        private void CheckCollision(CollisionMethodArgs args)
        {
            MataliPhysicsObject mataliPhysicsObj = scene.Factory.PhysicsObjectManager.Get(args.OwnerIndex);
            MataliObject mataliObj = (MataliObject)mataliPhysicsObj.UserTagObj;
            Dictionary<MataliPhysicsObject, bool> table = collisionTable[mataliPhysicsObj];

            for (int i = 0; i < args.Collisions.Count; i++)
            {
                MataliPhysicsObject collidingObject = scene.Factory.PhysicsObjectManager.Get(args.Collisions[i]);
                if (table.ContainsKey(collidingObject))
                {
                    if (mataliObj.CollisionContinueCallback != null)
                        mataliObj.CollisionContinueCallback(mataliPhysicsObj, collidingObject);
                }
                else
                {
                    table.Add(collidingObject, true);
                    if (mataliObj.CollisionStartCallback != null)
                        mataliObj.CollisionStartCallback(mataliPhysicsObj, collidingObject);
                }
            }
        }

        #endregion
    }
}
