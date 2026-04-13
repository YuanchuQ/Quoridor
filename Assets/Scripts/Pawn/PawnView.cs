using System.Collections;
using Quoridor.Board;
using Quoridor.Core;
using UnityEngine;

namespace Quoridor.Pawn
{
    /// <summary>
    /// Scene-facing pawn view that owns visual position and movement animation.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PawnView : MonoBehaviour
    {
        [SerializeField] private PlayerId playerId;
        [SerializeField] private BoardView boardView;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float moveDuration = 0.18f;

        private Coroutine moveRoutine;

        /// <summary>
        /// Player represented by this pawn.
        /// </summary>
        public PlayerId PlayerId => playerId;

        /// <summary>
        /// Current board coordinate occupied by this pawn.
        /// </summary>
        public BoardPosition Position { get; private set; }

        /// <summary>
        /// True while a movement animation is active.
        /// </summary>
        public bool IsMoving => moveRoutine != null;

        /// <summary>
        /// Assigns this pawn's player, board and starting position.
        /// </summary>
        public void Configure(PlayerId nextPlayerId, BoardView nextBoardView, BoardPosition startPosition, float nextMoveDuration)
        {
            playerId = nextPlayerId;
            boardView = nextBoardView;
            moveDuration = Mathf.Max(0.01f, nextMoveDuration);
            SnapTo(startPosition);
        }

        /// <summary>
        /// Immediately places the pawn at a board coordinate.
        /// </summary>
        public void SnapTo(BoardPosition position)
        {
            Position = position;
            transform.position = GetWorldPosition(position);
        }

        /// <summary>
        /// Animates the pawn to a board coordinate.
        /// </summary>
        public void MoveTo(BoardPosition position)
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }

            Position = position;
            moveRoutine = StartCoroutine(MoveRoutine(position));
        }

        /// <summary>
        /// Assigns the material used by this pawn renderer.
        /// </summary>
        public void SetMaterial(Material material)
        {
            CacheRenderer();

            if (spriteRenderer != null && material != null)
            {
                spriteRenderer.sharedMaterial = material;
            }
        }

        private void Reset()
        {
            CacheRenderer();
        }

        private void OnValidate()
        {
            CacheRenderer();
            moveDuration = Mathf.Max(0.01f, moveDuration);
        }

        private IEnumerator MoveRoutine(BoardPosition targetPosition)
        {
            Vector3 start = transform.position;
            Vector3 end = GetWorldPosition(targetPosition);
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / moveDuration);
                transform.position = Vector3.Lerp(start, end, progress);
                yield return null;
            }

            transform.position = end;
            moveRoutine = null;
        }

        private Vector3 GetWorldPosition(BoardPosition position)
        {
            if (boardView != null && boardView.TryGetCell(position, out CellView cell))
            {
                Vector3 cellPosition = cell.transform.position;
                return new Vector3(cellPosition.x, cellPosition.y, transform.position.z);
            }

            return transform.position;
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
