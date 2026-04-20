using NUnit.Framework;
using Quoridor.Core;

namespace Quoridor.Tests.EditMode
{
    public sealed class VictoryRulesTests
    {
        [Test]
        public void HasPlayerWon_PlayerOneWinsOnTopRow()
        {
            Assert.That(VictoryRules.HasPlayerWon(PlayerId.PlayerOne, new BoardPosition(4, 8), QuoridorRules.BoardSize), Is.True);
            Assert.That(VictoryRules.HasPlayerWon(PlayerId.PlayerOne, new BoardPosition(4, 7), QuoridorRules.BoardSize), Is.False);
        }

        [Test]
        public void HasPlayerWon_PlayerTwoWinsOnBottomRow()
        {
            Assert.That(VictoryRules.HasPlayerWon(PlayerId.PlayerTwo, new BoardPosition(4, 0), QuoridorRules.BoardSize), Is.True);
            Assert.That(VictoryRules.HasPlayerWon(PlayerId.PlayerTwo, new BoardPosition(4, 1), QuoridorRules.BoardSize), Is.False);
        }
    }
}
