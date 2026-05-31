// Describes a successfully placed wall
using Quoridor.Core;

namespace Quoridor.Wall
{
    /// <summary>
    /// Describes a successfully placed wall.
    /// </summary>
    public readonly struct WallPlacedEvent
    {
        /// <summary>
        /// Creates wall placement event data.
        /// </summary>
        public WallPlacedEvent(PlayerId playerId, WallPlacement placement, int remainingWalls)
        {
            PlayerId = playerId;
            Placement = placement;
            RemainingWalls = remainingWalls;
        }

        /// <summary>
        /// Player that placed the wall.
        /// </summary>
        public PlayerId PlayerId { get; }

        /// <summary>
        /// Wall that was placed.
        /// </summary>
        public WallPlacement Placement { get; }

        /// <summary>
        /// Walls remaining for the player after placement.
        /// </summary>
        public int RemainingWalls { get; }
    }
}
