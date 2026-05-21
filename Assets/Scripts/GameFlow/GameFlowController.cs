using System;
using Quoridor.Config;
using Quoridor.Core;
using Quoridor.Input;
using Quoridor.Networking;
using Quoridor.Pawn;
using Quoridor.UI;
using Quoridor.Wall;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quoridor.GameFlow
{
    /// <summary>
    /// Owns local match turn state, victory detection, and UI refreshes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private InputRouter inputRouter;
        [SerializeField] private PawnController pawnController;
        [SerializeField] private WallController wallController;
        [SerializeField] private MatchUiView matchUiView;
        [SerializeField] private CharacterVisualCatalog characterCatalog;
        [SerializeField] private string lobbySceneName = "MainMenu";

        private GameState state = GameState.PlayerOneTurn;
        private PlayerId activePlayer = PlayerId.PlayerOne;
        private PlayerId winner = PlayerId.PlayerOne;
        private bool networkControlled;

        /// <summary>
        /// Raised whenever the high-level match state changes.
        /// </summary>
        public event Action<GameState> StateChanged;

        /// <summary>
        /// Current high-level match state.
        /// </summary>
        public GameState State => state;

        /// <summary>
        /// Player whose turn is currently active.
        /// </summary>
        public PlayerId ActivePlayer => activePlayer;

        /// <summary>
        /// Winning player when the match is over.
        /// </summary>
        public PlayerId Winner => winner;

        /// <summary>
        /// Starts a fresh local match.
        /// </summary>
        public void StartNewMatch()
        {
            activePlayer = PlayerId.PlayerOne;
            state = GameState.PlayerOneTurn;

            if (pawnController != null)
            {
                pawnController.ResetMatch();
                pawnController.SetActivePlayer(activePlayer);
                pawnController.SetInputEnabled(true);
            }

            if (wallController != null)
            {
                wallController.ResetMatch();
                wallController.SetInputEnabled(true);
            }

            if (inputRouter != null)
            {
                inputRouter.SetInputMode(InputMode.PawnMove);
                inputRouter.SetWallOrientation(WallOrientation.Horizontal);
            }

            StateChanged?.Invoke(state);
            RefreshPlayerCharacters();
            RefreshUi();
            RefreshNetworkInputState();
        }

        /// <summary>
        /// Enables network-controlled input gating for this match.
        /// </summary>
        public void SetNetworkControlled(bool isNetworkControlled)
        {
            networkControlled = isNetworkControlled;
            if (pawnController != null)
            {
                pawnController.SetDirectInputEnabled(!networkControlled);
            }

            if (wallController != null)
            {
                wallController.SetDirectInputEnabled(!networkControlled);
            }

            RefreshNetworkInputState();
        }

        /// <summary>
        /// Applies a completed pawn move to turn state and UI without relying on local events.
        /// </summary>
        public void ApplyNetworkPawnMove(PawnMoveEvent moveEvent)
        {
            HandlePawnMoved(moveEvent);
            RefreshNetworkInputState();
        }

        /// <summary>
        /// Applies a completed wall placement to turn state and UI without relying on local events.
        /// </summary>
        public void ApplyNetworkWallPlacement(WallPlacedEvent placedEvent)
        {
            HandleWallPlaced(placedEvent);
            RefreshNetworkInputState();
        }

        /// <summary>
        /// Refreshes player character portraits from the current local selection.
        /// </summary>
        public void RefreshPlayerCharacters()
        {
            if (matchUiView == null || characterCatalog == null)
            {
                return;
            }

            matchUiView.SetPlayerCharacters(
                ResolveCharacter(PlayerId.PlayerOne),
                ResolveCharacter(PlayerId.PlayerTwo));
        }

        private void Start()
        {
            StartNewMatch();
        }

        private void OnEnable()
        {
            if (matchUiView != null)
            {
                matchUiView.ReturnToLobbyRequested += ReturnToLobby;
            }

            if (pawnController != null)
            {
                pawnController.PawnMoved += HandlePawnMoved;
            }

            if (wallController != null)
            {
                wallController.WallPlaced += HandleWallPlaced;
            }

            if (inputRouter != null)
            {
                inputRouter.InputModeChanged += HandleInputModeChanged;
                inputRouter.WallOrientationChanged += HandleWallOrientationChanged;
            }
        }

        private void OnDisable()
        {
            if (matchUiView != null)
            {
                matchUiView.ReturnToLobbyRequested -= ReturnToLobby;
            }

            if (pawnController != null)
            {
                pawnController.PawnMoved -= HandlePawnMoved;
            }

            if (wallController != null)
            {
                wallController.WallPlaced -= HandleWallPlaced;
            }

            if (inputRouter != null)
            {
                inputRouter.InputModeChanged -= HandleInputModeChanged;
                inputRouter.WallOrientationChanged -= HandleWallOrientationChanged;
            }
        }

        private void HandlePawnMoved(PawnMoveEvent moveEvent)
        {
            if (state == GameState.GameOver)
            {
                return;
            }

            if (HasWon(moveEvent.PlayerId, moveEvent.To))
            {
                SetGameOver(moveEvent.PlayerId);
                return;
            }

            AdvanceTurn();
        }

        private void HandleWallPlaced(WallPlacedEvent placedEvent)
        {
            if (state == GameState.GameOver)
            {
                return;
            }

            AdvanceTurn();
        }

        private void AdvanceTurn()
        {
            activePlayer = activePlayer == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne;
            state = activePlayer == PlayerId.PlayerOne ? GameState.PlayerOneTurn : GameState.PlayerTwoTurn;

            if (pawnController != null)
            {
                pawnController.SetActivePlayer(activePlayer);
            }

            StateChanged?.Invoke(state);
            RefreshUi();
            RefreshNetworkInputState();
        }

        private void SetGameOver(PlayerId winningPlayer)
        {
            winner = winningPlayer;
            state = GameState.GameOver;

            if (pawnController != null)
            {
                pawnController.SetInputEnabled(false);
            }

            if (wallController != null)
            {
                wallController.SetInputEnabled(false);
            }

            StateChanged?.Invoke(state);
            RefreshUi();
            RefreshNetworkInputState();
        }

        private bool HasWon(PlayerId playerId, BoardPosition position)
        {
            int boardSize = config != null ? config.BoardSize : QuoridorRules.BoardSize;
            return VictoryRules.HasPlayerWon(playerId, position, boardSize);
        }

        private void HandleInputModeChanged(InputMode mode)
        {
            RefreshUi();
        }

        private void HandleWallOrientationChanged(WallOrientation orientation)
        {
            RefreshUi();
        }

        private void ReturnToLobby()
        {
            QuoridorNetworkManager networkManager = FindFirstObjectByType<QuoridorNetworkManager>();
            if (networkManager != null)
            {
                networkManager.StopLanSession();
            }

            SceneManager.LoadScene(lobbySceneName);
        }

        private void RefreshUi()
        {
            if (matchUiView == null)
            {
                return;
            }

            matchUiView.Refresh(
                state,
                activePlayer,
                inputRouter != null ? inputRouter.ActiveMode : InputMode.PawnMove,
                inputRouter != null ? inputRouter.CurrentWallOrientation : WallOrientation.Horizontal,
                wallController != null ? wallController.PlayerOneWallsRemaining : QuoridorRules.InitialWallCount,
                wallController != null ? wallController.PlayerTwoWallsRemaining : QuoridorRules.InitialWallCount,
                winner);
        }

        private void RefreshNetworkInputState()
        {
            if (!networkControlled)
            {
                return;
            }

            bool isEnabled = state != GameState.GameOver;
            if (pawnController != null)
            {
                pawnController.SetInputEnabled(isEnabled);
            }

            if (wallController != null)
            {
                wallController.SetInputEnabled(isEnabled);
            }
        }

        private CharacterVisualDefinition ResolveCharacter(PlayerId playerId)
        {
            string selectedId = LocalPlayerSelection.GetCharacterId(playerId);
            CharacterVisualDefinition selected = characterCatalog.GetById(selectedId);
            return selected ?? characterCatalog.GetDefault(playerId);
        }
    }
}
