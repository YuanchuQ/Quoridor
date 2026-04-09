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
        [SerializeField] private int coordinateX;
        [SerializeField] private int coordinateY;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material hoverMaterial;
        [SerializeField] private Material moveHintMaterial;

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
            ApplyMaterial(defaultMaterial);
        }

        /// <summary>
        /// Shows the hover cell material.
        /// </summary>
        public void SetHover()
        {
            ApplyMaterial(hoverMaterial);
        }

        /// <summary>
        /// Shows the legal movement hint material.
        /// </summary>
        public void SetMoveHint()
        {
            ApplyMaterial(moveHintMaterial);
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
            SetHover();
            HoverEntered?.Invoke(this);
        }

        private void OnMouseExit()
        {
            SetDefault();
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
    }
}
