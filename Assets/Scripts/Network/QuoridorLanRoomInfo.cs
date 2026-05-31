// Immutable LAN room data shown in the room list
using System;

namespace Quoridor.Networking
{
    /// <summary>
    /// Immutable LAN room data shown in the room list.
    /// </summary>
    public readonly struct QuoridorLanRoomInfo
    {
        /// <summary>
        /// Creates one LAN room row from discovery data.
        /// </summary>
        public QuoridorLanRoomInfo(long serverId, Uri uri, string roomName, int playerCount, int maxPlayers)
        {
            ServerId = serverId;
            Uri = uri;
            RoomName = string.IsNullOrWhiteSpace(roomName) ? "Quoridor Room" : roomName;
            PlayerCount = playerCount;
            MaxPlayers = maxPlayers;
        }

        /// <summary>
        /// Stable Mirror discovery server id.
        /// </summary>
        public long ServerId { get; }

        /// <summary>
        /// Connection URI reported by the host transport.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Display name for the advertised room.
        /// </summary>
        public string RoomName { get; }

        /// <summary>
        /// Current number of connected players.
        /// </summary>
        public int PlayerCount { get; }

        /// <summary>
        /// Maximum supported players for this demo.
        /// </summary>
        public int MaxPlayers { get; }

        /// <summary>
        /// User-facing list label.
        /// </summary>
        public string DisplayLabel => $"{RoomName}    {PlayerCount}/{MaxPlayers}";
    }
}
