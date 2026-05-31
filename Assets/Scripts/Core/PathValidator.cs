// Validates goal-row reachability with breadth-first search
using System;
using System.Collections.Generic;

namespace Quoridor.Core
{
    /// <summary>
    /// Validates goal-row reachability with breadth-first search.
    /// </summary>
    public sealed class PathValidator
    {
        /// <summary>
        /// Checks whether both players still have a path after applying the candidate wall.
        /// </summary>
        public WallValidationResult ValidateAfterWall(
            BoardGraph currentGraph,
            WallPlacement candidate,
            BoardPosition playerOnePosition,
            BoardPosition playerTwoPosition)
        {
            BoardGraph simulatedGraph = currentGraph.Clone();
            simulatedGraph.ApplyWall(candidate);

            if (!HasPathToGoal(simulatedGraph, playerOnePosition, PlayerId.PlayerOne))
            {
                return WallValidationResult.Invalid("Wall blocks player one from reaching the goal row.");
            }

            if (!HasPathToGoal(simulatedGraph, playerTwoPosition, PlayerId.PlayerTwo))
            {
                return WallValidationResult.Invalid("Wall blocks player two from reaching the goal row.");
            }

            return WallValidationResult.Valid;
        }

        /// <summary>
        /// Returns true when a player can reach their target row in the graph.
        /// </summary>
        public bool HasPathToGoal(BoardGraph graph, BoardPosition start, PlayerId playerId)
        {
            if (!graph.Contains(start))
            {
                return false;
            }

            var visited = new HashSet<BoardPosition> { start };
            var queue = new Queue<BoardPosition>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                BoardPosition current = queue.Dequeue();
                if (IsGoalRow(current, graph.Size, playerId))
                {
                    return true;
                }

                foreach (BoardPosition neighbor in graph.GetNeighbors(current))
                {
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        private static bool IsGoalRow(BoardPosition position, int boardSize, PlayerId playerId)
        {
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
