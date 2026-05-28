// High-level intent currently requested by local player input
namespace Quoridor.Input
{
    /// <summary>
    /// High-level intent currently requested by local player input.
    /// </summary>
    public enum InputMode
    {
        /// <summary>
        /// Cell selection is routed as pawn movement input.
        /// </summary>
        PawnMove = 0,

        /// <summary>
        /// Cell selection is routed as wall placement input.
        /// </summary>
        WallPlacement = 1
    }
}
