using Quoridor.Board;
using Quoridor.Config;
using Quoridor.Core;
using UnityEngine;

namespace Quoridor.Wall
{
    /// <summary>
    /// Scene-facing wall view used for both preview and placed wall visuals.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WallView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;
        [SerializeField] private Material placedMaterial;
        [SerializeField] private int sortingOrder = 15;

        /// <summary>
        /// Current wall placement represented by this view.
        /// </summary>
        public WallPlacement Placement { get; private set; }

        /// <summary>
        /// Configures this view to display a wall at the supplied placement.
        /// </summary>
        public void Configure(WallPlacement placement, BoardView boardView, GameConfig config, Material material)
        {
            Placement = placement;
            ApplyMaterial(material);
            ApplyTransform(placement, boardView, config);
        }

        /// <summary>
        /// Shows preview styling for valid or invalid candidate placement.
        /// </summary>
        public void SetPreviewValidity(bool isValid)
        {
            ApplyMaterial(isValid ? validPreviewMaterial : invalidPreviewMaterial);
        }

        /// <summary>
        /// Shows final placed-wall styling.
        /// </summary>
        public void SetPlaced()
        {
            ApplyMaterial(placedMaterial);
        }

        /// <summary>
        /// Shows or hides this wall view.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        private void Reset()
        {
            CacheRenderer();
        }

        private void OnValidate()
        {
            CacheRenderer();
        }

        private void ApplyTransform(WallPlacement placement, BoardView boardView, GameConfig config)
        {
            float spacing = config != null ? config.CellSpacing : 1f;
            float thickness = config != null ? config.WallThickness : 0.14f;
            Vector3 anchorPosition = transform.position;

            if (boardView != null && boardView.TryGetCell(placement.Anchor, out CellView anchorCell))
            {
                anchorPosition = anchorCell.transform.position;
            }

            transform.position = new Vector3(
                anchorPosition.x + spacing * 0.5f,
                anchorPosition.y + spacing * 0.5f,
                transform.position.z);

            transform.localRotation = Quaternion.identity;
            transform.localScale = placement.Orientation == WallOrientation.Horizontal
                ? new Vector3(spacing * 2f, thickness, 1f)
                : new Vector3(thickness, spacing * 2f, 1f);
        }

        private void ApplyMaterial(Material material)
        {
            CacheRenderer();

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sortingOrder = sortingOrder;
            if (material != null)
            {
                spriteRenderer.sharedMaterial = material;
            }
        }

        private void CacheRenderer()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }
    }
}
