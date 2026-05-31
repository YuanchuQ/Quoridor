// Shared tunable configuration for a local Quoridor match
using UnityEngine;

namespace Quoridor.Config
{
    /// <summary>
    /// Shared tunable configuration for a local Quoridor match.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Quoridor/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        public const int DefaultBoardSize = 9;
        public const int DefaultInitialWallCount = 10;

        [Header("Rules")]
        [Tooltip("Number of cells on each side of the board")]
        [SerializeField, Min(2)] private int boardSize = DefaultBoardSize;
        [Tooltip("Number of walls each player starts with")]
        [SerializeField, Min(0)] private int initialWallCount = DefaultInitialWallCount;
        [Tooltip("Starting coordinate for player one")]
        [SerializeField] private Vector2Int playerOneStart = new(4, 0);
        [Tooltip("Starting coordinate for player two")]
        [SerializeField] private Vector2Int playerTwoStart = new(4, 8);

        [Header("Board Layout")]
        [Tooltip("World-space spacing between neighboring cell centers")]
        [SerializeField, Min(0.1f)] private float cellSpacing = 1f;
        [Tooltip("World-space visual size of a board cell")]
        [SerializeField, Min(0.1f)] private float cellSize = 0.86f;
        [Tooltip("World-space thickness used for wall visuals")]
        [SerializeField, Min(0.01f)] private float wallThickness = 0.14f;
        [Tooltip("Duration of pawn movement animation in seconds")]
        [SerializeField, Min(0.1f)] private float pawnMoveDuration = 0.18f;

        /// <summary>
        /// Number of cells along one side of the square board.
        /// </summary>
        public int BoardSize => boardSize;

        /// <summary>
        /// Number of walls each player receives at the start of the match.
        /// </summary>
        public int InitialWallCount => initialWallCount;

        /// <summary>
        /// Starting board coordinate for player one.
        /// </summary>
        public Vector2Int PlayerOneStart => playerOneStart;

        /// <summary>
        /// Starting board coordinate for player two.
        /// </summary>
        public Vector2Int PlayerTwoStart => playerTwoStart;

        /// <summary>
        /// World-space distance between neighboring cell centers.
        /// </summary>
        public float CellSpacing => cellSpacing;

        /// <summary>
        /// World-space visual size of each cell.
        /// </summary>
        public float CellSize => cellSize;

        /// <summary>
        /// World-space visual thickness of wall segments.
        /// </summary>
        public float WallThickness => wallThickness;

        /// <summary>
        /// Duration in seconds for pawn movement animation.
        /// </summary>
        public float PawnMoveDuration => pawnMoveDuration;
    }
}
