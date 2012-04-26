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
using System.Xml;

namespace GoblinXNA.Network
{
    /// <summary>
    /// An interface that defines the properties and methods of an object that can be sent 
    /// over the network.
    /// </summary>
    public interface INetworkObject
    {
        #region Properties

        /// <summary>
        /// Gets an identifier of this network object (has to be unique).
        /// </summary>
        String Identifier { get; }

        /// <summary>
        /// Gets or sets whether this network object is ready to be sent. This variable
        /// can be used for optimization to not send any objects that did not change.
        /// </summary>
        bool ReadyToSend { get; set; }

        /// <summary>
        /// Gets or sets whether to hold the information to be transferred.
        /// </summary>
        bool Hold { get; set; }

        /// <summary>
        /// Gets or sets whether the receiver is guaranteed to receive the information
        /// </summary>
        bool Reliable { get; set; }

        /// <summary>
        /// Gets or sets whether the receiver will receive the information in the order
        /// sent by the sender. 
        /// </summary>
        bool Ordered { get; set; }

        /// <summary>
        /// Gets or sets the channel used to transfer this network object.
        /// </summary>
        //int Channel { get; set; }

        /// <summary>
        /// Gets or sets the frequency to send information in terms of Hz. For example,
        /// 2 Hz means send twice per second, and 60 Hz means send 60 times per second.
        /// </summary>
        int SendFrequencyInHertz { get; set; }

        /// <summary>
        /// Gets all of the information that needs to be sent over the network.
        /// </summary>
        /// <returns></returns>
        byte[] GetMessage();

        /// <summary>
        /// Interprets the information associated with this object received over the network.
        /// </summary>
        /// <param name="msg">A byte array that contains the message associated with this network object</param>
        /// <param name="startIndex">The starting index in the byte array where associated information starts</param>
        /// <param name="length"></param>
        void InterpretMessage(byte[] msg, int startIndex, int length);

        #endregion
    }
}
