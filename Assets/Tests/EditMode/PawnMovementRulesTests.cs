// Covers pawn movement rule edge cases in edit mode tests
using NUnit.Framework;
using Quoridor.Core;

namespace Quoridor.Tests.EditMode
{
    /// <summary>
    /// Covers pawn movement rule edge cases in edit mode tests.
    /// </summary>
    public sealed class PawnMovementRulesTests
    {
        [Test]
        public void GetLegalMoves_ReturnsOrthogonalMovesWhenOpponentIsNotAdjacent()
        {
            BoardGraph graph = BoardGraph.CreateOpenBoard(QuoridorRules.BoardSize);

            var moves = PawnMovementRules.GetLegalMoves(graph, new BoardPosition(4, 4), new BoardPosition(4, 8));

            Assert.That(ContainsMove(moves, new BoardPosition(4, 5)), Is.True);
            Assert.That(ContainsMove(moves, new BoardPosition(4, 3)), Is.True);
            Assert.That(ContainsMove(moves, new BoardPosition(3, 4)), Is.True);
            Assert.That(ContainsMove(moves, new BoardPosition(5, 4)), Is.True);
            Assert.That(moves.Count, Is.EqualTo(4));
        }

        [Test]
        public void GetLegalMoves_JumpsOverAdjacentOpponentWhenBehindEdgeIsOpen()
        {
            BoardGraph graph = BoardGraph.CreateOpenBoard(QuoridorRules.BoardSize);

            var moves = PawnMovementRules.GetLegalMoves(graph, new BoardPosition(4, 4), new BoardPosition(4, 5));

            Assert.That(ContainsMove(moves, new BoardPosition(4, 6)), Is.True);
            Assert.That(ContainsMove(moves, new BoardPosition(4, 5)), Is.False);
        }

        [Test]
        public void GetLegalMoves_UsesDiagonalMovesWhenStraightJumpIsBlocked()
        {
            BoardGraph graph = BoardGraph.CreateOpenBoard(QuoridorRules.BoardSize);
            graph.ApplyWall(new WallPlacement(new BoardPosition(4, 5), WallOrientation.Horizontal));

            var moves = PawnMovementRules.GetLegalMoves(graph, new BoardPosition(4, 4), new BoardPosition(4, 5));

            Assert.That(ContainsMove(moves, new BoardPosition(3, 5)), Is.True);
            Assert.That(ContainsMove(moves, new BoardPosition(5, 5)), Is.True);
            Assert.That(ContainsMove(moves, new BoardPosition(4, 6)), Is.False);
        }

        private static bool ContainsMove(System.Collections.Generic.IEnumerable<BoardPosition> moves, BoardPosition expected)
        {
            foreach (BoardPosition move in moves)
            {
                if (move == expected)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
