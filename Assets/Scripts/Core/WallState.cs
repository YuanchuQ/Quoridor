using System;
using System.Collections.Generic;

namespace Quoridor.Core
{
    /// <summary>
    /// Pure runtime state for placed walls, remaining wall counts, and board connectivity.
    /// </summary>
    public sealed class WallState
    {
        private readonly List<WallPlacement> placedWalls = new List<WallPlacement>();
        private readonly WallPlacementValidator validator;
        private int playerOneRemainingWalls;
        private int playerTwoRemainingWalls;

        /// <summary>
        /// Creates standard Quoridor wall state on a 9x9 board.
        /// </summary>
        public WallState()
            : this(QuoridorRules.BoardSize, QuoridorRules.InitialWallCount)
        {
        }

        /// <summary>
        /// Creates wall state for a board size and per-player starting wall count.
        /// </summary>
        public WallState(int boardSize, int initialWallCount)
            : this(BoardGraph.CreateOpenBoard(boardSize), initialWallCount, new WallPlacementValidator())
        {
        }

        /// <summary>
        /// Creates wall state with explicit graph and validator dependencies.
        /// </summary>
        public WallState(BoardGraph graph, int initialWallCount, WallPlacementValidator validator)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));

            if (initialWallCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialWallCount), "Initial wall count cannot be negative.");
            }

            playerOneRemainingWalls = initialWallCount;
            playerTwoRemainingWalls = initialWallCount;
        }

        /// <summary>
        /// Current board connectivity after all placed walls have been applied.
        /// </summary>
        public BoardGraph Graph { get; }

        /// <summary>
        /// Walls that have been successfully placed.
        /// </summary>
        public IReadOnlyList<WallPlacement> PlacedWalls => placedWalls;

        /// <summary>
        /// Returns the remaining wall count for a player.
        /// </summary>
        public int GetRemainingWalls(PlayerId playerId)
        {
            switch (playerId)
            {
                case PlayerId.PlayerOne:
                    return playerOneRemainingWalls;
                case PlayerId.PlayerTwo:
                    return playerTwoRemainingWalls;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerId), playerId, null);
            }
        }

        /// <summary>
        /// Validates whether a player can place a wall without mutating state.
        /// </summary>
        public WallValidationResult CanPlaceWall(
            PlayerId playerId,
            WallPlacement wall,
            BoardPosition playerOnePosition,
            BoardPosition playerTwoPosition)
        {
            if (GetRemainingWalls(playerId) <= 0)
            {
                return WallValidationResult.Invalid("Player has no remaining walls.");
            }

            return validator.Validate(wall, placedWalls, Graph, playerOnePosition, playerTwoPosition);
        }

        /// <summary>
        /// Attempts to place a wall, mutating wall state only when every validation step passes.
        /// </summary>
        public WallValidationResult TryPlaceWall(
            PlayerId playerId,
            WallPlacement wall,
            BoardPosition playerOnePosition,
            BoardPosition playerTwoPosition)
        {
            WallValidationResult result = CanPlaceWall(playerId, wall, playerOnePosition, playerTwoPosition);
            if (!result.IsValid)
            {
                return result;
            }

            placedWalls.Add(wall);
            Graph.ApplyWall(wall);
            DecrementRemainingWalls(playerId);
            return WallValidationResult.Valid;
        }

        private void DecrementRemainingWalls(PlayerId playerId)
        {
            switch (playerId)
            {
                case PlayerId.PlayerOne:
                    playerOneRemainingWalls--;
                    break;
                case PlayerId.PlayerTwo:
                    playerTwoRemainingWalls--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerId), playerId, null);
            }
        }
    }
}
