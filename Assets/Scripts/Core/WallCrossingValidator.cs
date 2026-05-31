// Validates whether a new wall crosses an existing wall of the opposite orientation
using System.Collections.Generic;

namespace Quoridor.Core
{
    /// <summary>
    /// Validates whether a new wall crosses an existing wall of the opposite orientation.
    /// </summary>
    public sealed class WallCrossingValidator
    {
        /// <summary>
        /// Checks that the candidate wall does not cross an existing wall.
        /// </summary>
        public WallValidationResult Validate(WallPlacement candidate, IEnumerable<WallPlacement> existingWalls)
        {
            foreach (WallPlacement existing in existingWalls)
            {
                if (candidate.Crosses(existing))
                {
                    return WallValidationResult.Invalid("Wall crosses an existing wall.");
                }
            }

            return WallValidationResult.Valid;
        }
    }
}
