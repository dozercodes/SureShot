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
 * Author: Levi Lister (levi.lister@gmail.com)
 *         Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Lidgren.Network;

using GoblinXNA.Helpers;

namespace GoblinXNA.Network
{
    /// <summary>
    /// An implementation of the IServer interface using the Lidgren network library
    /// (http://code.google.com/p/lidgren-library-network).
    /// </summary>
    public class LidgrenServer : IServer
    {
        #region Member Fields

        protected int portNumber;
        protected byte[] myIPAddress;
        protected bool enableEncryption;
        protected String appName;
        protected NetPeerConfiguration netConfig;
        protected NetServer netServer;
        protected Dictionary<String, String> approveList;
        protected NetXtea xtea;

        protected NetConnection prevSender;
        protected Dictionary<String, NetConnection> clients;
        protected List<NetConnection> clientList; // same as clients, but just an IList implmentation
        protected int sequenceChannel;
        protected bool useSequencedInsteadOfOrdered;

        #endregion

        #region Events

        public event HandleClientConnection ClientConnected;
        public event HandleClientDisconnection ClientDisconnected;

        #endregion

        #region Properties

        public int PortNumber
        {
            get { return portNumber; }
            set 
            {
                if (portNumber != value)
                {
                    Shutdown();
                    portNumber = value;
                    Initialize();
                }
            }
        }

        public byte[] MyIPAddress
        {
            get { return myIPAddress; }
        }

        public int NumConnectedClients
        {
            get { return clients.Count; }
        }

        public List<String> ClientIPAddresses
        {
            get
            {
                List<String> ipAddresses = new List<string>();
                foreach (String ipAddress in clients.Keys)
                    ipAddresses.Add(ipAddress);

                return ipAddresses;
            }
        }

        public bool EnableEncryption
        {
            get { return enableEncryption; }
            set { enableEncryption = value; }
        }

        /// <summary>
        /// Gets or sets the net configuration for a Lidgren server.
        /// </summary>
        /// <remarks>
        /// For detailed information about each of the properties of NetConfiguration,
        /// please see the documentation included in the Lidgren's distribution package.
        /// </remarks>
        public NetPeerConfiguration NetConfig
        {
            get { return netConfig; }
            set { netConfig = value; }
        }

        /// <summary>
        /// Gets or sets the delivery channel. Default value is 0. Must be between 0 and 63.
        /// </summary>
        public int SequenceChannel
        {
            get { return sequenceChannel; }
            set { sequenceChannel = value; }
        }

        /// <summary>
        /// Gets or sets whether to use Sequenced deliver method instead of Ordered.
        /// (e.g., ReliableOrdered becomes ReliableSequenced when INetworkObject's Ordered
        /// and Reliable are both set to true)
        /// </summary>
        public bool UseSequencedInsteadOfOrdered
        {
            get { return useSequencedInsteadOfOrdered; }
            set { useSequencedInsteadOfOrdered = value; }
        }

        #endregion 

        #region Constructors
        /// <summary>
        /// Creates a Lidgren network server with an application name and the port number
        /// to establish the connection.
        /// </summary>
        /// <param name="appName">An application name. Can be any names</param>
        /// <param name="portNumber">The port number to establish the connection</param>
        public LidgrenServer(String appName, int portNumber)
        {
            this.portNumber = portNumber;
            this.appName = appName;
            enableEncryption = false;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress addr = ipEntry.AddressList[0];
            myIPAddress = addr.GetAddressBytes();
            approveList = new Dictionary<string, string>();
            prevSender = null;
            clients = new Dictionary<string, NetConnection>();
            clientList = new List<NetConnection>();

            // Create a net configuration
            netConfig = new NetPeerConfiguration(appName);
            netConfig.MaximumConnections = 32;
            netConfig.Port = portNumber;
            netConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            netConfig.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);

            xtea = new NetXtea("GoblinXNA");
            sequenceChannel = 0;
            useSequencedInsteadOfOrdered = false;
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            // Create a server
            netServer = new NetServer(netConfig);
            netServer.Start();
        }

        public void BroadcastMessage(byte[] msg, bool reliable, bool inOrder, bool excludeSender)
        {
            // Test if any connections have been made to this machine, then send data
            if (clients.Count > 0)
            {
                // create new message to send to all clients
                NetOutgoingMessage om = netServer.CreateMessage();
                om.Write(msg);
                if (enableEncryption)
                    om.Encrypt(xtea);

                //Log.Write("Sending message: " + msg.ToString(), Log.LogLevel.Log);
                //Console.WriteLine("Sending message: " + ByteHelper.ConvertToString(msg));

                NetDeliveryMethod deliverMethod = NetDeliveryMethod.Unreliable;
                int channel = sequenceChannel;
                if (reliable)
                {
                    if (inOrder)
                    {
                        if (useSequencedInsteadOfOrdered)
                            deliverMethod = NetDeliveryMethod.ReliableSequenced;
                        else
                            deliverMethod = NetDeliveryMethod.ReliableOrdered;
                    }
                    else
                    {
                        deliverMethod = NetDeliveryMethod.ReliableUnordered;
                        channel = 0;
                    }
                }
                else if (inOrder)
                    deliverMethod = NetDeliveryMethod.UnreliableSequenced;

                // broadcast the message in order
                if (excludeSender && (prevSender != null))
                {
                    // if there is only one connection, then the sender to be excluded is
                    // the only connection the server has, so there is no point to broadcast
                    if (clients.Count > 1)
                    {
                        List<NetConnection> recepients = new List<NetConnection>(
                            clients.Values);
                        recepients.Remove(prevSender);
                        netServer.SendMessage(om, recepients, deliverMethod, channel);
                    }
                }
                else
                    netServer.SendMessage(om, clientList, deliverMethod, channel);
            }
        }

        public void SendMessage(byte[] msg, List<String> ipAddresses, bool reliable, bool inOrder)
        {
            // Test if any connections have been made to this machine, then send data
            if (clients.Count > 0)
            {
                // create new message to send to all clients
                NetOutgoingMessage om = netServer.CreateMessage();
                om.Write(msg);
                if (enableEncryption)
                    om.Encrypt(xtea);

                //Log.Write("Sending message: " + msg.ToString(), Log.LogLevel.Log);
                //Console.WriteLine("Sending message: " + ByteHelper.ConvertToString(msg));

                NetDeliveryMethod deliverMethod = NetDeliveryMethod.Unreliable;
                int channel = sequenceChannel;
                if (reliable)
                {
                    if (inOrder)
                    {
                        if (useSequencedInsteadOfOrdered)
                            deliverMethod = NetDeliveryMethod.ReliableSequenced;
                        else
                            deliverMethod = NetDeliveryMethod.ReliableOrdered;
                    }
                    else
                    {
                        deliverMethod = NetDeliveryMethod.ReliableUnordered;
                        channel = 0;
                    }
                }
                else if (inOrder)
                    deliverMethod = NetDeliveryMethod.UnreliableSequenced;

                List<NetConnection> recipients = new List<NetConnection>();
                foreach (String ipAddress in ipAddresses)
                    if (clients.ContainsKey(ipAddress))
                        recipients.Add(clients[ipAddress]);

                if (recipients.Count > 0)
                    netServer.SendMessage(om, recipients, deliverMethod, channel);
            }
        }

        public int ReceiveMessage(ref byte[] messages)
        {
            NetIncomingMessage msg;
            int totalLength = 0;
            
            // read a packet if available
            while ((msg = netServer.ReadMessage()) != null)
            {
                if (enableEncryption)
                    msg.Decrypt(xtea);

                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        netServer.SendDiscoveryResponse(null, msg.SenderEndpoint);
                        break;
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Log.Write(msg.ReadString(), Log.LogLevel.Log);
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        Log.Write(msg.ReadString(), Log.LogLevel.Warning);
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Log.Write(msg.ReadString(), Log.LogLevel.Error);
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        if (!approveList.ContainsKey(msg.SenderEndpoint.ToString()))
                        {
                            Log.Write("Connection request from IP address: " + msg.SenderEndpoint.Address.ToString(),
                                Log.LogLevel.Log);
                            msg.SenderConnection.Approve();
                            approveList.Add(msg.SenderEndpoint.ToString(), "");
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        string reason = msg.ReadString();
                        Log.Write("New status: " + status + " (" + reason + ")", Log.LogLevel.Log);
                        if (status == NetConnectionStatus.Connected)
                        {
                            clients.Add(msg.SenderEndpoint.ToString(), msg.SenderConnection);
                            clientList.Add(msg.SenderConnection);
                            approveList.Remove(msg.SenderEndpoint.ToString());
                            if (msg.SenderConnection != null)
                                prevSender = clients[msg.SenderEndpoint.ToString()];
                            if (ClientConnected != null)
                                ClientConnected(msg.SenderEndpoint.Address.ToString(), msg.SenderEndpoint.Port);
                        }
                        else if (status == NetConnectionStatus.Disconnected)
                        {
                            clientList.Remove(clients[msg.SenderEndpoint.ToString()]);
                            clients.Remove(msg.SenderEndpoint.ToString());
                            if (ClientDisconnected != null)
                                ClientDisconnected(msg.SenderEndpoint.Address.ToString(), msg.SenderEndpoint.Port);
                        }

                        break;
                    case NetIncomingMessageType.Data:
                        totalLength += msg.LengthBytes;
                        if (messages.Length < totalLength)
                            ByteHelper.ExpandArray(ref messages, totalLength);
                        msg.ReadBytes(messages, totalLength - msg.LengthBytes, msg.LengthBytes);
                        if (msg.SenderConnection != null)
                            prevSender = clients[msg.SenderEndpoint.ToString()];
                        break;
                }
                netServer.Recycle(msg);
            }

            return totalLength;
        }

        public void Shutdown()
        {
            // shutdown; sends disconnect to all connected clients with this reason string
            netServer.Shutdown("Application exiting");
        }

        #endregion
    }
}
