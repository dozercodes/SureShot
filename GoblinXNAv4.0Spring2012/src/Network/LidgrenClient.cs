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
using System.Threading;
using System.Net.Sockets;

using Lidgren.Network;

using GoblinXNA.Helpers;

namespace GoblinXNA.Network
{
    /// <summary>
    /// An implementation of the IClient interface using the Lidgren network library
    /// (http://code.google.com/p/lidgren-library-network).
    /// </summary>
    public class LidgrenClient : IClient
    {
        #region Member Fields

        protected String appName;
        protected int portNumber;
        protected IPEndPoint hostPoint;
        protected byte[] myIPAddress;
        protected bool enableEncryption;
        protected bool isConnected;
        protected bool isServerDiscovered;
        protected bool waitForServer;
        protected int connectionTrialTimeout;
        protected int elapsedTime;
        protected bool shutDownForced;

        protected NetClient netClient;
        protected IPAddress myAddr;
        protected NetPeerConfiguration netConfig;
        protected bool isLocalAddress;

        protected int sequenceChannel;
        protected NetXtea xtea;
        protected bool useSequencedInsteadOfOrdered;

        #endregion

        #region Events

        public event HandleServerConnection ServerConnected;
        public event HandleServerDisconnection ServerDisconnected;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a Lidgren network client with an application name, the port number,
        /// and the host name.
        /// </summary>
        /// <param name="appName">An application name. Must be the same as the server app name.</param>
        /// <param name="portNumber">The port number to establish the connection</param>
        /// <param name="hostName">The name of the server machine</param>
        public LidgrenClient(String appName, int portNumber, String hostName)
        {
            this.appName = appName;
            this.portNumber = portNumber;
            isConnected = false;
            isServerDiscovered = false;
            shutDownForced = false;
            enableEncryption = false;
            waitForServer = false;
            connectionTrialTimeout = -1;
            elapsedTime = 0;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            myAddr = ipEntry.AddressList[0];
            myIPAddress = myAddr.GetAddressBytes();

            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            IPAddress hostAddr = hostEntry.AddressList[0];
            hostPoint = new IPEndPoint(hostAddr, portNumber);

            isLocalAddress = IsLocalIpAddress(hostName);

            // Create a configuration for the client
            netConfig = new NetPeerConfiguration(appName);

            xtea = new NetXtea("GoblinXNA");
            sequenceChannel = 0;
            useSequencedInsteadOfOrdered = false;
        }

        /// <summary>
        /// Creates a Lidgren network client with an application name, the port number,
        /// and the host IP address in 4 bytes.
        /// </summary>
        /// <param name="appName">An application name. Must be the same as the server app name.</param>
        /// <param name="portNumber">The port number to establish the connection</param>
        /// <param name="hostIPAddress">The IP address of the host in 4 bytes</param>
        public LidgrenClient(String appName, int portNumber, byte[] hostIPAddress)
        {
            this.appName = appName;
            this.portNumber = portNumber;
            isConnected = false;
            isServerDiscovered = false;
            enableEncryption = false;
            shutDownForced = false;
            waitForServer = false;
            connectionTrialTimeout = -1;
            elapsedTime = 0;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            myAddr = ipEntry.AddressList[0];
            myIPAddress = myAddr.GetAddressBytes();

            IPAddress hostAddr = new IPAddress(hostIPAddress);
            hostPoint = new IPEndPoint(hostAddr, portNumber);

            isLocalAddress = IsLocalIpAddress(hostAddr.ToString());

            // Create a configuration for the client
            netConfig = new NetPeerConfiguration(appName);

            xtea = new NetXtea("GoblinXNA");
            sequenceChannel = 0;
            useSequencedInsteadOfOrdered = false;
        }
        #endregion

        #region Properties

        public int PortNumber
        {
            get { return portNumber; }
        }

        public byte[] MyIPAddress
        {
            get { return myIPAddress; }
        }

        public bool IsConnected
        {
            get { return isConnected; }
        }

        /// <summary>
        /// Gets or sets whether to enable encryption.
        /// </summary>
        public bool EnableEncryption
        {
            get { return enableEncryption; }
            set { enableEncryption = value; }
        }

        public bool WaitForServer
        {
            get { return waitForServer; }
            set { waitForServer = value; }
        }

        public int ConnectionTrialTimeOut
        {
            get { return connectionTrialTimeout; }
            set { connectionTrialTimeout = value; }
        }

        /// <summary>
        /// Gets or sets the net configuration for a Lidgren client.
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

        #region Public Methods

        public static bool IsLocalIpAddress(string host)
        {
            try
            { // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public void Connect()
        {
            if (netClient != null && netClient.Status == NetPeerStatus.Running)
                return;

            isConnected = false;

            netConfig.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);

            // Create a client
            netClient = new NetClient(netConfig);
            netClient.Start();

            try
            {
                if (waitForServer)
                {
                    Thread connectionThread = new Thread(new ThreadStart(TryConnect));
                    connectionThread.Start();
                }
                else
                {
                    if (isLocalAddress)
                        netClient.DiscoverLocalPeers(portNumber);
                    else
                        netClient.DiscoverKnownPeer(hostPoint);
                }
            }
            catch (SocketException se)
            {
                Log.Write("Socket exception is thrown in Connect: " + se.StackTrace);
            }
        }

        private void TryConnect()
        {
            while (!isServerDiscovered && !shutDownForced)
            {
                if (isLocalAddress)
                    netClient.DiscoverLocalPeers(portNumber);
                else
                    netClient.DiscoverKnownPeer(hostPoint);

                Thread.Sleep(500);

                elapsedTime += 500;

                if ((connectionTrialTimeout != -1) && (elapsedTime >= connectionTrialTimeout))
                    break;
            }
        }

        public int ReceiveMessage(ref byte[] messages)
        {
            NetIncomingMessage msg;
            int totalLength = 0;

            try
            {
                // read a packet if available
                while ((msg = netClient.ReadMessage()) != null)
                {
                    if (enableEncryption)
                        msg.Decrypt(xtea);

                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryResponse:
                            netClient.Connect(msg.SenderEndpoint);
                            isServerDiscovered = true;
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
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            string reason = msg.ReadString();
                            Log.Write("New status: " + status + " (" + reason + ")", Log.LogLevel.Log);
                            if (status == NetConnectionStatus.Connected)
                            {
                                isConnected = true;
                                if (ServerConnected != null)
                                    ServerConnected();
                            }
                            else if(status == NetConnectionStatus.Disconnected)
                            {
                                isConnected = false;
                                if (ServerDisconnected != null)
                                    ServerDisconnected();
                            }
                            break;
                        case NetIncomingMessageType.Data:
                            totalLength += msg.LengthBytes;
                            if (messages.Length < totalLength)
                                ByteHelper.ExpandArray(ref messages, totalLength);
                            msg.ReadBytes(messages, totalLength - msg.LengthBytes, msg.LengthBytes);
                            break;
                    }
                }
            }
            catch (SocketException se)
            {
                Log.Write("Socket exception is thrown in ReceiveMessage: " + se.StackTrace);
            }

            return totalLength;
        }

        public void SendMessage(byte[] msg, bool reliable, bool inOrder)
        {
            // subsequent input; send chat message to server
            // create a message
            NetOutgoingMessage om = netClient.CreateMessage();
            om.Write(msg);
            if (enableEncryption)
                om.Encrypt(xtea);

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

            try
            {
                netClient.SendMessage(om, deliverMethod, sequenceChannel);
            }
            catch (SocketException se)
            {
                Log.Write("Socket exception is thrown in SendMessage: " + se.StackTrace);
            }
        }

        public void Shutdown()
        {
            shutDownForced = true;
            netClient.Disconnect("Disconnecting....");
            netClient.Shutdown("Client exitting");
        }

        #endregion
    }
}
