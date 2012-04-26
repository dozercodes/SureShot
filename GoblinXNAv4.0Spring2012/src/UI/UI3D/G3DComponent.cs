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
using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.UI.UI3D
{
    /// <summary>
    /// NOT FINISHED YET.
    /// </summary>
    internal class G3DComponent : Component, IDisposable
    {
        #region Member Fields
        /// <summary>
        /// Original center of the widget
        /// </summary>
        protected Matrix center;
        /// <summary>
        /// 
        /// </summary>
        protected Matrix paintCenter;
        /// <summary>
        /// TVMesh objects that are used to render the 3D widget
        /// </summary>
        protected Model widgetComponent;
        /// <summary>
        /// Indicates whether to show shadow volume for this widget
        /// </summary>
        protected bool showShadow;
        /// <summary>
        /// 
        /// </summary>
        protected Effect widgetEffect;

        protected String effectTechnique;

        protected String effectWorldMatrix;

        protected Vector3 halfExtent;

        protected VertexBuffer vb;
        protected IndexBuffer ib;

        protected GoblinEnums.DisplayConfig displayMode;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a GoblinXNA 3D GUI component
        /// </summary>
        /// <param name="center">A Matrix that defines the center location and orientation
        /// of the entire widget components</param>
        /// <param name="alpha">Transparency value of this component [0.0f - 1.0f]</param>
        public G3DComponent(Matrix center, Model widget, Effect effect, float alpha) : base(Color.White, alpha)
        {
            Transform = center;
            widgetEffect = effect;
            widgetComponent = widget;
            name = "G3DComponent";
            widgetComponent = null;
            showShadow = false;
            halfExtent = new Vector3();

            displayMode = GoblinEnums.DisplayConfig.WorldFixed;
        }

        /// <summary>
        /// Constructs a GoblinXNA 3D GUI component
        /// </summary>
        /// <param name="center">A Matrix that defines the center location and orientation
        /// of the entire widget components</param>
        public G3DComponent(Matrix center, float alpha) :
            this(center, null, null, DEFAULT_ALPHA){ }

        public G3DComponent(Matrix center) :
            this(center, null, null, DEFAULT_ALPHA) { }

        /// <summary>
        /// Constructs a default GoblinXNA 3D GUI component
        /// </summary>
        public G3DComponent() :
            this(Matrix.Identity, null, null, DEFAULT_ALPHA) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or Sets the Matrix that defines the center location and orientation of the entire
        /// widget components
        /// </summary>
        public virtual Matrix Transform
        {
            get { return center; }
            set
            {
                if (value != null)
                {
                    this.center = value;
                    this.paintCenter = value;
                }
            }
        }

        public virtual Vector3 HalfExtent
        {
            get { return halfExtent; }
            set { halfExtent = value; }
        }

        public virtual Vector3 Center
        {
            get { return center.Translation; }
        }

        public virtual GoblinEnums.DisplayConfig DisplayMode
        {
            get { return displayMode; }
            set { displayMode = value; }
        }
        #endregion

        #region Set Methods

        public virtual void SetWidgetEffect(Effect effect, String effectTechnique, String effectWorldMatrix)
        {
            widgetEffect = effect;
            this.effectTechnique = effectTechnique;
            this.effectWorldMatrix = effectWorldMatrix;
        }

        public virtual void SetWidgetComponent(String modelName)
        {
            if (widgetEffect == null)
                throw new GoblinException("Widget Effect needs to be set before you can call this method");

            widgetComponent = FillModelFromFile(modelName);
        }

        public virtual void SetWidgetComponent(Model model)
        {
            widgetComponent = model;
        }

        public virtual void EnableShadow(bool b)
        {
            showShadow = b;
        }
        #endregion

        #region Override Properties
        /// <summary>
        /// Gets or Sets the parent of this component
        /// </summary>
        public override Component Parent
        {
            get { return parent; }
            set
            {
                G3DComponent g3dParent = null;

                try
                {
                    if (value != null)
                        g3dParent = (G3DComponent)value;
                }
                catch (Exception)
                {
                    throw new GoblinException("Can not assign non-G3DComponent as a parent of a G3DComponent");
                }

                base.Parent = value;

                paintCenter = UI3DHelper.CopyMatrix(center);

                if (g3dParent != null)
                {
                    // We want to have the child widget translates by the center of the parent
                    // and rotates by the orientation of the parent

                    // We first multiply the pure rotation matrix of the child and parent matrix
                    Matrix parentMat = UI3DHelper.CopyMatrix(g3dParent.Transform);
                    parentMat.M41 = parentMat.M42 = parentMat.M43 = 0;
                    paintCenter.M41 = paintCenter.M42 = paintCenter.M43 = 0;
                    paintCenter = Matrix.Multiply(parentMat, paintCenter);
                    // Finally, we translate the matrix to parent location + child location
                    paintCenter *= Matrix.CreateTranslation(Vector3.Add(Center, g3dParent.Center));
                }
            }
        }
        #endregion

        #region Paint Methods
        /// <summary>
        /// Implement how the component should be painted. 
        /// NOTE: This base class method only paints the background
        /// </summary>
        /// <param name="Scr2D"></param>
        /// <param name="ScrText"></param>
        protected virtual void PaintComponent() {

            if (!drawBackground)
                return;

            if (widgetComponent != null && widgetEffect == null)
                throw new GoblinException("Widget Effect needs to be set before you can call this method");

            // Render the 3D widget components
            foreach (EffectPass pass in widgetEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (ModelMesh modmesh in widgetComponent.Meshes)
                {
                    foreach (Effect currenteffect in modmesh.Effects)
                    {
                        currenteffect.CurrentTechnique = currenteffect.Techniques[effectTechnique];
                        currenteffect.Parameters[effectWorldMatrix].SetValue(paintCenter);
                    }
                    modmesh.Draw();
                }
            }
        }
        /// <summary>
        /// Implements how this component should be rendered
        /// </summary>
        /// <param name="Scr2D"></param>
        /// <param name="ScrText"></param>
        public virtual void RenderWidget() 
        {
            if (!visible)
                return;

            PaintComponent();
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Get the name of this component
        /// </summary>
        /// <returns>Name of this component</returns>
        public override string ToString()
        {
            return name;
        }
        #endregion

        #region Inner Class Methods
        protected Model FillModelFromFile(string asset)
        {
            Model mod = State.Content.Load<Model>(asset);
            foreach (ModelMesh modmesh in mod.Meshes)
                foreach (ModelMeshPart modmeshpart in modmesh.MeshParts)
                    modmeshpart.Effect = widgetEffect.Clone();
            return mod;
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (vb != null)
            {
                vb.Dispose();
                ib.Dispose();
            }
        }

        #endregion
    }
}
