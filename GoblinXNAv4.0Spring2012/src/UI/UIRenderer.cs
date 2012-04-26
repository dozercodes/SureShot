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
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;
using GoblinXNA.UI.UI2D;
using GoblinXNA.UI.UI3D;

namespace GoblinXNA.UI
{
    /// <summary>
    /// The renderer for all 2D and 3D UI components, including FPS, triangle count, and
    /// debugging/notification messages.
    /// 
    /// An UI component needs to be added to this renderer through Scene.UIRender before it
    /// can be rendered in 3D scene.
    /// </summary>
    public class UIRenderer
    {
        #region Member Fields
        public static G2DComponent GlobalFocus2DComp = null;

        protected List<G2DComponent> comp2Ds;
        private List<G3DComponent> comp3Ds;
        protected SpriteFont debugFont;
        protected int triangleCount;
        protected int globalUIShift;

        #region FPS
        protected int counter;
        protected int prevCounter;
        protected double passedTime;
        #endregion

        #region Notification
        private List<Notifier.NotifierMessage> messages;
        #endregion

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a renderer class for taking care of the UI rendering.
        /// </summary>
        /// <remarks>
        /// Do not instantiate this class since this class is already instantiated in the 
        /// GoblinXNA.SceneGraph.Scene class.
        /// </remarks>
        public UIRenderer()
        {
            try
            {
                debugFont = State.Content.Load<SpriteFont>(@"" + Path.Combine(
                    State.GetSettingVariable("FontDirectory"), "DebugFont"));
            }
            catch (Exception exp)
            {
            }
            
            comp2Ds = new List<G2DComponent>();
            comp3Ds = new List<G3DComponent>();

            messages = new List<Notifier.NotifierMessage>();
            Notifier.Font = debugFont;

            counter = 1;
            prevCounter = 0;
            passedTime = 0;
            globalUIShift = 0;

            triangleCount = 0;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the triangle count of currently drawn polygons.
        /// </summary>
        internal int TriangleCount
        {
            get { return triangleCount; }
            set { triangleCount = value; }
        }

        /// <summary>
        /// Gets the frames per second count.
        /// </summary>
        internal int FPS
        {
            get { return prevCounter; }
        }

        /// <summary>
        /// Gets or sets the X shift amount applied to all 2D HUD drawings including the text. 
        /// </summary>
        internal int GlobalUIShift
        {
            get { return globalUIShift; }
            set { globalUIShift = value; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a 2D UI component to be rendered on the screen.
        /// </summary>
        /// <remarks>You should only add top-level components that do not have a parent component.</remarks>
        /// <param name="comp2d">A top-level G2DComponent object</param>
        public void Add2DComponent(G2DComponent comp2d)
        {
            if (!comp2Ds.Contains(comp2d))
            {
                if (comp2d.HasParent)
                    throw new GoblinException("You should only add root components to this list. "
                        + "Any component with parents are automatically rendered by its parent " +
                        "component");
                else
                {
                    comp2Ds.Add(comp2d);
                    comp2d.RegisterKeyInput();
                    comp2d.RegisterMouseInput();
                }
            }
        }

        /// <summary>
        /// Removes a 2D UI component from the rendering process.
        /// </summary>
        /// <param name="comp2d"></param>
        public void Remove2DComponent(G2DComponent comp2d)
        {
            comp2Ds.Remove(comp2d);
            comp2d.RemoveKeyInput();
            comp2d.RemoveMouseInput();
        }

        /// <summary>
        /// Adds a 3D UI component to be rendered in the scene.
        /// </summary>
        /// <remarks>You should only add top-level components that do not have a parent component.</remarks>
        /// <param name="comp3d">A top-level G3DComponent object</param>
        internal void Add3DComponent(G3DComponent comp3d)
        {
            if (!comp3Ds.Contains(comp3d))
                comp3Ds.Add(comp3d);
        }

        /// <summary>
        /// Removes a 3D UI component from the rendering process.
        /// </summary>
        /// <param name="comp3d"></param>
        internal void Remove3DComponent(G3DComponent comp3d)
        {
            comp3Ds.Remove(comp3d);
        }

        /// <summary>
        /// Draws all of the user interface components.
        /// </summary>
        /// <remarks>Do not call this method. This method will be called automatically</remarks>
        /// <param name="elapsedTime"></param>
        /// <param name="clear"></param>
        /// <param name="renderRightEye"></param>
        public void Draw(float elapsedTime, bool clear, bool renderRightEye)
        {
            State.Device.DepthStencilState = DepthStencilState.None;

            passedTime += elapsedTime;
            if (passedTime >= 1000)
            {
                passedTime = 0;
                prevCounter = counter;
                counter = 1;
            }
            else if (elapsedTime != 0)
                counter++;

            float y = 5;
            float x = 5;
            if (State.ShowFPS)
            {
                if (debugFont == null)
                    throw new GoblinException("You need to add 'DebugFont.spritefont' file to your " +
                        "content directory before you can display debug information");

                UI2DRenderer.WriteText(new Vector2(x, y), prevCounter + " FPS", State.DebugTextColor, debugFont);
                y += 20;
            }

            if (State.ShowTriangleCount)
            {
                if (debugFont == null)
                    throw new GoblinException("You need to add 'DebugFont.spritefont' file to your " +
                        "content directory before you can display debug information");

                UI2DRenderer.WriteText(new Vector2(x, y), triangleCount + " Triangles", State.DebugTextColor, debugFont);
                y += 20;
            }

            if (State.ShowNotifications)
            {
                if (debugFont == null)
                    throw new GoblinException("You need to add 'DebugFont.spritefont' file to your " +
                        "content directory before you can display debug information");

                if (Notifier.MessageCount > 0)
                    messages.AddRange(Notifier.GetMessages());

                if (messages.Count > 0)
                {
                    float yGap = Notifier.Font.MeasureString(messages[0].Message).Y;
                    Color color;
                    switch (Notifier.Placement)
                    {
                        case Notifier.NotifierPlacement.TopRight:
                            y = 0;
                            for (int i = messages.Count - 1; i >= 0; i--)
                            {
                                color = Notifier.Color;
                                if (messages[i].StartFadeOut)
                                    color = new Color(color.R, color.G, color.B, 
                                        (byte)messages[i].FadeOutInterpolator.Value);
                                UI2DRenderer.WriteText(new Vector2(0, y), messages[i].Message, color,
                                    Notifier.Font, Vector2.One, GoblinEnums.HorizontalAlignment.Right, 
                                    GoblinEnums.VerticalAlignment.None);
                                y += yGap;
                            }
                            break;
                        case Notifier.NotifierPlacement.TopMiddle:
                            y = 0;
                            for (int i = messages.Count - 1; i >= 0; i--)
                            {
                                color = Notifier.Color;
                                if (messages[i].StartFadeOut)
                                    color = new Color(color.R, color.G, color.B,
                                        (byte)messages[i].FadeOutInterpolator.Value);
                                UI2DRenderer.WriteText(new Vector2(0, y), messages[i].Message, color,
                                    Notifier.Font, Vector2.One, GoblinEnums.HorizontalAlignment.Center,
                                    GoblinEnums.VerticalAlignment.None);
                                y += yGap;
                            }
                            break;
                        case Notifier.NotifierPlacement.TopLeft:
                            for (int i = messages.Count - 1; i >= 0; i--)
                            {
                                color = Notifier.Color;
                                if (messages[i].StartFadeOut)
                                    color = new Color(color.R, color.G, color.B,
                                        (byte)messages[i].FadeOutInterpolator.Value);
                                UI2DRenderer.WriteText(new Vector2(0, y), messages[i].Message, color,
                                    Notifier.Font, Vector2.One, GoblinEnums.HorizontalAlignment.Left,
                                    GoblinEnums.VerticalAlignment.None);
                                y += yGap;
                            }
                            break;
                        case Notifier.NotifierPlacement.BottomRight:
                            y = State.Height - yGap;
                            for (int i = messages.Count - 1; i >= 0; i--)
                            {
                                color = Notifier.Color;
                                if (messages[i].StartFadeOut)
                                    color = new Color(color.R, color.G, color.B,
                                        (byte)messages[i].FadeOutInterpolator.Value);
                                UI2DRenderer.WriteText(new Vector2(0, y), messages[i].Message, color,
                                    Notifier.Font, Vector2.One, GoblinEnums.HorizontalAlignment.Right,
                                    GoblinEnums.VerticalAlignment.None);
                                y -= yGap;
                            }
                            break;
                        case Notifier.NotifierPlacement.BottomMiddle:
                            y = State.Height - yGap;
                            for (int i = messages.Count - 1; i >= 0; i--)
                            {
                                color = Notifier.Color;
                                if (messages[i].StartFadeOut)
                                    color = new Color(color.R, color.G, color.B,
                                        (byte)messages[i].FadeOutInterpolator.Value);
                                UI2DRenderer.WriteText(new Vector2(0, y), messages[i].Message, color,
                                    Notifier.Font, Vector2.One, GoblinEnums.HorizontalAlignment.Center,
                                    GoblinEnums.VerticalAlignment.None);
                                y -= yGap;
                            }
                            break;
                        case Notifier.NotifierPlacement.BottomLeft:
                            y = State.Height - yGap;
                            for (int i = messages.Count - 1; i >= 0; i--)
                            {
                                color = Notifier.Color;
                                if (messages[i].StartFadeOut)
                                    color = new Color(color.R, color.G, color.B,
                                        (byte)messages[i].FadeOutInterpolator.Value);
                                UI2DRenderer.WriteText(new Vector2(0, y), messages[i].Message, color,
                                    Notifier.Font, Vector2.One, GoblinEnums.HorizontalAlignment.Left,
                                    GoblinEnums.VerticalAlignment.None);
                                y -= yGap;
                            }
                            break;
                        case Notifier.NotifierPlacement.Custom:
                            x = Notifier.CustomStartLocation.X;
                            y = Notifier.CustomStartLocation.Y;
                            for (int i = messages.Count - 1; i >= 0; i--)
                            {
                                color = Notifier.Color;
                                if (messages[i].StartFadeOut)
                                    color = new Color(color.R, color.G, color.B,
                                        (byte)messages[i].FadeOutInterpolator.Value);
                                UI2DRenderer.WriteText(new Vector2(x, y), messages[i].Message, color,
                                    Notifier.Font, Vector2.One, GoblinEnums.HorizontalAlignment.Right,
                                    GoblinEnums.VerticalAlignment.None);
                                x -= Notifier.CustomAppearDirection.X;
                                y += Notifier.CustomAppearDirection.Y;
                            }
                            break;
                    }

                    if (Notifier.FadeOutTime > 0)
                    {
                        double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        for (int i = 0; i < messages.Count; i++)
                        {
                            if(!messages[i].StartFadeOut){
                                if ((currentTime - messages[i].StartTime) >= Notifier.FadeOutTime)
                                {
                                    messages[i].StartFadeOut = true;
                                    messages[i].FadeOutInterpolator.Start();
                                }
                                else
                                    break;
                            }
                        }

                        int deleteCount = 0;
                        for (int i = 0; i < messages.Count; i++)
                        {
                            if (messages[i].StartFadeOut)
                            {
                                if (messages[i].FadeOutInterpolator.Done)
                                    deleteCount++;
                            }
                            else
                                break;
                        }

                        messages.RemoveRange(0, deleteCount);
                    }
                }
            }

            foreach (G2DComponent comp2d in comp2Ds)
                comp2d.RenderWidget();

            if (renderRightEye)
                UI2DRenderer.Flush(clear, -globalUIShift/2);
            else
                UI2DRenderer.Flush(clear, globalUIShift/2);

            State.Device.DepthStencilState = DepthStencilState.Default;

            UI3DRenderer.Flush(clear);
        }

        public void Dispose()
        {
            comp2Ds.Clear();
            comp3Ds.Clear();
            UI2DRenderer.Dispose();
            UI3DRenderer.Dispose();
        }
        #endregion
    }
}
