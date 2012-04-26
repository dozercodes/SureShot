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
using System.Linq;
using System.Text;

using GoblinXNA.Helpers;

namespace GoblinXNA.Network
{
    public class SocketNetworkHandler : INetworkHandler
    {
        #region Member Fields

        protected const int INITIAL_BUFFER_SIZE = 8192;

        protected IServer socketServer;
        protected IClient socketClient;
        protected TransferSize transferSize;
        protected bool started;
        protected int networkObjSize;
        protected int maxNetworkObjSize;

        protected byte[] receivedBytes;

        protected Dictionary<String, NetObj> networkObjects;

        protected bool updating;

        #endregion

        #region Constructors

        public SocketNetworkHandler()
        {
            networkObjects = new Dictionary<string, NetObj>();
            updating = false;

            receivedBytes = new byte[INITIAL_BUFFER_SIZE];

            transferSize = TransferSize.UShort;
            networkObjSize = sizeof(ushort);
            switch (transferSize)
            {
                case TransferSize.Byte:
                    maxNetworkObjSize = byte.MaxValue;
                    break;
                case TransferSize.UShort:
                    maxNetworkObjSize = ushort.MaxValue;
                    break;
                case TransferSize.Int:
                    maxNetworkObjSize = int.MaxValue;
                    break;
            }
            started = false;
        }

        #endregion

        #region Properties

        public virtual IServer NetworkServer
        {
            get { return socketServer; }
            set
            {
                if (socketServer != null)
                    socketServer.Shutdown();

                socketServer = value;
                socketServer.Initialize();
            }
        }

        public virtual IClient NetworkClient
        {
            get { return socketClient; }
            set
            {
                if (socketClient != null)
                    socketClient.Shutdown();

                socketClient = value;
                socketClient.Connect();
            }
        }

        public virtual TransferSize TransferSizePerNetworkObject
        {
            get { return transferSize; }
            set
            {
                if (started)
                    throw new GoblinException("You can not modify this property after the network " +
                        "transfer starts. Change this property in your Initialize() function");

                transferSize = value;
                networkObjSize = (int)transferSize;
                switch (transferSize)
                {
                    case TransferSize.Byte:
                        maxNetworkObjSize = byte.MaxValue;
                        break;
                    case TransferSize.UShort:
                        maxNetworkObjSize = ushort.MaxValue;
                        break;
                    case TransferSize.Int:
                        maxNetworkObjSize = int.MaxValue;
                        break;
                }
            }
        }

        #endregion

        #region Public Methods
        
        public virtual void AddNetworkObject(INetworkObject networkObj)
        {
            // busy wait while the network handler is being updated
            while (updating) { }
            if (!networkObjects.ContainsKey(networkObj.Identifier))
            {
                if (networkObj.Identifier.Length > 255)
                    throw new GoblinException("A network object's Identifier should not exceed 255 " +
                        "characters long");
                networkObjects.Add(networkObj.Identifier, new NetObj(networkObj));
            }
        }

        public virtual void RemoveNetworkObject(INetworkObject networkObj)
        {
            // busy wait while the network handler is being updated
            while (updating) { }
            networkObjects.Remove(networkObj.Identifier);
        }

        public virtual void Dispose()
        {
            if (socketServer != null)
                socketServer.Shutdown();
            if (socketClient != null)
                socketClient.Shutdown();

            networkObjects.Clear();
            receivedBytes = null;
        }

        public virtual void Update(float elapsedMsecs)
        {
            started = true;
            int receivedLength = 0;

            if (State.IsServer)
                receivedLength = socketServer.ReceiveMessage(ref receivedBytes);
            else
                receivedLength = socketClient.ReceiveMessage(ref receivedBytes);

            String identifier = "";
            int size = 0;
            int idLength = 0;
            int index = 0;
            int dataLength = 0;

            while (index < receivedLength)
            {
                switch (transferSize)
                {
                    case TransferSize.Byte:
                        size = (int)receivedBytes[index];
                        break;
                    case TransferSize.UShort:
                        size = (int)BitConverter.ToUInt16(receivedBytes, index);
                        break;
                    case TransferSize.Int:
                        size = ByteHelper.ConvertToInt(receivedBytes, index);
                        break;
                }

                index += networkObjSize;
                idLength = (int)receivedBytes[index++];
                identifier = ByteHelper.ConvertToString(receivedBytes, index, idLength);
                index += idLength;
                dataLength = size - (idLength + 1);

                if (networkObjects.ContainsKey(identifier) && dataLength > 0)
                    networkObjects[identifier].NetworkObject.InterpretMessage(receivedBytes, index, dataLength);
                else
                    Log.Write("Network Identifier: " + identifier + " is not found", Log.LogLevel.Log);

                index += dataLength;
            }

            updating = true;

            foreach (NetObj netObj in networkObjects.Values)
                if (!netObj.NetworkObject.Hold)
                    netObj.TimeElapsedSinceLastTransmit += elapsedMsecs;

            List<byte> msgs = new List<byte>();

            if (State.IsServer)
            {
                if (socketServer.NumConnectedClients >= State.NumberOfClientsToWait)
                {
                    foreach (NetObj netObj in networkObjects.Values)
                        if (!netObj.NetworkObject.Hold &&
                            (netObj.NetworkObject.ReadyToSend || netObj.IsTimeToTransmit))
                        {
                            AddNetMessage(msgs, netObj.NetworkObject);

                            netObj.NetworkObject.ReadyToSend = false;
                            netObj.TimeElapsedSinceLastTransmit = 0;
                        }
                }

                if (msgs.Count > 0)
                {
                    socketServer.BroadcastMessage(msgs.ToArray(), true, true, false);
                }
            }
            else
            {
                if (socketClient.IsConnected)
                {
                    foreach (NetObj netObj in networkObjects.Values)
                        if (!netObj.NetworkObject.Hold &&
                            (netObj.NetworkObject.ReadyToSend || netObj.IsTimeToTransmit))
                        {
                            AddNetMessage(msgs, netObj.NetworkObject);

                            netObj.NetworkObject.ReadyToSend = false;
                            netObj.TimeElapsedSinceLastTransmit = 0;
                        }

                    if (msgs.Count > 0)
                        socketClient.SendMessage(msgs.ToArray(), true, true);
                }
            }

            msgs.Clear();
            updating = false;
        }

        #endregion

        #region Protected Methods

        protected virtual void AddNetMessage(List<byte> msgs, INetworkObject networkObj)
        {
            byte[] id = ByteHelper.ConvertToByte(networkObj.Identifier);
            byte[] data = networkObj.GetMessage();
            int totalSize = id.Length + data.Length + 1;
            if (totalSize > maxNetworkObjSize)
                throw new GoblinException(networkObj.Identifier + " contains data larger than the current " +
                    "transfer size of " + maxNetworkObjSize + ". Change the TransferSize to a larger size.");

            byte[] size = null;

            switch (transferSize)
            {
                case TransferSize.Byte:
                    size = new byte[1];
                    size[0] = (byte)totalSize;
                    break;
                case TransferSize.UShort:
                    size = BitConverter.GetBytes((ushort)totalSize);
                    break;
                case TransferSize.Int:
                    size = BitConverter.GetBytes(totalSize);
                    break;
            }

            msgs.AddRange(size);
            msgs.Add((byte)networkObj.Identifier.Length);
            msgs.AddRange(id);
            msgs.AddRange(data);
        }

        #endregion

        #region Protected Classes
        protected class NetObj
        {
            private INetworkObject networkObject;
            private float timeElapsedSinceLastTransmit;
            private float transmitSpan;

            public NetObj(INetworkObject netObj)
            {
                this.networkObject = netObj;
                timeElapsedSinceLastTransmit = 0;
                if (networkObject.SendFrequencyInHertz != 0)
                    transmitSpan = 1000 / (float)networkObject.SendFrequencyInHertz;
                else
                    transmitSpan = float.MaxValue;
            }

            public INetworkObject NetworkObject
            {
                get { return networkObject; }
            }

            public float TimeElapsedSinceLastTransmit
            {
                get { return timeElapsedSinceLastTransmit; }
                set { timeElapsedSinceLastTransmit = value; }
            }

            public bool IsTimeToTransmit
            {
                get { return (timeElapsedSinceLastTransmit >= transmitSpan); }
            }
        }
        #endregion
    }
}
