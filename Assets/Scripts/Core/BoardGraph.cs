using System;
using System.Collections.Generic;

namespace Quoridor.Core
{
    /// <summary>
    /// Mutable adjacency graph for Quoridor cell connectivity.
    /// </summary>
    public sealed class BoardGraph
    {
        private readonly Dictionary<BoardPosition, HashSet<BoardPosition>> adjacency;

        private BoardGraph(int size, Dictionary<BoardPosition, HashSet<BoardPosition>> adjacency)
        {
            Size = size;
            this.adjacency = adjacency;
        }

        /// <summary>
        /// Number of cells along one side of the square board.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Creates a full board graph with all orthogonal cell edges open.
        /// </summary>
        public static BoardGraph CreateOpenBoard(int size)
        {
            if (size < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Board size must be at least 2.");
            }

            var adjacency = new Dictionary<BoardPosition, HashSet<BoardPosition>>();
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var position = new BoardPosition(x, y);
                    adjacency[position] = new HashSet<BoardPosition>();
                }
            }

            var graph = new BoardGraph(size, adjacency);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var position = new BoardPosition(x, y);
                    graph.TryConnect(position, position.Offset(1, 0));
                    graph.TryConnect(position, position.Offset(0, 1));
                }
            }

            return graph;
        }

        /// <summary>
        /// Creates a deep copy of this graph.
        /// </summary>
        public BoardGraph Clone()
        {
            var copy = new Dictionary<BoardPosition, HashSet<BoardPosition>>();
            foreach (KeyValuePair<BoardPosition, HashSet<BoardPosition>> pair in adjacency)
            {
                copy[pair.Key] = new HashSet<BoardPosition>(pair.Value);
            }

            return new BoardGraph(Size, copy);
        }

        /// <summary>
        /// Returns true when the position is inside the board.
        /// </summary>
        public bool Contains(BoardPosition position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < Size && position.Y < Size;
        }

        /// <summary>
        /// Returns open neighboring cells for the supplied position.
        /// </summary>
        public IReadOnlyCollection<BoardPosition> GetNeighbors(BoardPosition position)
        {
            if (!adjacency.TryGetValue(position, out HashSet<BoardPosition> neighbors))
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is outside the board.");
            }

            return neighbors;
        }

        /// <summary>
        /// Returns true when two cells are currently connected by an open edge.
        /// </summary>
        public bool AreConnected(BoardPosition a, BoardPosition b)
        {
            return adjacency.TryGetValue(a, out HashSet<BoardPosition> neighbors) && neighbors.Contains(b);
        }

        /// <summary>
        /// Applies a wall to this graph by removing the two blocked edges.
        /// </summary>
        public void ApplyWall(WallPlacement wall)
        {
            foreach (BoardEdge edge in wall.GetBlockedEdges())
            {
                Disconnect(edge.A, edge.B);
            }
        }

        private void TryConnect(BoardPosition a, BoardPosition b)
        {
            if (!Contains(a) || !Contains(b))
            {
                return;
            }

            adjacency[a].Add(b);
            adjacency[b].Add(a);
        }

        private void Disconnect(BoardPosition a, BoardPosition b)
        {
            if (!Contains(a) || !Contains(b))
            {
                return;
            }

            adjacency[a].Remove(b);
            adjacency[b].Remove(a);
        }
    }
}
