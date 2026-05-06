using UnityEngine;
using UnityEngine.UI;

namespace Quoridor.Menu
{
    /// <summary>
    /// Bridges one character selection button to the main menu controller.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterSelectButton : MonoBehaviour
    {
        [SerializeField] private MainMenuController menuController;
        [SerializeField] private Button button;
        [SerializeField] private string characterId;
        [SerializeField] private string characterName;

        /// <summary>
        /// Stable id passed to the menu controller when this character is selected.
        /// </summary>
        public string CharacterId => characterId;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            if (menuController != null)
            {
                menuController.SelectCharacter(characterId, characterName);
            }
        }
    }
}
