namespace Quoridor.Core
{
    /// <summary>
    /// Central constants for the v0.1.0 local Quoridor ruleset.
    /// </summary>
    public static class QuoridorRules
    {
        /// <summary>
        /// Standard Quoridor board width and height.
        /// </summary>
        public const int BoardSize = 9;

        /// <summary>
        /// Standard number of walls each player starts with.
        /// </summary>
        public const int InitialWallCount = 10;

        /// <summary>
        /// Starting cell for player one.
        /// </summary>
        public static BoardPosition PlayerOneStart { get; } = new BoardPosition(4, 0);

        /// <summary>
        /// Starting cell for player two.
        /// </summary>
        public static BoardPosition PlayerTwoStart { get; } = new BoardPosition(4, 8);
    }
}
