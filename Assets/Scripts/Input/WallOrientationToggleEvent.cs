// Carries a requested wall orientation change from input into game-facing systems
using Quoridor.Core;

namespace Quoridor.Input
{
    /// <summary>
    /// Carries a requested wall orientation change from input into game-facing systems.
    /// </summary>
    public readonly struct WallOrientationToggleEvent
    {
        /// <summary>
        /// Creates a wall orientation toggle event.
        /// </summary>
        public WallOrientationToggleEvent(WallOrientation previousOrientation, WallOrientation nextOrientation)
        {
            PreviousOrientation = previousOrientation;
            NextOrientation = nextOrientation;
        }

        /// <summary>
        /// Wall orientation before the toggle was requested.
        /// </summary>
        public WallOrientation PreviousOrientation { get; }

        /// <summary>
        /// Wall orientation requested by the input event.
        /// </summary>
        public WallOrientation NextOrientation { get; }
    }
}
