using System;
using System.Collections.Generic;

namespace Quoridor.Core
{
    /// <summary>
    /// Immutable placement of a two-cell wall, anchored at the lower-left wall slot.
    /// </summary>
    public readonly struct WallPlacement : IEquatable<WallPlacement>
    {
        /// <summary>
        /// Creates a wall placement at the supplied anchor and orientation.
        /// </summary>
        public WallPlacement(BoardPosition anchor, WallOrientation orientation)
        {
            Anchor = anchor;
            Orientation = orientation;
        }

        /// <summary>
        /// Lower-left wall-slot coordinate for this placement.
        /// </summary>
        public BoardPosition Anchor { get; }

        /// <summary>
        /// Orientation of the wall.
        /// </summary>
        public WallOrientation Orientation { get; }

        /// <summary>
        /// Returns true when this wall physically crosses the supplied wall.
        /// </summary>
        public bool Crosses(WallPlacement other)
        {
            return Orientation != other.Orientation && Anchor == other.Anchor;
        }

        /// <summary>
        /// Returns true when this wall occupies the same wall line as the supplied wall.
        /// </summary>
        public bool Overlaps(WallPlacement other)
        {
            if (Orientation != other.Orientation)
            {
                return false;
            }

            if (Orientation == WallOrientation.Horizontal)
            {
                return Anchor.Y == other.Anchor.Y && Math.Abs(Anchor.X - other.Anchor.X) < 2;
            }

            return Anchor.X == other.Anchor.X && Math.Abs(Anchor.Y - other.Anchor.Y) < 2;
        }

        /// <summary>
        /// Enumerates the board graph edges blocked by this wall.
        /// </summary>
        public IEnumerable<BoardEdge> GetBlockedEdges()
        {
            if (Orientation == WallOrientation.Horizontal)
            {
                yield return new BoardEdge(Anchor, Anchor.Offset(0, 1));
                yield return new BoardEdge(Anchor.Offset(1, 0), Anchor.Offset(1, 1));
                yield break;
            }

            yield return new BoardEdge(Anchor, Anchor.Offset(1, 0));
            yield return new BoardEdge(Anchor.Offset(0, 1), Anchor.Offset(1, 1));
        }

        /// <inheritdoc />
        public bool Equals(WallPlacement other)
        {
            return Anchor == other.Anchor && Orientation == other.Orientation;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is WallPlacement other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Anchor.GetHashCode() * 397) ^ (int)Orientation;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Orientation} wall at {Anchor}";
        }
    }
}
