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
        [SerializeField] private string characterName;

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
                menuController.SelectCharacter(characterName);
            }
        }
    }
}
