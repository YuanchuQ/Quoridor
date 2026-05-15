using Mirror;

namespace Quoridor.Networking
{
    /// <summary>
    /// Client request to move the active pawn.
    /// </summary>
    public struct PawnMoveRequestMessage : NetworkMessage
    {
        /// <summary>
        /// Destination x coordinate.
        /// </summary>
        public int X;

        /// <summary>
        /// Destination y coordinate.
        /// </summary>
        public int Y;
    }

    /// <summary>
    /// Authoritative pawn move broadcast from the server.
    /// </summary>
    public struct PawnMoveAppliedMessage : NetworkMessage
    {
        /// <summary>
        /// Player index as defined by PlayerId.
        /// </summary>
        public int PlayerId;

        /// <summary>
        /// Source x coordinate.
        /// </summary>
        public int FromX;

        /// <summary>
        /// Source y coordinate.
        /// </summary>
        public int FromY;

        /// <summary>
        /// Destination x coordinate.
        /// </summary>
        public int ToX;

        /// <summary>
        /// Destination y coordinate.
        /// </summary>
        public int ToY;
    }

    /// <summary>
    /// Client request to place a wall.
    /// </summary>
    public struct WallPlaceRequestMessage : NetworkMessage
    {
        /// <summary>
        /// Anchor x coordinate.
        /// </summary>
        public int X;

        /// <summary>
        /// Anchor y coordinate.
        /// </summary>
        public int Y;

        /// <summary>
        /// Wall orientation as defined by WallOrientation.
        /// </summary>
        public int Orientation;
    }

    /// <summary>
    /// Authoritative wall placement broadcast from the server.
    /// </summary>
    public struct WallPlaceAppliedMessage : NetworkMessage
    {
        /// <summary>
        /// Player index as defined by PlayerId.
        /// </summary>
        public int PlayerId;

        /// <summary>
        /// Anchor x coordinate.
        /// </summary>
        public int X;

        /// <summary>
        /// Anchor y coordinate.
        /// </summary>
        public int Y;

        /// <summary>
        /// Wall orientation as defined by WallOrientation.
        /// </summary>
        public int Orientation;

        /// <summary>
        /// Walls remaining after placement.
        /// </summary>
        public int RemainingWalls;
    }
}
