using Mirror;
using Quoridor.Config;
using Quoridor.Core;
using UnityEngine;

namespace Quoridor.Networking
{
    /// <summary>
    /// Networked lobby player state for the LAN room.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuoridorRoomPlayer : NetworkBehaviour
    {
        [SyncVar(hook = nameof(HandleCharacterChanged))]
        private string characterId = string.Empty;

        [SyncVar(hook = nameof(HandleSlotChanged))]
        private int playerSlot;

        /// <summary>
        /// Networked character id chosen by this player.
        /// </summary>
        public string CharacterId => characterId;

        /// <summary>
        /// One-based player slot assigned by the host.
        /// </summary>
        public int PlayerSlot => playerSlot;

        /// <summary>
        /// True for the local client's room player object.
        /// </summary>
        public bool IsLocalRoomPlayer => isLocalPlayer;

        /// <summary>
        /// Last one-based slot assigned to the local network player.
        /// </summary>
        public static int LocalPlayerSlot { get; private set; }

        /// <summary>
        /// Clears cached local slot information when leaving network play.
        /// </summary>
        public static void ClearLocalPlayerSlot()
        {
            LocalPlayerSlot = 0;
        }

        /// <summary>
        /// Called on clients when visible room data changes.
        /// </summary>
        public event System.Action StateChanged;

        /// <summary>
        /// Sends a local character choice to the server.
        /// </summary>
        public void SetLocalCharacter(string selectedCharacterId)
        {
            if (!isLocalPlayer)
            {
                return;
            }

            CmdSetCharacter(selectedCharacterId);
        }

        /// <summary>
        /// Assigns the player slot on the server.
        /// </summary>
        [Server]
        public void ServerAssignSlot(int slot)
        {
            playerSlot = slot;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            CacheLocalSlot();

            string selectedId = LocalPlayerSelection.GetCharacterId(PlayerId.PlayerOne);
            if (!string.IsNullOrWhiteSpace(selectedId))
            {
                CmdSetCharacter(selectedId);
            }
        }

        [Command]
        private void CmdSetCharacter(string selectedCharacterId)
        {
            characterId = string.IsNullOrWhiteSpace(selectedCharacterId) ? string.Empty : selectedCharacterId;
        }

        private void HandleCharacterChanged(string oldValue, string newValue)
        {
            StateChanged?.Invoke();
        }

        private void HandleSlotChanged(int oldValue, int newValue)
        {
            CacheLocalSlot();
            StateChanged?.Invoke();
        }

        private void CacheLocalSlot()
        {
            if (isLocalPlayer && playerSlot > 0)
            {
                LocalPlayerSlot = playerSlot;
            }
        }
    }
}
