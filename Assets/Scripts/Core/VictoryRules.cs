// Evaluates local Quoridor victory conditions
using System;

namespace Quoridor.Core
{
    /// <summary>
    /// Evaluates local Quoridor victory conditions.
    /// </summary>
    public static class VictoryRules
    {
        /// <summary>
        /// Returns true when the player's pawn has reached their goal row.
        /// </summary>
        public static bool HasPlayerWon(PlayerId playerId, BoardPosition position, int boardSize)
        {
            if (boardSize < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(boardSize), "Board size must be at least 2.");
            }

            switch (playerId)
            {
                case PlayerId.PlayerOne:
                    return position.Y == boardSize - 1;
                case PlayerId.PlayerTwo:
                    return position.Y == 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerId), playerId, null);
            }
        }
    }
}
