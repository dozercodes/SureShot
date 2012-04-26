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

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// A panel that can hold other 2D UI components. Each added component is drawn relative to
    /// its parent panel.
    /// </summary>
    public class G2DPanel : G2DComponent
    {
        #region Member Fields
        public static Color DEFAULT_BORDER_COLOR = Color.LightGray;
        protected List<G2DComponent> children;
        protected GoblinEnums.BorderFactory border;
        protected String title;

        #region For Drawing purpose only
        protected Color etchWhiteColor;
        protected Color etchBlackColor;
        protected Rectangle etchedBorderRect;
        #endregion
        #endregion

        #region Events
        /// <summary>
        /// An event triggered whenever a component is added to this panel.
        /// </summary>
        public event ComponentAdded ComponentAddedEvent;

        /// <summary>
        /// An event triggered whenever a component is removed from this panel.
        /// </summary>
        public event ComponentRemoved ComponentRemovedEvent;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a panel to hold other 2D UI components.
        /// </summary>
        public G2DPanel() 
            : base()
        {
            children = new List<G2DComponent>();
            border = GoblinEnums.BorderFactory.EmptyBorder;
            name = "G2DPanel";
            title = "";

            etchWhiteColor = Color.White;
            etchBlackColor = Color.Black;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the border type.
        /// </summary>
        public virtual GoblinEnums.BorderFactory Border
        {
            get { return border; }
            set { border = value; }
        }
        /// <summary>
        /// Gets whether this panel has any child components added.
        /// </summary>
        public virtual bool HasChildren
        {
            get { return children.Count > 0; }
        }
        /// <summary>
        /// Gets the child components added to this panel.
        /// </summary>
        public virtual List<G2DComponent> Children
        {
            get { return children; }
        }
        #endregion

        #region Override Properties
        public override Component Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                base.Parent = value;

                // Recompute children's bounds if this panel becomes
                // a child of another panel
                foreach (G2DComponent comp in children)
                    comp.Bounds = comp.Bounds;

                UpdateBorderBounds();
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

                borderColor.A = etchBlackColor.A = etchWhiteColor.A = alpha;

                foreach (G2DComponent child in children)
                    child.Transparency = value;
            }
        }

        public override float TextTransparency
        {
            get
            {
                return base.TextTransparency;
            }
            set
            {
                base.TextTransparency = value;

                foreach (G2DComponent child in children)
                    child.TextTransparency = value;
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

                foreach (G2DComponent child in children)
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

                foreach (G2DComponent child in children)
                    child.Enabled = value;
            }
        }

        public override Rectangle Bounds
        {
            get
            {
                return base.Bounds;
            }
            set
            {
                base.Bounds = value;
                ResetParent();

                UpdateBorderBounds();
            }
        }
        #endregion

        #region Not Implemented Methods
        /*/// <summary>
        /// Set the border to be a titled border with a title
        /// </summary>
        /// <param name="title"></param>
        public virtual void SetTitledBorder(String title)
        {
            this.title = title;
            this.border = GoblinEnums.BorderFactory.TitledBorder;
        }*/
        #endregion

        #region Action Methods
        /// <summary>
        /// Adds a child component to this panel.
        /// </summary>
        /// <remarks>
        /// Note that when you add a child component, the child component will automatically inherit the following
        /// properties from this panel: Transparency, TextTransparency, Visible, and Enabled. If you want to have
        /// a different value for these properties, then you should set these property values of the child component 
        /// after adding to this panel. 
        /// </remarks>
        /// <param name="comp">A 2D component to be added</param>
        public virtual void AddChild(G2DComponent comp)
        {
            if (comp.Parent != null)
                throw new GoblinException("This G2DComponent already has a parent");

            if (comp != null && !children.Contains(comp))
            {
                comp.Parent = this;
                comp.Transparency = Transparency;
                comp.TextTransparency = TextTransparency;
                comp.Visible = Visible;
                comp.Enabled = Enabled;
                comp.RegisterKeyInput();
                comp.RegisterMouseInput();
                children.Add(comp);

                G2DPanel root = (G2DPanel)comp.RootParent;

                InvokeComponentAddedEvent(this, comp);
            }
        }
        /// <summary>
        /// Removes a child component from this panel if already added.
        /// </summary>
        /// <param name="comp">A 2D component to be removed</param>
        /// <returns>Whether removal succeeded</returns>
        public virtual bool RemoveChild(G2DComponent comp)
        {
            bool removed = children.Remove(comp);
            if (removed)
            {
                comp.Parent = null;
                comp.RemoveKeyInput();
                comp.RemoveMouseInput();

                InvokeComponentRemovedEvent(this, comp);
            }

            return removed;
        }
        /// <summary>
        /// Removes a child component from this panel at a specific index.
        /// </summary>
        /// <param name="i">The index where a child component will be removed</param>
        /// <returns>Whether removal succeeded</returns>
        public virtual bool RemoveChildAt(int i)
        {
            if (i < 0 || i >= children.Count)
                return false;
            else
            {
                return RemoveChild(children[i]);
            }
        }
        /// <summary>
        /// Removes all the child components added to this panel.
        /// </summary>
        public virtual void RemoveChildren()
        {
            foreach (G2DComponent comp in children)
                RemoveChild(comp);
        }

        #endregion

        #region Override Methods

        protected override void PaintBorder()
        {
            switch (border)
            {
                case GoblinEnums.BorderFactory.EtchedBorder:
                    UI2DRenderer.DrawRectangle(etchedBorderRect, etchWhiteColor, 1);
                    UI2DRenderer.DrawRectangle(paintBounds, etchBlackColor, 1);
                    break;

                case GoblinEnums.BorderFactory.LineBorder:
                    base.PaintBorder();
                    break;

                /*case GoblinEnums.BorderFactory.LoweredBevelBorder:

                    break;

                case GoblinEnums.BorderFactory.RaisedBevelBorder:
                    break;

                case GoblinEnums.BorderFactory.TitledBorder:
                    break;*/
            }
        }

        protected override void PaintComponent()
        {
            if(!HasParent)
                base.PaintComponent();
        }

        public override void RenderWidget()
        {
            base.RenderWidget();

            // Also render all of its children components
            foreach (G2DComponent comp in children)
            {
                comp.RenderWidget();
            }
        }
        #endregion

        #region Protected Methods

        protected void InvokeComponentAddedEvent(object source, Component component)
        {
            if (ComponentAddedEvent != null)
                ComponentAddedEvent(source, component);
        }

        protected void InvokeComponentRemovedEvent(object source, Component component)
        {
            if (ComponentRemovedEvent != null)
                ComponentRemovedEvent(source, component);
        }

        /// <summary>
        /// Gets the parent right before reaching the root
        /// </summary>
        /// <param name="comp"></param>
        /// <returns></returns>
        protected virtual G2DPanel GetPreRootParent(G2DComponent comp)
        {
            if (comp.HasParent && ((G2DPanel)comp.Parent).HasParent)
                return GetPreRootParent((G2DComponent)comp.Parent);
            else
            {
                if (comp is G2DPanel)
                    return (G2DPanel)comp;
                else
                    return null;
            }
        }

        protected virtual void ResetParent()
        {
            foreach (G2DComponent child in children)
            {
                child.Parent = this;
                if(child is G2DPanel)
                {
                    ((G2DPanel)child).ResetParent();
                }
            }
        }

        protected virtual void UpdateBorderBounds()
        {
            etchedBorderRect = new Rectangle(paintBounds.X + 1, paintBounds.Y + 1, paintBounds.Width, paintBounds.Height);
        }

        protected void ManageFocusGroup(object source)
        {
            foreach (G2DComponent child in children)
                if (!child.Equals(source))
                    child.Focused = false;
        }

        #endregion
    }
}
