using System;
using System.Net;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

namespace Quoridor.Networking
{
    /// <summary>
    /// LAN discovery adapter that advertises Quoridor room metadata.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuoridorLanDiscovery : NetworkDiscoveryBase<QuoridorDiscoveryRequest, QuoridorDiscoveryResponse>
    {
        [SerializeField] private QuoridorNetworkManager networkManager;

        /// <summary>
        /// Raised when a LAN room is discovered or refreshed.
        /// </summary>
        public event Action<QuoridorLanRoomInfo> RoomFound;

        /// <summary>
        /// Starts LAN discovery from the menu.
        /// </summary>
        public void SearchForRooms()
        {
            StartDiscovery();
        }

        /// <summary>
        /// Stops LAN discovery polling.
        /// </summary>
        public void StopSearching()
        {
            StopDiscovery();
        }

        protected override QuoridorDiscoveryResponse ProcessRequest(QuoridorDiscoveryRequest request, IPEndPoint endpoint)
        {
            QuoridorNetworkManager manager = ResolveNetworkManager();
            return new QuoridorDiscoveryResponse
            {
                ServerId = ServerId,
                Uri = transport.ServerUri(),
                RoomName = manager != null ? manager.RoomName : "Quoridor Room",
                PlayerCount = manager != null ? manager.PlayerCount : 1,
                MaxPlayers = manager != null ? manager.MaxPlayers : 2
            };
        }

        protected override QuoridorDiscoveryRequest GetRequest()
        {
            return new QuoridorDiscoveryRequest();
        }

        protected override void ProcessResponse(QuoridorDiscoveryResponse response, IPEndPoint endpoint)
        {
            UriBuilder uriBuilder = new(response.Uri)
            {
                Host = endpoint.Address.ToString()
            };

            QuoridorLanRoomInfo roomInfo = new(
                response.ServerId,
                uriBuilder.Uri,
                response.RoomName,
                response.PlayerCount,
                response.MaxPlayers);
            RoomFound?.Invoke(roomInfo);
        }

        private QuoridorNetworkManager ResolveNetworkManager()
        {
            if (networkManager == null)
            {
                networkManager = GetComponent<QuoridorNetworkManager>();
            }

            return networkManager;
        }
    }
}
