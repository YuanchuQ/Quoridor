using NUnit.Framework;
using Quoridor.Core;

namespace Quoridor.Tests.EditMode
{
    public sealed class WallStateTests
    {
        [Test]
        public void TryPlaceWall_WhenValid_AddsWallBlocksEdgesAndConsumesOnlyCurrentPlayersWall()
        {
            var state = new WallState();
            var wall = new WallPlacement(new BoardPosition(3, 3), WallOrientation.Horizontal);

            WallValidationResult result = state.TryPlaceWall(
                PlayerId.PlayerOne,
                wall,
                QuoridorRules.PlayerOneStart,
                QuoridorRules.PlayerTwoStart);

            Assert.That(result.IsValid, Is.True);
            Assert.That(state.PlacedWalls, Has.Count.EqualTo(1));
            Assert.That(state.PlacedWalls[0], Is.EqualTo(wall));
            Assert.That(state.GetRemainingWalls(PlayerId.PlayerOne), Is.EqualTo(QuoridorRules.InitialWallCount - 1));
            Assert.That(state.GetRemainingWalls(PlayerId.PlayerTwo), Is.EqualTo(QuoridorRules.InitialWallCount));
            Assert.That(state.Graph.AreConnected(new BoardPosition(3, 3), new BoardPosition(3, 4)), Is.False);
            Assert.That(state.Graph.AreConnected(new BoardPosition(4, 3), new BoardPosition(4, 4)), Is.False);
        }

        [Test]
        public void CanPlaceWall_WhenValid_DoesNotMutateState()
        {
            var state = new WallState();
            var wall = new WallPlacement(new BoardPosition(2, 2), WallOrientation.Vertical);

            WallValidationResult result = state.CanPlaceWall(
                PlayerId.PlayerTwo,
                wall,
                QuoridorRules.PlayerOneStart,
                QuoridorRules.PlayerTwoStart);

            Assert.That(result.IsValid, Is.True);
            Assert.That(state.PlacedWalls, Is.Empty);
            Assert.That(state.GetRemainingWalls(PlayerId.PlayerTwo), Is.EqualTo(QuoridorRules.InitialWallCount));
            Assert.That(state.Graph.AreConnected(new BoardPosition(2, 2), new BoardPosition(3, 2)), Is.True);
            Assert.That(state.Graph.AreConnected(new BoardPosition(2, 3), new BoardPosition(3, 3)), Is.True);
        }

        [Test]
        public void TryPlaceWall_WhenPlayerHasNoWalls_RejectsWithoutMutatingState()
        {
            var state = new WallState(QuoridorRules.BoardSize, 0);
            var wall = new WallPlacement(new BoardPosition(3, 3), WallOrientation.Horizontal);

            WallValidationResult result = state.TryPlaceWall(
                PlayerId.PlayerOne,
                wall,
                QuoridorRules.PlayerOneStart,
                QuoridorRules.PlayerTwoStart);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Reason, Does.Contain("no remaining walls"));
            Assert.That(state.PlacedWalls, Is.Empty);
            Assert.That(state.Graph.AreConnected(new BoardPosition(3, 3), new BoardPosition(3, 4)), Is.True);
            Assert.That(state.GetRemainingWalls(PlayerId.PlayerOne), Is.Zero);
        }

        [Test]
        public void TryPlaceWall_WhenPathValidationFails_DoesNotMutateState()
        {
            var state = new WallState(QuoridorRules.BoardSize, QuoridorRules.InitialWallCount);

            PlaceRequiredWall(state, new WallPlacement(new BoardPosition(0, 0), WallOrientation.Horizontal));
            PlaceRequiredWall(state, new WallPlacement(new BoardPosition(2, 0), WallOrientation.Horizontal));
            PlaceRequiredWall(state, new WallPlacement(new BoardPosition(4, 0), WallOrientation.Horizontal));
            PlaceRequiredWall(state, new WallPlacement(new BoardPosition(7, 0), WallOrientation.Vertical));

            int placedCountBefore = state.PlacedWalls.Count;
            int remainingBefore = state.GetRemainingWalls(PlayerId.PlayerOne);
            var candidate = new WallPlacement(new BoardPosition(6, 0), WallOrientation.Horizontal);

            WallValidationResult result = state.TryPlaceWall(
                PlayerId.PlayerOne,
                candidate,
                QuoridorRules.PlayerOneStart,
                QuoridorRules.PlayerTwoStart);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Reason, Does.Contain("player one"));
            Assert.That(state.PlacedWalls, Has.Count.EqualTo(placedCountBefore));
            Assert.That(state.GetRemainingWalls(PlayerId.PlayerOne), Is.EqualTo(remainingBefore));
            Assert.That(state.Graph.AreConnected(new BoardPosition(6, 0), new BoardPosition(6, 1)), Is.True);
            Assert.That(state.Graph.AreConnected(new BoardPosition(7, 0), new BoardPosition(7, 1)), Is.True);
        }

        private static void PlaceRequiredWall(WallState state, WallPlacement wall)
        {
            WallValidationResult result = state.TryPlaceWall(
                PlayerId.PlayerTwo,
                wall,
                QuoridorRules.PlayerOneStart,
                QuoridorRules.PlayerTwoStart);

            Assert.That(result.IsValid, Is.True, result.Reason);
        }
    }
}
