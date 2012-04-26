using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoblinXNA.Network
{
    public enum TransferSize { Byte = 1, UShort = 2, Int = 4 };

    /// <summary>
    /// An interface that defines how messages will be transferred over the network.
    /// </summary>
    public interface INetworkHandler
    {
        /// <summary>
        /// Gets or sets the specific network client implementation.
        /// </summary>
        IClient NetworkClient { get; set; }

        /// <summary>
        /// Gets or sets the specific network server implementation.
        /// </summary>
        IServer NetworkServer { get; set; }

        /// <summary>
        /// Gets or sets the size to use when transfering each INetworkObject. For example, if you
        /// are going to transfer INetworkObjects that will contain less than than 256 bytes including
        /// its Identifier length, then TransferSize.Byte would be good enough. The default size is
        /// TransferSize.Short.
        /// </summary>
        TransferSize TransferSizePerNetworkObject { get; set; }

        /// <summary>
        /// Adds a network object to send or receive messages associated with the
        /// object over the network.
        /// </summary>
        /// <param name="networkObj">A network object to be transfered over the network</param>
        void AddNetworkObject(INetworkObject networkObj);

        /// <summary>
        /// Removes a network object.
        /// </summary>
        /// <param name="networkObj">A network object to be transfered over the network</param>
        void RemoveNetworkObject(INetworkObject networkObj);

        /// <summary>
        /// Disposes the network objects.
        /// </summary>
        void Dispose();

        /// <summary>
        /// Retrieves and broadcasts messages over the network.
        /// </summary>
        /// <param name="elapsedMsecs"></param>
        void Update(float elapsedMsecs);
    }
}
