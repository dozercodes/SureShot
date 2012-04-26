/************************************************************************************ 
 * Copyright (c) 2008-2012, Columbia University
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

using GoblinXNA.Network;
using GoblinXNA.Helpers;

namespace Tutorial10___Networking
{
    /// <summary>
    /// An implementation of INetworkObject interface for transmitting mouse press information.
    /// </summary>
    public class MouseNetworkObject : INetworkObject
    {
        /// <summary>
        /// A delegate function to be called when a mouse press event is sent over the network
        /// </summary>
        /// <param name="near">A point on the near clipping plane</param>
        /// <param name="far">A point on the far clipping plane</param>
        public delegate void ShootFunction(Vector3 near, Vector3 far);

        #region Member Fields

        private bool readyToSend;
        private bool hold;
        private int sendFrequencyInHertz;

        private bool reliable;
        private bool inOrder;

        private int pressedButton;
        private Vector3 nearPoint;
        private Vector3 farPoint;
        private ShootFunction callbackFunc;

        #endregion

        #region Constructors

        public MouseNetworkObject()
        {
            readyToSend = false;
            hold = false;
            sendFrequencyInHertz = 0;

            reliable = true;
            inOrder = true;
        }

        #endregion

        #region Properties
        public String Identifier
        {
            get { return "MouseNetworkObject"; }
        }

        public bool ReadyToSend
        {
            get { return readyToSend; }
            set { readyToSend = value; }
        }

        public bool Hold
        {
            get { return hold; }
            set { hold = value; }
        }

        public int SendFrequencyInHertz
        {
            get { return sendFrequencyInHertz; }
            set { sendFrequencyInHertz = value; }
        }

        public bool Reliable
        {
            get { return reliable; }
            set { reliable = value; }
        }

        public bool Ordered
        {
            get { return inOrder; }
            set { inOrder = value; }
        }

        public ShootFunction CallbackFunc
        {
            get { return callbackFunc; }
            set { callbackFunc = value; }
        }

        public int PressedButton
        {
            get { return pressedButton; }
            set { pressedButton = value; }
        }

        public Vector3 NearPoint
        {
            get { return nearPoint; }
            set { nearPoint = value; }
        }

        public Vector3 FarPoint
        {
            get { return farPoint; }
            set { farPoint = value; }
        }
        #endregion

        #region Public Methods
        public byte[] GetMessage()
        {
            // 1 byte: pressedButton
            // 12 bytes: near point (3 floats)
            // 12 bytes: far point (3 floats)
            byte[] data = new byte[1 + 12 + 12];

            data[0] = (byte)pressedButton;
            ByteHelper.FillByteArray(ref data, 1, BitConverter.GetBytes(nearPoint.X));
            ByteHelper.FillByteArray(ref data, 5, BitConverter.GetBytes(nearPoint.Y));
            ByteHelper.FillByteArray(ref data, 9, BitConverter.GetBytes(nearPoint.Z));
            ByteHelper.FillByteArray(ref data, 13, BitConverter.GetBytes(farPoint.X));
            ByteHelper.FillByteArray(ref data, 17, BitConverter.GetBytes(farPoint.Y));
            ByteHelper.FillByteArray(ref data, 21, BitConverter.GetBytes(farPoint.Z));

            return data;
        }

        public void InterpretMessage(byte[] msg, int startIndex, int length)
        {
            pressedButton = (int)msg[startIndex];

            nearPoint.X = ByteHelper.ConvertToFloat(msg, startIndex + 1);
            nearPoint.Y = ByteHelper.ConvertToFloat(msg, startIndex + 5);
            nearPoint.Z = ByteHelper.ConvertToFloat(msg, startIndex + 9);

            farPoint.X = ByteHelper.ConvertToFloat(msg, startIndex + 13);
            farPoint.Y = ByteHelper.ConvertToFloat(msg, startIndex + 17);
            farPoint.Z = ByteHelper.ConvertToFloat(msg, startIndex + 21);

            if (callbackFunc != null)
                callbackFunc(nearPoint, farPoint);
        }

        #endregion
    }
}
