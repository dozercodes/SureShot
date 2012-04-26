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

using GoblinXNA.Helpers;

namespace GoblinXNA.UI
{
    /// <summary>
    /// A helper class for displaying graphical debugging/notification messages on the HUD.
    /// </summary>
    /// <remarks>
    /// By default, all of the appended messages are displayed on the HUD forever. If you want
    /// to make them disappear after a specified period of time, set FadeOutTime to force the messages
    /// to fade out after the specified time.
    /// </remarks>
    public sealed class Notifier
    {
        /// <summary>
        /// An enum for display location of the debugging/notification messages.
        /// </summary>
        public enum NotifierPlacement
        {
            TopRight,
            TopMiddle,
            TopLeft,
            BottomRight,
            BottomMiddle,
            BottomLeft,
            Custom
        }

        /// <summary>
        /// A helper class to hold the notifier info and states
        /// </summary>
        internal class NotifierMessage
        {
            public String Message;
            public double StartTime;
            public Interpolator FadeOutInterpolator;
            public bool StartFadeOut;

            public NotifierMessage(String message, double startTime, Interpolator fadeOut)
            {
                Message = message;
                StartTime = startTime;
                FadeOutInterpolator = fadeOut;
                StartFadeOut = false;
            }
        }

        private static List<NotifierMessage> notifications;
        private static NotifierPlacement placement;
        private static Vector2 customLocation;
        private static Vector2 customAppearDir;
        private static int fadeoutTime;
        private static Interpolator fadeoutInterporator;
        private static int messageCount;
        private static SpriteFont font;
        private static Color color;

        /// <summary>
        /// A static constructor to initialize all of the necessary member fields.
        /// </summary>
        /// <remarks>
        /// You don't need to call this constructor. This constructor is called automatically when
        /// you access any of its properties or functions.
        /// </remarks>
        static Notifier()
        {
            notifications = new List<NotifierMessage>();
            placement = NotifierPlacement.TopRight;
            fadeoutInterporator = new Interpolator(255, 0, 1000, InterpolationMethod.Linear);
            fadeoutTime = -1;
            font = null;
            customLocation = Vector2.Zero;
            customAppearDir = Vector2.Zero;
            color = Color.Red;
        }

        /// <summary>
        /// Gets or sets the display location of the debugging/notification messages.
        /// </summary>
        /// <remarks>
        /// The default value is NotifierPlacement.TopRight. If defined as Custom, then you need to
        /// set the CustomStartLocation and CustomAppearDirection properties.
        /// </remarks>
        /// <see cref="CustomStartLocation"/>
        /// <seealso cref="CustomAppearDirection"/>
        public static NotifierPlacement Placement
        {
            get { return placement; }
            set { placement = value; }
        }

        /// <summary>
        /// Gets or sets the custom starting location of the notifier messages to appear when Placement 
        /// property is set to Custom.
        /// </summary>
        /// <see cref="Placement"/>
        public static Vector2 CustomStartLocation
        {
            get { return customLocation; }
            set { customLocation = value; }
        }

        /// <summary>
        /// Gets or sets the direction in which the text appears when Placement property is set
        /// to Custom.
        /// </summary>
        /// <see cref="Placement"/>
        public static Vector2 CustomAppearDirection
        {
            get { return customAppearDir; }
            set { customAppearDir = value; }
        }

        /// <summary>
        /// Gets or sets how long each message will be displayed on the HUD before they
        /// start to fade out specified in milliseconds. The newly set values affects
        /// only the newly added messages.
        /// </summary>
        /// <remarks>
        /// The default value is -1, which means the messages never fade out.
        /// </remarks>
        public static int FadeOutTime
        {
            get { return fadeoutTime; }
            set { fadeoutTime = value; }
        }

        /// <summary>
        /// Gets or sets the sprite font to use to display the messages.
        /// </summary>
        /// <remarks>
        /// The default sprint font is same as the one used for FPS and triangle count.
        /// </remarks>
        public static SpriteFont Font
        {
            get { return font; }
            set { font = value; }
        }

        /// <summary>
        /// Gets or sets the color of the debugging/notification messages.
        /// </summary>
        /// <remarks>
        /// The default color is Color.Red
        /// </remarks>
        public static Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Gets the number of currently displayed messages.
        /// </summary>
        internal static int MessageCount
        {
            get { return messageCount; }
        }

        /// <summary>
        /// Appends a new debugging/notification message.
        /// </summary>
        /// <param name="msg">A debugging/notification message to be appended</param>
        public static void AddMessage(String msg)
        {
            notifications.Add(new NotifierMessage(msg, DateTime.Now.TimeOfDay.TotalMilliseconds,
                new Interpolator(fadeoutInterporator.StartValue, fadeoutInterporator.EndValue,
                fadeoutInterporator.Duration, fadeoutInterporator.Method)));
            messageCount++;
        }

        /// <summary>
        /// Gets all of the newly appended messages from the last time calling this method.
        /// All of the newly appended messages are cleared when this method is called.
        /// </summary>
        /// <returns></returns>
        internal static List<NotifierMessage> GetMessages()
        {
            List<NotifierMessage> msgs = new List<NotifierMessage>(notifications);
            notifications.Clear();
            messageCount = 0;
            return msgs;
        }
    }
}
