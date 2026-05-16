using System;
using System.Collections.Generic;
using Mirror;
using Quoridor.Config;
using Quoridor.Core;
using UnityEngine;

namespace Quoridor.Networking
{
    /// <summary>
    /// Mirror network manager for local LAN room creation, joining, and match scene transition.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuoridorNetworkManager : NetworkManager
    {
        [SerializeField] private string roomName = "Princess Room";
        [SerializeField] private string matchSceneName = "QuoridorDemo";
        [SerializeField] private QuoridorLanDiscovery discovery;

        private readonly List<QuoridorRoomPlayer> roomPlayers = new();
        private string matchPlayerOneCharacterId = string.Empty;
        private string matchPlayerTwoCharacterId = string.Empty;

        /// <summary>
        /// Raised when connection or room player state changes.
        /// </summary>
        public event Action RoomStateChanged;

        /// <summary>
        /// Room name advertised on LAN.
        /// </summary>
        public string RoomName => string.IsNullOrWhiteSpace(roomName) ? "Quoridor Room" : roomName;

        /// <summary>
        /// Current player count known by the host.
        /// </summary>
        public int PlayerCount => roomPlayers.Count;

        /// <summary>
        /// Maximum player count for local multiplayer.
        /// </summary>
        public int MaxPlayers => maxConnections;

        /// <summary>
        /// True when this process is hosting the room.
        /// </summary>
        public bool IsHostActive => mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly;

        /// <summary>
        /// True when the client has a live connection.
        /// </summary>
        public bool IsClientConnected => NetworkClient.isConnected;

        /// <summary>
        /// True when the host has enough players to start.
        /// </summary>
        public bool CanStartMatch => IsHostActive && roomPlayers.Count >= 2;

        /// <summary>
        /// Creates and advertises a LAN host room.
        /// </summary>
        public void StartLanHost(string selectedCharacterId)
        {
            CacheLocalSelection(selectedCharacterId);
            EnsureDiscovery();
            StartHost();
            discovery.AdvertiseServer();
            NotifyRoomStateChanged();
        }

        /// <summary>
        /// Starts searching for LAN rooms.
        /// </summary>
        public void SearchLanRooms(Action<QuoridorLanRoomInfo> onRoomFound)
        {
            EnsureDiscovery();
            discovery.RoomFound -= onRoomFound;
            discovery.RoomFound += onRoomFound;
            discovery.SearchForRooms();
        }

        /// <summary>
        /// Stops LAN room discovery.
        /// </summary>
        public void StopLanSearch(Action<QuoridorLanRoomInfo> onRoomFound)
        {
            if (discovery == null)
            {
                return;
            }

            discovery.RoomFound -= onRoomFound;
            discovery.StopSearching();
        }

        /// <summary>
        /// Joins a LAN room from discovery data.
        /// </summary>
        public void JoinLanRoom(QuoridorLanRoomInfo roomInfo, string selectedCharacterId)
        {
            CacheLocalSelection(selectedCharacterId);
            networkAddress = roomInfo.Uri.Host;
            StartClient(roomInfo.Uri);
            NotifyRoomStateChanged();
        }

        /// <summary>
        /// Stops the current host or client session and returns to offline mode.
        /// </summary>
        public void StopLanSession()
        {
            QuoridorRoomPlayer.ClearLocalPlayerSlot();

            if (NetworkServer.active && NetworkClient.isConnected)
            {
                StopHost();
            }
            else if (NetworkClient.active)
            {
                StopClient();
            }
            else if (NetworkServer.active)
            {
                StopServer();
            }
        }

        /// <summary>
        /// Host-only match start.
        /// </summary>
        public void StartNetworkMatch()
        {
            if (!CanStartMatch)
            {
                return;
            }

            ApplyNetworkCharacterSelection();
            ServerChangeScene(matchSceneName);
        }

        /// <summary>
        /// Clears room player tracking when the server starts.
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();
            roomPlayers.Clear();
            NetworkServer.ReplaceHandler<MatchConfigRequestMessage>(HandleMatchConfigRequest);
            NotifyRoomStateChanged();
        }

        /// <summary>
        /// Clears room player tracking when the server stops.
        /// </summary>
        public override void OnStopServer()
        {
            base.OnStopServer();
            roomPlayers.Clear();
            matchPlayerOneCharacterId = string.Empty;
            matchPlayerTwoCharacterId = string.Empty;
            NotifyRoomStateChanged();
        }

        /// <summary>
        /// Registers a newly connected room player and assigns its slot.
        /// </summary>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);

            QuoridorRoomPlayer roomPlayer = conn.identity != null
                ? conn.identity.GetComponent<QuoridorRoomPlayer>()
                : null;
            if (roomPlayer != null && !roomPlayers.Contains(roomPlayer))
            {
                roomPlayer.ServerAssignSlot(roomPlayers.Count + 1);
                roomPlayers.Add(roomPlayer);
            }

            NotifyRoomStateChanged();
        }

        /// <summary>
        /// Removes a disconnected room player from host-side tracking.
        /// </summary>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (conn.identity != null)
            {
                QuoridorRoomPlayer roomPlayer = conn.identity.GetComponent<QuoridorRoomPlayer>();
                if (roomPlayer != null)
                {
                    roomPlayers.Remove(roomPlayer);
                }
            }

            base.OnServerDisconnect(conn);
            NotifyRoomStateChanged();
        }

        /// <summary>
        /// Notifies listeners after a client connects.
        /// </summary>
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            NotifyRoomStateChanged();
        }

        /// <summary>
        /// Notifies listeners after a client disconnects.
        /// </summary>
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            QuoridorRoomPlayer.ClearLocalPlayerSlot();
            NotifyRoomStateChanged();
        }

        /// <summary>
        /// Applies synchronized character choices before match scene UI resolves characters.
        /// </summary>
        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();
            ApplyVisibleRoomSelections();
        }

        private void EnsureDiscovery()
        {
            if (discovery == null)
            {
                discovery = GetComponent<QuoridorLanDiscovery>();
            }
        }

        private static void CacheLocalSelection(string selectedCharacterId)
        {
            if (!string.IsNullOrWhiteSpace(selectedCharacterId))
            {
                LocalPlayerSelection.SetCharacter(PlayerId.PlayerOne, selectedCharacterId);
            }
        }

        private void ApplyNetworkCharacterSelection()
        {
            matchPlayerOneCharacterId = roomPlayers.Count > 0 ? roomPlayers[0].CharacterId : string.Empty;
            matchPlayerTwoCharacterId = roomPlayers.Count > 1 ? roomPlayers[1].CharacterId : string.Empty;
            LocalPlayerSelection.SetCharacter(PlayerId.PlayerOne, matchPlayerOneCharacterId);
            LocalPlayerSelection.SetCharacter(PlayerId.PlayerTwo, matchPlayerTwoCharacterId);
        }

        private void HandleMatchConfigRequest(NetworkConnectionToClient connection, MatchConfigRequestMessage message)
        {
            if (connection == null)
            {
                return;
            }

            connection.Send(CreateMatchConfigMessage());
        }

        private MatchConfigMessage CreateMatchConfigMessage()
        {
            return new MatchConfigMessage
            {
                PlayerOneCharacterId = matchPlayerOneCharacterId,
                PlayerTwoCharacterId = matchPlayerTwoCharacterId
            };
        }

        private static void ApplyVisibleRoomSelections()
        {
            QuoridorRoomPlayer[] visiblePlayers = FindObjectsByType<QuoridorRoomPlayer>(FindObjectsSortMode.None);
            foreach (QuoridorRoomPlayer roomPlayer in visiblePlayers)
            {
                if (roomPlayer == null)
                {
                    continue;
                }

                PlayerId playerId = roomPlayer.PlayerSlot <= 1 ? PlayerId.PlayerOne : PlayerId.PlayerTwo;
                LocalPlayerSelection.SetCharacter(playerId, roomPlayer.CharacterId);
            }
        }

        private void NotifyRoomStateChanged()
        {
            RoomStateChanged?.Invoke();
        }
    }
}
