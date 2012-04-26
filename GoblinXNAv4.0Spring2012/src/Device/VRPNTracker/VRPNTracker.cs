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
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Helpers;

using Vrpn; //required library to read VRPN streams

namespace GoblinXNA.Device.VRPNTracker
{
    /// <summary>
    /// A 6DOF input device class that supports the VRPN protocol to get a device's
    /// orientation and position. 
    /// </summary>
    public class VRPNTracker:InputDevice_6DOF
    {
        #region Member fields

        private String identifier;
        private String trackerName;
        private String ipAddress;
        private bool isAvailable;

        private Microsoft.Xna.Framework.Quaternion _orientation;
        
        private float _yaw;
        private float _pitch;
        private float _roll;

        private Microsoft.Xna.Framework.Vector3 _position;

        private float _xpos;
        private float _ypos;
        private float _zpos;

        private TrackerRemote tracker;

        private static VRPNTracker VRPNtracker;

        #endregion

        #region Constructor
        
        /// <summary>
        /// The constructor for the VRPN tracker
        /// </summary>
        public VRPNTracker()
        {
            identifier = "VRPNTracker";
            trackerName = "Tracker 1";
            ipAddress = "localhost";
            tracker = new TrackerRemote(trackerName + "@" + ipAddress);
            isAvailable = true;
            _orientation = Microsoft.Xna.Framework.Quaternion.Identity;
            _yaw = 0;
            _pitch = 0;
            _roll = 0;

            _position = Microsoft.Xna.Framework.Vector3.One;
            _xpos = 0;
            _ypos = 0;
            _zpos = 0;
        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Returns and sets the identifier of the VRPN Tracker
        /// </summary>
        public String Identifier
        {
            get { return identifier; }
            set { identifier = value; }
        }

        /// <summary>
        /// Sets and returns the name of the objects tracked through VRPN
        /// </summary>
        public String TrackerName
        {
            get { return trackerName; }
            set { trackerName = value; }
        }

        /// <summary>
        /// Sets and returns the IP Address of the VRPN tracker
        /// </summary>
        public String IPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }

        /// <summary>
        /// Returns whether the VRPN tracker is available
        /// </summary>
        public bool IsAvailable
        {
            get { return isAvailable; }
        }

        /// <summary>
        /// Gets the yaw (in radians) updated by the device's orientation tracker. 
        /// </summary>
        public float Yaw
        {
            get { return _yaw; }
        }

        /// <summary>
        /// Gets the pitch (in radians) updated by the device's orientation tracker. 
        /// </summary>
        public float Pitch
        {
            get { return _pitch; }
        }

        /// <summary>
        /// Gets the roll (in radians) updated by the device's orientation tracker. 
        /// </summary>
        public float Roll
        {
            get { return _roll; }
        }

        /// <summary>
        /// Gets the X position (in millimeters) updated by the device's orientation tracker. 
        /// </summary>
        public float XPos
        {
            get { return _xpos; }
        }

        /// <summary>
        /// Gets the Y position (in millimeters) updated by the device's orientation tracker. 
        /// </summary>
        public float YPos
        {
            get { return _ypos; }
        }

        /// <summary>
        /// Gets the Z position (in millimeters) updated by the device's orientation tracker. 
        /// </summary>
        public float ZPos
        {
            get { return _zpos; }
        }

        /// <summary>
        /// Returns the Rotation Quaternion of the VRPN tracker 
        /// </summary>
        public Microsoft.Xna.Framework.Quaternion Rotation
        {
            get
            {
                return _orientation;
            }
        }

        /// <summary>
        /// Returns the Vector3 position of the VRPN tracker
        /// </summary>
        public Microsoft.Xna.Framework.Vector3 Translation
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// Returns the World Transformation matrix of the VRPN Tracker
        /// </summary>
        public Matrix WorldTransformation
        {
            get
            {
                Matrix temp2 = Matrix.CreateFromQuaternion(_orientation);
                Matrix temp1 = new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
                temp1.Translation = _position;

                temp2 = Matrix.CreateFromQuaternion(_orientation) * Matrix.CreateTranslation(_position);

                return temp2;
            }
        }

        /// <summary>
        /// Gets the instance of VRPN Tracker.
        /// </summary>
        public static VRPNTracker Instance
        {
            get
            {
                if (VRPNtracker == null)
                    VRPNtracker = new VRPNTracker();

                return VRPNtracker;
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the VRPN tracker.
        /// </summary>
        public void Initialize()
        {
            tracker = new TrackerRemote(trackerName + "@" + ipAddress);
            tracker.PositionChanged += new TrackerChangeEventHandler(PositionChanged);
            tracker.MuteWarnings = false;
        }

        /// <summary>
        /// Updates the VRPN tracker whenever it changes position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"> Information about the object tracked</param>
        private void PositionChanged(object sender, TrackerChangeEventArgs e)
        {
            //We convert movement to millimeters.

            _position = new Microsoft.Xna.Framework.Vector3((float)e.Position.X, (float)e.Position.Y, (float)e.Position.Z);
            _xpos = (float)e.Position.X;
            _ypos = (float)e.Position.Y;
            _zpos = (float)e.Position.Z;
            
            _orientation = new Microsoft.Xna.Framework.Quaternion((float)e.Orientation.X, (float)e.Orientation.Y, (float)e.Orientation.Z, (float)e.Orientation.W);
            _roll = (float)Math.Atan2(2 * (e.Orientation.X * e.Orientation.Y + e.Orientation.W * e.Orientation.Z), e.Orientation.W * e.Orientation.W + e.Orientation.X * e.Orientation.X - e.Orientation.Y * e.Orientation.Y - e.Orientation.Z * e.Orientation.Z);
            _pitch = (float)Math.Atan2(2 * (e.Orientation.Y * e.Orientation.Z + e.Orientation.W * e.Orientation.X), e.Orientation.W * e.Orientation.W - e.Orientation.X * e.Orientation.X - e.Orientation.Y * e.Orientation.Y + e.Orientation.Z * e.Orientation.Z);
            _yaw = (float)Math.Asin(-2 * (e.Orientation.X * e.Orientation.Z - e.Orientation.W * e.Orientation.Y));
            
        }

        public void Update(TimeSpan elapsedTime, bool deviceActive)
        {
            tracker.Update();
        }

        public void Dispose()
        {
            tracker.Dispose();
            tracker = null;
        }
        #endregion
    }
}
