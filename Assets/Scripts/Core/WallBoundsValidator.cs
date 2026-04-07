namespace Quoridor.Core
{
    /// <summary>
    /// Validates whether a wall anchor fits within the board's wall slot bounds.
    /// </summary>
    public sealed class WallBoundsValidator
    {
        /// <summary>
        /// Checks that a two-cell wall can fit at its anchor on the board.
        /// </summary>
        public WallValidationResult Validate(WallPlacement wall, int boardSize)
        {
            if (boardSize < 2)
            {
                return WallValidationResult.Invalid("Board size must be at least 2.");
            }

            int maxAnchor = boardSize - 2;
            if (wall.Anchor.X < 0 || wall.Anchor.Y < 0 || wall.Anchor.X > maxAnchor || wall.Anchor.Y > maxAnchor)
            {
                return WallValidationResult.Invalid("Wall anchor is outside valid wall-slot bounds.");
            }

            return WallValidationResult.Valid;
        }
    }
}
