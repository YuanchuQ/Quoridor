// Describes a completed pawn movement
using Quoridor.Core;

namespace Quoridor.Pawn
{
    /// <summary>
    /// Describes a completed pawn movement.
    /// </summary>
    public readonly struct PawnMoveEvent
    {
        /// <summary>
        /// Creates a completed pawn movement event.
        /// </summary>
        public PawnMoveEvent(PlayerId playerId, BoardPosition from, BoardPosition to)
        {
            PlayerId = playerId;
            From = from;
            To = to;
        }

        /// <summary>
        /// Player that moved.
        /// </summary>
        public PlayerId PlayerId { get; }

        /// <summary>
        /// Board coordinate before movement.
        /// </summary>
        public BoardPosition From { get; }

        /// <summary>
        /// Board coordinate after movement.
        /// </summary>
        public BoardPosition To { get; }
    }
}
