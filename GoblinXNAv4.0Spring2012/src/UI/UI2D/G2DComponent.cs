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

using GoblinXNA.Device.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GoblinXNA.UI.UI2D
{
    /// <summary>
    /// The top-level class for any Goblin XNA 2D GUI.
    /// </summary>
    /// <remarks>
    /// Any GoblinXNA 2D GUI class should extend this class.
    /// </remarks>
    public class G2DComponent : Component
    {
        #region Member Fields
        /// <summary>
        /// Original background bounds of this component
        /// </summary>
        protected Rectangle bounds;
        /// <summary>
        /// Drawing background bounds of this component. 
        /// NOTE: This is different from 'bounds' in case this component has a parent
        /// </summary>
        protected Rectangle paintBounds;

        protected float textWidth;

        protected float textHeight;

        protected SpriteFont textFont;

        #region Event Listeners Fields

        /// <summary>
        /// Indicator of whether a key input control is already associated with 
        /// this component
        /// </summary>
        protected bool keyInputRegistered;
        /// <summary>
        /// Indicator of whether a mouse input control is already associated with 
        /// this component
        /// </summary>
        protected bool mouseInputRegistered;

        private HandleKeyType keyType;
        private HandleKeyPress keyPress;
        private HandleKeyRelease keyRelease;

        private HandleMousePress mousePress;
        private HandleMouseRelease mouseRelease;
        private HandleMouseMove mouseMove;
        private HandleMouseDrag mouseDrag;
        private HandleMouseWheelMove mouseWheelMove;

        #endregion
        #endregion

        #region Events

        /// <summary>
        /// An event triggered whenever a key is pressed when this component is focused.
        /// </summary>
        public event KeyPressed KeyPressedEvent;

        /// <summary>
        /// An event triggered whenever a key is released when this component is focused.
        /// </summary>
        public event KeyReleased KeyReleasedEvent;

        /// <summary>
        /// An event triggered whenever a key is typed when this component is focused.
        /// </summary>
        public event KeyTyped KeyTypedEvent;

        /// <summary>
        /// An event triggered whenever a mouse is clicked within the bound of this component.
        /// </summary>
        public event MouseClicked MouseClickedEvent;

        /// <summary>
        /// An event triggered whenever a mouse is pressed within the bound of this component.
        /// </summary>
        public event MousePressed MousePressedEvent;

        /// <summary>
        /// An event triggered whenever a mouse is released within the bound of this component.
        /// </summary>
        public event MouseReleased MouseReleasedEvent;

        /// <summary>
        /// An event triggered whenever a mouse enters the bound of this component.
        /// </summary>
        public event MouseEntered MouseEnteredEvent;

        /// <summary>
        /// An event triggered whenever a mouse exits from the bound of this component.
        /// </summary>
        public event MouseExited MouseExitedEvent;

        /// <summary>
        /// An event triggered whenever a mouse is moved within the bound of this component.
        /// </summary>
        public event MouseMoved MouseMovedEvent;

        /// <summary>
        /// An event triggered whenever a mouse is dragged within the bound of this component.
        /// </summary>
        public event MouseDragged MouseDraggedEvent;

        /// <summary>
        /// An event triggered whenever the middle mouse button is rotated when the mouse location is
        /// within the bound of this component.
        /// </summary>
        public event MouseWheelMoved MouseWheelMovedEvent;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a 2D GUI component with the specified rectangular bounds, background color,
        /// and transparency value.
        /// </summary>
        /// <param name="bounds">Rectangular background bounds of this component</param>
        /// <param name="bgColor">Background color of this component</param>
        /// <param name="alpha">Transparency value of this component [0.0f - 1.0f]. 1.0f
        /// meaning totally opague, and 0.0f meaning totally transparent</param>
        public G2DComponent(Rectangle bounds, Color bgColor, float alpha) :
            base(bgColor, alpha)
        {
            this.bounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            paintBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);

            keyInputRegistered = false;
            mouseInputRegistered = false;

            mouseDown = false;
        }

        /// <summary>
        /// Creates a 2D GUI component with the specified rectangular bounds and background color, and
        /// transparency of 1.0f.
        /// </summary>
        /// <param name="bounds">Rectangular background bounds of this component</param>
        /// <param name="bgColor">Background color of this component</param>
        public G2DComponent(Rectangle bounds, Color bgColor) :
            this(bounds, bgColor, DEFAULT_ALPHA) { }

        /// <summary>
        /// Creates a 2D GUI component with the specified rectangular bounds, light gray background
        /// color, and transparency of 1.0f.
        /// </summary>
        /// <param name="bounds">Rectangular background bounds of this component</param>
        public G2DComponent(Rectangle bounds) :
            this(bounds, DEFAULT_COLOR, DEFAULT_ALPHA){ }

        /// <summary>
        /// Creates a 2D GUI component with 1x1 bounds at position (0, 0), light gray background
        /// color, and transparency of 1.0f.
        /// </summary>
        public G2DComponent() :
            this(new Rectangle(0, 0, 1, 1), DEFAULT_COLOR, DEFAULT_ALPHA) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the sprite font used to display any text associated with this component.
        /// </summary>
        public virtual SpriteFont TextFont
        {
            get { return textFont; }
            set
            {
                textFont = value;
                Text = label;
            }
        }

        /// <summary>
        /// Indicates whether a key input is already associated with this component
        /// </summary>
        internal bool KeyInputRegistered
        {
            get { return keyInputRegistered; }
        }

        /// <summary>
        /// Indicates whether a mouse input is already associated with this component
        /// </summary>
        internal bool MouseInputRegistered
        {
            get { return mouseInputRegistered; }
        }

        /// <summary>
        /// Gets or sets the background bounds of this component.
        /// </summary>
        public virtual Rectangle Bounds
        {
            get { return bounds; }
            set
            {
                if (value != null)
                {
                    bounds = new Rectangle(value.X, value.Y, value.Width, value.Height);

                    if (parent != null)
                    {
                        paintBounds = new Rectangle(bounds.X + ((G2DComponent)parent).paintBounds.X,
                            bounds.Y + ((G2DComponent)parent).paintBounds.Y, bounds.Width,
                            bounds.Height);
                    }
                    else
                    {
                        paintBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the actual painting bound. This is different from Bounds if this component has a parent.
        /// </summary>
        public Rectangle PaintBounds
        {
            get { return paintBounds; }
        }

        #endregion

        #region Override Properties
        /// <summary>
        /// Gets or sets the parent of this component
        /// </summary>
        /// <exception cref="GoblinException">
        /// Throws GoblinException if non-G2DComponent is assigned
        /// </exception>
        public override Component Parent
        {
            get { return parent; }
            set
            {
                G2DComponent g2dParent = null;

                try
                {
                    if (value != null)
                        g2dParent = (G2DComponent)value;
                }
                catch (Exception)
                {
                    throw new GoblinException("Can not assign non-G2DComponent as a parent of a G2DComponent");
                }

                base.Parent = value;

                if (g2dParent == null)
                    paintBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width,
                        bounds.Height);
                else
                    paintBounds = new Rectangle(bounds.X + g2dParent.paintBounds.X,
                        bounds.Y + g2dParent.paintBounds.Y, bounds.Width,
                        bounds.Height);
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                if (textFont != null)
                {
                    textWidth = textFont.MeasureString(value).X;
                    textHeight = textFont.MeasureString(value).Y;
                }
            }
        }
        #endregion

        #region Paint Methods
        /// <summary>
        /// Implements how the component should be painted. 
        /// </summary>
        /// <remarks>
        /// This base class method paints only the background
        /// </remarks>
        protected virtual void PaintComponent() {

            if (!drawBackground)
                return;

            // Draw normal background if no image is set
            if (backTexture == null)
            {
                Color color = (enabled) ? backgroundColor : disabledColor;
                // Draw the background
                UI2DRenderer.FillRectangle(paintBounds, State.BlankTexture, color);
            }
            else
            {
                // Draw the background
                UI2DRenderer.FillRectangle(paintBounds, backTexture, textureColor);
            }
        }

        /// <summary>
        /// Implements how the border of the component should be painted. 
        /// </summary>
        /// <remarks>
        /// This base class method paints only the outer-most border
        /// </remarks>
        protected virtual void PaintBorder() 
        {
            UI2DRenderer.DrawRectangle(paintBounds, borderColor, 1);
        }

        /// <summary>
        /// Implements how this component should be rendered
        /// </summary>
        public virtual void RenderWidget() 
        {
            if (!visible)
                return;

            PaintComponent();

            if(drawBorder)
                PaintBorder();
        }
        #endregion

        #region Register Key Input
        internal virtual void RegisterKeyInput()
        {
            if (keyInputRegistered)
                return;

            keyType = new HandleKeyType(HandleKeyType);
            keyPress = new HandleKeyPress(HandleKeyPress);
            keyRelease = new HandleKeyRelease(HandleKeyRelease);

            KeyboardInput.Instance.KeyTypeEvent += keyType;
            KeyboardInput.Instance.KeyPressEvent += keyPress;
            KeyboardInput.Instance.KeyReleaseEvent += keyRelease;

            keyInputRegistered = true;
        }

        internal virtual void RemoveKeyInput()
        {
            if (!keyInputRegistered)
                return;

            KeyboardInput.Instance.KeyTypeEvent -= keyType;
            KeyboardInput.Instance.KeyPressEvent -= keyPress;
            KeyboardInput.Instance.KeyReleaseEvent -= keyRelease;

            keyInputRegistered = false;
        }
        #endregion

        #region Key Input
        /// <summary>
        /// Implements how a key typed event should be handled. 
        /// </summary>
        /// <param name="key">The key typed</param>
        /// <param name="modifier">A struct that indicates whether any of the modifier keys 
        /// (e.g., Shift, Alt, or Ctrl) are pressed</param>
        protected virtual void HandleKeyType(Keys key, KeyModifier modifier)
        {
            if (!focused || !enabled || !visible)
                return;

            if(KeyTypedEvent != null)
                KeyTypedEvent(key, modifier);
        }

        /// <summary>
        /// Implements how a key press event should be handled. 
        /// </summary>
        /// <param name="key">The key pressed</param>
        /// <param name="modifier">A struct that indicates whether any of the modifier keys 
        /// (e.g., Shift, Alt, or Ctrl) are pressed</param>
        protected virtual void HandleKeyPress(Keys key, KeyModifier modifier)
        {
            if (!focused || !enabled || !visible)
                return;

            if(KeyPressedEvent != null)
                KeyPressedEvent(key, modifier);

            keyDown = true;
        }

        /// <summary>
        /// Implements how a key release event should be handled. 
        /// </summary>
        /// <param name="key">The key released</param>
        /// <param name="modifier">A struct that indicates whether any of the modifier keys 
        /// (e.g., Shift, Alt, or Ctrl) are pressed</param>
        protected virtual void HandleKeyRelease(Keys key, KeyModifier modifier)
        {
            if (!focused || !enabled || !visible)
                return;

            if(KeyReleasedEvent != null)
                KeyReleasedEvent(key, modifier);

            keyDown = false;
        }
        #endregion

        #region Mouse Input Registration
        /// <summary>
        /// Registers mouse input events from an existing Control. 
        /// NOTE: Does not allow registering mouse inputs from more than 2 sources
        /// </summary>
        internal virtual void RegisterMouseInput()
        {
            if (mouseInputRegistered)
                return;

            mousePress = new HandleMousePress(HandleMousePress);
            mouseRelease = new HandleMouseRelease(HandleMouseRelease);
            mouseMove = new HandleMouseMove(HandleMouseMove);
            mouseDrag = new HandleMouseDrag(HandleMouseDrag);
            mouseWheelMove = new HandleMouseWheelMove(HandleMouseWheel);

            // We handle mouse click internally instead of relying on MouseInput class.
            //MouseInput.Instance.MouseClickEvent += mouseClick;
            MouseInput.Instance.MousePressEvent += mousePress;
            MouseInput.Instance.MouseReleaseEvent += mouseRelease;
            MouseInput.Instance.MouseMoveEvent += mouseMove;
            MouseInput.Instance.MouseDragEvent += mouseDrag;
            MouseInput.Instance.MouseWheelMoveEvent += mouseWheelMove;

            mouseInputRegistered = true;
        }

        /// <summary>
        /// Removes the existing mouse input
        /// </summary>
        internal virtual void RemoveMouseInput()
        {
            if (!mouseInputRegistered)
                return;

            //MouseInput.Instance.MouseClickEvent -= mouseClick;
            MouseInput.Instance.MousePressEvent -= mousePress;
            MouseInput.Instance.MouseReleaseEvent -= mouseRelease;
            MouseInput.Instance.MouseMoveEvent -= mouseMove;
            MouseInput.Instance.MouseDragEvent -= mouseDrag;
            MouseInput.Instance.MouseWheelMoveEvent -= mouseWheelMove;

            mouseInputRegistered = false;
        }

        /// <summary>
        /// Implements how a mouse click event should be handled 
        /// </summary>
        /// <param name="button">MouseInput.LeftButton, MouseInput.MiddleButton, 
        /// or MouseInput.RightButton</param>
        /// <param name="mouseLocation">The location in screen coordinates where 
        /// the mouse is clicked</param>
        protected virtual void HandleMouseClick(int button, Point mouseLocation)
        {
            if (!within || !enabled || !visible)
                return;

            if(MouseClickedEvent != null)
                MouseClickedEvent(button, mouseLocation);
        }

        /// <summary>
        /// Implements how a mouse press event should be handled. 
        /// </summary>
        /// <param name="button">MouseInput.LeftButton, MouseInput.MiddleButton, 
        /// or MouseInput.RightButton</param>
        /// <param name="mouseLocation">The location in screen coordinates where 
        /// the mouse is pressed</param>
        protected virtual void HandleMousePress(int button, Point mouseLocation)
        {
#if WINDOWS_PHONE
            within = TestWithin(mouseLocation);
#endif

            if (!within || !enabled || !visible)
                return;

            if(MousePressedEvent != null)
                MousePressedEvent(button, mouseLocation);

            mouseDown = true;
        }

        /// <summary>
        /// Implements how a mouse release event should be handled. 
        /// </summary>
        /// <param name="button">MouseInput.LeftButton, MouseInput.MiddleButton, 
        /// or MouseInput.RightButton</param>
        /// <param name="mouseLocation">The location in screen coordinates where 
        /// the mouse is released</param>
        protected virtual void HandleMouseRelease(int button, Point mouseLocation)
        {
#if WINDOWS_PHONE
            within = TestWithin(mouseLocation);
#endif
            if (!within || !enabled || !visible)
                return;

            if(MouseReleasedEvent != null)
                MouseReleasedEvent(button, mouseLocation);

            if (mouseDown)
                HandleMouseClick(button, mouseLocation);

            mouseDown = false;
        }

        /// <summary>
        /// Implements how a mouse drag event should be handled.
        /// </summary>
        /// <param name="button">MouseInput.LeftButton, MouseInput.MiddleButton, 
        /// or MouseInput.RightButton</param>
        /// <param name="startLocation">The start location of the mouse drag in 
        /// screen coordinates</param>
        /// <param name="currentLocation">The current location of the mouse drag 
        /// in screen coordinates</param>
        protected virtual void HandleMouseDrag(int button, Point startLocation,
            Point currentLocation)
        {
            within = TestWithin(currentLocation);

            if (!within || !enabled || !visible)
                return;

            if(MouseDraggedEvent != null)
                MouseDraggedEvent(button, startLocation, currentLocation);
        }

        /// <summary>
        /// Implements how a mouse move event should be handled.
        /// </summary>
        /// <param name="mouseLocation">The current location of the mouse in 
        /// screen coordinates</param>
        protected virtual void HandleMouseMove(Point mouseLocation)
        {
            within = TestWithin(mouseLocation);

            if (!within || !enabled || !visible)
            {
                mouseDown = false;
                return;
            }

            if(MouseMovedEvent != null)
                MouseMovedEvent(mouseLocation);

            // FIXME: need fix for enter and exit event for boundary cases!!
            if (UI2DHelper.OnEdge(mouseLocation, paintBounds))
            {
                if (!entered) // Enter event
                {
                    if(MouseEnteredEvent != null)
                        MouseEnteredEvent();
                    entered = true;
                }
                else // Exit event
                {
                    if(MouseExitedEvent != null)
                        MouseExitedEvent();
                    entered = false;
                }
            }
        }

        /// <summary>
        /// Implements how a mouse drag event should be handled. 
        /// </summary>
        /// <param name="delta">The difference of current mouse scroll wheel value from previous
        /// mouse scroll wheel value</param>
        /// <param name="value">The cumulative mouse scroll wheel value since the game/application
        /// was started</param>
        protected virtual void HandleMouseWheel(int delta, int value)
        {
            if (!within || !enabled || !visible)
                return;

            if(MouseWheelMovedEvent != null)
                MouseWheelMovedEvent(delta, value);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Tests whether the mouse is within the bounds.
        /// </summary>
        /// <returns></returns>
        protected virtual bool TestWithin(Point location)
        {
            return UI2DHelper.IsWithin(location, paintBounds);
        }

        /// <summary>
        /// Invokes the key press event.
        /// </summary>
        /// <param name="key">The key pressed.</param>
        /// <param name="modifier"></param>
        protected void InvokeKeyPressedEvent(Keys key, KeyModifier modifier)
        {
            if (KeyPressedEvent != null)
                KeyPressedEvent(key, modifier);
        }

        /// <summary>
        /// Invokes the key release event.
        /// </summary>
        /// <param name="key">The key released.</param>
        /// <param name="modifier"></param>
        protected void InvokeKeyReleasedEvent(Keys key, KeyModifier modifier)
        {
            if (KeyReleasedEvent != null)
                KeyReleasedEvent(key, modifier);
        }

        /// <summary>
        /// Invokes the key type event.
        /// </summary>
        /// <param name="key">The key typed.</param>
        /// <param name="modifier"></param>
        protected void InvokeKeyTypedEvent(Keys key, KeyModifier modifier)
        {
            if (KeyTypedEvent != null)
                KeyTypedEvent(key, modifier);
        }

        /// <summary>
        /// Invokes the mouse click event.
        /// </summary>
        /// <param name="button">The button clicked.</param>
        /// <param name="mouseLocation">The location where mouse is clicked in screen coordinate.</param>
        protected void InvokeMouseClickedEvent(int button, Point mouseLocation)
        {
            if (MouseClickedEvent != null)
                MouseClickedEvent(button, mouseLocation);
        }

        /// <summary>
        /// Invokes the mouse press event.
        /// </summary>
        /// <param name="button">The button pressed.</param>
        /// <param name="mouseLocation">The location where mouse is pressed in screen coordinate.</param>
        protected void InvokeMousePressedEvent(int button, Point mouseLocation)
        {
            if (MousePressedEvent != null)
                MousePressedEvent(button, mouseLocation);
        }

        /// <summary>
        /// Invokes the mouse release event.
        /// </summary>
        /// <param name="button">The button released.</param>
        /// <param name="mouseLocation">The location where mouse is released in screen coordinate.</param>
        protected void InvokeMouseReleasedEvent(int button, Point mouseLocation)
        {
            if (MouseReleasedEvent != null)
                MouseReleasedEvent(button, mouseLocation);
        }

        /// <summary>
        /// Invokes the mouse enter event.
        /// </summary>
        protected void InvokeMouseEnteredEvent()
        {
            if (MouseEnteredEvent != null)
                MouseEnteredEvent();
        }

        /// <summary>
        /// Invokes the mouse exit event.
        /// </summary>
        protected void InvokeMouseExitedEvent()
        {
            if (MouseExitedEvent != null)
                MouseExitedEvent();
        }

        /// <summary>
        /// Invokes the mouse moved event.
        /// </summary>
        /// <param name="curPosition">The current position of the mouse in screen coordinate.</param>
        protected void InvokeMouseMoved(Point curPosition)
        {
            if (MouseMovedEvent != null)
                MouseMovedEvent(curPosition);
        }

        /// <summary>
        /// Invokes the mouse dragged event.
        /// </summary>
        /// <param name="button">The button dragged.</param>
        /// <param name="startPosition">The starting dragging position in screen coordinate.</param>
        /// <param name="curPosition">The current position of the mouse in screen coordinate.</param>
        protected void InvokeMouseDragged(int button, Point startPosition, Point curPosition)
        {
            if (MouseDraggedEvent != null)
                MouseDraggedEvent(button, startPosition, curPosition);
        }

        /// <summary>
        /// Invokes the mouse wheel moved event.
        /// </summary>
        /// <param name="delta">The diff value from the previous middle button wheel position.</param>
        /// <param name="value">The current value of the middle button wheel position.</param>
        protected void InvokeMouseWheelMovedEvent(int delta, int value)
        {
            if (MouseWheelMovedEvent != null)
                MouseWheelMovedEvent(delta, value);
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Gets the name of this component
        /// </summary>
        /// <returns>Name of this component</returns>
        public override string ToString()
        {
            return name;
        }

        #endregion
    }
}
