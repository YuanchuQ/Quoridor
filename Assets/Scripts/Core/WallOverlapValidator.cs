using System.Collections.Generic;

namespace Quoridor.Core
{
    /// <summary>
    /// Validates whether a new wall overlaps an existing wall on the same line.
    /// </summary>
    public sealed class WallOverlapValidator
    {
        /// <summary>
        /// Checks that the candidate wall does not overlap existing walls.
        /// </summary>
        public WallValidationResult Validate(WallPlacement candidate, IEnumerable<WallPlacement> existingWalls)
        {
            foreach (WallPlacement existing in existingWalls)
            {
                if (candidate.Overlaps(existing))
                {
                    return WallValidationResult.Invalid("Wall overlaps an existing wall.");
                }
            }

            return WallValidationResult.Valid;
        }
    }
}
