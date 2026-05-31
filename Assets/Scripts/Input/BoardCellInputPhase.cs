// Describes the pointer action reported for a board cell
namespace Quoridor.Input
{
    /// <summary>
    /// Describes the pointer action reported for a board cell.
    /// </summary>
    public enum BoardCellInputPhase
    {
        /// <summary>
        /// The pointer entered a board cell.
        /// </summary>
        HoverEntered = 0,

        /// <summary>
        /// The pointer exited a board cell.
        /// </summary>
        HoverExited = 1,

        /// <summary>
        /// A board cell was selected.
        /// </summary>
        Selected = 2
    }
}
