using Mirror;
using Quoridor.Core;
using Quoridor.GameFlow;
using Quoridor.Input;
using Quoridor.Pawn;
using Quoridor.Wall;
using UnityEngine;

namespace Quoridor.Networking
{
    /// <summary>
    /// Bridges local match input to Mirror messages and replays authoritative actions.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkMatchController : MonoBehaviour
    {
        [SerializeField] private InputRouter inputRouter;
        [SerializeField] private PawnController pawnController;
        [SerializeField] private WallController wallController;
        [SerializeField] private GameFlowController gameFlowController;

        private bool networkMatchActive;
        private bool applyingNetworkAction;

        /// <summary>
        /// True when this scene is running under Mirror networking.
        /// </summary>
        public bool NetworkMatchActive => networkMatchActive;

        private void Start()
        {
            networkMatchActive = NetworkClient.active || NetworkServer.active;
            if (!networkMatchActive)
            {
                return;
            }

            RegisterHandlers();

            if (gameFlowController != null)
            {
                gameFlowController.SetNetworkControlled(true);
            }
        }

        private void OnEnable()
        {
            if (inputRouter != null)
            {
                inputRouter.BoardCellInput += HandleBoardCellInput;
            }
        }

        private void OnDisable()
        {
            if (inputRouter != null)
            {
                inputRouter.BoardCellInput -= HandleBoardCellInput;
            }
        }

        private void OnDestroy()
        {
            if (gameFlowController != null)
            {
                gameFlowController.SetNetworkControlled(false);
            }
        }

        private void HandleBoardCellInput(BoardCellInputEvent inputEvent)
        {
            if (!networkMatchActive || applyingNetworkAction)
            {
                return;
            }

            if (inputEvent.Phase != BoardCellInputPhase.Selected)
            {
                return;
            }

            if (!IsLocalTurn())
            {
                return;
            }

            if (inputEvent.Mode == InputMode.PawnMove)
            {
                SendPawnMoveRequest(inputEvent.Position);
                return;
            }

            if (inputEvent.Mode == InputMode.WallPlacement && inputRouter != null)
            {
                SendWallPlaceRequest(new WallPlacement(inputEvent.Position, inputRouter.CurrentWallOrientation));
            }
        }

        private void RegisterHandlers()
        {
            if (NetworkServer.active)
            {
                NetworkServer.ReplaceHandler<PawnMoveRequestMessage>(HandleServerPawnMoveRequest);
                NetworkServer.ReplaceHandler<WallPlaceRequestMessage>(HandleServerWallPlaceRequest);
            }

            if (NetworkClient.active)
            {
                NetworkClient.ReplaceHandler<PawnMoveAppliedMessage>(HandleClientPawnMoveApplied);
                NetworkClient.ReplaceHandler<WallPlaceAppliedMessage>(HandleClientWallPlaceApplied);
            }
        }

        private void SendPawnMoveRequest(BoardPosition destination)
        {
            if (!NetworkClient.active)
            {
                return;
            }

            NetworkClient.Send(new PawnMoveRequestMessage
            {
                X = destination.X,
                Y = destination.Y
            });
        }

        private void SendWallPlaceRequest(WallPlacement placement)
        {
            if (!NetworkClient.active)
            {
                return;
            }

            NetworkClient.Send(new WallPlaceRequestMessage
            {
                X = placement.Anchor.X,
                Y = placement.Anchor.Y,
                Orientation = (int)placement.Orientation
            });
        }

        private void HandleServerPawnMoveRequest(NetworkConnectionToClient connection, PawnMoveRequestMessage message)
        {
            if (!IsConnectionTurnOwner(connection))
            {
                return;
            }

            PlayerId playerId = gameFlowController != null ? gameFlowController.ActivePlayer : PlayerId.PlayerOne;
            BoardPosition destination = new(message.X, message.Y);
            BoardPosition from = GetPlayerPosition(playerId);
            if (pawnController == null || !pawnController.ApplyRemoteMove(playerId, destination))
            {
                return;
            }

            if (gameFlowController != null)
            {
                gameFlowController.ApplyNetworkPawnMove(new PawnMoveEvent(playerId, from, destination));
            }

            NetworkServer.SendToReady(new PawnMoveAppliedMessage
            {
                PlayerId = (int)playerId,
                FromX = from.X,
                FromY = from.Y,
                ToX = destination.X,
                ToY = destination.Y
            });
        }

        private void HandleServerWallPlaceRequest(NetworkConnectionToClient connection, WallPlaceRequestMessage message)
        {
            if (!IsConnectionTurnOwner(connection))
            {
                return;
            }

            PlayerId playerId = gameFlowController != null ? gameFlowController.ActivePlayer : PlayerId.PlayerOne;
            WallPlacement placement = new(new BoardPosition(message.X, message.Y), (WallOrientation)message.Orientation);
            if (wallController == null || !wallController.ApplyRemoteWall(playerId, placement))
            {
                return;
            }

            int remainingWalls = playerId == PlayerId.PlayerOne
                ? wallController.PlayerOneWallsRemaining
                : wallController.PlayerTwoWallsRemaining;
            if (gameFlowController != null)
            {
                gameFlowController.ApplyNetworkWallPlacement(new WallPlacedEvent(playerId, placement, remainingWalls));
            }

            NetworkServer.SendToReady(new WallPlaceAppliedMessage
            {
                PlayerId = (int)playerId,
                X = placement.Anchor.X,
                Y = placement.Anchor.Y,
                Orientation = (int)placement.Orientation,
                RemainingWalls = remainingWalls
            });
        }

        private void HandleClientPawnMoveApplied(PawnMoveAppliedMessage message)
        {
            PlayerId playerId = (PlayerId)message.PlayerId;
            BoardPosition to = new(message.ToX, message.ToY);
            BoardPosition from = new(message.FromX, message.FromY);
            if (IsHostClient())
            {
                return;
            }

            applyingNetworkAction = true;
            try
            {
                if (pawnController != null && pawnController.ApplyRemoteMove(playerId, to) && gameFlowController != null)
                {
                    gameFlowController.ApplyNetworkPawnMove(new PawnMoveEvent(playerId, from, to));
                }
            }
            finally
            {
                applyingNetworkAction = false;
            }
        }

        private void HandleClientWallPlaceApplied(WallPlaceAppliedMessage message)
        {
            PlayerId playerId = (PlayerId)message.PlayerId;
            WallPlacement placement = new(new BoardPosition(message.X, message.Y), (WallOrientation)message.Orientation);
            if (IsHostClient())
            {
                return;
            }

            applyingNetworkAction = true;
            try
            {
                if (wallController != null && wallController.ApplyRemoteWall(playerId, placement) && gameFlowController != null)
                {
                    gameFlowController.ApplyNetworkWallPlacement(new WallPlacedEvent(playerId, placement, message.RemainingWalls));
                }
            }
            finally
            {
                applyingNetworkAction = false;
            }
        }

        private bool IsLocalTurn()
        {
            if (gameFlowController == null)
            {
                return false;
            }

            PlayerId localPlayerId = GetLocalPlayerId();
            return gameFlowController.ActivePlayer == localPlayerId;
        }

        private bool IsConnectionTurnOwner(NetworkConnectionToClient connection)
        {
            if (gameFlowController == null || connection == null || connection.identity == null)
            {
                return false;
            }

            QuoridorRoomPlayer roomPlayer = connection.identity.GetComponent<QuoridorRoomPlayer>();
            if (roomPlayer == null)
            {
                return false;
            }

            PlayerId connectionPlayerId = roomPlayer.PlayerSlot <= 1 ? PlayerId.PlayerOne : PlayerId.PlayerTwo;
            return gameFlowController.ActivePlayer == connectionPlayerId;
        }

        private PlayerId GetLocalPlayerId()
        {
            QuoridorRoomPlayer localPlayer = NetworkClient.localPlayer != null
                ? NetworkClient.localPlayer.GetComponent<QuoridorRoomPlayer>()
                : null;
            if (localPlayer != null)
            {
                return localPlayer.PlayerSlot <= 1 ? PlayerId.PlayerOne : PlayerId.PlayerTwo;
            }

            QuoridorRoomPlayer[] roomPlayers = FindObjectsByType<QuoridorRoomPlayer>(FindObjectsSortMode.None);
            foreach (QuoridorRoomPlayer roomPlayer in roomPlayers)
            {
                if (roomPlayer != null && roomPlayer.IsLocalRoomPlayer)
                {
                    return roomPlayer.PlayerSlot <= 1 ? PlayerId.PlayerOne : PlayerId.PlayerTwo;
                }
            }

            return PlayerId.PlayerOne;
        }

        private BoardPosition GetPlayerPosition(PlayerId playerId)
        {
            if (pawnController == null)
            {
                return playerId == PlayerId.PlayerOne ? QuoridorRules.PlayerOneStart : QuoridorRules.PlayerTwoStart;
            }

            return playerId == PlayerId.PlayerOne ? pawnController.PlayerOnePosition : pawnController.PlayerTwoPosition;
        }

        private static bool IsHostClient()
        {
            return NetworkServer.active && NetworkClient.isConnected;
        }
    }
}
