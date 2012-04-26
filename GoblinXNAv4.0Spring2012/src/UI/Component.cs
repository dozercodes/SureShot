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
using Microsoft.Xna.Framework.Input;

using GoblinXNA.Device.Generic;

namespace GoblinXNA.UI
{
    #region Enum

    public enum SelectionType
    {
        Selection,
        Deselection,
        Mixed
    }

    #endregion

    #region Event Delegates

    /// <summary>
    /// Invoked when the component is focused
    /// </summary>
    /// <param name="source"></param>
    public delegate void FocusGained(object source);

    /// <summary>
    /// Invoked when the component loses focus
    /// </summary>
    /// <param name="source"></param>
    public delegate void FocusLost(object source);

    /// <summary>
    /// Invoked when a component is added to the container
    /// </summary>
    /// <param name="source"></param>
    /// <param name="component"></param>
    public delegate void ComponentAdded(object source, Component component);

    /// <summary>
    /// Invoked when a component is removed from the container
    /// </summary>
    /// <param name="source"></param>
    /// <param name="component"></param>
    public delegate void ComponentRemoved(object source, Component component);

    /// <summary>
    /// Invoked when a certain action is performed
    /// </summary>
    /// <param name="source"></param>
    public delegate void ActionPerformed(object source);

    /// <summary>
    /// Invoked when the target of the listener has changed its state
    /// </summary>
    /// <param name="source"></param>
    public delegate void StateChanged(object source);

    /// <summary>
    /// Invoked when an item has been selected or deselected by the user
    /// </summary>
    /// <param name="source"></param>
    /// <param name="item"></param>
    /// <param name="selected"></param>
    public delegate void ItemStateChanged(object source, object item, bool selected);

    /// <summary>
    /// Invoked when caret position is updated
    /// </summary>
    /// <param name="source"></param>
    /// <param name="caretPosition"></param>
    public delegate void CaretUpdate(object source, Point caretPosition);

    /// <summary>
    /// Invoked when key is pressed
    /// </summary>
    public delegate void KeyPressed(Keys key, KeyModifier modifier);

    /// <summary>
    /// Invoked when key is released
    /// </summary>
    public delegate void KeyReleased(Keys key, KeyModifier modifier);

    /// <summary>
    /// Invoked when key is typed
    /// </summary>
    public delegate void KeyTyped(Keys key, KeyModifier modifier);

    /// <summary>
    /// Invoked when the contents of the list has changed.
    /// </summary>
    public delegate void ContentsChanged(object source, int index0, int index1);

    /// <summary>
    /// Invoked when one or more elements are inserted in the indices from index0 to index1 in the data model.
    /// </summary>
    public delegate void IntervalAdded(object source, int index0, int index1);

    /// <summary>
    /// Invoked when one or more elements are removed from the indices between index0 and index1 in the data model.
    /// </summary>
    public delegate void IntervalRemoved(object source, int index0, int index1);

    /// <summary>
    /// Invoked when the value of the selection changes.
    /// </summary>
    public delegate void ValueChanged(object src, SelectionType type, int firstIndex, int lastIndex, bool isAdjusting);

    /// <summary>
    /// Invoked when mouse is clicked within the bounds.
    /// </summary>
    public delegate void MouseClicked(int button, Point mouseLocation);

    /// <summary>
    /// Invoked when mouse enters in the bounds.
    /// </summary>
    public delegate void MouseEntered();

    /// <summary>
    /// Invoked when mouse exits from the bounds.
    /// </summary>
    public delegate void MouseExited();

    /// <summary>
    /// Invoked when mouse is pressed within the bounds.
    /// </summary>
    public delegate void MousePressed(int button, Point mouseLocation);

    /// <summary>
    /// Invoked when mouse is released within the bounds.
    /// </summary>
    public delegate void MouseReleased(int button, Point mouseLocation);

    /// <summary>
    /// Invoked when mouse is moved within the bounds.
    /// </summary>
    /// <param name="button">The button held down</param>
    /// <param name="startLocation">The start mouse location of the dragging</param>
    /// <param name="currentLocation">The current mouse location of the dragging</param>
    public delegate void MouseDragged(int button, Point startLocation, Point currentLocation);

    /// <summary>
    /// Invoked when mouse is dragged within the bounds.
    /// </summary>
    /// <param name="mouseLocation">The location of the mouse in screen coordinate</param>
    public delegate void MouseMoved(Point mouseLocation);

    /// <summary>
    /// Invoked when mouse wheel is moved if mouse wheel is available
    /// </summary>
    /// <param name="delta">The amount moved from previous mouse wheel</param>
    /// <param name="value">The current mouse wheel value</param>
    public delegate void MouseWheelMoved(int delta, int value);

    #endregion

    /// <summary>
    /// An abstract UI component. This class cannot be instantiated.
    /// </summary>
    abstract public class Component
    {
        #region Member Fields

        /// <summary>
        /// Default color of the background
        /// </summary>
        public static Color DEFAULT_COLOR = Color.LightGray;

        /// <summary>
        /// Default transparency value for any colors associated with this component
        /// </summary>
        public static float DEFAULT_ALPHA = 1.0f;

        /// <summary>
        /// Parent component of this component for scene-graph-based drawing
        /// </summary>
        protected Component parent;

        /// <summary>
        /// Background color of this component if enabled
        /// </summary>
        protected Color backgroundColor;

        /// <summary>
        /// Background color of this component if not enabled
        /// </summary>
        protected Color disabledColor;

        /// <summary>
        /// Border color of this component's background
        /// </summary>
        protected Color borderColor;

        /// <summary>
        /// Indicator of whether to paint the border
        /// </summary>
        protected bool drawBorder;

        /// <summary>
        /// Indicator of whether to paint the background
        /// </summary>
        protected bool drawBackground;

        /// <summary>
        /// Transparency value in the range [0 -- 255]
        /// </summary>
        protected byte alpha;

        /// <summary>
        /// Indicator of whether this component is visible
        /// </summary>
        protected bool visible;

        /// <summary>
        /// Indicator of whether this component is enabled
        /// </summary>
        protected bool enabled;

        /// <summary>
        /// Indicator of whether this component is focused . 
        /// NOTE: This variable is useful only for indicating that the component
        /// should receive key input
        /// </summary>
        protected bool focused;

        /// <summary>
        /// Name of this component. 
        /// NOTE: Mostly used only for debugging (See ToString() method)
        /// </summary>
        protected String name;

        /// <summary>
        /// Label/Text associated with this component
        /// </summary>
        protected String label;

        /// <summary>
        /// Color of the texture (it's always Color.White, but it contains alpha info as well)
        /// </summary>
        protected Color textureColor;

        /// <summary>
        /// Indicator of how label/text should be aligned horizontally
        /// </summary>
        protected GoblinEnums.HorizontalAlignment horizontalAlignment;

        /// <summary>
        /// Indicator of how label/text should be aligned vertically
        /// </summary>
        protected GoblinEnums.VerticalAlignment verticalAlignment;

        /// <summary>
        /// Label/Text color
        /// </summary>
        protected Color textColor;

        /// <summary>
        /// Transparency value of the label/text
        /// </summary>
        protected byte textAlpha;

        /// <summary>
        /// Indicator of whether a key is held down
        /// </summary>
        protected bool keyDown;

        /// <summary>
        /// Indicator of whether any mouse button is held down. 
        /// NOTE: Mainly used for detecting mouse dragging event
        /// </summary>
        protected bool mouseDown;

        /// <summary>
        /// Indicator of whether the mouse pointer is hovering on this component
        /// </summary>
        protected bool within;

        /// <summary>
        /// Indicator of whether the mouse has entered the bound of this component. 
        /// NOTE: Mainly used for detecting mouse enter and exit event
        /// </summary>
        protected bool entered;

        /// <summary>
        /// An XNA class used for loading a texture for the background image
        /// </summary>
        protected Texture2D backTexture;
        #endregion

        #region Events

        /// <summary>
        /// An event triggered whenever a focus is gained on this component.
        /// </summary>
        public event FocusGained FocusGainedEvent;

        /// <summary>
        /// An event triggered whenever a focus is lost on this component.
        /// </summary>
        public event FocusLost FocusLostEvent;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a UI component with the specified background color and transparency value.
        /// </summary>
        /// <param name="bgColor">Background color of this component</param>
        /// <param name="alpha">Transparency value of this component [0.0f - 1.0f]. 1.0f meaning
        /// totally opaque, and 0.0f meaning totally transparent</param>
        public Component(Color bgColor, float alpha)
        {
            this.alpha = (byte)(alpha * 255);
            this.backgroundColor= new Color(bgColor.R, bgColor.G, bgColor.B, this.alpha);
         
            name = "Component";
            visible = true;
            enabled = true;
            parent = null;
            drawBorder = true;
            drawBackground = true;
            focused = false;
            borderColor = Color.Black;
            disabledColor = Color.Gray;
            textureColor = Color.White;

            label = "";
            horizontalAlignment = GoblinEnums.HorizontalAlignment.Left;
            verticalAlignment = GoblinEnums.VerticalAlignment.Top;

            textColor = Color.Black;
            textAlpha = (byte)255;
            keyDown = false;

            mouseDown = false;
            within = false;
            entered = false;
        }

        /// <summary>
        /// Creates a UI component with the specified background color and transparency of 1.0f.
        /// </summary>
        /// <param name="bgColor">Background color of this component</param>
        public Component(Color bgColor) :
            this(bgColor, DEFAULT_ALPHA) { }

        /// <summary>
        /// Creates a UI component with a light gray background color and transparency of 1.0f.
        /// </summary>
        public Component() :
            this(DEFAULT_COLOR, DEFAULT_ALPHA){ }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the background color of this component.
        /// </summary>
        public virtual Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                this.backgroundColor = new Color(value.R, value.G, value.B, alpha);
            }
        }

        /// <summary>
        /// Gets or sets the border color of the background of this component.
        /// </summary>
        public virtual Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = new Color(value.R, value.G, value.B, alpha);
            }
        }

        /// <summary>
        /// Gets or sets the background color of this component when disabled.
        /// </summary>
        public virtual Color DisabledColor
        {
            get { return disabledColor; }
            set
            {
                disabledColor = new Color(value.R, value.G, value.B, alpha);
            }
        }

        /// <summary>
        /// Gets or sets the transparency value of this component.
        /// </summary>
        /// <remarks>
        /// A transparency value in the range [0.0f -- 1.0f]. 1.0f meaning
        /// totally opaque, and 0.0f meaning totally transparent
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Throws ArgumentException if alpha value is outside of the range [0.0f -- 1.0f]
        /// </exception>
        public virtual float Transparency
        {
            get { return alpha / 255f; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Invalid alpha value: " + value +
                        " Possible values: 0.0 - 1.0");

                alpha = (byte)(value * 255);

                backgroundColor.A = borderColor.A = disabledColor.A = textureColor.A = alpha;
            }
        }

        /// <summary>
        /// Gets or sets whether this component is visible.
        /// </summary>
        public virtual bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        /// <summary>
        /// Gets or sets whether this component is enabled.
        /// </summary>
        public virtual bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets whether this component is focused. 
        /// </summary>
        /// <remarks>
        /// Focused variable is mainly used for determining that the component
        /// should receive key input, since key input should be received by only one
        /// component at a time
        /// </remarks>
        public virtual bool Focused
        {
            get { return focused; }
            internal set
            {
                if (focused != value)
                {
                    focused = value;

                    if (focused)
                        InvokeFocusGainedEvent(this);
                    else
                        InvokeFocusLostEvent(this);
                }
            }
        }

        /// <summary>
        /// Get or sets the background image with the given image texture.
        /// </summary>
        /// <remarks>
        /// This will automatically disable the border drawing, and enable the background drawing.
        /// </remarks>
        public virtual Texture2D Texture
        {
            get { return backTexture; }
            set 
            { 
                backTexture = value;
                drawBorder = false;
                drawBackground = true;
            }
        }

        /// <summary>
        /// Gets or sets the color of the background texture if set. The default color is Color.White.
        /// </summary>
        public virtual Color TextureColor
        {
            get { return textureColor; }
            set { textureColor = value; }
        }

        /// <summary>
        /// Gets or sets whether the border should be painted.
        /// </summary>
        public virtual bool DrawBorder
        {
            get { return drawBorder; }
            set { drawBorder = value; }
        }

        /// <summary>
        /// Gets or sets whether the background should be painted.
        /// </summary>
        public virtual bool DrawBackground
        {
            get { return drawBackground; }
            set { drawBackground = value; }
        }

        /// <summary>
        /// Gets or sets the parent of this component.
        /// </summary>
        public virtual Component Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Gets the root ancestor of this component.
        /// </summary>
        internal virtual Component RootParent
        {
            get
            {
                if (HasParent)
                    return parent.RootParent;
                else
                    return this;
            }
        }

        /// <summary>
        /// Gets whether this component has a parent.
        /// </summary>
        public virtual bool HasParent
        {
            get { return (parent != null); }
        }

        /// <summary>
        /// Gets or sets the name of this component. 
        /// </summary>
        /// <remarks>
        /// Name information is mainly used for debugging purpose only.
        /// </remarks>
        public virtual String Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the label/text associated with this component.
        /// </summary>
        public virtual String Text
        {
            get { return label; }
            set { label = value; }
        }

        /// <summary>
        /// Gets or sets the color of the label/text.
        /// </summary>
        public virtual Color TextColor
        {
            get { return textColor; }
            set
            {
                textColor = new Color(value.R, value.G, value.B, textAlpha);
            }
        }

        /// <summary>
        /// Gets or sets the transparency of the label/text.
        /// </summary>
        /// <remarks>
        /// A transparency value in the range [0.0f -- 1.0f]
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Throws ArgumentException if alpha value is outside of the range [0.0f -- 1.0f].
        /// </exception>
        public virtual float TextTransparency
        {
            get { return textAlpha / 255f; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("Invalid alpha value: " + value +
                        " Possible values: 0.0 - 1.0");

                this.textAlpha = (byte)(value * 255);
                textColor.A = textAlpha;
            }
        }

        /// <summary>
        /// Gets or sets how label/text should be aligned horizontally.
        /// </summary>
        public virtual GoblinEnums.HorizontalAlignment HorizontalAlignment
        {
            get { return horizontalAlignment; }
            set { horizontalAlignment = value; }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of the label/text.
        /// </summary>
        public virtual GoblinEnums.VerticalAlignment VerticalAlignment
        {
            get { return verticalAlignment; }
            set { verticalAlignment = value; }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Invokes focus gained event.
        /// </summary>
        /// <param name="source">The class that invoked this event.</param>
        protected void InvokeFocusGainedEvent(object source)
        {
            if (FocusGainedEvent != null)
                FocusGainedEvent(source);
        }

        /// <summary>
        /// Invokes focus lost event.
        /// </summary>
        /// <param name="source">The class that invoked this event.</param>
        protected void InvokeFocusLostEvent(object source)
        {
            if (FocusLostEvent != null)
                FocusLostEvent(source);
        }

        #endregion
    }
}
