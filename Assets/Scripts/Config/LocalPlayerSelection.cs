// Small cross-scene holder for local player character choices
using Quoridor.Core;

namespace Quoridor.Config
{
    /// <summary>
    /// Small cross-scene holder for local player character choices.
    /// </summary>
    public static class LocalPlayerSelection
    {
        private static string playerOneCharacterId;
        private static string playerTwoCharacterId;

        /// <summary>
        /// Stores the selected character id for one local player.
        /// </summary>
        public static void SetCharacter(PlayerId playerId, string characterId)
        {
            if (playerId == PlayerId.PlayerOne)
            {
                playerOneCharacterId = characterId;
                return;
            }

            playerTwoCharacterId = characterId;
        }

        /// <summary>
        /// Returns the selected character id for one local player, or an empty string if none was chosen.
        /// </summary>
        public static string GetCharacterId(PlayerId playerId)
        {
            return playerId == PlayerId.PlayerOne ? playerOneCharacterId : playerTwoCharacterId;
        }

        /// <summary>
        /// Clears any previously stored local choices.
        /// </summary>
        public static void Clear()
        {
            playerOneCharacterId = string.Empty;
            playerTwoCharacterId = string.Empty;
        }
    }
}
