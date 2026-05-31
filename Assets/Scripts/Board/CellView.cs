// Scene-facing view for one board cell
using System;
using Quoridor.Core;
using UnityEngine;

namespace Quoridor.Board
{
    /// <summary>
    /// Scene-facing view for one board cell.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class CellView : MonoBehaviour
    {
        [Tooltip("Board X coordinate assigned during edit-time board generation")]
        [SerializeField] private int coordinateX;
        [Tooltip("Board Y coordinate assigned during edit-time board generation")]
        [SerializeField] private int coordinateY;
        [Tooltip("Renderer that displays this cell")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Material used when the cell is idle")]
        [SerializeField] private Material defaultMaterial;
        [Tooltip("Material used while the pointer is over the cell")]
        [SerializeField] private Material hoverMaterial;
        [Tooltip("Material used when the cell is a legal pawn destination")]
        [SerializeField] private Material moveHintMaterial;

        private CellHighlightState highlightState = CellHighlightState.None;
        private bool isPointerOver;

        /// <summary>
        /// Raised when the pointer enters this cell.
        /// </summary>
        public event Action<CellView> HoverEntered;

        /// <summary>
        /// Raised when the pointer exits this cell.
        /// </summary>
        public event Action<CellView> HoverExited;

        /// <summary>
        /// Raised when this cell is clicked.
        /// </summary>
        public event Action<CellView> Selected;

        /// <summary>
        /// Current immutable board coordinate for this cell.
        /// </summary>
        public BoardPosition Position => new BoardPosition(coordinateX, coordinateY);

        /// <summary>
        /// Renderer used by this cell's visual state.
        /// </summary>
        public SpriteRenderer Renderer => spriteRenderer;

        /// <summary>
        /// Assigns this cell's board coordinate.
        /// </summary>
        public void SetCoordinate(BoardPosition coordinate)
        {
            coordinateX = coordinate.X;
            coordinateY = coordinate.Y;
        }

        /// <summary>
        /// Assigns this cell's board coordinate.
        /// </summary>
        public void SetCoordinate(int x, int y)
        {
            coordinateX = x;
            coordinateY = y;
        }

        /// <summary>
        /// Shows the default cell material.
        /// </summary>
        public void SetDefault()
        {
            highlightState = CellHighlightState.None;
            isPointerOver = false;
            ApplyMaterial(defaultMaterial);
        }

        /// <summary>
        /// Shows the hover cell material.
        /// </summary>
        public void SetHover()
        {
            SetPointerHover(true);
        }

        /// <summary>
        /// Shows the legal movement hint material.
        /// </summary>
        public void SetMoveHint()
        {
            highlightState = CellHighlightState.MoveHint;
            ApplyCurrentMaterial();
        }

        /// <summary>
        /// Updates pointer hover state without changing the persistent highlight state.
        /// </summary>
        public void SetPointerHover(bool isHovered)
        {
            isPointerOver = isHovered;
            ApplyCurrentMaterial();
        }

        private void Reset()
        {
            CacheRenderer();
        }

        private void OnValidate()
        {
            CacheRenderer();
        }

        private void OnMouseEnter()
        {
            SetPointerHover(true);
            HoverEntered?.Invoke(this);
        }

        private void OnMouseExit()
        {
            SetPointerHover(false);
            HoverExited?.Invoke(this);
        }

        private void OnMouseDown()
        {
            Selected?.Invoke(this);
        }

        private void CacheRenderer()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void ApplyMaterial(Material material)
        {
            CacheRenderer();

            if (spriteRenderer != null && material != null)
            {
                spriteRenderer.sharedMaterial = material;
            }
        }

        private void ApplyCurrentMaterial()
        {
            if (isPointerOver)
            {
                ApplyMaterial(hoverMaterial);
                return;
            }

            ApplyMaterial(highlightState == CellHighlightState.MoveHint ? moveHintMaterial : defaultMaterial);
        }

        private enum CellHighlightState
        {
            None = 0,
            MoveHint = 1
        }
    }
}
