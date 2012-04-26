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
using Microsoft.Xna.Framework.Input;
#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif

using GoblinXNA.Helpers;

namespace GoblinXNA.Device.Generic
{
    #region Mouse Delegates
    /// <summary>
    /// A delegate/callback function that defines what to do when the mouse is pressed.
    /// </summary>
    /// <param name="button">LeftButton, MiddleButton, or RightButton</param>
    /// <param name="mouseLocation">The location in screen coordinates where the mouse is pressed</param>
    public delegate void HandleMousePress(int button, Point mouseLocation);

    /// <summary>
    /// A delegate/callback function that defines what to do when the mouse is released.
    /// </summary>
    /// <param name="button">LeftButton, MiddleButton, or RightButton</param>
    /// <param name="mouseLocation">The location in screen coordinates where the mouse is released</param>
    public delegate void HandleMouseRelease(int button, Point mouseLocation);

    /// <summary>
    /// A delegate/callback function that defines what to do when the mouse is clicked.
    /// </summary>
    /// <param name="button">LeftButton, MiddleButton, or RightButton</param>
    /// <param name="mouseLocation">The location in screen coordinates where the mouse is clicked</param>
    public delegate void HandleMouseClick(int button, Point mouseLocation);

    /// <summary>
    /// A delegate/callback function that defines what to do when the mouse is moved.
    /// </summary>
    /// <param name="mouseLocation">The current location of the mouse in screen coordinates</param>
    public delegate void HandleMouseMove(Point mouseLocation);

    /// <summary>
    /// A delegate/callback function that defines what to do when the mouse is dragged.
    /// </summary>
    /// <param name="button">LeftButton, MiddleButton, or RightButton</param>
    /// <param name="startLocation">The start location of the mouse drag in screen coordinates</param>
    /// <param name="currentLocation">The current location of the mouse drag in screen coordinates</param>
    public delegate void HandleMouseDrag(int button, Point startLocation, Point currentLocation);

    /// <summary>
    /// A delegate/callback function that defines what to do when the mouse wheel is moved.
    /// </summary>
    /// <param name="delta">The difference of current mouse scroll wheel value from previous
    /// mouse scroll wheel value</param>
    /// <param name="value">The cumulative mouse scroll wheel value since the game/application
    /// was started</param>
    public delegate void HandleMouseWheelMove(int delta, int value);
    #endregion

    #region Mouse Enums
    /// <summary>
    /// An enum that defines the mouse event type.
    /// </summary>
    public enum MouseEventType : byte{
        Press = (byte)0,
        Release,
        Click,
        Move,
        Drag,
        WheelMove
    }
    #endregion

    /// <summary>
    /// A helper class for handling the mouse input. This class wraps the functionalities provided
    /// by XNA's MouseState class, and mouse events are handled based on interrupt method (callback
    /// functions), rather than polling method (XNA's MouseState), so that the developer doesn't need to 
    /// poll the status of the mouse state every frame by herself/himself.
    /// </summary>
    /// <example>
    /// An example of adding a mouse press event handler:
    /// 
    /// MouseInput mouseInput = MouseInput.Instance;
    /// mouseInput.MousePressEvent += new HandleMousePress(MousePressHandler);
    ///
    /// private void MousePressHandler(int button, Point mouseLocation)
    /// {
    ///    //Insert your mouse press handling code here
    ///    if(button == MouseInput.LeftButton)
    ///    {
    ///        ....
    ///    }
    /// }
    /// </example>
    /// <remarks>
    /// MouseInput is a singleton class, so you should access this class through Instance property.
    /// </remarks>
    public class MouseInput : InputDevice
    {
        #region Constants
        /// <summary>
        /// Indicates the left mouse button of a 3-state mouse device.
        /// </summary>
        public static int LeftButton = 0;

        /// <summary>
        /// Indicates the middle mouse button of a 3-state mouse device. A middle button is
        /// usually the mouse wheel.
        /// </summary>
        public static int MiddleButton = 1;

        /// <summary>
        /// Indicates the right mouse button  of a 3-state mouse device.
        /// </summary>
        public static int RightButton = 2;

        #endregion

        #region Member Fields

        private static bool isAvailable;

        /// <summary>
        /// Mouse state, set every frame in the Update method.
        /// </summary>
        private MouseState mouseState;
        private bool mousePressed;

        /// <summary>
        /// Mouse wheel delta this frame. We do not get the total scroll value, but we usually 
        /// need the current delta!
        /// </summary>
        private int mouseWheelDelta;
        private int mouseWheelValue;

        // Start dragging pos, will be set when we just pressed the left mouse button. Used for the MouseDraggingAmount property.		
        private Point startDraggingPos;
        private Point prevDraggingPos;
        private int dragButton;

        // X and y movements of the mouse this frame		
        private float lastMouseX;
        private float lastMouseY;

        private bool onlyTrackInsideWindow;
        private bool onlyTrackWhenFocused;

        private static MouseInput input;

        #endregion

        #region Events
        /// <summary>
        /// An event to add or remove mouse click delegate/callback functions
        /// </summary>
        public event HandleMouseClick MouseClickEvent;

        /// <summary>
        /// An event to add or remove mouse press delegate/callback functions
        /// </summary>
        public event HandleMousePress MousePressEvent;

        /// <summary>
        /// An event to add or remove mouse release delegate/callback functions
        /// </summary>
        public event HandleMouseRelease MouseReleaseEvent;

        /// <summary>
        /// An event to add or remove mouse move delegate/callback functions
        /// </summary>
        public event HandleMouseMove MouseMoveEvent;

        /// <summary>
        /// An event to add or remove mouse drag delegate/callback functions
        /// </summary>
        public event HandleMouseDrag MouseDragEvent;

        /// <summary>
        /// An event to add or remove mouse wheel move delegate/callback functions
        /// </summary>
        public event HandleMouseWheelMove MouseWheelMoveEvent;
        #endregion

        #region Constructors
        /// <summary>
        /// A private constructor.
        /// </summary>
        /// <remarks>
        /// Don't instatiate this constructor.
        /// </remarks>
        private MouseInput()
        {
            mousePressed = false;
            mouseWheelDelta = 0;
            mouseWheelValue = 0;

            lastMouseX = 0;
            lastMouseY = 0;

            onlyTrackInsideWindow = true;
            onlyTrackWhenFocused = true;

            isAvailable = true;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets whether the left mouse button is currently pressed.
        /// </summary>
        private bool MouseLeftButtonPressed
        {
            get { return mouseState.LeftButton == ButtonState.Pressed; }
        }

        /// <summary>
        /// Gets whether the right mouse button is currently pressed.
        /// </summary>
        private bool MouseRightButtonPressed
        {
            get { return mouseState.RightButton == ButtonState.Pressed; }
        }

        /// <summary>
        /// Gets whether the middle mouse button is currently pressed.
        /// </summary>
        private bool MouseMiddleButtonPressed
        {
            get { return mouseState.MiddleButton == ButtonState.Pressed; }
        }

        /// <summary>
        /// Gets whether the left mouse button is currently released.
        /// </summary>
        private bool MouseLeftButtonReleased
        {
            get { return mouseState.LeftButton == ButtonState.Released; }
        }

        /// <summary>
        /// Gets whether the right mouse button is currently released.
        /// </summary>
        private bool MouseRightButtonReleased
        {
            get { return mouseState.RightButton == ButtonState.Released; }
        }

        /// <summary>
        /// Gets whether the middle mouse button is currently released.
        /// </summary>
        private bool MouseMiddleButtonReleased
        {
            get { return mouseState.MiddleButton == ButtonState.Released; }
        }

        /// <summary>
        /// Gets the current mouse position in the screen coordinate.
        /// </summary>
        private Point MouseLocation
        {
            get { return new Point(mouseState.X, mouseState.Y); }
        }

        #endregion

        #region Public Properties

        public String Identifier
        {
            get { return "Mouse"; }
        }

        public bool IsAvailable
        {
            get { return isAvailable; }
        }

        /// <summary>
        /// Gets or sets whether to handle mouse events only if the mouse cursor is inside
        /// the window. The default value is true.
        /// </summary>
        public bool OnlyHandleInsideWindow
        {
            get { return onlyTrackInsideWindow; }
            set { onlyTrackInsideWindow = value; }
        }

        /// <summary>
        /// Gets or sets whether to handle mouse events only if the current window is
        /// focused. The default value is true.
        /// </summary>
        public bool OnlyTrackWhenFocused
        {
            get { return onlyTrackWhenFocused; }
            set { onlyTrackWhenFocused = value; }
        }

        /// <summary>
        /// Gets the instantiation of MouseInput class.
        /// </summary>
        public static MouseInput Instance
        {
            get
            {
                if (input == null)
                {
                    input = new MouseInput();
                }

                return input;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Triggers the mouse event callback functions programatically with the given byte data
        /// array. The data format differs depending on the MouseEventType. Use GetNetworkData(...)
        /// functions to convert each of the mouse events and the necessary information (e.g., button)
        /// to a byte array.
        /// </summary>
        /// <see cref="GetNetworkData(MouseEventType, int, Point)"/>
        /// <seealso cref="GetNetworkData(Point)"/>
        /// <seealso cref="GetNetworkData(int, Point, Point)"/>
        /// <seealso cref="GetNetworkData(int, int)"/>
        /// <param name="data">An array of bytes containing specific data used to trigger
        /// the mouse event callback functions</param>
        public void TriggerDelegates(byte[] data)
        {
            byte type = data[0];
            int button;
            Point mouseLocation;
            switch (type)
            {
                case (byte)MouseEventType.Press:
                    button = (int)data[1];
                    mouseLocation = new Point(ByteHelper.ConvertToShort(data, 2),
                        ByteHelper.ConvertToShort(data, 4));
                    if(MousePressEvent != null)
                        MousePressEvent(button, mouseLocation);
                    break;
                case (byte)MouseEventType.Release:
                    button = (int)data[1];
                    mouseLocation = new Point(ByteHelper.ConvertToShort(data, 2),
                        ByteHelper.ConvertToShort(data, 4));
                    if(MouseReleaseEvent != null)
                        MouseReleaseEvent(button, mouseLocation);
                    break;
                case (byte)MouseEventType.Click:
                    button = (int)data[1];
                    mouseLocation = new Point(ByteHelper.ConvertToShort(data, 2),
                        ByteHelper.ConvertToShort(data, 4));
                    if(MouseClickEvent != null)
                        MouseClickEvent(button, mouseLocation);
                    break;
                case (byte)MouseEventType.Move:
                    mouseLocation = new Point(ByteHelper.ConvertToShort(data, 1),
                        ByteHelper.ConvertToShort(data, 3));
                    if(MouseMoveEvent != null)
                        MouseMoveEvent(mouseLocation);
                    break;
                case (byte)MouseEventType.Drag:
                    button = (int)data[1];
                    mouseLocation = new Point(ByteHelper.ConvertToShort(data, 2),
                        ByteHelper.ConvertToShort(data, 4));
                    Point curLocation = new Point(ByteHelper.ConvertToShort(data, 6),
                        ByteHelper.ConvertToShort(data, 8));
                    if (MouseDragEvent != null)
                        MouseDragEvent(button, mouseLocation, curLocation);
                    break;
                case (byte)MouseEventType.WheelMove:
                    int delta = (int)ByteHelper.ConvertToShort(data, 1);
                    int value = (int)ByteHelper.ConvertToShort(data, 3);
                    if(MouseWheelMoveEvent != null)
                        MouseWheelMoveEvent(delta, value);
                    break;
            }
        }

        /// <summary>
        /// Converts the mouse event type, mouse button, and mouse location to an array of bytes
        /// so that it can be sent over the network.
        /// </summary>
        /// <param name="type">Must be Press, Release, or Click</param>
        /// <param name="button">RightButton, MiddleButton, or LeftButton</param>
        /// <param name="mouseLocation"></param>
        /// <exception cref="GoblinException"></exception>
        /// <returns></returns>
        public byte[] GetNetworkData(MouseEventType type, int button, Point mouseLocation)
        {
            byte btype = (byte)type;
            if (btype > 2)
                throw new GoblinException("For this function, the type has to " +
                    "be Press, Release or Click");

            // 1 byte for type, 1 byte for button, and 2 (short) * 2 bytes for mouseLocation
            byte[] data = new byte[6];

            data[0] = btype;
            data[1] = (byte)button;
            List<short> mouseLocs = new List<short>();
            mouseLocs.Add((short)mouseLocation.X);
            mouseLocs.Add((short)mouseLocation.Y);
            byte[] mousePos = ByteHelper.ConvertShortArray(mouseLocs);
            ByteHelper.FillByteArray(ref data, 2, mousePos);

            return data;
        }

        /// <summary>
        /// Converts mouse location to an array of bytes for the 'Move' mouse event type
        /// so that it can be sent over the network.
        /// </summary>
        /// <param name="mouseLocation"></param>
        /// <returns></returns>
        public byte[] GetNetworkData(Point mouseLocation)
        {
            // 1 byte for type and 2 (short) * 2 bytes for mouseLocation
            byte[] data = new byte[5];

            data[0] = (byte)MouseEventType.Move;
            List<short> mouseLocs = new List<short>();
            mouseLocs.Add((short)mouseLocation.X);
            mouseLocs.Add((short)mouseLocation.Y);
            byte[] mousePos = ByteHelper.ConvertShortArray(mouseLocs);
            ByteHelper.FillByteArray(ref data, 1, mousePos);

            return data;
        }

        /// <summary>
        /// Converts the mouse button, mouse start location, and current mouse location to an 
        /// array of bytes for the 'Drag' mouse event type so that it can be sent over the network.
        /// </summary>
        /// <param name="button">RightButton, MiddleButton, or LeftButton</param>
        /// <param name="startLocation"></param>
        /// <param name="currentLocation"></param>
        /// <returns></returns>
        public byte[] GetNetworkData(int button, Point startLocation,
            Point currentLocation)
        {
            // 1 byte for type, 1 byte for button, 2 (short) * 2 bytes for startLocation,
            // and 2 (float) * 2 bytes for mouseLocation
            byte[] data = new byte[10];

            data[0] = (byte)MouseEventType.Drag;
            data[1] = (byte)button;
            List<short> mouseLocs = new List<short>();
            mouseLocs.Add((short)startLocation.X);
            mouseLocs.Add((short)startLocation.Y);
            mouseLocs.Add((short)currentLocation.X);
            mouseLocs.Add((short)currentLocation.Y);
            byte[] mousePos = ByteHelper.ConvertShortArray(mouseLocs);
            ByteHelper.FillByteArray(ref data, 2, mousePos);

            return data;
        }

        /// <summary>
        /// Converts the delta and value for the 'WheelMove' mouse event type to an array of 
        /// bytes so that it can be sent over the network.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetNetworkData(int delta, int value)
        {
            // 1 byte for type, 2 (short) bytes for delta, and 2 (short) bytes for value
            byte[] data = new byte[5];

            data[0] = (byte)MouseEventType.WheelMove;
            List<short> val = new List<short>();
            val.Add((short)delta);
            val.Add((short)value);
            byte[] vals = ByteHelper.ConvertShortArray(val);
            ByteHelper.FillByteArray(ref data, 1, vals);

            return data;
        }

        public void Update(TimeSpan elapsedTime, bool deviceActive)
        {
#if WINDOWS_PHONE

            TouchCollection touches = TouchPanel.GetState();

            if (touches.Count > 0)
            {
                Point mouseLocation = new Point((int)touches[0].Position.X, (int)touches[0].Position.Y);
                // We only care about the primary touch for MouseInput implementation
                switch (touches[0].State)
                {
                    case TouchLocationState.Pressed:
                        if(MousePressEvent != null)
                            MousePressEvent(LeftButton, mouseLocation);

                        mousePressed = true;

                        // Handle drag start
                        startDraggingPos = mouseLocation;
                        prevDraggingPos = mouseLocation;
                        dragButton = LeftButton;
                        break;
                    case TouchLocationState.Released:
                        // RELEASE
                        if (MouseReleaseEvent != null)
                            MouseReleaseEvent(dragButton, mouseLocation);

                        // CLICK
                        if (MouseClickEvent != null)
                            MouseClickEvent(dragButton, mouseLocation);

                        mousePressed = false;
                        break;
                    case TouchLocationState.Moved:
                        if (!prevDraggingPos.Equals(mouseLocation))
                        {
                            if (MouseDragEvent != null)
                                MouseDragEvent(dragButton, startDraggingPos, mouseLocation);

                            if(MouseMoveEvent != null)
                                MouseMoveEvent(mouseLocation);

                            prevDraggingPos = mouseLocation;
                        }
                        break;
                }
            }

#elif WINDOWS
            if (onlyTrackWhenFocused && !deviceActive)
                return;

            // Get the current state of the mouse
            mouseState = Mouse.GetState();

            // If only want to track inside of window, then ignore any mouse events 
            // triggered outside of the window
            if (onlyTrackInsideWindow && (mouseState.X < 0 || mouseState.X >= State.Width
                || mouseState.Y < 0 || mouseState.Y >= State.Height))
            {
                return;
            }

            // HANDLE MOUSE PRESS
            if (!mousePressed)
            {
                if (MouseLeftButtonPressed || MouseRightButtonPressed || MouseMiddleButtonPressed)
                {
                    int button = LeftButton;
                    if (MouseRightButtonPressed)
                        button = RightButton;
                    else if (MouseMiddleButtonPressed)
                        button = MiddleButton;

                    if(MousePressEvent != null)
                        MousePressEvent(button, MouseLocation);

                    mousePressed = true;

                    // Handle drag start
                    startDraggingPos = MouseLocation;
                    dragButton = button;
                }
            }
            else
            {
                // HANDLE MOUSE RELEASE & CLICK
                if ((dragButton == RightButton && MouseRightButtonReleased) ||
                    (dragButton == MiddleButton && MouseMiddleButtonPressed) ||
                    (dragButton == LeftButton && MouseLeftButtonReleased))
                {
                    // RELEASE
                    if(MouseReleaseEvent != null)
                        MouseReleaseEvent(dragButton, MouseLocation);

                    // CLICK
                    if(MouseClickEvent != null)
                        MouseClickEvent(dragButton, MouseLocation);

                    mousePressed = false;
                }

                // HANDLE MOUSE DRAG
                if ((MouseLeftButtonPressed || MouseRightButtonPressed || MouseMiddleButtonPressed) 
                    && !startDraggingPos.Equals(MouseLocation) && !prevDraggingPos.Equals(MouseLocation))
                {
                    int button = LeftButton;
                    if (MouseRightButtonPressed)
                        button = RightButton;
                    else if (MouseMiddleButtonPressed)
                        button = MiddleButton;

                    if (dragButton == button)
                    {
                        if(MouseDragEvent != null)
                            MouseDragEvent(button, startDraggingPos, MouseLocation);

                        prevDraggingPos = MouseLocation;
                    }
                }
            }

            // HANDLE MOUSE MOVE
            if ((lastMouseX != mouseState.X) || (lastMouseY != mouseState.Y))
            {
                if(MouseMoveEvent != null)
                    MouseMoveEvent(MouseLocation);

                lastMouseX = mouseState.X;
                lastMouseY = mouseState.Y;
            }

            // HANDLE MOUSE WHEEL MOVE
            mouseWheelDelta = mouseState.ScrollWheelValue - mouseWheelValue;
            if (mouseWheelDelta != 0)
            {
                mouseWheelValue = mouseState.ScrollWheelValue;
                if(MouseWheelMoveEvent != null)
                    MouseWheelMoveEvent(mouseWheelDelta, mouseWheelValue);
            }

            if (MouseRightButtonReleased && MouseLeftButtonReleased && MouseMiddleButtonReleased)
                mousePressed = false;
#endif
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        #endregion
    }
}
