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
using System.Device.Location;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GoblinXNA.Device.Accelerometer
{
    public class GPSMobile : InputDevice
    {
        #region Member Fields

        Vector3 LatLonAlt;
        Vector3 tempLatLonAlt;

        GeoCoordinateWatcher geoWatcher;

        DateTimeOffset geoTimeStamp;

        String id;

        #endregion

        #region Constructors

        public GPSMobile()
        {
            geoWatcher = new GeoCoordinateWatcher();
            LatLonAlt = new Vector3();
            tempLatLonAlt = new Vector3();
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets a unique identifier of this input device
        /// </summary>
        public String Identifier
        {
            get { return id; }
            set { id = value; }
        }

        public Vector3 CurrentPosition
        {
            get { return LatLonAlt; }
        }

        public DateTimeOffset TimeStamp
        {
            get {return geoTimeStamp;}
        }
        /// <summary>
        /// Gets whether this input device is available to use
        /// </summary>
       public bool IsAvailable { get { return geoWatcher.Status.Equals(GeoPositionStatus.Ready); } }
        #endregion

        #region Public Methods

        public void Initialize()
        {
            geoWatcher.PositionChanged += OnGeoWatcherPositionChanged;
            geoWatcher.Start();
        }

        public void Stop()
        {
            geoWatcher.Stop();
        }


        /// <summary>
        /// Updates the state of this input device
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="deviceActive"></param>
        public void Update(TimeSpan elapsedTime, bool deviceActive)
        {
            //Nothing to implement? Interesting... Must validate this.
        }
        /// <summary>
        /// Triggers the callback functions specified in this InputDevice programatically.
        /// </summary>
        /// <param name="data"></param>
        public void TriggerDelegates(byte[] data)
        {

            throw new GoblinException("TriggerDelegates for Accelerometer is not implemented yet.");

        }

        /// <summary>
        /// Disposes this input device.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region Private Methods

        void OnGeoWatcherPositionChanged(object sender,
                                         GeoPositionChangedEventArgs<GeoCoordinate> args)
        {
            LatLonAlt.X = (float)args.Position.Location.Latitude;
            LatLonAlt.Y = (float)args.Position.Location.Longitude;
            LatLonAlt.Z = (float)args.Position.Location.Altitude;

            geoTimeStamp = args.Position.Timestamp;
        }

        #endregion
    }
}
