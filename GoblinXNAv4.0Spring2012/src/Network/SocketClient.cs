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
using System.Net.Sockets;
using System.Threading;
using System.Net;

using GoblinXNA.Helpers;

namespace GoblinXNA.Network
{
    public class SocketClient : IClient
    {
        #region Member Fields

        private const int MAX_BUFFER_SIZE = 1024 * 64;
        private const int INITIAL_BUFFER_SIZE = 1024 * 2;
        private Thread serverWaitThread;
        private double waitStartTime;
        private bool abortWait;

        private StateObject state;
        private Socket sendSocket;
        private Socket recvSocket;
        private int sendBufferSize;
        private int recvBufferSize;

        private byte[] myIPAddress;
        private bool isConnected;

        private String hostName;
        private int portNumber;
        private AddressFamily addressFamily;
        private SocketType socketType;
        private ProtocolType protocolType;

        private DnsEndPoint hostEntry;
        private SocketAsyncEventArgs sendSocketArg;

        private ManualResetEvent shutdownEvent;
        private ManualResetEvent sendEvent;
        private ManualResetEvent connectEvent;
        private bool shutDown;
        private bool isShuttingDown;
        private bool transmittingData;
        private bool copyingRecvBuffer;
        private bool receivingData;

        private List<byte[]> receivedDataList;

        #endregion

        #region Constructors

        public SocketClient(String hostName, int portNumber, AddressFamily addressFamily, SocketType socketType,
            ProtocolType protocolType)
        {
            this.hostName = hostName;
            this.portNumber = portNumber;
            this.addressFamily = addressFamily;
            this.socketType = socketType;
            this.protocolType = protocolType;

            ConnectionTrialTimeOut = 5000;
            ShutdownWaitTimeout = 5000;
            sendBufferSize = INITIAL_BUFFER_SIZE;
            recvBufferSize = INITIAL_BUFFER_SIZE;

            shutdownEvent = new ManualResetEvent(false);
            sendEvent = new ManualResetEvent(false);
            connectEvent = new ManualResetEvent(false);

            isConnected = false;
            EnableEncryption = false;
            WaitForServer = false;
            shutDown = false;
            copyingRecvBuffer = false;
            isShuttingDown = false;
            transmittingData = false;
            abortWait = false;
            receivingData = false;

            receivedDataList = new List<byte[]>();
        }

        /// <summary>
        /// Creates a UDP client.
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="portNumber"></param>
        public SocketClient(String hostName, int portNumber)
            : this(hostName, portNumber, AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
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

        public bool EnableEncryption
        {
            get;
            set;
        }

        public bool IsConnected
        {
            get { return isConnected; }
        }

        public bool WaitForServer
        {
            get;
            set;
        }

        public int ConnectionTrialTimeOut
        {
            get;
            set;
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

        public event HandleServerConnection ServerConnected;

        public event HandleServerDisconnection ServerDisconnected;

        #endregion

        #region Public Methods

        public void Connect()
        {
            if (isConnected)
                return;

            hostEntry = new DnsEndPoint(hostName, portNumber);
            sendSocket = new Socket(addressFamily, socketType, protocolType);

            sendSocketArg = new SocketAsyncEventArgs();

            sendSocketArg.RemoteEndPoint = hostEntry;
            sendSocketArg.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArg_Callback);
            sendSocketArg.UserToken = sendSocket;

            if (WaitForServer)
            {
                serverWaitThread = new Thread(new ThreadStart(WaitServer));
                waitStartTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                serverWaitThread.Start();
            }
            else
                sendSocket.ConnectAsync(sendSocketArg);
        }

        public void Shutdown()
        {
            if (!isConnected)
            {
                if (serverWaitThread != null && serverWaitThread.IsAlive)
                {
                    abortWait = true;
                    serverWaitThread.Join();
                }
                return;
            }

            isShuttingDown = true;
            while (transmittingData) { }

            recvSocket.Shutdown(SocketShutdown.Receive);

            byte[] shutdownMsg = Encoding.UTF8.GetBytes("@Shutdown@");

            state.ExpectedSendBytes = 4;
#if USE_VARIABLE_BUFFER_SIZE
            sendSocket.SendBufferSize = 4;
#endif
            sendEvent.Reset();
            sendSocketArg.SetBuffer(BitConverter.GetBytes(shutdownMsg.Length), 0, 4);
            sendSocket.SendAsync(sendSocketArg);
            sendEvent.WaitOne(100);

#if USE_VARIABLE_BUFFER_SIZE
            sendSocket.SendBufferSize = 16;
#endif
            sendSocketArg.SetBuffer(shutdownMsg, 0, shutdownMsg.Length);

            shutdownEvent.Reset();
            shutDown = true;
            sendSocket.SendAsync(sendSocketArg);
            shutdownEvent.WaitOne(ShutdownWaitTimeout);

            if (sendSocket.Connected)
            {
                sendSocket.Shutdown(SocketShutdown.Send);
                sendSocket.Close();
                isConnected = false;
            }

            recvSocket.Close();
        }

        public int ReceiveMessage(ref byte[] messages)
        {
            int totalLength = 0;

            if (isConnected && receivedDataList.Count > 0 && !isShuttingDown)
            {
                while (receivingData) { }
                copyingRecvBuffer = true;
                for (int i = 0; i < receivedDataList.Count; ++i)
                    totalLength += receivedDataList[i].Length;

                if(messages.Length < totalLength)
                    ByteHelper.ExpandArray(ref messages, totalLength);

                int index = 0;
                for (int i = 0; i < receivedDataList.Count; ++i)
                {
                    Buffer.BlockCopy(receivedDataList[i], 0, messages, index, receivedDataList[i].Length);
                    index += receivedDataList[i].Length;
                }
                receivedDataList.Clear();
                copyingRecvBuffer = false;
            }

            return totalLength;
        }

        public void SendMessage(byte[] msg, bool reliable, bool inOrder)
        {
            if (isShuttingDown)
                return;

            while (transmittingData) { }
            transmittingData = true;

            int bufferLength = ByteHelper.RoundToPowerOfTwo(msg.Length);

            // first, send the number of bytes it will transfer in the next send
            state.ExpectedSendBytes = 4;
#if USE_VARIABLE_BUFFER_SIZE
            sendSocket.SendBufferSize = 4;
#endif
            sendSocketArg.SetBuffer(BitConverter.GetBytes(msg.Length), 0, 4);

            sendEvent.Reset();
            sendSocket.SendAsync(sendSocketArg);
            sendEvent.WaitOne(100);

            // now we are ready to send the actual data
            state.ExpectedSendBytes = msg.Length;
#if USE_VARIABLE_BUFFER_SIZE
            sendSocket.SendBufferSize = sendBufferSize;
#endif
            if (bufferLength > sendSocket.SendBufferSize)
                sendSocket.SendBufferSize = bufferLength;
            sendSocketArg.SetBuffer(msg, 0, msg.Length);

            sendSocket.SendAsync(sendSocketArg);
        }

        #endregion

        #region Private Methods

        private void WaitServer()
        {
            while (!abortWait && !isConnected &&
                ((DateTime.Now.TimeOfDay.TotalMilliseconds - waitStartTime) < ConnectionTrialTimeOut))
            {
                connectEvent.Reset();

                sendSocket.ConnectAsync(sendSocketArg);

                connectEvent.WaitOne();
            }
        }

        private void SocketEventArg_Callback(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;

                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;

                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;

                default:
                    throw new Exception("Invalid operation completed");
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket connectedSocket = (Socket)e.UserToken;
                // if we just connected the send socket, now it's time to connect
                // the receive socket
                if (connectedSocket == sendSocket)
                {
                    sendSocket.SendBufferSize = sendBufferSize;

                    recvSocket = new Socket(addressFamily, socketType, protocolType);

                    SocketAsyncEventArgs recvSocketArg = new SocketAsyncEventArgs();

                    recvSocketArg.RemoteEndPoint = hostEntry;
                    recvSocketArg.Completed += new
                        EventHandler<SocketAsyncEventArgs>(SocketEventArg_Callback);
                    recvSocketArg.UserToken = recvSocket;

                    recvSocket.ConnectAsync(recvSocketArg);
                }
                // if we just connected the receive socket, then we can start receiving messages
                // from the server
                else
                {
                    isConnected = true;

                    state = new StateObject(recvBufferSize);

                    recvSocket.ReceiveBufferSize = recvBufferSize;
                    state.ExpectedReceiveBytes = state.PreReceiveBuffer.Length;
                    e.SetBuffer(state.PreReceiveBuffer, 0, state.PreReceiveBuffer.Length);
#if DEBUG_NETWORK
                    Log.Write("Waiting to receive data length");
#endif
                    recvSocket.ReceiveAsync(e);

                    if (ServerConnected != null)
                        ServerConnected();
                }
            }
            else
            {
                Log.Write("Failed to connect: " + e.SocketError.ToString());
            }

            if (WaitForServer)
                connectEvent.Set();
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (isShuttingDown)
                return;

            if (e.SocketError == SocketError.Success)
            {
#if DEBUG_NETWORK
                string debugData = "Received " + e.BytesTransferred + " byte. ";
#endif
                if (e.BytesTransferred == 4)
                {
                    state.ExpectedReceiveBytes = BitConverter.ToInt32(state.PreReceiveBuffer, 0);
                    int bufferSize = ByteHelper.RoundToPowerOfTwo(state.ExpectedReceiveBytes);

                    if (bufferSize > recvBufferSize)
                    {
                        bool change = false;
                        if (bufferSize > MAX_BUFFER_SIZE)
                        {
                            if (recvBufferSize != MAX_BUFFER_SIZE)
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

                        if (change)
                        {
                            recvSocket.ReceiveBufferSize = recvBufferSize;
                            state.ReceiveBuffer = new byte[recvBufferSize];
                        }
                    }

#if USE_VARIABLE_BUFFER_SIZE
                    recvSocket.ReceiveBufferSize = recvBufferSize;
#endif
                    int receivingBytes = (state.ExpectedReceiveBytes > recvBufferSize) ? recvBufferSize :
                        state.ExpectedReceiveBytes;
                    e.SetBuffer(state.ReceiveBuffer, 0, receivingBytes);

#if DEBUG_NETWORK
                    debugData += "Expected receiving bytes: " + receivingBytes + ". ";
                    Log.Write(debugData);
#endif
                    try
                    {
                        recvSocket.ReceiveAsync(e);
                    }
                    catch (Exception) { }
                }
                else
                {
                    if (e.BytesTransferred == 10)
                    {
                        string msg = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
                        if (msg.Equals("@Shutdown@"))
                        {
                            recvSocket.Shutdown(SocketShutdown.Receive);
                            recvSocket.Close();

                            sendSocket.Shutdown(SocketShutdown.Send);
                            sendSocket.Close();

                            isConnected = false;

                            if (ServerDisconnected != null)
                                ServerDisconnected();

                            return;
                        }
                    }

                    while (copyingRecvBuffer) { }
#if DEBUG_NETWORK
                    debugData += "Actual received data: ";
                    for (int i = 0; i < e.BytesTransferred; ++i)
                        debugData += e.Buffer[i] + " ";
                    Log.Write(debugData);
#endif
                    receivingData = true;
                    int newOffset = state.Offset + e.BytesTransferred;
                    if (newOffset > state.ReceivedData.Length)
                    {
                        byte[] tmp = new byte[state.Offset];
                        Buffer.BlockCopy(state.ReceivedData, 0, tmp, 0, state.Offset);
                        int newLength = ByteHelper.RoundToPowerOfTwo(newOffset);
                        state.ReceivedData = new byte[newLength];
                        Buffer.BlockCopy(tmp, 0, state.ReceivedData, 0, state.Offset);
                    }
                    Buffer.BlockCopy(e.Buffer, 0, state.ReceivedData, state.Offset, e.BytesTransferred);
                    state.Offset += e.BytesTransferred;
                    receivingData = false;

                    if (state.Offset >= state.ExpectedReceiveBytes)
                    {
                        state.Offset = 0;
                        receivedDataList.Add(ByteHelper.Truncate(state.ReceivedData, 0, 
                            state.ExpectedReceiveBytes));
#if USE_VARIABLE_BUFFER_SIZE
                        recvSocket.ReceiveBufferSize = 4;
#endif
                        state.ExpectedReceiveBytes = 4;
                        e.SetBuffer(state.PreReceiveBuffer, 0, 4);
#if DEBUG_NETWORK
                        Log.Write("Waiting to receive data length");
#endif
                        try
                        {
                            recvSocket.ReceiveAsync(e);
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        int receivingBytes = state.ExpectedReceiveBytes - state.Offset;
                        if (receivingBytes > recvBufferSize)
                            receivingBytes = recvBufferSize;
                        e.SetBuffer(state.ReceiveBuffer, 0, receivingBytes);
#if DEBUG_NETWORK
                        debugData += "Expected receiving bytes: " + receivingBytes + ". ";
                        Log.Write(debugData);
#endif
                        try
                        {
                            recvSocket.ReceiveAsync(e);
                        }
                        catch (Exception) { }
                    }
                }
            }
            else
            {
                Log.Write("Failed to receive: " + e.SocketError.ToString());
            }
        }

        // Called when a SendAsync operation completes 
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (shutDown)
                {
                    if (sendSocket.Connected)
                    {
                        sendSocket.Shutdown(SocketShutdown.Send);
                        sendSocket.Close();
                    }

                    shutdownEvent.Set();
                }
                else
                {
                    if(e.BytesTransferred != state.ExpectedSendBytes)
                        Log.Write("Could not send all expected bytes. Expected: " + 
                            state.ExpectedSendBytes + ", Received: " + e.BytesTransferred);

                    sendEvent.Set();

                    if (state.ExpectedSendBytes != 4)
                        transmittingData = false;
                }
            }
            else
            {
                Log.Write("Failed to send: " + e.SocketError.ToString());
                /*if (isConnected && e.SocketError == SocketError.ConnectionReset)
                {
                    isShuttingDown = true;
                    recvSocket.Shutdown(SocketShutdown.Receive);
                    recvSocket.Close();

                    sendSocket.Shutdown(SocketShutdown.Send);
                    sendSocket.Close();
                    isConnected = false;

                    if (ServerDisconnected != null)
                        ServerDisconnected();
                }*/
            }
        }

        #endregion

        #region Public Class

        private class StateObject
        {
            public byte[] PreReceiveBuffer;

            public byte[] ReceiveBuffer;

            public byte[] ReceivedData;

            public int ExpectedReceiveBytes;

            public int ExpectedSendBytes;

            public int Offset;

            public StateObject(int bufferSize)
            {
                ReceiveBuffer = new byte[bufferSize];
                ReceivedData = new byte[bufferSize];
                PreReceiveBuffer = new byte[4];
            }
        }

        #endregion
    }
}
