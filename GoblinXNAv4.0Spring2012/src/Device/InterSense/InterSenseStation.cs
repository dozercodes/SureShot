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
 * Authors: Mark Eaddy
 *          Ohan Oda (ohan@cs.columbia.edu)
 *          Steve Henderson (henderso@cs.columbia.edu)
 * 
 *************************************************************************************/ 

using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using System.Xml.Serialization;

namespace GoblinXNA.Device.InterSense
{
	/// <summary>
	/// Summary description for InterSenseStation.
	/// </summary>
	public class InterSenseStation
    {
        #region Member Fields
		private long nStationIndex;

		private Matrix mat;
		private ISDllBridge.ISD_STATION_STATE_TYPE state;

        private Vector3 positionVector;
        private Vector3 orientationVector;
        #endregion

        #region Constructors

        public InterSenseStation(long _nStationIndex)
		{
			nStationIndex = _nStationIndex;
            mat = Matrix.Identity;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The raw position vector from the Intersense Tracker
        /// </summary>
        public Vector3 PositionVector
        {
            get { return positionVector; }
        }

        /// <summary>
        /// The raw orientation vector from the Intersense Tracker
        /// </summary>
        public Vector3 OrientationVector
        {
            get { return orientationVector; }
        }

        public Matrix WorldTransformation
        {
            get { return mat; }
        }

        #endregion

        #region Public Methods

        public void SetData(ISDllBridge.ISD_TRACKER_DATA_TYPE dataISense)
		{
			Debug.Assert(nStationIndex != -1);
			state = dataISense.Station[nStationIndex];
            positionVector.X = state.Position[0];
            positionVector.Y = state.Position[1];
            positionVector.Z = state.Position[2];
            orientationVector.X = state.Orientation[0];
            orientationVector.Y = state.Orientation[1];
            orientationVector.Z = state.Orientation[2];
            CreateWorldMatrix();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the WorldViewMatrix of the tracker to reflect the following coordinate frame 
        /// (right handed coordinates):        
        /// 
        /// //  ---Ceiling in Lab---
        ///
        ///     +Y
        ///      ^  ^ +Z (Back Wall)    
        ///  +X  | /       
        ///  ----         
        ///
        ///  ---Floor in Lab ----
        ///
        /// The code in this class is a specic to the coordinate frame listed above
        ///            
        /// </summary>
        private void CreateWorldMatrix()
		{
            // InterSense is a right-hand coordinate system.
            // In a ceiling mounted array, the coordinate systems is natively oriented like this:
            //
            // InterSense:    
            //
            //  ---Ceiling in Lab---
            //
            //        ^       
            //   X   / y      
            //  <---/         
            //      |         
            //    z v
            //
            //  ---Floor in Lab ----            
            //
            // The following code has been tested to perform the coordinate frame transform required to produce an
            // XNA-friendly (Y Up) world.  Note: the default intersense coordinate frame is also right-handed, so the
            // operations below are strictly coordinate frame transforms.  This cannot be done with simple rotations, because
            // the axis angles in XNA don't coincide with intersense.
            //
            Vector3 posV = this.positionVector;
            Vector3 orientV = this.orientationVector;

            float xnaPitch, xnaYaw, xnaRoll;
            float xnaPitchDeg, xnaYawDeg, xnaRollDeg;
            xnaPitchDeg = orientV.Y;
            xnaYawDeg = -1 * (orientV.X + 90.0f);
            xnaRollDeg = orientV.Z;

            xnaRoll = MathHelper.ToRadians(xnaRollDeg);
            xnaPitch = MathHelper.ToRadians(xnaPitchDeg);
            xnaYaw = MathHelper.ToRadians(xnaYawDeg);

            xnaRoll = -1 * MathHelper.WrapAngle(xnaRoll);
            xnaYaw = MathHelper.WrapAngle(xnaYaw);
            xnaPitch = MathHelper.WrapAngle(xnaPitch);

            float x, y, z;

            z = posV.Y;
            x = posV.X;
            y = -1 * posV.Z;

            mat = Matrix.CreateRotationZ(xnaRoll) * Matrix.CreateRotationX(xnaPitch) * 
                Matrix.CreateRotationY(xnaYaw) * Matrix.CreateTranslation(x, y, z);
        }

        #endregion
    }
}
