// Centralizes local player board and keyboard input before game systems consume it
using System;
using Quoridor.Board;
using Quoridor.Core;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Quoridor.Input
{
    /// <summary>
    /// Centralizes local player board and keyboard input before game systems consume it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputRouter : MonoBehaviour
    {
        [Tooltip("Board view used for pointer hit testing")]
        [SerializeField] private BoardView boardView;
        [Tooltip("Current action mode for board clicks")]
        [SerializeField] private InputMode activeMode = InputMode.PawnMove;
        [Tooltip("Current wall orientation used by wall placement")]
        [SerializeField] private WallOrientation currentWallOrientation = WallOrientation.Horizontal;
        [Tooltip("Camera used to convert pointer positions into world positions")]
        [SerializeField] private Camera inputCamera;
        [Tooltip("Keyboard key that switches between movement and wall placement")]
        [SerializeField] private KeyCode toggleInputModeKey = KeyCode.Tab;
        [Tooltip("Keyboard key that rotates wall placement orientation")]
        [SerializeField] private KeyCode toggleWallOrientationKey = KeyCode.R;

        private CellView hoveredCell;

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

            ClearHoveredCell();
            boardView = nextBoardView;
        }

        /// <summary>
        /// Rebuilds cell subscriptions from the currently assigned board view.
        /// </summary>
        public void RefreshBoardSubscriptions()
        {
            ClearHoveredCell();
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

        private void OnDisable()
        {
            ClearHoveredCell();
        }

        private void Update()
        {
            RoutePointerInput();

            if (WasKeyPressed(toggleInputModeKey))
            {
                ToggleInputMode();
            }

            if (WasKeyPressed(toggleWallOrientationKey))
            {
                ToggleWallOrientation();
            }
        }

        private void RoutePointerInput()
        {
            CellView currentCell = GetCellUnderPointer();
            if (currentCell != hoveredCell)
            {
                if (hoveredCell != null)
                {
                    hoveredCell.SetPointerHover(false);
                    CellHoverExited?.Invoke(hoveredCell);
                    PublishBoardCellInput(hoveredCell, BoardCellInputPhase.HoverExited);
                }

                hoveredCell = currentCell;

                if (hoveredCell != null)
                {
                    hoveredCell.SetPointerHover(true);
                    CellHoverEntered?.Invoke(hoveredCell);
                    PublishBoardCellInput(hoveredCell, BoardCellInputPhase.HoverEntered);
                }
            }

            if (hoveredCell != null && WasPrimaryPointerPressed())
            {
                CellSelected?.Invoke(hoveredCell);
                PublishBoardCellInput(hoveredCell, BoardCellInputPhase.Selected);
            }
        }

        private CellView GetCellUnderPointer()
        {
            if (!TryGetPointerScreenPosition(out Vector2 screenPosition))
            {
                return null;
            }

            Camera cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                return null;
            }

            Vector3 worldPosition = cameraToUse.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -cameraToUse.transform.position.z));
            Collider2D collider = Physics2D.OverlapPoint(worldPosition);
            return collider != null ? collider.GetComponent<CellView>() : null;
        }

        private static bool WasKeyPressed(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                switch (keyCode)
                {
                    case KeyCode.R:
                        return keyboard.rKey.wasPressedThisFrame;
                    case KeyCode.Tab:
                        return keyboard.tabKey.wasPressedThisFrame;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKeyDown(keyCode);
#else
            return false;
#endif
        }

        private void ClearHoveredCell()
        {
            if (hoveredCell != null)
            {
                hoveredCell.SetPointerHover(false);
                hoveredCell = null;
            }
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

        private static bool TryGetPointerScreenPosition(out Vector2 screenPosition)
        {
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            screenPosition = UnityEngine.Input.mousePosition;
            return true;
#else
            screenPosition = default;
            return false;
#endif
        }

        private static bool WasPrimaryPointerPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                return mouse.leftButton.wasPressedThisFrame;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }
    }
}
