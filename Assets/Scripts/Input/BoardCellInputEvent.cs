using Quoridor.Board;
using Quoridor.Core;

namespace Quoridor.Input
{
    /// <summary>
    /// Input payload for pointer interaction with a board cell.
    /// </summary>
    public readonly struct BoardCellInputEvent
    {
        /// <summary>
        /// Creates input data for a board cell interaction.
        /// </summary>
        public BoardCellInputEvent(BoardPosition position, BoardCellInputPhase phase, InputMode mode, CellView cellView = null)
        {
            Position = position;
            Phase = phase;
            CellView = cellView;
            Mode = mode;
        }

        /// <summary>
        /// Board coordinate targeted by the input.
        /// </summary>
        public BoardPosition Position { get; }

        /// <summary>
        /// Pointer action that produced this board cell input event.
        /// </summary>
        public BoardCellInputPhase Phase { get; }

        /// <summary>
        /// Scene view that raised the input event.
        /// </summary>
        public CellView CellView { get; }

        /// <summary>
        /// Input mode active when the event was raised.
        /// </summary>
        public InputMode Mode { get; }
    }
}
