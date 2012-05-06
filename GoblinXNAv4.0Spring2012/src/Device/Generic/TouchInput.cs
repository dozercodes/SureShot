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
 * Author: Nicolas Dedual (dedual@cs.columbia.edu)
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GoblinXNA.Device.Generic
{
    #region Touch Gesture Delegates

    public delegate void HandleTouchesPresent(TouchCollection touchPanel);

    public delegate void HandleTap(Vector2 position, Vector2 delta);
    public delegate void HandleHold(Vector2 position, Vector2 delta);
    public delegate void HandleDoubleTap(Vector2 position, Vector2 delta);
    public delegate void HandleFlick(Vector2 position, Vector2 delta);
    public delegate void HandlePinch(Vector2 position1, Vector2 position2, Vector2 delta1, Vector2 delta2);
    public delegate void HandlePinchComplete(Vector2 position1, Vector2 position2, Vector2 delta1, Vector2 delta2);
    public delegate void HandleFreeDrag(Vector2 position, Vector2 delta);
    public delegate void HandleHorizontalDrag(Vector2 position, Vector2 delta);
    public delegate void HandleVerticalDrag(Vector2 position, Vector2 delta);
    public delegate void HandleDragComplete(Vector2 position, Vector2 delta);

    #endregion

    public enum TouchEventType : byte{
        Tap = (byte)0,
        DoubleTap,
        Pinch,
        Hold,
        Flick,
        PinchComplete,
        FreeDrag,
        HorizontalDrag,
        VerticalDrag,
        DragComplete
    }
    
    public class TouchInput : InputDevice
    {
        #region Member Fields
        TouchCollection touches;

        String id;

        bool isAvailable;
        bool areGesturesEnabled;

        #endregion

        #region Events

        public event HandleTouchesPresent TouchesPresentEvent;
        public event HandleTap TapEvent;
        public event HandleHold HoldEvent;
        public event HandleFlick FlickEvent;
        public event HandleDoubleTap DoubleTapEvent;
        public event HandlePinch PinchEvent;
        public event HandlePinchComplete PinchCompleteEvent;
        public event HandleFreeDrag FreeDragEvent;
        public event HandleHorizontalDrag HorizontalDragEvent;
        public event HandleVerticalDrag VerticalDragEvent;
        public event HandleDragComplete DragCompleteEvent;

        #endregion

        #region Constructors

        public TouchInput()
        {
            touches = TouchPanel.GetState();
            isAvailable = true;
            id = "TouchScreen";
            
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets a unique identifier of this input device
        /// </summary>
        public String Identifier 
        {
            get { return id; }
            set {id = value;}
        }

        public Boolean IsAvailable
        {
            get{return isAvailable;}
        }

        public Boolean GesturesEnabled
        {
            get{return TouchPanel.IsGestureAvailable;}
        }

        #endregion

        #region Public Methods

        public void Initialize(List<TouchEventType> listOfEventsToEnable)
        {
            if(listOfEventsToEnable.Count > 0)
            {
                foreach(TouchEventType touchEvent in listOfEventsToEnable)
                {
                    switch(touchEvent)
                    {
                        case TouchEventType.Tap:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.Tap; 
                            break;
                        case TouchEventType.DoubleTap:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.DoubleTap; 
                            break;
                        case TouchEventType.Hold:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.Hold; 
                            break;
                        case TouchEventType.Flick:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.Flick; 
                            break;
                        case TouchEventType.Pinch:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.Pinch; 
                            break;
                        case TouchEventType.PinchComplete:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.PinchComplete; 
                            break;
                        case TouchEventType.FreeDrag:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.FreeDrag; 
                            break;
                        case TouchEventType.HorizontalDrag:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.HorizontalDrag; 
                            break;
                        case TouchEventType.VerticalDrag:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.VerticalDrag; 
                            break;
                        case TouchEventType.DragComplete:
                            TouchPanel.EnabledGestures =TouchPanel.EnabledGestures | GestureType.DragComplete; 
                            break; 
                        default:
                            TouchPanel.EnabledGestures = TouchPanel.EnabledGestures | GestureType.None;
                            break;
                    }
                }
            }

        }

        //These two functions need to be implemented once networking is in place
        public void TriggerDelegates(byte[] data)
        {
        }

        public byte[] GetNetworkData(TouchCollection touches, GestureSample gest)
        {
            return null;
        }

        public void Update(TimeSpan elapsedTime)
        {
            Update(elapsedTime, true);
        }
        
        /// <summary>
        /// Updates the state of this input device
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="deviceActive"></param>
        public void Update(TimeSpan elapsedTime, bool deviceEnabled)
        {
            touches = TouchPanel.GetState();

            if(TouchesPresentEvent !=null)
            {
                TouchesPresentEvent(touches);
            }

            while(TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                switch(gesture.GestureType)
                {
                    case GestureType.Tap:
                        if(TapEvent != null)
                        {
                            TapEvent(gesture.Position, gesture.Delta);
                        }
                    break;

                    case GestureType.DoubleTap:
                        if(DoubleTapEvent !=null)
                        {
                            DoubleTapEvent(gesture.Position, gesture.Delta);
                        }
                    break;

                    case GestureType.Hold:
                        if(HoldEvent !=null)
                        {
                            HoldEvent(gesture.Position, gesture.Delta);
                        }
                    break;
                    case GestureType.Flick:
                        if(FlickEvent !=null)
                        {
                            FlickEvent(gesture.Position, gesture.Delta);
                        }
                    break;
                    case GestureType.Pinch:
                        if(PinchEvent !=null)
                        {
                            PinchEvent(gesture.Position, gesture.Position2, gesture.Delta, gesture.Delta2);
                        }
                    break;
                    case GestureType.PinchComplete:
                        if(PinchCompleteEvent !=null)
                        {
                            PinchCompleteEvent(gesture.Position, gesture.Position2, gesture.Delta, gesture.Delta2);
                        }
                    break;
                    case GestureType.FreeDrag:
                        if(FreeDragEvent !=null)
                        {
                            FreeDragEvent(gesture.Position, gesture.Delta);
                        }
                    break;

                    case GestureType.HorizontalDrag:
                        if(HorizontalDragEvent !=null)
                        {
                            HorizontalDragEvent(gesture.Position, gesture.Delta);
                        }
                    break;
                    case GestureType.VerticalDrag:
                        if(VerticalDragEvent !=null)
                        {
                            VerticalDragEvent(gesture.Position, gesture.Delta);
                        }
                    break;
                    case GestureType.DragComplete:
                        if(DragCompleteEvent !=null)
                        {
                            DragCompleteEvent(gesture.Position, gesture.Delta);
                        }
                    break;

                    default:

                    break;
                }
            }

        }

        /// <summary>
        /// Disposes this input device.
        /// </summary>
        public void Dispose()
        {
            
        }
        
        #endregion

    }
}
