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
//#define DEBUG_NETWORK
//#define USE_VARIABLE_BUFFER_SIZE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using GoblinXNA.Helpers;

namespace GoblinXNA.Network
{
    public class SocketServer : IServer
    {
        #region Member Fields

        private const int MAX_BUFFER_SIZE = 1024 * 64;
        private const int INITIAL_BUFFER_SIZE = 1024 * 2;
        private Socket listener;
        private int sendBufferSize;
        private int recvBufferSize;
        private Thread listenThread;
        private List<string> clientAddresses;
        private Dictionary<string, StateObject> clientTable;
        private List<string> removeList;

        private int portNumber;
        private byte[] myIPAddress;
        private bool enableEncryption;

        private AddressFamily addressFamily;
        private SocketType socketType;
        private ProtocolType protocolType;

        private ManualResetEvent listenEvent;
        private ManualResetEvent shutDownEvent;
        private ManualResetEvent sendEvent;
        private bool copyingRecvBuffer;
        private bool isShuttingDown;
        private bool transmittingData;
        private bool receivingData;

        private bool initialized;

        private List<StateObject> recentlyReceivedStates;

        #endregion

        #region Constructors

        public SocketServer(int portNumber, AddressFamily addressFamily, SocketType socketType,
            ProtocolType protocolType)
        {
            this.portNumber = portNumber;
            this.addressFamily = addressFamily;
            this.socketType = socketType;
            this.protocolType = protocolType;

            clientAddresses = new List<string>();
            clientTable = new Dictionary<string, StateObject>();
            recentlyReceivedStates = new List<StateObject>();
            removeList = new List<string>();

            listenEvent = new ManualResetEvent(false);
            shutDownEvent = new ManualResetEvent(false);
            sendEvent = new ManualResetEvent(false);

            listener = null;
            sendBufferSize = INITIAL_BUFFER_SIZE;
            recvBufferSize = INITIAL_BUFFER_SIZE;
            ShutdownWaitTimeout = 5000;

            enableEncryption = false;
            initialized = false;
            copyingRecvBuffer = false;
            isShuttingDown = false;
            transmittingData = false;
            receivingData = false;
        }

        public SocketServer(int portNumber)
            : this(portNumber, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
        }

        #endregion

        #region Properties

        public int PortNumber
        {
            get { return portNumber; }
            set { portNumber = value; }
        }

        public byte[] MyIPAddress
        {
            get { return myIPAddress; }
        }

        public int NumConnectedClients
        {
            get { return clientAddresses.Count; }
        }

        public List<string> ClientIPAddresses
        {
            get { return clientAddresses; }
        }

        public bool EnableEncryption
        {
            get { return enableEncryption; }
            set { enableEncryption = value; }
        }

        /// <summary>
        /// Gets or sets the maximum wait timeout in milliseconds for shutting down each client socket.
        /// Default timeout is 5000 milliseconds.
        /// </summary>
        public int ShutdownWaitTimeout
        {
            get;
            set;
        }

        #endregion

        #region Events

        public event HandleClientConnection ClientConnected;

        public event HandleClientDisconnection ClientDisconnected;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, portNumber);
            listener = new Socket(addressFamily, socketType, protocolType);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                listenThread = new Thread(new ThreadStart(KeepListening));
                listenThread.Start();

                initialized = true;
            }
            catch (Exception e)
            {
                Log.Write(e.Message);
            }
        }

        public void BroadcastMessage(byte[] msg, bool reliable, bool inOrder, bool excludeSender)
        {
            if (!initialized || clientTable.Count == 0 || isShuttingDown)
                return;

            int bufferLength = ByteHelper.RoundToPowerOfTwo(msg.Length);

            try
            {
                foreach (StateObject client in clientTable.Values)
                {
                    if (client.SendSocket != null)
                    {
                        Send(client, msg, bufferLength);
                    }
                }
            }
            catch (Exception) { }

            if(removeList.Count > 0)
            {
                foreach(string ip in removeList)
                {
                    clientTable.Remove(ip);
                    clientAddresses.Remove(ip);
                }

                removeList.Clear();
            }
        }

        public void SendMessage(byte[] msg, List<string> ipAddresses, bool reliable, bool inOrder)
        {
            if (!initialized || clientTable.Count == 0 || isShuttingDown)
                return;

            int bufferLength = ByteHelper.RoundToPowerOfTwo(msg.Length);

            StateObject client;
            foreach (string ipAddress in ipAddresses)
            {
                if (clientTable.ContainsKey(ipAddress))
                {
                    client = clientTable[ipAddress];
                    if (client.SendSocket != null)
                    {
                        Send(client, msg, bufferLength);
                    }
                }
            }

            if (removeList.Count > 0)
            {
                foreach (string ip in removeList)
                {
                    clientTable.Remove(ip);
                    clientAddresses.Remove(ip);
                }

                removeList.Clear();
            }
        }

        public int ReceiveMessage(ref byte[] messages)
        {
            if (!initialized || recentlyReceivedStates.Count == 0)
                return 0;

            while (receivingData) { }
            copyingRecvBuffer = true;

            int totalLength = 0;
            foreach (StateObject state in recentlyReceivedStates)
            {
                totalLength += state.Offset;
                if (messages.Length < totalLength)
                    ByteHelper.ExpandArray(ref messages, totalLength);
                Buffer.BlockCopy(state.ReceivedData, 0, messages, totalLength - state.Offset, state.Offset);
                state.Offset = 0;
            }

            recentlyReceivedStates.Clear();
            copyingRecvBuffer = false;

            return totalLength;
        }

        public void Shutdown()
        {
            if (listenThread != null && listenThread.IsAlive)
                listenThread.Abort();

            isShuttingDown = true;
            while (transmittingData) { }

            byte[] shutdownMsg = Encoding.UTF8.GetBytes("@Shutdown@");
            foreach (StateObject client in clientTable.Values)
            {

                try
                {
                    client.ReceiveSocket.Shutdown(SocketShutdown.Receive);

                    if (client.SendSocket != null && client.SendSocket.Connected)
                    {
                        client.ExpectedSendBytes = 4;
#if USE_VARIABLE_BUFFER_SIZE
                    client.SendSocket.SendBufferSize = 4;
#endif
                        sendEvent.Reset();
                        client.SendSocket.BeginSend(BitConverter.GetBytes(shutdownMsg.Length), 0, 4, 0,
                            new AsyncCallback(SendCallback), client);
                        sendEvent.WaitOne(100);

                        client.ExpectedSendBytes = shutdownMsg.Length;
#if USE_VARIABLE_BUFFER_SIZE
                    client.SendSocket.SendBufferSize = 16;
#endif

                        shutDownEvent.Reset();

                        client.SendSocket.BeginSend(shutdownMsg, 0, shutdownMsg.Length, 0,
                            new AsyncCallback(ShutdownCallback), client);

                        shutDownEvent.WaitOne(ShutdownWaitTimeout);

                        // if the send socket is still connected after the wait timeout, then
                        // it means the client was disconnected unexpectedly
                        if (client.SendSocket.Connected)
                        {
                            client.SendSocket.Shutdown(SocketShutdown.Send);
                            client.SendSocket.Close();
                        }
                    }
                }
                catch (Exception exp)
                {
                    client.SendSocket.Close();
                }
                finally
                {
                    client.ReceiveSocket.Close();
                }
            }

            if (listener != null && listener.Connected)
            {
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }

            initialized = false;
        }

        #endregion

        #region Private Methods

        private void Send(StateObject client, byte[] msg, int bufferLength)
        {
            while (transmittingData) { }
            transmittingData = true;

            // first, send the number of bytes it will transfer in the next send
            client.ExpectedSendBytes = 4;
#if USE_VARIABLE_BUFFER_SIZE
            client.SendSocket.SendBufferSize = 4;
#endif
            sendEvent.Reset();
            try
            {
#if DEBUG_NETWORK
                Log.Write("Sending length: " + msg.Length);
#endif
                client.SendSocket.BeginSend(BitConverter.GetBytes(msg.Length), 0, 4, 0,
                    new AsyncCallback(SendCallback), client);
            }
            catch (SocketException exp)
            {
                DisconnectClient(client, true);
                sendEvent.Set();
                return;
            }
            sendEvent.WaitOne(100);

            if (bufferLength > sendBufferSize)
            {
                sendBufferSize = bufferLength;
                client.SendSocket.SendBufferSize = sendBufferSize;
            }

#if USE_VARIABLE_BUFFER_SIZE
            client.SendSocket.SendBufferSize = sendBufferSize;
#endif
            // now we are ready to send the actual data
            client.ExpectedSendBytes = msg.Length;

            try
            {
#if DEBUG_NETWORK
                string debugData = "Sending actual data: ";
                foreach (byte b in msg)
                    debugData += b + " ";
                Log.Write(debugData);
#endif
                client.SendSocket.BeginSend(msg, 0, msg.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }
            catch (SocketException exp)
            {
                DisconnectClient(client, true);
                sendEvent.Set();
                return;
            }

        }

        private void KeepListening()
        {
            try
            {
                while (true)
                {
                    // Set the event to nonsignaled state.
                    listenEvent.Reset();

                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.
                    listenEvent.WaitOne();
                }
            }
            catch (Exception e)
            {
                Log.Write(e.Message);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            listenEvent.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            string clientKey = handler.RemoteEndPoint.ToString();
            string[] tmp = clientKey.Split('{', ':', '}');
            string clientIP = tmp[0];
            string port = tmp[1];

            if (clientTable.ContainsKey(clientIP))
            {
                handler.SendBufferSize = sendBufferSize;
                clientTable[clientIP].SendSocket = handler;
            }
            else
            {
                // Create the state object.
                StateObject state = new StateObject(recvBufferSize);
                state.IPAddress = clientIP;
                state.PortNumber = int.Parse(port);
                state.ReceiveSocket = handler;

                clientTable.Add(clientIP, state);
                clientAddresses.Add(clientIP);

                state.ReceiveSocket.ReceiveBufferSize = recvBufferSize;
                state.ExpectedReceiveBytes = 4;
#if USE_VARIABLE_BUFFER_SIZE
                state.ReceiveSocket.ReceiveBufferSize = 4;
#endif
                handler.BeginReceive(state.PreReceiveBuffer, 0, state.PreReceiveBuffer.Length, 0,
                    new AsyncCallback(ReadCallback), state);

                if (ClientConnected != null)
                    ClientConnected(clientIP, state.PortNumber);
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.ReceiveSocket;

            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (Exception) { return; }

            /*if (bytesRead != state.ExpectedReceiveBytes)
                Log.Write("Did not receive all expected bytes. Expected: " + state.ExpectedReceiveBytes +
                    ", Received: " + bytesRead);*/

            if (bytesRead == 4)
            {
                state.ExpectedReceiveBytes = BitConverter.ToInt32(state.PreReceiveBuffer, 0);
                int bufferSize = ByteHelper.RoundToPowerOfTwo(state.ExpectedReceiveBytes);

                if (bufferSize > recvBufferSize)
                {
                    bool change = false;
                    if(bufferSize > MAX_BUFFER_SIZE)
                    {
                        if(recvBufferSize != MAX_BUFFER_SIZE)
                        {
                            recvBufferSize = MAX_BUFFER_SIZE;
                            change = true;
                        }
                    }
                    else
                    {
                        recvBufferSize = bufferSize;
                        change = true;
                    }

                    if(change)
                    {
                        state.ReceiveSocket.ReceiveBufferSize = recvBufferSize;
                        state.ReceiveBuffer = new byte[recvBufferSize];
                    }
                }

#if USE_VARIABLE_BUFFER_SIZE
                state.ReceiveSocket.ReceiveBufferSize = recvBufferSize;
#endif
                int receivingBytes = (state.ExpectedReceiveBytes > recvBufferSize) ? recvBufferSize :
                    state.ExpectedReceiveBytes;
                try
                {
                    handler.BeginReceive(state.ReceiveBuffer, 0, receivingBytes, 0,
                        new AsyncCallback(ReadCallback), state);
                }
                catch (Exception exp) { }
            }
            else
            {
                if (bytesRead == 10)
                {
                    string msg = Encoding.UTF8.GetString(state.ReceiveBuffer, 0, bytesRead);
                    if (msg.Equals("@Shutdown@"))
                    {
                        DisconnectClient(state, false);

                        return;
                    }
                }

                // busy wait until the ReceiveMessage call finishes
                while (copyingRecvBuffer) { }
                receivingData = true;

                int newOffset = state.Offset + bytesRead;
                if (newOffset > state.ReceivedData.Length)
                {
                    byte[] tmp = new byte[state.Offset];
                    Buffer.BlockCopy(state.ReceivedData, 0, tmp, 0, state.Offset);
                    int newLength = ByteHelper.RoundToPowerOfTwo(newOffset);
                    state.ReceivedData = new byte[newLength];
                    Buffer.BlockCopy(tmp, 0, state.ReceivedData, 0, state.Offset);
                }
                Buffer.BlockCopy(state.ReceiveBuffer, 0, state.ReceivedData, state.Offset, bytesRead);
                state.Offset += bytesRead;

                if(!recentlyReceivedStates.Contains(state) && state.Offset >= state.ExpectedReceiveBytes)
                    recentlyReceivedStates.Add(state);
                receivingData = false;

                if (state.Offset >= state.ExpectedReceiveBytes)
                {
#if USE_VARIABLE_BUFFER_SIZE
                    state.ReceiveSocket.ReceiveBufferSize = 4;
#endif
                    state.ExpectedReceiveBytes = 4;
                    try
                    {
                        handler.BeginReceive(state.PreReceiveBuffer, 0, state.PreReceiveBuffer.Length, 0,
                            new AsyncCallback(ReadCallback), state);
                    }
                    catch (Exception exp) { }
                }
                else
                {
                    int receivingBytes = state.ExpectedReceiveBytes - state.Offset;
                    if (receivingBytes > recvBufferSize)
                        receivingBytes = recvBufferSize;
                    try
                    {
                        handler.BeginReceive(state.ReceiveBuffer, 0, receivingBytes, 0,
                            new AsyncCallback(ReadCallback), state);
                    }
                    catch (Exception exp) { }
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.SendSocket;

                // Complete sending the data to the remote device.
                int bytesSent = 0;

                try
                {
                    bytesSent = handler.EndSend(ar);
                }
                catch (Exception) { return; }

#if DEBUG_NETWORK
                Log.Write("Sent " + bytesSent + " bytes");
#endif

                if(bytesSent != state.ExpectedSendBytes)
                    Log.Write("Could not send all expected bytes. Expected: " + state.ExpectedSendBytes +
                        ", Received: " + bytesSent);

                sendEvent.Set();

                if (state.ExpectedSendBytes != 4)
                    transmittingData = false;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString());
            }
        }

        private void ShutdownCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.SendSocket;

            handler.EndSend(ar);

            if (handler.Connected)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }

            shutDownEvent.Set();
        }

        private void DisconnectClient(StateObject client, bool remoteListLater)
        {
            if (!client.ReceiveSocket.Connected)
                return;

            client.ReceiveSocket.Shutdown(SocketShutdown.Receive);
            client.ReceiveSocket.Close();
            if (client.SendSocket != null && client.SendSocket.Connected)
            {
                client.SendSocket.Shutdown(SocketShutdown.Send);
                client.SendSocket.Close();
            }

            if (remoteListLater)
            {
                removeList.Add(client.IPAddress);
            }
            else
            {
                clientAddresses.Remove(client.IPAddress);
                clientTable.Remove(client.IPAddress);
            }

            if (ClientDisconnected != null)
                ClientDisconnected(client.IPAddress, client.PortNumber);
        }

        #endregion

        #region Private Class

        private class StateObject
        {
            // Client socket for receiving data
            public Socket ReceiveSocket;
            // Client socket for sending data
            public Socket SendSocket;

            public string IPAddress;

            public int PortNumber;

            public byte[] PreReceiveBuffer;

            public byte[] ReceiveBuffer;

            public byte[] ReceivedData;

            public int ExpectedSendBytes;

            public int ExpectedReceiveBytes;

            public int Offset;

            public StateObject(int bufferSize)
            {
                ReceiveBuffer = new byte[bufferSize];
                ReceivedData = new byte[bufferSize];
                PreReceiveBuffer = new byte[4];
                Offset = 0;
            }
        }

        #endregion
    }
}
