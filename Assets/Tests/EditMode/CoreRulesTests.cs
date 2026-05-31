// Covers board graph and wall validator rules in edit mode tests
using System.Collections.Generic;
using NUnit.Framework;
using Quoridor.Core;

namespace Quoridor.Tests.EditMode
{
    /// <summary>
    /// Covers board graph and wall validator rules in edit mode tests.
    /// </summary>
    public sealed class CoreRulesTests
    {
        [Test]
        public void OpenBoard_ConnectsOrthogonalNeighbors()
        {
            BoardGraph graph = BoardGraph.CreateOpenBoard(QuoridorRules.BoardSize);
            var center = new BoardPosition(4, 4);

            Assert.That(graph.GetNeighbors(center), Has.Count.EqualTo(4));
            Assert.That(graph.AreConnected(center, center.Offset(1, 0)), Is.True);
            Assert.That(graph.AreConnected(center, center.Offset(0, 1)), Is.True);
        }

        [Test]
        public void ApplyHorizontalWall_RemovesTwoVerticalEdges()
        {
            BoardGraph graph = BoardGraph.CreateOpenBoard(QuoridorRules.BoardSize);
            var wall = new WallPlacement(new BoardPosition(3, 3), WallOrientation.Horizontal);

            graph.ApplyWall(wall);

            Assert.That(graph.AreConnected(new BoardPosition(3, 3), new BoardPosition(3, 4)), Is.False);
            Assert.That(graph.AreConnected(new BoardPosition(4, 3), new BoardPosition(4, 4)), Is.False);
            Assert.That(graph.AreConnected(new BoardPosition(2, 3), new BoardPosition(2, 4)), Is.True);
        }

        [Test]
        public void BoundsValidator_RejectsAnchorOutsideWallSlots()
        {
            var validator = new WallBoundsValidator();
            var wall = new WallPlacement(new BoardPosition(8, 0), WallOrientation.Horizontal);

            WallValidationResult result = validator.Validate(wall, QuoridorRules.BoardSize);

            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void OverlapValidator_RejectsAdjacentSameLineWall()
        {
            var validator = new WallOverlapValidator();
            var existing = new[] { new WallPlacement(new BoardPosition(3, 3), WallOrientation.Horizontal) };
            var candidate = new WallPlacement(new BoardPosition(4, 3), WallOrientation.Horizontal);

            WallValidationResult result = validator.Validate(candidate, existing);

            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void CrossingValidator_RejectsOppositeWallAtSameAnchor()
        {
            var validator = new WallCrossingValidator();
            var existing = new[] { new WallPlacement(new BoardPosition(3, 3), WallOrientation.Horizontal) };
            var candidate = new WallPlacement(new BoardPosition(3, 3), WallOrientation.Vertical);

            WallValidationResult result = validator.Validate(candidate, existing);

            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void PathValidator_AllowsPathOnOpenBoard()
        {
            BoardGraph graph = BoardGraph.CreateOpenBoard(QuoridorRules.BoardSize);
            var validator = new PathValidator();

            Assert.That(validator.HasPathToGoal(graph, QuoridorRules.PlayerOneStart, PlayerId.PlayerOne), Is.True);
            Assert.That(validator.HasPathToGoal(graph, QuoridorRules.PlayerTwoStart, PlayerId.PlayerTwo), Is.True);
        }

        [Test]
        public void WallPlacementValidator_RejectsWallThatClosesOnlyRemainingGap()
        {
            BoardGraph graph = BoardGraph.CreateOpenBoard(QuoridorRules.BoardSize);
            var existingWalls = new List<WallPlacement>();

            int[] blockedAnchors = { 0, 2, 4 };
            foreach (int x in blockedAnchors)
            {
                WallPlacement wall = new WallPlacement(new BoardPosition(x, 0), WallOrientation.Horizontal);
                existingWalls.Add(wall);
                graph.ApplyWall(wall);
            }

            WallPlacement sideBlocker = new WallPlacement(new BoardPosition(7, 0), WallOrientation.Vertical);
            existingWalls.Add(sideBlocker);
            graph.ApplyWall(sideBlocker);

            var candidate = new WallPlacement(new BoardPosition(6, 0), WallOrientation.Horizontal);
            var validator = new WallPlacementValidator();

            WallValidationResult result = validator.Validate(
                candidate,
                existingWalls,
                graph,
                QuoridorRules.PlayerOneStart,
                QuoridorRules.PlayerTwoStart);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Reason, Does.Contain("player one"));
        }
    }
}
