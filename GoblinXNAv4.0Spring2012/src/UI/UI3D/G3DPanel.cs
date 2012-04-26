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
    internal class G3DPanel : G3DComponent
    {
        #region Member Fields
        protected List<G3DComponent> children;
        protected String title;

        #region For Drawing purpose only
        VertexPositionColorTexture[] coords;
        #endregion
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a default GoblinXNA 2D GUI panel
        /// </summary>
        public G3DPanel(Matrix center, Model widget, Effect effect, float alpha) 
            : base(center, widget, effect, alpha)
        {
            children = new List<G3DComponent>();

            name = "G3DPanel";
            title = "";

            coords = new VertexPositionColorTexture[8];
            for (int i = 0; i < 8; i++)
                coords[i].Color = backgroundColor;
        }

        public G3DPanel(Matrix center, float alpha) : this(center, null, null, alpha) { }

        public G3DPanel() : this(Matrix.Identity, null, null, 1.0f) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets whether this panel has any children components added
        /// </summary>
        public virtual bool HasChildren
        {
            get { return children.Count > 0; }
        }

        /// <summary>
        /// Gets all of the components added to this panel
        /// </summary>
        public virtual List<G3DComponent> Children
        {
            get { return children; }
        }
        #endregion

        #region Override Properties
        public override Color BackgroundColor
        {
            get
            {
                return base.BackgroundColor;
            }
            set
            {
                base.BackgroundColor = value;
                backgroundColor = new Color(value.R, value.G, value.B, alpha);

                SetupVertexIndexBuffer();
            }
        }

        public override float Transparency
        {
            get
            {
                return base.Transparency;
            }
            set
            {
                base.Transparency = value;
                backgroundColor = new Color(borderColor.R, borderColor.G, borderColor.B, alpha);

                SetupVertexIndexBuffer();

                foreach (G3DComponent comp in children)
                {
                    comp.Transparency = alpha;
                }
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;

                foreach (G3DComponent child in children)
                    child.Visible = value;
            }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;

                foreach (G3DComponent child in children)
                    child.Enabled = value;
            }
        }

        public override Vector3 HalfExtent
        {
            get
            {
                return base.HalfExtent;
            }
            set
            {
                base.HalfExtent = value;

                SetupVertexIndexBuffer();
            }
        }
        #endregion  

        #region Get Methods

        /// <summary>
        /// Get the component at ith position if exists. If no component exists at ith position,
        /// it will return a null.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual G3DComponent GetComponentAt(int i)
        {
            if (i < 0 || i >= children.Count)
                return null;
            else
                return children[i];
        }

        #endregion

        #region Action Methods
        /// <summary>
        /// Add a child component to this panel
        /// </summary>
        /// <param name="comp"></param>
        public virtual void Add(G3DComponent comp)
        {
            if (comp != null && !children.Contains(comp))
            {
                comp.Parent = this;
                comp.Transparency = alpha;
                children.Add(comp);

                G3DPanel root = (G3DPanel)comp.RootParent;
                //InvokeComponentAddedEvent(this, comp);
            }
        }
        /// <summary>
        /// Remove a child component from this panel if already added
        /// </summary>
        /// <param name="comp"></param>
        /// <returns></returns>
        public virtual bool Remove(G3DComponent comp)
        {
            bool removed = children.Remove(comp);
            if (removed)
            {
                comp.Parent = null;

                //InvokeComponentRemovedEvent(this, comp);
            }

            return removed;
        }
        /// <summary>
        /// Remove a child component from this panel at a specific index
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual bool RemoveComponentAt(int i)
        {
            if (i < 0 || i >= children.Count)
                return false;
            else
            {
                return Remove(children[i]);
            }
        }
        /// <summary>
        /// Remove all of the child components added to this panel
        /// </summary>
        public virtual void RemoveAllComponents()
        {
            foreach (G3DComponent comp in children)
                Remove(comp);
        }

        #endregion

        #region Override Methods
        

        protected override void PaintComponent()
        {
            if (!HasParent)
            {
                if (widgetComponent != null)
                    base.PaintComponent();
                else
                {
                    /*if (backTexture != null)
                    {
                        GoblinSetting.Shader.UseTextureTechnique();
                        GoblinSetting.Shader.TextureParam.SetValue(backTexture);
                    }
                    else
                        GoblinSetting.Shader.UseColorTechnique();

                    GoblinSetting.Shader.WorldParam.SetValue(paintCenter);

                    GoblinSetting.Shader.RenderPrimitives(vb, ib, VertexPositionColorTexture.VertexElements,
                        VertexPositionColorTexture.SizeInBytes, 8, 12);*/
                }
            }
        }

        public override void RenderWidget()
        {
            base.RenderWidget();

            // Also render all of its children components
            foreach (G3DComponent comp in children)
            {
                comp.RenderWidget();
            }
        }
        #endregion

        #region Inner Class Methods
        /// <summary>
        /// 
        ///       0             1
        ///        -------------
        ///       /|           /|
        ///    3 / |        2 / |
        ///     --------------  |
        ///     | 4|_________|__| 5
        ///     | /          | /
        ///     |/           |/
        ///   7 -------------- 6
        /// 
        /// </summary>
        protected virtual void SetupVertexIndexBuffer()
        {
            Dispose();

            Color c = (backTexture == null) ? backgroundColor : new Color(255, 255, 255, alpha);
            for (int i = 0; i < 8; i++)
                coords[i].Color = c;

            coords[0].Position = new Vector3(-halfExtent.X, -halfExtent.Y, halfExtent.Z);
            coords[0].TextureCoordinate = new Vector2(0, 1);
            coords[1].Position = new Vector3(halfExtent.X, -halfExtent.Y, halfExtent.Z);
            coords[1].TextureCoordinate = new Vector2(1, 1);
            coords[2].Position = new Vector3(halfExtent.X, halfExtent.Y, halfExtent.Z);
            coords[2].TextureCoordinate = new Vector2(1, 0);
            coords[3].Position = new Vector3(-halfExtent.X, halfExtent.Y, halfExtent.Z);
            coords[3].TextureCoordinate = new Vector2(0, 0);

            coords[4].Position = new Vector3(-halfExtent.X, -halfExtent.Y, -halfExtent.Z);
            coords[5].Position = new Vector3(halfExtent.X, -halfExtent.Y, -halfExtent.Z);
            coords[6].Position = new Vector3(halfExtent.X, halfExtent.Y, -halfExtent.Z);
            coords[7].Position = new Vector3(-halfExtent.X, halfExtent.Y, -halfExtent.Z);

            vb = new VertexBuffer(State.Device, typeof(VertexPositionColorTexture), 8, 
                BufferUsage.WriteOnly);
            vb.SetData(coords);

            #region Index assignment
            short[] indices = new short[36];

            indices[0] = 0;
            indices[1] = 3;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 1;
            indices[6] = 0;
            indices[7] = 4;
            indices[8] = 3;
            indices[9] = 3;
            indices[10] = 4;
            indices[11] = 7;
            indices[12] = 0;
            indices[13] = 1;
            indices[14] = 4;
            indices[15] = 1;
            indices[16] = 5;
            indices[17] = 4;
            indices[18] = 1;
            indices[19] = 2;
            indices[20] = 5;
            indices[21] = 2;
            indices[22] = 6;
            indices[23] = 5;
            indices[24] = 2;
            indices[25] = 3;
            indices[26] = 7;
            indices[27] = 2;
            indices[28] = 7;
            indices[29] = 6;
            indices[30] = 7;
            indices[31] = 4;
            indices[32] = 6;
            indices[33] = 6;
            indices[34] = 4;
            indices[35] = 5;
            #endregion

            ib = new IndexBuffer(State.Device, typeof(short), 36, BufferUsage.WriteOnly);
            ib.SetData(indices);
        }

        /// <summary>
        /// Get the parent right before reaching the root
        /// </summary>
        /// <param name="comp"></param>
        /// <returns></returns>
        protected virtual G3DPanel GetPreRootParent(G3DComponent comp)
        {
            if (comp.HasParent && ((G3DPanel)comp.Parent).HasParent)
                return GetPreRootParent((G3DComponent)comp.Parent);
            else
            {
                if (comp is G3DPanel)
                    return (G3DPanel)comp;
                else
                    return null;
            }
        }

        protected virtual void ResetParent()
        {
            foreach (G3DComponent child in children)
            {
                child.Parent = this;
                if(child is G3DPanel)
                {
                    ((G3DPanel)child).ResetParent();
                }
            }
        }
        #endregion
    }
}
