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
    /// A callback/delegate function for client connection event
    /// </summary>
    /// <param name="clientIP">The IP address of the client that got connected</param>
    /// <param name="portNumber">The port number used for the connection</param>
    public delegate void HandleClientConnection(String clientIP, int portNumber);

    /// <summary>
    /// A callback/delegate function for client disconnection event
    /// </summary>
    /// <param name="clientIP">The IP address of the client that got disconnected</param>
    /// <param name="portNumber">The port number used for the connection</param>
    public delegate void HandleClientDisconnection(String clientIP, int portNumber);

    /// <summary>
    /// An interface that defines the properties and methods of a network server.
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// Gets or sets the port number used to transfer messages.
        /// </summary>
        int PortNumber { get; set; }

        /// <summary>
        /// Gets the address of this server in 4 bytes.
        /// </summary>
        /// <remarks>
        /// It returns 4 bytes instead of String in order to optimize the network transfer.
        /// (e.g., 192.168.0.1 will be byte[0] = (byte)192, byte[1] = (byte)168, byte[2] = (byte)0,
        /// byte[3] = (byte)1)
        /// </remarks>
        byte[] MyIPAddress { get; }

        /// <summary>
        /// Gets the number of connected clients to this server.
        /// </summary>
        int NumConnectedClients { get; }

        /// <summary>
        /// Gets the addresses of connected clients.
        /// </summary>
        List<String> ClientIPAddresses { get; }

        /// <summary>
        /// Gets or sets whether to enable encryption.
        /// </summary>
        bool EnableEncryption { get; set; }

        /// <summary>
        /// Adds or removes an event handler for client connection event.
        /// </summary>
        event HandleClientConnection ClientConnected;

        /// <summary>
        /// Adds or removes an event handler for client disconnection event.
        /// </summary>
        event HandleClientDisconnection ClientDisconnected;

        /// <summary>
        /// Initializes this server for accepting connections.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Broadcasts a message to all of the connected clients except the sender if 
        /// 'excludeSender' is set to false. Otherwise, it broadcasts to all connected clients.
        /// </summary>
        /// <param name="msg">The message to broadcast</param>
        /// <param name="reliable">Whether the message is guaranteed to arrive at the
        /// receiver side</param>
        /// <param name="inOrder">Whether the message arrives in order at the receiver side</param>
        /// <param name="excludeSender">Whether to exclude the sender</param>
        void BroadcastMessage(byte[] msg, bool reliable, bool inOrder, bool excludeSender);

        /// <summary>
        /// Sends a message to a list of specific clients with the given IP addresses.
        /// </summary>
        /// <param name="msg">The message to send</param>
        /// <param name="ipAddresses">The IP addresses of the clients to send to</param>
        /// <param name="reliable">Whether the message is guaranteed to arrive at the
        /// receiver side</param>
        /// <param name="inOrder">Whether the message arrives in order at the receiver side</param>
        void SendMessage(byte[] msg, List<String> ipAddresses, bool reliable, bool inOrder);

        /// <summary>
        /// Concatenates received messages in byte arrays to the passed list.
        /// </summary>
        /// <param name="messages">Received messages in array of bytes</param>
        /// <param name="inOrder">Whether the message arrives in order at the receiver side</param>
        int ReceiveMessage(ref byte[] messages);

        /// <summary>
        /// Shuts down the server.
        /// </summary>
        void Shutdown();
    }
}
