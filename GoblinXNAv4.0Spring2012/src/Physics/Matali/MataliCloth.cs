using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;
using GoblinXNA.Graphics.Geometry;
using Komires.MataliPhysics;

namespace GoblinXNA.Physics.Matali
{
    public class MataliCloth : MataliObject
    {
        #region Member Fields

        protected List<ConstraintPair> constraints;
        protected List<MataliObject> particles;
        protected MataliObject particleTemplate;
        protected string uniqueName;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a cloth using physical methods with particles.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="particleTemplate">A template to use for the cloth particles</param>
        public MataliCloth(object container, string uniqueName, MataliObject particleTemplate)
            : base(container)
        {
            this.uniqueName = uniqueName;
            if (particleTemplate == null)
            {
                this.particleTemplate = new MataliObject(null);
                this.particleTemplate.Shape = ShapeType.Extra;
                this.particleTemplate.ExtraShape = ExtraShapeType.Point;
                this.particleTemplate.ShapeData.Add(0);
                this.particleTemplate.ShapeData.Add(0);
                this.particleTemplate.ShapeData.Add(0);
                this.particleTemplate.RelativeTransform = Matrix.CreateScale(0.1f);
                this.particleTemplate.ShapeCollisionMargin = 0.1f;
                this.particleTemplate.Density = 1;
            }
            else
                this.particleTemplate = particleTemplate;

            constraints = new List<ConstraintPair>();
            particles = new List<MataliObject>();

            Tearable = false;
            MinTearVelocity = 20;
            Streachable = true;
            Stiffness = 0.1f;
        }

        public MataliCloth(object container, string uniqueName)
            : this(container, uniqueName, null)
        {
        }

        #endregion

        #region Properties

        public List<ConstraintPair> Constraints
        {
            get { return constraints; }
        }

        public List<MataliObject> Particles
        {
            get { return particles; }
        }

        public bool Tearable
        {
            get;
            set;
        }

        public float MinTearVelocity
        {
            get;
            set;
        }

        public bool Streachable
        {
            get;
            set;
        }

        /// <summary>
        /// The stiffness of the cloth. The larger the value, the stiffer.
        /// </summary>
        public float Stiffness
        {
            get;
            set;
        }

        #endregion

        #region Overriden Properties

        public override IModel Model
        {
            get { return base.Model; }
            set 
            { 
                base.Model = value;

                if (model is IPhysicsMeshProvider)
                {
                    IPhysicsMeshProvider mesh = (IPhysicsMeshProvider)model;
                    if (mesh.PrimitiveType != PrimitiveType.TriangleList)
                        throw new GoblinException(mesh.PrimitiveType.ToString() + " is not supported. " +
                            "Only TriangleList type is supported currently");

                    ConstructClothParticles(mesh.Vertices, mesh.Indices);
                }
            }
        }

        #endregion

        #region Private Methods

        private void ConstructClothParticles(List<Vector3> vertices, List<int> indices)
        {
            foreach (Vector3 vertex in vertices)
            {
                MataliObject particle = new MataliObject(null);
                particle.Copy(particleTemplate);
                particle.Collidable = true;
                particle.Interactable = true;
                particle.CompoundInitialWorldTransform =
                    particle.RelativeTransform * 
                    RelativeTransform *
                    Matrix.CreateTranslation(vertex);

                particles.Add(particle);
            }

            // use a hashmap to make sure that we don't create duplicate constraints between the same
            // two vertices
            Dictionary<string, bool> constraintTable = new Dictionary<string, bool>();
            int id0, id1, s, l;
            string key;
            float distance;
            Vector3 position1 = Vector3.Zero, position2 = Vector3.Zero;
            for (int i = 0; i < indices.Count; i += 3)
            {
                for (int j = 0; j < 3; ++j)
                {
                    id0 = indices[i + j];
                    id1 = indices[i + (j + 1) % 3];

                    s = Math.Min(id0, id1);
                    l = Math.Max(id0, id1);

                    key = s + "_" + l;
                    if (!constraintTable.ContainsKey(key))
                    {
                        constraintTable.Add(key, true);

                        ConstraintPair pair = new ConstraintPair();
                        pair.Name = uniqueName + " Point Cloth Constraint " + key;
                        pair.PhysicsObject1 = particles[s];
                        pair.PhysicsObject2 = particles[l];
                        pair.Callback = delegate(Constraint constraint)
                        {
                            constraint.PhysicsObject1.MainWorldTransform.GetPosition(ref position1);
                            constraint.PhysicsObject2.MainWorldTransform.GetPosition(ref position2);
                            constraint.SetAnchor1(position1);
                            constraint.SetAnchor2(position2);
                            distance = Vector3.Distance(position1, position2);
                            constraint.Distance = Streachable ? distance : -distance;
                            constraint.Force = Stiffness;
                            constraint.EnableBreak = Tearable;
                            constraint.MinBreakVelocity = MinTearVelocity;
                        };

                        constraints.Add(pair);
                    }
                }
            }

            constraintTable.Clear();
        }

        #endregion
    }
}
