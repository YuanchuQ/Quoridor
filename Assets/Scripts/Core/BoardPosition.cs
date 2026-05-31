// Immutable board coordinate measured in cell units from the bottom-left corner
using System;

namespace Quoridor.Core
{
    /// <summary>
    /// Immutable board coordinate measured in cell units from the bottom-left corner.
    /// </summary>
    public readonly struct BoardPosition : IEquatable<BoardPosition>
    {
        /// <summary>
        /// Creates a board position from integer coordinates.
        /// </summary>
        public BoardPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Horizontal board coordinate.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Vertical board coordinate.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Returns a neighboring position offset by the given delta.
        /// </summary>
        public BoardPosition Offset(int deltaX, int deltaY)
        {
            return new BoardPosition(X + deltaX, Y + deltaY);
        }

        /// <inheritdoc />
        public bool Equals(BoardPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is BoardPosition other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        /// <summary>
        /// Returns true when both positions have the same coordinates.
        /// </summary>
        public static bool operator ==(BoardPosition left, BoardPosition right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns true when the positions have different coordinates.
        /// </summary>
        public static bool operator !=(BoardPosition left, BoardPosition right)
        {
            return !left.Equals(right);
        }
    }
}
