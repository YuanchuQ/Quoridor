// Client discovery probe for Quoridor LAN rooms
using System;
using Mirror;

namespace Quoridor.Networking
{
    /// <summary>
    /// Client discovery probe for Quoridor LAN rooms.
    /// </summary>
    public struct QuoridorDiscoveryRequest : NetworkMessage
    {
    }

    /// <summary>
    /// Host discovery response with room metadata.
    /// </summary>
    public struct QuoridorDiscoveryResponse : NetworkMessage
    {
        /// <summary>
        /// Mirror discovery server id.
        /// </summary>
        public long ServerId;

        /// <summary>
        /// URI used by the client to connect to the host.
        /// </summary>
        public Uri Uri;

        /// <summary>
        /// Human-readable room name.
        /// </summary>
        public string RoomName;

        /// <summary>
        /// Current connected player count.
        /// </summary>
        public int PlayerCount;

        /// <summary>
        /// Maximum player count.
        /// </summary>
        public int MaxPlayers;
    }
}
