using System;
using System.Collections.Generic;
using Quoridor.Board;
using Quoridor.Core;
using UnityEngine;

namespace Quoridor.Input
{
    /// <summary>
    /// Centralizes local player board and keyboard input before game systems consume it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputRouter : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private InputMode activeMode = InputMode.PawnMove;
        [SerializeField] private WallOrientation currentWallOrientation = WallOrientation.Horizontal;
        [SerializeField] private KeyCode toggleInputModeKey = KeyCode.Tab;
        [SerializeField] private KeyCode toggleWallOrientationKey = KeyCode.R;

        private readonly HashSet<CellView> subscribedCells = new();

        /// <summary>
        /// Raised when board cell input is routed through the active input mode.
        /// </summary>
        public event Action<BoardCellInputEvent> BoardCellInput;

        /// <summary>
        /// Raised when the pointer enters a board cell.
        /// </summary>
        public event Action<CellView> CellHoverEntered;

        /// <summary>
        /// Raised when the pointer exits a board cell.
        /// </summary>
        public event Action<CellView> CellHoverExited;

        /// <summary>
        /// Raised when a board cell is selected.
        /// </summary>
        public event Action<CellView> CellSelected;

        /// <summary>
        /// Raised whenever the active input mode changes.
        /// </summary>
        public event Action<InputMode> InputModeChanged;

        /// <summary>
        /// Raised when wall orientation is toggled by input.
        /// </summary>
        public event Action<WallOrientationToggleEvent> WallOrientationToggled;

        /// <summary>
        /// Raised whenever the active wall orientation changes.
        /// </summary>
        public event Action<WallOrientation> WallOrientationChanged;

        /// <summary>
        /// Board currently used as the source for cell input events.
        /// </summary>
        public BoardView BoardView => boardView;

        /// <summary>
        /// Input mode applied to routed board-cell events.
        /// </summary>
        public InputMode ActiveMode => activeMode;

        /// <summary>
        /// Wall orientation currently selected for wall placement input.
        /// </summary>
        public WallOrientation CurrentWallOrientation => currentWallOrientation;

        /// <summary>
        /// Assigns the board view used for cell event subscriptions.
        /// </summary>
        public void SetBoardView(BoardView nextBoardView)
        {
            if (boardView == nextBoardView)
            {
                return;
            }

            UnsubscribeFromCells();
            boardView = nextBoardView;

            if (isActiveAndEnabled)
            {
                SubscribeToCells();
            }
        }

        /// <summary>
        /// Rebuilds cell subscriptions from the currently assigned board view.
        /// </summary>
        public void RefreshBoardSubscriptions()
        {
            UnsubscribeFromCells();

            if (isActiveAndEnabled)
            {
                SubscribeToCells();
            }
        }

        /// <summary>
        /// Changes the active input mode used by board-cell events.
        /// </summary>
        public void SetInputMode(InputMode mode)
        {
            if (activeMode == mode)
            {
                return;
            }

            activeMode = mode;
            InputModeChanged?.Invoke(activeMode);
        }

        /// <summary>
        /// Toggles the active input mode between pawn movement and wall placement.
        /// </summary>
        public void ToggleInputMode()
        {
            SetInputMode(activeMode == InputMode.PawnMove ? InputMode.WallPlacement : InputMode.PawnMove);
        }

        /// <summary>
        /// Changes the active wall orientation.
        /// </summary>
        public void SetWallOrientation(WallOrientation orientation)
        {
            if (currentWallOrientation == orientation)
            {
                return;
            }

            currentWallOrientation = orientation;
            WallOrientationChanged?.Invoke(currentWallOrientation);
        }

        /// <summary>
        /// Toggles the wall orientation and raises wall orientation input events.
        /// </summary>
        public void ToggleWallOrientation()
        {
            WallOrientation previousOrientation = currentWallOrientation;
            WallOrientation nextOrientation = previousOrientation == WallOrientation.Horizontal
                ? WallOrientation.Vertical
                : WallOrientation.Horizontal;

            currentWallOrientation = nextOrientation;
            WallOrientationToggled?.Invoke(new WallOrientationToggleEvent(previousOrientation, nextOrientation));
            WallOrientationChanged?.Invoke(currentWallOrientation);
        }

        private void OnEnable()
        {
            SubscribeToCells();
        }

        private void OnDisable()
        {
            UnsubscribeFromCells();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(toggleInputModeKey))
            {
                ToggleInputMode();
            }

            if (UnityEngine.Input.GetKeyDown(toggleWallOrientationKey))
            {
                ToggleWallOrientation();
            }
        }

        private void SubscribeToCells()
        {
            if (boardView == null)
            {
                return;
            }

            foreach (CellView cell in boardView.Cells)
            {
                if (cell == null || !subscribedCells.Add(cell))
                {
                    continue;
                }

                cell.HoverEntered += HandleCellHoverEntered;
                cell.HoverExited += HandleCellHoverExited;
                cell.Selected += HandleCellSelected;
            }
        }

        private void UnsubscribeFromCells()
        {
            foreach (CellView cell in subscribedCells)
            {
                if (cell == null)
                {
                    continue;
                }

                cell.HoverEntered -= HandleCellHoverEntered;
                cell.HoverExited -= HandleCellHoverExited;
                cell.Selected -= HandleCellSelected;
            }

            subscribedCells.Clear();
        }

        private void HandleCellHoverEntered(CellView cell)
        {
            CellHoverEntered?.Invoke(cell);
            PublishBoardCellInput(cell, BoardCellInputPhase.HoverEntered);
        }

        private void HandleCellHoverExited(CellView cell)
        {
            CellHoverExited?.Invoke(cell);
            PublishBoardCellInput(cell, BoardCellInputPhase.HoverExited);
        }

        private void HandleCellSelected(CellView cell)
        {
            CellSelected?.Invoke(cell);
            PublishBoardCellInput(cell, BoardCellInputPhase.Selected);
        }

        private void PublishBoardCellInput(CellView cell, BoardCellInputPhase phase)
        {
            if (cell == null)
            {
                return;
            }

            var inputEvent = new BoardCellInputEvent(cell.Position, phase, activeMode, cell);
            BoardCellInput?.Invoke(inputEvent);
        }
    }
}
