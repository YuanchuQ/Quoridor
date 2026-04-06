using System;

namespace Quoridor.Core
{
    /// <summary>
    /// Undirected edge between two adjacent board positions.
    /// </summary>
    public readonly struct BoardEdge : IEquatable<BoardEdge>
    {
        /// <summary>
        /// Creates an undirected edge from two endpoints.
        /// </summary>
        public BoardEdge(BoardPosition a, BoardPosition b)
        {
            A = a;
            B = b;
        }

        /// <summary>
        /// First endpoint of the edge.
        /// </summary>
        public BoardPosition A { get; }

        /// <summary>
        /// Second endpoint of the edge.
        /// </summary>
        public BoardPosition B { get; }

        /// <inheritdoc />
        public bool Equals(BoardEdge other)
        {
            return (A == other.A && B == other.B) || (A == other.B && B == other.A);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is BoardEdge other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int first = A.GetHashCode();
                int second = B.GetHashCode();
                return first < second ? (first * 397) ^ second : (second * 397) ^ first;
            }
        }
    }
}
