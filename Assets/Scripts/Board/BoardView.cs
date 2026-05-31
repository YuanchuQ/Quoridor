// Scene-facing board view that indexes edit-time-created cell objects
using System;
using System.Collections.Generic;
using Quoridor.Config;
using Quoridor.Core;
using UnityEngine;

namespace Quoridor.Board
{
    /// <summary>
    /// Scene-facing board view that indexes edit-time-created cell objects.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [Tooltip("Rules and layout values used by this board")]
        [SerializeField] private GameConfig config;
        [Tooltip("Edit-time generated cells indexed by board coordinate")]
        [SerializeField] private CellView[] cells = Array.Empty<CellView>();

        private readonly Dictionary<BoardPosition, CellView> cellLookup = new();

        /// <summary>
        /// Configuration asset used by this board view.
        /// </summary>
        public GameConfig Config => config;

        /// <summary>
        /// Cells currently registered with the board view.
        /// </summary>
        public IReadOnlyList<CellView> Cells => cells;

        /// <summary>
        /// Replaces the serialized cell list and rebuilds the coordinate lookup.
        /// </summary>
        public void SetCells(CellView[] newCells)
        {
            cells = newCells ?? Array.Empty<CellView>();
            SortCells();
            RebuildLookup();
        }

        /// <summary>
        /// Adds a cell to the serialized list when it is not already registered.
        /// </summary>
        public void RegisterCell(CellView cell)
        {
            if (cell == null || ContainsCell(cell))
            {
                return;
            }

            var nextCells = new CellView[cells.Length + 1];
            Array.Copy(cells, nextCells, cells.Length);
            nextCells[^1] = cell;
            SetCells(nextCells);
        }

        /// <summary>
        /// Returns true when a cell exists at the requested board position.
        /// </summary>
        public bool TryGetCell(BoardPosition position, out CellView cell)
        {
            EnsureLookup();
            return cellLookup.TryGetValue(position, out cell);
        }

        /// <summary>
        /// Returns the cell at the requested board position.
        /// </summary>
        public CellView GetCell(BoardPosition position)
        {
            if (TryGetCell(position, out CellView cell))
            {
                return cell;
            }

            throw new KeyNotFoundException($"No board cell is registered at {position}.");
        }

        /// <summary>
        /// Clears hover and movement hint visuals on every registered cell.
        /// </summary>
        public void ClearHighlights()
        {
            foreach (CellView cell in cells)
            {
                if (cell != null)
                {
                    cell.SetDefault();
                }
            }
        }

        private void Awake()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            SortCells();
            RebuildLookup();
        }

        private void EnsureLookup()
        {
            if (cellLookup.Count != cells.Length)
            {
                RebuildLookup();
            }
        }

        private void RebuildLookup()
        {
            cellLookup.Clear();

            foreach (CellView cell in cells)
            {
                if (cell == null)
                {
                    continue;
                }

                BoardPosition position = cell.Position;
                if (!IsInsideBoard(position))
                {
                    Debug.LogWarning($"Cell {cell.name} has out-of-board coordinate {position}.", cell);
                    continue;
                }

                if (cellLookup.ContainsKey(position))
                {
                    Debug.LogWarning($"Duplicate board cell coordinate {position} on {cell.name}.", cell);
                    continue;
                }

                cellLookup.Add(position, cell);
            }
        }

        private void SortCells()
        {
            Array.Sort(cells, CompareCells);
        }

        private int CompareCells(CellView left, CellView right)
        {
            if (left == right)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            int yComparison = left.Position.Y.CompareTo(right.Position.Y);
            return yComparison != 0 ? yComparison : left.Position.X.CompareTo(right.Position.X);
        }

        private bool ContainsCell(CellView target)
        {
            foreach (CellView cell in cells)
            {
                if (cell == target)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInsideBoard(BoardPosition position)
        {
            int boardSize = config != null ? config.BoardSize : QuoridorRules.BoardSize;
            return position.X >= 0 && position.X < boardSize && position.Y >= 0 && position.Y < boardSize;
        }
    }
}
