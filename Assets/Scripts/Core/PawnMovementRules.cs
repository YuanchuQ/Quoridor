// Calculates legal pawn destinations from the current board graph and opponent position
using System;
using System.Collections.Generic;

namespace Quoridor.Core
{
    /// <summary>
    /// Calculates legal pawn destinations from the current board graph and opponent position.
    /// </summary>
    public static class PawnMovementRules
    {
        /// <summary>
        /// Returns all legal destinations for a pawn using standard Quoridor jump rules.
        /// </summary>
        public static IReadOnlyList<BoardPosition> GetLegalMoves(
            BoardGraph graph,
            BoardPosition currentPosition,
            BoardPosition opponentPosition)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (!graph.Contains(currentPosition))
            {
                throw new ArgumentOutOfRangeException(nameof(currentPosition), "Pawn position is outside the board.");
            }

            if (!graph.Contains(opponentPosition))
            {
                throw new ArgumentOutOfRangeException(nameof(opponentPosition), "Opponent position is outside the board.");
            }

            var moves = new List<BoardPosition>();
            foreach (BoardPosition neighbor in graph.GetNeighbors(currentPosition))
            {
                if (neighbor != opponentPosition)
                {
                    AddUnique(moves, neighbor);
                    continue;
                }

                AddOpponentJumpMoves(graph, moves, currentPosition, opponentPosition);
            }

            return moves;
        }

        private static void AddOpponentJumpMoves(
            BoardGraph graph,
            List<BoardPosition> moves,
            BoardPosition currentPosition,
            BoardPosition opponentPosition)
        {
            int deltaX = opponentPosition.X - currentPosition.X;
            int deltaY = opponentPosition.Y - currentPosition.Y;
            BoardPosition jumpPosition = opponentPosition.Offset(deltaX, deltaY);

            if (graph.Contains(jumpPosition) && graph.AreConnected(opponentPosition, jumpPosition))
            {
                AddUnique(moves, jumpPosition);
                return;
            }

            foreach (BoardPosition opponentNeighbor in graph.GetNeighbors(opponentPosition))
            {
                if (opponentNeighbor == currentPosition || opponentNeighbor == jumpPosition)
                {
                    continue;
                }

                AddUnique(moves, opponentNeighbor);
            }
        }

        private static void AddUnique(List<BoardPosition> moves, BoardPosition position)
        {
            if (!moves.Contains(position))
            {
                moves.Add(position);
            }
        }
    }
}
