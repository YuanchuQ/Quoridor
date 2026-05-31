// Runs the complete wall validation pipeline in deterministic order
using System.Collections.Generic;

namespace Quoridor.Core
{
    /// <summary>
    /// Runs the complete wall validation pipeline in deterministic order.
    /// </summary>
    public sealed class WallPlacementValidator
    {
        private readonly WallBoundsValidator boundsValidator;
        private readonly WallOverlapValidator overlapValidator;
        private readonly WallCrossingValidator crossingValidator;
        private readonly PathValidator pathValidator;

        /// <summary>
        /// Creates a validator with the standard Quoridor validation pipeline.
        /// </summary>
        public WallPlacementValidator()
            : this(new WallBoundsValidator(), new WallOverlapValidator(), new WallCrossingValidator(), new PathValidator())
        {
        }

        /// <summary>
        /// Creates a validator with explicit validation step instances.
        /// </summary>
        public WallPlacementValidator(
            WallBoundsValidator boundsValidator,
            WallOverlapValidator overlapValidator,
            WallCrossingValidator crossingValidator,
            PathValidator pathValidator)
        {
            this.boundsValidator = boundsValidator;
            this.overlapValidator = overlapValidator;
            this.crossingValidator = crossingValidator;
            this.pathValidator = pathValidator;
        }

        /// <summary>
        /// Validates a candidate wall against bounds, overlap, crossing, and path reachability.
        /// </summary>
        public WallValidationResult Validate(
            WallPlacement candidate,
            IEnumerable<WallPlacement> existingWalls,
            BoardGraph currentGraph,
            BoardPosition playerOnePosition,
            BoardPosition playerTwoPosition)
        {
            WallValidationResult boundsResult = boundsValidator.Validate(candidate, currentGraph.Size);
            if (!boundsResult.IsValid)
            {
                return boundsResult;
            }

            WallValidationResult overlapResult = overlapValidator.Validate(candidate, existingWalls);
            if (!overlapResult.IsValid)
            {
                return overlapResult;
            }

            WallValidationResult crossingResult = crossingValidator.Validate(candidate, existingWalls);
            if (!crossingResult.IsValid)
            {
                return crossingResult;
            }

            return pathValidator.ValidateAfterWall(currentGraph, candidate, playerOnePosition, playerTwoPosition);
        }
    }
}
