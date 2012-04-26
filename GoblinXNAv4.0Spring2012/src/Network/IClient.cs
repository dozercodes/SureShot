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

namespace GoblinXNA.Network
{
    /// <summary>
    /// A callback/delegate function for server connection event
    /// </summary>
    public delegate void HandleServerConnection();

    /// <summary>
    /// A callback/delegate function for server disconnection event
    /// </summary>
    public delegate void HandleServerDisconnection();

    /// <summary>
    /// An interface that defines the properties and methods of a network client.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Gets the port number used to transfer messages.
        /// </summary>
        int PortNumber { get;}

        /// <summary>
        /// Gets the address of this client in 4 bytes.
        /// </summary>
        /// <remarks>
        /// It returns 4 bytes instead of String in order to optimize network transfer.
        /// (e.g., 192.168.0.1 will be byte[0] = (byte)192, byte[1] = (byte)168, byte[2] = (byte)0,
        /// byte[3] = (byte)1)
        /// </remarks>
        byte[] MyIPAddress { get; }

        /// <summary>
        /// Gets or sets whether to enable encryption.
        /// </summary>
        bool EnableEncryption { get; set; }

        /// <summary>
        /// Adds or removes an event handler for client connection event.
        /// </summary>
        event HandleServerConnection ServerConnected;

        /// <summary>
        /// Adds or removes an event handler for client disconnection event.
        /// </summary>
        event HandleServerDisconnection ServerDisconnected;

        /// <summary>
        /// Gets whether this client is connected to a server.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets or sets whether to wait for the server to start up in case the server is 
        /// not running when client tried to connect. To set the timeout for connection
        /// trial, set the ConnectionTrialTimeOut property. Default value is false.
        /// </summary>
        /// <see cref="ConnectionTrialTimeOut"/>
        bool WaitForServer { get; set; }

        /// <summary>
        /// Gets or sets the timeout in milliseconds for connection trial when the server was
        /// not up when client was started. This property is only effective if WaitForServer
        /// is set to true. Default value is -1 which means the client waits for infinite time. 
        /// </summary>
        /// <see cref="WaitForServer"/>
        int ConnectionTrialTimeOut { get; set; }

        /// <summary>
        /// Connects to the server specified by HostName.
        /// </summary>
        void Connect();

        /// <summary>
        /// Concatenates received messages from the server in byte arrays to the passed list.
        /// </summary>
        /// <param name="messages">Received messages in array of bytes</param>
        /// <returns>The number of received bytes</returns>
        int ReceiveMessage(ref byte[] messages);

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="msg">The message to be sent</param>
        /// <param name="reliable">Whether the message is guaranteed to arrive at the
        /// receiver side</param>
        /// <param name="inOrder">Whether the message arrives in order at the receiver side</param>
        void SendMessage(byte[] msg, bool reliable, bool inOrder);

        /// <summary>
        /// Shuts down the client.
        /// </summary>
        void Shutdown();
    }
}
