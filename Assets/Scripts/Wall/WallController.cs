// Handles wall preview, validation, placement, and remaining wall counts
using System;
using Quoridor.Board;
using Quoridor.Config;
using Quoridor.Core;
using Quoridor.Input;
using Quoridor.Pawn;
using UnityEngine;

namespace Quoridor.Wall
{
    /// <summary>
    /// Handles wall preview, validation, placement, and remaining wall counts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WallController : MonoBehaviour
    {
        [Tooltip("Rules and layout values used by wall placement")]
        [SerializeField] private GameConfig config;
        [Tooltip("Board view used to position wall visuals")]
        [SerializeField] private BoardView boardView;
        [Tooltip("Input router that supplies board click and orientation events")]
        [SerializeField] private InputRouter inputRouter;
        [Tooltip("Pawn controller used to validate path connectivity around current pawn positions")]
        [SerializeField] private PawnController pawnController;
        [Tooltip("Reusable wall view shown while previewing placement")]
        [SerializeField] private WallView previewWall;
        [Tooltip("Prefab instantiated for each placed wall")]
        [SerializeField] private WallView placedWallPrefab;
        [Tooltip("Parent transform for placed wall instances")]
        [SerializeField] private Transform placedWallsRoot;
        [Tooltip("Material used by a valid wall preview")]
        [SerializeField] private Material validPreviewMaterial;
        [Tooltip("Material used by an invalid wall preview")]
        [SerializeField] private Material invalidPreviewMaterial;
        [Tooltip("Material used by committed wall visuals")]
        [SerializeField] private Material placedMaterial;

        private WallState wallState;
        private WallPlacement currentPreview;
        private bool hasPreview;
        private bool inputEnabled = true;
        private bool directInputEnabled = true;
        private bool suppressWallEvent;

        /// <summary>
        /// Raised after a wall is placed successfully.
        /// </summary>
        public event Action<WallPlacedEvent> WallPlaced;

        /// <summary>
        /// Walls remaining for player one.
        /// </summary>
        public int PlayerOneWallsRemaining => wallState != null
            ? wallState.GetRemainingWalls(PlayerId.PlayerOne)
            : GetInitialWallCount();

        /// <summary>
        /// Walls remaining for player two.
        /// </summary>
        public int PlayerTwoWallsRemaining => wallState != null
            ? wallState.GetRemainingWalls(PlayerId.PlayerTwo)
            : GetInitialWallCount();

        /// <summary>
        /// True when wall placement input is accepted.
        /// </summary>
        public bool InputEnabled => inputEnabled;

        /// <summary>
        /// True when this controller consumes board input directly.
        /// </summary>
        public bool DirectInputEnabled => directInputEnabled;

        /// <summary>
        /// Reinitializes wall state for a fresh local match.
        /// </summary>
        public void ResetMatch()
        {
            int boardSize = config != null ? config.BoardSize : QuoridorRules.BoardSize;
            int initialWallCount = config != null ? config.InitialWallCount : QuoridorRules.InitialWallCount;

            wallState = new WallState(boardSize, initialWallCount);
            ClearPlacedWallViews();

            if (pawnController != null)
            {
                pawnController.SetBoardGraph(wallState.Graph);
            }

            HidePreview();
        }

        /// <summary>
        /// Attempts to place a wall at the supplied anchor and active orientation.
        /// </summary>
        public bool TryPlaceWall(BoardPosition anchor)
        {
            if (!inputEnabled)
            {
                return false;
            }

            WallPlacement candidate = new WallPlacement(anchor, GetCurrentOrientation());
            PlayerId playerId = GetActivePlayer();
            WallValidationResult result = TryCommitWall(playerId, candidate);
            if (!result.IsValid)
            {
                ShowPreview(candidate, false);
                return false;
            }

            CreatePlacedWall(candidate);

            if (pawnController != null)
            {
                pawnController.SetBoardGraph(wallState.Graph);
            }

            if (inputRouter != null)
            {
                inputRouter.SetInputMode(InputMode.PawnMove);
            }

            HidePreview();
            if (!suppressWallEvent)
            {
                WallPlaced?.Invoke(new WallPlacedEvent(playerId, candidate, wallState.GetRemainingWalls(playerId)));
            }

            return true;
        }

        /// <summary>
        /// Attempts to place a specific wall for a specific player.
        /// </summary>
        public bool TryPlaceWall(PlayerId playerId, WallPlacement placement)
        {
            if (!inputEnabled)
            {
                return false;
            }

            WallValidationResult result = TryCommitWall(playerId, placement);
            if (!result.IsValid)
            {
                ShowPreview(placement, false);
                return false;
            }

            CreatePlacedWall(placement);

            if (pawnController != null)
            {
                pawnController.SetBoardGraph(wallState.Graph);
            }

            if (inputRouter != null)
            {
                inputRouter.SetInputMode(InputMode.PawnMove);
            }

            HidePreview();
            if (!suppressWallEvent)
            {
                WallPlaced?.Invoke(new WallPlacedEvent(playerId, placement, wallState.GetRemainingWalls(playerId)));
            }

            return true;
        }

        /// <summary>
        /// Applies a validated wall placement without raising a local wall event.
        /// </summary>
        public bool ApplyRemoteWall(PlayerId playerId, WallPlacement placement)
        {
            bool previousSuppressWallEvent = suppressWallEvent;
            bool previousInputEnabled = inputEnabled;
            suppressWallEvent = true;
            inputEnabled = true;
            bool placed = TryPlaceWall(playerId, placement);
            inputEnabled = previousInputEnabled;
            suppressWallEvent = previousSuppressWallEvent;
            return placed;
        }

        /// <summary>
        /// Enables or disables wall placement input.
        /// </summary>
        public void SetInputEnabled(bool isEnabled)
        {
            inputEnabled = isEnabled;

            if (!inputEnabled)
            {
                HidePreview();
            }
        }

        /// <summary>
        /// Enables or disables direct local board input handling.
        /// </summary>
        public void SetDirectInputEnabled(bool isEnabled)
        {
            directInputEnabled = isEnabled;
        }

        private void Awake()
        {
            ResetMatch();
        }

        private void OnEnable()
        {
            if (inputRouter == null)
            {
                return;
            }

            inputRouter.BoardCellInput += HandleBoardCellInput;
            inputRouter.WallOrientationChanged += HandleWallOrientationChanged;
            inputRouter.InputModeChanged += HandleInputModeChanged;
        }

        private void OnDisable()
        {
            if (inputRouter == null)
            {
                return;
            }

            inputRouter.BoardCellInput -= HandleBoardCellInput;
            inputRouter.WallOrientationChanged -= HandleWallOrientationChanged;
            inputRouter.InputModeChanged -= HandleInputModeChanged;
        }

        private void HandleBoardCellInput(BoardCellInputEvent inputEvent)
        {
            if (!inputEnabled || !directInputEnabled)
            {
                return;
            }

            if (inputEvent.Mode != InputMode.WallPlacement)
            {
                return;
            }

            if (inputEvent.Phase == BoardCellInputPhase.HoverExited)
            {
                HidePreview();
                return;
            }

            WallPlacement candidate = new WallPlacement(inputEvent.Position, GetCurrentOrientation());
            WallValidationResult result = Validate(candidate);
            ShowPreview(candidate, result.IsValid);

            if (inputEvent.Phase == BoardCellInputPhase.Selected)
            {
                TryPlaceWall(inputEvent.Position);
            }
        }

        private void HandleWallOrientationChanged(WallOrientation orientation)
        {
            if (!hasPreview || inputRouter == null || inputRouter.ActiveMode != InputMode.WallPlacement)
            {
                return;
            }

            WallPlacement candidate = new WallPlacement(currentPreview.Anchor, orientation);
            WallValidationResult result = Validate(candidate);
            ShowPreview(candidate, result.IsValid);
        }

        private void HandleInputModeChanged(InputMode mode)
        {
            if (mode != InputMode.WallPlacement)
            {
                HidePreview();
            }
        }

        private WallValidationResult Validate(WallPlacement candidate)
        {
            EnsureGraph();
            return wallState.CanPlaceWall(
                GetActivePlayer(),
                candidate,
                GetPlayerPosition(PlayerId.PlayerOne),
                GetPlayerPosition(PlayerId.PlayerTwo));
        }

        private WallValidationResult TryCommitWall(PlayerId playerId, WallPlacement candidate)
        {
            EnsureGraph();
            return wallState.TryPlaceWall(
                playerId,
                candidate,
                GetPlayerPosition(PlayerId.PlayerOne),
                GetPlayerPosition(PlayerId.PlayerTwo));
        }

        private void ShowPreview(WallPlacement placement, bool isValid)
        {
            if (previewWall == null)
            {
                return;
            }

            hasPreview = true;
            currentPreview = placement;
            previewWall.SetVisible(true);
            previewWall.Configure(placement, boardView, config, isValid ? validPreviewMaterial : invalidPreviewMaterial);
            previewWall.SetPreviewValidity(isValid);
        }

        private void HidePreview()
        {
            hasPreview = false;

            if (previewWall != null)
            {
                previewWall.SetVisible(false);
            }
        }

        private void CreatePlacedWall(WallPlacement placement)
        {
            if (placedWallPrefab == null)
            {
                return;
            }

            Transform parent = placedWallsRoot != null ? placedWallsRoot : transform;
            WallView wallView = Instantiate(placedWallPrefab, parent);
            wallView.name = $"{placement.Orientation}Wall_{placement.Anchor.X}_{placement.Anchor.Y}";
            wallView.Configure(placement, boardView, config, placedMaterial);
            wallView.SetPlaced();
            wallView.SetVisible(true);
        }

        private void ClearPlacedWallViews()
        {
            if (placedWallsRoot == null)
            {
                return;
            }

            for (int index = placedWallsRoot.childCount - 1; index >= 0; index--)
            {
                Destroy(placedWallsRoot.GetChild(index).gameObject);
            }
        }

        private void EnsureGraph()
        {
            if (wallState == null)
            {
                int boardSize = config != null ? config.BoardSize : QuoridorRules.BoardSize;
                wallState = new WallState(boardSize, GetInitialWallCount());
            }
        }

        private int GetInitialWallCount()
        {
            return config != null ? config.InitialWallCount : QuoridorRules.InitialWallCount;
        }

        private PlayerId GetActivePlayer()
        {
            return pawnController != null ? pawnController.ActivePlayer : PlayerId.PlayerOne;
        }

        private WallOrientation GetCurrentOrientation()
        {
            return inputRouter != null ? inputRouter.CurrentWallOrientation : WallOrientation.Horizontal;
        }

        private BoardPosition GetPlayerPosition(PlayerId playerId)
        {
            if (pawnController == null)
            {
                return playerId == PlayerId.PlayerOne ? QuoridorRules.PlayerOneStart : QuoridorRules.PlayerTwoStart;
            }

            return playerId == PlayerId.PlayerOne ? pawnController.PlayerOnePosition : pawnController.PlayerTwoPosition;
        }

        private static PlayerId GetNextPlayer(PlayerId playerId)
        {
            return playerId == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne;
        }
    }
}
