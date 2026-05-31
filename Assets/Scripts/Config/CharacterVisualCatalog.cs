// ScriptableObject catalog for character sprites used by menus, HUD, and pawns
using Quoridor.Core;
using UnityEngine;

namespace Quoridor.Config
{
    /// <summary>
    /// ScriptableObject catalog for character sprites used by menus, HUD, and pawns.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterVisualCatalog", menuName = "Quoridor/Character Visual Catalog")]
    public sealed class CharacterVisualCatalog : ScriptableObject
    {
        [Tooltip("Selectable character visual definitions")]
        [SerializeField] private CharacterVisualDefinition[] characters = System.Array.Empty<CharacterVisualDefinition>();
        [Tooltip("Default character id used by player one")]
        [SerializeField] private string defaultPlayerOneCharacterId = "yui";
        [Tooltip("Default character id used by player two")]
        [SerializeField] private string defaultPlayerTwoCharacterId = "karyl";

        /// <summary>
        /// All available character visual definitions.
        /// </summary>
        public CharacterVisualDefinition[] Characters => characters;

        /// <summary>
        /// Default character id used for player one.
        /// </summary>
        public string DefaultPlayerOneCharacterId => defaultPlayerOneCharacterId;

        /// <summary>
        /// Default character id used for player two.
        /// </summary>
        public string DefaultPlayerTwoCharacterId => defaultPlayerTwoCharacterId;

        /// <summary>
        /// Returns the configured default character for a player.
        /// </summary>
        public CharacterVisualDefinition GetDefault(PlayerId playerId)
        {
            string id = playerId == PlayerId.PlayerOne ? defaultPlayerOneCharacterId : defaultPlayerTwoCharacterId;
            return GetById(id) ?? GetFirstValid();
        }

        /// <summary>
        /// Finds a character by stable id or returns null when it is unavailable.
        /// </summary>
        public CharacterVisualDefinition GetById(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            foreach (CharacterVisualDefinition character in characters)
            {
                if (character != null && character.CharacterId == characterId)
                {
                    return character;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a character by localized display name or returns null.
        /// </summary>
        public CharacterVisualDefinition GetByDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return null;
            }

            foreach (CharacterVisualDefinition character in characters)
            {
                if (character != null && character.DisplayName == displayName)
                {
                    return character;
                }
            }

            return null;
        }

        private CharacterVisualDefinition GetFirstValid()
        {
            foreach (CharacterVisualDefinition character in characters)
            {
                if (character != null)
                {
                    return character;
                }
            }

            return null;
        }
    }
}
