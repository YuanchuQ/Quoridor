using UnityEngine;

namespace Quoridor.Config
{
    /// <summary>
    /// Describes the UI and board visuals for a selectable character.
    /// </summary>
    [System.Serializable]
    public sealed class CharacterVisualDefinition
    {
        [SerializeField] private string characterId;
        [SerializeField] private string displayName;
        [SerializeField] private string latinName;
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private Sprite pawnSprite;
        [SerializeField, Min(0.01f)] private float pawnScale = 0.13f;
        [SerializeField] private Vector2 pawnOffset = new(0f, 0.1f);

        /// <summary>
        /// Stable id used by menu selections and save data.
        /// </summary>
        public string CharacterId => characterId;

        /// <summary>
        /// Localized display name shown in the match HUD.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Secondary romanized name shown below the display name.
        /// </summary>
        public string LatinName => latinName;

        /// <summary>
        /// Sprite used inside the match HUD portrait frame.
        /// </summary>
        public Sprite PortraitSprite => portraitSprite;

        /// <summary>
        /// Sprite used by the pawn object on the board.
        /// </summary>
        public Sprite PawnSprite => pawnSprite;

        /// <summary>
        /// Local scale applied to the pawn view when this character is active.
        /// </summary>
        public float PawnScale => pawnScale;

        /// <summary>
        /// Small visual offset from the board cell center.
        /// </summary>
        public Vector2 PawnOffset => pawnOffset;
    }
}
