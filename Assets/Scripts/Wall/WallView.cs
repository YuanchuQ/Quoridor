// Scene-facing wall view used for both preview and placed wall visuals
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
        [Tooltip("Renderer used for the wall sprite")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Material used when preview placement is valid")]
        [SerializeField] private Material validPreviewMaterial;
        [Tooltip("Material used when preview placement is invalid")]
        [SerializeField] private Material invalidPreviewMaterial;
        [Tooltip("Material used after the wall is placed")]
        [SerializeField] private Material placedMaterial;
        [Tooltip("Sprite sorting order used by wall visuals")]
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

            CacheRenderer();
            if (spriteRenderer != null)
            {
                spriteRenderer.drawMode = SpriteDrawMode.Simple;
                bool isVertical = placement.Orientation == WallOrientation.Vertical;
                Vector2 targetSize = placement.Orientation == WallOrientation.Horizontal
                    ? new Vector2(spacing * 2f, thickness)
                    : new Vector2(thickness, spacing * 2f);
                transform.localRotation = !isVertical
                    ? Quaternion.identity
                    : Quaternion.Euler(0f, 0f, 90f);
                transform.localScale = CalculateScale(targetSize, isVertical);
            }
            else
            {
                transform.localRotation = Quaternion.identity;
                transform.localScale = placement.Orientation == WallOrientation.Horizontal
                    ? new Vector3(spacing * 2f, thickness, 1f)
                    : new Vector3(thickness, spacing * 2f, 1f);
            }
        }

        private void ApplyMaterial(Material material)
        {
            CacheRenderer();

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.color = Color.white;
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

        private Vector3 CalculateScale(Vector2 targetSize, bool isVertical)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return new Vector3(targetSize.x, targetSize.y, 1f);
            }

            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            float width = isVertical
                ? CalculateScaleAxis(targetSize.y, spriteSize.x)
                : CalculateScaleAxis(targetSize.x, spriteSize.x);
            float height = isVertical
                ? CalculateScaleAxis(targetSize.x, spriteSize.y)
                : CalculateScaleAxis(targetSize.y, spriteSize.y);
            return new Vector3(width, height, 1f);
        }

        private static float CalculateScaleAxis(float targetSize, float spriteSize)
        {
            return spriteSize > Mathf.Epsilon ? targetSize / spriteSize : targetSize;
        }
    }
}
