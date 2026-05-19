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
        [SerializeField] private Image portraitImage;
        [SerializeField] private Material selectedMaterial;

        private Material defaultMaterial;
        private Vector3 defaultScale;

        /// <summary>
        /// Stable id passed to the menu controller when this character is selected.
        /// </summary>
        public string CharacterId => characterId;

        private void Awake()
        {
            if (portraitImage == null && transform.Find("Portrait") != null)
            {
                portraitImage = transform.Find("Portrait").GetComponent<Image>();
            }

            defaultMaterial = portraitImage != null ? portraitImage.material : null;
            defaultScale = transform.localScale;
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnEnable()
        {
            RefreshSelection(false);
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

            CharacterSelectButton[] buttons = transform.parent != null
                ? transform.parent.GetComponentsInChildren<CharacterSelectButton>(true)
                : System.Array.Empty<CharacterSelectButton>();
            foreach (CharacterSelectButton selectButton in buttons)
            {
                selectButton.RefreshSelection(selectButton == this);
            }
        }

        private void RefreshSelection(bool isSelected)
        {
            if (portraitImage != null)
            {
                portraitImage.material = isSelected && selectedMaterial != null ? selectedMaterial : defaultMaterial;
            }

            transform.localScale = isSelected ? defaultScale * 1.04f : defaultScale;
        }
    }
}
